using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class ACUCallbackSystem { //TODO make this its own mod "acu metabolism"
		
		public static readonly ACUCallbackSystem instance = new ACUCallbackSystem();
		
		private readonly Dictionary<TechType, float> edibleFish = new Dictionary<TechType, float>(){
			{TechType.Reginald, 0},
			{TechType.Peeper, 0},
			{TechType.HoleFish, 0},
			{TechType.Oculus, 0},
			{TechType.GarryFish, 0},
			{TechType.Boomerang, 0},
			{TechType.Spadefish, 0},
			{TechType.Bladderfish, 0},
			{TechType.Eyeye, 0},
			{TechType.LavaEyeye, 0},
			{TechType.LavaBoomerang, 0},
			{TechType.Hoopfish, 0},
			{TechType.Spinefish, 0},
			{TechType.Hoverfish, 0}
		};
		
		private readonly Dictionary<VanillaFlora, float> ediblePlants = new Dictionary<VanillaFlora, float>(){
			{VanillaFlora.CREEPVINE, 0.1F},
			{VanillaFlora.CREEPVINE_FERTILE, 0.2F},
			{VanillaFlora.JELLYSHROOM, 0.25F},
			{VanillaFlora.EYE_STALK, 0.15F},
			{VanillaFlora.GABE_FEATHER, 0.25F},
			{VanillaFlora.GHOSTWEED, 0.25F},
			{VanillaFlora.HORNGRASS, 0.05F},
			{VanillaFlora.KOOSH, 0.15F},
			{VanillaFlora.MEMBRAIN, 0.3F},
			{VanillaFlora.PAPYRUS, 0.15F},
			{VanillaFlora.VIOLET_BEAU, 0.2F},
			{VanillaFlora.CAVE_BUSH, 0.05F},
			{VanillaFlora.REGRESS, 0.2F},
			{VanillaFlora.ROUGE_CRADLE, 0.05F},
			{VanillaFlora.SEACROWN, 0.4F},
			{VanillaFlora.SPOTTED_DOCKLEAF, 0.25F},
			{VanillaFlora.VEINED_NETTLE, 0.15F},
			{VanillaFlora.WRITHING_WEED, 0.15F},
			{VanillaFlora.TIGER, 0.5F},
		};
	    
	    private readonly Dictionary<TechType, ACUMetabolism> metabolisms = new Dictionary<TechType, ACUMetabolism>() {
			{TechType.RabbitRay, new ACUMetabolism(0.01F, 600, false)},
			{TechType.Gasopod, new ACUMetabolism(0.05F, 480, false)},
			{TechType.Jellyray, new ACUMetabolism(0.04F, 420, false)},
	    	{TechType.Stalker, new ACUMetabolism(0.05F, 300, false)},
	    	{TechType.Sandshark, new ACUMetabolism(0.03F, 480, false)},
	    	{TechType.BoneShark, new ACUMetabolism(0.03F, 300, false)},
	    	{TechType.Shocker, new ACUMetabolism(0.1F, 240, false)},
	    	{TechType.Crabsnake, new ACUMetabolism(0.08F, 180, false)},
	    	{TechType.CrabSquid, new ACUMetabolism(0.15F, 180, false)},
	    	{TechType.LavaLizard, new ACUMetabolism(0.05F, 480, false)},
	    	{TechType.SpineEel, new ACUMetabolism(0.03F, 240, false)},
	    };
		
		private ACUCallbackSystem() {
			foreach (TechType tt in new List<TechType>(edibleFish.Keys)) {
				GameObject go = CraftData.GetPrefabForTechType(SNUtil.getTechType("Cooked"+tt));
				Eatable ea = go.GetComponent<Eatable>();
				edibleFish[tt] = ea.foodValue;
				SNUtil.log(tt+" > "+ea.foodValue);
			}
		}
		
		public void tick(WaterPark acu) {
			float dT = Time.deltaTime;
			foreach (WaterParkItem wp in new List<WaterParkItem>(acu.items)) {
				if (wp && wp is WaterParkCreature) {
					TechTag tt = wp.gameObject.GetComponentInChildren<TechTag>();
					if (tt && metabolisms.ContainsKey(tt.type)) {
						ACUMetabolism am = metabolisms[tt.type];	
						Creature c = wp.gameObject.GetComponentInChildren<Creature>();
						c.Hunger.Add(dT*am.metabolismPerSecond);
						if (c.Hunger.Value >= 0.5F)
							if (tryEat(c, acu, am))
								c.Happy.Add(0.05F);
					}
					Shocker s = wp.GetComponentInChildren<Shocker>();
					if (s) {
						float trash;
						acu.GetComponentInParent<BaseRoot>().powerRelay.AddEnergy(dT*0.5F*Mathf.Clamp01(((WaterParkCreature)wp).age), out trash);
					}
				}
	   	 	}
		}
		
		private bool tryEat(Creature c, WaterPark acu, ACUMetabolism am) {
			if (am.isCarnivore) {
				foreach (WaterParkItem wp in acu.items) {
					TechTag tt = wp.GetComponentInChildren<TechTag>();
					if (tt && edibleFish.ContainsKey(tt.type)) {
						if (c.Hunger.Value+edibleFish[tt.type] <= 1) {
							c.Hunger.Add(-edibleFish[tt.type]);
							acu.RemoveItem(tt.GetComponent<Pickupable>());
							return true;
						}
					}
				}
				return false;
			}
			else {
				StorageContainer sc = acu.planter.GetComponentInChildren<StorageContainer>();
				foreach (TechTag tt in sc.GetComponentsInChildren<TechTag>()) {
					if (tt && ediblePlants.ContainsKey(VanillaFlora.getFromID(CraftData.GetClassIdForTechType(tt.type)))) {
						if (c.Hunger.Value+ediblePlants[tt.type] <= 1) {
							c.Hunger.Add(-ediblePlants[tt.type]);
							LiveMixin lv = tt.gameObject.GetComponent<LiveMixin>();
							if (lv && lv.IsAlive())
								lv.TakeDamage(10, c.transform.position, DamageType.Normal, c.gameObject);
							else
								sc.container.DestroyItem(tt.type);
							return true;
						}
					}
				}
				return false;
			}
		}
		
		class ACUMetabolism {
			
			internal readonly bool isCarnivore;
			internal readonly float metabolismPerSecond;
			internal readonly float secondsPerPoop;
			
			internal ACUMetabolism(float mf, float sp, bool isc) {
				secondsPerPoop = sp;
				metabolismPerSecond = mf*0.25F;
				isCarnivore = isc;
			}
			
		}
	}
	
}
