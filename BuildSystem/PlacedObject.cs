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
		internal sealed class PlacedObject : CustomPrefab {
		
			public static readonly new string TAGNAME = "object";
			
			public static readonly string BUBBLE_PREFAB = "fca5cdd9-1d00-4430-8836-a747627cdb2f";
			
			private static GameObject bubblePrefab = null;
			
			private static readonly Dictionary<string, PlacedObject> ids = new Dictionary<string, PlacedObject>();
			
			[SerializeField]
			internal int referenceID;
			[SerializeField]
			internal GameObject obj;
			[SerializeField]
			internal GameObject fx;
			
			[SerializeField]
			internal bool isSelected;
			
			internal PlacedObject parent = null;
			
			static PlacedObject() {
				registerType(TAGNAME, e => PlacedObject.fromXML(e, false));
			}
		
			public override string getTagName() {
				return TAGNAME;
			}
			
			internal PlacedObject(GameObject go, string pfb) : base(pfb) {
				if (go == null)
					throw new Exception("Tried to make a place of a null obj!");
				if (go.transform == null)
					SNUtil.log("Place of obj "+go+" has null transform?!");
				position = go.transform.position;
				rotation = go.transform.rotation;
				scale = go.transform.localScale;
				tech = CraftData.GetTechType(go);
				isBasePiece = pfb.StartsWith("Base_", StringComparison.InvariantCultureIgnoreCase);
				if (isBasePiece)
					prefabName = prefabName.Substring(5);
				key(go);				
				createFX();
			}
			
			private void createFX() {
				try {
					if (fx != null) {
						UnityEngine.Object.Destroy(fx);
					}
					if (bubblePrefab == null) {
						if (!UWE.PrefabDatabase.TryGetPrefab(BUBBLE_PREFAB, out bubblePrefab)) {
						//if (!SNUtil.getPrefab("fca5cdd9-1d00-4430-8836-a747627cdb2f", out bubblePrefab)) {
							SNUtil.writeToChat("Bubbles not loadable!");
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
							SNUtil.writeToChat("Bubbles not constructable!");
						}
					}
					else {
						SNUtil.writeToChat("Bubbles not found.");
					}
				}
				catch (Exception e) {
					throw new Exception("Error in bubbles", e);
				}
			}
		
			public sealed override void replaceObject(string pfb) {
				base.replaceObject(pfb);
				
				GameObject put = ObjectUtil.createWorldObject(pfb);
				if (put != null && put.transform != null) {
					UnityEngine.Object.Destroy(obj);
					key(put);
					put.transform.position = this.position;
					put.transform.rotation = this.rotation;
					put.transform.localScale = this.scale;
					createFX();
				}
			}
			
			private void key(GameObject go) {
				obj = go;
				referenceID = BuildingHandler.genID(go);
				xmlID = Guid.NewGuid();
				ids[xmlID.ToString()] = this;
				tech = CraftData.GetTechType(go);
			}
			
			internal void destroy() {
				if (xmlID != null && xmlID.HasValue)
					ids.Remove(xmlID.Value.ToString());
			}
			
			internal void setSelected(bool sel) {
				isSelected = sel;
				if (fx == null) {
					SNUtil.writeToChat("Could not set enabled visual of "+this+" due to null FX GO");
					return;
				}
				try {
					fx.SetActive(isSelected);
				}
				catch (Exception ex) {
					SNUtil.writeToChat("Could not set enabled visual of "+this+" due to FX ("+fx+") GO error");
					SNUtil.log("Could not set enabled visual of "+this+" due to FX ("+fx+") GO error: "+ex.ToString());
				}
			}
			
			internal void setPosition(Vector3 pos) {
				position = pos;
				obj.transform.position = position;
				if (fx != null && fx.transform != null)
					fx.transform.position = position;
			}
		
			internal void move(Vector3 mov) {
				move(mov.x, mov.y, mov.z);
			}
		
			internal void move(double x, double y, double z) {
				Vector3 vec = obj.transform.position;
				vec.x += (float)x;
				vec.y += (float)y;
				vec.z += (float)z;
				setPosition(vec);
				//SNUtil.writeToChat(go.obj.transform.position.ToString());
			}
			
			internal void rotateYaw(double ang, Vector3? relTo) {
				rotate(0, ang, 0, relTo);
			}
			
			internal void rotate(double roll, double yaw, double pitch, Vector3? relTo) {
				Vector3 ctr = position;
				Vector3 up = obj.transform.up;
				Vector3 forward = obj.transform.forward;
				Vector3 right = obj.transform.right;
				if (relTo != null && relTo.HasValue) {
					ctr = relTo.Value;
					up = Vector3.up;
					forward = Vector3.forward;
					right = Vector3.right;
					if (Math.Abs(yaw) > 0.001)
						obj.transform.RotateAround(ctr, up, (float)yaw);
					if (Math.Abs(roll) > 0.001)
						obj.transform.RotateAround(ctr, forward, (float)roll);
					if (Math.Abs(pitch) > 0.001)
						obj.transform.RotateAround(ctr, right, (float)pitch);
					setRotation(obj.transform.rotation);
				}
				else {
					Vector3 euler = obj.transform.rotation.eulerAngles;
					setRotation(Quaternion.Euler(euler.x+(float)roll, euler.y+(float)yaw, euler.z+(float)pitch));
				//SNUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
				}
			}
			
			internal void setRotation(Quaternion rot) {
				obj.transform.rotation = rot;
				if (fx != null && fx.transform != null)
					fx.transform.rotation = rot;
				//SNUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
				rotation = rot;
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
			
			private void nestObject(GameObject go, XmlElement e) {
				PlacedObject p = createNewObject(go);
				if (p != null) {
					XmlElement e2 = e.OwnerDocument.CreateElement("child");
					p.saveToXML(e2);
					e.AppendChild(e2);
					foreach (Transform t in go.transform) {
						nestObject(t.gameObject, e2);
					}
				}
			}
			
			public override void saveToXML(XmlElement e) {
				base.saveToXML(e);
				
				if (parent != null && parent.xmlID != null && parent.xmlID.HasValue) {
					e.addProperty("parent", parent.xmlID.ToString());
				}
				if (isSeabase) {
					foreach (Transform t in obj.transform) {
						GameObject go2 = t.gameObject;
						PlacedObject p2 = createNewObject(go2);
						if (p2 != null) {
							XmlElement cell = e.OwnerDocument.CreateElement("part");
							p2.saveToXML(cell);
							BaseCell bc = go2.GetComponent<BaseCell>();
							StorageContainer sc = go2.GetComponent<StorageContainer>();
							Charger cg = go2.GetComponent<Charger>();
							if (bc != null) {
								XmlElement e2 = e.OwnerDocument.CreateElement("cellData");
								foreach (Transform t2 in t) {
									PlacedObject p3 = createNewObject(t2.gameObject);
									if (p3 != null) {
										XmlElement e3 = e.OwnerDocument.CreateElement("component");
										p3.saveToXML(e3);
										e2.AppendChild(e3);
										foreach (Transform t3 in t2) {
											nestObject(t3.gameObject, e3);
										}
									}
								}
								cell.AppendChild(e2);
							}
							else if (sc != null) {
								XmlElement e2 = e.OwnerDocument.CreateElement("inventory");
								foreach (TechType tt in sc.container.GetItemTypes()) {
									XmlElement e3 = e.OwnerDocument.CreateElement("item");
									e3.addProperty("type", ""+tt);
									e3.addProperty("amount", sc.container.GetItems(tt).Count);
									e2.AppendChild(e3);
								}
								cell.AppendChild(e2);
							}
							else if (cg != null) {
								XmlElement e2 = e.OwnerDocument.CreateElement("inventory");
								foreach (KeyValuePair<string, InventoryItem> kvp in cg.equipment.equipment) {
									XmlElement e3 = e.OwnerDocument.CreateElement("item");
									e3.addProperty("type", ""+kvp.Value.item.GetTechType());
									e3.addProperty("slot", kvp.Key);
									e2.AppendChild(e3);
								}
								cell.AppendChild(e2);
							}
							e.AppendChild(cell);
						}
					}
				}
				else if (isBasePiece) {
					BaseFoundationPiece bf = obj.GetComponent<BaseFoundationPiece>();
					if (bf != null) {
						XmlElement e2 = e.OwnerDocument.CreateElement("supportData");
						e2.addProperty("maxHeight", bf.maxPillarHeight);
						e2.addProperty("extra", bf.extraHeight);
						e2.addProperty("minHeight", bf.minHeight);
						foreach (BaseFoundationPiece.Pillar p in bf.pillars) {
							Transform l = p.adjustable;
							if (l != null) {
								XmlElement e3 = e.OwnerDocument.CreateElement("pillar");
								e3.addProperty("position", l.position);
								e3.addProperty("rotation", l.rotation);
								e3.addProperty("scale", l.localScale);
								e2.AppendChild(e3);
							}
						}
						e.AppendChild(e2);
					}
				}
			}
			
			public override void loadFromXML(XmlElement e) {
				if (xmlID != null && xmlID.HasValue)
					ids.Remove(xmlID.Value.ToString());
				base.loadFromXML(e);
				if (xmlID != null && xmlID.HasValue)
					ids[xmlID.Value.ToString()] = this;
				
				if (isDatabox) {
					//SNUtil.writeToChat("Reprogramming databox");
					//SNUtil.setDatabox(obj.EnsureComponent<BlueprintHandTarget>(), tech);
				}
				else if (isCrate) {
					//SNUtil.writeToChat("Reprogramming crate");
					//SNUtil.setCrateItem(obj.EnsureComponent<SupplyCrate>(), tech);
				}
				else if (isFragment) {
					//TechFragment frag = b.obj.EnsureComponent<TechFragment>();
				}
				else if (isPDA) {
					//SNUtil.setPDAPage(obj.EnsureComponent<StoryHandTarget>(), page);
				}
				else if (isBasePiece) {
					//SNUtil
				}
				
				setPosition(position);
				setRotation(rotation);
				obj.transform.localScale = scale;
				if (fx != null && fx.transform != null)
					fx.transform.localScale = scale;
				
				string pp = e.getProperty("parent", true);
				if (!string.IsNullOrEmpty(pp) && ids.ContainsKey(pp)) {
					parent = ids[pp];
					if (parent != null)
						obj.transform.parent = parent.obj.transform;
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
			
			public static PlacedObject fromXML(XmlElement e, bool readXML = true) {
				CustomPrefab pfb = new CustomPrefab("");
				pfb.loadFromXML(e);
				SNUtil.log("Building placed object from custom prefab "+pfb+" > "+e.format());
				PlacedObject b = createNewObject(pfb);
				if (readXML)
					b.loadFromXML(e);
				return b;
			}
			
			internal static PlacedObject createNewObject(string id) {
				return createNewObject(id, id.StartsWith("base_", StringComparison.InvariantCultureIgnoreCase));
			}
		
			internal static PlacedObject createNewObject(CustomPrefab pfb) {
				return createNewObject(pfb.prefabName, pfb.isBasePiece);
			}
		
			internal static PlacedObject createNewObject(GameObject go) {
				string id = null;
				//SNUtil.log("Attempting builderObject from '"+go.name+"'");
				PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
				if (pi != null)
					id = pi.classId;
				if (id == BUBBLE_PREFAB)
					return null;
				if (pi == null && go.name.StartsWith("Base", StringComparison.InvariantCulture)) {
					string name = go.name.Replace("(Clone)", "").Substring(4);
					Base.Piece get = Base.Piece.Invalid;
					if (Enum.TryParse<Base.Piece>(name, out get)) {
						if (get != Base.Piece.Invalid) {
							id = "Base_"+name;
						}
					}
					if (id == null) {
						TechTag tt = go.GetComponent<TechTag>();
						if (tt != null && tt.type == TechType.BaseFoundation) {
							id = "Base_Foundation";
						}
					}
				}
				return string.IsNullOrEmpty(id) ? null : createNewObject(go, id);
			}
			
			private static PlacedObject createNewObject(string id, bool basePiece) {
				if (id == null) {
					SNUtil.writeToChat("Prefab not placed; ID was null");
					return null;
				}
				GameObject go = basePiece ? ObjectUtil.getBasePiece(id) : ObjectUtil.createWorldObject(id);
				return go == null ? null : createNewObject(go);
			}
			
			private static PlacedObject createNewObject(GameObject go, string id) {
				BuilderPlaced sel = go.AddComponent<BuilderPlaced>();
				PlacedObject ret = new PlacedObject(go, id);
				sel.placement = ret;
				//SNUtil.dumpObjectData(ret.obj);
				return ret;
			}
			
		}
}
