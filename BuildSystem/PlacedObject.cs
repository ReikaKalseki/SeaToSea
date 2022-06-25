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
			
			private static GameObject bubblePrefab = null;
			
			[SerializeField]
			internal int referenceID;
			[SerializeField]
			internal GameObject obj;
			[SerializeField]
			internal GameObject fx;
			
			[SerializeField]
			internal bool isSelected;
			
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
				key(go);				
				createFX();
			}
			
			private void createFX() {
				try {
					if (fx != null) {
						UnityEngine.Object.Destroy(fx);
					}
					if (bubblePrefab == null) {
						if (!UWE.PrefabDatabase.TryGetPrefab("fca5cdd9-1d00-4430-8836-a747627cdb2f", out bubblePrefab)) {
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
				tech = CraftData.GetTechType(go);
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
			
			public override void saveToXML(XmlElement e) {
				base.saveToXML(e);
			}
			
			public override void loadFromXML(XmlElement e) {
				base.loadFromXML(e);
				
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
				
				foreach (ManipulationBase mb in manipulations) {
					mb.applyToObject(this);
				}
				
				setPosition(position);
				setRotation(rotation);
				obj.transform.localScale = scale;
				if (fx != null && fx.transform != null)
					fx.transform.localScale = scale;
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
			
			private static PlacedObject createNewObject(string id, bool basePiece) {
				if (id == null) {
					SNUtil.writeToChat("Prefab not placed; ID was null");
					return null;
				}
				GameObject go = basePiece ? ObjectUtil.getBasePiece(id) : ObjectUtil.createWorldObject(id);
				if (go != null) {
					BuilderPlaced sel = go.AddComponent<BuilderPlaced>();
					PlacedObject ret = new PlacedObject(go, id);
					sel.placement = ret;
					//SNUtil.dumpObjectData(ret.obj);
					return ret;
				}
				return null;
			}
			
		}
}
