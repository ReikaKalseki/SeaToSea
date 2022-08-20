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
		
		public VentKelp() : base(SeaToSeaMod.itemLocale.getEntry("VENT_KELP"), VanillaFlora.CREEPVINE, "Samples") {
			glowIntensity = 0.8F;
			finalCutBonus = 2;
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(1, 1);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<GlowKelpTag>();
			//go.transform.localScale = Vector3.one*2;
			go.transform.rotation = Quaternion.identity;
			foreach (Material m in r.materials) {
				m.SetColor("_GlowColor", new Color(1, 1, 1, 1));/*
				m.SetVector("_Scale", new Vector4(0.35F, 0.2F, 0.1F, 0.0F));
				m.SetVector("_Frequency", new Vector4(1.2F, 0.5F, 1.5F, 0.5F));
				m.SetVector("_Speed", new Vector4(0.2F, 0.5F, 1.5F, 0.5F));
				m.SetVector("_ObjectUp", new Vector4(1F, 1F, 1F, 1F));
				m.SetFloat("_WaveUpMin", 0F);*/
			}
		}
		
		public override float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
	}
	
	class GlowKelpTag : MonoBehaviour {
		
		void Start() {
    		
		}
		
		void Update() {
			
		}
		
	}
}
