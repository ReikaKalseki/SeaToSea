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
	internal class TabletFragmentTag : MonoBehaviour {
		
		private MeshRenderer[] models;
		
		private GameObject sparker;
		
		private Light light;
		
		private ParticleSystem[] particles;
		
		private float lastRenderUpdateTime;
		
		void Update() {
			if (models == null)
				models = ObjectUtil.getChildObject(gameObject, "precursor_key_cracked_01").GetComponentsInChildren<MeshRenderer>();
			if (!light)
				light = GetComponentInChildren<Light>();
			if (light && !sparker) {
				sparker = ObjectUtil.getChildObject(gameObject, "Sparks");
				if (!sparker)
					sparker = ObjectUtil.createWorldObject("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc");
				sparker.name = "Sparks";
				sparker.transform.parent = transform;
				sparker.transform.localScale = new Vector3(0.4F, 0.4F, 0.4F);
				sparker.transform.localPosition = models.Length > 1 ? Vector3.zero : models[0].transform.localPosition;
			}
			float time = DayNightCycle.main.timePassedAsFloat;
			if (sparker && time-lastRenderUpdateTime > 0.5F) {
				ObjectUtil.removeComponent<DamagePlayerInRadius>(sparker);
				ObjectUtil.removeChildObject(sparker, "ElecLight");
				ObjectUtil.removeChildObject(sparker, "xElec");
				ObjectUtil.removeComponent<DamagePlayerInRadius>(sparker);
				ObjectUtil.removeChildObject(gameObject, "xUnderwaterElecSource_medium");
				foreach (ParticleSystemRenderer r in sparker.GetComponentsInChildren<ParticleSystemRenderer>()) {
					foreach (Material m in r.materials) {
						m.SetColor("_Color", light.color);
						m.SetVector("_ScrollSpeed", Vector4.one*-0.01F);
					}
				}
				lastRenderUpdateTime = time;
			}
			if (particles == null) {
				particles = sparker.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem pp in particles) {
					ParticleSystem.MainModule main = pp.main;
					ParticleSystem.EmissionModule em = pp.emission;
					em.rateOverTime = 2.4F;
					main.startSize = models.Length > 1 ? 1 : 0.67F;
					main.startLifetime = 2.5F;
				}
			}
		}
		
		void OnScanned() {
	   		TechType tt = CraftData.GetTechType(gameObject);
	   		SNUtil.log("Scanned tablet fragment "+gameObject+" @ "+transform.position+", TT="+tt);
	   		if (tt == TechType.PrecursorKey_PurpleFragment) {
	   			if (Vector3.Distance(transform.position, WorldUtil.DEGASI_FLOATING_BASE) <= 20) {
	   				Story.StoryGoal.Execute("ScanFloatingIslandTablet", Story.GoalType.Story);
	   			}
	   			else if (WorldUtil.isMountainIsland(transform.position)) {
	   				Story.StoryGoal.Execute("ScanMountainIslandTablet", Story.GoalType.Story);
	   			}
	   		}
		}
		
	}
}
