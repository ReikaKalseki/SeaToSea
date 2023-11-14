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
	
	public class SanctuaryJellyray : RetexturedFish {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
	    internal SanctuaryJellyray(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.JELLYRAY.prefab) {
			locale = e;
			glowIntensity = 0.5F;
			
			scanTime = 5;
			eggBase = TechType.JellyrayEgg;
			eggMaturationTime = 3600;
			//eggSpawnRate = 0.25F;
			//eggSpawns.Add(BiomeType.GrandReef_TreaderPath);
	    }
			
		public override void prepareGameObject(GameObject world, Renderer[] r0) {
			PurpleJellyrayTag kc = world.EnsureComponent<PurpleJellyrayTag>();
			foreach (Renderer r in r0) {
				r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
				RenderUtil.disableTransparency(r.materials[0]);
				r.materials[0].SetFloat("_EmissionLM", 0.1F);
				r.materials[0].SetFloat("_EmissionLMNight", 0.1F);
			}
	    }
		
		public override BehaviourType getBehavior() {
			return BehaviourType.MediumFish;
		}
			
	}
	
	class PurpleJellyrayTag : MonoBehaviour {
		
		private Renderer[] renders;
		
		void Update() {
			if (renders == null)
				renders = GetComponentsInChildren<Renderer>();
		}
		
	}
}
