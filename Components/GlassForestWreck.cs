using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	public class GlassForestWreck : MonoBehaviour {

		private float lastCheckTime = -1;

		void Apply() {
			//SNUtil.writeToChat("Initializing glass forest wreck");
			gameObject.removeChildObject("Slots");
			//gameObject.removeChildObject("Starship_exploded_debris_*");
			//gameObject.removeChildObject("Starship_cargo_*");
			//gameObject.removeChildObject("*starship_work*");
			gameObject.removeChildObject("*DataBox*");
			gameObject.removeChildObject("*Spawner*");
			gameObject.removeChildObject("*PDA*");
			GameObject interior = gameObject.getChildObject("InteriorEntities");
			GameObject interior2 = gameObject.getChildObject("InteriorProps");
			GameObject exterior = gameObject.getChildObject("ExteriorEntities");
			exterior.removeChildObject("ExplorableWreckHull01");
			exterior.removeChildObject("ExplorableWreckHull02");
			/*
            foreach (GameObject go in gameObject.getChildObjects("*Starship_work*")) {
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
			interior.destroy(false);
			interior2.destroy(false);
			exterior.destroy(false);
		}

		private void addGravity(Transform t) {
			string n = t.name.ToLowerInvariant();
			if (!t.GetComponentInChildren<ParticleSystem>() && !n.Contains("modular_wall") && !n.Contains("details") && !n.Contains("virtualentity") && n[0] != 'x' && !n.Contains("crack") && !n.Contains("engine_console") && !n.Contains("wires") && !n.Contains("wall_planter") && !n.Contains("monitor") && !n.Contains("tech_box")) {
				Collider cc = t.GetComponentInChildren<Collider>(true);
				SNUtil.log("Adding gravity to " + n + " in " + t.gameObject.GetFullHierarchyPath().Substring(gameObject.GetFullHierarchyPath().Length) + " @ " + t.position + " (" + (cc != null) + ")");
				if (cc) {
					ObjectUtil.applyGravity(t.gameObject);
					GlassForestWreckProp prop = t.gameObject.EnsureComponent<GlassForestWreckProp>();
					prop.init("glassforestwreck", 90);
				}
				else {
					t.gameObject.destroy();
				}
			}
		}

		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastCheckTime >= 1) {
				lastCheckTime = time;
				if (gameObject.getChildObject("Slots") || gameObject.getChildObject("ExteriorEntities/ExplorableWreckHull01") || !this.GetComponentInChildren<GlassForestWreckProp>())
					this.Apply();
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
