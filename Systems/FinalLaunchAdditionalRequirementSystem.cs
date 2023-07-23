using System;

using System.Collections;
using System.Collections.Generic;
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
	
	public class FinalLaunchAdditionalRequirementSystem {
		
		public static readonly FinalLaunchAdditionalRequirementSystem instance = new FinalLaunchAdditionalRequirementSystem();
		
		private readonly Dictionary<TechType, int> requiredItems = new Dictionary<TechType, int>();
		
		private readonly HashSet<string> biomes = new HashSet<string>();
		private readonly HashSet<BiomeGoal> biomeGoals = new HashSet<BiomeGoal>();
		
		private FinalLaunchAdditionalRequirementSystem() {
			requiredItems[TechType.BoneShark] = 1;
			//requiredItems[TechType.Shocker] = 1;
			requiredItems[TechType.Sandshark] = 1;
			requiredItems[TechType.LavaLizard] = 1;
			requiredItems[TechType.Crabsnake] = 1;
			requiredItems[TechType.Cutefish] = 1;
			requiredItems[TechType.RabbitRay] = 1;
			requiredItems[TechType.SpineEel] = 1;
			requiredItems[TechType.Mesmer] = 1;
			requiredItems[TechType.Jumper] = 2;
			
			requiredItems[SeaToSeaMod.deepStalker.TechType] = 1;
			
			requiredItems[TechType.Bladderfish] = 2;
			requiredItems[TechType.Peeper] = 2;
			requiredItems[TechType.Hoverfish] = 4;
			requiredItems[TechType.Floater] = 6;
			
			requiredItems[TechType.SeaCrownSeed] = 1;
			requiredItems[TechType.MembrainTreeSeed] = 1;
			requiredItems[TechType.FernPalmSeed] = 1;
			requiredItems[TechType.JellyPlant] = 3;
			//requiredItems[TechType.HangingFruit] = 1;
			requiredItems[TechType.JeweledDiskPiece] = 4;
			
			requiredItems[C2CItems.kelp.seed.TechType] = 2;
			requiredItems[C2CItems.healFlower.seed.TechType] = 4;
			requiredItems[C2CItems.mountainGlow.seed.TechType] = 1;
			
			requiredItems[TechType.PrecursorIonCrystal] = 3;
			//requiredItems[TechType.HatchingEnzymes] = 2;
			requiredItems[TechType.Diamond] = 1;
			
			requiredItems[TechType.PrecursorKey_Purple] = 1;
			requiredItems[TechType.PrecursorKey_Orange] = 1;
			requiredItems[TechType.PrecursorKey_Blue] = 1;
			requiredItems[TechType.PrecursorKey_White] = 1;
			requiredItems[TechType.PrecursorKey_Red] = 1;
			
			biomes.Add("dunes");
			biomes.Add("bloodKelp");
			biomes.Add("UnderwaterIslands");
			biomes.Add("mountains");
			biomes.Add("kooshZone");
			//biomes.Add("crashedShip");
			//biomes.Add("cragField"); has no trigger
			biomes.Add("grandreef");
			biomes.Add("SparseReef");
			//biomes.Add("SeaTreaderPath");
			biomes.Add("mushroomForest");
			biomes.Add("grassyPlateaus");
			biomes.Add("kelpForest");
			biomes.Add("FloatingIsland");
			biomes.Add("JellyshroomCaves");
		}
		
		public void addRequiredItem(TechType tt, int amt) {
			requiredItems[tt] = amt;
		}
		
		internal string hasAllCargo(LaunchRocket r) {
			Dictionary<TechType, int> need = new Dictionary<TechType, int>(requiredItems);
			Rocket root = r.gameObject.FindAncestor<Rocket>();
			foreach (RocketLocker l in root.GetComponentsInChildren<RocketLocker>()) {
				StorageContainer sc = l.GetComponent<StorageContainer>();
				foreach (TechType tt in requiredItems.Keys) {
					if (!need.ContainsKey(tt))
						continue;
					int has = sc.container.GetCount(tt);
					if (has >= 0) {
						need[tt] = need[tt]-has;
						if (need[tt] <= 0)
							need.Remove(tt);
					}
				}
			}
			if (need.Count > 0)
				return "Missing cargo "+need.toDebugString<TechType, int>();
			int hero = 0;
			foreach (Creature c in root.GetComponentsInChildren<Creature>(true)) {
				if (c is SandShark) {
					InfectedMixin mix = c.GetComponent<InfectedMixin>();
					//SNUtil.writeToChat("Sandshark is infected: "+(mix && mix.IsInfected()));
					if (!mix || !mix.IsInfected() || mix.IsHealedByPeeper()) {
						return "Sandshark not infected: "+mix+" & "+(mix ? mix.IsInfected()+"+"+mix.IsHealedByPeeper() : "null");
					}
				}
				else if (c is Peeper) {
					if (((Peeper)c).isHero)
						hero++;
				}
			}
			//SNUtil.writeToChat("Sparkle peepers: "+hero);
			return hero < 2 ? "Insufficient ("+hero+") sparkle peepers" : null;
		}
		
		private void generateBiomeGoalList() {
			foreach (string biome in biomes) {
				foreach (BiomeGoal bg in BiomeGoalTracker.main.goalData.goals) {
					if (bg.biome == biome) {
						biomeGoals.Add(bg);
						break;
					}
				}
			}
		}
		
		private bool visitedAllBiomes() {
			if (biomeGoals.Count == 0) {
				generateBiomeGoalList();
			}
			foreach (BiomeGoal s in biomeGoals) {
				if (!StoryGoalManager.main.IsGoalComplete(s.key)) {
					SNUtil.writeToChat("Missing biome goal '"+s+"' in "+s.biome);
					return false;
				}
			}
			return true;
		}
		
		internal void forceLaunch() {
			forceLaunch(UnityEngine.Object.FindObjectOfType<LaunchRocket>());
		}
		
		internal void forceLaunch(LaunchRocket r) {
			LaunchRocket.SetLaunchStarted();
			PlayerTimeCapsule.main.Submit(null);
			r.StartCoroutine(r.StartEndCinematic());
			HandReticle.main.RequestCrosshairHide();
		}
		
		public bool checkIfFullyLoaded(LaunchRocket r) {
			return checkConditionAndShowPDAAndVoicelogIfNot(hasAllCargo(r) == null, "needlaunchcargo", PDAMessages.Messages.NeedLaunchCargoMessage);
		}
		
		public bool checkIfVisitedAllBiomes() {
			return checkConditionAndShowPDAAndVoicelogIfNot(visitedAllBiomes(), "notvisitedallbiomes", PDAMessages.Messages.NotSeenBiomesMessage);
		}
		
		private bool checkConditionAndShowPDAAndVoicelogIfNot(bool check, string page, PDAMessages.Messages msg) {
			if (check) {
				return true;
			}
			else {
				if (PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(msg).key)) {
					PDAManager.getPage(page).unlock(false);
				}
				return false;
			}
		}
	}
	
}
