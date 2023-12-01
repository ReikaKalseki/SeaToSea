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
	internal class FCSWreck : MonoBehaviour {
	
		private float lastCheckTime = -1;
       	
		void Apply() {
			//SNUtil.writeToChat("Initializing FCS wreck");
			ObjectUtil.removeChildObject(gameObject, "ExteriorEntities/Starship_doors_frame");
			ObjectUtil.removeChildObject(gameObject, "ExteriorEntities/vent_constructor_section_01");
			//GameObject hull1 = ObjectUtil.getChildObject(gameObject, "ExteriorEntities/ExplorableWreckHull01");
			GameObject hull2 = ObjectUtil.getChildObject(gameObject, "ExteriorEntities/ExplorableWreckHull02");
			
			hull2.transform.rotation = Quaternion.Euler(0, 116, 210.7F);
			hull2.transform.localPosition += new Vector3(-1, 0, -7.5F);
		}
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastCheckTime >= 1) {
				lastCheckTime = time;
				if (ObjectUtil.getChildObject(gameObject, "ExteriorEntities/vent_constructor_section_01")) {
					Apply();
					
					foreach (BlueprintHandTarget bpt in gameObject.GetComponentsInChildren<BlueprintHandTarget>()) {
						UnityEngine.Object.Destroy(bpt.gameObject);
					}
					foreach (DataboxSpawner bpt in gameObject.GetComponentsInChildren<DataboxSpawner>()) {
						UnityEngine.Object.Destroy(bpt.gameObject);
					}
					foreach (StoryHandTarget bpt in gameObject.GetComponentsInChildren<StoryHandTarget>()) {
						UnityEngine.Object.Destroy(bpt.gameObject);
					}
				}
			}
		}
		
	}
}
