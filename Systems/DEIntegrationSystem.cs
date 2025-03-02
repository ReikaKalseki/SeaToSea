using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using UnityEngine.UI;

using FMOD;
using FMODUnity;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class DEIntegrationSystem {
		
		public static readonly DEIntegrationSystem instance = new DEIntegrationSystem();
		
		private readonly bool isDeELoaded;
		
		private readonly HashSet<TechType> creatures = new HashSet<TechType>();
		private TechType thelassaceanType;
		private TechType lrThelassaceanType;
		private TechType jellySpinnerType;
    
	    internal WorldCollectedItem thalassaceanCud;
		
		private DEIntegrationSystem() {
	    	isDeELoaded = QModManager.API.QModServices.Main.ModPresent("DeExtinction");
	    	if (isDeELoaded) {
	    		
	    	}
		}
		
		public bool isLoaded() {
			return isDeELoaded;
		}
		
		public TechType getThalassacean() {
			return thelassaceanType;
		}
		
		public TechType getLRThalassacean() {
			return lrThelassaceanType;
		}
		
		internal void applyPatches() {
			if (isDeELoaded)
				doApplyPatches();
		}
		
		private void doApplyPatches() {
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
						
			thalassaceanCud = new WorldCollectedItem(SeaToSeaMod.itemLocale.getEntry("ThalassaceanCud"), "bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782");
			thalassaceanCud.renderModify = (r) => {
				C2CThalassaceanCudTag.setupRenderer(r);
			};
			thalassaceanCud.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/ThalassaceanCud");
			thalassaceanCud.Patch();
			
			thelassaceanType = findCreature("StellarThalassacean");
			lrThelassaceanType = findCreature("JasperThalassacean");
			
			jellySpinnerType = findCreature("JellySpinner");
			
			creatures.Add(thelassaceanType);
			creatures.Add(lrThelassaceanType);
			creatures.Add(jellySpinnerType);
			creatures.Add(findCreature("Twisteel"));
			creatures.Add(findCreature("Filtorb"));
			creatures.Add(findCreature("GulperLeviathan"));
			creatures.Add(findCreature("GulperLeviathanBaby"));
			
			int amt = RecipeUtil.removeIngredient(C2CItems.powerSeal.TechType, EcoceanMod.glowOil.TechType).amount;
			RecipeUtil.addIngredient(C2CItems.powerSeal.TechType, thalassaceanCud.TechType, amt);
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.HeatSealant).TechType, thalassaceanCud.TechType, 2);
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.SealFabric).TechType, thalassaceanCud.TechType, 1);
			RecipeUtil.addIngredient(C2CItems.depth1300.TechType, thalassaceanCud.TechType, 4);
			RecipeUtil.addIngredient(C2CItems.bandage.TechType, thalassaceanCud.TechType, 1);
			
			amt = RecipeUtil.removeIngredient(C2CItems.breathingFluid.TechType, TechType.Eyeye).amount;
			RecipeUtil.addIngredient(C2CItems.breathingFluid.TechType, jellySpinnerType, amt*3/2); //from 2 to 3
			
			//TODO other creatures
		}
		
		private TechType findCreature(string id) {
			TechType tt = TechType.None;
			if (!TechTypeHandler.TryGetModdedTechType(id, out tt))
				if (!TechTypeHandler.TryGetModdedTechType(id.ToLowerInvariant(), out tt))
					TechTypeHandler.TryGetModdedTechType(id.setLeadingCase(false), out tt);
			if (tt == TechType.None)
				throw new Exception("Could not find DeE TechType for '"+id+"'");
			return tt;
		}
	    
	    internal class C2CThalassacean : MonoBehaviour {
	    	
	    	public static readonly string MOUTH_NAME = "Mouth"; //already has one
			
			public static readonly float REGROW_TIME = 3600; //60 min, but do not serialize, so will reset if leave and come back
			
			internal float lastCollect = -9999;
	    	
	    	private GameObject mouthInteract;
	    	
	    	private GameObject mouthItem;
	    	
	    	void Start() {
	    		mouthInteract = ObjectUtil.getChildObject(gameObject, MOUTH_NAME);
	    	}
			
			void Update() {
	    		if (!mouthInteract)
	    			mouthInteract = ObjectUtil.getChildObject(gameObject, MOUTH_NAME);
	    		
				bool act = DayNightCycle.main.timePassedAsFloat-lastCollect >= REGROW_TIME;
				//mouthInteract.SetActive(act);
				if (act && mouthInteract && (!mouthItem || !mouthItem.activeInHierarchy || mouthItem.transform.parent != mouthInteract.transform)) {
					mouthItem = ObjectUtil.createWorldObject(instance.thalassaceanCud.ClassID);
					mouthItem.SetActive(true);
	    			mouthItem.transform.SetParent(mouthInteract.transform);
				}
				if (mouthItem)
	    			mouthItem.transform.localPosition = new Vector3(0, 0, -0.5F);
			}
			/*
			public bool collect() {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time-lastCollect < REGROW_TIME)
					return false;
				InventoryUtil.addItem(instance.thalassaceanCud.TechType);
				lastCollect = time;
				return true;
			}*/
	    	
	    }
	    /*
	    internal class C2CThalassaceanMouthTag : MonoBehaviour, IHandTarget {
	    	
	    	private SphereCollider interact;			
			private C2CThalassacean owner;
	    	
	    	void Start() {
	    		interact = gameObject.EnsureComponent<SphereCollider>();
	    		interact.radius = 0.5F;
				owner = gameObject.FindAncestor<C2CThalassacean>();
	    	}
			
			public void OnHandHover(GUIHand hand) {
				HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
				HandReticle.main.SetInteractText("ThalassaceanMouthClick");
				HandReticle.main.SetTargetDistance(8);
			}
		
			public void OnHandClick(GUIHand hand) {
				owner.collect();
			}
	    	
	    }*/
	    internal class C2CThalassaceanCudTag : MonoBehaviour {
	    	
	    	void Start() {
	    		Invoke("setupRenderer", 0.5F);
	    	}
	    	
	    	public void setupRenderer() {
	    		setupRenderer(this);
	    	}
	    	
	    	public static void setupRenderer(Component c) {
				GameObject root = c.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
				GasPod gp = root.GetComponent<GasPod>();
				ObjectUtil.removeComponent<UWE.TriggerStayTracker>(root);
				ObjectUtil.removeComponent<FMOD_StudioEventEmitter>(root);
				ObjectUtil.removeComponent<ResourceTracker>(root);
				GameObject pfb = ObjectUtil.lookupPrefab("505e7eff-46b3-4ad2-84e1-0fadb7be306c");
				GameObject mdl = UnityEngine.Object.Instantiate(pfb.GetComponentInChildren<Animator>().gameObject);
				ObjectUtil.removeChildObject(mdl, "root", false);
				mdl.transform.SetParent(gp.model.transform.parent);
				mdl.transform.localPosition = gp.model.transform.localPosition;
				UnityEngine.Object.Destroy(gp.model);
				UnityEngine.Object.Destroy(gp);
				Renderer r = root.GetComponentInChildren<Renderer>();
				Color clr = new Color(0.67F, 0.95F, 0.2F, 0.5F);//new Color(0.4F, 0.3F, 0.1F);
				Animator a = root.GetComponentInChildren<Animator>();
				a.transform.localScale = Vector3.one*2;
				a.speed = 0.5F;
				r.materials[0].SetColor("_Color", clr);
				r.materials[0].SetColor("_SpecColor", clr);
				r.materials[0].SetFloat("_Fresnel", 0.5F);
				r.materials[0].SetFloat("_Shininess", 0F);
				r.materials[0].SetFloat("_SpecInt", 0.75F);
				r.materials[0].SetFloat("_EmissionLM", 15F);
				r.materials[0].SetFloat("_EmissionLMNight", 15F);
				r.materials[0].SetFloat("_MyCullVariable", 1.6F);
				root.GetComponent<SphereCollider>().radius = 0.7F;
	    	}
	    	
	    }
		
	}
	
}
