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
	
	public class ACUCallbackSystem { //TODO make this its own mod "acu metabolism" ; TODO add "healthy ecosystem" bonus to breeding
		
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
			{TechType.RabbitRay, new ACUMetabolism(0.01F, 0.1F, false)},
			{TechType.Gasopod, new ACUMetabolism(0.05F, 0.4F, false)},
			{TechType.Jellyray, new ACUMetabolism(0.04F, 0.3F, false)},
	    	{TechType.Stalker, new ACUMetabolism(0.05F, 0.5F, true)},
	    	{TechType.Sandshark, new ACUMetabolism(0.03F, 0.6F, true)},
	    	{TechType.BoneShark, new ACUMetabolism(0.03F, 0.8F, true)},
	    	{TechType.Shocker, new ACUMetabolism(0.1F, 0.5F, true)},
	    	{TechType.Crabsnake, new ACUMetabolism(0.08F, 1F, true)},
	    	{TechType.CrabSquid, new ACUMetabolism(0.15F, 1F, true)},
	    	{TechType.LavaLizard, new ACUMetabolism(0.05F, 0.5F, true)},
	    	{TechType.SpineEel, new ACUMetabolism(0.03F, 1.5F, true)},
	    };
		
		private ACUCallbackSystem() {
			foreach (TechType tt in new List<TechType>(edibleFish.Keys)) {
				GameObject go = CraftData.GetPrefabForTechType(SNUtil.getTechType("Cooked"+tt));
				Eatable ea = go.GetComponent<Eatable>();
				edibleFish[tt] = ea.foodValue*0.01F; //so a reginald is ~40%
			}
		}
		
		public void tick(WaterPark acu) {
			float dT = Time.deltaTime;
			foreach (WaterParkItem wp in new List<WaterParkItem>(acu.items)) {
				if (wp && wp is WaterParkCreature) {
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					if (metabolisms.ContainsKey(tt)) {
						ACUMetabolism am = metabolisms[tt];	
						Creature c = wp.gameObject.GetComponentInChildren<Creature>();
						c.Hunger.Add(dT*am.metabolismPerSecond);
						c.Hunger.Falloff = 0;
						if (c.Hunger.Value >= 0.5F) {
							float amt;
							if (tryEat(c, acu, am, out amt)) {
								c.Happy.Add(0.05F);
								float f = am.normalizedPoopChance*amt*Mathf.Pow(((WaterParkCreature)wp).age, 2F);
								//SNUtil.writeToChat(c+" ate > "+f);
								if (UnityEngine.Random.Range(0F, 1F) < f) {
									GameObject poo = ObjectUtil.createWorldObject(CraftingItems.getItem(CraftingItems.Items.MiniPoop).ClassID);
									poo.transform.position = c.transform.position+Vector3.down*0.05F;
									poo.transform.rotation = UnityEngine.Random.rotationUniform;
									//SNUtil.writeToChat("Poo spawned");
								}
							}
						}
					}
					Shocker s = wp.GetComponentInChildren<Shocker>();
					if (s) {
						float trash;
						acu.GetComponentInParent<BaseRoot>().powerRelay.AddEnergy(dT*0.5F*Mathf.Clamp01(((WaterParkCreature)wp).age), out trash);
					}
				}
	   	 	}
		}
		
		private bool tryEat(Creature c, WaterPark acu, ACUMetabolism am, out float amt) {
			if (am.isCarnivore) {
				foreach (WaterParkItem wp in acu.items) {
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					//SNUtil.writeToChat(pp+" > "+tt+" > "+edibleFish.ContainsKey(tt));
					if (edibleFish.ContainsKey(tt)) {
						if (c.Hunger.Value >= edibleFish[tt]) {
							c.Hunger.Add(-edibleFish[tt]);
							acu.RemoveItem(wp);
							UnityEngine.Object.DestroyImmediate(pp.gameObject);
							amt = edibleFish[tt];
							//SNUtil.writeToChat(c+" ate a "+tt+" and got "+amt);
							return true;
						}
					}
				}
				amt = 0;
				return false;
			}
			else {
				StorageContainer sc = acu.planter.GetComponentInChildren<StorageContainer>();
				foreach (PrefabIdentifier tt in sc.GetComponentsInChildren<PrefabIdentifier>()) {
					if (tt) {
						VanillaFlora vf = VanillaFlora.getFromID(tt.ClassId);
						//SNUtil.writeToChat(tt+" > "+vf+" > "+ediblePlants.ContainsKey(vf));
						if (vf != null && ediblePlants.ContainsKey(vf)) {
							if (c.Hunger.Value >= ediblePlants[vf]) {
								c.Hunger.Add(-ediblePlants[vf]);
								amt = ediblePlants[vf];
								//SNUtil.writeToChat(c+" ate a "+vf+" and got "+amt);
								LiveMixin lv = tt.gameObject.GetComponent<LiveMixin>();
								if (lv && lv.IsAlive())
									lv.TakeDamage(10, c.transform.position, DamageType.Normal, c.gameObject);
								else
									sc.container.DestroyItem(CraftData.entClassTechTable[tt.ClassId]);
								return true;
							}
						}
					}
				}
				amt = 0;
				return false;
			}
		}
		
		class ACUMetabolism {
			
			internal readonly bool isCarnivore;
			internal readonly float metabolismPerSecond;
			internal readonly float normalizedPoopChance;
			
			internal ACUMetabolism(float mf, float pp, bool isc) {
				normalizedPoopChance = pp*0.25F;
				metabolismPerSecond = mf*0.02F;
				isCarnivore = isc;
			}
			
		}
	}
	
}
