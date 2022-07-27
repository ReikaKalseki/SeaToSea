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
	
	public class RebreatherRecharger : CustomMachine<RebreatherRechargerLogic> {
		
		internal static readonly float POWER_COST = 1.5F; //per second
		internal static readonly float ITEM_VALUE = 20*60; //seconds
		
		static RebreatherRecharger() {
			
		}
		
		public RebreatherRecharger() : base("rebreathercharger", "Liquid Breathing Recharger", "Refills liquid breathing systems in proximity.", "bedc40fb-bd97-4b4d-a943-d39360c9c7bd") { //nuclear waste disposal
			addIngredient(TechType.AdvancedWiringKit, 1);
			addIngredient(TechType.FiberMesh, 4);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1);
			addIngredient(TechType.PlasteelIngot, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Trashcan>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			con.hoverText = "Reload Fluid";
			con.storageLabel = "FLUID";
			con.enabled = true;
			con.Resize(3, 3);
			//con.prefabRoot = go;
			RebreatherRechargerLogic lgc = go.GetComponent<RebreatherRechargerLogic>();
			lgc.storage = con;
		 	
			GameObject air = ObjectUtil.lookupPrefab("7b4b90b8-6294-4354-9ebb-3e5aa49ae453");
			FMOD_CustomLoopingEmitter snd = air.GetComponentInChildren<FMOD_CustomLoopingEmitter>(true);
			GameObject mdl = RenderUtil.setModel(go, "discovery_trashcan_01_d", ObjectUtil.getChildObject(air, "model"));
			mdl.transform.localScale = new Vector3(3, 4, 3);
			lgc.sound = go.EnsureComponent<FMOD_CustomLoopingEmitter>();
			lgc.sound.copyObject<FMOD_CustomLoopingEmitter>(snd);
			lgc.turbine = ObjectUtil.getChildObject(mdl, "_pipes_floating_air_intake_turbine_geo");
			
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
		
		internal StorageContainer storage;
		internal Renderer mainRenderer;
		
		internal GameObject turbine;
		internal FMOD_CustomLoopingEmitter sound;
		
		private bool isPowered;
		private float secsNoPwr = 0;
		private float speed = 0;
		
		private float available = 0;
		
		void Start() {
			SNUtil.log("Reinitializing rebreather charger");
			SeaToSeaMod.rebreatherCharger.initializeMachine(gameObject);
		}
		
		protected override void updateEntity(float seconds) {
			if (storage == null)
				storage = gameObject.GetComponentInChildren<StorageContainer>();
			if (mainRenderer == null)
				mainRenderer = ObjectUtil.getChildObject(gameObject, "model").GetComponent<Renderer>();
			if (storage == null) {
				return;
			}
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (seconds <= 0)
				return;

			Transform seabase = gameObject.transform.parent;
			if (seabase != null) {
				//seabase.gameObject.EnsureComponent<RebreatherRechargerSeaBaseLogic>().addMachine(this);
				if (Player.main.currentSub != null && seabase.gameObject == Player.main.currentSub.gameObject) {
					//SNUtil.writeToChat("Player in base with recharger, has "+available);
					float addable = Player.main.GetOxygenCapacity()-Player.main.GetOxygenAvailable();
					if (available > 0 && addable > 0 && Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType) != 0) {
						float add = consume(addable);
						C2CHooks.forceAllowO2 = true;
						Player.main.oxygenMgr.AddOxygen(add);
						//SNUtil.writeToChat("Added "+add);
					}
				}
			}
			
			isPowered = consumePower(seconds);
			if (isPowered) {
				speed = Math.Min(speed*1.05F+0.15F, 150);
				secsNoPwr = 0;
				sound.Play();
				if (storage.container.RemoveItem(SeaToSeaMod.breathingFluid.TechType) != null)
					available += RebreatherRecharger.ITEM_VALUE;
			}
			else {
				speed = Math.Max(speed*0.98F-0.02F, 0);
				secsNoPwr += seconds;
				if (secsNoPwr >= 1)
					sound.Stop();
			}
			Vector3 angs = turbine.transform.localEulerAngles;
			angs.y += speed*seconds;
			turbine.transform.localEulerAngles = angs;
		}
		
		public float consume(float time) {
			return isPowered && consumePower(4) ? consumeUpTo(time) : 0;
		}
		
		private float consumeUpTo(float amt) {
			float use = Math.Min(amt, available);
			available -= use;
			return use;
		}
		
		private bool consumePower(float sc = 1) {
			SubRoot sub = gameObject.GetComponentInParent<SubRoot>();
			if (sub == null)
				return false;
			float receive;
			sub.powerRelay.ConsumeEnergy(RebreatherRecharger.POWER_COST*sc, out receive);
			receive += 0.0001F;
			return receive >= RebreatherRecharger.POWER_COST*sc;
		}
		
	}
	
	public class RebreatherRechargerSeaBaseLogic {
		
		private readonly Dictionary<string, RebreatherRechargerLogic> machines = new Dictionary<string, RebreatherRechargerLogic>();
		
		private void addMachine(RebreatherRechargerLogic lgc) {
			machines[lgc.gameObject.GetComponent<PrefabIdentifier>().id] = lgc;
		}
		
	}
}
