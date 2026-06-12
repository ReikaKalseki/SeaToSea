using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using FCS_AlterraHub.Objects;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	internal class C2CBrainCoral : MonoBehaviour {

		private bool isInCave;

		private IntermittentInstantiate spawner;

		private GameObject fxHolder;
		private GameObject collider;

		void Start() {
			InvokeRepeating("tickCheck", 0f, 1.0F);
			//InvokeRepeating("tickSpawn", 0f, 0.05F);
			spawner = GetComponent<IntermittentInstantiate>();
		}

		private void OnKill() {
			this.destroy(false);
		}

		void OnDisable() {
			CancelInvoke("tickCheck");
			CancelInvoke("tickSpawn");
		}

		internal void tickCheck() {
			string biome = WaterBiomeManager.main.GetBiome(transform.position);
			bool cave = BiomeBase.getBiome(biome).isCaveBiome() || biome.ToLowerInvariant().Contains("cave");
			if (cave != isInCave) {
				if (cave) {
					foreach (Renderer r in this.GetComponentsInChildren<Renderer>()) {
						RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Plants/CaveBrainCoral");
					}
					GetComponent<TechTag>().type = C2CItems.caveBrainCoral;
					//ObjectUtil.removeComponent<IntermittentInstantiate>(gameObject);
					GetComponent<IntermittentInstantiate>().enabled = false;
					string n = "C2C Brain Coral FX Holder";
					if (!fxHolder)
						fxHolder = ObjectUtil.getChildObject(gameObject, n);
					if (!fxHolder) {
						fxHolder = new GameObject(n);
						fxHolder.transform.SetParent(transform);
						Utils.ZeroTransform(fxHolder.transform);
						fxHolder.transform.localPosition = new Vector3(0.1F, -0.2F, 1.2F);
						DynamicBubbler bubbler = fxHolder.EnsureComponent<DynamicBubbler>().setBubbleCount(12);
						bubbler.scatter = new Vector3(0.8F, 0.3F, 0.8F);
						bubbler.shape = DynamicBubbler.Shape.UPPERHEMI;
						bubbler.currentIntensity = 1;
					}
					fxHolder.SetActive(true);

					n = "C2C Brain Coral Air Box";
					if (!collider)
						collider = ObjectUtil.getChildObject(gameObject, n);
					if (!collider) {
						collider = new GameObject(n);
						collider.transform.SetParent(transform);
						Utils.ZeroTransform(collider.transform);
						collider.transform.localPosition = new Vector3(0.0F, 0.0F, 0.5F);
						CapsuleCollider cc = collider.EnsureComponent<CapsuleCollider>();
						cc.radius = 0.625F;
						cc.height = 2.5F;
						cc.direction = 2;
						cc.isTrigger = true;
						collider.EnsureComponent<CaveBrainCoralAirBox>();
						collider.EnsureComponent<OxygenArea>().oxygenPerSecond = 5F;
					}
					collider.SetActive(true);
				}
				else {
					GetComponent<TechTag>().type = TechType.BrainCoral;
					GetComponent<IntermittentInstantiate>().enabled = true;
					if (fxHolder)
						fxHolder.SetActive(false);
					if (collider)
						collider.SetActive(false);
				}
			}
			isInCave = cave;
		}

		internal void tickSpawn() {
			if (isInCave && spawner && spawner.prefab) {
				for (int i = 0; i < 9; i++) {
					Vector3 pos = UnityEngine.Random.onUnitSphere;
					pos.y = Mathf.Abs(pos.y * 0.67F);
					pos = transform.position + pos;
					GameObject go = UWE.Utils.InstantiateDeactivated(spawner.prefab, pos, Quaternion.identity);
					go.GetComponentInChildren<Renderer>().transform.localScale = Vector3.one * 0.03F;
					go.EnsureComponent<CaveBrainCoralBubble>();
					go.GetComponent<Bubble>().oxygenSeconds = 0.5F;
					go.SetActive(true);
				}
			}
		}

	}

	internal class CaveBrainCoralAirBox : MonoBehaviour {

		void OnTriggerStay(Collider other) {
			if (other.isPlayer(true)) {
				C2CHooks.lastCaveBrainCoralBubbleTouch = DayNightCycle.main.timePassedAsFloat;
			}
		}

	}

	internal class CaveBrainCoralBubble : MonoBehaviour {

	}
}
