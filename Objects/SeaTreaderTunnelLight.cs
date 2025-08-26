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

	public class SeaTreaderTunnelLight : PickedUpAsOtherItem {

		public static readonly float BASE_RANGE = 30;
		public static readonly float BASE_BRIGHTNESS = 1.0F;

		public static readonly float MTN_RANGE = 18;
		public static readonly float MTN_BRIGHTNESS = 0.8F;

		public static readonly float BASE_EMISSIVE_DAY = 1.5F;
		public static readonly float BASE_EMISSIVE_NIGHT = 2F;

		internal SeaTreaderTunnelLight() : base("SeaTreaderTunnelLight", "67744b32-93c2-4aba-8a18-ffb87204a8eb") {

		}

		protected override void prepareGameObject(GameObject go) {
			Light l = go.GetComponentInChildren<Light>();
			l.range = BASE_RANGE;
			l.intensity = BASE_BRIGHTNESS;
			go.GetComponentInChildren<Rigidbody>().isKinematic = true;
			go.EnsureComponent<SeaTreaderTunnelLightTag>();
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
		}

	}

	class SeaTreaderTunnelLightTag : MonoBehaviour { //use to make light flicker to be more obvious

		private Light light;
		private Rigidbody body;
		private Renderer[] renders;

		private float intensityFactor = 1;
		private float targetIntensity = 1;

		private float shiftSpeed = 2F;

		void Update() {
			if (!light) {
				light = this.GetComponentInChildren<Light>();
			}
			if (!body) {
				body = this.GetComponentInChildren<Rigidbody>();
			}
			if (renders == null) {
				renders = this.GetComponentsInChildren<Renderer>();
			}
			body.isKinematic = true;
			if (light) {
				float dT = Time.deltaTime;
				if (dT <= 0.01F)
					return;
				if (Mathf.Approximately(intensityFactor, targetIntensity)) {
					targetIntensity = UnityEngine.Random.Range(0.8F, 1.05F);
					shiftSpeed = UnityEngine.Random.Range(1.5F, 2.5F);
					if (UnityEngine.Random.Range(0, 6) == 0) {
						targetIntensity = UnityEngine.Random.Range(0.2F, 0.5F);
						shiftSpeed = 5;
					}
					else if (intensityFactor <= 0.5) {
						shiftSpeed = 3.6F;
					}
				}
				else {
					intensityFactor = targetIntensity > intensityFactor
						? Mathf.Min(intensityFactor + (shiftSpeed * dT), targetIntensity)
						: Mathf.Max(intensityFactor - (shiftSpeed * dT), targetIntensity);
				}
				float f = UnityEngine.Random.Range(0.95F, 1.05F);
				bool mtn = transform.position.y >= -250 && transform.position.x > 0 && transform.position.z > 0;
				light.intensity = (mtn ? SeaTreaderTunnelLight.MTN_BRIGHTNESS : SeaTreaderTunnelLight.BASE_BRIGHTNESS) * intensityFactor * f;
				light.range = (mtn ? SeaTreaderTunnelLight.MTN_RANGE : SeaTreaderTunnelLight.BASE_RANGE) * intensityFactor * f;
				float f2 = Mathf.Pow(intensityFactor*f, 0.75F);
				foreach (Renderer r in renders) {
					r.materials[0].SetFloat("_GlowStrength", SeaTreaderTunnelLight.BASE_EMISSIVE_DAY * f2);
					r.materials[0].SetFloat("_GlowStrengthNight", SeaTreaderTunnelLight.BASE_EMISSIVE_NIGHT * f2);
				}
			}
		}

	}
}
