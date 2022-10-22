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
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class EjectedHeatSink : Spawnable {
		
		internal static readonly float HEAT_RADIUS = 20;
		internal static readonly float MAX_TEMPERATURE = 600;
	        
	    internal EjectedHeatSink(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("bcb52360-f014-4ca1-9cf2-9e32504c645f");
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			//world.GetComponent<Rigidbody>().isKinematic = true;
			WorldForces wf = world.GetComponent<WorldForces>();
			wf.underwaterGravity = 0F;
			wf.underwaterDrag *= 2.5F;
			ObjectUtil.removeComponent<Pickupable>(world);
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
			HeatSinkTag g = world.EnsureComponent<HeatSinkTag>();
			Light l = ObjectUtil.addLight(world);
			l.bounceIntensity *= 3;
			l.color = new Color(1F, 1F, 0.75F, 1F);
			l.intensity = 3;
			l.range = 40;
			Renderer r = world.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/HeatSink");
			r.materials[0].SetFloat("_SpecInt", 50);
			r.materials[0].SetFloat("_Shininess", 0);
			r.materials[0].SetFloat("_Fresnel", 1);
			r.materials[0].SetColor("_Color", new Color(1, 1, 1, 1));
			return world;
	    }
			
	}
		
	class HeatSinkTag : MonoBehaviour {
		
		private Light light;
		
		private Renderer mainRender;
		private PrefabIdentifier prefab;
		private Rigidbody mainBody;
		
		private float temperature;
		
		private static readonly Color glowNew = new Color(1F, 1F, 0.75F, 1F);
		private static readonly Color glowFinal = new Color(0.67F, 0.15F, 0.12F, 1);
		
		private float lastPLayerDistanceCheckTime;
		
		private float spawnTime;
		
		void Update() {
			if (!mainRender)
				mainRender = GetComponentInChildren<Renderer>();
			if (!mainBody)
				mainBody = GetComponentInChildren<Rigidbody>();
			if (!prefab)
				prefab = GetComponentInChildren<PrefabIdentifier>();
			if (!light)
				light = GetComponentInChildren<Light>();
			
			transform.localScale = Vector3.one*2.5F;
			
			float time = DayNightCycle.main.timePassedAsFloat;
			
			temperature = Mathf.Max(0, temperature-Time.deltaTime*40);
			
			if (time-lastPLayerDistanceCheckTime >= 0.5) {
				lastPLayerDistanceCheckTime = time;
				if (Vector3.Distance(transform.position, Player.main.transform.position) > 250) {
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
			}
			
			float f = getIntensity();
			if (light)
				light.intensity = UnityEngine.Random.Range(2.8F, 3.2F)*f;
			RenderUtil.setEmissivity(mainRender.materials[0], (0.25F+0.75F*f)*90, "GlowStrength");
			mainRender.materials[0].SetColor("_GlowColor", getColor(f));
		}
		
		internal Color getColor(float f) {
			return Color.Lerp(glowNew, glowFinal, 1-f);
		}
		
		internal void onFired() {
			temperature = EjectedHeatSink.MAX_TEMPERATURE;
			spawnTime = DayNightCycle.main.timePassedAsFloat;
		}
		
		public float getTemperature() {
			return temperature;
		}
		
		internal float getIntensity() {
			return temperature/EjectedHeatSink.MAX_TEMPERATURE;
		}
		
	}
}
