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
		//TODO add cheat commands, story goal times, death stats, etc
		
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
			
			xmlDoc = new XmlDocument();
			xmlDoc.AppendChild(xmlDoc.CreateElement("Root"));
			
			XmlElement e = xmlDoc.DocumentElement.addChild("Bases");
			foreach (SubRoot sub in bases) {
				XmlElement e2 = e.addChild("Base");
				Vector3 pos = sub.transform.position;
				BiomeBase bb = BiomeBase.getBiome(pos);
				e2.addProperty("centerX", pos.x);
				e2.addProperty("centerY", pos.y);
				e2.addProperty("centerZ", pos.z);
				e2.addProperty("biome", bb.displayName);
				e2.addProperty("cellSize", sub.GetComponentsInChildren<BaseCell>().Length);
				e2.addProperty("scannerCount", sub.GetComponentsInChildren<MapRoomFunctionality>().Length);
				e2.addProperty("moonpoolCount", sub.GetComponentsInChildren<VehicleDockingBay>().Length);
				e2.addProperty("acuCount", sub.GetComponentsInChildren<WaterPark>().Length);
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
				e2.addProperty("type", CraftData.GetTechType(v.gameObject).AsString());
				foreach (TechType tt in InventoryUtil.getVehicleUpgrades(v)) {
					e2.addProperty("module", tt.AsString());
				}
			}
			
			e = xmlDoc.DocumentElement.addChild("StoryGoals");
			foreach (string goal in StoryGoalManager.main.completedGoals) {
				XmlElement e2 = e.addChild("Goal");
				e2.addProperty("key", goal);
				e2.addProperty("unlockTime", 0);
			}
			
			e = xmlDoc.DocumentElement.addChild("TechUnlocks");
			TechUnlockTracker.forAllUnlocksNewerThan(-1, (tt, u) => {
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
			foreach (StorageContainer sc in from.GetComponentsInChildren<StorageContainer>()) {
				XmlElement e3 = root.addChild("storage");
				e3.addProperty("type", CraftData.GetTechType(sc.gameObject).AsString());
				XmlElement items = e3.addChild("items");
				InventoryUtil.forEach(sc.container, ii => items.addProperty(ii.item.GetTechType().AsString()));
			}
		}
		
		public void writeToFile(string file) {
			Directory.CreateDirectory(Path.GetDirectoryName(file));
			xmlDoc.Save(file);
		}
		
		public void submit() {
			
		}

	}
	
}
