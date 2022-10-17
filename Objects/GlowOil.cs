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
	
	public class GlowOil : Spawnable {
		
		internal static readonly float MAX_GLOW = 4;
		
		private readonly XMLLocale.LocaleEntry locale;
		
		private static float lastPlayerLightCheck;
		private static float lastLightRaytrace;
		
		internal static readonly Simplex3DGenerator sizeNoise = new Simplex3DGenerator();
	        
	    internal GlowOil(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("505e7eff-46b3-4ad2-84e1-0fadb7be306c");
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			ResourceTracker rt = world.EnsureComponent<ResourceTracker>();
			rt.techType = TechType;
			rt.overrideTechType = TechType;
			//world.GetComponent<Rigidbody>().isKinematic = true;
			//world.GetComponent<WorldForces>().underwaterGravity = 0;
			ObjectUtil.removeComponent<EnzymeBall>();
			GlowOilTag g = world.EnsureComponent<GlowOilTag>();
			Light l = ObjectUtil.addLight(world);
			l.bounceIntensity *= 2;
			l.color = new Color(0.5F, 0.8F, 1F, 1F);
			l.intensity = 0;
			l.range = 10;
			Renderer r = world.GetComponentInChildren<Renderer>();
			return world;
	    }
		
		public void register() {
			Patch();
        	KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>(){TechType});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = TechType;
			e.locked = true;
			e.scanTime = 3;
			PDAHandler.AddCustomScannerEntry(e);
		}
		
		public static void checkPlayerLightTick(Player ep) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastPlayerLightCheck >= 0.25F) {
				lastPlayerLightCheck = time;
				PlayerTool pt = Inventory.main.GetHeldTool();
				if (pt && pt.energyMixin && pt.energyMixin.charge > 0) {
					if ((pt is Seaglide && ((Seaglide)pt).toggleLights.lightsActive) || (pt is FlashLight && ((FlashLight)pt).toggleLights.lightsActive)) {
						handleLightTick(MainCamera.camera.transform);
					}
				}
			}
		}
		
		public static void handleLightTick(Transform go) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastLightRaytrace >= 0.25F) {
				lastLightRaytrace = time;
				foreach (RaycastHit hit in Physics.SphereCastAll(go.position, 2.5F, go.forward, 180)) {
					if (hit.transform) {
						GlowOilTag g = hit.transform.GetComponentInParent<GlowOilTag>();
						if (g)
							g.onLit();
					}
				}
			}
		}
			
	}
		
	class GlowOilTag : MonoBehaviour {
		
		private Light light;
		private Renderer render;
		
		private float glowIntensity = 0;
		
		private float lastGlowUpdate;
		private float lastLitTime;
		
		void Update() {
			if (!render)
				render = GetComponentInChildren<Renderer>();
			if (!light)
				light = GetComponentInChildren<Light>();
			transform.localScale = Vector3.one*(1.5F+0.5F*(float)GlowOil.sizeNoise.getValue(transform.position));
			
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = time-lastGlowUpdate;
			if (dT >= 0.02F) {
				lastGlowUpdate = time;
				updateGlowStrength(time, dT);
			}
			
			light.intensity = glowIntensity*GlowOil.MAX_GLOW;
			RenderUtil.setEmissivity(render, glowIntensity*3.5F, "GlowStrength");
		}
		
		private void updateGlowStrength(float time, float dT) {
			float delta = time-lastLitTime < 1.5F ? 0.5F : -0.15F;
			glowIntensity = Mathf.Clamp01(glowIntensity+delta*dT);
		}
		
		internal void onLit() {
			SNUtil.writeToChat("Lit");
			lastLitTime = DayNightCycle.main.timePassedAsFloat;
		}
		
	}
}
