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
		private readonly int SCRAP_COUNT = 45;//60;//UnityEngine.Random.Range(45, 71); //45-70
		private readonly string xmlPathRoot;
		
		private string avo;
		
		private readonly Vector3 eventCenter = new Vector3(215, 425.6F, 2623.6F);
		private readonly Vector3 eventUITargetLocation = new Vector3(297.2F, 3.5F, 1101);
		private readonly Vector3 mountainCenter = new Vector3(359.9F, 29F, 985.9F);
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
			//addItem(TechType.Beacon, 3);
			
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
			
			GenUtil.registerOreWorldgen(spawnerObject, false, BiomeType.Mountains_Grass, 1, 0.8F);
			//GenUtil.registerOreWorldgen(spawnerObject, false, BiomeType.Mountains_Rock, 1, 0.75F);
			GenUtil.registerOreWorldgen(spawnerObject, false, BiomeType.Mountains_Sand, 1, 0.55F);
			//LootDistributionHandler.EditLootDistributionData(spawnerObject, BiomeType.Mountains_ThermalVent, 0.2F, 1);
		
			IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
			IngameMenuHandler.Main.RegisterOnSaveEvent(save);
			
			avo = CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).ClassID;
		}
		
		private void loadSave() {
			string path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".xml");
			if (File.Exists(path)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				objects.Clear();
				objectCounts.Clear();
				objectCountsToGo.Clear();
				foreach (KeyValuePair<string, int> kvp in itemChoices)
					objectCountsToGo[kvp.Key] = kvp.Value;
				foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
					PositionedPrefab pfb = new PositionedPrefab("");
					pfb.loadFromXML(e);
					addObject(pfb);
				}
			}
			SNUtil.log("Loaded sunbeam debris cache: ");
			SNUtil.log(objects.toDebugString());
			SNUtil.log("Remaining:");
			SNUtil.log(objectCountsToGo.toDebugString());
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
			int max = pfb.prefabName == avo ? AVOLITE_COUNT : (itemChoices.ContainsKey(pfb.prefabName) ? itemChoices[pfb.prefabName] : SCRAP_COUNT);
			int rem = max-getCount(pfb.prefabName);
			if (rem > 0)
				objectCountsToGo[pfb.prefabName] = rem;
			else if (objectCountsToGo.ContainsKey(pfb.prefabName))
				objectCountsToGo.Remove(pfb.prefabName);
			//SNUtil.log(objectCountsToGo[pfb.prefabName]+" remaining of "+pfb.prefabName+" from "+max);
		}
		
		private PositionedPrefab allocateItem(Transform t) {
			string pfb = tryFindItem(t.position);
			if (pfb == null)
				return null;
			PositionedPrefab ret = new PositionedPrefab(pfb, t.position, t.rotation);
			ret.scale = t.localScale;
			GameObject go = ret.createWorldObject();
			if (pfb != "471852d4-03b6-4c47-9d4e-2ae893d63ff7" && pfb != "86589e2f-bd06-447f-b23a-1f35e6368010") //wiring kit, glass
				go.transform.rotation = UnityEngine.Random.rotationUniform;
			Rigidbody rb = go.GetComponentInChildren<Rigidbody>();
			if (rb)
				rb.isKinematic = true;
			go.transform.position = go.transform.position+Vector3.up*0.05F;
			addObject(ret);
			return ret;
		}
		
		private int getCount(string pfb) {
			return objectCounts.ContainsKey(pfb) ? objectCounts[pfb] : 0;
		}
		
		private string tryFindItem(Vector3 pos) {
			//SNUtil.log("Avo count = "+getCount(avo));
			if (getCount(avo) < AVOLITE_COUNT && getClosest(avo, pos) >= 160) {
				return avo;
			}
			if (objectCountsToGo.Count == 0 || UnityEngine.Random.Range(0, 5) == 0) {
				if (getCount("947f2823-c42a-45ef-94e4-52a9f1d3459c") < SCRAP_COUNT)
					return getRandomScrap().prefab;
				else
					return null;
			}
			else {
				string pfb = objectCountsToGo.Keys.ToList<string>()[UnityEngine.Random.Range(0, objectCountsToGo.Count)];
				int amt = objectCountsToGo[pfb];
				//SNUtil.log("Tried "+pfb+" > "+getCount(pfb)+"/"+objectCountsToGo[pfb]);
				if (amt > 1) {
					objectCountsToGo[pfb] = amt-1;
				}
				else {
					objectCountsToGo.Remove(pfb);
					//SNUtil.log("Removing "+pfb+" from dict: "+objectCountsToGo.toDebugString<string, int>());
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
			//SNUtil.log("Closest avo to "+pos+" was "+dist);
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
		
		bool isValidPosition(Vector3 pos) {
			if (pos.y >= -100)
				return false;
			string biome = WaterBiomeManager.main.GetBiome(pos, false);
			if (!string.Equals(biome, "mountains", StringComparison.InvariantCultureIgnoreCase))
				return false;
			return Vector3.Distance(pos.setY(0), biomeCenter.setY(0)) <= 600 && Vector3.Distance(pos.setY(0), mountainCenter.setY(0)) >= 360;
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
				go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
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
				if (!instance.isValidPosition(transform.position)) {
					SNUtil.log("Invalid sunbeam debris location, deleting @ "+transform.position);
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
				else if (PDAManager.getPage("sunbeamdebrishint").isUnlocked()) {
					PositionedPrefab pfb = instance.allocateItem(gameObject.transform);
					if (pfb != null) {
						SNUtil.log("Converted sunbeam debris placeholder @ "+transform.position+" to "+pfb.prefabName);
					}
					else {
						SNUtil.log("Item set exhausted, deleting @ "+transform.position);
					}
					enabled = false;
					UnityEngine.Object.DestroyImmediate(gameObject);
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
