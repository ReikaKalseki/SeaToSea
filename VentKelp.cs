using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class VentKelp : BasicCustomPlant {
		
		public VentKelp() : base(SeaToSeaMod.itemLocale.getEntry("VENT_KELP"), VanillaFlora.CREEPVINE, "Samples") {
			glowIntensity = 0.8F;
			finalCutBonus = 2;
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(1, 1);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<GlowKelpTag>().renderer = r;
			foreach (Material m in r.materials) {/*
				m.SetColor("_GlowColor", );
				m.SetVector("_Scale", new Vector4(0.35F, 0.2F, 0.1F, 0.0F));
				m.SetVector("_Frequency", new Vector4(1.2F, 0.5F, 1.5F, 0.5F));
				m.SetVector("_Speed", new Vector4(0.2F, 0.5F, 1.5F, 0.5F));
				m.SetVector("_ObjectUp", new Vector4(1F, 1F, 1F, 1F));
				m.SetFloat("_WaveUpMin", 0F);*/
			}
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
	}
	
	class GlowKelpTag : MonoBehaviour {
		
		private static readonly Color idleColor = new Color(0.1F, 0, 0.5F, 1);
		private static readonly Color activeColor = new Color(0.7F, 0.2F, 1, 1);
		
		internal Renderer renderer;
		
		private Geyser closestGeyser = null;
		
		private float activity = 0;
		
		void Start() {
			foreach (Geyser g in UnityEngine.Object.FindObjectsOfType<Geyser>()) {
				float dist = Vector3.Distance(g.gameObject.transform.position, gameObject.transform.position);
				if (closestGeyser == null || Vector3.Distance(closestGeyser.gameObject.transform.position, gameObject.transform.position) > dist) {
					closestGeyser = g;
				}
			}
			if (renderer == null)
				renderer = gameObject.GetComponentInChildren<Renderer>();
			if (gameObject.GetComponent<GrownPlant>() != null) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = new Vector3(1, 0.25F, 1);
    		}
    		else {
    			gameObject.transform.localScale = new Vector3(2, 0.5F, 2);
				gameObject.transform.rotation = Quaternion.identity;
    		}
		}
		
		void Update() {/*
			EcoRegionManager ecoRegionManager = EcoRegionManager.main;
			if (ecoRegionManager != null) {
				Vector3 wsPos = gameObject.transform.position;
				IEcoTarget ecoTarget = ecoRegionManager.FindNearestTarget(EcoTargetType.HeatArea, wsPos, null, 3);
				if (ecoTarget != null) {
					float dist = Vector3.Distance(ecoTarget.GetPosition(), wsPos);
					if (dist < 32) {
						
					}
				}
			}*/
			if (closestGeyser != null && closestGeyser.erupting) {
				activity = Math.Min(1, activity+0.02F);
			}
			else {
				activity = Math.Max(0, activity-0.05F);
			}
			
			if (renderer != null) {
				foreach (Material m in renderer.materials) {
					m.SetColor("_GlowColor", Color.Lerp(idleColor, activeColor, activity));
				}
			}
		}
		
	}
}
