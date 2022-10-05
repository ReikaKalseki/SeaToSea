﻿using System;
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
		
		public static float minTemperature = 25;
		public static float idealTemperature = 60;
		public static float maxTemperature = 150;
		
		internal static readonly Simplex3DGenerator noiseField = (Simplex3DGenerator)new Simplex3DGenerator(DateTime.Now.Ticks).setFrequency(0.1);
		internal static readonly SimplexNoiseGenerator heightNoiseField = (SimplexNoiseGenerator)new SimplexNoiseGenerator(873428712).setFrequency(1.0);
		
		private static readonly string CHILD_NAME = "column_plant_";
		private static readonly string CHILD_NAME_2 = "leaf_aux_plant_";
		
		public static readonly float GROWTH_TIME = 1800;
		
		private static bool leavesOnlyRendering;
		
		public VentKelp() : base(SeaToSeaMod.itemLocale.getEntry("VENT_KELP"), VanillaFlora.FERN_PALM, "afba45cf-00f9-4d80-a203-429d6ce7ff62", "Samples") {
			glowIntensity = 0.8F;
			finalCutBonus = 2;
		}
		
		internal static double getGrowthSpeed(float temp) {
			return temp <= idealTemperature ? MathUtil.linterpolate(temp, minTemperature, idealTemperature, 0, 1, true) : MathUtil.linterpolate(temp, idealTemperature, maxTemperature, 1, 0, true);
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(1, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r0) {
			base.prepareGameObject(go, r0);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
			GlowKelpTag g = go.EnsureComponent<GlowKelpTag>();
			float h = 0;/*
			int n = (int)Math.Min(9, 3+((1+heightNoiseField.getValue(go.transform.position))*8));
			for (int i = 0; i < n; i++) {
				string pfb = VanillaFlora.FERN_PALM.getRandomPrefab(false);
				string nm = CHILD_NAME+i;
				GameObject child = getOrCreateSubplant(pfb, go, h, nm);
				prepareSubplant(child);
				
				float dh = pfb.StartsWith("1d6d8", StringComparison.InvariantCultureIgnoreCase) ? 0.55F : 0.9F;//1.25F;
				h += dh;//*UnityEngine.Random.Range(0.2F, 1F);
			}
			*/
			float maxH = (float)Math.Min(9, 3+((1+heightNoiseField.getValue(go.transform.position))*8));//h;
			h = 0;
			int i0 = 0;
			while (h < maxH-1) {
				string pfb = VanillaFlora.CAVE_BUSH.getRandomPrefab(false);
				string nm = CHILD_NAME_2+i0;
				GameObject child = getOrCreateSubplant(pfb, go, h, nm);
				prepareSubplant(child, true);
				h += UnityEngine.Random.Range(0.75F, 1.5F);
				i0++;
			}
			ObjectUtil.removeChildObject(go, "land_plant_middle_03_01");
			ObjectUtil.removeChildObject(go, "land_plant_middle_03_02");
			ObjectUtil.removeChildObject(go, "coral_reef_plant_middle_12");
			go.SetActive(true);
		}
		
		protected override void ProcessPrefab(GameObject go) {
			base.ProcessPrefab(go);
			
			go.SetActive(true);
			ModPrefabCache.AddPrefab(go, true);
		}
		
		private GameObject getOrCreateSubplant(string pfb, GameObject go, float h, string nm) {
			GameObject child = ObjectUtil.getChildObject(go, nm);
			if (!child) {
				child = ObjectUtil.createWorldObject(pfb);	
				child.name = nm;
				child.transform.parent = go.transform;
				child.transform.localPosition = Vector3.up*h;
				child.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0);
				child.transform.localScale = Vector3.one;
			}
			return child;
		}
		
		private void prepareSubplant(GameObject child, bool leavesOnly = false) {
			child.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
			child.EnsureComponent<TechTag>().type = TechType;
			
			Renderer r = child.GetComponentInChildren<Renderer>(true);
			leavesOnlyRendering = leavesOnly;
			if (leavesOnly) {
				r.materials[0].color = new Color(0, 0, 0, 0);
				//child.transform.localScale = new Vector3(1.1F, 1.5F, 1.1F);
				r.materials[0].DisableKeyword("MARMO_EMISSION");
				r.materials[0].DisableKeyword("MARMO_SPECMAP");
			}
			else {
				r.materials[0].EnableKeyword("FX_KELP");
				r.materials[0].SetVector("_Scale", new Vector4(0.7F, 0.4F, 0.8F, 0.4F));
				r.materials[0].SetVector("_Frequency", new Vector4(0.16F, 0.1F, 0.12F, 0.6F));
				r.materials[0].SetVector("_Speed", new Vector4(1F, 0.8F, 0.0F, 0.0F));
				r.materials[0].SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
				r.materials[0].SetFloat("_WaveUpMin", 0.4F);
				r.materials[0].SetFloat("_minYpos", 1F);
				r.materials[0].SetFloat("_maxYpos", 0F);
			}
			
			r.materials[1].EnableKeyword("FX_KELP");
			r.materials[1].SetFloat("_minYpos", 1F);
			r.materials[1].SetFloat("_maxYpos", 0F);
			if (leavesOnly) {
				r.materials[1].SetVector("_Scale", new Vector4(0.5F, 0.3F, 0.5F, 0.4F));
				r.materials[1].SetVector("_Frequency", new Vector4(0.16F, 0.1F, 0.12F, 0.6F));
				r.materials[1].SetVector("_Speed", new Vector4(0.5F, 0.6F, 0.0F, 0.0F));
				r.materials[1].SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
				r.materials[1].SetFloat("_WaveUpMin", 0.25F);
			}
			else {
				r.materials[1].SetVector("_Scale", new Vector4(1.1F, 0.6F, 1.3F, 0.6F));
				r.materials[1].SetVector("_Frequency", new Vector4(0.16F, 0.1F, 0.12F, 0.6F));
				r.materials[1].SetVector("_Speed", new Vector4(1F, 0.8F, 0.0F, 0.0F));
				r.materials[1].SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
				r.materials[1].SetFloat("_WaveUpMin", 0.4F);
			}
			r.materials[0].SetColor("_GlowColor", GlowKelpTag.idleColor);
			r.materials[1].SetColor("_GlowColor", GlowKelpTag.idleColor);
			RenderUtil.makeTransparent(r, leavesOnly ? new HashSet<int>{0, 1} : new HashSet<int>{1});
			RenderUtil.setEmissivity(r, 8, "GlowStrength", new HashSet<int>{1});
			RenderUtil.swapToModdedTextures(child.GetComponentInChildren<Renderer>(), this);
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
		public Dictionary<int,string> getTextureLayers() {
			return leavesOnlyRendering ? new Dictionary<int, string>(){{0, ""}, {1, "Leaves"}} : new Dictionary<int, string>(){{0, ""}, {1, ""}};
		}
		/*
		public override float getGrowthTime() {
			return GROWTH_TIME;
		}*/
		/*
		public override void prepareGrowingPlant(GrowingPlant g) {
			g.gameObject.EnsureComponent<GrowingGlowKelp>();
		}*/
		
	}
	/*
	class GrowingGlowKelp : MonoBehaviour {
		
		private GrowingPlant plant;
		
		void Update() {
			if (!plant)
				plant = gameObject.GetComponent<GrowingPlant>();
			Planter p = gameObject.GetComponentInParent<Planter>();
			Vector3 pos = p.gameObject.transform.position;
			float temp = WaterTemperatureSimulation.main.GetTemperature(pos); //this method is patched to have the hooks for hotter ILZ and so on 
			float spd = (float)VentKelp.getGrowthSpeed(temp);
			//SNUtil.writeToChat(temp+"C @ "+pos+" > "+spd);
			if (spd <= 0) {
				plant.growthDuration = 99999;
				plant.timeStartGrowth = DayNightCycle.main.timePassedAsFloat;
			}
			else {
				plant.growthDuration = VentKelp.GROWTH_TIME/spd;
			}
		}
		
	}
	*/
	class GlowKelpTag : MonoBehaviour {
		
		internal static readonly Color idleColor = new Color(0.1F, 0, 0.5F, 1);
		internal static readonly Color activeColor = new Color(0.7F, 0.2F, 1, 1);
		
		private Renderer[] renderers;
		private bool redoRenderers;
		private float creationTime = 999999;
		
		void Start() {
			SeaToSeaMod.kelp.prepareGameObject(gameObject, null);
			creationTime = DayNightCycle.main.timePassedAsFloat;
			if (gameObject.GetComponent<GrownPlant>()) {
    			gameObject.SetActive(true);
    		}
    		else {
    			gameObject.transform.localScale = new Vector3(12, 12, 12);
				gameObject.transform.rotation = Quaternion.identity;
    		}
		}
		
		void Update() {
			bool isNew = DayNightCycle.main.timePassedAsFloat-creationTime <= 0.25F;
			if (renderers == null || renderers.Length == 0 || isNew || redoRenderers)
				renderers = gameObject.GetComponentsInChildren<Renderer>();
			if (gameObject.GetComponent<GrownPlant>()) {
    			gameObject.transform.localScale = new Vector3(2, 3.5F, 2);
    			if (isNew) {
	    			foreach (Renderer r in renderers) {
						r.materials[1].SetVector("_Scale", new Vector4(0.05F, 0.0F, 0.05F, 0.0F));
						r.materials[1].SetVector("_Frequency", new Vector4(0.8F, 0.8F, 0.8F, 0.8F));
						r.materials[1].SetVector("_Speed", new Vector4(0.2F, 0.2F, 0.2F, 0.2F));
						r.materials[1].SetFloat("_WaveUpMin", 0F);
	    			}
    			}
			}
			else if (transform.position.y >= -400 || MathUtil.getDistanceToLineSegment(transform.position, UnderwaterIslandsFloorBiome.wreckCtrPos1, UnderwaterIslandsFloorBiome.wreckCtrPos2) <= 12) {
				UnityEngine.Object.DestroyImmediate(gameObject);
				return;
			}
			float f = 1-(float)((VentKelp.noiseField.getValue(gameObject.transform.position+Vector3.down*DayNightCycle.main.timePassedAsFloat*7.5F)+1)/2D);
			bool kill = false;
			List<int> presenceSet = new List<int>();
			foreach (Renderer r in renderers) {
				if (!r || !r.gameObject) {
					kill = true;
					continue;
				}
				//float f = (float)Math.Abs(2*VentKelp.noiseField.getValue(r.gameObject.transform.position+Vector3.up*DayNightCycle.main.timePassedAsFloat*7.5F))-0.75F;
				foreach (Material m in r.materials) {
					m.SetColor("_GlowColor", Color.Lerp(idleColor, activeColor, f*1.5F-0.5F));
				}
				LiveMixin lv = r.gameObject.GetComponent<LiveMixin>();
				if (lv && lv.health <= 0) {
					kill = true;
				}
				GameObject go = r.transform.parent.gameObject;
				if (go.transform.position.y >= -3) {
					UnityEngine.Object.DestroyImmediate(go);
					redoRenderers = true;
					continue;
				}
				string n = go.name;
				if (!string.IsNullOrEmpty(n) && n.Contains("leaf_aux")) {
					int num = int.Parse(n.Substring(n.Length-1));
					presenceSet.Add(num);
				}
			}
			presenceSet.Sort();
			int last = -1;
			foreach (int val in presenceSet) {
				if (val-last > 1) {
					kill = true;
				}
				last = val;
			}
			if (kill && !isNew) {/*
				Planter p = gameObject.GetComponentInParent<Planter>();
				GrownPlant g = gameObject.GetComponentInParent<GrownPlant>();
				if (p && g) {
					p.RemoveItem(p.GetSlotID(g.seed));
					p.storageContainer.container.DestroyItem(SeaToSeaMod.kelp.seed.TechType);
					//p.RemoveItem(g.seed);
				}
				UnityEngine.Object.DestroyImmediate(gameObject);*/
				gameObject.GetComponentInParent<LiveMixin>().TakeDamage(99999F);
			}
		}
		
	}
}
