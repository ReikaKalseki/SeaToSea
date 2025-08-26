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

	public class BloodKelpBaseNuclearReactorMelter : Spawnable {

		internal BloodKelpBaseNuclearReactorMelter() : base("BloodKelpBaseNuclearReactorMelter", "", "") {

		}

		public override GameObject GetGameObject() {
			GameObject go = new GameObject();
			go.EnsureComponent<BloodKelpBaseNuclearReactorMelterTag>();
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
			return go;
		}

		class BloodKelpBaseNuclearReactorMelterTag : MonoBehaviour {

			private bool triggered = false;

			void Update() {
				if (triggered)
					return;
				BaseNuclearReactorGeometry go = WorldUtil.getClosest<BaseNuclearReactorGeometry>(C2CHooks.bkelpBaseNuclearReactor);
				if (go && Vector3.Distance(go.transform.position, C2CHooks.bkelpBaseNuclearReactor) < 5F) {
					/*
		    		LeakingRadiation lr = go.EnsureComponent<LeakingRadiation>();
		    		lr.leaks = new List<RadiationLeak>();
		    		lr.radiationFixed = false;
		    		lr.kGrowRate = 0;
		    		lr.kNaturalDissipation = 0;
		    		lr.kStartRadius = lr.kMaxRadius = lr.currentRadius = 9;
		    		lr.damagePlayerInRadius = go.EnsureComponent<DamagePlayerInRadius>();
		    		lr.damagePlayerInRadius.damageType = DamageType.Radiation;
		    		lr.damagePlayerInRadius.damageAmount = 3;
		    		lr.radiatePlayerInRange = go.EnsureComponent<RadiatePlayerInRange>();
		    		*/
					go.gameObject.EnsureComponent<BloodKelpBaseNuclearReactorGlower>();
					triggered = true;
				}
			}

		}

		class BloodKelpBaseNuclearReactorGlower : MonoBehaviour {

			private bool textured;
			private Text text;

			private readonly List<ParticleSystem> bubbles = new List<ParticleSystem>();

			private void Update() {
				if (!textured) {
					textured = true;
					foreach (Renderer r in this.GetComponentsInChildren<Renderer>())
						RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/bkelpreactor");
				}

				if (!text) {
					GameObject child = gameObject.getChildObject("UI/Canvas/Text");
					text = child.GetComponent<Text>();
				}
				text.text = "<color=#ff0000>OPERATOR ERROR\n\nMOLTEN CORE WARNING\nTEMP AT SPIKEVALUE \n999999999999999</color>";

				while (bubbles.Count < 11) {
					GameObject go = ObjectUtil.createWorldObject("0dbd3431-62cc-4dd2-82d5-7d60c71a9edf");
					go.transform.SetParent(transform);
					float y = UnityEngine.Random.Range(-0.2F, 1.2F);
					float r = 0.8F;
					if (y < 0.2)
						r += y * 0.33F;
					float ang = UnityEngine.Random.Range(0F, 360F)*Mathf.PI/180F;
					go.transform.localPosition = new Vector3(r * Mathf.Cos(ang), -y, r * Mathf.Sin(ang));
					go.transform.rotation = Quaternion.Euler(270, 0, 0); //not local - force to always be up
					ParticleSystem ps = go.GetComponent<ParticleSystem>();
					go.SetActive(true);
					bubbles.Add(ps);
					ps.Play();
				}
			}

		}

	}
}
