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

	public class UnmovingHeatBlade : PickedUpAsOtherItem {

		internal UnmovingHeatBlade() : base("UnmovingHeatBlade", TechType.HeatBlade) {

		}

		protected override void prepareGameObject(GameObject go) {
			go.GetComponent<Rigidbody>().isKinematic = true;
			go.EnsureComponent<UnmovingHeatBladeTag>();
			Light l = go.addLight(0.8F, 2.4F);
			l.transform.localPosition = new Vector3(0, 0, 0.2F);
			l.lightShadowCasterMode = LightShadowCasterMode.Everything;
			l.shadows = LightShadows.Soft;
		}

	}

	class UnmovingHeatBladeTag : MonoBehaviour {

		private Rigidbody body;
		private Light light;

		void Update() {
			if (!body) {
				body = this.GetComponent<Rigidbody>();
			}
			if (!light) {
				light = this.GetComponentInChildren<Light>();
				float num = 0.5F + (0.5F * Mathf.Sin(DayNightCycle.main.timePassedAsFloat * 3.417F));
				light.color = new Color(1, Mathf.Lerp(70F, 140F, num) / 255F, 0);
				light.intensity = Mathf.Lerp(0.45F, 0.67F, 1F - num);
			}
			body.isKinematic = true;
		}

	}
}
