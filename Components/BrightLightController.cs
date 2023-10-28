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
	internal class BrightLightController : MonoBehaviour {
		
		private MonoBehaviour vehicle;
		private Ecocean.ECHooks.ECMoth ecoceanComponent;
		private CyclopsLightingPanel cyclopsControl;
			
		private List<Light> bonusLights = new List<Light>();
		
		private bool hasModule;
		
		private float energyConsumptionBaseline;
		private float energyConsumptionBrights;
		
		private float lightRange;
		private float lightRangeBrights;
		private float lightIntensity;
		private float lightIntensityBrights;
		private float lightAngle;
		
		private float ecLightIntensity = 1;
		
		internal BrightLightController setPowerValues(float baseline, float bright) {
			energyConsumptionBaseline = baseline;
			energyConsumptionBrights = bright;
			return this;
		}
		
		internal BrightLightController setLightValues(float range, float intensity, float angle, float range2, float intensity2) {
			lightRange = range;
			lightIntensity = intensity;
			lightAngle = angle;
			lightRangeBrights = range2;
			lightIntensityBrights = intensity2;
			return this;
		}
			
		internal Light createBonusLight(GameObject orig) {
			if (orig.name.Contains("BonusLight"))
				return null;
			if (!orig.GetComponent<Light>())
				return null;
			GameObject go = UnityEngine.Object.Instantiate(orig);
			go.name += "BonusLight";
			go.transform.SetParent(orig.transform.parent);
			go.transform.position = orig.transform.position;
			go.transform.rotation = orig.transform.rotation;
			go.transform.localScale = orig.transform.localScale;
			Light l = go.GetComponent<Light>();
			l.color = Color.Lerp(l.color, Color.white, 0.5F);//new Color(0.75F, 0.95F, 1);
			l.range = lightRange;
			l.intensity = lightIntensity;
			l.spotAngle = lightAngle;
			l.innerSpotAngle = lightAngle*0.5F;
			ObjectUtil.removeChildObject(go, "x_FakeVolumletricLight");
			return l;
		}
		
		void Update() {
			if (!vehicle) {
				vehicle = GetComponent<Vehicle>();
				if (!vehicle)
					vehicle = GetComponent<SubRoot>();
				if (vehicle is BaseRoot) {
					UnityEngine.Object.Destroy(this);
					return;
				}
			}

			bool cyclops = vehicle is SubRoot;
			if (cyclops && !cyclopsControl)
				cyclopsControl = GetComponentInChildren<CyclopsLightingPanel>();
			if (vehicle) {
				if (bonusLights.Count == 0) {
					rebuildLights(cyclops);
				}
				if (!cyclops && !ecoceanComponent) {
					ecoceanComponent = GetComponent<Ecocean.ECHooks.ECMoth>();
					if (ecoceanComponent)
						ecoceanComponent.getLightIntensity = () => ecLightIntensity;
				}
			}
			else {
				return;
			}
			
			bool flag1 = hasModule && areLightsOn();
			bool flag2 = flag1 && (cyclops || InventoryUtil.isVehicleUpgradeSelected((Vehicle)vehicle, C2CItems.lightModule.TechType));
			foreach (Light l in bonusLights) {
				if (l) {
					l.gameObject.SetActive(flag1);
					l.intensity = flag2 ? lightIntensityBrights : lightIntensity;
					l.range = flag2 ? lightRangeBrights : lightRange;
				}
				else {
					rebuildLights(cyclops);
					return;
				}
			}
			
			ecLightIntensity = flag1 ? (flag2 ? 4 : 2.5F) : 1;
			
			if (flag1) {
				float amt = Time.deltaTime*(flag2 ? energyConsumptionBrights : energyConsumptionBaseline);
				if (cyclops) {
					float trash;
					((SubRoot)vehicle).powerRelay.ConsumeEnergy(amt, out trash);
				}
				else {
					((Vehicle)vehicle).ConsumeEnergy(amt);
				}
			}
		}
		
		private void rebuildLights(bool cyclops) {
			foreach (Light l in bonusLights) {
				if (l)
					UnityEngine.Object.Destroy(l.gameObject);
			}
			bonusLights.Clear();
			GameObject go = ObjectUtil.getChildObject(gameObject, cyclops ? "Floodlights" : "lights_parent");
			if (!go) {
				SNUtil.writeToChat("Could not find light parent on "+gameObject+"="+vehicle);
				return;
			}
			foreach (Transform t in go.transform) {
				if (!t)
					continue;
				Light l = createBonusLight(t.gameObject);
				if (l)
					bonusLights.Add(l);
			}
		}
		
		public void recalculateModule() {
			if (!vehicle) {
				Invoke("recalculateModule", 0.5F);
				return;
			}
			hasModule = vehicle is SubRoot ? InventoryUtil.cyclopsHasUpgrade((SubRoot)vehicle, C2CItems.lightModule.TechType) : InventoryUtil.vehicleHasUpgrade((Vehicle)vehicle, C2CItems.lightModule.TechType);
		}
		
		bool areLightsOn() {
			if (vehicle is SeaMoth)
				return ((SeaMoth)vehicle).lightsActive;
			if (vehicle is SubRoot)
				return cyclopsControl && cyclopsControl.floodlightsOn;
			return true;
		}
		
	}
}
