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
	internal class SeabaseReconstruction : ManipulationBase {
		
		private static readonly Dictionary<string, XmlElement> dataCache = new Dictionary<string, XmlElement>();
		
		private readonly XmlElement data;
		private readonly string id;
		
		internal SeabaseReconstruction(XmlElement e) {
			data = e;
			id = e.getProperty("identifier");
			dataCache[id] = data;
		}
		
		internal override void applyToObject(GameObject go) {
			SNUtil.log("Reconstructing seabase with "+data.ChildNodes.Count+" parts");/*
			BaseRoot b = go.GetComponent<BaseRoot>();
			b.noPowerNotification = null;
			b.welcomeNotification = null;
			b.welcomeNotificationEmergency = null;
			b.welcomeNotificationIssue = null;
			b.hullBreachNotification = null;
			b.hullRestoredNotification = null;
			b.hullDamageNotification = null;
			b.fireNotification = null;*/
			go.SetActive(true);/*
			foreach (Planter p in go.GetComponentsInChildren<Planter>(true)) {
				try {
					p.InitPlantsDelayed();
				}
				catch (Exception e) {
				
				}
			}*/
			ObjectUtil.removeChildObject(go, "SubDamageSounds");
			ObjectUtil.removeChildObject(go, "PowerAttach");
			ObjectUtil.removeComponent<BaseFloodSim>(go);
			ObjectUtil.removeComponent<BaseHullStrength>(go);
			ObjectUtil.removeComponent<BasePowerRelay>(go);
			ObjectUtil.removeComponent<PowerFX>(go);
			ObjectUtil.removeComponent<VoiceNotificationManager>(go);
			ObjectUtil.removeComponent<VoiceNotification>(go);
			ObjectUtil.removeComponent<BaseRoot>(go);
			ObjectUtil.removeComponent<Base>(go);
			WorldgenSeabaseController ws = go.EnsureComponent<WorldgenSeabaseController>();
			ws.reconstructionData = data;
			ws.seabaseID = id;
			GameObject holder = new GameObject();
			holder.name = id;
			holder.EnsureComponent<SeabaseIDHolder>();
			holder.transform.parent = go.transform;
			go.GetComponent<LightingController>().state = LightingController.LightingState.Damaged;
			//go.EnsureComponent<BaseHider>();
			Vector3 pos = data.getVector("position").Value;
			go.transform.position = pos;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			SNUtil.log("Finished deserializing seabase @ "+pos);
		}
		
		class SeabaseIDHolder : MonoBehaviour {
			
		}
		
		class WorldgenSeabaseController : MonoBehaviour {
			
			private static readonly string GEN_MARKER = "GenMarker";
			
			internal XmlElement reconstructionData;
			internal string seabaseID;
		
			//private Planter[] planters = null;
			private StorageContainer[] storages = null;
			private Charger[] chargers = null;
			
			void rebuild() {
				SNUtil.log("Seabase '"+seabaseID+"' undergoing reconstruction");
				if (reconstructionData == null) {
					SNUtil.writeToChat("Cannot rebuild worldgen seabase @ "+transform.position+" - no data");
					return;
				}
				bool isNew = true;
				foreach (XmlElement e2 in reconstructionData.getDirectElementsByTagName("part")) {
					CustomPrefab pfb = new CustomPrefab("9d3e9fa5-a5ac-496e-89f4-70e13c0bedd5"); //BaseCell
					pfb.loadFromXML(e2);
					bool isNewL = !baseHasPart(gameObject, pfb);
					if (!isNewL && pfb.prefabName != "9d3e9fa5-a5ac-496e-89f4-70e13c0bedd5") { //ie is loose
						SNUtil.log("Skipped recreate of loose piece: "+pfb);
						isNew = false;
						continue;
					}
					SNUtil.log("Reconstructed BaseCell/loose piece: "+pfb);
					GameObject go2 = pfb.createWorldObject();
					go2.transform.parent = gameObject.transform;
					List<XmlElement> li1 = e2.getDirectElementsByTagName("cellData");
					if (li1.Count == 1) {
						foreach (XmlElement e3 in li1[0].getDirectElementsByTagName("component")) {
							CustomPrefab pfb2 = new CustomPrefab("basePart");
							//Base.Piece type = Enum.Parse(typeof(Base.Piece), e3.getProperty("piece"));
							pfb2.loadFromXML(e3);
							if (pfb2.prefabName == PlacedObject.BUBBLE_PREFAB)
								continue;
							SNUtil.log("Reconstructed base component: "+pfb2);
							GameObject go3 = pfb2.createWorldObject();
							go3.transform.parent = go2.transform;
							rebuildNestedObjects(go3, e3);
							if (!reconstructionData.getBoolean("allowDeconstruct")) {
								ObjectUtil.removeComponent<BaseDeconstructable>(go3);
								ObjectUtil.removeComponent<Constructable>(go3);
								PreventDeconstruction pv = go3.EnsureComponent<PreventDeconstruction>();
								pv.inBase = true;
								pv.inCyclops = true;
								pv.inEscapePod = true;
							}
							List<XmlElement> li0 = e3.getDirectElementsByTagName("supportData");
							if (li0.Count == 1)
								new SeabaseLegLengthPreservation(li0[0]).applyToObject(go3);
							li0 = e3.getDirectElementsByTagName("modify");
							if (li0.Count == 1) {
								List<ManipulationBase> li2 = new List<ManipulationBase>();
								CustomPrefab.loadManipulations(li0[0], li2);
								foreach (ManipulationBase mb in li2) {
									mb.applyToObject(go3);
								}
							}
						}
					}
					if (isNew) {
						li1 = e2.getDirectElementsByTagName("inventory");
						if (li1.Count == 1) {
							StorageContainer sc = go2.GetComponent<StorageContainer>();
							Charger cg = go2.GetComponent<Charger>();
							Planter p = go2.GetComponent<Planter>();
							if (sc == null && cg == null) {
								SNUtil.log("Tried to deserialize inventory to a null container in "+go2);
								continue;
							}
							GrowbedPropifier pg = null;
							if (p != null) {
								pg = go2.EnsureComponent<GrowbedPropifier>();
							}
							foreach (XmlElement e3 in li1[0].getDirectElementsByTagName("item")) {
								TechType tt = SNUtil.getTechType(e3.getProperty("type"));
								if (tt == TechType.None) {
									SNUtil.log("Could not deserialize item - null TechType: "+e3.OuterXml);
								}
								else {
									GameObject igo = CraftData.GetPrefabForTechType(tt);
									if (igo == null) {
										SNUtil.log("Could not deserialize item - resulted in null: "+e3.OuterXml);
										continue;
									}
									int amt = e3.getInt("amount", 1);
									string slot = e3.getProperty("slot", true);
									for (int i = 0; i < amt; i++) {
										GameObject igo2 = UnityEngine.Object.Instantiate(igo);
										igo2.SetActive(false);
										Pickupable pp = igo2.GetComponent<Pickupable>();
										InventoryItem item = null;
										if (pp == null) {
											SNUtil.log("Could not deserialize item - no pickupable: "+e3.OuterXml);
										} 
										if (cg != null) {
											cg.equipment.AddItem(slot, new InventoryItem(pp), true);
										}
										else if (sc != null) {
											item = sc.container.AddItem(pp);
										}
									}
								}
							}
						}
					}
				}
			}

			void Update() {
				if (seabaseID == null)
					seabaseID = gameObject.GetComponentInChildren<SeabaseIDHolder>().name;
				if (reconstructionData == null) {
					reconstructionData = dataCache[seabaseID];
				}
				foreach (Transform t in transform) {
					if (t.gameObject.name.Contains("BaseCell") && t.childCount == 0) {
						UnityEngine.Object.Destroy(t.gameObject);
					}
				}
				if (!ObjectUtil.getChildObject(gameObject, "BaseCell")) {/*
					GameObject marker = ObjectUtil.getChildObject(gameObject, GEN_MARKER);
					bool isNew = !marker;
					if (!marker) {
						marker = new GameObject();
						marker.name = GEN_MARKER;
						marker.transform.parent = transform;
					}*/
					rebuild();
				}
				GetComponent<LightingController>().state = LightingController.LightingState.Damaged;/*
				if (planters == null) {
					planters = gameObject.GetComponentsInChildren<Planter>();
				}*/
				if (storages == null) {
					storages = gameObject.GetComponentsInChildren<StorageContainer>();
				}
				if (chargers == null) {
					chargers = gameObject.GetComponentsInChildren<Charger>();
				}
				foreach (StorageContainer p in storages) {
					if (p.container.IsEmpty() && p.storageRoot.transform.childCount > 0) {
						try {
							foreach (Pickupable pp in p.GetComponentsInChildren<Pickupable>(true)) {
								p.container.AddItem(pp);
							}
						}
						catch (Exception e) {
							SNUtil.log("Exception initializing worldgen seabase inventory @ "+p.transform.position+": "+e);
						}
					}
				}
				/*
				foreach (Planter p in planters) {
					if (p.grownPlantsRoot.childCount == 0 && p.storageContainer.storageRoot.transform.childCount > 0) {
						try {
							//p.InitPlantsDelayed();
							foreach (Pickupable pp in p.storageContainer.GetComponentsInChildren<Pickupable>(true)) {
								p.AddItem(new InventoryItem(pp));
							}
						}
						catch (Exception e) {
							SNUtil.log("Exception initializing worldgen seabase planter @ "+p.transform.position+": "+e);
						}
					}
				}*/
				foreach (Charger p in chargers) {
					//SNUtil.writeToChat(p+" @ "+p.transform.position+" : "+p.equipment.equippedCount.Count+" : "+p.equipmentRoot.transform.childCount);
					if (p.equipment.equippedCount.Count == 0 && p.equipmentRoot.transform.childCount > 0) {
						try {
							int i = 0;
							Pickupable[] pc = p.equipmentRoot.GetComponentsInChildren<Pickupable>(true);
							//SNUtil.writeToChat("PC"+pc.Length+" > "+string.Join(",", p.slots.Keys));
							foreach (string key in p.slots.Keys) {
								p.equipment.AddItem(key, new InventoryItem(pc[i]), true);
								i++;
								if (i >= pc.Length)
									break;
							}
							p.opened = true;
							p.animator.SetBool(p.animParamOpen, true);
							p.ToggleUI(true);
						}
						catch (Exception e) {
							SNUtil.log("Exception initializing worldgen seabase charger @ "+p.transform.position+": "+e);
						}
					}
				}
			}
			
		}
		/*
		class BaseHider : MonoBehaviour {
			
			void Update() {
				bool active = gameObject.activeSelf;
				gameObject.SetActive(Vector3.Distance(Player.main.transform.position, transform.position) <= 100);
				if (active != gameObject.activeSelf) {
					SNUtil.writeToChat("Toggling seabase @ "+transform.position+": "+active+" > "+gameObject.activeSelf);
				}
			}
			
		}*/
		
		private static bool baseHasPart(GameObject main, CustomPrefab pfb) {
			foreach (Transform t in main.transform) {
				PrefabIdentifier pi = t.GetComponent<PrefabIdentifier>();
				if (!pi || pi.ClassId != pfb.prefabName)
					continue;
				if (Vector3.Distance(pfb.position, t.position) >= 0.1)
					continue;
				return true;
			}
			return false;
		}
			
		private static void rebuildNestedObjects(GameObject main, XmlElement e) {
			foreach (XmlElement e2 in e.getDirectElementsByTagName("child")) {
				CustomPrefab pfb = new CustomPrefab(e2.getProperty("prefab"));
				pfb.loadFromXML(e2);
				GameObject go = pfb.createWorldObject();
				if (go != null) {
					go.transform.parent = main.transform;
					rebuildNestedObjects(go, e2);
				}
			}
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			
		}
		
		internal override void saveToXML(XmlElement e) {
			
		}
		
		public override bool needsReapplication() {
			return true;
		}
		
		class GrowbedPropifier : MonoBehaviour {
			
			void Update() {
				Planter p = gameObject.GetComponent<Planter>();
				if (p != null) {
					foreach (Transform t in p.slots) {
						if (t != null) {
							GrowingPlant g = t.gameObject.GetComponentInChildren<GrowingPlant>(true);
							if (g != null)
								g.SetProgress(1);
						}
					}
					foreach (Transform t in p.bigSlots) {
						if (t != null) {
							GrowingPlant g = t.gameObject.GetComponentInChildren<GrowingPlant>(true);
							if (g != null)
								g.SetProgress(1);
						}
					}
					gameObject.GetComponent<StorageContainer>().enabled = false;
				}
			}
			
		}
		
	}
}
