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
		
		private readonly XmlElement data;
		private bool preventDeconstruction;
		
		internal SeabaseReconstruction(XmlElement e) {
			data = e;
			preventDeconstruction = !e.getBoolean("allowDeconstruct");
		}
		
		internal override void applyToObject(GameObject go) {
			SNUtil.log("Reconstructing seabase with "+data.ChildNodes.Count+" parts");
			foreach (XmlElement e2 in data.getDirectElementsByTagName("part")) {
				CustomPrefab pfb = new CustomPrefab("9d3e9fa5-a5ac-496e-89f4-70e13c0bedd5"); //BaseCell
				pfb.loadFromXML(e2);
				SNUtil.log("Reconstructed BaseCell/loose piece: "+pfb);
				GameObject go2 = pfb.createWorldObject();
				go2.transform.parent = go.transform;
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
						if (preventDeconstruction) {
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
								igo = UnityEngine.Object.Instantiate(igo);
								igo.SetActive(false);
								Pickupable pp = igo.GetComponent<Pickupable>();
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
			BaseRoot b = go.GetComponent<BaseRoot>();
			b.noPowerNotification = null;
			b.welcomeNotification = null;
			b.welcomeNotificationEmergency = null;
			b.welcomeNotificationIssue = null;
			b.hullBreachNotification = null;
			b.hullRestoredNotification = null;
			b.hullDamageNotification = null;
			b.fireNotification = null;
			SNUtil.log("Finished deserializing seabase.");
		}
			
		private void rebuildNestedObjects(GameObject main, XmlElement e) {
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
