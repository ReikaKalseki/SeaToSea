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
	internal class WreckDoorSwaps : ManipulationBase {
		
		private List<DoorSwap> swaps = new List<DoorSwap>();
		
		internal override void applyToObject(GameObject go) {
			foreach (DoorSwap d in swaps) {
				bool found = false;
				foreach (Transform t in ObjectUtil.getChildObject(go, "Doors").transform) {
					if (!t || !t.gameObject)
						continue;
					Vector3 pos = t.position;
					SNUtil.log("Checking door "+t.position);
					if (Vector3.Distance(d.position, pos) <= 0.5) {
						found = true;
						d.applyTo(t.gameObject);
						SNUtil.log("Matched to door "+pos+", converted to "+d.doorType);
					}
				}
				if (!found)
					SNUtil.writeToChat("Door swap @ "+d.position+" found no match!!");
			}
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			swaps.Clear();
			foreach (XmlElement e2 in e.getDirectElementsByTagName("door")) {
				DoorSwap d = new DoorSwap(e2.getVector("position").Value, e2.getProperty("type"));
				swaps.Add(d);
			}
		}
		
		internal override void saveToXML(XmlElement e) {
			foreach (DoorSwap d in swaps) {
				XmlElement e2 = e.OwnerDocument.CreateElement("door");
				e2.addProperty("position", d.position);
				e2.addProperty("type", d.doorType);
				e.AppendChild(e2);
			}
		}
		
		class DoorSwap {
			
			internal readonly Vector3 position;
			internal readonly string doorType;
			
			internal static readonly Dictionary<string, string> doorPrefabs = new Dictionary<string, string>{
				{"Blocked", "d79ab37f-23b6-42b9-958c-9a1f4fc64cfd"},
				{"Handle", "d9524ffa-11cf-4265-9f61-da6f0fe84a3f"},
				{"Laser", "6f01d2df-03b8-411f-808f-b3f0f37b0d5c"},
				{"Repair", "b86d345e-0517-4f6e-bea4-2c5b40f623b4"},
				{"Openable", "b86d345e-0517-4f6e-bea4-2c5b40f623b4"},
			};
			
			internal DoorSwap(Vector3 pos, string t) {
				position = pos;
				doorType = t;
			}
			
			internal void applyTo(GameObject go) {
				Transform par = go.transform.parent;
				GameObject put = ObjectUtil.createWorldObject(doorPrefabs[doorType], true, true);
				if (put == null) {
					SNUtil.writeToChat("Could not find prefab for door type "+doorType);
					return;
				}
				put.transform.position = go.transform.position;
				put.transform.rotation = go.transform.rotation;
				put.transform.parent = par;
				UnityEngine.Object.DestroyImmediate(go);
				StarshipDoor d = put.GetComponent<StarshipDoor>();
				if (d) {
					if (doorType == "Openable") {
						d.UnlockDoor();
					}
					if (doorType == "Repair") {
						d.LockDoor();
						GameObject panel = ObjectUtil.createWorldObject("bb16d2bf-bc85-4bfa-a90e-ddc7343b0ac2", true, true);
						panel.transform.position = put.transform.position;
						panel.transform.rotation = put.transform.rotation;
						WeldableWallPanelGeneric weld = panel.EnsureComponent<WeldableWallPanelGeneric>();
						//FIXME finish
					}
				}
			}
			
			
		}
		
	}
}
