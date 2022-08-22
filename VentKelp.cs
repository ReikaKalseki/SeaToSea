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
		
		internal static readonly Simplex3DGenerator noiseField = (Simplex3DGenerator)new Simplex3DGenerator(DateTime.Now.Ticks).setFrequency(0.1);
		
		private static readonly string CHILD_NAME = "column_plant_";
		
		public VentKelp() : base(SeaToSeaMod.itemLocale.getEntry("VENT_KELP"), VanillaFlora.GRUB_BASKET, "Samples") {
			glowIntensity = 0.8F;
			finalCutBonus = 2;
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(1, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
			GlowKelpTag g = go.EnsureComponent<GlowKelpTag>();
			g.renderer = r;/*
			foreach (Material m in r.materials) {
				m.SetColor("_GlowColor", );
				m.SetVector("_Scale", new Vector4(0.35F, 0.2F, 0.1F, 0.0F));
				m.SetVector("_Frequency", new Vector4(1.2F, 0.5F, 1.5F, 0.5F));
				m.SetVector("_Speed", new Vector4(0.2F, 0.5F, 1.5F, 0.5F));
				m.SetVector("_ObjectUp", new Vector4(1F, 1F, 1F, 1F));
				m.SetFloat("_WaveUpMin", 0F);
			}*/
			//RenderUtil.makeTransparent(r);
			while (g.column.Count < 4 || (UnityEngine.Random.Range(0, 4) > 0 && g.column.Count < 12)) {
				GameObject go2 = ObjectUtil.getChildObject(ObjectUtil.lookupPrefab(VanillaFlora.GRUB_BASKET.getRandomPrefab(false)), "land_plant_middle_02");
				//RenderUtil.convertToModel(go2);
				go2.name = CHILD_NAME+g.column.Count;
				g.column.Add(go2);
				go2.transform.parent = go.transform;
				//go.transform.position = gameObject.transform.position+Vector3.up*2.5F;
				go2.transform.localPosition = Vector3.up*2.5F*g.column.Count;
				go2.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(0F, 360F), 0);
				go2.transform.localScale = Vector3.one;
				RenderUtil.swapToModdedTextures(go2.GetComponentInChildren<Renderer>(), this);
			}
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
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
		
		internal Renderer renderer;
		
		internal readonly List<GameObject> column = new List<GameObject>();
		
		void Start() {
			if (renderer == null)
				renderer = gameObject.GetComponentInChildren<Renderer>();
			if (gameObject.GetComponent<GrownPlant>() != null) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = new Vector3(2, 1.5F, 2);
    		}
    		else {
    			gameObject.transform.localScale = new Vector3(8, 6.5F, 8);
				gameObject.transform.rotation = Quaternion.identity;
    		}
		}
		
		void Update() {
			float f = (1+(float)VentKelp.noiseField.getValue(transform.position+Vector3.up*DayNightCycle.main.timePassedAsFloat*0.1F))/2F;
			if (renderer != null) {
				foreach (Material m in renderer.materials) {
					m.SetColor("_GlowColor", Color.Lerp(idleColor, activeColor, f));
				}
			}
			foreach (GameObject go in column) {
				foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
					foreach (Material m in renderer.materials) {
						m.SetColor("_GlowColor", Color.Lerp(idleColor, activeColor, f));
					}
				}
			}
		}
		
	}
}
