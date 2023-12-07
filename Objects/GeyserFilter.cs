using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class GeyserFilter : CustomMachine<GeyserFilterLogic>, MultiTexturePrefab {
		
		//internal static readonly float POWER_COST = 1.5F; //per second
		internal static readonly float PRODUCTION_RATE = 45F; //seconds per item
		
		static GeyserFilter() {
			
		}
		
		public GeyserFilter(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "bedc40fb-bd97-4b4d-a943-d39360c9c7bd") { //nuclear waste disposal
			addIngredient(TechType.FiberMesh, 4);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1);
			addIngredient(TechType.Titanium, 3);
			addIngredient(TechType.CopperWire, 2);
			addIngredient(CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		public override bool isOutdoors() {
			return true;
		}
		
		public Dictionary<int, string> getTextureLayers(Renderer r) {
			return new Dictionary<int, string>{{0, ""}, {1, ""}};
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Trashcan>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
			
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			con.hoverText = "Collect Filtrate";
			con.storageLabel = "FILTRATE";
			con.enabled = true;
			con.Resize(5, 2);
			//con.prefabRoot = go;
			GeyserFilterLogic lgc = go.GetComponent<GeyserFilterLogic>();
			//lgc.storage = con;
		 	
			GameObject mdl = RenderUtil.setModel(go, "discovery_trashcan_01_d", ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("8fb8a082-d40a-4473-99ec-1ded36cc6813"), "Starship_cargo"));
			mdl.transform.localRotation = Quaternion.Euler(0, 0, 0);
			mdl.transform.localPosition = new Vector3(0, -0.05F, -2.25F);
			float w = 4;//2.5F;
			float t = 0.4F;//0.125F;
			mdl.transform.localScale = new Vector3(w, t, w);
			Constructable c = go.GetComponent<Constructable>();
			c.model = mdl;
			c.allowedOnCeiling = false;
			c.allowedOnGround = true;
			c.allowedOnWall = false;
			c.allowedOnConstructables = false;
			c.allowedInBase = false;
			c.allowedInSub = false;
			c.allowedOutside = true;
			c.forceUpright = true;
			
			GameObject mdl2 = UnityEngine.Object.Instantiate(mdl);
			mdl2.transform.SetParent(mdl.transform.parent);
			mdl2.transform.localRotation = Quaternion.Euler(180, 0, 0);
			mdl2.transform.localPosition = new Vector3(0, -(0.05F+0.18F*t/0.125F), 2.25F);
			mdl2.transform.localScale = new Vector3(w, t, w);
			
			BoxCollider box = go.GetComponentInChildren<BoxCollider>();
			box.size = new Vector3(w, t*2, w);
			box.center = Vector3.down*t*1.5F;
			
			Renderer[] r = mdl.GetComponentsInChildren<Renderer>();
			RenderUtil.swapToModdedTextures(r, this);
			foreach (Renderer rr in r)
				RenderUtil.setEmissivity(rr, 1);
			r = mdl2.GetComponentsInChildren<Renderer>();
			RenderUtil.swapToModdedTextures(r, this);
			foreach (Renderer rr in r)
				RenderUtil.setEmissivity(rr, 1);
			
			//go.EnsureComponent<PowerFX>().vfxPrefab = ObjectUtil.lookupPrefab(TechType.PowerTransmitter).GetComponent<PowerFX>().vfxPrefab;
		}
		
	}
		
	public class GeyserFilterLogic : DiscreteOperationalMachineLogic {
		
		internal Renderer[] mainRenderers;
		
		private Geyser liveGeyser;
		private float geyserDutyCycle = 0.2F;
		
		//private PowerFX lineRenderer;
		
		//private bool isPowered;
		//private bool checkedPower;
		
		private float collectionTime;
		
		private float lastGeyserCheckTime = -1;
		
		private bool showedFullMessage;
		
		void Start() {
			SNUtil.log("Reinitializing geyser filter");
			SeaToSeaMod.geyserFilter.initializeMachine(gameObject);
		}
		
		public override bool isWorking() {
			return true;
		}
		
		public override float getProgressScalar() {
			return Mathf.Clamp01(collectionTime/GeyserFilter.PRODUCTION_RATE);
		}
		
		protected override void load(System.Xml.XmlElement data) {
			collectionTime = (float)data.getFloat("collectionTime", 0);
			geyserDutyCycle = (float)data.getFloat("geyserActivity", 0.2);
		}
		
		protected override void save(System.Xml.XmlElement data) {
			data.addProperty("collectionTime", collectionTime);
			data.addProperty("geyserActivity", geyserDutyCycle);
		}
		
		protected override void updateEntity(float seconds) {
			if (mainRenderers == null)
				mainRenderers = GetComponentsInChildren<Renderer>();
			//if (!lineRenderer)
			//	lineRenderer = GetComponent<PowerFX>();
			if (seconds <= 0)
				return;
			if ((Player.main.transform.position-transform.position).sqrMagnitude >= 90000)
				return;
			float time = DayNightCycle.main.timePassedAsFloat;
			if (DIHooks.getWorldAge() > 0.5F && !liveGeyser && seconds > 0 && time-lastGeyserCheckTime >= 2.5F) {
				liveGeyser = findGeyser();
				if (liveGeyser)
					geyserDutyCycle = liveGeyser.eruptionLength/liveGeyser.eruptionInterval;
				lastGeyserCheckTime = time;
			}
			//lineRenderer.target = geyser ? geyser.gameObject : null;
			//SNUtil.writeToChat("Geyser: "+geyser+" @ "+(geyser ? geyser.transform.position.ToString() : "null"));
			foreach (Renderer r in mainRenderers)
				r.materials[0].SetColor("_GlowColor", Color.Lerp(Color.red, Color.green, collectionTime/GeyserFilter.PRODUCTION_RATE));
			//geyser.transform.SetParent(null);
			StorageContainer sc = getStorage();
			if (!sc) {
				return;
			}
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			sc.hoverText = "Collect filtrate";
			
			//setPowered(seconds);
			float increase = liveGeyser ? (liveGeyser.erupting ? seconds : 0) : (seconds*geyserDutyCycle);
			if (liveGeyser || liveGeyser.erupting) {
				collectionTime += increase;
				if (collectionTime >= GeyserFilter.PRODUCTION_RATE) {
					if (addItemToInventory(CraftingItems.getItem(CraftingItems.Items.GeyserMinerals).TechType) > 0) {
						collectionTime = 0;
						showedFullMessage = false;
					}
					else if (!showedFullMessage) {
						SNUtil.writeToChat(Language.main.Get(SeaToSeaMod.geyserFilter.TechType)+" in "+WorldUtil.getRegionalDescription(transform.position)+" is full");
						showedFullMessage = true;
					}
				}
			}
		}
		
		public static bool forceAlign = false;
		
		private Geyser findGeyser() {
			return findGeyser(transform.position);
		}
		
		public static Geyser findGeyser(Vector3 position) {/*
			IEcoTarget ret = EcoRegionManager.main.FindNearestTarget(EcoTargetType.HeatArea, transform.position, tgt => tgt.GetGameObject().GetComponent<Geyser>(), 10);
			if (ret == null)
				return null;
			GameObject go = ret.GetGameObject();*/
			foreach (Geyser g in WorldUtil.getObjectsNearWithComponent<Geyser>(position, 50)) {
				if (g.transform.position.y < position.y && position.y-g.transform.position.y <= 25) {
					if (Vector3.Distance(g.transform.position.setY(0), position.setY(0)) < 15)
						return g;
				}
			}
			return null;
		}
		
		/*
		private void setPowered(float seconds) {
			bool pwr = isPowered;
			isPowered = consumePower(GeyserFilter.POWER_COST*seconds);
			if (isPowered != pwr || !checkedPower) {
				foreach (Renderer r in mainRenderers)
					r.materials[0].SetColor("_GlowColor", isPowered ? Color.green : Color.red);
			}
			checkedPower = true;
		}*/
	}
}
