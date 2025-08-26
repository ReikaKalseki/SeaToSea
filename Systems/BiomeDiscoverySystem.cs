using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.UI;

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
			this.mapBiome(VanillaBiomes.SHALLOWS, "Goal_Lifepod2"); //"Aurora suffered orbital hull failure"
			this.mapBiome(VanillaBiomes.KELP, "Goal_BiomeKelpForest");
			this.mapBiome(VanillaBiomes.REDGRASS, "Goal_BiomeGrassyPlateaus");
			this.mapBiome(VanillaBiomes.MUSHROOM, "Goal_BiomeMushroomForest");
			this.mapBiome(VanillaBiomes.JELLYSHROOM, "Goal_BiomeJellyCave");
			this.mapBiome(VanillaBiomes.DEEPGRAND, "Goal_BiomeDeepGrandReef");
			this.mapBiome(VanillaBiomes.KOOSH, "Goal_BiomeKooshZone");
			this.mapBiome(VanillaBiomes.DUNES, "Goal_BiomeDunes");
			this.mapBiome(VanillaBiomes.CRASH, "Goal_BiomeCrashedShip");
			this.mapBiome(VanillaBiomes.SPARSE, "Goal_BiomeSparseReef");
			this.mapBiome(VanillaBiomes.MOUNTAINS, "Goal_BiomeMountains");
			this.mapBiome(VanillaBiomes.TREADER, "Goal_BiomeSeaTreaderPath");
			this.mapBiome(VanillaBiomes.UNDERISLANDS, "Goal_BiomeUnderwaterIslands");
			this.mapBiome(VanillaBiomes.BLOODKELP, "Goal_BiomeBloodKelp");
			this.mapBiome(VanillaBiomes.BLOODKELPNORTH, "Goal_BiomeBloodKelp2");
			this.mapBiome(VanillaBiomes.LOSTRIVER, "Goal_BiomeLostRiver");
			this.mapBiome(VanillaBiomes.ILZ, "ILZChamber_Dragon"); //"energy in structure in center of chamber"
			this.mapBiome(VanillaBiomes.ALZ, "Emperor_Telepathic_Contact3");
			this.mapBiome(VanillaBiomes.AURORA, "Goal_LocationAuroraEntry");
			this.mapBiome(VanillaBiomes.FLOATISLAND, "Goal_BiomeFloatingIsland");
			this.mapBiome(VanillaBiomes.VOID, "Goal_BiomeVoid");

			//no triggers in vanilla
			this.createTrigger(VanillaBiomes.GRANDREEF, "grandreef");
			this.createTrigger(VanillaBiomes.CRAG, "CragField");
			this.createTrigger(VanillaBiomes.COVE, "LostRiver_TreeCove");

			StoryHandler.instance.registerTickedGoal(StoryHandler.instance.createLocationGoal(WorldUtil.SUNBEAM_SITE, 1000, "Goal_BiomeMountainIsland", WorldUtil.isMountainIsland));

			foreach (CustomBiome cb in BiomeBase.getCustomBiomes()) {
				if (cb.discoveryGoal != null) {
					this.mapBiome(cb, cb.discoveryGoal.key);
				}
				else {
					this.createTrigger(cb, cb.biomeName);
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
			bg.key = "Goal_Biome" + id;
			bg.biome = id;
			bg.delay = 0;
			bg.goalType = Story.GoalType.Story;
			bg.minStayDuration = 2;
			this.mapBiome(bb, bg.key);
			StoryHandler.instance.registerTickedGoal(bg);
		}

		public void NotifyGoalComplete(string key) {
			if (goalMap.ContainsKey(key)) {
				LifeformScanningSystem.instance.onBiomeDiscovered();
			}
		}

		public void forceDiscovery(BiomeBase bb) {
			if (bb == null || bb == BiomeBase.UNRECOGNIZED)
				return;
			if (!basicEntryGoal.ContainsKey(bb)) {
				SNUtil.log("No cached biome goal to apply for biome " + bb);
				return;
			}
			StoryGoal.Execute(basicEntryGoal[bb], Story.GoalType.Story);
		}

		public bool isDiscovered(BiomeBase bb) {
			if (bb == null || bb == BiomeBase.UNRECOGNIZED)
				return true;
			//if (basicEntryGoal.Count == 0) {
			//	generateBiomeGoalList();
			//}
			if (!basicEntryGoal.ContainsKey(bb)) {
				SNUtil.log("No cached biome goal to check for biome " + bb);
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
					SNUtil.writeToChat("Missing biome goal '" + kvp.Value + " for biome " + kvp.Key);
					return false;
				}
			}
			return true;
		}

		public bool checkIfVisitedAllBiomes() {
			return C2CUtil.checkConditionAndShowPDAAndVoicelogIfNot(this.visitedAllBiomes(), "notvisitedallbiomes", PDAMessages.Messages.NotSeenBiomesMessage);
		}

	}

}
