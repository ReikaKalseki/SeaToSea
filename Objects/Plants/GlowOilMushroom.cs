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
	
	public class GlowOilMushroom : BasicCustomPlant, MultiTexturePrefab<VanillaFlora> {
		
		public GlowOilMushroom() : base(SeaToSeaMod.itemLocale.getEntry("GLOWSHROOM"), VanillaFlora.JELLYSHROOM_LIVE, "7fcf1275-0687-491e-a086-d928dd3ba67a") {
			glowIntensity = 1.5F;
			finalCutBonus = 1;
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<GlowShroomTag>();
			ObjectUtil.removeComponent<CrabsnakeMushroom>(go);
			ObjectUtil.removeComponent<PrefabPlaceholder>(go);
			ObjectUtil.removeComponent<PrefabPlaceholdersGroup>(go);
			ObjectUtil.removeComponent<EcoTarget>(go);
			ObjectUtil.removeChildObject(go, "CrabsnakeSpawnPoint");
			ObjectUtil.removeChildObject(go, "Jellyshroom_Loot_InsideShroom");
			ObjectUtil.removeChildObject(go, "Jellyshroom_Creature_CrabSnake");
			if (!go.GetComponentInChildren<Light>()) {
				Light l = ObjectUtil.addLight(go);
				l.color = new Color(0.4F, 0.7F, 1F, 1F);
				l.range = 30F;
				l.intensity = 2;
				l.gameObject.transform.localPosition = Vector3.up*5;
			}
		}
		
		public Dictionary<int, string> getTextureLayers(Renderer r) {
			bool hasGlow = r.materials.Length > 1;
			string N = r.material.name;
			N = N.Substring(N.LastIndexOf('_')+1);
			return hasGlow ? new Dictionary<int, string>{{0, "Trunk_"+N}, {1, "Cap_"+N}} : new Dictionary<int, string>{{0, "Inner_"+N}};
		}

		public string getTextureFolder() {
			return Path.Combine(base.getTextureFolder(), "GlowOilMushroom");
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return 0.1F;
		}
		
		public override Plantable.PlantSize getSize() {
			return Plantable.PlantSize.Large;
		}
		/*
		public override float getGrowthTime() {
			return 6000; //5x
		}*/
		
	}
	
	class GlowShroomTag : MonoBehaviour {
		
		private static readonly SoundManager.SoundData fireSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "glowshroomfire", "Sounds/glowshroom-fire.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 40);}, SoundSystem.masterBus);
		
		private Renderer[] renderers;
		private Light[] lights;
		
		private bool isGrown;
		
		private float lastEmitTime;
		private float nextEmitTime;
		
		void Start() {
			isGrown = gameObject.GetComponent<GrownPlant>() != null;
    		if (gameObject.transform.position.y > -200)
    			UnityEngine.Object.Destroy(gameObject);
    		else if (isGrown) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.1F, 0.125F);
    		}
    		else {
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.75F, 1F);
    		}
		}
		
		void Update() {
			if (renderers == null) {
				renderers = GetComponentsInChildren<Renderer>();
			}
			if (lights == null) {
				lights = GetComponentsInChildren<Light>();
				foreach (Light l in lights) {
					l.color = new Color(0.4F, 0.7F, 1F, 1F);
					l.range *= 0.75F;
					l.intensity = 2;
				}
			}
			
			if (isGrown) {
				setBrightness(0.75F);
			}
			else {
				float time = DayNightCycle.main.timePassedAsFloat;
				float dT = nextEmitTime-time;
				if (dT <= 0) {
					emit(time);
				}
				else {
					float dT2 = time-lastEmitTime;
					if (dT <= 3)
						setBrightness(1-dT/3F);
					else if (dT2 <= 1)
						setBrightness(1-dT2);
					else
						setBrightness(0);
				}
			}
		}
		
		private void setBrightness(float f) {
			foreach (Light l in lights) {
				l.intensity = 2*f;
			}
			foreach (Renderer r in renderers) {
				if (r.materials.Length > 1)
					RenderUtil.setEmissivity(r.materials[1], 0.5F+f*2.5F, "GlowStrength");
			}
		}
		
		private void emit(float time) {
			lastEmitTime = time;
			nextEmitTime = time+UnityEngine.Random.Range(30, 120F);
			GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.glowOil.ClassID);
			ObjectUtil.ignoreCollisions(go, gameObject);
			go.transform.position = transform.position;
			Rigidbody rb = go.GetComponent<Rigidbody>();
			rb.isKinematic = false;
			rb.angularVelocity = MathUtil.getRandomVectorAround(Vector3.zero, 15);
			Vector3 vec = MathUtil.getRandomVectorAround(transform.up.normalized*20, 0.5F);
			rb.AddForce(vec, ForceMode.VelocityChange);
			SoundManager.playSoundAt(fireSound, transform.position, false, 40);
			//rb.drag = go.GetComponent<WorldForces>().underwaterDrag;
		}
		
	}
}
