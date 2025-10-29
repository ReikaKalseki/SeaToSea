using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;

using HarmonyLib;

using QModManager.API.ModLoading;

using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.Auroresource;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.Exscansion;
using ReikaKalseki.Reefbalance;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.SeaToSea {
	public static class C2CItems {

		public static SeamothVoidStealthModule voidStealth;
		public static CyclopsHeatModule cyclopsHeat;
		public static CyclopsStorageModule cyclopsStorage;
		public static SeamothDepthModule depth1300;
		public static SeamothPowerSealModule powerSeal;
		public static SeamothHeatSinkModule heatSinkModule;
		public static SeamothSpeedModule speedModule;
		public static VehicleLightModule lightModule;
		public static SeamothTetherModule tetherModule;

		public static SealedSuit sealSuit;
		public static SealedGloves sealGloves;
		public static AzuriteBattery t2Battery;

		public static RebreatherV2 rebreatherV2;
		public static LiquidTank liquidTank;
		//public static OxygeniteTank oxygeniteTank;

		public static ChargeFinRelay chargeFinRelay;

		public static BreathingFluid breathingFluid;
		public static SeamothHeatSink heatSink;
		public static CurativeBandage bandage;
		public static KharaaTreatment treatment;
		public static OxygeniteCharge oxygeniteCharge;

		public static AlkaliPlant alkali;
		public static VentKelp kelp;
		public static HealingFlower healFlower;
		public static MountainGlow mountainGlow;
		public static SanctuaryPlant sanctuaryPlant;

		public static TechType emperorRootCommon;
		public static readonly Dictionary<string, EmperorRoot> emperorRoots = new Dictionary<string, EmperorRoot>();
		//public static TechType postCoveTreeCommon;
		//public static readonly Dictionary<DecoPlants, PostCoveTree> postCoveTrees = new Dictionary<DecoPlants, PostCoveTree>();

		public static BrokenTablet brokenRedTablet;
		public static BrokenTablet brokenWhiteTablet;
		public static BrokenTablet brokenOrangeTablet;
		public static BrokenTablet brokenBlueTablet;

		public static DeepStalker deepStalker;
		public static SanctuaryJellyray sanctuaryray;
		public static PurpleHolefish purpleHolefish;
		public static PurpleBoomerang purpleBoomerang;
		public static PurpleHoopfish purpleHoopfish;
		public static VoltaicBladderfish voltaicBladderfish;
		//public static GiantRockGrub giantRockGrub;
		public static BloodKelpBroodmother broodmother;
		public static VoidSpikeLeviathan voidSpikeLevi;

		public static LargeOxygenite largeOxygenite;

		public static TechType brineCoral;
		public static WorldCollectedItem brineCoralPiece;
		public static EmperorRootOil emperorRootOil;
		public static WorldCollectedItem bkelpBumpWormItem;
		//public static WorldCollectedItem brineSalt;   
		//public static WorldCollectedItem wateryGel;   

		public static Bioprocessor processor;
		public static RebreatherRecharger rebreatherCharger;
		public static GeyserFilter geyserFilter;
		//public static IncubatorInjector incubatorInjector;

		public static TechCategory chemistryCategory;
		public static TechCategory ingotCategory;

		public static CraftTree.Type hatchingEnzymes;

		private static readonly Dictionary<TechType, IngotDefinition> ingots = new Dictionary<TechType, IngotDefinition>();
		private static readonly Dictionary<TechType, IngotDefinition> ingotsByUnpack = new Dictionary<TechType, IngotDefinition>();
		private static readonly Dictionary<TechType, TechType> brokenTablets = new Dictionary<TechType, TechType>();

		internal static void registerTabletTechKey(BrokenTablet tb) {
			brokenTablets[tb.TechType] = tb.tablet;
			brokenTablets[tb.tablet] = tb.tablet;
		}
		/*
         public static bool hasNoGasMask() {
             return Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0;
         }*/

		internal static void preAdd() {
			XMLLocale.LocaleEntry e = SeaToSeaMod.miscLocale.getEntry("CraftingNodes");
			chemistryCategory = TechCategoryHandler.Main.AddTechCategory("C2Chemistry", e.getString("chemistry"));
			TechCategoryHandler.Main.TryRegisterTechCategoryToTechGroup(TechGroup.Resources, chemistryCategory);
			CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "C2Chemistry", e.getString("chemistry"), TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/chemistry"), "Resources");

			ingotCategory = TechCategoryHandler.Main.AddTechCategory("C2CIngots", e.getString("ingots"));
			TechCategoryHandler.Main.TryRegisterTechCategoryToTechGroup(TechGroup.Resources, ingotCategory);
			CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "C2CIngots", e.getString("ingots"), TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/ingotmaking"), "Resources");
			CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "C2CIngots2", e.getString("ingotUnpack"), TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/ingotbreaking"), "Resources");

			CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Workbench, "C2CMedical", e.getString("medical"), TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/medical"));
			CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Workbench, "C2CHelmet", e.getString("helmet"), SpriteManager.Get(TechType.Rebreather));
			CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Workbench, "C2CModElectronics", e.getString("modelectric"), TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/CraftTab/modelectronic"));

			brokenRedTablet = new BrokenTablet(TechType.PrecursorKey_Red);
			brokenWhiteTablet = new BrokenTablet(TechType.PrecursorKey_White);
			brokenOrangeTablet = new BrokenTablet(TechType.PrecursorKey_Orange);
			brokenBlueTablet = new BrokenTablet(TechType.PrecursorKey_Blue);

			voidStealth = new SeamothVoidStealthModule();
			depth1300 = new SeamothDepthModule("SMDepth4", "Seamoth Depth Module MK4", "Increases crush depth to 1300m.", 1300);
			powerSeal = new SeamothPowerSealModule();
			heatSinkModule = new SeamothHeatSinkModule();
			speedModule = new SeamothSpeedModule();
			lightModule = new VehicleLightModule();
			tetherModule = new SeamothTetherModule();
			cyclopsHeat = new CyclopsHeatModule();
			cyclopsStorage = new CyclopsStorageModule();
			sealSuit = new SealedSuit();
			sealGloves = new SealedGloves();
			t2Battery = new AzuriteBattery();
			rebreatherV2 = new RebreatherV2();
			liquidTank = new LiquidTank();
			//oxygeniteTank = new OxygeniteTank();
			chargeFinRelay = new ChargeFinRelay();
			breathingFluid = new BreathingFluid();
			heatSink = new SeamothHeatSink();
			bandage = new CurativeBandage();
			treatment = new KharaaTreatment();
			oxygeniteCharge = new OxygeniteCharge();
			bkelpBumpWormItem = new WorldCollectedItem(SeaToSeaMod.itemLocale.getEntry("BKelpBumpWormItem"), "WorldEntities/Natural/StalkerTooth");
			bkelpBumpWormItem.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/BumpWormItem");
			bkelpBumpWormItem.inventorySize = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? new Vector2int(3, 2) : new Vector2int(2, 1);
			bkelpBumpWormItem.renderModify = (r) => {
				r.transform.localScale = new Vector3(8, 8, 6);
				r.materials[0].SetFloat("_Shininess", 0);
				r.materials[0].SetFloat("_SpecInt", 0.2F);
				r.materials[0].SetFloat("_Fresnel", 0F);
				RenderUtil.setEmissivity(r, 0.75F);
			};
			bkelpBumpWormItem.Patch();

			/*
            Color c = new Color(0.5F, 1.6F, 0.8F);
            brineSalt = new WorldCollectedItem(SeaToSeaMod.itemLocale.getEntry("BrineSalt"), "WorldEntities/Natural/salt");
            brineSalt.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/BrineSalt");
            brineSalt.renderModify = (r) => {
                r.materials[0].SetColor("_Color", c);
                r.materials[0].SetColor("_SpecColor", c);
            };
            brineSalt.Patch();

            c = new Color(0.5F, 0.8F, 1.6F);
            wateryGel = new WorldCollectedItem(SeaToSeaMod.itemLocale.getEntry("WateryGel"), "WorldEntities/Natural/salt");
            wateryGel.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/WateryGel");
            wateryGel.Patch();*/
		}

		internal static void addCraftingItems() {
			CraftingItems.addAll();

			largeOxygenite = new LargeOxygenite(SeaToSeaMod.itemLocale.getEntry("OXYGENITE"));
			largeOxygenite.Patch();
		}

		internal static void addCreatures() {
			deepStalker = new DeepStalker(SeaToSeaMod.itemLocale.getEntry("DeepStalker"));
			deepStalker.Patch();
			sanctuaryray = new SanctuaryJellyray(SeaToSeaMod.itemLocale.getEntry("SanctuaryJellyray"));
			sanctuaryray.Patch();
			purpleBoomerang = new PurpleBoomerang(SeaToSeaMod.itemLocale.getEntry("PurpleBoomerang"));
			purpleBoomerang.cookableIntoBase = 1;
			purpleBoomerang.Patch();
			purpleHolefish = new PurpleHolefish(SeaToSeaMod.itemLocale.getEntry("PurpleHolefish"));
			purpleHolefish.Patch();
			purpleHoopfish = new PurpleHoopfish(SeaToSeaMod.itemLocale.getEntry("PurpleHoopfish"));
			purpleHoopfish.cookableIntoBase = 1;
			purpleHoopfish.Patch();
			voltaicBladderfish = new VoltaicBladderfish(SeaToSeaMod.itemLocale.getEntry("VoltaicBladderfish"));
			voltaicBladderfish.Patch();
			//giantRockGrub = new GiantRockGrub(SeaToSeaMod.itemLocale.getEntry("GiantRockGrub"));
			//giantRockGrub.Patch();
			broodmother = new BloodKelpBroodmother(SeaToSeaMod.itemLocale.getEntry("BloodKelpBroodmother"));
			broodmother.Patch();

			WaterParkCreature.waterParkCreatureParameters[deepStalker.TechType] = SNUtil.getModifiedACUParams(TechType.Stalker, 1, 1, 1, 1.5F);
			WaterParkCreature.waterParkCreatureParameters[sanctuaryray.TechType] = SNUtil.getModifiedACUParams(TechType.Jellyray, 1, 1, 1, 1.25F);
			WaterParkCreature.waterParkCreatureParameters[purpleBoomerang.TechType] = SNUtil.getModifiedACUParams(TechType.Boomerang, 1, 1, 1, 0.67F);
			WaterParkCreature.waterParkCreatureParameters[purpleHoopfish.TechType] = SNUtil.getModifiedACUParams(TechType.Hoopfish, 1.2F, 1.2F, 1.2F, 1.25F);
			WaterParkCreature.waterParkCreatureParameters[purpleHolefish.TechType] = SNUtil.getModifiedACUParams(TechType.HoleFish, 4F, 4F, 4F, 3.0F);
			WaterParkCreature.waterParkCreatureParameters[voltaicBladderfish.TechType] = SNUtil.getModifiedACUParams(TechType.Bladderfish, 1, 1, 1, 1);

			voidSpikeLevi = new VoidSpikeLeviathan(SeaToSeaMod.itemLocale.getEntry("VoidSpikeLevi"));
			voidSpikeLevi.register();
		}

		internal static void addMainItems() {
			breathingFluid.Patch();
			heatSink.Patch();

			depth1300.preventNaturalUnlock();
			depth1300.Patch();

			powerSeal.preventNaturalUnlock();
			powerSeal.Patch();

			voidStealth.preventNaturalUnlock();
			voidStealth.Patch();

			heatSinkModule.preventNaturalUnlock();
			heatSinkModule.Patch();

			speedModule.preventNaturalUnlock();
			speedModule.Patch();

			lightModule.preventNaturalUnlock();
			lightModule.Patch();

			tetherModule.preventNaturalUnlock();
			tetherModule.Patch();

			cyclopsHeat.preventNaturalUnlock();
			cyclopsHeat.Patch();

			cyclopsStorage.preventNaturalUnlock();
			cyclopsStorage.Patch();

			sealGloves.Patch(); //has to be before suit since suit references this in craft
			sealSuit.Patch();

			t2Battery.Patch();

			rebreatherV2.Patch();

			liquidTank.Patch();
			//oxygeniteTank.Patch();

			chargeFinRelay.Patch();

			bandage.Patch();
			CraftData.useEatSound[bandage.TechType] = CraftData.useEatSound[TechType.FirstAidKit];
			treatment.Patch();
			CraftData.useEatSound[treatment.TechType] = CraftData.useEatSound[TechType.FirstAidKit];
			oxygeniteCharge.Patch();
			CraftData.useEatSound[oxygeniteCharge.TechType] = CraftData.pickupSoundList[TechType.HighCapacityTank];
		}

		internal static void addFlora() {
			alkali = new AlkaliPlant();
			alkali.Patch();
			XMLLocale.LocaleEntry e = SeaToSeaMod.itemLocale.getEntry(alkali.ClassID);
			alkali.addPDAEntry(e.pda, 3, e.getString("header"));
			SNUtil.log(" > " + alkali);
			GenUtil.registerPlantWorldgen(alkali, BiomeType.Mountains_IslandCaveFloor, 1, 1F);
			GenUtil.registerPlantWorldgen(alkali, BiomeType.Mountains_CaveFloor, 1, 0.5F);
			GenUtil.registerPlantWorldgen(alkali, BiomeType.Dunes_CaveFloor, 1, 0.5F);
			GenUtil.registerPlantWorldgen(alkali, BiomeType.KooshZone_CaveFloor, 1, 2F);
			GenUtil.registerPlantWorldgen(alkali, BiomeType.SeaTreaderPath_CaveFloor, 1, 1F);
			//GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.UnderwaterIslands_ValleyFloor, 1, 0.5F);

			kelp = new VentKelp();
			kelp.Patch();
			e = SeaToSeaMod.itemLocale.getEntry(kelp.ClassID);
			kelp.addPDAEntry(e.pda, 3, e.getString("header"));
			SNUtil.log(" > " + kelp);

			healFlower = new HealingFlower();
			healFlower.Patch();
			e = SeaToSeaMod.itemLocale.getEntry(healFlower.ClassID);
			healFlower.addPDAEntry(e.pda, 5, e.getString("header"));
			SNUtil.log(" > " + healFlower);
			GenUtil.registerPlantWorldgen(healFlower, BiomeType.GrassyPlateaus_CaveFloor, 1, 2.5F);

			mountainGlow = new MountainGlow();
			mountainGlow.Patch();
			e = SeaToSeaMod.itemLocale.getEntry(mountainGlow.ClassID);
			mountainGlow.addPDAEntry(e.pda, 8, e.getString("header"));
			SNUtil.log(" > " + mountainGlow);
			GenUtil.registerPrefabWorldgen(mountainGlow, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, BiomeType.Mountains_Grass, 1, 0.5F);
			GenUtil.registerPrefabWorldgen(mountainGlow, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, BiomeType.Mountains_Rock, 1, 0.1F);
			GenUtil.registerPrefabWorldgen(mountainGlow, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, BiomeType.Mountains_Sand, 1, 0.3F);

			sanctuaryPlant = new SanctuaryPlant();
			sanctuaryPlant.Patch();
			e = SeaToSeaMod.itemLocale.getEntry(sanctuaryPlant.ClassID);
			sanctuaryPlant.addPDAEntry(e.pda, 10, e.getString("header"));
			SNUtil.log(" > " + sanctuaryPlant);

			e = SeaToSeaMod.itemLocale.getEntry("BRINE_CORAL");
			brineCoral = SNUtil.addTechTypeToVanillaPrefabs(e, SeaToSeaMod.lrCoralClusters.ToArray());
			SNUtil.addPDAEntry(brineCoral, e.key, e.name, 3, e.getString("category"), e.pda, e.getString("header"));

			brineCoralPiece = new WorldCollectedItem(SeaToSeaMod.itemLocale.getEntry("BrineCoralPiece"), VanillaResources.TITANIUM.prefab);
			brineCoralPiece.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/BrineCoralPiece");
			brineCoralPiece.renderModify = (r) => {
				GameObject mdl = r.setModel(ObjectUtil.lookupPrefab("908d3f0e-04b9-42b4-80c8-a70624eb5455").getChildObject("lost_river_skull_coral_01"));
				//r = mdl.GetComponentInChildren<Renderer>();
				//RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/BrineCoralPiece"); //no such texture
			};
			brineCoralPiece.Patch();

			e = SeaToSeaMod.itemLocale.getEntry("EMPEROR_ROOT");
			foreach (string pfb in VanillaFlora.BLOOD_ROOT_FERTILE) {
				emperorRoots[pfb] = new EmperorRoot(e, pfb);
				emperorRoots[pfb].Patch();
			}
			emperorRootCommon = TechTypeHandler.AddTechType(SeaToSeaMod.modDLL, e.key, e.name, e.desc);
			SNUtil.addPDAEntry(emperorRootCommon, e.key, e.name, 5, e.getString("category"), e.pda, e.getString("header"));

			emperorRootOil = new EmperorRootOil(SeaToSeaMod.itemLocale.getEntry("EmperorRootOil"));
			emperorRootOil.Patch();

			/*
            e = SeaToSeaMod.itemLocale.getEntry("POST_COVE_TREE");
            foreach (DecoPlants pfb in PostCoveTree.templates.Keys) {
                postCoveTrees[pfb] = new PostCoveTree(e, pfb);
                postCoveTrees[pfb].Patch();
            }
            postCoveTreeCommon = TechTypeHandler.AddTechType(SeaToSeaMod.modDLL, e.key, e.name, e.desc);
            PostCoveTree.postRegister().encyclopedia = SNUtil.addPDAEntry(postCoveTreeCommon, e.key, e.name, 5, e.getString("category"), e.pda, e.getString("header")).id;
            */
			BioReactorHandler.Main.SetBioReactorCharge(alkali.seed.TechType, BaseBioReactor.GetCharge(TechType.RedBushSeed) * 1.5F);
			BioReactorHandler.Main.SetBioReactorCharge(kelp.seed.TechType, BaseBioReactor.GetCharge(TechType.BloodOil) * 0.8F);
			BioReactorHandler.Main.SetBioReactorCharge(healFlower.seed.TechType, BaseBioReactor.GetCharge(TechType.Peeper));
			BioReactorHandler.Main.SetBioReactorCharge(mountainGlow.seed.TechType, BaseBioReactor.GetCharge(TechType.Oculus) * 2F);
			BioReactorHandler.Main.SetBioReactorCharge(sanctuaryPlant.seed.TechType, BaseBioReactor.GetCharge(TechType.RedBasketPlantSeed) * 1.5F);
			BioReactorHandler.Main.SetBioReactorCharge(CraftingItems.getItem(CraftingItems.Items.AmoeboidSample).TechType, BaseBioReactor.GetCharge(TechType.CreepvinePiece));
			BioReactorHandler.Main.SetBioReactorCharge(CustomEgg.getEgg(deepStalker.TechType).TechType, BaseBioReactor.GetCharge(TechType.StalkerEgg) * 0.9F);
			BioReactorHandler.Main.SetBioReactorCharge(CustomEgg.getEgg(purpleHolefish.TechType).TechType, BaseBioReactor.GetCharge(TechType.GasopodEgg) * 1.5F);
			BioReactorHandler.Main.SetBioReactorCharge(emperorRootOil.TechType, BaseBioReactor.GetCharge(TechType.BloodOil) * 0.5F);
		}

		internal static void addTablets() {
			brokenBlueTablet.register();
			brokenRedTablet.register();
			brokenWhiteTablet.register();
			brokenOrangeTablet.register();
		}

		internal static void addMachines() {
			XMLLocale.LocaleEntry e = SeaToSeaMod.itemLocale.getEntry("bioprocessor");
			processor = new Bioprocessor(e);
			processor.Patch();
			SNUtil.log("Registered custom machine " + processor);
			processor.addPDAPage(e.pda, "Bioprocessor");
			processor.addFragments(4, 5, SeaToSeaMod.bioprocFragments);
			Bioprocessor.addRecipes();

			e = SeaToSeaMod.itemLocale.getEntry("rebreathercharger");
			rebreatherCharger = new RebreatherRecharger(e);
			rebreatherCharger.Patch();
			SNUtil.log("Registered custom machine " + rebreatherCharger);
			rebreatherCharger.addPDAPage(e.pda, "RebreatherCharger");
			rebreatherCharger.addFragments(4, 7.5F, SeaToSeaMod.rebreatherChargerFragments);

			e = SeaToSeaMod.itemLocale.getEntry("geyserfilter");
			geyserFilter = new GeyserFilter(e);
			geyserFilter.Patch();
			SNUtil.log("Registered custom machine " + geyserFilter);
			geyserFilter.addPDAPage(e.pda, "GeyserFilter");
			/*
            e = SeaToSeaMod.itemLocale.getEntry("incubatorinjector");
            incubatorInjector = new IncubatorInjector(e);
            incubatorInjector.Patch();
            SNUtil.log("Registered custom machine "+incubatorInjector);
            incubatorInjector.addPDAPage(e.pda, "??");*/
		}

		internal static void postAdd() {
			registerTabletTechKey(brokenBlueTablet);
			registerTabletTechKey(brokenOrangeTablet);
			registerTabletTechKey(brokenWhiteTablet);
			registerTabletTechKey(brokenRedTablet);

			BatteryCharger.compatibleTech.Add(t2Battery.TechType);

			//override first aid kit
			UsableItemRegistry.instance.addUsableItem(TechType.FirstAidKit, (s, go) => {
				if (C2CUtil.playerCanHeal() && !Player.main.GetComponent<HealingOverTime>() && Player.main.GetComponent<LiveMixin>().AddHealth(0.1F) > 0.05) {
					HealingOverTime ht = Player.main.gameObject.EnsureComponent<HealingOverTime>();
					ht.setValues(20, 20);
					ht.activate();
					return true;
				}
				return false;
			});
			UsableItemRegistry.instance.addUsableItem(bandage.TechType, (s, go) => {
				if (C2CUtil.playerCanHeal() && !Player.main.GetComponent<HealingOverTime>() && Player.main.GetComponent<LiveMixin>().AddHealth(0.1F) > 0.05) {
					HealingOverTime ht = Player.main.gameObject.EnsureComponent<HealingOverTime>();
					ht.setValues(50, 5);
					ht.activate();
					foreach (DamageOverTime dt in Player.main.gameObject.GetComponentsInChildren<DamageOverTime>()) {
						dt.damageRemaining = 0;
						dt.CancelInvoke("DoDamage");
						dt.destroy();
					}
					foreach (PlayerMovementSpeedModifier ds in Player.main.gameObject.GetComponents<PlayerMovementSpeedModifier>()) {
						if (ds.speedModifier < 1)
							ds.destroy();
					}
					Ecocean.FoodEffectSystem.instance.clearNegativeEffects();
					Player.main.gameObject.removeComponent<Drunk>();
					return true;
				}
				return false;
			});
			UsableItemRegistry.instance.addUsableItem(treatment.TechType, (s, go) => {
				float time = DayNightCycle.main.timePassedAsFloat;
				return LiquidBreathingSystem.instance.useKharaaTreatment();
			});
			UsableItemRegistry.instance.addUsableItem(CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).TechType, (s, go) => {
				float time = DayNightCycle.main.timePassedAsFloat;
				return LiquidBreathingSystem.instance.applyTemporaryKharaaTreatment();
			});
			UsableItemRegistry.instance.addUsableItem(oxygeniteCharge.TechType, (s, go) => {
				if (LiquidBreathingSystem.instance.hasLiquidBreathing())
					return false;
				InventoryItem ii = Inventory.main.equipment.GetItemInSlot("Tank");
				if (ii == null || !ii.item)
					return false;
				Oxygen ox = ii.item.GetComponent<Oxygen>();
				if (!ox)
					return false;
				float max = ox.oxygenCapacity*5;
				float has = ox.oxygenAvailable;
				if (has > max * 0.75F)
					return false;
				OxygenBoost ob = ox.gameObject.EnsureComponent<OxygenBoost>();
				ob.limit = max;
				ob.original = ox.oxygenCapacity;
				ob.oxygen = ox;
				ox.oxygenCapacity = max;
				ox.oxygenAvailable = max;
				return true;
			});

			IrreplaceableItemRegistry.instance.registerItem(CraftingItems.getItem(CraftingItems.Items.BrokenT2Battery));
			IrreplaceableItemRegistry.instance.registerItem(CraftingItems.getItem(CraftingItems.Items.DenseAzurite));
			IrreplaceableItemRegistry.instance.registerItem(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL));
			IrreplaceableItemRegistry.instance.registerItem(C2CItems.voidStealth, SeamothVoidStealthModule.lossData);
			IrreplaceableItemRegistry.instance.registerItem(TechType.PrecursorKey_Blue);
			IrreplaceableItemRegistry.instance.registerItem(TechType.PrecursorKey_Red);
		}

		internal static void onTechUnlocked(TechType tech) {
			if (DIHooks.getWorldAge() < 0.25F)
				return;
			if (brokenTablets.ContainsKey(tech))
				SNUtil.triggerTechPopup(brokenTablets[tech]);
			else if (tech == CraftingItems.getItem(CraftingItems.Items.BacterialSample).TechType || tech == CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType)
				SNUtil.triggerTechPopup(tech);
		}

		class OxygenBoost : MonoBehaviour {

			internal float limit;
			internal float original;
			internal Oxygen oxygen;

			void Update() {
				oxygen.oxygenCapacity = Mathf.Min(limit, oxygen.oxygenAvailable);
				if (oxygen.oxygenAvailable <= original) {
					oxygen.oxygenCapacity = original;
					this.destroy();
				}
			}

		}

		internal class IngotDefinition {

			internal readonly TechType material;
			internal readonly TechType ingot;
			internal readonly DuplicateRecipeDelegateWithRecipe unpackingRecipe;
			internal readonly int count;

			internal IngotDefinition(TechType mat, TechType ing, DuplicateRecipeDelegateWithRecipe unpack, int amt) {
				material = mat;
				ingot = ing;
				count = amt;
				unpackingRecipe = unpack;
			}

			public void pickupUnpacked() {
				for (int i = 0; i < count; i++) {
					InventoryUtil.addItem(material);
				}
			}

		}

		internal static void addIngot(TechType item, BasicCraftingItem ing, DuplicateRecipeDelegateWithRecipe unpack, int amt) {
			addIngot(item, ing.TechType, unpack, amt);
		}

		internal static void addIngot(TechType item, TechType ing, DuplicateRecipeDelegateWithRecipe unpack, int amt) {
			IngotDefinition id = new IngotDefinition(item, ing, unpack, amt);
			ingots[item] = id;
			ingotsByUnpack[unpack.TechType] = id;
		}

		internal static IngotDefinition getIngot(TechType item) {
			return ingots[item];
		}

		internal static IngotDefinition getIngotByUnpack(TechType item) {
			return ingotsByUnpack.ContainsKey(item) ? ingotsByUnpack[item] : null;
		}

		internal static List<IngotDefinition> getIngots() {
			return new List<IngotDefinition>(ingots.Values);
		}

		internal static void setChemistry(TechType item) {
			RecipeUtil.changeRecipePath(item, "Resources", "C2Chemistry");
			RecipeUtil.setItemCategory(item, TechGroup.Resources, chemistryCategory);
		}

		internal static void setModElectronics(TechType item) {
			RecipeUtil.changeRecipePath(item, CraftTree.Type.Workbench, "C2CModElectronics");
			RecipeUtil.setItemCategory(item, TechGroup.Workbench, TechCategory.Workbench);
		}

		public static bool hasSealedOrReinforcedSuit(out bool isSealed, out bool isReinf) {
			InventoryItem suit = Inventory.main.equipment.GetItemInSlot("Body");
			InventoryItem glove = Inventory.main.equipment.GetItemInSlot("Gloves");
			bool sealSuit = suit != null && suit.item.GetTechType() == C2CItems.sealSuit.TechType;
			bool reinfSuit = suit != null && suit.item.GetTechType() == TechType.ReinforcedDiveSuit;
			bool sealGlove = glove != null && glove.item.GetTechType() == C2CItems.sealGloves.TechType;
			bool reinfGlove = glove != null && glove.item.GetTechType() == TechType.ReinforcedGloves;
			isSealed = sealSuit && sealGlove;
			isReinf = reinfSuit && reinfGlove;
			return (sealSuit || reinfSuit) && (sealGlove || reinfGlove);
		}

	}
}
