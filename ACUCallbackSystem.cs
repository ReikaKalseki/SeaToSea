using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class ACUCallbackSystem { //TODO make this its own mod and split into many classes
		
		public static readonly ACUCallbackSystem instance = new ACUCallbackSystem();
		
		private static readonly float FOOD_SCALAR = 0.2F; //all food values and metabolism multiplied by this, to give granularity
		private static readonly string ACU_DECO_SLOT_NAME = "ACUDecoHolder";
		
		private readonly Dictionary<TechType, AnimalFood> edibleFish = new Dictionary<TechType, AnimalFood>();
		
		private readonly Dictionary<VanillaFlora, PlantFood> ediblePlants = new Dictionary<VanillaFlora, PlantFood>();
		
		private readonly Dictionary<RegionType, WeightedRandom<ACUPropDefinition>> propTypes = new Dictionary<RegionType, WeightedRandom<ACUPropDefinition>>();
	   
		private readonly Dictionary<string, MaterialPropertyDefinition> terrainGrassTextures = new Dictionary<string, MaterialPropertyDefinition>();
		
		private readonly string rootCachePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "GrassTex");
	    
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
			{TechType.GhostRayBlue, new ACUMetabolism(0.033F, 0.3F, false, RegionType.LostRiver)},
			{TechType.GhostRayRed, new ACUMetabolism(0.06F, 0.3F, false, RegionType.LavaZone)},
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
			
			addFood(new PlantFood(VanillaFlora.CREEPVINE, 0.15F, RegionType.Kelp));
			addFood(new PlantFood(VanillaFlora.CREEPVINE_FERTILE, 0.25F, RegionType.Kelp));
			addFood(new PlantFood(VanillaFlora.BLOOD_KELP, 0.25F, RegionType.BloodKelp));
			addFood(new PlantFood(VanillaFlora.JELLYSHROOM, 0.25F, RegionType.Jellyshroom));
			addFood(new PlantFood(VanillaFlora.EYE_STALK, 0.15F, RegionType.Other));
			addFood(new PlantFood(VanillaFlora.GABE_FEATHER, 0.15F, RegionType.BloodKelp, RegionType.Other));
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
			addFood(new PlantFood(VanillaFlora.WRITHING_WEED, 0.15F, RegionType.Shallows, RegionType.Mushroom));
			addFood(new PlantFood(VanillaFlora.BLUE_PALM, 0.25F, RegionType.Shallows, RegionType.Mushroom));
			addFood(new PlantFood(VanillaFlora.PYGMY_FAN, 0.33F, RegionType.Mushroom));
			addFood(new PlantFood(VanillaFlora.TIGER, 0.5F, RegionType.RedGrass));
			addFood(new PlantFood(VanillaFlora.DEEP_MUSHROOM, 0.1F, RegionType.LostRiver, RegionType.LavaZone));
			
			registerGrassProp(RegionType.Kelp, null, 25, 0.5F);
			registerGrassProp(RegionType.RedGrass, "Coral_reef_red_seaweed_03", 25, 0.5F);
			registerGrassProp(RegionType.RedGrass, "Coral_reef_red_seaweed_02", 25, 0.5F);
			registerGrassProp(RegionType.Koosh, "Coral_reef_small_deco_03_billboards", 15, 0.5F);
			registerGrassProp(RegionType.Koosh, "coral_reef_grass_03_02", 15, 0.5F);
			registerGrassProp(RegionType.GrandReef, "coral_reef_grass_11_02_gr", 25, 0.5F);
			registerGrassProp(RegionType.GrandReef, "coral_reef_grass_07_gr", 25, 0.5F);
			//registerGrassProp(RegionType.GrandReef, "coral_reef_grass_10_gr", 25, 0.5F);
			registerGrassProp(RegionType.BloodKelp, "coral_reef_grass_07_bk", 25, 0.5F);
			registerGrassProp(RegionType.LostRiver, "coral_reef_grass_11_03_lr", 25, 0.5F);
			registerGrassProp(RegionType.LavaZone, "coral_reef_grass_10_lava", 25, 0.5F);
				
			//registerProp(RegionType.Koosh, "eb5ea858-930d-4272-91b5-e9ebe2286ca8", 25, 0.5F);
			
			//foreach (string pfb in VanillaFlora.BLOOD_GRASS.getPrefabs(false, true))
			//	registerProp(RegionType.RedGrass, pfb, 15);
			
			registerProp(RegionType.Mushroom, "961194a9-e88b-40d7-900d-a48c5b739352", 5, false, 0.4F);
			registerProp(RegionType.Mushroom, "fe145621-5b25-4000-a3dd-74c1aaa961e2", 5, false, 0.4F);
			registerProp(RegionType.Mushroom, "f3de21af-550b-4901-a6e8-e45e31c1509d", 5, false, 0.4F);
			registerProp(RegionType.Mushroom, "5086a02a-ea6d-41ba-90c3-ea74d97cf6b5", 5, false, 0.4F);
			registerProp(RegionType.Mushroom, "7c7e0e95-8311-4ee0-80dd-30a61b151161", 5, false, 0.4F);
			
			registerProp(RegionType.BloodKelp, "7bfe0629-a008-43b8-bd16-d69ad056769f", 15, true, prepareBloodTendril);
			registerProp(RegionType.BloodKelp, "e291d076-bf95-4cdd-9dd9-6acd37566cf6", 15, true, prepareBloodTendril);
			registerProp(RegionType.BloodKelp, "2bfcbaf4-1ae6-4628-9816-28a6a26ff340", 15, true, prepareBloodTendril);
			registerProp(RegionType.BloodKelp, "2ab96dc4-5201-4a41-aa5c-908f0a9a0da8", 15, true, prepareBloodTendril);
			registerProp(RegionType.BloodKelp, "18229b4b-3ed3-4b35-ae30-43b1c31a6d8d", 25, true, 0.4F, 0.165F); //blood oil
			/* too finicky
			foreach (string pfb in VanillaFlora.DEEP_MUSHROOM.getPrefabs(false, true)) {
				Action<GameObject> a = go => {
					go.transform.localScale = Vector3.one*0.33F;
					go.transform.localRotation = Quaternion.Euler(UnityEngine.Random.Range(260F, 280F), UnityEngine.Random.Range(0F, 360F)*0, 0);
				};
				registerProp(RegionType.BloodKelp, pfb, 5, true, a);
				//registerProp(RegionType.LostRiver, pfb, 5, a); is a native flora here
				//registerProp(RegionType.LavaZone, pfb, 5, a); and here
			}*/
			
			foreach (string pfb in VanillaFlora.JELLYSHROOM_TINY.getPrefabs(true, true))
				registerProp(RegionType.Jellyshroom, pfb, 5, false);
			
			foreach (string pfb in VanillaFlora.TREE_LEECH.getPrefabs(false, true))
				registerProp(RegionType.Mushroom, pfb, 5, false, 0.25F);
			foreach (string pfb in VanillaFlora.GRUE_CLUSTER.getPrefabs(true, true))
				registerProp(RegionType.Mushroom, pfb, 5, false, 0.00004F); //why the hell is this thing so huge in native scale and vanilla scales it to 0.0001F
			
			registerProp(RegionType.LostRiver, VanillaFlora.BRINE_LILY.getRandomPrefab(false), 10, false, 0.25F);
			foreach (string pfb in VanillaFlora.CLAW_KELP.getPrefabs(true, true))
				registerProp(RegionType.LostRiver, pfb, 5, true, 0.1F, 0, go => go.transform.rotation = Quaternion.Euler(270, 0, 0));
			
			registerProp(RegionType.GrandReef, VanillaFlora.ANCHOR_POD_SMALL1.getRandomPrefab(false), 10, true, 0.1F);
			registerProp(RegionType.GrandReef, VanillaFlora.ANCHOR_POD_SMALL2.getRandomPrefab(false), 10, true, 0.1F);
			
			registerProp(RegionType.LavaZone, "077ebe13-eb45-4ee4-8f6f-f566cfe11ab2", 10, false, 0.5F);
			
			if (Directory.Exists(rootCachePath)) {
				foreach (string folder in Directory.EnumerateDirectories(rootCachePath)) {
					string name = Path.GetFileName(folder);
					try {
						SNUtil.log("Loading cached grass material '"+name+"' from "+folder);
						MaterialPropertyDefinition m = new MaterialPropertyDefinition(name);
						m.readFromFile(SeaToSeaMod.modDLL, folder);
						terrainGrassTextures[m.name] = m;
					}
					catch (Exception ex) {
						SNUtil.log("Could not load cached grass material '"+name+"': "+ex);
					}
				}
			}
			else {
				SNUtil.writeToChat("Grass material cache does not exist at "+rootCachePath+".");
				Directory.CreateDirectory(rootCachePath);
			}
		}
		
		public void cacheGrassMaterial(Material m) {
			string n = m.mainTexture.name.Replace(" (Instance)", "");
			if (!terrainGrassTextures.ContainsKey(n)) {
				MaterialPropertyDefinition def = new MaterialPropertyDefinition(m);
				terrainGrassTextures[n] = def;
				string path = Path.Combine(rootCachePath, n);
				def.writeToFile(path);
				SNUtil.log("Saved grass material '"+n+"' to "+path);
			}
		}
		
		private void prepareBloodTendril(GameObject go) {
			go.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.15F, 0.25F);
			go.transform.rotation = Quaternion.identity;
		}
		
		private void registerGrassProp(RegionType r, string texture, double wt, float scale, float voff = 0) {
			Action<GameObject> a = go => {
			    go.transform.localScale = Vector3.one*UnityEngine.Random.Range(scale*0.95F, scale*1.05F);
				go.transform.position = go.transform.position+Vector3.up*voff;
				if (!string.IsNullOrEmpty(texture)) {
					Renderer rn = go.GetComponentInChildren<Renderer>();
					if (terrainGrassTextures.ContainsKey(texture))
						terrainGrassTextures[texture].applyToMaterial(rn.materials[0], true, false);//.mainTexture = RenderUtil.getVanillaTexture(texture);
					else
						UnityEngine.Object.DestroyImmediate(go);
				}
			};
			registerProp(r, "880b59b7-8fd6-412f-bbcb-a4260b263124", wt*0.75F, false, a);
			registerProp(r, "bac42c90-8995-439f-be2f-29a6d164c82a", wt*0.25F, false, a);
		}
		
		private void registerProp(RegionType r, string s, double wt, bool up, float scale, float voff = 0, Action<GameObject> a = null) {
			registerProp(r, s, wt, up, go => {
			    go.transform.localScale = Vector3.one*UnityEngine.Random.Range(scale*0.95F, scale*1.05F);
				go.transform.position = go.transform.position+Vector3.up*voff;
				if (a != null)
					a(go);
			});
		}
		
		private void registerProp(RegionType r, string s, double wt, bool up, Action<GameObject> a = null) {
			WeightedRandom<ACUPropDefinition> wr = propTypes.ContainsKey(r) ? propTypes[r] : new WeightedRandom<ACUPropDefinition>();
			wr.addEntry(new ACUPropDefinition(s, wt, up, a), wt);
			propTypes[r] = wr;
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
			if (acu && acu.gameObject)
				acu.gameObject.EnsureComponent<ACUCallback>().setACU(acu);
		}
		
		public class ACUCallback : MonoBehaviour {
			
			private WaterPark acu;
			private float lastTick;
			
			private StorageContainer sc;
			private List<WaterParkPiece> column;
			private GameObject lowestSegment;
			private GameObject floor;
			private List<GameObject> decoHolders;
			
			private RegionType currentTheme = RegionType.Shallows;
			private int plantCount;
			private int herbivoreCount;
			private int carnivoreCount;
			private int sparkleCount;
			private float stalkerToyValue;
			
			internal void setACU(WaterPark w) {
				if (acu != w) {
					
					CancelInvoke("tick");
					sc = null;
					column = null;
					decoHolders = null;
					lowestSegment = null;
					floor = null;
					
					acu = w;
					
					if (acu) {
						//SNUtil.writeToChat("Setup ACU Hook");
						SNUtil.log("Switching ACU "+acu+" @ "+acu.transform.position+" to "+this);
						InvokeRepeating("tick", 0, 1);
						sc = acu.planter.GetComponentInChildren<StorageContainer>();
						column = ACUCallbackSystem.instance.getACUComponents(acu);
						lowestSegment = ACUCallbackSystem.instance.getACUFloor(column);
						floor = ObjectUtil.getChildObject(lowestSegment, "Large_Aquarium_Room_generic_ground");
						decoHolders = ObjectUtil.getChildObjects(lowestSegment, ACU_DECO_SLOT_NAME);
					}
				}
			}
		
			public void tick() {
				float time = DayNightCycle.main.timePassedAsFloat;
				float dT = time-lastTick;
				lastTick = time;
				if (dT <= 0.0001)
					return;
				//SNUtil.writeToChat(dT+" s");
				bool healthy = false;
				bool consistent = true;
				HashSet<RegionType> possibleBiomes = new HashSet<RegionType>();
				possibleBiomes.AddRange((IEnumerable<RegionType>)Enum.GetValues(typeof(RegionType)));
				//SNUtil.writeToChat("SC:"+sc);
				PrefabIdentifier[] plants = sc.GetComponentsInChildren<PrefabIdentifier>();
				plantCount = 0;
				herbivoreCount = 0;
				carnivoreCount = 0;
				int teeth = 0;
				sparkleCount = 0;
				//SNUtil.writeToChat("@@"+string.Join(",", possibleBiomes));
				List<WaterParkCreature> foodFish = new List<WaterParkCreature>();
				List<Stalker> stalkers = new List<Stalker>();
				stalkerToyValue = 0;
				foreach (WaterParkItem wp in new List<WaterParkItem>(acu.items)) {
					if (!wp)
						continue;
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					if (tt == TechType.Titanium || tt == TechType.ScrapMetal || tt == TechType.Silver) {
						pp.gameObject.transform.localScale = Vector3.one*0.5F;
						float v = 0;
						switch(tt) {
							case TechType.Titanium:
								v = 0.5F;
								break;
							case TechType.ScrapMetal:
								v = 1;
								break;
							case TechType.Silver:
								v = 2;
								break;
						}
						stalkerToyValue += v;
					}
					else if (tt == TechType.StalkerTooth) {
						pp.gameObject.transform.localScale = Vector3.one*0.125F;
						teeth++;
					}
					else if (wp is WaterParkCreature) {
						if (ACUCallbackSystem.instance.edibleFish.ContainsKey(tt)) {
							if (tt == TechType.Peeper && wp.gameObject.GetComponent<Peeper>().isHero)
								sparkleCount++;
							else //sparkle peepers are always valid
								possibleBiomes = new HashSet<RegionType>(possibleBiomes.Intersect(ACUCallbackSystem.instance.edibleFish[tt].regionType));
							//if (possibleBiomes.Count <= 0)
							//	SNUtil.writeToChat("Biome list empty after "+tt+" > "+edibleFish[tt]);
							//SNUtil.writeToChat(tt+" > "+edibleFish[tt]+" > "+string.Join(",", possibleBiomes));
							foodFish.Add((WaterParkCreature)wp);
							herbivoreCount++;
						}
						else if (ACUCallbackSystem.instance.metabolisms.ContainsKey(tt)) {
							ACUMetabolism am = ACUCallbackSystem.instance.metabolisms[tt];
							if (am.isCarnivore)
								carnivoreCount++;
							else
								herbivoreCount += tt == TechType.Gasopod ? 4 : (tt == TechType.GhostRayRed || tt == TechType.GhostRayBlue ? 3 : 2);
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
								if (tryEat(c, am, plants, out amt, out eaten)) {
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
						if (vf != null && ACUCallbackSystem.instance.ediblePlants.ContainsKey(vf)) {
							PlantFood pf = ACUCallbackSystem.instance.ediblePlants[vf];
							possibleBiomes = new HashSet<RegionType>(possibleBiomes.Intersect(pf.regionType));
							//if (possibleBiomes.Count <= 0)
							//	SNUtil.writeToChat("Biome list empty after "+vf+" > "+pf);
							//SNUtil.writeToChat(vf+" > "+pf+" > "+string.Join(",", possibleBiomes));
							plantTypes.Add(vf);
							plantCount++;
						}
					}
				}
				consistent = possibleBiomes.Count > 0 && plantCount > 0;
				healthy = plantCount > 0 && plantTypes.Count > (possibleBiomes.Count == 1 && possibleBiomes.First<RegionType>() == RegionType.LavaZone ? 0 : 1) && herbivoreCount > 0 && carnivoreCount > 0 && carnivoreCount <= Math.Max(1, herbivoreCount/Mathf.Max(1, 6-sparkleCount*0.5F)) && carnivoreCount <= acu.height*1.5F && herbivoreCount > 0 && herbivoreCount <= plantCount*(4+sparkleCount*0.5F);
				float boost = 0;
				if (consistent)
					boost += 1F;
				if (healthy)
					boost += 2F;
				if (sparkleCount > 0)
					boost *= 1+sparkleCount*0.5F;
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
						float f = dT*stalkerToyValue*0.001F*s.Happy.Value;
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
				if (possibleBiomes.Count == 1) {
					updateACUTheming(possibleBiomes.First<RegionType>());
				}
			}
			
			private void updateACUTheming(RegionType theme) {
				if (theme == RegionType.Other)
					theme = RegionType.Shallows;
				bool changed = theme != currentTheme;
				currentTheme = theme;
				
				string floorTex = Enum.GetName(typeof(RegionType), theme);
				//SNUtil.writeToChat(""+li.Count);
				//SNUtil.writeToChat("##"+theme+" > "+floor+" & "+glass+" & "+decoHolders.Count);
				foreach (Transform t in lowestSegment.transform) {
					string n = t.gameObject.name;
					if (n.StartsWith("Coral_reef_shell_plates", StringComparison.InvariantCulture)) { //because is flat, skip it
						t.gameObject.SetActive(false);
					}
					else if (n.StartsWith("Coral_reef_small_deco", StringComparison.InvariantCulture)) {
						bool flag = true;
						if (decoHolders.Count > 0) {
							foreach (GameObject slot in decoHolders) {
								if (Vector3.Distance(slot.transform.position, t.position) <= 0.05F) {
									UnityEngine.Object.DestroyImmediate(t.gameObject);
									flag = false;
									break;
								}
							}
						}
						if (flag) {
							GameObject slot = new GameObject();
							slot.name = ACU_DECO_SLOT_NAME;
							slot.SetActive(true);
							slot.transform.parent = lowestSegment.transform;
							slot.transform.position = t.position;
							slot.transform.rotation = t.rotation;
							//slot.transform.rotation = Quaternion.identity;
							addProp(t.gameObject, slot, RegionType.Shallows);
							decoHolders.Add(slot);
						}
					}
				}
				foreach (GameObject slot in decoHolders) {
					bool found = false;
					foreach (Transform bt in slot.transform) {
						GameObject biomeSlot = bt.gameObject;
						bool match = biomeSlot.name == Enum.GetName(typeof(RegionType), theme);
						biomeSlot.SetActive(match);
						if (match) {
							found = true;
							if (bt.childCount == 0) {
								ACUPropDefinition def = ACUCallbackSystem.instance.getRandomACUProp(acu, theme);
								//SNUtil.writeToChat("$$"+def);
								//SNUtil.log("$$"+def);
								if (def != null)
									addProp(def.spawn(), slot, theme, biomeSlot);
							}
						}
					}
					if (!found) {
						addProp(null, slot, theme);
					}
				}
				
				if (!changed)
					return;
				
				if (!string.IsNullOrEmpty(floorTex)) {
					Renderer r = floor.GetComponentInChildren<Renderer>();
					Texture2D tex = TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/ACUFloor/"+floorTex);
					if (tex)
						r.material.mainTexture = tex;
				}
				Biome b = getAttr(theme);
				//SNUtil.writeToChat("::"+b);
				if (b != null) {
					mset.Sky biomeSky = WorldUtil.getSkybox(b.biomeName);
					if (biomeSky) {
						foreach (WaterParkPiece wp in column) {
							GameObject glass = ObjectUtil.getChildObject(wp.gameObject, "model/Large_Aquarium_generic_room_glass_01");
							ObjectUtil.setSky(glass, biomeSky);
							Renderer r = glass.GetComponentInChildren<Renderer>();
							if (!r) {
								SNUtil.writeToChat("No glass renderer");
								return;
							}
							Material m = r.materials[0];
							if (!m) {
								SNUtil.writeToChat("No glass material");
								return;
							}
							m.SetFloat("_Fresnel", 0.5F);
							m.SetFloat("_Shininess", 7.5F);
							m.SetFloat("_SpecInt", 0.75F);
							m.SetColor("_Color", b.waterColor);
							m.SetColor("_SpecColor", b.waterColor);
							//m.SetInt("_ZWrite", 1);
						}
						foreach (WaterParkItem wp in acu.items) {
							if (wp)
								ObjectUtil.setSky(wp.gameObject, biomeSky);
						}
						foreach (GameObject go in decoHolders) {
							ObjectUtil.setSky(go, biomeSky);
						}
					}
				}
			}
		
			private void addProp(GameObject go, GameObject slot, RegionType r, GameObject rSlot = null) {
				string rname = Enum.GetName(typeof(RegionType), r);
				if (!rSlot)
					rSlot = ObjectUtil.getChildObject(slot, rname);
				if (!rSlot) {
					rSlot = new GameObject();
					rSlot.name = rname;
					rSlot.transform.parent = slot.transform;
					rSlot.transform.localPosition = Vector3.zero;
					rSlot.transform.localRotation = Quaternion.identity;
				}
				if (go) {
					go.transform.parent = rSlot.transform;
					go.transform.localPosition = Vector3.zero;
					//go.transform.localRotation = Quaternion.identity;
					ObjectUtil.removeComponent<PrefabIdentifier>(go);
					ObjectUtil.removeComponent<TechTag>(go);
					ObjectUtil.removeComponent<Pickupable>(go);
					ObjectUtil.removeComponent<Collider>(go);
					ObjectUtil.removeComponent<PickPrefab>(go);
					ObjectUtil.removeComponent<Light>(go);
					ObjectUtil.removeComponent<SkyApplier>(go);
					SkyApplier sk = go.EnsureComponent<SkyApplier>();
					sk.renderers = go.GetComponentsInChildren<Renderer>(true);
					ObjectUtil.setSky(go, MarmoSkies.main.skyBaseInterior);
				}
			}
		
			private bool tryEat(Creature c, ACUMetabolism am, PrefabIdentifier[] pia, out Food amt, out GameObject eaten) {
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
						if (ACUCallbackSystem.instance.edibleFish.ContainsKey(tt)) {
							eaten = pp.gameObject;
							amt = ACUCallbackSystem.instance.edibleFish[tt];
							//SNUtil.writeToChat(c+" ate a "+tt+" and got "+amt);
							return true;
						}
					}
					amt = null;
					eaten = null;
					return false;
				}
				else if (pia.Length > 0) {
					int idx = UnityEngine.Random.Range(0, pia.Length);
					PrefabIdentifier tt = pia[idx];
					if (tt) {
						VanillaFlora vf = VanillaFlora.getFromID(tt.ClassId);
						//SNUtil.writeToChat(tt+" > "+vf+" > "+ediblePlants.ContainsKey(vf));
						if (vf != null && ACUCallbackSystem.instance.ediblePlants.ContainsKey(vf)) {
							amt = ACUCallbackSystem.instance.ediblePlants[vf];
							//SNUtil.writeToChat(c+" ate a "+vf+" and got "+amt);
							eaten = tt.gameObject;
							return true;
						}
					}
				}
				amt = null;
				eaten = null;
				return false;
			}
		}
		
		private ACUPropDefinition getRandomACUProp(WaterPark acu, RegionType r) {
			return propTypes.ContainsKey(r) ? propTypes[r].getRandomEntry() : null;
		}
		
		private List<WaterParkPiece> getACUComponents(WaterPark acu) {
			List<WaterParkPiece> li = new List<WaterParkPiece>();
			foreach (WaterParkPiece wp in acu.transform.parent.GetComponentsInChildren<WaterParkPiece>()) {
				if (wp && wp.name.ToLowerInvariant().Contains("bottom") && wp.GetBottomPiece().GetModule() == acu)
					li.Add(wp);
			}
			return li;
		}
		
		private GameObject getACUFloor(List<WaterParkPiece> li) {
			foreach (WaterParkPiece wp in li) {
				if (wp.floorBottom && wp.floorBottom.activeSelf && wp.IsBottomPiece())
					return wp.floorBottom;
			}
			return null;
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
				metabolismPerSecond = mf*0.033F;
				isCarnivore = isc;
				primaryRegion = r;
				additionalRegions.AddRange(rr.ToList());
			}
			
			public override string ToString()
			{
				return string.Format("[ACUMetabolism IsCarnivore={0}, MetabolismPerSecond={1}, NormalizedPoopChance={2}, PrimaryRegion={3}, AdditionalRegions=[{4}]]]", isCarnivore, metabolismPerSecond.ToString("0.0000"), normalizedPoopChance, primaryRegion, string.Join(",", additionalRegions));
			}			
		}
		
		class ACUPropDefinition {
			
			private readonly double weight;
			private readonly string prefab;
			private readonly bool forceUpright;
			private readonly Action<GameObject> modify;
			
			internal ACUPropDefinition(string pfb, double wt, bool up, Action<GameObject> a = null) {
				weight = wt;
				prefab = pfb;
				modify = a;
				forceUpright = up;
			}
			
			internal GameObject spawn() {
				GameObject go = ObjectUtil.createWorldObject(prefab, true, false);
				if (go == null) {
					SNUtil.writeToChat("Could not spawn GO for "+this);
					return null;
				}
				Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
				if (rs.Length == 1)
					go = rs[0].gameObject;//go.GetComponentInChildren<Renderer>(true).gameObject;
				go.SetActive(true);
				if (forceUpright)
					go.transform.rotation = Quaternion.identity;
				if (modify != null)
					modify(go);
				return go;
			}
			
			public override string ToString()
			{
				return string.Format("[ACUPropDefinition Weight={0}, Prefab={1}]", weight, prefab);
			}
			
		}
		
		enum RegionType {
			[Biome("SafeShallows", 1F, 1F, 1F, 0.3F)]Shallows,
			[Biome("KelpForest", 0.3F, 0.6F, 0.3F, 0.67F)]Kelp,
			[Biome("GrassyPlateaus", 1F, 1F, 1F, 0.3F)]RedGrass,
			[Biome("MushroomForest", 1F, 1F, 1F, 0.3F)]Mushroom,
			[Biome("JellyshroomCaves", 0.8F, 0.2F, 0.5F, 0.8F)]Jellyshroom,
			[Biome("KooshZone", 0.6F, 0.3F, 0.8F, 0.8F)]Koosh,
			[Biome("BloodKelp", 0, 0, 0, 0.95F)]BloodKelp,
			[Biome("GrandReef", 0, 0, 0.5F, 0.9F)]GrandReef,
			[Biome("lostriver_bonesfield", 0.1F, 0.5F, 0.2F, 0.92F)]LostRiver,
			[Biome("ilzchamber", 0.7F, 0.5F, 0.1F, 0.75F)]LavaZone,
			[Biome("Dunes", 0.1F, 0.4F, 0.7F, 0.5F)]Other,
		}
		
		private class Biome : Attribute {
			
			public readonly string biomeName;
			internal readonly Color waterColor;
			
			internal Biome(string b, float r, float g, float bl, float a) {
				biomeName = b;
				waterColor = new Color(r, g, bl, a);
			}
			
			public override string ToString() {
				return biomeName;
			}
		}
		
		private static Biome getAttr(RegionType key) {
			System.Reflection.FieldInfo info = typeof(RegionType).GetField(Enum.GetName(typeof(RegionType), key));
			return (Biome)Attribute.GetCustomAttribute(info, typeof(Biome));
		}
	}
	
}
