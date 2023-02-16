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
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class SeaTreaderTunnelLight : PickedUpAsOtherItem {
		
		public static readonly float BASE_RANGE = 30;
		public static readonly float BASE_BRIGHTNESS = 1.0F;
	        
	    internal SeaTreaderTunnelLight() : base("SeaTreaderTunnelLight", "67744b32-93c2-4aba-8a18-ffb87204a8eb") {
			
	    }

		protected override void prepareGameObject(GameObject go) {
			Light l = go.GetComponentInChildren<Light>();
			l.range = BASE_RANGE;
			l.intensity = BASE_BRIGHTNESS;
			go.GetComponentInChildren<Rigidbody>().isKinematic = true;
			go.EnsureComponent<SeaTreaderTunnelLightTag>();
		}
			
	}
		
	class SeaTreaderTunnelLightTag : MonoBehaviour { //use to make light flicker to be more obvious
		
		private Light light;
		private Rigidbody body;
		
		private float intensityFactor = 1;
		private float targetIntensity = 1;
		
		private float shiftSpeed = 2F;
		
		void Update() {
			if (!light) {
				light = GetComponentInChildren<Light>();
			}
			if (!body) {
				body = GetComponentInChildren<Rigidbody>();
			}
			body.isKinematic = true;
			if (light) {
				float dT = Time.deltaTime;
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
				else if (targetIntensity > intensityFactor) {
					intensityFactor = Mathf.Min(intensityFactor+shiftSpeed*dT, targetIntensity);
				}
				else {
					intensityFactor = Mathf.Max(intensityFactor-shiftSpeed*dT, targetIntensity);
				}
				float f = UnityEngine.Random.Range(0.95F, 1.05F);
				light.intensity = SeaTreaderTunnelLight.BASE_BRIGHTNESS*intensityFactor*f;
				light.range = SeaTreaderTunnelLight.BASE_RANGE*intensityFactor*f;
			}
		}
		
	}
}
