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
	
	public class DeepStalker : Spawnable {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
	    internal DeepStalker(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaCreatures.STALKER.prefab, true, true);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			DeepStalkerTag kc = world.EnsureComponent<DeepStalkerTag>();
			Renderer r = world.GetComponentInChildren<Renderer>();
			RenderUtil.setEmissivity(r, 2, "GlowStrength");
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Creature/DeepStalker");
			r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
			return world;
	    }
		
		public void register() {
			Patch();
			SNUtil.addPDAEntry(this, 8, "Lifeforms/Fauna/Carnivores", locale.pda, locale.getField<string>("header"), null);
		}
			
	}
	
	class DeepStalkerTag : MonoBehaviour {
		
		private Renderer render;
		private Stalker creatureComponent;
		
		private readonly Color peacefulColor = new Color(0.2F, 0.67F, 1F, 1);
		private readonly Color aggressiveColor = new Color(1, 0, 0, 1);
		private readonly float colorChangeSpeed = 1;
		
		private float aggressionForColor = 0;
		
		private void Update() {
			if (!render) {
				render = GetComponentInChildren<Renderer>();
			}
			if (!creatureComponent) {
				creatureComponent = GetComponent<Stalker>();
			}
			if (render && creatureComponent) {
				float dT = Time.deltaTime;
				if (aggressionForColor < creatureComponent.Aggression.Value) {
					aggressionForColor = Mathf.Min(creatureComponent.Aggression.Value, aggressionForColor+dT*colorChangeSpeed);
				}
				else if (aggressionForColor > creatureComponent.Aggression.Value) {
					aggressionForColor = Mathf.Max(creatureComponent.Aggression.Value, aggressionForColor-dT*colorChangeSpeed);
				}
				render.materials[0].SetColor("_GlowColor", Color.Lerp(peacefulColor, aggressiveColor, aggressionForColor));
			}
			
		}
		
	}
}
