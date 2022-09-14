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
		
		private static readonly float FOOD_SCALAR = 0.2F; //all food values and metabolism multiplied by this, to give granularity
		
		private readonly Dictionary<TechType, AnimalFood> edibleFish = new Dictionary<TechType, AnimalFood>();
		
		private readonly Dictionary<VanillaFlora, PlantFood> ediblePlants = new Dictionary<VanillaFlora, PlantFood>();
	    
	    private readonly Dictionary<TechType, ACUMetabolism> metabolisms = new Dictionary<TechType, ACUMetabolism>() {
			{TechType.RabbitRay, new ACUMetabolism(0.01F, 0.1F, false, RegionType.Shallows)},
			{TechType.Gasopod, new ACUMetabolism(0.05F, 0.4F, false, RegionType.Shallows, RegionType.Other)},
			{TechType.Jellyray, new ACUMetabolism(0.04F, 0.3F, false, RegionType.Mushroom)},
	    	{TechType.Stalker, new ACUMetabolism(0.05F, 0.5F, true, RegionType.Kelp)},
	    	{TechType.Sandshark, new ACUMetabolism(0.03F, 0.6F, true, RegionType.RedGrass)},
	    	{TechType.BoneShark, new ACUMetabolism(0.03F, 0.8F, true, RegionType.Koosh, RegionType.Mushroom, RegionType.Other)},
	    	{TechType.Shocker, new ACUMetabolism(0.1F, 0.5F, true, RegionType.Koosh, RegionType.BloodKelp)},
	    	{TechType.Crabsnake, new ACUMetabolism(0.08F, 1F, true, RegionType.Jellyshroom)},
	    	{TechType.CrabSquid, new ACUMetabolism(0.15F, 1F, true, RegionType.BloodKelp, RegionType.LostRiver, RegionType.GrandReef)},
	    	{TechType.LavaLizard, new ACUMetabolism(0.05F, 0.5F, true, RegionType.LavaZone)},
	    	{TechType.SpineEel, new ACUMetabolism(0.03F, 1.5F, true, RegionType.LostRiver)},
	    };
		
		private ACUCallbackSystem() {
			addFood(new AnimalFood(TechType.Reginald, RegionType.RedGrass, RegionType.BloodKelp, RegionType.LostRiver, RegionType.GrandReef, RegionType.Other));
			addFood(new AnimalFood(TechType.Peeper, RegionType.Shallows, RegionType.RedGrass, RegionType.Mushroom, RegionType.GrandReef, RegionType.Koosh, RegionType.Other));
			addFood(new AnimalFood(TechType.HoleFish, RegionType.Shallows));
			addFood(new AnimalFood(TechType.Oculus, RegionType.Jellyshroom));
			addFood(new AnimalFood(TechType.GarryFish, RegionType.Shallows, RegionType.Other));
			addFood(new AnimalFood(TechType.Boomerang, RegionType.Shallows, RegionType.RedGrass, RegionType.Koosh, RegionType.GrandReef, RegionType.Other));
			addFood(new AnimalFood(TechType.Spadefish, RegionType.RedGrass, RegionType.GrandReef, RegionType.Mushroom, RegionType.Other));
			addFood(new AnimalFood(TechType.Bladderfish, RegionType.Shallows, RegionType.RedGrass, RegionType.Mushroom, RegionType.GrandReef, RegionType.LostRiver, RegionType.Other));
			addFood(new AnimalFood(TechType.Eyeye, RegionType.Jellyshroom, RegionType.GrandReef, RegionType.Koosh));
			addFood(new AnimalFood(TechType.LavaEyeye, RegionType.LavaZone));
			addFood(new AnimalFood(TechType.LavaBoomerang, RegionType.LavaZone));
			addFood(new AnimalFood(TechType.Hoopfish, RegionType.Kelp, RegionType.Koosh, RegionType.GrandReef, RegionType.Other));
			addFood(new AnimalFood(TechType.Spinefish, RegionType.BloodKelp, RegionType.LostRiver));
			addFood(new AnimalFood(TechType.Hoverfish, RegionType.Kelp));
			
			addFood(new PlantFood(VanillaFlora.CREEPVINE, 0.1F, RegionType.Kelp));
			addFood(new PlantFood(VanillaFlora.CREEPVINE_FERTILE, 0.2F, RegionType.Kelp));
			addFood(new PlantFood(VanillaFlora.JELLYSHROOM, 0.25F, RegionType.Jellyshroom));
			addFood(new PlantFood(VanillaFlora.EYE_STALK, 0.15F, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.GABE_FEATHER, 0.25F, RegionType.BloodKelp, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.GHOSTWEED, 0.25F, RegionType.LostRiver));
			addFood(new PlantFood(VanillaFlora.HORNGRASS, 0.05F, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.KOOSH, 0.15F, RegionType.Koosh));
			addFood(new PlantFood(VanillaFlora.MEMBRAIN, 0.3F, RegionType.GrandReef));
			addFood(new PlantFood(VanillaFlora.PAPYRUS, 0.15F, RegionType.RedGrass, RegionType.Jellyshroom, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.VIOLET_BEAU, 0.2F, RegionType.Jellyshroom, RegionType.RedGrass, RegionType.Koosh, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.CAVE_BUSH, 0.05F, RegionType.Koosh, RegionType.Jellyshroom, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.REGRESS, 0.2F, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.REDWORT, 0.15F, RegionType.RedGrass, RegionType.Koosh, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.ROUGE_CRADLE, 0.05F, RegionType.RedGrass, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.SEACROWN, 0.4F, RegionType.Koosh, RegionType.RedGrass));
			addFood(new PlantFood(VanillaFlora.SPOTTED_DOCKLEAF, 0.25F, RegionType.Koosh, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.VEINED_NETTLE, 0.15F, RegionType.Shallows));
			addFood(new PlantFood(VanillaFlora.WRITHING_WEED, 0.15F, RegionType.Shallows));
			addFood(new PlantFood(VanillaFlora.TIGER, 0.5F, RegionType.RedGrass));
		}
		
		private void addFood(Food f) {
			if (f is AnimalFood) {
				edibleFish[((AnimalFood)f).item] = (AnimalFood)f;
			}
			else if (f is PlantFood) {
				ediblePlants[((PlantFood)f).plant] = (PlantFood)f;
			}
		}
		
		public void tick(WaterPark acu) {
			float dT = Time.deltaTime;
			bool healthy = false;
			bool consistent = true;
			HashSet<RegionType> possibleBiomes = new HashSet<RegionType>();
			possibleBiomes.AddRange((IEnumerable<RegionType>)Enum.GetValues(typeof(RegionType)));
			StorageContainer sc = acu.planter.GetComponentInChildren<StorageContainer>();
			PrefabIdentifier[] plants = sc.GetComponentsInChildren<PrefabIdentifier>();
			int plant = 0;
			int herb = 0;
			int carn = 0;
			List<WaterParkCreature> foodFish = new List<WaterParkCreature>();
			foreach (WaterParkItem wp in new List<WaterParkItem>(acu.items)) {
				if (wp && wp is WaterParkCreature) {
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					if (edibleFish.ContainsKey(tt)) {
						possibleBiomes = new HashSet<RegionType>(possibleBiomes.Intersect(edibleFish[tt].regionType));
						foodFish.Add((WaterParkCreature)wp);
						herb++;
					}
					else if (metabolisms.ContainsKey(tt)) {
						ACUMetabolism am = metabolisms[tt];
						if (am.isCarnivore)
							carn++;
						else
							herb += tt == TechType.Gasopod ? 4 : 2;
						List<RegionType> li = new List<RegionType>(am.additionalRegions);
						li.Add(am.primaryRegion);
						possibleBiomes = new HashSet<RegionType>(possibleBiomes.Intersect(li));
						Creature c = wp.gameObject.GetComponentInChildren<Creature>();
						c.Hunger.Add(dT*am.metabolismPerSecond);
						c.Hunger.Falloff = 0;
						if (c.Hunger.Value >= 0.5F) {
							Food amt;
							if (tryEat(c, acu, am, sc, plants, out amt)) {
								float food = amt.foodValue;
								if (amt.isRegion(am.primaryRegion)) {
									food *= 3;
								}
								else {
									foreach (RegionType r in am.additionalRegions) {
										if (amt.isRegion(r)) {
											food *= 2;
											break;
										}
									}
								}
								if (c.Hunger.Value >= food) {
									c.Happy.Add(0.05F);
									c.Hunger.Add(-food);
									float f = am.normalizedPoopChance*amt.foodValue*Mathf.Pow(((WaterParkCreature)wp).age, 2F);
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
					}
					Shocker s = wp.GetComponentInChildren<Shocker>();
					if (s) {
						float trash;
						acu.GetComponentInParent<BaseRoot>().powerRelay.AddEnergy(dT*0.5F*Mathf.Clamp01(((WaterParkCreature)wp).age), out trash);
					}
				}
	   	 	}
			foreach (PrefabIdentifier pi in plants) {
				if (pi) {
					VanillaFlora vf = VanillaFlora.getFromID(pi.ClassId);
					if (vf != null && ediblePlants.ContainsKey(vf)) {
						possibleBiomes = new HashSet<RegionType>(possibleBiomes.Intersect(ediblePlants[vf].regionType));
						plant++;
					}
				}
			}
			consistent = possibleBiomes.Count > 0 && plant > 0;
			healthy = plant > 0 && herb > 0 && carn > 0 && carn <= Math.Max(1, herb/6) && herb > 0 && herb <= plant*4;
			//SNUtil.writeToChat(plant+"/"+herb+"/"+carn+" & "+string.Join(", ", possibleBiomes)+" > "+healthy+" & "+consistent);
			float boost = 0;
			if (consistent)
				boost += 1F;
			if (healthy)
				boost += 2F;
			if (boost > 0) {
				boost *= dT;
				foreach (WaterParkCreature wp in foodFish) {
					//SNUtil.writeToChat(wp+" > "+boost+" > "+wp.matureTime+"/"+wp.timeNextBreed);
					if (wp.canBreed) {
						if (wp.isMature) {
							wp.timeNextBreed -= boost;
						}
						else {
							wp.matureTime -= boost;
						}
					}
				}
			}
		}
		
		private bool tryEat(Creature c, WaterPark acu, ACUMetabolism am, StorageContainer sc, PrefabIdentifier[] pia, out Food amt) {
			if (am.isCarnivore) {
				WaterParkItem wp = acu.items[UnityEngine.Random.Range(0, acu.items.Count)];
				if (wp) {
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					//SNUtil.writeToChat(pp+" > "+tt+" > "+edibleFish.ContainsKey(tt));
					if (edibleFish.ContainsKey(tt)) {
						acu.RemoveItem(wp);
						UnityEngine.Object.DestroyImmediate(pp.gameObject);
						amt = edibleFish[tt];
						//SNUtil.writeToChat(c+" ate a "+tt+" and got "+amt);
						return true;
					}
				}
				amt = null;
				return false;
			}
			else {
				PrefabIdentifier tt = pia[UnityEngine.Random.Range(0, pia.Length)];
				if (tt) {
					VanillaFlora vf = VanillaFlora.getFromID(tt.ClassId);
					//SNUtil.writeToChat(tt+" > "+vf+" > "+ediblePlants.ContainsKey(vf));
					if (vf != null && ediblePlants.ContainsKey(vf)) {
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
				amt = null;
				return false;
			}
		}
		
		class Food {
			
			internal readonly float foodValue;
			internal readonly HashSet<RegionType> regionType = new HashSet<RegionType>();
			
			internal Food(float f, params RegionType[] r) {
				foodValue = f;
				regionType.AddRange(r.ToList());
			}
			
			internal bool isRegion(RegionType r) {
				return regionType.Contains(r);
			}
			
		}
		
		class AnimalFood : Food {
			
			internal readonly TechType item;
			
			internal AnimalFood(TechType tt, params RegionType[] r) : base(calculateFoodValue(tt), r) {
				item = tt;
			}
			
			static float calculateFoodValue(TechType tt) {
				GameObject go = CraftData.GetPrefabForTechType(SNUtil.getTechType("Cooked"+tt));
				Eatable ea = go.GetComponent<Eatable>();
				return ea.foodValue*0.01F; //so a reginald is ~40%
			}
			
		}
		
		class PlantFood : Food {
			
			internal readonly VanillaFlora plant;
			
			internal PlantFood(VanillaFlora vf, float f, params RegionType[] r) : base(f, r) {
				plant = vf;
			}
			
		}
		
		class ACUMetabolism {
			
			internal readonly bool isCarnivore;
			internal readonly float metabolismPerSecond;
			internal readonly float normalizedPoopChance;
			internal readonly RegionType primaryRegion;
			internal readonly HashSet<RegionType> additionalRegions = new HashSet<RegionType>();
			
			internal ACUMetabolism(float mf, float pp, bool isc, RegionType r, params RegionType[] rr) {
				normalizedPoopChance = pp*0.25F;
				metabolismPerSecond = mf*0.1F*FOOD_SCALAR;
				isCarnivore = isc;
				primaryRegion = r;
				additionalRegions.AddRange(rr.ToList());
			}
			
		}
		
		enum RegionType {
			Shallows,
			Kelp,
			RedGrass,
			Mushroom,
			Jellyshroom,
			Koosh,
			BloodKelp,
			GrandReef,
			LostRiver,
			LavaZone,
			Other,
		}
	}
	
}
