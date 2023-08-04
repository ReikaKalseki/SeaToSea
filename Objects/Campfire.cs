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
	
	public class Campfire : Spawnable {
		
		internal static readonly Dictionary<TechType, SmokingRecipe> cookMap = new Dictionary<TechType, SmokingRecipe>();
		
		static Campfire() {
			addRecipe(TechType.Peeper, TechType.CuredEyeye, 6);
		}
		
		private static void addRecipe(TechType inp, TechType outp, float secs = 2) {
			cookMap[inp] = new SmokingRecipe(inp, outp, secs);
		}
	        
	    internal Campfire() : base("Campfire", "", "") {
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<CampfireTag>().addField("cooking"));
			};
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject("14bbf7f0-4276-48bf-868b-317b366edd16");
			go.EnsureComponent<CampfireTag>();
			/*
			SphereCollider sc = go.EnsureComponent<SphereCollider>();
			sc.radius = 1;
			sc.center = Vector3.zero;
			sc.isTrigger = true;
			*/
			go.layer = LayerID.Useable;
			Light l = ObjectUtil.addLight(go);
			l.intensity = 0.8F;
			l.color = new Color(1, 0.5F, 0, 1);
			l.range = 12;
			l.transform.localPosition = new Vector3(0, 0.5F, 0);
			
			GameObject pot = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(ObjectUtil.lookupPrefab(TechType.PlanterPot), "model/Base_interior_Planter_Pot_01"));
			ObjectUtil.removeChildObject(pot, "pot_generic_plant_01");
			GameObject cone = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(ObjectUtil.lookupPrefab(TechType.HangingFruit), "Fruit_03"));
			pot.transform.SetParent(go.transform);
			pot.transform.localRotation = Quaternion.Euler(-90, 0, 0);
			pot.transform.localPosition = Vector3.down*0.15F;
			cone.transform.SetParent(go.transform);
			cone.transform.localRotation = Quaternion.Euler(-90, 0, 0);
			cone.transform.localPosition = Vector3.up*0.45F;
			cone.transform.localScale = new Vector3(0.04F, 0.04F, 0.02F);
			Renderer r = cone.GetComponentInChildren<Renderer>();
			RenderUtil.setGlossiness(r, 2, 0, 0.5F);
			RenderUtil.setEmissivity(r, 120);
			return go;
	    }
			
	}
	
	class CampfireTag : MonoBehaviour, IHandTarget {
		
		//private static readonly float COOK_TIME = 2;
		
		private LiveMixin live;
		private Light light;
		
		private GameObject fire;
		
		private float cookProgress;
		private SmokingRecipe cooking;
		
		void Update() {
			if (!live)
				live = GetComponent<LiveMixin>();
			if (!light)
				light = GetComponentInChildren<Light>();
			live.health = live.maxHealth;
			live.invincible = true;
			
			if (!fire) {
				fire = ObjectUtil.getChildObject(gameObject, "Extinguishable_Fire_small(Clone)");
				fire.transform.localPosition = Vector3.up*0.23F;
				fire.transform.localRotation = Quaternion.identity;
				fire.transform.localScale = new Vector3(0.5F, 1, 0.5F);
			}
			
			float time = DayNightCycle.main.timePassedAsFloat;
			light.range = 12+1.2F*Mathf.Sin(time*9.917F)+0.5F*Mathf.Sin(time*14.371F+217F)+0.2F*Mathf.Sin(time*35.713F+62F);
			light.intensity = 0.8F*(0.5F+light.range/24F);
			
			if (cooking != null) {
				cookProgress += Time.deltaTime/cooking.cookTime;
				if (cookProgress >= 1) {
					cookProgress = 0;
					cook();
					cooking = null;
				}
			}
		}
		
		private void cook() {
			InventoryUtil.addItem(cooking.output);
		}
		
		public void OnHandHover(GUIHand hand) {
			if (cooking != null) {
			  	HandReticle.main.SetProgress(cookProgress);
				HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
				HandReticle.main.SetInteractText(C2CHooks.campfireCookingLocaleKey);
			   	HandReticle.main.SetTargetDistance(8);
			}
			else {
			   	HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
			   	HandReticle.main.SetInteractText(C2CHooks.campfireUseLocaleKey);
			   	HandReticle.main.SetTargetDistance(8);
			}
		}
	
		public void OnHandClick(GUIHand hand) {
			if (cooking == null) {
				Pickupable held = Inventory.main.GetHeld();
				if (held && Campfire.cookMap.ContainsKey(held.GetTechType()) && Inventory.main.TryRemoveItem(held)) { //sound
					cooking = Campfire.cookMap[held.GetTechType()];
					UnityEngine.Object.DestroyImmediate(held.gameObject);
				}
			}
		}
		
	}
	
	class SmokingRecipe {
		
		internal readonly TechType input;
		internal readonly TechType output;
		internal readonly float cookTime;
		
		internal SmokingRecipe(TechType inp, TechType outp, float time) {
			input = inp;
			output = outp;
			cookTime = time;
		}
		
	}
}
