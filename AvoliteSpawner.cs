using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class AvoliteSpawner {
		
		public static readonly AvoliteSpawner instance = new AvoliteSpawner();
		
		public readonly int AVOLITE_COUNT = 9;//6;
		private readonly int SCRAP_COUNT = 60;//UnityEngine.Random.Range(45, 71); //45-70
		private readonly string xmlPathRoot;
		
		private readonly Vector3 eventCenter = new Vector3(215, 425.6F, 2623.6F);
		private readonly Vector3 eventUITargetLocation = new Vector3(297.2F, 3.5F, 1101);
		private readonly Vector3 mountainCenter = new Vector3(356.3F, 29F, 1039.4F);
		private readonly Vector3 biomeCenter = new Vector3(800, 0, 1300);//new Vector3(966, 0, 1336);
		
		private readonly Dictionary<string, int> itemChoices = new Dictionary<string, int>();
		private readonly Spawnable spawnerObject;
		
		private readonly Dictionary<Vector3, PositionedPrefab> objects = new Dictionary<Vector3, PositionedPrefab>();
		private readonly Dictionary<string, int> objectCounts = new Dictionary<string, int>();
		private readonly Dictionary<string, int> objectCountsToGo = new Dictionary<string, int>();
		
		private AvoliteSpawner() {		
			xmlPathRoot = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "avolite_spawns");
			
			spawnerObject = new SunbeamDebrisObject();
		}
		
		private void addItem(TechType item, int amt) {
			string id = CraftData.GetClassIdForTechType(item);
			if (string.IsNullOrEmpty(id))
				throw new Exception("Could not find spawnable item for techtype "+item);
			addItem(id, amt);
		}
		
		private void addItem(string id, int amt) {
			itemChoices[id] = amt;
		}
		
		public void register() {
			//addItem(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, AVOLITE_COUNT);
			
			addItem(TechType.NutrientBlock, 10);
			addItem(TechType.DisinfectedWater, 8);
			addItem(TechType.Battery, 4);
			addItem(TechType.Beacon, 3);
			
			addItem(TechType.PowerCell, 1);
			addItem(TechType.FireExtinguisher, 2);
			
			addItem(TechType.EnameledGlass, 5);
			addItem(TechType.Titanium, 25);
			addItem(TechType.ComputerChip, 3);
			addItem(TechType.WiringKit, 6);
			addItem(TechType.CopperWire, 20);
			
			addItem(CraftingItems.getItem(CraftingItems.Items.LathingDrone).ClassID, 1);
			addItem(CraftingItems.getItem(CraftingItems.Items.Motor).ClassID, 2);
			
			//addItem(TechType.ScrapMetal, SCRAP_COUNT);
			
			spawnerObject.Patch();
			
			GenUtil.registerOreWorldgen(spawnerObject, false, BiomeType.Mountains_Grass, 1, 2F);
			GenUtil.registerOreWorldgen(spawnerObject, false, BiomeType.Mountains_Rock, 1, 0.75F);
			GenUtil.registerOreWorldgen(spawnerObject, false, BiomeType.Mountains_Sand, 1, 1F);
			//LootDistributionHandler.EditLootDistributionData(spawnerObject, BiomeType.Mountains_ThermalVent, 0.2F, 1);
		
			IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
			IngameMenuHandler.Main.RegisterOnSaveEvent(save);
		}
		
		private void loadSave() {
			string path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".xml");
			if (File.Exists(path)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				objects.Clear();
				objectCounts.Clear();
				objectCountsToGo.Clear();
				foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
					PositionedPrefab pfb = new PositionedPrefab("");
					pfb.loadFromXML(e);
					addObject(pfb);
				}
			}
		}
		
		private void save() {
			string path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".xml");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (PositionedPrefab go in objects.Values) {
				XmlElement e = doc.CreateElement(go.getTagName());
				go.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
			Directory.CreateDirectory(xmlPathRoot);
			doc.Save(path);
		}
		
		private void addObject(PositionedPrefab pfb) {
			objects[pfb.position] = pfb;
			objectCounts[pfb.prefabName] = getCount(pfb.prefabName)+1;
			objectCountsToGo[pfb.prefabName] = itemChoices[pfb.prefabName]-objectCounts[pfb.prefabName];
		}
		
		private PositionedPrefab allocateItem(Transform t) {
			string pfb = tryFindItem(t.position);
			if (pfb == null)
				return null;
			PositionedPrefab ret = new PositionedPrefab(pfb, t.position, t.rotation);
			ret.scale = t.localScale;
			ret.createWorldObject();
			addObject(ret);
			return ret;
		}
		
		private int getCount(string pfb) {
			return objectCounts.ContainsKey(pfb) ? objectCounts[pfb] : 0;
		}
		
		private string tryFindItem(Vector3 pos) {
			string avo = CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).ClassID;
			if (getCount(avo) < AVOLITE_COUNT && getClosest(avo, pos) >= 200) {
				return avo;
			}
			string scrap = "947f2823-c42a-45ef-94e4-52a9f1d3459c";
			if (objectCountsToGo.Count == 0) {
				if (getCount(scrap) < SCRAP_COUNT)
					return getRandomScrap().prefab;
				else
					return null;
			}
			else {
				string pfb = objectCountsToGo.Keys.ToList<string>()[UnityEngine.Random.Range(0, objectCountsToGo.Count-1)];
				int amt = objectCountsToGo[pfb];
				if (amt > 1) {
					objectCountsToGo[pfb] = amt-1;
				}
				else {
					objectCountsToGo.Remove(pfb);
				}
				return pfb;
			}
		}
		
		private double getClosest(string pfb, Vector3 pos) {
			double dist = 999999;
			foreach (PositionedPrefab pp in objects.Values) {
				if (pp.prefabName == pfb) {
					double d = Vector3.Distance(pp.position, pos);
					if (d < dist)
						dist = d;
				}
			}
			return dist;
		}
		
		private VanillaResources getRandomScrap() {
			switch(UnityEngine.Random.Range(0, 4)) {
				case 0:
				default:
					return VanillaResources.SCRAP1;
				case 1:
					return VanillaResources.SCRAP2;
				case 2:
					return VanillaResources.SCRAP3;
				case 3:
					return VanillaResources.SCRAP4;
			}
		}
		
		class SunbeamDebrisObject : Spawnable {
			
			public SunbeamDebrisObject() : base("SunbeamDebris", "", "") {
				
			}
			
			public override GameObject GetGameObject() {
				GameObject go = ObjectUtil.createWorldObject(VanillaResources.KYANITE.prefab, true, false);
				go.SetActive(false);
				ObjectUtil.removeComponent<Pickupable>(go);
				ObjectUtil.removeComponent<Collider>(go);
				ObjectUtil.removeComponent<ResourceTrackerUpdater>(go);
				ObjectUtil.removeComponent<ResourceTracker>(go);
				go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Batch;
				go.EnsureComponent<SunbeamDebris>();
				foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
					r.enabled = false;
				}
				return go;
			}
			
		}
		
		class SunbeamDebris : MonoBehaviour {
			
			void Update() {
				//SNUtil.writeToChat("SunbeamCheckPlayerRange > "+Story.StoryGoalManager.main.IsGoalComplete("SunbeamCheckPlayerRange"));
				//SNUtil.writeToChat("sunbeamdebrishint > "+Story.StoryGoalManager.main.IsGoalComplete("sunbeamdebrishint"));
				if (PDAManager.getPage("sunbeamdebrishint").isUnlocked()) {
					PositionedPrefab pfb = instance.allocateItem(gameObject.transform);
					if (pfb != null) {
						SNUtil.writeToChat("Converted sunbeam debris placeholder @ "+transform.position+" to "+pfb.prefabName);
					}
					UnityEngine.Object.DestroyImmediate(this);
				}
			}
			
		}
		
		public class TriggerCallback : MonoBehaviour {
			
			void trigger() {
				PDAManager.getPage("sunbeamdebrishint").unlock();
			}
			
		}
	}
	
}
