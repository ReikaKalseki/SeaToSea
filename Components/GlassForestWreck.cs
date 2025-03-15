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
		public class GlassForestWreck : MonoBehaviour {
		
			private float lastCheckTime = -1;
        	
			void Apply() {
				//SNUtil.writeToChat("Initializing glass forest wreck");
				ObjectUtil.removeChildObject(gameObject, "Slots");
				//ObjectUtil.removeChildObject(gameObject, "Starship_exploded_debris_*");
				//ObjectUtil.removeChildObject(gameObject, "Starship_cargo_*");
				//ObjectUtil.removeChildObject(gameObject, "*starship_work*");
				ObjectUtil.removeChildObject(gameObject, "*DataBox*");
				ObjectUtil.removeChildObject(gameObject, "*Spawner*");
				ObjectUtil.removeChildObject(gameObject, "*PDA*");
				GameObject interior = ObjectUtil.getChildObject(gameObject, "InteriorEntities");
				GameObject interior2 = ObjectUtil.getChildObject(gameObject, "InteriorProps");
				GameObject exterior = ObjectUtil.getChildObject(gameObject, "ExteriorEntities");
				ObjectUtil.removeChildObject(exterior, "ExplorableWreckHull01");
				ObjectUtil.removeChildObject(exterior, "ExplorableWreckHull02");
				/*
				foreach (GameObject go in ObjectUtil.getChildObjects(gameObject, "*Starship_work*")) {
					ObjectUtil.applyGravity(go);
				}*/
				
				/*
				foreach (Transform t in interior.transform) {
					addGravity(t);
				}
				foreach (Transform t in interior2.transform) {
					addGravity(t);
				}
				foreach (Transform t in exterior.transform) {
					addGravity(t);
				}*/
				UnityEngine.Object.Destroy(interior);
				UnityEngine.Object.Destroy(interior2);
				UnityEngine.Object.Destroy(exterior);
			}
		
			private void addGravity(Transform t) {
				string n = t.name.ToLowerInvariant();
				if (!t.GetComponentInChildren<ParticleSystem>() && !n.Contains("modular_wall") && !n.Contains("details") && !n.Contains("virtualentity") && n[0] != 'x' && !n.Contains("crack") && !n.Contains("engine_console") && !n.Contains("wires") && !n.Contains("wall_planter") && !n.Contains("monitor") && !n.Contains("tech_box")) {
					Collider cc = t.GetComponentInChildren<Collider>(true);
					SNUtil.log("Adding gravity to "+n+" in "+t.gameObject.GetFullHierarchyPath().Substring(gameObject.GetFullHierarchyPath().Length)+" @ "+t.position+" ("+(cc != null)+")");
					if (cc) {
						ObjectUtil.applyGravity(t.gameObject);
						GlassForestWreckProp prop = t.gameObject.EnsureComponent<GlassForestWreckProp>();
						prop.init("glassforestwreck", 90);
					}
					else {
						UnityEngine.Object.DestroyImmediate(t.gameObject);
					}
				}
			}
			
			void Update() {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time-lastCheckTime >= 1) {
					lastCheckTime = time;
					if (ObjectUtil.getChildObject(gameObject, "Slots") || ObjectUtil.getChildObject(gameObject, "ExteriorEntities/ExplorableWreckHull01") || !GetComponentInChildren<GlassForestWreckProp>())
						Apply();
				}
			}
			
		}
	
	class GlassForestWreckProp : PhysicsSettlingProp {
		/*
		private static readonly Vector3 vent1 = new Vector3(-134.15F, -501, 940.29F);
		private static readonly Vector3 vent2 = new Vector3(-125.20F, -503, 936.16F);
		
		protected override void onUpdate() {
			Vector3 pos = transform.position;			
			//keep the vents clear
			if (Vector3.Distance(pos, vent1) <= 2F) {
				body.isKinematic = false;
				body.AddForce((pos-vent1).normalized*15, ForceMode.VelocityChange);
				return;
			}
			if (Vector3.Distance(pos, vent2) <= 2F) {
				body.isKinematic = false;
				body.AddForce((pos-vent2).normalized*15, ForceMode.VelocityChange);
				return;
			}
		}*/
		
	}
}
