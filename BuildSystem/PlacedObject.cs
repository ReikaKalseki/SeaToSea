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
		
			public static readonly string TAGNAME = "object";
			
			private static GameObject bubblePrefab = null;
			
			[SerializeField]
			internal int referenceID;
			[SerializeField]
			internal GameObject obj;
			[SerializeField]
			internal GameObject fx;
			
			[SerializeField]
			internal bool isSelected;
			
			static PlacedObject() { //TODO might not be inited in time, might need to call explicitly, eg via buildinghandler class init
				registerType(TAGNAME, e => {
					CustomPrefab pfb = new CustomPrefab(e.getProperty("prefab"));
					pfb.loadFromXML(e);
					return PlacedObject.fromXML(e, pfb);
				});
			}
		
			public string getTagName() {
				return TAGNAME;
			}
			
			internal PlacedObject(GameObject go, string pfb) : base(pfb) {
				if (go == null)
					throw new Exception("Tried to make a place of a null obj!");
				if (go.transform == null)
					SBUtil.log("Place of obj "+go+" has null transform?!");
				key(go);
				
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
		
			public sealed override void replaceObject(string pfb) {
				base.replaceObject(pfb);
				
				GameObject put = SBUtil.createWorldObject(pfb);
				if (put != null && put.transform != null) {
					UnityEngine.Object.Destroy(obj);
					key(put);
					put.transform.position = this.position;
					put.transform.rotation = this.rotation;
					put.transform.localScale = this.scale;
				}
			}
			
			private void key(GameObject go) {
				obj = go;
				referenceID = BuildingHandler.genID(go);
				tech = CraftData.GetTechType(go);
			}
			
			internal void setSelected(bool sel) {
				isSelected = sel;
				try {
					fx.SetActive(isSelected);
				}
				catch (Exception ex) {
					SBUtil.writeToChat("Could not set enabled state of "+this+" due to FX ("+fx+") GO error: "+ex.ToString());
				}
			}
			
			internal void setPosition(Vector3 pos, bool printCoord = false) {
				position = pos;
				obj.transform.position = position;
				fx.transform.position = position;
				if (printCoord) {
					SBUtil.writeToChat(position.ToString());
				}
			}
		
			internal void move(Vector3 mov, bool printCoord = false) {
				move(mov.x, mov.y, mov.z, printCoord);
			}
		
			internal void move(double x, double y, double z, bool printCoord = false) {
				Vector3 vec = obj.transform.position;
				vec.x += (float)x;
				vec.y += (float)y;
				vec.z += (float)z;
				setPosition(vec, printCoord);
				//SBUtil.writeToChat(go.obj.transform.position.ToString());
			}
			
			internal void rotateYaw(double ang, bool printCoord = false) {
				rotate(0, ang, 0, printCoord);
			}
			
			internal void rotate(double roll, double yaw, double pitch, bool printCoord = false) {
				Vector3 euler = obj.transform.rotation.eulerAngles;
				setRotation(Quaternion.Euler(euler.x+(float)roll, euler.y+(float)yaw, euler.z+(float)pitch), printCoord);
				//SBUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
			}
			
			internal void setRotation(Quaternion rot, bool printCoord = false) {
				obj.transform.rotation = rot;
				fx.transform.rotation = rot;
				//SBUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
				rotation = rot;
				if (printCoord) {
					SBUtil.writeToChat(rotation.eulerAngles.ToString());
				}
			}
			
			public override string ToString() {
				try {
					Transform t = obj.transform;
					string pos = t == null ? "null-transform @ "+position+" / "+rotation+" / "+scale : t.position+" / "+t.rotation.eulerAngles+" / "+t.localScale;
					return prefabName+" ["+tech+"] @ "+pos+" ("+referenceID+")"+" "+(isSelected ? "*" : "");
				}
				catch (Exception ex) {
					return "Errored "+prefabName+" @ "+position+": "+ex.ToString();
				}
			}
			
			public override void saveToXML(XmlElement e) {
				base.saveToXML(e);
			}
			
			public override void loadFromXML(XmlElement e) {
				base.loadFromXML(e);
				
				if (isDatabox) {
					//SBUtil.writeToChat("Reprogramming databox");
					BlueprintHandTarget bpt = obj.EnsureComponent<BlueprintHandTarget>();
					bpt.unlockTechType = tech;
				}
				else if (isCrate) {
					//SBUtil.writeToChat("Reprogramming crate");
					SupplyCrate bpt = obj.EnsureComponent<SupplyCrate>();
					SBUtil.setCrateItem(bpt, tech);
				}
				else if (isFragment) {
					//TechFragment frag = b.obj.EnsureComponent<TechFragment>();
				}
				
				foreach (ManipulationBase mb in manipulations) {
					mb.applyToObject(this);
				}
			}
		
			protected override void setPrefabName(string name) {
				string old = prefabName;
				base.setPrefabName(name);
				if (old != name)
					replaceObject(name);
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
							BlueprintHandTarget bpt = b.obj.EnsureComponent<BlueprintHandTarget>();
							bpt.unlockTechType = b.tech;
						}
						else if (pfb.isCrate) {
							//SBUtil.writeToChat("Reprogramming crate");
							SupplyCrate bpt = b.obj.EnsureComponent<SupplyCrate>();
							SBUtil.setCrateItem(bpt, b.tech);
						}
						else if (pfb.isFragment) {
							//TechFragment frag = b.obj.EnsureComponent<TechFragment>();
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
				if (id == null) {
					SBUtil.writeToChat("Prefab not placed; ID was null");
					return null;
				}
				GameObject go = SBUtil.createWorldObject(id);
				if (go != null) {
					BuilderPlaced sel = go.AddComponent<BuilderPlaced>();
					PlacedObject ret = new PlacedObject(go, id);
					sel.placement = ret;
					//SBUtil.dumpObjectData(ret.obj);
					return ret;
				}
				return null;
			}
			
		}
}
