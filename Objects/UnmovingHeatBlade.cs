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
	
	public class UnmovingHeatBlade : PickedUpAsOtherItem {
	        
	    internal UnmovingHeatBlade() : base("UnmovingHeatBlade", TechType.HeatBlade) {
			
	    }
			
	    protected override void prepareGameObject(GameObject go) {
			go.GetComponent<Rigidbody>().isKinematic = true;
			go.EnsureComponent<UnmovingHeatBladeTag>();
			Light l = ObjectUtil.addLight(go);
			l.intensity = 0.8F;
			l.range = 2.4F;
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
				body = GetComponent<Rigidbody>();
			}
			if (!light) {
				light = GetComponentInChildren<Light>();
				float num = 0.5F + 0.5F * Mathf.Sin(DayNightCycle.main.timePassedAsFloat * 3.417F);
				light.color = new Color(1, Mathf.Lerp(70F, 140F, num) / 255F, 0);
				light.intensity = Mathf.Lerp(0.45F, 0.67F, 1F - num);
			}
			body.isKinematic = true;
		}
		
	}
}
