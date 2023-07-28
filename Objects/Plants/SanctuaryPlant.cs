﻿using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class SanctuaryPlant : BasicCustomPlant {
		
		public SanctuaryPlant() : base(SeaToSeaMod.itemLocale.getEntry("SANCTUARY_PLANT"), new FloraPrefabFetch("99bbd145-d50e-4afb-bff0-27b33243642b"), "ce20c267-b52b-4866-8134-f3f78072af3e", "Core") {
			glowIntensity = 1F;
			collectionMethod = HarvestType.None;
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<SanctuaryPlantTag>().addField("lastHarvest"));
			};
		}

		protected override bool isExploitable() {
			return true;
		}
		
		protected override bool generateSeed() {
			return true;
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<SanctuaryPlantTag>();
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
			RenderUtil.setEmissivity(go.GetComponentInChildren<Renderer>(), 2);
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
		public override Plantable.PlantSize getSize() {
			return Plantable.PlantSize.Large;
		}
		
	}
	
	class SanctuaryPlantTag : MonoBehaviour, IHandTarget {
		
		private bool isGrown;
		
		private Renderer mainRender;
		private Light light;
		private SphereCollider bounds;
		
		private float lastHarvest = -1;
		bool prevHarvested;
		
		//private float lastAreaCheck = -1;
		
		private static readonly float GROW_TIME = 1200; //20 min
		
		void Start() {
			isGrown = gameObject.GetComponent<GrownPlant>() != null;
    		//if (gameObject.transform.position.y > -10)
    		//	UnityEngine.Object.Destroy(gameObject);
    		if (isGrown) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.8F, 1.2F);
    		}
    		else {
    			gameObject.transform.localScale = Vector3.one*3;	
    		}
		}
		
		void Update() {
			if (!light)
				light = GetComponentInChildren<Light>();
			if (!bounds)
				bounds = GetComponentInChildren<SphereCollider>();
			if (mainRender == null)
				mainRender = GetComponentInChildren<Renderer>();
			light.transform.localPosition = Vector3.up*0.91F;
			/*
			if (!isGrown && DayNightCycle.main.timePassedAsFloat-lastAreaCheck >= 1 && CrashZoneSanctuaryBiome.instance.isInBiome(transform.position)) {
				foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(transform.position, 2.0F)) {
					if (pi.classId == SeaToSeaMod.crashSanctuaryGrass.ClassID || CrashZoneSanctuarySpawner.spawnsPlant(pi.ClassId))
						UnityEngine.Object.DestroyImmediate(pi.gameObject);
				}
				lastAreaCheck = DayNightCycle.main.timePassedAsFloat;
			}*/
			
			bool harvested = isHarvested();
			if (harvested != prevHarvested) {
				string fn = harvested ? "_Harvest" : "";
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, mainRender, "Textures/Plants/SanctuaryPlant"+fn);
			}
			prevHarvested = harvested;
			
			bounds.radius = 0.3F;
			bounds.center = Vector3.up*0.25F;
			light.transform.localPosition = Vector3.up*(harvested ? 1.4F : 0.91F);
			light.intensity = harvested ? 0.75F : 1.6F;
			light.range = harvested ? 7 : 24;
			light.color = harvested ? new Color(130/255F, 134/255F, 1, 1) : new Color(26/255F, 231/255F, 220/255F, 1);
		}
		
		internal bool isHarvested() {
			return lastHarvest >= 0 && DayNightCycle.main.timePassedAsFloat-lastHarvest < GROW_TIME;
		}
		
		internal float getGrowthProgress() {
			return (DayNightCycle.main.timePassedAsFloat-lastHarvest)/GROW_TIME;
		}
		
		internal bool tryHarvest() {
			if (isHarvested())
				return false;
			lastHarvest = DayNightCycle.main.timePassedAsFloat;
			return true;
		}
		
		public void OnHandHover(GUIHand hand) {
			if (isHarvested()) {
			  	HandReticle.main.SetProgress(getGrowthProgress());
				HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
				HandReticle.main.SetInteractText(C2CHooks.sanctuaryPlantGrowingLocaleKey);
			   	HandReticle.main.SetTargetDistance(8);
			}
			else {
			   	HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
			   	HandReticle.main.SetInteractText(C2CHooks.sanctuaryPlantClickLocaleKey);
			   	HandReticle.main.SetTargetDistance(8);
			}
		}
	
		public void OnHandClick(GUIHand hand) {
			if (tryHarvest())
			    InventoryUtil.addItem(C2CItems.sanctuaryPlant.seed.TechType);
		}
		
	}
}
