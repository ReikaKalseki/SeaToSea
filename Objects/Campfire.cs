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
		
		private static readonly CampfireSaveHandler saveHandler = new CampfireSaveHandler();
		
		static Campfire() {
			addRecipe(TechType.Peeper, 3);
			addRecipe(TechType.Reginald, 4);
			addRecipe(TechType.Spadefish, 2);
			addRecipe(TechType.HoleFish, 2);
			addRecipe(TechType.Oculus, 3);
			addRecipe(TechType.Eyeye, 3);
			addRecipe(TechType.Boomerang, 2);
			addRecipe(TechType.GarryFish, 2);
			addRecipe(TechType.Hoopfish, 2);
			addRecipe(TechType.Spinefish, 2);
			addRecipe(TechType.LavaBoomerang, 1);
			addRecipe(TechType.LavaEyeye, 2);
			addRecipe(TechType.Bladderfish, 1);
			addRecipe(TechType.Hoverfish, 2);
		}
		
		internal class CampfireSaveHandler : SaveSystem.SaveHandler {
			
			public override void save(PrefabIdentifier pi) {
				CampfireTag lgc = pi.GetComponent<CampfireTag>();
				if (lgc)
					lgc.save(data);
			}
			
			public override void load(PrefabIdentifier pi) {
				CampfireTag lgc = pi.GetComponent<CampfireTag>();
				if (lgc)
					lgc.load(data);
			}
		}
		
		private static void addRecipe(TechType inp, float secs = 2) {
			SmokingRecipe sr = new SmokingRecipe(inp, new SmokedFish(inp), secs);
			sr.output.Patch();
			cookMap[inp] = sr;
		}
	        
	    internal Campfire() : base("Campfire", "", "") {
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, saveHandler);
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
			
			BoxCollider box = go.GetComponentInChildren<BoxCollider>();
			box.size = new Vector3(box.size.x*0.15F, box.size.y, box.size.z*0.15F);
			
			GameObject pot = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(ObjectUtil.lookupPrefab(TechType.PlanterPot), "model/Base_interior_Planter_Pot_01"));
			ObjectUtil.removeChildObject(pot, "pot_generic_plant_01");
			GameObject cone = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(ObjectUtil.lookupPrefab(TechType.HangingFruit), "Fruit_03"));
			ObjectUtil.removeChildObject(cone, "Capsule");
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
		
		public static void updateLocale() {
			foreach (KeyValuePair<TechType, SmokingRecipe> kvp in cookMap) {
				kvp.Value.output.updateLocale();
			}
		}
			
	}
	
	class SmokedFish : Spawnable {
		
		private readonly TechType rawFish;
		private readonly TechType cookedFish;
		private readonly TechType curedFish;
		
		private readonly Atlas.Sprite sprite;
		
		internal SmokedFish(TechType raw) : base("Smoked"+raw, "", "") {
			rawFish = raw;
			
			curedFish = (TechType)Enum.Parse(typeof(TechType), "Cured"+rawFish);
			cookedFish = (TechType)Enum.Parse(typeof(TechType), "Cooked"+rawFish);
			/*
			sprite = RenderUtil.copySprite(SpriteManager.Get(cookedFish));
			Texture2D repl = new Texture2D(sprite.texture.width, sprite.texture.height, sprite.texture.format, false);
			for (int i = 0; i < sprite.texture.width; i++) {
				for (int k = 0; k < sprite.texture.height; k++) {
					Color c = sprite.texture.GetPixel(i, k);
					c = Color.Lerp(c, new Color(111/255F, 78/255F, 55/255F, 1), 0.33F);
					repl.SetPixel(i, k, c);
				}
			}
			repl.Apply(false, false);
			sprite.texture = repl;*/
			sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/SmokedFish/"+rawFish.AsString().ToLowerInvariant());
			
			typeof(ModPrefab).GetField("Mod", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this, SeaToSeaMod.modDLL);
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return sprite;
		}
		
	    public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject(curedFish);
			go.GetComponent<Eatable>().waterValue = ObjectUtil.lookupPrefab(cookedFish).GetComponent<Eatable>().waterValue*0.67F;
			return go;
		}
		
		internal void updateLocale() {
			LanguageHandler.SetLanguageLine(TechType.AsString(), "Smoked "+Language.main.Get(rawFish));
			LanguageHandler.SetLanguageLine("Tooltip_"+TechType.AsString(), Language.main.Get("Tooltip_"+cookedFish.AsString())+" "+Language.main.Get("SmokedNoExpire"));
			SNUtil.log("Relocalized smoked fish "+this+" > "+TechType.AsString()+" > "+Language.main.Get(TechType), SNUtil.diDLL);
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
			if (live) {
				live.health = live.maxHealth;
				live.invincible = true;
			}
			
			if (!fire) {
				fire = ObjectUtil.getChildObject(gameObject, "Extinguishable_Fire_small(Clone)");
				if (!fire)
					return;
				fire.transform.localPosition = Vector3.up*0.23F;
				fire.transform.localRotation = Quaternion.identity;
				fire.transform.localScale = new Vector3(0.5F, 1, 0.5F);
			}
			
			float time = DayNightCycle.main.timePassedAsFloat;
			light.range = 12+1.2F*Mathf.Sin(time*12.917F)+0.5F*Mathf.Sin(time*18.371F+217F)+0.2F*Mathf.Sin(time*45.713F+62F);
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
			InventoryUtil.addItem(cooking.output.TechType);
			Story.StoryGoal.Execute("campfire", Story.GoalType.Story);
		}
		
		internal void load(XmlElement data) {
			string rec = data.getProperty("cooking");
			if (string.IsNullOrEmpty(rec))
				cooking = null;
			else
				cooking = Campfire.cookMap[(TechType)Enum.Parse(typeof(TechType), rec)];
		}
		
		internal void save(XmlElement data) {
			data.addProperty("cooking", cooking == null ? "null" : cooking.input.AsString());
		}
		
		public void OnHandHover(GUIHand hand) {
			if (cooking != null) {
			  	HandReticle.main.SetProgress(cookProgress);
				HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
				HandReticle.main.SetInteractText("CampfireCooking");
			   	HandReticle.main.SetTargetDistance(8);
			}
			else {
			   	HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
			   	HandReticle.main.SetInteractText("CampfireClick");
			   	HandReticle.main.SetTargetDistance(8);
			}
		}
	
		public void OnHandClick(GUIHand hand) {
			if (cooking == null) {
				Pickupable held = Inventory.main.GetHeld();
				if (held) {
					TechType tt = held.GetTechType();
					RetexturedFish rf = RetexturedFish.getFish(tt);
					if (rf != null)
						tt = CraftData.entClassTechTable[rf.baseTemplate.prefab];
					if (Campfire.cookMap.ContainsKey(tt) && Inventory.main.TryRemoveItem(held)) { //sound
						cooking = Campfire.cookMap[tt];
						UnityEngine.Object.DestroyImmediate(held.gameObject);
					}
				}
			}
		}
		
	}
	
	class SmokingRecipe {
		
		internal readonly TechType input;
		internal readonly SmokedFish output;
		internal readonly float cookTime;
		
		internal SmokingRecipe(TechType inp, SmokedFish outp, float time) {
			input = inp;
			output = outp;
			cookTime = time;
		}
		
	}
}
