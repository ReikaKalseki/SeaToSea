using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using UnityEngine.UI;

using FMOD;
using FMODUnity;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class GameStatistics {
		//TODO add cheat commands, death stats, etc
		
		private List<SubRoot> bases = new List<SubRoot>();
		private List<Vehicle> vehicles = new List<Vehicle>();
		private List<SubRoot> cyclops = new List<SubRoot>();
		
		private XmlDocument xmlDoc;
		
		private GameStatistics() {
	    	
		}
		
		public static GameStatistics collect() {
			GameStatistics gs = new GameStatistics();
			gs.populate();
			return gs;
		}
		
		private void populate() {
			cyclops = new List<SubRoot>();
			bases = new List<SubRoot>();
			//hasSeamoth = GameInfoIcon.Has(TechType.Seamoth);
			//hasCyclops = GameInfoIcon.Has(TechType.Cyclops);
			//hasPrawn = GameInfoIcon.Has(TechType.Exosuit);
			foreach (Vehicle s in UnityEngine.Object.FindObjectsOfType<Vehicle>()) {
				vehicles.Add(s);
			}
			foreach (SubRoot s in UnityEngine.Object.FindObjectsOfType<SubRoot>()) {
				if (s.isBase)
					bases.Add(s);
				else
				if (s.isCyclops)
					cyclops.Add(s);
			}
			
			bases.Sort((b1, b2) => b2.transform.position.y.CompareTo(b1.transform.position.y));
			
			xmlDoc = new XmlDocument();
			xmlDoc.AppendChild(xmlDoc.CreateElement("Root"));
			
			XmlElement e = xmlDoc.DocumentElement.addChild("Player");
			e.addProperty("health", Player.main.liveMixin.health);
			Survival sv = Player.main.GetComponent<Survival>();
			if (sv) {
				e.addProperty("food", sv.food);
				e.addProperty("water", sv.water);
			}
			collectStorage(e.addProperty("inventory"), Inventory.main.container);
			collectStorage(e.addProperty("equipment"), Inventory.main.equipment);
			
			e = xmlDoc.DocumentElement.addChild("Bases");
			foreach (SubRoot sub in bases) {
				XmlElement e2 = e.addChild("Base");
				Vector3 pos = sub.transform.position;
				e2.addProperty("centerX", pos.x);
				e2.addProperty("centerY", pos.y);
				e2.addProperty("centerZ", pos.z);
				e2.addProperty("biome", WorldUtil.getRegionalDescription(pos, false));
				e2.addProperty("cellSize", sub.GetComponentsInChildren<BaseCell>(true).Length);
				e2.addProperty("scannerCount", sub.GetComponentsInChildren<MapRoomFunctionality>(true).Length);
				e2.addProperty("moonpoolCount", sub.GetComponentsInChildren<VehicleDockingBay>(true).Length);
				e2.addProperty("acuCount", sub.GetComponentsInChildren<WaterPark>(true).Length);
				e2.addProperty("currentPower", sub.powerRelay.GetPower());
				e2.addProperty("maxPower", sub.powerRelay.GetMaxPower());
				collectStorage(e2, sub.gameObject);
			}
			
			e = xmlDoc.DocumentElement.addChild("Cyclopses");
			foreach (SubRoot sub in cyclops) {
				XmlElement e2 = e.addChild("Cyclops");
				foreach (TechType tt in InventoryUtil.getCyclopsUpgrades(sub)) {
					e2.addProperty("module", tt.AsString());
				}
				collectStorage(e2, sub.gameObject);
			}
			
			e = xmlDoc.DocumentElement.addChild("Vehicles");
			foreach (Vehicle v in vehicles) {
				XmlElement e2 = e.addChild("Vehicle");
				e2.addProperty("type", getObjectType(v));
				foreach (TechType tt in InventoryUtil.getVehicleUpgrades(v)) {
					e2.addProperty("module", tt.AsString());
				}
			}
			
			e = xmlDoc.DocumentElement.addChild("StoryGoals");
			/*
			foreach (string goal in StoryGoalManager.main.completedGoals) {
				XmlElement e2 = e.addChild("Goal");
				e2.addProperty("key", goal);
				e2.addProperty("unlockTime", 0);
			}*/
			StoryHandler.instance.forAllGoalsNewerThan(9999999, (k, g) => {
				XmlElement e2 = e.addChild("Unlock");    
				e2.addProperty("tech", g.goal);
				e2.addProperty("unlockTime", g.unlockTime);
			});
			
			e = xmlDoc.DocumentElement.addChild("TechUnlocks");
			TechUnlockTracker.forAllUnlocksNewerThan(9999999, (tt, u) => {
				XmlElement e2 = e.addChild("Unlock");    
				e2.addProperty("tech", tt.AsString());
				e2.addProperty("unlockTime", u.unlockTime);
			});
			
			e = xmlDoc.DocumentElement.addChild("Cheats");
			{
				XmlElement e2 = e.addChild("SpawnedItems");
				SpawnedItemTracker.forAllSpawns(s => {
					XmlElement e3 = e2.addChild("SpawnedItem");    
					e3.addProperty("item", s.itemType.AsString());
					e3.addProperty("spawnTime", s.spawnTime);
				});
			}
		}
		
		private void collectStorage(XmlElement root, GameObject from) {
			XmlElement e = root.addChild("inventories");
			foreach (StorageContainer sc in from.GetComponentsInChildren<StorageContainer>(true)) {
				XmlElement e3 = e.addChild("storage");
				e3.addProperty("type", getObjectType(sc));
				collectStorage(e3, sc.container);
			}
		}
		
		private void collectStorage(XmlElement e3, Equipment sc) {
			XmlElement items = e3.addChild("items");
			foreach (KeyValuePair<string, InventoryItem> kvp in sc.equipment) {
				if (kvp.Value != null) {
					TechType tt = kvp.Value.item.GetTechType();
					XmlElement added = items.addProperty(kvp.Key, tt.AsString());
					added.SetAttribute("displayName", Language.main.Get(tt));
				}
			}
		}
		
		private void collectStorage(XmlElement e3, ItemsContainer sc) {
			Dictionary<TechType, int> counts = new Dictionary<TechType, int>();
			InventoryUtil.forEach(sc, ii => {
				TechType tt = ii.item.GetTechType();
				int has = counts.ContainsKey(tt) ? counts[tt] : 0;
				counts[tt] = has + 1;
			});
			if (counts.Count == 0)
				return;
			XmlElement items = e3.addChild("items");
			foreach (KeyValuePair<TechType, int> kvp in counts) {
				XmlElement added = items.addProperty(kvp.Key.AsString(), kvp.Value);
				added.SetAttribute("displayName", Language.main.Get(kvp.Key));
			}
		}
		
		private string getObjectType(Component c) {
			TechType tt = CraftData.GetTechType(c.gameObject);
			return tt == TechType.None ? c.gameObject.name : tt.AsString();
		}
		
		public void writeToFile(string file) {
			Directory.CreateDirectory(Path.GetDirectoryName(file));
			xmlDoc.Save(file);
		}
		
		public void submit() {
			string file = Path.Combine(SNUtil.getCurrentSaveDir(), "finalStatistics.xml");
			writeToFile(file);
		}

	}
	
}
