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
	internal class FCSWreck : MonoBehaviour {

		private float lastCheckTime = -1;

		void Apply() {
			//SNUtil.writeToChat("Initializing FCS wreck");
			gameObject.removeChildObject("ExteriorEntities/Starship_doors_frame");
			gameObject.removeChildObject("ExteriorEntities/vent_constructor_section_01");
			//GameObject hull1 = gameObject.getChildObject("ExteriorEntities/ExplorableWreckHull01");
			GameObject hull2 = gameObject.getChildObject("ExteriorEntities/ExplorableWreckHull02");

			hull2.transform.rotation = Quaternion.Euler(0, 116, 210.7F);
			hull2.transform.localPosition += new Vector3(-1, 0, -7.5F);
		}

		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastCheckTime >= 1) {
				lastCheckTime = time;
				if (gameObject.getChildObject("ExteriorEntities/vent_constructor_section_01")) {
					this.Apply();

					foreach (BlueprintHandTarget bpt in gameObject.GetComponentsInChildren<BlueprintHandTarget>()) {
						bpt.gameObject.destroy(false);
					}
					foreach (DataboxSpawner bpt in gameObject.GetComponentsInChildren<DataboxSpawner>()) {
						bpt.gameObject.destroy(false);
					}
					foreach (StoryHandTarget bpt in gameObject.GetComponentsInChildren<StoryHandTarget>()) {
						bpt.gameObject.destroy(false);
					}
				}
			}
		}

	}
}
