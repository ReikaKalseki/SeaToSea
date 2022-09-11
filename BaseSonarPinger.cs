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
	
	public class BaseSonarPinger : CustomMachine<BaseSonarPingerLogic> {
		
		internal static readonly float POWER_COST = 10F; //per ping
		internal static readonly float FIRE_RATE = 4F; //interval in seconds
		internal static readonly float MAX_RANGE = 300F; //m
		
		static BaseSonarPinger() {
			
		}
		
		public BaseSonarPinger() : base("basesonarping", "Seabase Sonar Antenna", "Continuously fires sonar pulses from a seabase.", "8949b0da-5173-431f-a989-e621af02f942") {
			addIngredient(CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 3);
			addIngredient(TechType.Gold, 2);
			addIngredient(TechType.CyclopsSonarModule, 1);
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
			ObjectUtil.removeComponent<PowerRelay>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
						
			BaseSonarPingerLogic lgc = go.GetComponent<BaseSonarPingerLogic>();
			
			Renderer r = go.GetComponentInChildren<Renderer>();/*
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/
			
			//go.GetComponent<Constructable>().model = go;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}
		
	}
		
	public class BaseSonarPingerLogic : CustomMachineLogic {
		
		private float lastPing;
		
		void Start() {
			SNUtil.log("Reinitializing base sonar");
			SeaToSeaMod.sonarBlock.initializeMachine(gameObject);
		}
		
		private void ping(float time) {
			if (consumePower(BaseSonarPinger.POWER_COST, 1)) {
				lastPing = time;
				if (Inventory.main.equipment.GetCount(TechType.MapRoomHUDChip) > 0)
					SNCameraRoot.main.SonarPing();
				SNUtil.playSoundAt(SNUtil.getSound("event:/sub/cyclops/sonar"), Player.main.transform.position, false, BaseSonarPinger.MAX_RANGE, 4);
			}
		}
		
		private bool isInAppropriateLocation() {
			Vector3 p1 = Player.main.transform.position;
			Vector3 p2 = gameObject.transform.position;
			return p1.y >= p2.y-100 && Vector3.Distance(p1, p2) <= BaseSonarPinger.MAX_RANGE;
		}
		
		protected override void updateEntity(float seconds) {
			//if (mainRenderer == null)
			//	mainRenderer = ObjectUtil.getChildObject(gameObject, "model").GetComponent<Renderer>();
			
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastPing >= BaseSonarPinger.FIRE_RATE && isInAppropriateLocation()) {
				ping(time);
			}
		}	
	}
}
