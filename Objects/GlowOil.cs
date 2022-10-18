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
		
		internal static readonly float MAX_GLOW = 2;
		internal static readonly float MAX_RADIUS = 18;
		
		private readonly XMLLocale.LocaleEntry locale;
		
		private static float lastPlayerLightCheck;
		private static float lastLightRaytrace;
		
		internal static readonly Simplex3DGenerator sizeNoise = (Simplex3DGenerator)new Simplex3DGenerator(0).setFrequency(0.4);
	        
	    internal GlowOil(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
	    }
		
		public override Vector2int SizeInInventory {
			get { return new Vector2int(1, 1); }
		}

		protected sealed override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/GlowOil");
		}
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("18229b4b-3ed3-4b35-ae30-43b1c31a6d8d"); //enzyme 42: "505e7eff-46b3-4ad2-84e1-0fadb7be306c"
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			ResourceTracker rt = world.EnsureComponent<ResourceTracker>();
			rt.techType = TechType;
			rt.overrideTechType = TechType;
			world.GetComponentInChildren<PickPrefab>().pickTech = TechType;
			//world.GetComponent<Rigidbody>().isKinematic = true;
			WorldForces wf = world.GetComponent<WorldForces>();
			wf.underwaterGravity = 0;
			wf.underwaterDrag *= 1;
			Rigidbody rb = world.GetComponent<Rigidbody>();
			rb.angularDrag *= 3;
			rb.maxAngularVelocity = 6;
			rb.drag = wf.underwaterDrag;
			//ObjectUtil.removeComponent<EnzymeBall>(world);
			ObjectUtil.removeComponent<Plantable>(world);
			GlowOilTag g = world.EnsureComponent<GlowOilTag>();
			Light l = ObjectUtil.addLight(world);
			l.bounceIntensity *= 2;
			l.color = new Color(0.5F, 0.8F, 1F, 1F);
			l.intensity = 0;
			l.range = MAX_RADIUS;
			Renderer r = world.GetComponentInChildren<Renderer>();
			RenderUtil.setEmissivity(r.materials[0], 0, "GlowStrength");
			RenderUtil.setEmissivity(r.materials[1], 0, "GlowStrength");
			r.materials[0].SetFloat("_Shininess", 10);
			r.materials[0].SetFloat("_SpecInt", 3);
			r.materials[0].SetFloat("_Fresnel", 1);
			setupRenderer(r, "Main");
			RenderUtil.makeTransparent(r.materials[1]);
			r.materials[0].EnableKeyword("FX_KELP");
			r.materials[0].SetColor("_Color", new Color(0, 0, 0, 1F));
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
		
		internal static void setupRenderer(Renderer r, string texName) {
			//RenderUtil.makeTransparent(r.materials[1]);
			//r.materials[1].SetFloat("_ZWrite", 1);
			for (int i = 0; i < r.materials.Length; i++) {
				Material m = r.materials[i];
				//m.DisableKeyword("UWE_WAVING");
				m.DisableKeyword("FX_KELP");
				m.SetVector("_Scale", Vector4.one*(0.06F-0.02F*i));
				m.SetVector("_Frequency", new Vector4(2.5F, 2F, 1.5F, 0.5F));
				m.SetVector("_Speed", Vector4.one*(0.1F-0.025F*i));
				m.SetVector("_ObjectUp", new Vector4(0F, 0F, 0F, 0F));
				m.SetFloat("_WaveUpMin", 2.5F);
				m.SetColor("_Color", new Color(1, 1, 1, 1));
				m.SetFloat("_minYpos", 0.7F);
				m.SetFloat("_maxYpos", 0.3F);
			}
			r.materials[0].SetColor("_Color", new Color(1, 1, 1, 0.5F));
			r.materials[1].SetColor("_Color", new Color(0, 0, 0, 1));
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Resources/GlowOil/"+texName, new Dictionary<int, string>{{0, "Shell"}, {1, "Inner"}});
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
		
		private Renderer mainRender;
		private PrefabIdentifier prefab;
		private Rigidbody mainBody;
		
		//private GameObject bubble;
		
		private float glowIntensity = 0;
		
		private float lastGlowUpdate;
		private float lastLitTime;
		private float lastRepelTime;
		
		private readonly List<GlowSeed> seeds = new List<GlowSeed>();
		
		void Update() {
			if (!mainRender) {
				mainRender = GetComponentInChildren<Renderer>();
			}
			if (!mainBody) {
				mainBody = GetComponentInChildren<Rigidbody>();
			}
			if (!prefab) {
				prefab = GetComponentInChildren<PrefabIdentifier>();
			}
			int hash = prefab.Id.GetHashCode();
			while (seeds.Count < 4+((hash%5)+5)%5) {
				GameObject go = ObjectUtil.createWorldObject("18229b4b-3ed3-4b35-ae30-43b1c31a6d8d");
				RenderUtil.convertToModel(go);
				ObjectUtil.removeComponent<Collider>(go);
				ObjectUtil.removeComponent<PrefabIdentifier>(go);
				ObjectUtil.removeComponent<ChildObjectIdentifier>(go);
				go.transform.SetParent(transform);
				go.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.1F, 0.15F);
				go.transform.localPosition = MathUtil.getRandomVectorAround(Vector3.zero, 0.4F);
				go.transform.localRotation = UnityEngine.Random.rotationUniform;
				Renderer r = go.GetComponentInChildren<Renderer>();
				r.materials[0].SetFloat("_Shininess", 0F);
				r.materials[0].SetFloat("_SpecInt", 0F);
				r.materials[0].SetFloat("_Fresnel", 0F);
				r.materials[0].EnableKeyword("UWE_WAVING");
				r.materials[1].EnableKeyword("UWE_WAVING");
				RenderUtil.setEmissivity(r.materials[0], 0, "GlowStrength");
				RenderUtil.setEmissivity(r.materials[1], 0, "GlowStrength");
				GlowOil.setupRenderer(r, "Seed");
				Vector3 rot = UnityEngine.Random.rotationUniform.eulerAngles.normalized*UnityEngine.Random.Range(0.75F, 1.25F);
				seeds.Add(new GlowSeed{go = go, render = r, motion = MathUtil.getRandomVectorAround(Vector3.zero, 0.07F), rotation = rot});
			}
			if (!light)
				light = GetComponentInChildren<Light>();
			float sc = (0.25F+0.05F*(float)GlowOil.sizeNoise.getValue(transform.position));
			transform.localScale = Vector3.one*sc;
			
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = time-lastGlowUpdate;
			if (dT >= 0.02F) {
				lastGlowUpdate = time;
				updateGlowStrength(time, dT);
			}
			dT = Time.deltaTime;
			if (time-lastRepelTime >= 0.5) {
				lastRepelTime = time;
				foreach (RaycastHit hit in Physics.SphereCastAll(transform.position, 8F, Vector3.one, 8)) {
					if (hit.transform && hit.transform != transform) {
						GlowOilTag g = hit.transform.GetComponentInParent<GlowOilTag>();
						if (g)
							repel(g, dT);
					}
				}
			}
			
			foreach (GlowSeed g in seeds) {
				if (!g.go)
					continue;
				Vector3 dd = g.go.transform.localPosition;
				if (dd.HasAnyNaNs()) {
					dd = MathUtil.getRandomVectorAround(Vector3.zero, 0.4F);
					g.go.transform.localPosition = dd;
					//SNUtil.writeToChat(seeds.IndexOf(g)+" was nan pos");
				}
				else {
					//SNUtil.writeToChat(seeds.IndexOf(g)+" was NOT nan pos");
					float d = dd.sqrMagnitude;
					if (float.IsNaN(d)) {
						g.motion = MathUtil.getRandomVectorAround(Vector3.zero, 0.07F);
						//SNUtil.writeToChat(seeds.IndexOf(g)+" was nan dd - "+g.go.transform.localPosition);
						//SNUtil.log(seeds.IndexOf(g)+" was nan dd - "+g.go.transform.localPosition);
					}
					else {
						Vector3 norm = dd.normalized;
						if (norm.HasAnyNaNs()) {
							//SNUtil.writeToChat("NaN Norm");
							//SNUtil.log("NaN Norm");
							norm = Vector3.zero;
						}
						//SNUtil.writeToChat("Mot="+g.motion);
						//SNUtil.log("Mot="+g.motion);
						g.motion = g.motion-(norm*d*6F*dT);
						float maxD = 2.0F*transform.localScale.magnitude;
						if (float.IsNaN(maxD)) {
							//SNUtil.writeToChat("NaN maxD");
							//SNUtil.log("NaN maxD");
							maxD = 0;
						}
						if (d > maxD) {
							g.go.transform.position = norm*maxD;
						}
					}
					g.go.transform.localPosition = g.go.transform.localPosition+g.motion*dT;
					g.go.transform.Rotate(g.rotation, Space.Self);
				}
				RenderUtil.setEmissivity(g.render.materials[1], glowIntensity*9F, "GlowStrength");
			}
			RenderUtil.setEmissivity(mainRender.materials[0], glowIntensity*5F, "GlowStrength");
			if (light) {
				light.intensity = glowIntensity*GlowOil.MAX_GLOW;
				light.range = GlowOil.MAX_RADIUS*(0.5F+glowIntensity/2F);
			}
		}
		
		private void updateGlowStrength(float time, float dT) {
			float delta = time-lastLitTime < 1.5F ? 0.5F : -0.15F;
			glowIntensity = Mathf.Clamp01(glowIntensity+delta*dT);
		}
		
		internal void repel(GlowOilTag g, float dT) {
			Vector3 dd = transform.position-g.transform.position;
			//SNUtil.writeToChat("Repel from "+g.transform.position+" > "+dd);
			mainBody.AddForce(dd.normalized*(15F/dd.sqrMagnitude)*dT, ForceMode.VelocityChange);
			g.mainBody.AddForce(dd.normalized*(-15F/dd.sqrMagnitude)*dT, ForceMode.VelocityChange);
		}
		
		internal void onLit() {
			lastLitTime = DayNightCycle.main.timePassedAsFloat;
		}
		
		class GlowSeed {
			
			internal GameObject go;
			internal Renderer render;
			internal Vector3 motion = Vector3.zero;
			internal Vector3 rotation = Vector3.zero;
			
		}
		
	}
}
