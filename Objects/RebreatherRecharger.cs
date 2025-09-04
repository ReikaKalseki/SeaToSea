using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public class RebreatherRecharger : CustomMachine<RebreatherRechargerLogic> {

		internal static readonly float POWER_COST_IDLE = 0.5F; //per second
		internal static readonly float POWER_COST_ACTIVE = 2.5F; //per second
		internal static readonly float MAX_RATE = 7.5F; //seconds per second

		static RebreatherRecharger() {

		}

		public RebreatherRecharger(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "bedc40fb-bd97-4b4d-a943-d39360c9c7bd") { //nuclear waste disposal
			this.addIngredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1);
			this.addIngredient(TechType.AramidFibers, 2);
			this.addIngredient(TechType.Titanium, 1);
			this.addIngredient(TechType.Pipe, 15);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}

		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			go.removeComponent<Trashcan>();
			go.removeChildObject("Bubbles");

			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			con.hoverText = "Reload Fluid";
			con.storageLabel = "FLUID";
			con.enabled = true;
			con.Resize(3, 3);
			//con.prefabRoot = go;
			RebreatherRechargerLogic lgc = go.GetComponent<RebreatherRechargerLogic>();
			//lgc.storage = con;

			GameObject air = ObjectUtil.lookupPrefab("7b4b90b8-6294-4354-9ebb-3e5aa49ae453");
			FMOD_CustomLoopingEmitter snd = air.GetComponentInChildren<FMOD_CustomLoopingEmitter>(true);
			GameObject mdl = go.setModel("discovery_trashcan_01_d", air.getChildObject("model"));
			mdl.transform.localScale = new Vector3(3, 4, 3);
			lgc.sound = go.EnsureComponent<FMOD_CustomLoopingEmitter>();
			lgc.sound.copyObject<FMOD_CustomLoopingEmitter>(snd);
			lgc.turbine = mdl.getChildObject("_pipes_floating_air_intake_turbine_geo");

			Renderer r = mdl.GetComponentInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;

			go.GetComponent<Constructable>().model = mdl;
			go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}

	}

	public class RebreatherRechargerLogic : CustomMachineLogic {

		internal Renderer mainRenderer;

		internal GameObject turbine;
		internal FMOD_CustomLoopingEmitter sound;

		private bool inUse;
		private bool isPowered;
		private float secsNoPwr = 0;
		private float speed = 0;

		private float available = 0;

		void Start() {
			SNUtil.log("Reinitializing rebreather charger");
			C2CItems.rebreatherCharger.initializeMachine(gameObject);
		}

		protected override void load(System.Xml.XmlElement data) {
			available = (float)data.getFloat("fuel", float.NaN);
		}

		protected override void save(System.Xml.XmlElement data) {
			data.addProperty("fuel", available);
		}

		protected override void updateEntity(float seconds) {
			if (mainRenderer == null)
				mainRenderer = gameObject.getChildObject("model").GetComponent<Renderer>();
			if (!storage) {
				return;
			}
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (seconds <= 0)
				return;
			storage.hoverText = "Reload Fluid (" + available.ToString("0.00") + "s fluid in buffer)";

			Transform seabase = gameObject.transform.parent;
			if (available > 0 && seabase != null) {
				//seabase.gameObject.EnsureComponent<RebreatherRechargerSeaBaseLogic>().addMachine(this);
				Player p = Player.main;
				if (p.currentSub != null && seabase.gameObject == p.currentSub.gameObject) {
					//SNUtil.writeToChat("Player in base with recharger, has "+available);
					LiquidBreathingSystem.instance.refillFrom(this, seconds);
				}
				LiquidBreathingSystem.instance.applyToBasePipes(this, seabase);
			}

			float cost = inUse ? RebreatherRecharger.POWER_COST_ACTIVE : RebreatherRecharger.POWER_COST_IDLE;
			isPowered = this.consumePower(cost * seconds);
			if (isPowered) {
				speed = Math.Min((speed * 1.05F) + 0.15F, 150);
				secsNoPwr = 0;
				sound.Play();
				if (available < 6000 && storage.container.GetCount(C2CItems.breathingFluid.TechType) > 0) {
					available += LiquidBreathingSystem.ITEM_VALUE;
					storage.container.DestroyItem(C2CItems.breathingFluid.TechType);
				}
			}
			else {
				speed = Math.Max((speed * 0.98F) - 0.02F, 0);
				secsNoPwr += seconds;
				if (secsNoPwr >= 1)
					sound.Stop();
			}
			Vector3 angs = turbine.transform.localEulerAngles;
			angs.y += speed * seconds;
			turbine.transform.localEulerAngles = angs;
		}

		public void refund(float amt) {
			available += amt;
		}

		public float getFuel() {
			return available;
		}

		public float consume(float time, float seconds) {
			return isPowered ? this.consumeUpTo(time, seconds) : 0;
		}

		private float consumeUpTo(float amt, float seconds) {
			float use = Mathf.Min(amt, available, RebreatherRecharger.MAX_RATE*seconds);
			available -= use;
			inUse |= use > 0;
			return use;
		}
	}

	public class RebreatherRechargerSeaBaseLogic {

		private readonly Dictionary<string, RebreatherRechargerLogic> machines = new Dictionary<string, RebreatherRechargerLogic>();

		private void addMachine(RebreatherRechargerLogic lgc) {
			machines[lgc.gameObject.GetComponent<PrefabIdentifier>().id] = lgc;
		}

	}
}
