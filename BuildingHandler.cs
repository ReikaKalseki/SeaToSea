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
		
		private Selection lastPlaced = null;
		private Dictionary<int, Selection> selected = new Dictionary<int, Selection>();
		private Dictionary<int, Selection> placedPrefabs = new Dictionary<int, Selection>();
		
		private BuildingHandler()
		{
			
		}
		
		public void setEnabled(bool on) {
			isEnabled = on;
			foreach (Selection go in selected.Values) {
				go.fx.SetActive(on);
			}
		}
		
		private static int genID(GameObject go) {
			if (go.transform.root != null && go.transform.root.gameObject != null)
				return go.transform.root.gameObject.GetInstanceID();
			else
				return go.GetInstanceID();
		}
		
		private class SelectionComponent : Component {
			
			internal Selection select;
			
		}
		
		private class Selection {
			
			private static GameObject bubblePrefab = null;
			
			internal readonly int referenceID;
			internal readonly string prefabName;
			internal readonly TechType tech;
			internal readonly GameObject obj;
			internal readonly GameObject fx;
			
			internal Selection(GameObject go, string pfb) {
				referenceID = genID(go);
				prefabName = pfb;
				obj = go;
				tech = CraftData.GetTechType(go);
				
				GameObject bubb = null;
				if (bubblePrefab == null) {
					if (!UWE.PrefabDatabase.TryGetPrefab("fca5cdd9-1d00-4430-8836-a747627cdb2f", out bubblePrefab)) {
						SBUtil.writeToChat("Bubbles not found.");
					}
				}
				else {
					Vector3 pos = go.transform.position;
					bubb = Utils.SpawnFromPrefab(bubblePrefab, null);
					bubb.transform.position = pos;
				}
				fx = bubb;
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
			GameObject any = null;
			GameObject found = null;
			float dist;
			Targeting.GetTarget(Player.main.gameObject, 40, out found, out dist);
			if (found != null) {
				//SBUtil.writeToChat("Selected "+found+" @ "+found.transform.position);
				TechType tech;
				any = getMasterObject(found, out tech);
			}
			else {
				SBUtil.writeToChat("Raytrace found nothing.");
			}
			Selection sel = getSelectionFor(any);
			if (any == null) {
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
				select(any);
			}
		}
		
		private GameObject getMasterObject(GameObject found, out TechType tech) {
			GameObject use;
			if (Targeting.GetRoot(found, out tech, out use)) {
				if (use != null) {
					SBUtil.writeToChat("Raytrace found inner "+found+" @ "+found.transform.position);
					return use;
				}
			}
			SBUtil.writeToChat("Raytrace found "+found+" @ "+found.transform.position);
			return found;
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
			List<Selection> li = new List<Selection>(selected.Values);
			foreach (Selection go in li) {
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
			List<Selection> li = new List<Selection>(selected.Values);
			foreach (Selection go in li) {
				deselect(go);
				delete(go.obj);
			}
		}
		
		public void delete(GameObject go) {
			GameObject.Destroy(go);
		}
		
		private Selection getSelectionFor(GameObject go) {
			if (go == null)
				return null;
			Selection s = null;
			selected.TryGetValue(genID(go), out s);
			return s;
		}
		
		public bool isSelected(GameObject go) {
			return getSelectionFor(go) != null;
		}
		
		public void selectLastPlaced() {
			if (lastPlaced != null) {
				select(lastPlaced);
			}
		}
		
		public void select(GameObject go) {
			Selection pre = null;
			int id = genID(go);
			if (placedPrefabs.TryGetValue(id, out pre)) {
				select(pre);
			}
			else {
				SBUtil.writeToChat("Game object "+go+" ("+id+") was not mapped to a prefab.");
			}
		}
		
		private void select(Selection s) {
			if (!selected.TryGetValue(s.referenceID, out s)) {
				selected[s.referenceID] = s;
				SBUtil.writeToChat("Selected "+s);
			}
		}
		
		public void deselect(GameObject go) {
			Selection s = null;
			if (selected.TryGetValue(genID(go), out s)) {
				deselect(s);
			}
		}
		
		private void deselect(Selection go) {
			selected.Remove(go.referenceID);
			delete(go.fx);
		}
		
		public void clearSelection() {
			List<Selection> li = new List<Selection>(selected.Values);
			foreach (Selection go in li) {
				deselect(go);
			}
		}
		
		public void moveSelected(Vector3 mov) {
			foreach (Selection go in selected.Values) {
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
			foreach (Selection go in selected.Values) {
				Vector3 euler = go.obj.transform.rotation.eulerAngles;
				go.obj.transform.rotation = Quaternion.Euler(euler.x+(float)roll, euler.y+(float)yaw, euler.z+(float)pitch);
				//SBUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
			}
		}
    
	    public void spawnPrefabAtLook(string id) {
			if (!isEnabled)
				return;
	    	Transform transform = MainCamera.camera.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			Vector3 pos = position+(forward.normalized*7.5F);
			GameObject prefab;
			id = getPrefabKeyFromID(id);
			if (UWE.PrefabDatabase.TryGetPrefab(id, out prefab)) {
				GameObject go = GameObject.Instantiate(prefab);
				go.SetActive(true);
				go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, 0));
				SelectionComponent sel = go.AddComponent<SelectionComponent>();
				lastPlaced = new Selection(go, id);**
				sel.select = lastPlaced;
				cachePlace(lastPlaced);
				SBUtil.writeToChat("Spawned a "+lastPlaced);
				selectLastPlaced();
			}
			else {
				SBUtil.writeToChat("Prefab not placed; nothing found for '"+id+"'.");
			}
	    }
		
		private void cachePlace(Selection s) {
			placedPrefabs[s.referenceID] = s;
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
