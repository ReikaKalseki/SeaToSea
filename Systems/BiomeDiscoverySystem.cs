using System;

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class BiomeDiscoverySystem : IStoryGoalListener {
		
		public static readonly BiomeDiscoverySystem instance = new BiomeDiscoverySystem();
		
		//private readonly HashSet<string> biomes = new HashSet<string>();
		
		private readonly Dictionary<BiomeBase, string> basicEntryGoal = new Dictionary<BiomeBase, string>();
		private readonly Dictionary<BiomeBase, string> exploreGoal = new Dictionary<BiomeBase, string>();
		private readonly Dictionary<string, BiomeBase> goalMap = new Dictionary<string, BiomeBase>();
		
		private BiomeDiscoverySystem() {
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);	
		}
		
		public void register() {			
			mapBiome(VanillaBiomes.SHALLOWS, "Goal_Lifepod2"); //"Aurora suffered orbital hull failure"
			mapBiome(VanillaBiomes.KELP, "Goal_BiomeKelpForest");
			mapBiome(VanillaBiomes.REDGRASS, "Goal_BiomeGrassyPlateaus");
			mapBiome(VanillaBiomes.MUSHROOM, "Goal_BiomeMushroomForest");
			mapBiome(VanillaBiomes.JELLYSHROOM, "Goal_BiomeJellyCave");
			mapBiome(VanillaBiomes.DEEPGRAND, "Goal_BiomeDeepGrandReef");
			mapBiome(VanillaBiomes.KOOSH, "Goal_BiomeKooshZone");
			mapBiome(VanillaBiomes.DUNES, "Goal_BiomeDunes");
			mapBiome(VanillaBiomes.CRASH, "Goal_BiomeCrashedShip");
			mapBiome(VanillaBiomes.SPARSE, "Goal_BiomeSparseReef");
			mapBiome(VanillaBiomes.MOUNTAINS, "Goal_BiomeMountains");
			mapBiome(VanillaBiomes.TREADER, "Goal_BiomeSeaTreaderPath");
			mapBiome(VanillaBiomes.UNDERISLANDS, "Goal_BiomeUnderwaterIslands");
			mapBiome(VanillaBiomes.BLOODKELP, "Goal_BiomeBloodKelp");
			mapBiome(VanillaBiomes.BLOODKELPNORTH, "Goal_BiomeBloodKelp2");
			mapBiome(VanillaBiomes.LOSTRIVER, "Goal_BiomeLostRiver");
			mapBiome(VanillaBiomes.ILZ, "ILZChamber_Dragon"); //"energy in structure in center of chamber"
			mapBiome(VanillaBiomes.ALZ, "Emperor_Telepathic_Contact3");
			mapBiome(VanillaBiomes.AURORA, "Goal_LocationAuroraEntry");
			mapBiome(VanillaBiomes.FLOATISLAND, "Goal_BiomeFloatingIsland");
			mapBiome(VanillaBiomes.VOID, "Goal_BiomeVoid");
			
			//no triggers in vanilla
			createTrigger(VanillaBiomes.GRANDREEF, "grandreef");
			createTrigger(VanillaBiomes.CRAG, "CragField");
			createTrigger(VanillaBiomes.COVE, "LostRiver_TreeCove");
			
			StoryHandler.instance.registerTickedGoal(StoryHandler.instance.createLocationGoal(WorldUtil.SUNBEAM_SITE, 1000, "Goal_BiomeMountainIsland", WorldUtil.isMountainIsland));
						
			foreach (CustomBiome cb in BiomeBase.getCustomBiomes()) {
				if (cb.discoveryGoal != null) {
					mapBiome(cb, cb.discoveryGoal.key);
				}
				else {
					createTrigger(cb, cb.biomeName);
				}
			}
			
			StoryHandler.instance.addListener(this);
		}
		/*
		private void generateBiomeGoalList() {
			/*
			foreach (string biome in biomes) {
				foreach (BiomeGoal bg in BiomeGoalTracker.main.goalData.goals) {
					if (bg.biome == biome) {
						biomeGoals.Add(bg);
						break;
					}
				}
			}*//*
			
			foreach (BiomeGoal bg in BiomeGoalTracker.main.goalData.goals) {
				if (bg.key.StartsWith("Goal_Biome", StringComparison.InvariantCultureIgnoreCase)) {
					BiomeBase bb = BiomeBase.getBiome(bg.biome);
					if (bb == null) {
						SNUtil.log("Skipping handling of biome goal '"+bg.key+"', unrecognized biome '"+bg.biome+"'");
						continue;
					}
					if (basicEntryGoal.ContainsKey(bb)) {
						SNUtil.log("Multiple biome goals '"+bg.key+"' + '"+basicEntryGoal[bb].key+"', for biome '"+bg.biome+"'");
					}
					basicEntryGoal[bb] = bg;
					goalMap[bg.key] = bb;
				}
			}
			
			foreach (BiomeBase bb in BiomeBase.getAllBiomes()) {
				if (!basicEntryGoal.ContainsKey(bb)) {
					basicEntryGoal[bb] = new BiomeGoal();
					basicEntryGoal[bb].biome = bb.displayName;
				}
			}
		}*/
		
		private void mapBiome(BiomeBase bb, string goal) {
			basicEntryGoal[bb] = goal;
			goalMap[goal] = bb;
		}
		
		private void createTrigger(BiomeBase bb, string id) {
			BiomeGoal bg = new BiomeGoal();
			bg.key = "Goal_Biome"+id;
			bg.biome = id;
			bg.delay = 0;
			bg.goalType = Story.GoalType.Story;
			bg.minStayDuration = 2;
			mapBiome(bb, bg.key);
			StoryHandler.instance.registerTickedGoal(bg);
		}
		
		public void NotifyGoalComplete(string key) {
			if (goalMap.ContainsKey(key)) {
				LifeformScanningSystem.instance.onBiomeDiscovered();
			}
		}
		
		public bool isDiscovered(BiomeBase bb) {
			if (bb == null || bb == BiomeBase.UNRECOGNIZED)
				return true;
			//if (basicEntryGoal.Count == 0) {
			//	generateBiomeGoalList();
			//}
			if (!basicEntryGoal.ContainsKey(bb)) {
				SNUtil.log("No cached biome goal to check for biome "+bb);
				return true;
			}
			return StoryGoalManager.main.IsGoalComplete(basicEntryGoal[bb]);
		}
		
		public bool visitedAllBiomes() {
			//if (basicEntryGoal.Count == 0) {
			//	generateBiomeGoalList();
			//}
			foreach (KeyValuePair<BiomeBase, string> kvp in basicEntryGoal) {
				if (!StoryGoalManager.main.IsGoalComplete(kvp.Value)) {
					SNUtil.writeToChat("Missing biome goal '"+kvp.Value+" for biome "+kvp.Key);
					return false;
				}
			}
			return true;
		}
		
		public bool checkIfVisitedAllBiomes() {
			return SeaToSeaMod.checkConditionAndShowPDAAndVoicelogIfNot(visitedAllBiomes(), "notvisitedallbiomes", PDAMessages.Messages.NotSeenBiomesMessage);
		}
		
	}
	
}
