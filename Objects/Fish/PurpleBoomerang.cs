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
	
	public class PurpleBoomerang : RetexturedFish {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
	    internal PurpleBoomerang(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.BOOMERANG.prefab) {
			locale = e;
			glowIntensity = 1.0F;
	    }
			
		public override void prepareGameObject(GameObject world, Renderer[] r0) {
			PurpleBoomerangTag kc = world.EnsureComponent<PurpleBoomerangTag>();
			foreach (Renderer r in r0) {
				r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
				RenderUtil.setGlossiness(r, 0.5F, 6, 0.5F);
			}
	    }
		
		public override BehaviourType getBehavior() {
			return BehaviourType.SmallFish;
		}
			
	}
	
	class PurpleBoomerangTag : MonoBehaviour {
		
		private Renderer[] renders;
		
		void Update() {
			if (renders == null)
				renders = GetComponentsInChildren<Renderer>();
			
			float f = Mathf.Max(0, -0.5F+2*(0.5F+0.5F*Mathf.Sin(8*DayNightCycle.main.timePassedAsFloat+gameObject.GetInstanceID())));
			foreach (Renderer r in renders) {
				RenderUtil.setEmissivity(r, f);
			}
		}
		
	}
}
