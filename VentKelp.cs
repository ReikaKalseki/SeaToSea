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
	
	public class VentKelp : BasicCustomPlant, MultiTexturePrefab<VanillaFlora> {
		
		internal static readonly Simplex3DGenerator noiseField = (Simplex3DGenerator)new Simplex3DGenerator(DateTime.Now.Ticks).setFrequency(0.1);
		
		private static readonly string CHILD_NAME = "column_plant_";
		
		public VentKelp() : base(SeaToSeaMod.itemLocale.getEntry("VENT_KELP"), VanillaFlora.FERN_PALM, "Samples") {
			glowIntensity = 0.8F;
			finalCutBonus = 2;
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(1, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r0) {
			base.prepareGameObject(go, r0);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
			ObjectUtil.removeChildObject(go, "land_plant_middle_03_01");
			ObjectUtil.removeChildObject(go, "land_plant_middle_03_02");
			GlowKelpTag g = go.EnsureComponent<GlowKelpTag>();
			/*
			foreach (Material m in r.materials) {
				m.SetVector("_Scale", new Vector4(1, 1, 1, 1));
				m.SetVector("_Frequency", new Vector4(2, 2, 2, 2));
				m.SetVector("_Speed", new Vector4(0.1F, 0.1F, 0.1F, 0.1F));
				m.SetVector("_ObjectUp", new Vector4(1F, 1F, 1F, 1F));
				m.SetFloat("_WaveUpMin", 0.33F);
			}*/
			if (r0) {
				RenderUtil.swapToModdedTextures(r0, this);
				//RenderUtil.makeTransparent(r0, new HashSet<int>{1});
				RenderUtil.setEmissivity(r0, 8, "GlowStrength", new HashSet<int>{1});
			}
			float h = 0;
			for (int i = 0; i < 8; i++) {
				string pfb = VanillaFlora.FERN_PALM.getRandomPrefab(false);
				string n = CHILD_NAME+i;
				GameObject child = ObjectUtil.getChildObject(go, n);
				SNUtil.log("Vent kelp had child @ "+i+": "+child);
				if (!child) {
					child = ObjectUtil.createWorldObject(pfb);	
					child.name = n;
					child.transform.parent = go.transform;
					child.transform.localPosition = Vector3.up*h;
					child.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0);
					child.transform.localScale = Vector3.one;
				}
				child.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
				child.EnsureComponent<TechTag>().type = TechType;
				//child.EnsureComponent<PrefabIdentifier>().classId = ClassID;
				//child.EnsureComponent<LiveMixin>().;
				
				Renderer r = child.GetComponentInChildren<Renderer>(true);/*
				r.materials[0].EnableKeyword("UWE_WAVING");
				r.materials[0].SetVector("_Scale", new Vector4(0.1F, 0.1F, 0.1F, 0.1F));
				r.materials[0].SetVector("_Frequency", new Vector4(1, 1, 1, 1));
				r.materials[0].SetVector("_Speed", new Vector4(0.4F, 0.4F, 0.4F, 0.4F));
				r.materials[0].SetVector("_ObjectUp", new Vector4(1F, 1F, 1F, 1F));
				r.materials[0].SetFloat("_WaveUpMin", 0F);/*
				r.materials[1].EnableKeyword("UWE_WAVING");
				r.materials[1].SetVector("_Scale", new Vector4(2F, 2F, 1.5F, 1F));
				r.materials[1].SetVector("_Frequency", new Vector4(0.2F, 1, 2.5F, 0.2F));
				r.materials[1].SetVector("_Speed", new Vector4(0.1F, 0.2F, 0.2F, 0.2F));
				r.materials[1].SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
				r.materials[1].SetFloat("_WaveUpMin", 0.4F);
				*/
				r.materials[0].EnableKeyword("FX_KELP");
				r.materials[0].SetVector("_Scale", new Vector4(1.1F, 0.6F, 1.3F, 0.6F));
				r.materials[0].SetVector("_Frequency", new Vector4(0.16F, 0.1F, 0.12F, 0.6F));
				r.materials[0].SetVector("_Speed", new Vector4(1F, 0.8F, 0.0F, 0.0F));
				r.materials[0].SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
				r.materials[0].SetFloat("_WaveUpMin", 0.4F);
				r.materials[0].SetFloat("_minYpos", 1F);
				r.materials[0].SetFloat("_maxYpos", 0F);
				
				r.materials[1].EnableKeyword("FX_KELP");
				r.materials[1].SetVector("_Scale", new Vector4(1.1F, 0.6F, 1.3F, 0.6F));
				r.materials[1].SetVector("_Frequency", new Vector4(0.16F, 0.1F, 0.12F, 0.6F));
				r.materials[1].SetVector("_Speed", new Vector4(1F, 0.8F, 0.0F, 0.0F));
				r.materials[1].SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
				r.materials[1].SetFloat("_WaveUpMin", 0.4F);
				r.materials[1].SetFloat("_minYpos", 1F);
				r.materials[1].SetFloat("_maxYpos", 0F);
				RenderUtil.makeTransparent(r, new HashSet<int>{1});
				RenderUtil.setEmissivity(r, 8, "GlowStrength", new HashSet<int>{1});
				RenderUtil.swapToModdedTextures(child.GetComponentInChildren<Renderer>(), this);
				
				float dh = pfb.StartsWith("1d6d8", StringComparison.InvariantCultureIgnoreCase) ? 0.55F : 0.9F;//1.25F;
				h += dh;
			}
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
		public Dictionary<int,string> getTextureLayers() {
			return new Dictionary<int, string>(){{0, ""}, {1, ""}};
		}
	
		public static void doThingTo(Action<GameObject> a) {
				foreach (GlowKelpTag g in UnityEngine.Object.FindObjectsOfType<GlowKelpTag>()) {
				a(g.gameObject);
				}
		}
		
	}
	
	class GlowKelpTag : MonoBehaviour {
		
		private static readonly Color idleColor = new Color(0.1F, 0, 0.5F, 1);
		private static readonly Color activeColor = new Color(0.7F, 0.2F, 1, 1);
		
		private Renderer[] renderers;
		
		void Start() {
			if (renderers == null || renderers.Length == 0)
				renderers = gameObject.GetComponentsInChildren<Renderer>();
			if (gameObject.GetComponent<GrownPlant>() != null) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = new Vector3(2, 3.5F, 2);
    		}
    		else {
    			gameObject.transform.localScale = new Vector3(8, 12, 8);
				gameObject.transform.rotation = Quaternion.identity;
    		}
		}
		
		void Update() {
			float f = (float)Math.Abs(VentKelp.noiseField.getValue(transform.position+Vector3.up*DayNightCycle.main.timePassedAsFloat*5.1F));
			foreach (Renderer r in renderers) {
				foreach (Material m in r.materials) {
					m.SetColor("_GlowColor", Color.Lerp(idleColor, activeColor, f));
				}
			}
		}
		
	}
}
