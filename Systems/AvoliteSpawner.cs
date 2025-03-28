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
using ReikaKalseki.Exscansion;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class AvoliteSpawner {
		
		public static readonly AvoliteSpawner instance = new AvoliteSpawner();
		
		public readonly int AVOLITE_COUNT = FCSIntegrationSystem.instance.isLoaded() ? 13 : 9;//6;
		private readonly int SCRAP_COUNT = 45;//60;//UnityEngine.Random.Range(45, 71); //45-70
		private readonly string oldSaveDir = Path.Combine(Path.GetDirectoryName(SeaToSeaMod.modDLL.Location), "avolite_spawns");
		private static readonly string saveFileName = "AvoSpawns.dat";
		
		private string avo;
		
		private readonly Vector3 eventCenter = new Vector3(215, 425.6F, 2623.6F);
		private readonly Vector3 eventUITargetLocation = new Vector3(297.2F, 3.5F, 1101);
		private readonly Vector3 biomeCenter = new Vector3(800, 0, 1300);//new Vector3(966, 0, 1336);
		
		private readonly Dictionary<string, int> itemChoices = new Dictionary<string, int>();
		private readonly Spawnable spawnerObject;
		
		private readonly Dictionary<Vector3, PositionedPrefab> objects = new Dictionary<Vector3, PositionedPrefab>();
		private readonly Dictionary<string, int> objectCounts = new Dictionary<string, int>();
		private readonly Dictionary<string, int> objectCountsToGo = new Dictionary<string, int>();
		
		private AvoliteSpawner() {				
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
			
			addItem(TechType.Cap1, 2);
			addItem(TechType.Cap2, 3);
			addItem("dfabc84e-c4c5-45d9-8b01-ca0eaeeb8e65", 3);
			addItem(TechType.ArcadeGorgetoy, 2);
			addItem(TechType.PurpleVegetable, 3);
			
			addItem(CraftingItems.getItem(CraftingItems.Items.LathingDrone).ClassID, 1);
			addItem(CraftingItems.getItem(CraftingItems.Items.Motor).ClassID, 2);
			
			//addItem(TechType.ScrapMetal, SCRAP_COUNT);
			
			spawnerObject.Patch();
			
			GenUtil.registerPrefabWorldgen(spawnerObject, false, BiomeType.Mountains_Grass, 1, 0.5F);
			GenUtil.registerPrefabWorldgen(spawnerObject, false, BiomeType.Mountains_Sand, 1, 0.3F);
		
			IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
			IngameMenuHandler.Main.RegisterOnSaveEvent(save);
			SNUtil.migrateSaveDataFolder(oldSaveDir, ".xml", saveFileName);
			
			avo = CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).ClassID;
		}
		
		public void postRegister() {
			ESHooks.scannabilityEvent += isItemMapRoomDetectable;
		}
		
		private void loadSave() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
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
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (PositionedPrefab go in objects.Values) {
				XmlElement e = doc.CreateElement(go.getTagName());
				go.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
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
		
		private GameObject allocateItem(Transform t) {
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
			return go;
		}
		
		private int getCount(string pfb) {
			return objectCounts.ContainsKey(pfb) ? objectCounts[pfb] : 0;
		}
		
		private string tryFindItem(Vector3 pos) {
			//SNUtil.log("Avo count = "+getCount(avo));
			if (UnityEngine.Random.Range(0, 2) == 0 && getCount(avo) < AVOLITE_COUNT && getClosest(avo, pos) >= 120 && WorldUtil.getObjectsNearWithComponent<AvoliteTag>(pos, 120).Count == 0) {
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
			if (pos.y >= -100 || pos.y <= -400)
				return false;
			string biome = WaterBiomeManager.main.GetBiome(pos, false);
			if (!VanillaBiomes.MOUNTAINS.isInBiome(pos))
				return false;
			return pos.x >= C2CHooks.gunCenter.x && Vector3.Distance(pos.setY(0), biomeCenter.setY(0)) <= 600 && Vector3.Distance(pos.setY(0), C2CHooks.gunCenter.setY(0)) >= 200 && Vector3.Distance(pos.setY(0), C2CHooks.mountainCenter.setY(0)) >= 360;
		}
		
		void isItemMapRoomDetectable(ESHooks.ResourceScanCheck rt) {
			if (rt.resource.techType == spawnerObject.TechType || rt.resource.overrideTechType == spawnerObject.TechType)
				rt.isDetectable = PDAManager.getPage("sunbeamdebrishint").isUnlocked();
		}
		
		internal void cleanPickedUp(Pickupable pp) {
			SunbeamDebris s = pp.GetComponentInChildren<SunbeamDebris>();
			if (s) {
				Story.StoryGoal.Execute("SunbeamDebris", Story.GoalType.Story);
				s.destroy();
			}
		}
		
		internal void tickMapRoom(MapRoomFunctionality map) {
	    	if (C2CHooks.skipScannerTick)
	    		return;
			if (VanillaBiomes.MOUNTAINS.isInBiome(map.transform.position)) {/*
				float r = map.GetScanRange();
				//HashSet<SunbeamDebris> arr = WorldUtil.getObjectsNearWithComponent<SunbeamDebris>(map.transform.position, r); cannot use because no collider
				IEnumerable<SunbeamDebris> arr = UnityEngine.Object.FindObjectsOfType<SunbeamDebris>();
				//SNUtil.writeToChat("Scanner room @ "+map.transform.position+" found "+arr.Count()+" debris in range "+r);
				foreach (SunbeamDebris s in arr) {
					//SNUtil.log("Trying to convert sunbeam debris at "+s.transform.position);
					s.tryConvert();
				}*/
				if (map.scanActive && ResourceTracker.resources.ContainsKey(spawnerObject.TechType) && map.typeToScan == spawnerObject.TechType && map.resourceNodes.Count > 0) {
					//SNUtil.writeToChat("Scanner room is scanning and has "+map.resourceNodes.Count+" hits");
					//Dictionary<string, ResourceTracker.ResourceInfo> info = ResourceTracker.resources[spawnerObject.TechType];
					/*
					HashSet<SunbeamDebris> set = WorldUtil.getObjectsNearWithComponent<SunbeamDebris>(map.resourceNodes[UnityEngine.Random.Range(0, map.resourceNodes.Count)].position, 4);
					if (set.Count > 0)
						set.First().tryConvert();*/
					WorldUtil.getGameObjectsNear(map.transform.position, map.GetScanRange(), go => {SunbeamDebris s = go.GetComponent<SunbeamDebris>(); if (s){s.tryConvert();}});
				}
			}
			else {
				//SNUtil.writeToChat("Scanner room @ "+map.transform.position+" is not in mountains, is in "+BiomeBase.getBiome(map.transform.position));
			}
		}
		
		class SunbeamDebrisObject : Spawnable {
			
			public SunbeamDebrisObject() : base("SunbeamDebris", "Sunbeam Debris", "Dropped salvageable material from the Sunbeam.") {
				
			}
			
			public override GameObject GetGameObject() {
				GameObject go = ObjectUtil.createWorldObject(VanillaResources.KYANITE.prefab, true, false);
				go.SetActive(false);
				ObjectUtil.removeComponent<Pickupable>(go);
				ObjectUtil.removeComponent<Collider>(go);
				ObjectUtil.removeChildObject(go, "collider");
				ObjectUtil.removeChildObject(go, "kyanite_small_03");
				//ObjectUtil.removeComponent<ResourceTrackerUpdater>(go);
				go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
				SphereCollider trigger = go.EnsureComponent<SphereCollider>(); //this is so can be found with a SphereCast
				trigger.isTrigger = true;
				trigger.radius = 0.1F;
				trigger.center = Vector3.zero;
				go.EnsureComponent<SunbeamDebris>();/*
				foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
					if (r) {
						r.enabled = false;
						UnityEngine.Object.Destroy(r.gameObject);
					}
				}*/
				ResourceTracker rt = go.EnsureComponent<ResourceTracker>();
				rt.techType = TechType;
				rt.overrideTechType = TechType;
				return go;
			}
		
			protected sealed override Atlas.Sprite GetItemSprite() {
				return TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/SunbeamDebrisIcon");
			}
			
		}
		
		class SunbeamDebris : MonoBehaviour {
			
			//private float lastConversionCheck = -1;
			
			void Update() {
				//SNUtil.writeToChat("SunbeamCheckPlayerRange > "+Story.StoryGoalManager.main.IsGoalComplete("SunbeamCheckPlayerRange"));
				//SNUtil.writeToChat("sunbeamdebrishint > "+Story.StoryGoalManager.main.IsGoalComplete("sunbeamdebrishint"));
				if (!instance.isValidPosition(transform.position)) {
					SNUtil.log("Invalid sunbeam debris location, deleting @ "+transform.position);
					destroy();
				}/*
				else if (DayNightCycle.main.timePassedAsFloat-lastConversionCheck >= 1) {
					lastConversionCheck = DayNightCycle.main.timePassedAsFloat;
					if (PDAManager.getPage("sunbeamdebrishint").isUnlocked() && !transform.parent.GetComponent<Pickupable>()) {
						GameObject pfb = instance.allocateItem(gameObject.transform);
						if (pfb != null) {
							SNUtil.log("Converted sunbeam debris placeholder @ "+transform.position+" to "+pfb);
							transform.parent = pfb.transform;
						}
						else {
							SNUtil.log("Item set exhausted, deleting @ "+transform.position);
						}
						enabled = false;
						//UnityEngine.Object.DestroyImmediate(gameObject); do not destroy immediately, do that when the bound item is collected/destroyed
					}
				}*/
			}
			
			internal void tryConvert() {
				if (PDAManager.getPage("sunbeamdebrishint").isUnlocked() && !transform.parent.GetComponent<Pickupable>()) {
					GameObject pfb = instance.allocateItem(gameObject.transform);
					if (pfb) {
						SNUtil.log("Converted sunbeam debris placeholder @ "+transform.position+" to "+pfb);
						transform.parent = pfb.transform;
					}
					else {
						SNUtil.log("Item set exhausted, deleting @ "+transform.position);
						destroy();
					}
					enabled = false;
					//UnityEngine.Object.DestroyImmediate(gameObject); do not destroy immediately, do that when the bound item is collected/destroyed
				}
			}
			
			internal void destroy() {
				GetComponent<ResourceTracker>().Unregister();
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			
		}
		
		public class TriggerCallback : MonoBehaviour {
			
			void trigger() {
				PDAManager.getPage("sunbeamdebrishint").unlock();
			}
			
		}
	}
	
}
