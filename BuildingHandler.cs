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
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{
	public class BuildingHandler
	{
		public static readonly BuildingHandler instance = new BuildingHandler();
		
		public bool isEnabled {get; private set;}
		
		private PlacedObject lastPlaced = null;
		private Dictionary<int, PlacedObject> items = new Dictionary<int, PlacedObject>();
		
		private BuildingHandler()
		{
			
		}
		
		public void setEnabled(bool on) {
			isEnabled = on;
			foreach (PlacedObject go in items.Values) {
				go.fx.SetActive(go.isSelected);
			}
		}
		
		private static int genID(GameObject go) {
			if (go.transform.root != null && go.transform.root.gameObject != null)
				return go.transform.root.gameObject.GetInstanceID();
			else
				return go.GetInstanceID();
		}
		
		[Serializable]
		private class BuilderPlaced : MonoBehaviour {
			
			[SerializeField]
			internal PlacedObject placement;
			
			void Start() {
				SBUtil.log("Initialized builderplaced of "+placement);
			}
			
			void Update() {
				
			}
			
		}
		
		private class PlacedObject {
			
			private static GameObject bubblePrefab = null;
			
			[SerializeField]
			internal int referenceID;
			[SerializeField]
			internal string prefabName;
			[SerializeField]
			internal TechType tech;
			[SerializeField]
			internal GameObject obj;
			[SerializeField]
			internal GameObject fx;
			
			[SerializeField]
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
			
			internal void delete() {
				GameObject.Destroy(obj);
				GameObject.Destroy(fx);
				instance.items.Remove(referenceID);
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
		
		public void handleRClick(bool isCtrl = false) {
			Transform transform = MainCamera.camera.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			Ray ray = new Ray(position, forward);
			if (UWE.Utils.RaycastIntoSharedBuffer(ray, 30) > 0) {
				RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
				if (hit.transform != null) {
					foreach (PlacedObject go in items.Values) {
						if (go.isSelected)
							go.setPosition(hit.point);
					}
				}
			}
		}
		
		public void handleClick(bool isCtrl = false) {
			GameObject found = null;
			float dist;
			Targeting.GetTarget(Player.main.gameObject, 40, out found, out dist);
			Targeting.Reset();
			if (found == null) {
				SBUtil.writeToChat("Raytrace found nothing.");
			}
			if (found == null) {
				if (!isCtrl) {
					clearSelection();
				}
				return;
			}
			PlacedObject sel = getPlacement(found);
			if (sel == null) {
				return;
			}
			else if (sel.isSelected) {
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
			dumpSome(file, s => s.isSelected);
		}
		
		public void dumpAll(string file) {
			dumpSome(file, s => true);
		}
		
		private void dumpSome(string file, Func<PlacedObject, bool> flag) {
			string path = getDumpFile(file);
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			SBUtil.log("=================================");
			SBUtil.log("Building Handler has "+items.Count+" items: ");
			List<PlacedObject> li = new List<PlacedObject>(items.Values);
			foreach (PlacedObject go in li) {
				try {
					bool use = flag(go);
					SBUtil.log(go.ToString()+" dump = "+use);
					if (use)
						doc.DocumentElement.AppendChild(go.asXML(doc));
				}
				catch (Exception e) {
					throw new Exception(go.ToString(), e);
				}
			}
			SBUtil.log("=================================");
			doc.Save(path);
		}
		
		public void loadFile(string file) {
			XmlDocument doc = new XmlDocument();
			doc.Load(getDumpFile(file));
			XmlElement rootnode = doc.DocumentElement;
			foreach (XmlElement e in rootnode.ChildNodes) {
				string pfb = e.getProperty("prefab");
				if (pfb != null) {
					PlacedObject b = createPrefab(pfb);
					if (b != null) {
						string tech = e.getProperty("tech");
						if (b.tech == TechType.None && tech != "None") {
							b.tech = (TechType)Enum.Parse(typeof(TechType), tech);
						}
						Vector3 rot = e.getVector("rotation");
						b.obj.transform.SetPositionAndRotation(e.getVector("position"), Quaternion.Euler(rot.x, rot.y, rot.y));
						lastPlaced = b;
						selectLastPlaced();
					}
					else {
						SBUtil.writeToChat("Could not load XML block, prefab '"+pfb+"' did not build");
					}
				}
				else {
					SBUtil.writeToChat("Could not load XML block, no prefab: "+e.InnerText);
				}
			}
		}
		
		private string getDumpFile(string name) {
			string folder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ObjectDump");
			Directory.CreateDirectory(folder);
			return Path.Combine(folder, name+".xml");
		}
		
		public void deleteSelected() {
			List<PlacedObject> li = new List<PlacedObject>(items.Values);
			foreach (PlacedObject go in li) {
				if (go.isSelected) {
					go.delete();
				}
			}
		}
		/*
		private PlacedObject getSelectionFor(GameObject go) {
			PlacedObject s = getPlacement(go);
			if (s != null && selected.TryGetValue(s.referenceID, out s))
				return s;
			else
				return null;
		}*/
		
		public bool isSelected(GameObject go) {
			PlacedObject s = getPlacement(go);
			return s != null && s.isSelected;
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
			s.setSelected(true);
			SBUtil.writeToChat("Selected "+s);
		}
		
		public void deselect(GameObject go) {
			PlacedObject pre = getPlacement(go);
			if (pre != null) {
				deselect(pre);
			}
		}
		
		private void deselect(PlacedObject go) {
			go.setSelected(false);
		}
		
		public void clearSelection() {
			foreach (PlacedObject go in items.Values) {
				deselect(go);
			}
		}
		
		public void moveSelected(float s) {
			Vector3 vec = MainCamera.camera.transform.forward.normalized;
			Vector3 right = MainCamera.camera.transform.right.normalized;
			Vector3 up = MainCamera.camera.transform.up.normalized;
			if (KeyCodeUtils.GetKeyHeld(KeyCode.UpArrow))
	    		moveSelected(vec*s);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.DownArrow))
	    		moveSelected(vec*-s);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftArrow))
	    		moveSelected(right*-s);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.RightArrow))
	    		moveSelected(right*s);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Equals)) //+
	    		moveSelected(up*s);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Minus))
	    		moveSelected(up*-s);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.R))
	    		rotateSelectedYaw(1);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftBracket))
	    		rotateSelected(0, 0, -1);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.RightBracket))
	    		rotateSelected(0, 0, 1);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Comma))
	    		rotateSelected(-1, 0, 0);
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Period))
	    		rotateSelected(1, 0, 0);
		}
		
		public void moveSelected(Vector3 mov) {
			foreach (PlacedObject go in items.Values) {
				if (!go.isSelected)
					continue;
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
			foreach (PlacedObject go in items.Values) {
				if (!go.isSelected)
					continue;
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
			string id = getPrefabKeyFromID(arg);
			PlacedObject b = createPrefab(id);
			if (b != null) {
				b.obj.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, 0));
				lastPlaced = b;
				selectLastPlaced();
			}
	    }
		
		private PlacedObject createPrefab(string id) {
			GameObject prefab;
			if (id != null && UWE.PrefabDatabase.TryGetPrefab(id, out prefab)) {
				if (prefab != null) {
					GameObject go = GameObject.Instantiate(prefab);
					if (go != null) {
						go.SetActive(true);
						BuilderPlaced sel = go.AddComponent<BuilderPlaced>();
						PlacedObject ret = new PlacedObject(go, id);
						sel.placement = ret;
						SBUtil.writeToChat("Spawned a "+ret);
						//SBUtil.dumpObjectData(ret.obj);
						return ret;
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
			return null;
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
