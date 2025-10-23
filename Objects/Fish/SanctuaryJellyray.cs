using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class SanctuaryJellyray : RetexturedFish {

		private readonly XMLLocale.LocaleEntry locale;

		internal SanctuaryJellyray(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.JELLYRAY.prefab) {
			locale = e;
			glowIntensity = 0.5F;

			scanTime = 5;
			eggBase = TechType.Jellyray;
			eggMaturationTime = 3600;
			//eggSpawnRate = 0.25F;
			//eggSpawns.Add(BiomeType.GrandReef_TreaderPath);
		}

		public override void prepareGameObject(GameObject world, Renderer[] r0) {
			PurpleJellyrayTag kc = world.EnsureComponent<PurpleJellyrayTag>();
			foreach (Renderer r in r0) {
				r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
				RenderUtil.disableTransparency(r.materials[0]);
				r.materials[0].SetFloat("_EmissionLM", 0.1F);
				r.materials[0].SetFloat("_EmissionLMNight", 0.1F);
			}
		}

		public override BehaviourType getBehavior() {
			return BehaviourType.MediumFish;
		}

	}

	class PurpleJellyrayTag : MonoBehaviour {

		private Renderer[] renders;

		private float lastEyeFlameCheckTime = -1;
		private float lastEyeFlameEatTime = DayNightCycle.main.timePassedAsFloat-UnityEngine.Random.Range(0, 1200);

		private SanctuaryPlantTag currentTarget;

		private SwimRandom swimmer;

		void Update() {
			if (renders == null)
				renders = this.GetComponentsInChildren<Renderer>();
			if (!swimmer)
				swimmer = this.GetComponentInChildren<SwimRandom>();

			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastEyeFlameCheckTime >= 10 && time - lastEyeFlameEatTime >= 600 && !this.GetComponent<WaterParkCreature>()) { //10 min each
				lastEyeFlameCheckTime = time;
				WorldUtil.getObjectsNear<SanctuaryPlantTag>(transform.position, 180, this.tryTarget, go => go.GetComponent<SanctuaryPlantTag>());
			}

			if (currentTarget && swimmer) {
				float dist = (currentTarget.transform.position-transform.position).sqrMagnitude;
				if (dist < 5) {
					if (currentTarget.tryHarvest()) { //does not spawn item
						lastEyeFlameEatTime = time;
						SoundManager.playSoundAt(SoundManager.buildSound(CraftData.GetPickupSound(TechType.SeaTreaderPoop)), transform.position);
						Jellyray jr = this.GetComponent<Jellyray>();
						jr.Hunger.Value = 0;
						jr.Happy.Add(0.5F);
						currentTarget = null;
					}
				}
				else {
					swimmer.swimBehaviour.SwimTo(currentTarget.transform.position, 2.5F);
				}
			}
		}

		private bool tryTarget(SanctuaryPlantTag sp) {
			if (!currentTarget || (sp.transform.position - transform.position).sqrMagnitude < (currentTarget.transform.position - transform.position).sqrMagnitude)
				currentTarget = sp;
			return false;
		}

	}
}
