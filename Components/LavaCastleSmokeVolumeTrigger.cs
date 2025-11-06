using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

using ReikaKalseki.Ecocean;

namespace ReikaKalseki.SeaToSea {
	public class LavaCastleSmokeVolumeTrigger : MonoBehaviour {

		private static float lastCollectTime = 0;

		private GameObject sparkleObject;

		private ParticleSystem[] particles;

		private float animSpeed = UnityEngine.Random.Range(0.2F, 0.3F)*1.25F;

		private float age;

		void OnTriggerStay(Collider other) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastCollectTime < 2.5F || UnityEngine.Random.Range(0F, 1F) > 0.3F)
				return;
			SeaMoth sm = other.gameObject.FindAncestor<SeaMoth>();
			if (sm) {
				SeamothPlanktonScoop.checkAndTryScoop(sm, Time.deltaTime, CraftingItems.getItem(CraftingItems.Items.LavaPlankton).TechType, out GameObject drop);
				if (drop) {
					lastCollectTime = time;
				}
			}
			
		}

		void Update() {
			age += Time.deltaTime;

			if (!sparkleObject) {
				sparkleObject = gameObject.getChildObject("Sparkle");
				particles = sparkleObject.GetComponentsInChildren<ParticleSystem>();
			}

			foreach (ParticleSystem p in particles) {
				ParticleSystem.MainModule main = p.main;
				main.simulationSpeed = animSpeed * (float)MathUtil.linterpolate(age, 0, 2, 100, 1, true);
			}
		}


	}
}
