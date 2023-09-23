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
		
		internal static readonly float HEAT_RADIUS = 12;
		internal static readonly float MAX_TEMPERATURE = 600;

		internal static List<HeatSinkTag> activeHeatSinks = new List<HeatSinkTag>();
	        
	    internal EjectedHeatSink() : base("dumpedheatsink", "Ejected Heat Sink", "") {
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<HeatSinkTag>().addField("temperature").addField("spawnTime"));
			};
	    }

		internal static void iterateHeatSinks(Action<HeatSinkTag> a) {
			foreach (HeatSinkTag h in activeHeatSinks) {
				if (h) {
					a(h);
				}
			}
		}
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("bcb52360-f014-4ca1-9cf2-9e32504c645f");
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			Rigidbody rb = world.GetComponent<Rigidbody>();
			rb.mass *= 9;
			WorldForces wf = world.GetComponent<WorldForces>();
			wf.underwaterGravity = 0.2F;
			wf.underwaterDrag *= 0.33F;
			ObjectUtil.removeComponent<Pickupable>(world);
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
			HeatSinkTag g = world.EnsureComponent<HeatSinkTag>();
			Light l = ObjectUtil.addLight(world);
			l.color = new Color(1F, 1F, 0.75F, 1F);
			l.intensity = 1.5F;
			l.range = 40;
			Renderer r = world.GetComponentInChildren<Renderer>();
			setTexture(r);
			return world;
	    }
		
		internal static void setTexture(Renderer r) {
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/HeatSink");
			r.materials[0].SetFloat("_SpecInt", 20);
			r.materials[0].SetFloat("_Shininess", 7);
			r.materials[0].SetFloat("_Fresnel", 0.5F);
			r.materials[0].SetColor("_Color", new Color(1, 1, 1, 1));
		}
			
	}
		
	internal class HeatSinkTag : MonoBehaviour {
		
		private static readonly SoundManager.SoundData fireSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "fireheatsink", "Sounds/fireheatsink.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 40);}, SoundSystem.masterBus);
		
		private Light light;
		
		private Renderer mainRender;
		private PrefabIdentifier prefab;
		private Rigidbody mainBody;
		
		private float temperature;
		
		private DynamicBubbler bubbler;
		
		private static readonly Color glowNew = new Color(1F, 1F, 0.75F, 1F);
		private static readonly Color glowMid = new Color(1F, 0.4F, 0.25F, 1);
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
			if (!bubbler)
				bubbler = gameObject.EnsureComponent<DynamicBubbler>().setBubbleCount(4);
			
			transform.localScale = Vector3.one*1.5F;
			
			float time = DayNightCycle.main.timePassedAsFloat;
			
			temperature = Mathf.Max(0, temperature-Time.deltaTime*Mathf.Clamp(temperature/20F, 1, 20));
			
			if (time-lastPLayerDistanceCheckTime >= 0.5) {
				lastPLayerDistanceCheckTime = time;
				if (Vector3.Distance(transform.position, Player.main.transform.position) > Mathf.Clamp(temperature*2, 50, 250)) {
					UnityEngine.Object.DestroyImmediate(gameObject);
					EjectedHeatSink.activeHeatSinks.Remove(this);
				}
			}
			
			float f = getIntensity();
			bubbler.currentIntensity = f;
			if (light) {
				light.intensity = UnityEngine.Random.Range(1.45F, 1.55F)*f;
				light.color = getColor(f);
			}
			RenderUtil.setEmissivity(mainRender.materials[0], (0.67F+0.33F*f)*4F);
			mainRender.materials[0].SetColor("_GlowColor", getColor(f));
		}
		
		void OnDestroy() {
			EjectedHeatSink.activeHeatSinks.Remove(this);
		}
		
		void OnDisable() {
			EjectedHeatSink.activeHeatSinks.Remove(this);
		}
		
		internal Color getColor(float f) {
			if (f < 0.125F) {
				return Color.Lerp(Color.black, glowFinal, f*8);
			}
			else if (f < 0.5F) {
				f = (f-0.125F)/0.375F;
				return Color.Lerp(glowFinal, glowMid, f);
			}
			else {
				f = (f-0.5F)*2;
				return Color.Lerp(glowMid, glowNew, f);
			}
		}
		
		internal void onFired(float intensity) {
			temperature = EjectedHeatSink.MAX_TEMPERATURE*intensity;
			spawnTime = DayNightCycle.main.timePassedAsFloat;
			SoundManager.playSoundAt(fireSound, transform.position, false, 40, 2);
			EjectedHeatSink.activeHeatSinks.Add(this);
		}
		
		public float getTemperature() {
			return temperature;
		}
		
		internal float getIntensity() {
			return Mathf.Clamp01(temperature/EjectedHeatSink.MAX_TEMPERATURE);
		}
		
	}
}
