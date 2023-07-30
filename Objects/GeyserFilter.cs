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
	
	public class GeyserFilter : CustomMachine<GeyserFilterLogic> {
		
		internal static readonly float POWER_COST = 1.5F; //per second
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
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Trashcan>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
			
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
			c.allowedOnCeiling = true;
			c.allowedOnGround = true;
			c.allowedOnWall = true;
			c.allowedOnConstructables = true;
			
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
		}
		
	}
		
	public class GeyserFilterLogic : CustomMachineLogic {
		
		internal Renderer[] mainRenderers;
		private Geyser geyser;
		
		private bool isPowered;
		
		private float collectionTime;
		
		void Start() {
			SNUtil.log("Reinitializing geyser filter");
			SeaToSeaMod.geyserFilter.initializeMachine(gameObject);
		}
		
		protected override void load(System.Xml.XmlElement data) {
			
		}
		
		protected override void save(System.Xml.XmlElement data) {
			
		}
		
		protected override void updateEntity(float seconds) {
			if (!mainRenderers)
				mainRenderers = GetComponentsInChildren<Renderer>();
			if (!geyser && seconds > 0)
				geyser = WorldUtil.getClosest<Geyser>(transform.position);
			if (geyser && (geyser.transform.position.y > transform.position.y || Vector3.Distance(geyser.transform.position, transform.position) >= 30))
				geyser = null;
			SNUtil.writeToChat("Geyser: "+geyser+" @ "+(geyser ? geyser.transform.position.ToString() : "null"));
			if (!geyser)
				return;
			StorageContainer sc = getStorage();
			if (!sc) {
				return;
			}
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (seconds <= 0)
				return;
			sc.hoverText = "Collect filtrate";
			
			isPowered = consumePower(GeyserFilter.POWER_COST*seconds);
			if (isPowered && geyser.erupting) {
				collectionTime += seconds;
				if (collectionTime >= GeyserFilter.PRODUCTION_RATE) {
					if (addItemToInventory(CraftingItems.getItem(CraftingItems.Items.GeyserMinerals).TechType) > 0)
						collectionTime -= GeyserFilter.PRODUCTION_RATE;
				}
			}
		}
	}
}
