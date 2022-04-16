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
using UnityEngine.Serialization;
using UnityEngine.Scripting;
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
		//[ProtoContract]
		//[ProtoInclude(30000, typeof(BuilderPlaced))]
		private class BuilderPlaced : MonoBehaviour {
			
			[SerializeField]
			//[SerializeReference]
			internal PlacedObject placement;
			
			void Start() {
				SBUtil.log("Initialized builderplaced of "+placement);
			}
			
			void Update() {
				
			}
			
		}
		
		[Serializable]
		private class PlacedObject : PositionedPrefab {
			
			private static GameObject bubblePrefab = null;
			
			[SerializeField]
			internal int referenceID;
			[SerializeField]
			internal TechType tech;
			[SerializeField]
			internal GameObject obj;
			[SerializeField]
			internal GameObject fx;
			
			[SerializeField]
			internal bool isSelected;
			
			internal PlacedObject(GameObject go, string pfb) : base(pfb) {
				if (go == null)
					throw new Exception("Tried to make a place of a null obj!");
				if (go.transform == null)
					SBUtil.log("Place of obj "+go+" has null transform?!");
				referenceID = genID(go);
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
		
			internal void move(Vector3 mov) {
				Vector3 vec = obj.transform.position;
				vec.x += mov.x;
				vec.y += mov.y;
				vec.z += mov.z;
				setPosition(vec);
				//SBUtil.writeToChat(go.obj.transform.position.ToString());
			}
			
			internal void rotateYaw(double ang) {
				rotate(0, ang, 0);
			}
			
			internal void rotate(double roll, double yaw, double pitch) {
				Vector3 euler = obj.transform.rotation.eulerAngles;
				obj.transform.rotation = Quaternion.Euler(euler.x+(float)roll, euler.y+(float)yaw, euler.z+(float)pitch);
				//SBUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
			}
			
			internal void delete() {
				GameObject.Destroy(obj);
				GameObject.Destroy(fx);
				instance.items.Remove(referenceID);
			}
			
			public override string ToString() {
				return prefabName+" ["+tech+"] @ "+obj.transform.position+" / "+obj.transform.rotation.eulerAngles+" ("+referenceID+")"+" "+(isSelected ? "*" : "");
			}
			
			internal override XmlElement asXML(XmlDocument doc) {
				XmlElement n = base.asXML(doc);
				if (tech != TechType.None)
					n.addProperty("tech", Enum.GetName(typeof(TechType), tech));
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
					if (isCtrl) {
						if (lastPlaced != null) {
							PlacedObject b = createPrefab(lastPlaced.prefabName);
							if (b != null) {
								b.obj.transform.SetPositionAndRotation(hit.point, hit.transform.rotation);
								lastPlaced = b;
								selectLastPlaced();
							}
						}
					}
					else {
						foreach (PlacedObject go in items.Values) {
							if (go.isSelected) {
								go.setPosition(hit.point);
							}
						}
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
			PlacedObject has = getPlacement(found);
			//SBUtil.writeToChat("Has is "+has);
			if (has == null) {
				if (!isCtrl) {
					clearSelection();
				}
			}
			else if (isCtrl) {
				if (has.isSelected)
					deselect(has);
				else
					select(has);
			}
			else {
				clearSelection();
				select(has);
			}
		}
		
		public void selectAll() {
			foreach (PlacedObject go in items.Values) {
				select(go);
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
			foreach (PlacedObject go in items.Values) {
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
				try {
					PositionedPrefab pfb = PositionedPrefab.fromXML(e);
					if (pfb != null) {
						PlacedObject b = createPrefab(pfb.prefabName);
						if (b != null) {
							string tech = e.getProperty("tech", true);
							if (b.tech == TechType.None && tech != null && tech != "None") {
								b.tech = (TechType)Enum.Parse(typeof(TechType), tech);
							}
							b.obj.transform.SetPositionAndRotation(pfb.position, pfb.rotation);
							lastPlaced = b;
							selectLastPlaced();
						}
						else {
							SBUtil.writeToChat("Could not load XML block, prefab '"+pfb+"' did not build");
						}
					}
					else {
						SBUtil.writeToChat("Could not load XML builder block, no prefab: "+e.InnerText);
					}
				}
				catch (Exception ex) {
					SBUtil.writeToChat("Could not load XML block, threw exception: "+e.InnerText+" -> "+ex.ToString());
					SBUtil.log(ex.ToString());
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
			//SBUtil.writeToChat("Selected "+s);
			s.setSelected(true);
		}
		
		public void deselect(GameObject go) {
			PlacedObject pre = getPlacement(go);
			if (pre != null) {
				deselect(pre);
			}
		}
		
		private void deselect(PlacedObject go) {
			//SBUtil.writeToChat("Deselected "+go);
			go.setSelected(false);
		}
		
		public void clearSelection() {
			foreach (PlacedObject go in items.Values) {
				deselect(go);
			}
		}
		
		public void manipulateSelected(float s) {
			foreach (PlacedObject go in items.Values) {
				if (!go.isSelected)
					continue;
				Transform t = MainCamera.camera.transform;
				if (KeyCodeUtils.GetKeyHeld(KeyCode.Z)) {
					t = go.obj.transform;
				}
				Vector3 vec = t.forward.normalized;
				Vector3 right = t.right.normalized;
				Vector3 up = t.up.normalized;
				if (KeyCodeUtils.GetKeyHeld(KeyCode.UpArrow))
		    		go.move(vec*s);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.DownArrow))
		    		go.move(vec*-s);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftArrow))
		    		go.move(right*-s);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.RightArrow))
		    		go.move(right*s);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Equals)) //+
		    		go.move(up*s);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Minus))
		    		go.move(up*-s);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.R))
		    		go.rotateYaw(1);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftBracket))
		    		go.rotate(0, 0, -1);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.RightBracket))
		    		go.rotate(0, 0, 1);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Comma))
		    		go.rotate(-1, 0, 0);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Period))
		    		go.rotate(1, 0, 0);
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
						items[ret.referenceID] = ret;
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
			if (id.StartsWith("fauna_", StringComparison.InvariantCultureIgnoreCase)) {
				try {
					return ((VanillaCreatures)typeof(VanillaCreatures).GetField(id.Substring(6).ToUpper()).GetValue(null)).prefab;
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
