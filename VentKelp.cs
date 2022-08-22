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
		
		public VentKelp() : base(SeaToSeaMod.itemLocale.getEntry("VENT_KELP"), VanillaFlora.SPOTTED_DOCKLEAF, "Samples") {
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
			g.renderer = r;
			foreach (Material m in r.materials) {
				m.SetVector("_Scale", new Vector4(1, 1, 1, 1));
				m.SetVector("_Frequency", new Vector4(2, 2, 2, 2));
				m.SetVector("_Speed", new Vector4(0.1F, 0.1F, 0.1F, 0.1F));
				m.SetVector("_ObjectUp", new Vector4(1F, 1F, 1F, 1F));
				m.SetFloat("_WaveUpMin", 0.33F);
			}
			RenderUtil.makeTransparent(r);
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
		
		void Start() {
			if (renderer == null)
				renderer = gameObject.GetComponentInChildren<Renderer>();
			if (gameObject.GetComponent<GrownPlant>() != null) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = new Vector3(2, 3.5F, 2);
    		}
    		else {
    			gameObject.transform.localScale = new Vector3(12, 15, 12);
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
		}
		
	}
}
