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
		
		internal static readonly float POWER_COST_IDLE = 2.0F; //per second; was 1.5 then 2.5
		internal static readonly float POWER_COST_ACTIVE = 18.0F; //per second
		
		private static readonly string MACHINE_GO_NAME = "MachineModel";
		
		static RebreatherRecharger() {
			
		}
		
		public RebreatherRecharger() : base("rebreathercharger", "Liquid Breathing Recharger", "Refills liquid breathing systems in proximity.", "") {
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
		
		//protected OrientedBounds[] GetBounds { get; }
		
		public override void initializeMachine(GameObject go) { //FIXME sky tint issues
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Aquarium>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			con.hoverText = "Use Bioprocessor";
			con.storageLabel = "BIOPROCESSOR";
			con.enabled = true;
			con.Resize(6, 6);
			//con.prefabRoot = go;
			BioprocessorLogic lgc = go.GetComponent<BioprocessorLogic>();
			lgc.storage = con;
		 	
			GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("6ca93e93-5209-4c27-ba60-5f68f36a95fb"), "Starship_control_terminal_01"));
			mdl.transform.localEulerAngles = new Vector3(270, 0, 0);
			
		 	GameObject machineMdl = ObjectUtil.getChildObject(go, MACHINE_GO_NAME);
		 	if (machineMdl == null) {
			 	machineMdl = ObjectUtil.createWorldObject("02dfa77b-5407-4474-90c6-fcb0003ecf2d", true, false);
				machineMdl.name = MACHINE_GO_NAME;
			 	Vector3 vec = new Vector3(0, 1.41F, -0.975F);
			 	machineMdl.transform.localPosition = vec;
			 	machineMdl.transform.localScale = new Vector3(1, 1, 0.75F);
			 	machineMdl.transform.eulerAngles = new Vector3(90, 180, 0);
				machineMdl.transform.parent = go.transform;
		 	}
			
		 	foreach (Collider c in machineMdl.GetComponentsInChildren<Collider>()) {
				UnityEngine.Object.Destroy(c);
		 	}
			SphereCollider cc = machineMdl.AddComponent<SphereCollider>();
			cc.radius = 1.2F;
			cc.center = new Vector3(0, 0.25F, 0);
			cc = machineMdl.AddComponent<SphereCollider>();
			cc.radius = 1.2F;
			cc.center = new Vector3(0, 1F, 0);
			
			Renderer r = machineMdl.GetComponentInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			RenderUtil.setEmissivity(r, 2, "GlowStrength");
			r.materials[0].EnableKeyword("MARMO_EMISSION");
			r.sharedMaterial.EnableKeyword("MARMO_EMISSION");
			r.materials[0].SetFloat("_Shininess", 8);
			r.materials[0].SetFloat("_Fresnel", 0.4F);
			lgc.mainRenderer = r;
			
			go.GetComponent<Constructable>().model = machineMdl;
			go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 0.5F, 0);
			
			ObjectUtil.removeChildObject(go, "SubDamageSounds");
			
			go.EnsureComponent<SkyApplier>();
			SkyApplier[] skies = go.GetComponentsInChildren<SkyApplier>();
			foreach (SkyApplier sky in skies)
				sky.renderers = go.GetComponentsInChildren<Renderer>();
			
			//ObjectUtil.removeComponent<PrefabIdentifier>(machineMdl);
			//ChildObjectIdentifier coi = machineMdl.EnsureComponent<ChildObjectIdentifier>();
			//coi.classId = ClassID+"_mdl";
			
			foreach (SkyApplier sky in skies) {
				sky.renderers = go.GetComponentsInChildren<Renderer>();
				sky.enabled = true;
				sky.RefreshDirtySky();
				sky.ApplySkybox();
			}
			
			setTerminalBox(go);
		}
		
		internal static void setTerminalBox(GameObject go) {
			BoxCollider box = ObjectUtil.getChildObject(go, "Collider").EnsureComponent<BoxCollider>();
			box.center = new Vector3(0, 0.5F, 0);
			box.size = new Vector3(0.5F, 1.5F, 0.5F);
		}
		
	}
		
	public class RebreatherRechargerLogic : CustomMachineLogic {
		
		internal StorageContainer storage;
		internal Renderer mainRenderer;
		
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
			
			if (consumePower(seconds)) {
				
			}
			else {
				
			}
		}
		
		private bool consumePower(float sc = 1) {
			SubRoot sub = gameObject.GetComponentInParent<SubRoot>();
			if (sub == null)
				return false;
			float receive;
			sub.powerRelay.ConsumeEnergy(RebreatherRecharger.POWER_COST_IDLE*sc, out receive);
			receive += 0.0001F;
			return receive >= RebreatherRecharger.POWER_COST_IDLE*sc;
		}
		
	}
}
