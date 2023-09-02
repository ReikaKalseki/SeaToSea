﻿using System;

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
	
	public class FinalLaunchAdditionalRequirementSystem {
		
		public static readonly FinalLaunchAdditionalRequirementSystem instance = new FinalLaunchAdditionalRequirementSystem();
		
		private readonly Dictionary<TechType, RequiredItem> requiredItems = new Dictionary<TechType, RequiredItem>();
		
		private readonly HashSet<string> biomes = new HashSet<string>();
		private readonly HashSet<BiomeGoal> biomeGoals = new HashSet<BiomeGoal>();
		
		internal static readonly string NEED_CARGO_PDA = "needlaunchcargo";
		
		private FinalLaunchAdditionalRequirementSystem() {
			addRequiredItem(TechType.BoneShark, 1, "A large, armored, territorial predator");
			addRequiredItem(TechType.Sandshark, 1, "A burrowing ambush predator, infected with Kharaa").setSorting(2000).setAdditionalCheck(pp => {
				InfectedMixin mix = pp.GetComponent<InfectedMixin>();
				//SNUtil.writeToChat("Sandshark is infected: "+(mix && mix.IsInfected()));
				return mix && mix.IsInfected() && !mix.IsHealedByPeeper();
			});
			addRequiredItem(TechType.LavaLizard, 1, "A creature with extreme thermal resistance due to regular direct lava exposure");
			addRequiredItem(TechType.Crabsnake, 1, "A symbiotic predator");
			addRequiredItem(TechType.Cutefish, 1, "A highly intelligent herbivore");
			addRequiredItem(TechType.RabbitRay, 1, "A small ray species with vibration-detection capabilities, suitable for aquariums");
			addRequiredItem(TechType.SpineEel, 1, "A transparent-bodied predator");
			addRequiredItem(TechType.Mesmer, 1, "A predator that uses psychological manipulation");
			addRequiredItem(TechType.Jumper, 2, "A small cave-dwelling scavenger");
			
			addRequiredItem(SeaToSeaMod.deepStalker.TechType, 1, "A semi-intelligent predator with a strong attraction to shiny objects, adapted for deep water");
			
			addRequiredItem(TechType.Bladderfish, 2, "A fish with water filtering capabilities");
			addRequiredItem(TechType.Peeper, 2, "Enzyme host peeper").setSorting(2000).setAdditionalCheck(pp => {
				Creature c = pp.GetComponent<Creature>();
				return c is Peeper && ((Peeper)c).isHero;
			});
			addRequiredItem(TechType.Hoverfish, 4, "A small herbivore using ionic charge to maneuver, suitable as a small pet");
			addRequiredItem(TechType.Floater, 6, "A parasitic lifeform consisting of two symbiotic components");
			
			addRequiredItem(TechType.SeaCrownSeed, 1, "Flora with an internal bacteria-rich chamber");
			addRequiredItem(TechType.MembrainTreeSeed, 1, "Flora with a large bell filled with microbial and coral colonies");
			addRequiredItem(TechType.FernPalmSeed, 1, "Flora exhibiting signs of genetic modification");
			addRequiredItem(TechType.JellyPlant, 3, "An edible and low-density flora sample");
			addRequiredItem(TechType.JeweledDiskPiece, 4, "Coral containing rare resource nodules");
			
			addRequiredItem(C2CItems.kelp.seed.TechType, 2, "Flora with symbiotic chemosynethetic bacteria in the leaves");
			addRequiredItem(C2CItems.healFlower.seed.TechType, 4, "Leaves coated in oils suitable for medical applications");
			addRequiredItem(C2CItems.mountainGlow.seed.TechType, 1, "Aggressively parasitic flora that eats its host from within");
			addRequiredItem(C2CItems.sanctuaryPlant.seed.TechType, 2, "A glowing seed-bearing organ from a plant on the verge of extinction");
			
			addRequiredItem(TechType.PrecursorIonCrystal, 3, "Alien Power Storage Units").setSorting(1000);
			addRequiredItem(TechType.Diamond, 1, "*").setSorting(1000);
			
			addRequiredItem(TechType.PrecursorKey_Purple, 1, "*").setSorting(3000);
			addRequiredItem(TechType.PrecursorKey_Orange, 1, "*").setSorting(3000);
			addRequiredItem(TechType.PrecursorKey_Blue, 1, "*").setSorting(3000);
			addRequiredItem(TechType.PrecursorKey_White, 1, "*").setSorting(3000);
			addRequiredItem(TechType.PrecursorKey_Red, 1, "*").setSorting(3000);
			
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
		
		public RequiredItem addRequiredItem(TechType tt, int amt, string desc) {
			RequiredItem ri = new RequiredItem(tt, amt, desc);
			requiredItems[tt] = ri;
			return ri;
		}
		
		internal string hasAllCargo() {
			foreach (RequiredItem ri in requiredItems.Values) {
				if (ri.currentlyHas < ri.count)
					return "Missing cargo: "+ri;
			}
			return null;
		}
		
		internal void updateCounts(List<StorageContainer> lockers) {
			foreach (RequiredItem ri in requiredItems.Values) {
				ri.currentlyHas = 0;
			}
			foreach (StorageContainer sc in lockers) {
				foreach (KeyValuePair<TechType, ItemsContainer.ItemGroup> kvp in sc.container._items) {
					TechType tt = kvp.Key;
					if (!requiredItems.ContainsKey(tt))
						continue;
					RequiredItem ri = requiredItems[tt];
					//SNUtil.writeToChat("Checking list of "+ri+": "+kvp.Value.items.toDebugString());
					foreach (InventoryItem ii in kvp.Value.items) {
						if (ri.match(ii.item))
							ri.currentlyHas++;
						//else
						//	SNUtil.writeToChat("Match failed on "+ii+" vs "+ri);
					}
				}
			}
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
		
		public bool checkIfFullyLoaded() {
			return SeaToSeaMod.checkConditionAndShowPDAAndVoicelogIfNot(hasAllCargo() == null, NEED_CARGO_PDA, PDAMessages.Messages.NeedLaunchCargoMessage);
		}
		/*
		public bool checkIfVisitedAllBiomes() {
			return checkConditionAndShowPDAAndVoicelogIfNot(visitedAllBiomes(), "notvisitedallbiomes", PDAMessages.Messages.NotSeenBiomesMessage);
		}
		*/
			
		internal void updateContentsAndPDAPageChecklist(Rocket r, List<StorageContainer> lockers) {
			updateCounts(lockers);
			PDAManager.getPage(FinalLaunchAdditionalRequirementSystem.NEED_CARGO_PDA).update(generatePDAContent(), true);
		}
		
		private string generatePDAContent(/*Dictionary<TechType, RequiredItem> missing*/) {
			string desc = SeaToSeaMod.pdaLocale.getEntry(NEED_CARGO_PDA).pda+"\n";
			List<RequiredItem> li = requiredItems.Values.ToList();
			li.Sort();
			foreach (RequiredItem ri in li) {
				//int has = missing.ContainsKey(ri.item) ? ri.count-missing[ri.item] : ri.count;
				int has = ri.currentlyHas;
				//SNUtil.writeToChat("Currently has "+has+" of "+ri);
				string color = has < ri.count ? (has == 0 ? "FF2040" : "FFE020") : "20FF40";
				desc += string.Format("\t- {1} (<color=#{0}>{2}/{3}</color>)\n\n", color, ri.getDesc(), has, ri.count);
			}
			return desc;
		}
	}
	
	public class RequiredItem : IComparable<RequiredItem> {
		
		public readonly TechType item;
		public readonly int count;
		private readonly string description;
		
		private readonly string defaultSorting;
		
		private Func<Pickupable, bool> extraCheck;
		private int sortIndex;
		
		internal int currentlyHas = 0;
		
		internal RequiredItem(TechType tt, int amt, string desc) {
			item = tt;
			count = amt;
			description = desc;
			string s = desc.Trim();
			while (s.StartsWith("A ", StringComparison.InvariantCultureIgnoreCase))
				s = s.Substring(2);
			while (s.StartsWith("An ", StringComparison.InvariantCultureIgnoreCase))
				s = s.Substring(3);
			while (s.StartsWith("The ", StringComparison.InvariantCultureIgnoreCase))
				s = s.Substring(4);
			while (s.StartsWith("Flora ", StringComparison.InvariantCultureIgnoreCase))
				s = s.Substring(6);
			defaultSorting = s;
		}
		
		internal RequiredItem setAdditionalCheck(Func<Pickupable, bool> check) {
			extraCheck = check;
			return this;
		}
		
		internal RequiredItem setSorting(int idx) {
			sortIndex = idx;
			return this;
		}
		
		public bool match(Pickupable pp) {
			return extraCheck == null || extraCheck.Invoke(pp);
		}
		
		public string getDesc() {
			return description == "*" ? cleanString(Language.main.Get("Tooltip_"+item.AsString())) : description;
		}
		
		private string cleanString(string s) {
			while (s[2] == ' ' && char.IsLetterOrDigit(s[0]) && s.IndexOf('.') == 1)
				s = s.Substring(3);
			if (s[s.Length-1] == '.')
				s = s.Substring(0, s.Length-1);
			return s;
		}
		
		public override string ToString() {
			return item+" x"+count;
		}
		
		public int CompareTo(RequiredItem ro) {
			return sortIndex == ro.sortIndex ? CultureInfo.InvariantCulture.CompareInfo.Compare(defaultSorting, ro.defaultSorting, CompareOptions.IgnoreCase) : sortIndex.CompareTo(ro.sortIndex);
		}
		
	}
	
}
