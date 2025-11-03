using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using ECCLibrary;

using FMOD;

using FMODUnity;

using HarmonyLib;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
//using DeExtinctionMod;
//using DeExtinctionMod.Prefabs.Creatures;

using Story;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class DEIntegrationSystem {

		public static readonly DEIntegrationSystem instance = new DEIntegrationSystem();

		private readonly bool isDeELoaded;

		private readonly HashSet<TechType> creatures = new HashSet<TechType>();
		private readonly HashSet<TechType> eggs = new HashSet<TechType>();
		private TechType thelassaceanType;
		private TechType lrThelassaceanType;
		private TechType jellySpinnerType;
		private TechType rubyPincherType;
		private TechType gulperType;
		private TechType axetailType;
		private TechType filtorbType;

		private CreatureAsset voidThelassacean;

		public bool spawnVoidThalaAnywhere;
		public int maxVoidThala = 12;

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

		public TechType getRubyPincher() {
			return rubyPincherType;
		}

		public TechType getGulper() {
			return gulperType;
		}

		public TechType getFiltorb() {
			return filtorbType;
		}

		public CreatureAsset getVoidThalassacean() {
			return voidThelassacean;
		}

		internal void applyPatches() {
			if (isDeELoaded)
				this.doApplyPatches();
		}

		private void doApplyPatches() {
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);

			thalassaceanCud = new WorldCollectedItem(SeaToSeaMod.itemLocale.getEntry("ThalassaceanCud"), "bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782");
			thalassaceanCud.renderModify = (r) => {
				C2CThalassaceanCudTag.setupRenderer(r);
			};
			thalassaceanCud.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/ThalassaceanCud");
			thalassaceanCud.Patch();

			BioReactorHandler.SetBioReactorCharge(thalassaceanCud.TechType, BaseBioReactor.GetCharge(TechType.Hoopfish));

			thelassaceanType = this.findCreature("StellarThalassacean");
			lrThelassaceanType = this.findCreature("JasperThalassacean");

			jellySpinnerType = this.findCreature("JellySpinner", true);

			creatures.Add(thelassaceanType);
			creatures.Add(lrThelassaceanType);
			creatures.Add(jellySpinnerType);
			creatures.Add(this.findCreature("Twisteel"));
			gulperType = this.findCreature("GulperLeviathan");
			creatures.Add(gulperType);
			creatures.Add(this.findCreature("GulperLeviathanBaby"));
			creatures.Add(this.findCreature("GrandGlider"));
			axetailType = this.findCreature("Axetail", true);
			creatures.Add(axetailType);
			creatures.Add(this.findCreature("RibbonRay", true));
			filtorbType = this.findCreature("Filtorb", true);
			creatures.Add(filtorbType);
			creatures.Add(this.findCreature("TriangleFish", true));
			rubyPincherType = this.findCreature("RubyClownPincher", true);
			creatures.Add(rubyPincherType);
			creatures.Add(this.findCreature("SapphireClownPincher", true));
			creatures.Add(this.findCreature("EmeraldClownPincher", true));
			creatures.Add(this.findCreature("AmberClownPincher", true));
			creatures.Add(this.findCreature("CitrineClownPincher", true));

			eggs.Add(this.findCreature("GrandGliderEgg"));
			eggs.Add(this.findCreature("StellarThalassaceanEgg"));
			eggs.Add(this.findCreature("JasperThalassaceanEgg"));
			eggs.Add(this.findCreature("TwisteelEgg"));
			eggs.Add(this.findCreature("GulperEgg"));

			RecipeUtil.addIngredient(C2CItems.powerSeal.TechType, thalassaceanCud.TechType, 4);
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.HeatSealant).TechType, thalassaceanCud.TechType, 2);
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.SealFabric).TechType, thalassaceanCud.TechType, 1);
			RecipeUtil.addIngredient(C2CItems.depth1300.TechType, thalassaceanCud.TechType, 4);
			RecipeUtil.addIngredient(C2CItems.bandage.TechType, thalassaceanCud.TechType, 1);

			int amt = RecipeUtil.removeIngredient(C2CItems.breathingFluid.TechType, TechType.Eyeye).amount;
			RecipeUtil.addIngredient(C2CItems.breathingFluid.TechType, jellySpinnerType, amt * 3 / 2); //from 2 to 3

			foreach (TechType tt in eggs) {
				CreatureEggAsset egg = (CreatureEggAsset)tt.getModPrefabByTechType();
				foreach (LootDistributionData.BiomeData bd in egg.BiomesToSpawnIn) {
					float f = bd.probability;
					f = Mathf.Min(f, 0.75F) * 0.67F;
					f = Mathf.Round(f * 20F) / 20F; //round to nearest 0.05
					f = Mathf.Max(f, 0.05F);
					SNUtil.log("Reducing spawn chance of " + egg.ClassID + " in " + Enum.GetName(typeof(BiomeType), bd.biome) + " from " + bd.probability + " to " + f);
					LootDistributionHandler.EditLootDistributionData(egg.ClassID, bd.biome, f, 1);
				}
			}
			Spawnable filtorb = (Spawnable)filtorbType.getModPrefabByTechType();
			foreach (LootDistributionData.BiomeData bd in filtorb.BiomesToSpawnIn) {
				float f = bd.probability;
				f = Mathf.Min(f, 0.8F) * 0.75F;
				f = Mathf.Round(f * 20F) / 20F; //round to nearest 0.05
				f = Mathf.Max(f, 0.05F);
				SNUtil.log("Reducing spawn chance of filtorb in " + Enum.GetName(typeof(BiomeType), bd.biome) + " from " + bd.probability + " to " + f);
				LootDistributionHandler.EditLootDistributionData(filtorb.ClassID, bd.biome, f, 1);
			}

			voidThelassacean = new VoidThalassacean(SeaToSeaMod.itemLocale.getEntry("VoidThalassacean"));
			voidThelassacean.Patch();

			FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(filtorbType, 4, "A water-rich organism with a defense mechanism against being grabbed");

			CraftDataHandler.SetItemSize(axetailType, new Vector2int(2, 1));
		}

		private TechType findCreature(string id, bool edible = false) {
			TechType tt = TechType.None;
			if (!TechTypeHandler.TryGetModdedTechType(id, out tt))
				if (!TechTypeHandler.TryGetModdedTechType(id.ToLowerInvariant(), out tt))
					TechTypeHandler.TryGetModdedTechType(id.setLeadingCase(false), out tt);
			if (tt == TechType.None)
				throw new Exception("Could not find DeE TechType for '" + id + "'");
			if (edible) {
				Campfire.addRecipe(tt, tt == axetailType ? 4 : 2, f => f.itemTemplate = TechType.CuredPeeper);
			}
			return tt;
		}

		[Obsolete("Unimplemented")]
		public void convertEgg(string type, float r) {
			foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(Player.main.transform.position, r)) {
				if (pi && pi.ClassId == type) {
					//TODO
				}
			}
		}

		public void tickVoidThalassaceanSpawner(Player ep) {
			if (spawnVoidThalaAnywhere || (ep.transform.position.y >= -800 && VanillaBiomes.VOID.isInBiome(ep.transform.position))) {
				HashSet<VoidThalassaceanTag> has = WorldUtil.getObjectsNearWithComponent<VoidThalassaceanTag>(ep.transform.position, 200);
				if (has.Count < maxVoidThala) {
					for (int i = has.Count; i < maxVoidThala; i++) {
						Vector3 pos = MathUtil.getRandomPointAtSetDistance(ep.transform.position, 200);
						if (pos.y > -25)
							continue;
						if (spawnVoidThalaAnywhere || VanillaBiomes.VOID.isInBiome(pos)) {
							GameObject go = ObjectUtil.createWorldObject(voidThelassacean.ClassID);
							go.transform.position = pos;
							go.fullyEnable();
							//SNUtil.writeToChat("spawned void thalassacean at "+go.transform.position+" dist="+Vector3.Distance(pos, ep.transform.position));
						}
					}
				}
			}
		}

		internal class C2CGulper : MonoBehaviour { //stay out of my damn biome

			private SwimBehaviour swim;
			private LastTarget target;
			private Creature creature;

			private Vector3 leash = UnderwaterIslandsFloorBiome.biomeCenter.setY(-200);

			void Update() {
				if (!swim)
					swim = GetComponent<SwimBehaviour>();
				if (!creature)
					creature = GetComponent<Creature>();
				if (!target)
					target = GetComponent<LastTarget>();
				BiomeBase bb = BiomeBase.getBiome(transform.position);
				bool biome = bb == VanillaBiomes.UNDERISLANDS || bb == UnderwaterIslandsFloorBiome.instance;
				if (biome) {
					if (target && target.target && target.transform.position.y < -300)
						target.target = null;
					if (creature)
						creature.leashPosition = leash;
					if (transform.position.y < -300)
						swim.SwimTo(leash, 40);
				}
			}

		}

		internal class C2CThalassacean : MonoBehaviour {

			public static readonly string MOUTH_NAME = "Mouth"; //already has one

			public static readonly float REGROW_TIME = 3600; //60 min, but do not serialize, so will reset if leave and come back

			internal float lastCollect = -9999;

			private GameObject mouthInteract;

			private GameObject mouthItem;

			void Start() {
				mouthInteract = gameObject.getChildObject(MOUTH_NAME);
			}

			void Update() {
				if (!DayNightCycle.main)
					return;
				if (!mouthInteract)
					mouthInteract = gameObject.getChildObject(MOUTH_NAME);

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

			private float lastParentageCheck;

			void Start() {
				this.Invoke("setupRenderer", 0.5F);
			}

			public void setupRenderer() {
				setupRenderer(this);
			}

			public static void setupRenderer(Component c) {
				GameObject root = c.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
				GasPod gp = root.GetComponent<GasPod>();
				root.removeComponent<UWE.TriggerStayTracker>();
				root.removeComponent<FMOD_StudioEventEmitter>();
				root.removeComponent<ResourceTracker>();
				GameObject pfb = ObjectUtil.lookupPrefab("505e7eff-46b3-4ad2-84e1-0fadb7be306c");
				GameObject mdl = pfb.GetComponentInChildren<Animator>().gameObject.clone();
				mdl.removeChildObject("root", false);
				mdl.transform.SetParent(gp.model.transform.parent);
				mdl.transform.localPosition = gp.model.transform.localPosition;
				gp.model.destroy();
				gp.destroy();
				Renderer r = root.GetComponentInChildren<Renderer>();
				//SNUtil.log("Adjusting Thalassacean cud renderer "+r.gameObject.GetFullHierarchyPath());
				Color clr = new Color(0.67F, 0.95F, 0.2F, 0.5F);//new Color(0.4F, 0.3F, 0.1F);
				Animator a = root.GetComponentInChildren<Animator>();
				a.transform.localScale = Vector3.one * 2;
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

			void Update() {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time - lastParentageCheck >= 1) {
					lastParentageCheck = time;
					if (!gameObject.FindAncestor<Creature>())
						gameObject.destroy(false);
				}
			}

		}

	}

}
