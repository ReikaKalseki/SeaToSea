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
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{
	public class BuildingHandler {
		
		public static readonly BuildingHandler instance = new BuildingHandler();
		
		public bool isEnabled {get; private set;}
		
		private PlacedObject lastPlaced = null;
		private Dictionary<int, PlacedObject> items = new Dictionary<int, PlacedObject>();
		
		private List<string> text = new List<string>();
		private BasicText controlHint = new BasicText(TextAnchor.MiddleCenter);
		
		private BuildingHandler() {
			addText("LMB to select, Lalt+RMB to place selected on ground at look, LCtrl+RMB to duplicate them there");
			addText("Lalt+Arrow keys to move L/R Fwd/Back, +/- for U/D, add Z to make relative to obj");
			addText("Lalt+QR to yaw, [] to pitch (Z), ,. to roll (X)");
			addText("Add C for fast, X for slow; DEL to delete all selected");
		}
		
		private void addText(string s) {
			text.Add(s);
			controlHint.SetLocation(0, 300);
			controlHint.SetSize(16);
		}
		
		public void addCommand(string key, Action call) {
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>(key, call);
			addText("/"+key+": "+call.Method.Name);
		}
		
		public void addCommand<T>(string key, Action<T> call) {
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<T>>(key, call);
			addText("/"+key+": "+call.Method.Name);
		}
		
		public void addCommand<T1, T2>(string key, Action<T1, T2> call) {
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<T1, T2>>(key, call);
			addText("/"+key+": "+call.Method.Name);
		}
		
		public void setEnabled(bool on) {
			isEnabled = on;
			foreach (PlacedObject go in items.Values) {
				try {
					go.fx.SetActive(go.isSelected);
				}
				catch (Exception ex) {
					SBUtil.writeToChat("Could not set enabled state of "+go+" due to GO error: "+ex.ToString());
				}
			}
			if (on) {
				controlHint.ShowMessage(string.Join("\n", text.ToArray()));
			}
			else {
				controlHint.Hide();
			}
		}
		
		public void selectedInfo() {
			foreach (PlacedObject go in items.Values) {
				if (go.isSelected) {
					SBUtil.writeToChat(go.ToString());
				}
			}
		}
		
		internal static int genID(GameObject go) {
			if (go.transform.root != null && go.transform.root.gameObject != null)
				return go.transform.root.gameObject.GetInstanceID();
			else
				return go.GetInstanceID();
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
						List<PlacedObject> added = new List<PlacedObject>();
						foreach (PlacedObject p in new List<PlacedObject>(items.Values)) {
							if (p.isSelected) {
								PlacedObject b = PlacedObject.createPrefab(p.prefabName);
								items[b.referenceID] = b;
								b.obj.transform.SetPositionAndRotation(hit.point, hit.transform.rotation);
								lastPlaced = b;
								added.Add(b);
							}
						}
						clearSelection();
						foreach (PlacedObject b in added)
							select(b);
					}
					else if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftAlt)) {
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
		
		public void saveSelection(string file) {
			dumpSome(file, s => s.isSelected);
		}
		
		public void saveAll(string file) {
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
					string count = e.GetAttribute("count");
					int amt = string.IsNullOrEmpty(count) ? 1 : int.Parse(count);
					for (int i = 0; i < amt; i++) {
						CustomPrefab pfb = CustomPrefab.fromXML(e);
						if (pfb != null) {
							PlacedObject b = PlacedObject.fromXML(e, pfb);
							if (b != null) {
								SBUtil.writeToChat("Loaded a "+b);
								items[b.referenceID] = b;
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
					delete(go);
				}
			}
		}
		
		private void delete(PlacedObject go) {
			GameObject.Destroy(go.obj);
			GameObject.Destroy(go.fx);
			items.Remove(go.referenceID);
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
		
		public void manipulateSelected() {
    		float s = KeyCodeUtils.GetKeyHeld(KeyCode.C) ? 0.15F : (KeyCodeUtils.GetKeyHeld(KeyCode.X) ? 0.02F : 0.05F);
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
		    		go.rotateYaw(s*20);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Q))
		    		go.rotateYaw(-s*20);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftBracket))
		    		go.rotate(0, 0, -s*20);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.RightBracket))
		    		go.rotate(0, 0, s*20);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Comma))
		    		go.rotate(-s*20, 0, 0);
		    	if (KeyCodeUtils.GetKeyHeld(KeyCode.Period))
		    		go.rotate(s*20, 0, 0);
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
			PlacedObject b = PlacedObject.createPrefab(id);
			if (b != null) {
				items[b.referenceID] = b;
				b.obj.transform.SetPositionAndRotation(pos, Quaternion.identity);
				SBUtil.writeToChat("Spawned a "+b);
				lastPlaced = b;
				selectLastPlaced();
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
			//if (id.Length >= 24 && id[8] == '-' && id[13] == '-' && id[18] == '-' && id[23] == '-')
			//    return id;
			if (id.StartsWith("res_", StringComparison.InvariantCultureIgnoreCase)) {
				try {
					return ((VanillaResources)typeof(VanillaResources).GetField(id.Substring(4).ToUpper()).GetValue(null)).prefab;
				}
				catch (Exception ex) {
					SBUtil.log("Unable to fetch vanilla resource field '"+id+"': "+ex);
					return null;
				}
			}
			if (id.StartsWith("fauna_", StringComparison.InvariantCultureIgnoreCase)) {
				try {
					return ((VanillaCreatures)typeof(VanillaCreatures).GetField(id.Substring(6).ToUpper()).GetValue(null)).prefab;
				}
				catch (Exception ex) {
					SBUtil.log("Unable to fetch vanilla creature field '"+id+"': "+ex);
					return null;
				}
			}
			//if (id.IndexOf('/') >= 0)
			//    return PrefabData.getPrefabID(id);
			//return PrefabData.getPrefabIDByShortName(id);
			return id;
		}
	}
}
