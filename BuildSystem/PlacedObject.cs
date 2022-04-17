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
		[Serializable]
		internal class PlacedObject : CustomPrefab {
			
			private static GameObject bubblePrefab = null;
			
			[SerializeField]
			internal int referenceID;
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
				referenceID = BuildingHandler.genID(go);
				obj = go;
				tech = CraftData.GetTechType(go);
				
				try {
					if (bubblePrefab == null) {
						if (!UWE.PrefabDatabase.TryGetPrefab("fca5cdd9-1d00-4430-8836-a747627cdb2f", out bubblePrefab)) {
						//if (!SBUtil.getPrefab("fca5cdd9-1d00-4430-8836-a747627cdb2f", out bubblePrefab)) {
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
				position = pos;
				obj.transform.position = position;
				fx.transform.position = position;
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
				rotation = obj.transform.rotation;
			}
			
			public override string ToString() {
				return prefabName+" ["+tech+"] @ "+obj.transform.position+" / "+obj.transform.rotation.eulerAngles+" / "+obj.transform.localScale+" ("+referenceID+")"+" "+(isSelected ? "*" : "");
			}
			
			internal override XmlElement asXML(XmlDocument doc) {
				XmlElement n = base.asXML(doc);
				return n;
			}
			
			public static PlacedObject fromXML(XmlElement e, CustomPrefab pfb) {
				try {
					PlacedObject b = createPrefab(pfb.prefabName);
					if (b != null) {
						b.setPosition(pfb.position);
						b.rotation = pfb.rotation;
						b.obj.transform.rotation = b.rotation;
						b.fx.transform.rotation = b.rotation;
						b.obj.transform.localScale = b.scale;
						if (b.tech == TechType.None && pfb.tech != TechType.None)
							b.tech = pfb.tech;
						b.manipulations.AddRange(pfb.manipulations);
						//SBUtil.writeToChat("S"+b.prefabName);
						if (pfb.isDatabox) {
							//SBUtil.writeToChat("Reprogramming databox");
							BlueprintHandTarget bpt = b.obj.GetComponentInParent<BlueprintHandTarget>();
							if (bpt != null) {
								bpt.unlockTechType = b.tech;
							}
							else {
								SBUtil.writeToChat("Databox had no blueprint component!");
							}
						}
						else if (pfb.isCrate) {
							//SBUtil.writeToChat("Reprogramming crate");
							SupplyCrate bpt = b.obj.GetComponentInParent<SupplyCrate>();
							if (bpt != null) {
								SBUtil.setCrateItem(bpt, b.tech);
							}
							else {
								SBUtil.writeToChat("Crate had no supply component!");
							}
						}
						else if (pfb.isFragment) {
							
						}
						foreach (ManipulationBase mb in b.manipulations) {
							mb.applyToObject(b);
						}
						return b;
					}
					else {
						return null;
					}
				}
				catch (Exception ex) {
					SBUtil.log("Could not construct placed object from XML: "+ex);
					return null;
				}
			}
		
			internal static PlacedObject createPrefab(string id) {
				GameObject prefab;
				if (id != null && UWE.PrefabDatabase.TryGetPrefab(id, out prefab)) {
				//if (id != null && SBUtil.getPrefab(id, out prefab)) {
					if (prefab != null) {
						GameObject go = UnityEngine.Object.Instantiate(prefab);
						if (go != null) {
							go.SetActive(true);
							BuilderPlaced sel = go.AddComponent<BuilderPlaced>();
							PlacedObject ret = new PlacedObject(go, id);
							sel.placement = ret;
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
			
		}
}
