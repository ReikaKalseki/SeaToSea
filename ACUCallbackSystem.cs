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
			addFood(new PlantFood(VanillaFlora.BLOOD_KELP, 0.15F, RegionType.BloodKelp));
			addFood(new PlantFood(VanillaFlora.JELLYSHROOM, 0.25F, RegionType.Jellyshroom));
			addFood(new PlantFood(VanillaFlora.EYE_STALK, 0.15F, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.GABE_FEATHER, 0.25F, RegionType.LostRiver, RegionType.BloodKelp, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.GHOSTWEED, 0.25F, RegionType.LostRiver));
			addFood(new PlantFood(VanillaFlora.HORNGRASS, 0.05F, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.KOOSH, 0.15F, RegionType.Koosh));
			addFood(new PlantFood(VanillaFlora.MEMBRAIN, 0.3F, RegionType.GrandReef));
			addFood(new PlantFood(VanillaFlora.PAPYRUS, 0.15F, RegionType.RedGrass, RegionType.Jellyshroom, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.VIOLET_BEAU, 0.2F, RegionType.Jellyshroom, RegionType.RedGrass, RegionType.Koosh, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.CAVE_BUSH, 0.05F, RegionType.Koosh, RegionType.Jellyshroom, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.REGRESS, 0.2F, RegionType.GrandReef, RegionType.Other));
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
			if (dT <= 0.0001)
				return;
			bool healthy = false;
			bool consistent = true;
			HashSet<RegionType> possibleBiomes = new HashSet<RegionType>();
			possibleBiomes.AddRange((IEnumerable<RegionType>)Enum.GetValues(typeof(RegionType)));
			StorageContainer sc = acu.planter.GetComponentInChildren<StorageContainer>();
			PrefabIdentifier[] plants = sc.GetComponentsInChildren<PrefabIdentifier>();
			int plant = 0;
			int herb = 0;
			int carn = 0;
			int hero = 0;
			int teeth = 0;
			//SNUtil.writeToChat("@@"+string.Join(",", possibleBiomes));
			List<WaterParkCreature> foodFish = new List<WaterParkCreature>();
			List<Stalker> stalkers = new List<Stalker>();
			List<Pickupable> stalkerToys = new List<Pickupable>();
			foreach (WaterParkItem wp in new List<WaterParkItem>(acu.items)) {
				Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
				TechType tt = pp ? pp.GetTechType() : TechType.None;
				if (tt == TechType.Titanium || tt == TechType.ScrapMetal || tt == TechType.Silver) {
					pp.gameObject.transform.localScale = Vector3.one*0.5F;
					stalkerToys.Add(pp);
				}
				else if (tt == TechType.StalkerTooth) {
					pp.gameObject.transform.localScale = Vector3.one*0.125F;
					teeth++;
				}
				else if (wp is WaterParkCreature) {
					if (edibleFish.ContainsKey(tt)) {
						if (tt == TechType.Peeper && wp.gameObject.GetComponent<Peeper>().isHero)
							hero++;
						else //sparkle peepers are always valid
							possibleBiomes = new HashSet<RegionType>(possibleBiomes.Intersect(edibleFish[tt].regionType));
						//if (possibleBiomes.Count <= 0)
						//	SNUtil.writeToChat("Biome list empty after "+tt+" > "+edibleFish[tt]);
						//SNUtil.writeToChat(tt+" > "+edibleFish[tt]+" > "+string.Join(",", possibleBiomes));
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
						//SNUtil.writeToChat(tt+" > "+am+" > "+string.Join(",", possibleBiomes));
						//if (possibleBiomes.Count <= 0)
						//	SNUtil.writeToChat("Biome list empty after "+tt+" > "+am);
						Creature c = wp.gameObject.GetComponentInChildren<Creature>();
						c.Hunger.Add(dT*am.metabolismPerSecond*FOOD_SCALAR);
						c.Hunger.Falloff = 0;
						if (tt == TechType.Stalker) {
							stalkers.Add((Stalker)c);
						}
						if (c.Hunger.Value >= 0.5F) {
							Food amt;
							GameObject eaten;
							if (tryEat(c, acu, am, sc, plants, out amt, out eaten)) {
								float food = amt.foodValue*FOOD_SCALAR;
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
								InfectedMixin inf = eaten ? eaten.GetComponent<InfectedMixin>() : null;
								if (inf && inf.IsInfected()) {
									food *= 0.25F;
									c.gameObject.EnsureComponent<InfectedMixin>().IncreaseInfectedAmount(0.2F);
								}
								if (c.Hunger.Value >= food) {
									c.Happy.Add(0.25F);
									c.Hunger.Add(-food);
									float f = am.normalizedPoopChance*amt.foodValue*Mathf.Pow(((WaterParkCreature)wp).age, 2F);
									//SNUtil.writeToChat(c+" ate > "+f);
									amt.consume(c, acu, sc, eaten);
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
			HashSet<VanillaFlora> plantTypes = new HashSet<VanillaFlora>();
			foreach (PrefabIdentifier pi in plants) {
				if (pi) {
					VanillaFlora vf = VanillaFlora.getFromID(pi.ClassId);
					if (vf != null && ediblePlants.ContainsKey(vf)) {
						PlantFood pf = ediblePlants[vf];
						possibleBiomes = new HashSet<RegionType>(possibleBiomes.Intersect(pf.regionType));
						//if (possibleBiomes.Count <= 0)
						//	SNUtil.writeToChat("Biome list empty after "+vf+" > "+pf);
						//SNUtil.writeToChat(vf+" > "+pf+" > "+string.Join(",", possibleBiomes));
						plantTypes.Add(vf);
						plant++;
					}
				}
			}
			if (possibleBiomes.Count == 1) {
				updateACUTheming(acu, possibleBiomes.First<RegionType>());
			}
			consistent = possibleBiomes.Count > 0 && plant > 0;
			healthy = plant > 0 && plantTypes.Count > 1 && herb > 0 && carn > 0 && carn <= Math.Max(1, herb/Mathf.Max(1, 6-hero*0.5F)) && carn <= acu.height*1.5F && herb > 0 && herb <= plant*(4+hero*0.5F);
			float boost = 0;
			if (consistent)
				boost += 1F;
			if (healthy)
				boost += 2F;
			if (hero > 0)
				boost *= 1+hero*0.5F;
			//SNUtil.writeToChat(plant+"/"+herb+"/"+carn+"$"+hero+" & "+string.Join(", ", possibleBiomes)+" > "+healthy+" & "+consistent+" > "+boost);
			if (boost > 0) {
				boost *= dT;
				foreach (WaterParkCreature wp in foodFish) {
					//SNUtil.writeToChat(wp+" > "+boost+" > "+wp.matureTime+"/"+wp.timeNextBreed);
					if (wp.canBreed) {
						Peeper pp = wp.gameObject.GetComponent<Peeper>();
						if (pp && pp.isHero)
							wp.timeNextBreed = DayNightCycle.main.timePassedAsFloat+1000; //prevent sparkle peepers from breeding
						else if (wp.isMature)
							wp.timeNextBreed -= boost;
						else
							wp.matureTime -= boost;
					}
				}
			}
			if (teeth < 10 && consistent && healthy && possibleBiomes.Contains(RegionType.Kelp)) {
				foreach (Stalker s in stalkers) {
					float f = dT*stalkerToys.Count*0.001F*s.Happy.Value;
					//SNUtil.writeToChat(s.Happy.Value+" x "+stalkerToys.Count+" > "+f);
					if (UnityEngine.Random.Range(0F, 1F) < f) {
						//do not use, so can have ref to GO; reimplement // s.LoseTooth();
						GameObject go = UnityEngine.Object.Instantiate<GameObject>(s.toothPrefab);
						//SNUtil.writeToChat(s+" > "+go);
						go.transform.position = s.loseToothDropLocation.transform.position;
						go.transform.rotation = s.loseToothDropLocation.transform.rotation;
						if (go.activeSelf && s.isActiveAndEnabled) {
							foreach (Collider c in go.GetComponentsInChildren<Collider>())
								Physics.IgnoreCollision(s.stalkerBodyCollider, c);
						}
						Utils.PlayFMODAsset(s.loseToothSound, go.transform, 8f);
						LargeWorldEntity.Register(go);
						acu.AddItem(go.GetComponent<Pickupable>());
					}
				}
			}
		}
		
		private void updateACUTheming(WaterPark acu, RegionType theme) {
			string floorTex = Enum.GetName(typeof(RegionType), theme);
			GameObject container = getACUFloor(acu);
			if (!container)
				return;
			GameObject floor = ObjectUtil.getChildObject(container, "Large_Aquarium_Room_generic_ground");
			switch(theme) {
				case RegionType.Shallows:
					break;
				case RegionType.Kelp:
					break;
				case RegionType.RedGrass: //use shallows sand
					break;
				case RegionType.Mushroom:
					break;
				case RegionType.Jellyshroom:
					break;
				case RegionType.Koosh: //use shallows sand
					break;
				case RegionType.BloodKelp:
					break;
				case RegionType.GrandReef:
					break;
				case RegionType.LostRiver:
					break;
				case RegionType.LavaZone:
					break;
				case RegionType.Other:
					updateACUTheming(acu, RegionType.Shallows);
					return;
			}
			if (!string.IsNullOrEmpty(floorTex)) {
				Renderer r = floor.GetComponentInChildren<Renderer>();
				Texture2D tex = TextureManager.getTexture("Textures/ACUFloor/"+floorTex);
				if (tex)
					r.material.mainTexture = tex;
			}
		}
		
		private GameObject getACUFloor(WaterPark acu) {
			foreach (WaterParkPiece wp in acu.transform.parent.GetComponentsInChildren<WaterParkPiece>()) {
				if (wp.floorBottom && Vector3.Distance(wp.transform.position, acu.transform.position) <= 0.5)
					return wp.floorBottom;
			}
			return null;
		}
		
		private bool tryEat(Creature c, WaterPark acu, ACUMetabolism am, StorageContainer sc, PrefabIdentifier[] pia, out Food amt, out GameObject eaten) {
			if (am.isCarnivore) {
				WaterParkItem wp = acu.items[UnityEngine.Random.Range(0, acu.items.Count)];
				if (wp) {
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					if (tt == TechType.Peeper && wp.gameObject.GetComponent<Peeper>().isHero) { //do not allow eating sparkle peepers
						amt = null;
						eaten = null;
						return false;
					}
					//SNUtil.writeToChat(pp+" > "+tt+" > "+edibleFish.ContainsKey(tt));
					if (edibleFish.ContainsKey(tt)) {
						eaten = pp.gameObject;
						amt = edibleFish[tt];
						//SNUtil.writeToChat(c+" ate a "+tt+" and got "+amt);
						return true;
					}
				}
				amt = null;
				eaten = null;
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
						eaten = tt.gameObject;
						return true;
					}
				}
				amt = null;
				eaten = null;
				return false;
			}
		}
		
		abstract class Food {
			
			internal readonly float foodValue;
			internal readonly HashSet<RegionType> regionType = new HashSet<RegionType>();
			
			internal Food(float f, params RegionType[] r) {
				foodValue = f;
				regionType.AddRange(r.ToList());
			}
			
			internal bool isRegion(RegionType r) {
				return regionType.Contains(r);
			}
			
			public override string ToString()
			{
				return string.Format("[Food FoodValue={0}, RegionType=[{1}]]", foodValue, string.Join(",", regionType));
			}
			
			internal abstract void consume(Creature c, WaterPark acu, StorageContainer sc, GameObject go);
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
			
			internal override void consume(Creature c, WaterPark acu, StorageContainer sc, GameObject go) {
				acu.RemoveItem(go.GetComponent<WaterParkCreature>());
				UnityEngine.Object.DestroyImmediate(go);
			}
			
		}
		
		class PlantFood : Food {
			
			internal readonly VanillaFlora plant;
			
			internal PlantFood(VanillaFlora vf, float f, params RegionType[] r) : base(f, r) {
				plant = vf;
			}
			
			internal override void consume(Creature c, WaterPark acu, StorageContainer sc, GameObject go) {
				LiveMixin lv = go.GetComponent<LiveMixin>();
				if (lv && lv.IsAlive())
					lv.TakeDamage(10, c.transform.position, DamageType.Normal, c.gameObject);
				else
					sc.container.DestroyItem(CraftData.GetTechType(go));
			}
			
		}
		
		class ACUMetabolism {
			
			internal readonly bool isCarnivore;
			internal readonly float metabolismPerSecond;
			internal readonly float normalizedPoopChance;
			internal readonly RegionType primaryRegion;
			internal readonly HashSet<RegionType> additionalRegions = new HashSet<RegionType>();
			
			internal ACUMetabolism(float mf, float pp, bool isc, RegionType r, params RegionType[] rr) {
				normalizedPoopChance = pp*2;
				metabolismPerSecond = mf*0.05F;
				isCarnivore = isc;
				primaryRegion = r;
				additionalRegions.AddRange(rr.ToList());
			}
			
			public override string ToString()
			{
				return string.Format("[ACUMetabolism IsCarnivore={0}, MetabolismPerSecond={1}, NormalizedPoopChance={2}, PrimaryRegion={3}, AdditionalRegions=[{4}]]]", isCarnivore, metabolismPerSecond.ToString("0.0000"), normalizedPoopChance, primaryRegion, string.Join(",", additionalRegions));
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
