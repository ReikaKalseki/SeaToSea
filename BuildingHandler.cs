/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class BuildingHandler
	{
		public static readonly BuildingHandler instance = new BuildingHandler();
		
		public bool isEnabled {get; private set;}
		
		private PlacedObject lastPlaced = null;
		private Dictionary<int, PlacedObject> selected = new Dictionary<int, PlacedObject>();
		
		private BuildingHandler()
		{
			
		}
		
		public void setEnabled(bool on) {
			isEnabled = on;
			foreach (PlacedObject go in selected.Values) {
				go.fx.SetActive(on);
			}
		}
		
		private static int genID(GameObject go) {
			if (go.transform.root != null && go.transform.root.gameObject != null)
				return go.transform.root.gameObject.GetInstanceID();
			else
				return go.GetInstanceID();
		}
		
		private class BuilderPlaced : MonoBehaviour {
			
			internal PlacedObject placement;
			
			void Start() {
				SBUtil.log("Initialized builderplaced of "+placement);
			}
			
			void Update() {
				
			}
			
		}
		
		private class PlacedObject {
			
			private static GameObject bubblePrefab = null;
			
			internal readonly int referenceID;
			internal readonly string prefabName;
			internal readonly TechType tech;
			internal readonly GameObject obj;
			internal readonly GameObject fx;
			
			internal bool isSelected;
			
			internal PlacedObject(GameObject go, string pfb) {
				if (go == null)
					throw new Exception("Tried to make a place of a null obj!");
				if (go.transform == null)
					SBUtil.log("Place of obj "+go+" has null transform?!");
				referenceID = genID(go);
				prefabName = pfb;
				obj = go;
				tech = CraftData.GetTechType(go);
				
				try {
					if (bubblePrefab == null) {
						if (!UWE.PrefabDatabase.TryGetPrefab("fca5cdd9-1d00-4430-8836-a747627cdb2f", out bubblePrefab)) {
							SBUtil.writeToChat("Bubbles not loadable!");
						}
					}
					if (bubblePrefab != null) {
						fx = Utils.SpawnFromPrefab(bubblePrefab, obj.transform);
						if (fx != null) {
							if (fx.transform != null)
								fx.transform.position = obj.transform.position;
							fx.SetActive(false);
						}
						else {
							SBUtil.writeToChat("Bubbles not constructable!");
						}
					}
					else {
						SBUtil.writeToChat("Bubbles not found.");
					}
				}
				catch (Exception e) {
					throw new Exception("Error in bubbles", e);
				}
			}
			
			internal void setSelected(bool sel) {
				isSelected = sel;
				fx.SetActive(isSelected);
			}
			
			internal void setPosition(Vector3 pos) {
				obj.transform.position = pos;
				fx.transform.position = pos;
			}
			
			public override string ToString() {
				return prefabName+" ["+tech+"] @ "+obj.transform.position+" / "+obj.transform.rotation.eulerAngles+" ("+referenceID+")";
			}
			
			internal XmlNode asXML(XmlDocument doc) {
				XmlNode n = doc.CreateElement("object");
				n.addProperty("prefab", prefabName);
				if (tech != TechType.None)
					n.addProperty("tech", Enum.GetName(typeof(TechType), tech));
				n.addProperty("position", obj.transform.position);
				n.addProperty("rotation", obj.transform.rotation.eulerAngles);
				return n;
			}
			
		}
		
		public void handleClick(bool isCtrl = false) {
			GameObject found = null;
			float dist;
			Targeting.GetTarget(Player.main.gameObject, 40, out found, out dist);
			if (found == null) {
				SBUtil.writeToChat("Raytrace found nothing.");
			}
			PlacedObject sel = getSelectionFor(found);
			if (found == null) {
				if (!isCtrl) {
					clearSelection();
				}
			}
			else if (sel != null) {
				if (isCtrl)
					deselect(sel);
			}
			else {
				if (!isCtrl)
					clearSelection();
				select(found);
			}
		}
		
		public void dumpSelection(string file) {
			string folder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ObjectDump");
			string path = Path.Combine(folder, file+".xml");
			Directory.CreateDirectory(folder);
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			SBUtil.log("=================================");
			SBUtil.log("Building Handler has "+selected.Count+" items: ");
			List<PlacedObject> li = new List<PlacedObject>(selected.Values);
			foreach (PlacedObject go in li) {
				try {
					SBUtil.log(go.ToString());
					doc.DocumentElement.AppendChild(go.asXML(doc));
				}
				catch (Exception e) {
					throw new Exception(go.ToString(), e);
				}
			}
			SBUtil.log("=================================");
			doc.Save(path);
		}
		
		public void deleteSelected() {
			List<PlacedObject> li = new List<PlacedObject>(selected.Values);
			foreach (PlacedObject go in li) {
				deselect(go);
				GameObject.Destroy(go.obj);
				GameObject.Destroy(go.fx);
			}
		}
		
		private PlacedObject getSelectionFor(GameObject go) {
			PlacedObject s = getPlacement(go);
			if (s != null && selected.TryGetValue(s.referenceID, out s))
				return s;
			else
				return null;
		}
		
		public bool isSelected(GameObject go) {
			return getSelectionFor(go) != null;
		}
		
		public void selectLastPlaced() {
			if (lastPlaced != null) {
				select(lastPlaced);
			}
		}
		
		private PlacedObject getPlacement(GameObject go) {
			if (go == null)
				return null;
			BuilderPlaced pre = go.GetComponentInParent<BuilderPlaced>();
			if (pre != null) {
				return pre.placement;
			}
			else {
				SBUtil.writeToChat("Game object "+go+" ("+genID(go)+") was not was not placed by the builder system.");
				return null;
			}
		}
		
		public void select(GameObject go) {
			PlacedObject pre = getPlacement(go);
			if (go != null && pre == null)
				SBUtil.dumpObjectData(go);
			if (pre != null) {
				select(pre);
			}
		}
		
		private void select(PlacedObject s) {
			PlacedObject trash;
			if (!selected.TryGetValue(s.referenceID, out trash)) {
				selected[s.referenceID] = s;
				s.setSelected(true);
				SBUtil.writeToChat("Selected "+s);
			}
		}
		
		public void deselect(GameObject go) {
			PlacedObject pre = getPlacement(go);
			if (pre != null) {
				deselect(pre);
			}
		}
		
		private void deselect(PlacedObject go) {
			selected.Remove(go.referenceID);
			go.setSelected(false);
		}
		
		public void clearSelection() {
			List<PlacedObject> li = new List<PlacedObject>(selected.Values);
			foreach (PlacedObject go in li) {
				deselect(go);
			}
		}
		
		public void moveSelected(Vector3 mov) {
			foreach (PlacedObject go in selected.Values) {
				Vector3 vec = go.obj.transform.position;
				vec.x += mov.x;
				vec.y += mov.y;
				vec.z += mov.z;
				go.setPosition(vec);
				//SBUtil.writeToChat(go.obj.transform.position.ToString());
			}
		}
		
		public void rotateSelectedYaw(double ang) {
			rotateSelected(0, ang, 0);
		}
		
		public void rotateSelected(double roll, double yaw, double pitch) {
			foreach (PlacedObject go in selected.Values) {
				Vector3 euler = go.obj.transform.rotation.eulerAngles;
				go.obj.transform.rotation = Quaternion.Euler(euler.x+(float)roll, euler.y+(float)yaw, euler.z+(float)pitch);
				//SBUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
			}
		}
    
	    public void spawnPrefabAtLook(string arg) {
			if (!isEnabled)
				return;
	    	Transform transform = MainCamera.camera.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			Vector3 pos = position+(forward.normalized*7.5F);
			GameObject prefab;
			string id = getPrefabKeyFromID(arg);
			if (id != null && UWE.PrefabDatabase.TryGetPrefab(id, out prefab)) {
				if (prefab != null) {
					GameObject go = GameObject.Instantiate(prefab);
					if (go != null) {
						go.SetActive(true);
						go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, 0));
						BuilderPlaced sel = go.AddComponent<BuilderPlaced>();
						lastPlaced = new PlacedObject(go, id);
						sel.placement = lastPlaced;
						SBUtil.writeToChat("Spawned a "+lastPlaced);
						SBUtil.dumpObjectData(lastPlaced.obj);
						selectLastPlaced();
					}
					else {
						SBUtil.writeToChat("Prefab found and placed succeeeded but resulted in null?!");
					}
				}
				else {
					SBUtil.writeToChat("Prefab found but was null?!");
				}
			}
			else {
				SBUtil.writeToChat("Prefab not placed; nothing found for '"+(id == null ? "null" : id)+"'.");
			}
	    }
		/*
		public void spawnTechTypeAtLook(string tech) {
			spawnTechTypeAtLook(getTech(tech));
		}
		
		public void spawnTechTypeAtLook(TechType tech) {
			
		}
		
		private TechType getTech(string name) {
			
		}*/
		
		private string getPrefabKeyFromID(string id) {
			if (id.Length >= 24 && id[8] == '-' && id[13] == '-' && id[18] == '-' && id[23] == '-')
			    return id;
			if (id.StartsWith("res_", StringComparison.InvariantCultureIgnoreCase)) {
				try {
					return ((VanillaResources)typeof(VanillaResources).GetField(id.Substring(4).ToUpper()).GetValue(null)).prefab;
				}
				catch (Exception e) {
					return null;
				}
			}
			if (id.IndexOf('/') >= 0)
			    return PrefabData.getPrefabID(id);
			return PrefabData.getPrefabIDByShortName(id);
		}
	}
}
