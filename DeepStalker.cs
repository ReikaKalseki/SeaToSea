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
			RenderUtil.setEmissivity(r, 1.25F, "GlowStrength");
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Creature/DeepStalker");
			r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
			return world;
	    }
		
		public void register() {
			Patch();
			SNUtil.addPDAEntry(this, 8, "Lifeforms/Fauna/Carnivores", locale.pda, locale.getField<string>("header"), null);
	    
	   		GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, false, BiomeType.SeaTreaderPath_OpenDeep_CreatureOnly, 1, 0.5F);
	   		GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, false, BiomeType.GrandReef_TreaderPath, 1, 0.4F);
		}
			
	}
	
	class DeepStalkerTag : MonoBehaviour {
		
		private Renderer render;
		private Stalker creatureComponent;
		private AggressiveWhenSeeTarget playerHuntComponent;
		private CollectShiny collectorComponent;
		
		private readonly Color peacefulColor = new Color(0.2F, 0.67F, 1F, 1);
		private readonly Color aggressiveColor = new Color(1, 0, 0, 1);
		private readonly float colorChangeSpeed = 1;
		
		private float aggressionForColor = 0;
		
		private float platinumGrabTime = -1;
		
		private void Update() {
			if (!render) {
				render = GetComponentInChildren<Renderer>();
			}
			if (!creatureComponent) {
				creatureComponent = GetComponent<Stalker>();
			}
			if (!collectorComponent) {
				collectorComponent = GetComponent<CollectShiny>();
			}
			if (!playerHuntComponent) {
				foreach (AggressiveWhenSeeTarget agg in GetComponents<AggressiveWhenSeeTarget>()) {
					if (agg.targetType == EcoTargetType.Shark) {
						agg.aggressionPerSecond *= 0.15F;
						agg.ignoreSameKind = false;
						agg.maxRangeScalar *= 1.5F;
						playerHuntComponent = agg;
						break;
					}
				}
			}
			
			float dT = Time.deltaTime;
			
			if (render && creatureComponent) {
				if (aggressionForColor < creatureComponent.Aggression.Value) {
					aggressionForColor = Mathf.Min(creatureComponent.Aggression.Value, aggressionForColor+dT*colorChangeSpeed);
				}
				else if (aggressionForColor > creatureComponent.Aggression.Value) {
					aggressionForColor = Mathf.Max(creatureComponent.Aggression.Value, aggressionForColor-dT*colorChangeSpeed);
				}
				render.materials[0].SetColor("_GlowColor", Color.Lerp(peacefulColor, aggressiveColor, aggressionForColor));
			}
			
			Player ep = Player.main;
			float dist = Vector3.Distance(ep.transform.position, transform.position);
			if (dist <= 30)  {
				int amt = Inventory.main.GetPickupCount(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType);
				if (amt > 0 && UnityEngine.Random.Range(0F, 1F) <= Mathf.Min(amt*0.8F, 0.8F)*dT*15) {
					triggerPtAggro();
				}
			}
			
			if (DayNightCycle.main.timePassedAsFloat-platinumGrabTime <= 12) {
				triggerPtAggro(false);
			}
			else {
				SeaTreader anchor = WorldUtil.getClosest<SeaTreader>(gameObject);
				if (anchor && Vector3.Distance(transform.position, anchor.transform.position) >= 80) {
					GetComponent<SwimBehaviour>().SwimTo(anchor.transform.position, 25);
				}
			}
		}
		
		public void OnMeleeAttack(GameObject target) {
			//SNUtil.writeToChat(this+" attacked "+target);
			if (target == Player.main.gameObject) {
				Pickupable p = Inventory.main.container.RemoveItem(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType);
				if (p) {
					Inventory.main.InternalDropItem(p, false);
					grab(p.gameObject);
				}
			}
			else {
				Stalker s = target.GetComponentInParent<Stalker>();
				if (s) {
					CollectShiny c = s.GetComponent<CollectShiny>();
					GameObject go = c.shinyTarget;
					if (go) {
						c.DropShinyTarget();
						grab(go);
					}
				}
			}
		}
		
		private void grab(GameObject go) {
			collectorComponent.DropShinyTarget();
			collectorComponent.shinyTarget = go;
			collectorComponent.TryPickupShinyTarget();
		}
		
		internal void triggerPtAggro(bool isNew = true) {
			if (isNew)
				platinumGrabTime = DayNightCycle.main.timePassedAsFloat;
			if (creatureComponent && creatureComponent.liveMixin && creatureComponent.liveMixin.IsAlive()) {
				creatureComponent.Aggression.Add(isNew ? 0.3F : 0.1F);
				if (playerHuntComponent) {
					playerHuntComponent.lastTarget.SetTarget(Player.main.gameObject);
				}
			}
		}
		
	}
}
