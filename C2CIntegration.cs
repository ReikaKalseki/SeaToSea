//For data read/write methods
using System;
//For data read/write methods
using System.Collections.Generic;
//Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;
using System.Linq;
//Working with Lists and Collections
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.Auroresource;
//More advanced manipulation of lists/collections
using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.Exscansion;
using ReikaKalseki.Reefbalance;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public static class C2CIntegration {

		public static TechType seaVoyager;
		public static Type seaVoyagerComponent;

		public static void injectConfigValues() {
			ReefbalanceMod.config.load();
			AuroresourceMod.config.load();
			AqueousEngineeringMod.config.load();
			ExscansionMod.config.load();
			EcoceanMod.config.load();

			SNUtil.log("Overriding config entries in support mods", SeaToSeaMod.modDLL);
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);

			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.CHEAP_GLASS, false);
			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.CHEAP_HUDCHIP, false);
			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.CHEAP_SEABASE, true);
			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.COMPACT_DECO, true);
			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.COMPACT_KELP, true);
			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.COMPACT_SEEDS, false);
			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.REINF_GLASS, true);
			//ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.LARGE_CYCLOCKER, true);
			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.LANTERN_SPEED, hard ? 0.2F : 0.4F);
			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.NO_BUILDER_CLEAR, true);
			ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.URANPERROD, hard ? 4 : 3);

			AuroresourceMod.config.attachOverride(ARConfig.ConfigEntries.SPEED, f => Mathf.Clamp(f, 0.5F, 1F));
			AuroresourceMod.config.attachOverride(ARConfig.ConfigEntries.REENTRY_RATE, f => Mathf.Clamp(f, 0.5F, 2F));
			AuroresourceMod.config.attachOverride(ARConfig.ConfigEntries.REENTRY_WARNING, f => Mathf.Clamp(f, 0.5F, 4F));
			AuroresourceMod.config.attachOverride(ARConfig.ConfigEntries.GEYSER_RESOURCE_RATE, 1.5F);

			AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.POO_RATE, f => Mathf.Clamp(f, 0.25F, hard ? 3F : 4F));
			AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.ATPTAPRATE, f => hard ? 10 : 15);
			if (hard)
				AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.LEISUREDECO, f => Mathf.Max(f, 18));
			AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.PILLARHULL, f => hard ? 2 : 4);
			AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.PILLARLIM, f => Mathf.Clamp(f, 1, hard ? 1 : 2));
			AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.SLEEPMORALE, f => hard ? 10 : 20);

			ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.LEVISCAN, true);
			ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.RESSCAN, true);
			if (hard)
				ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.TOOTHSCAN, true);
			ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.BASERANGE, 200);
			ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.MAXRANGE, 600);
			ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.RANGEAMT, 200);
			ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.SPDAMT, 6);
			ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.BASESPEED, 18);
			ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.BASES, true);
			ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.ALIEN, true);

			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.GLOWFIRERATE, f => hard ? Mathf.Clamp(f, 0.33F, 0.67F) : Mathf.Clamp(f, 0.75F, 1F));
			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.GLOWLIFE, f => Mathf.Clamp(f, 0.5F, hard ? 1F : 2F));
			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.GLOWCOUNT, hard ? 2 : 3);
			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.BOMBDMG, f => Mathf.Clamp(f, 0.5F, 2F));
			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.ANCHORDMG, f => Mathf.Clamp(f, 0.25F, 1.5F));
			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.BLOODDMG, f => Mathf.Clamp(f, 1F, 3F));
			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.PLANKTONRATE, f => Mathf.Clamp(f, 2F, 4F));
			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.GLOBALCOMPASS, f => Mathf.Clamp(f, hard ? 0.75F : 0.25F, 1F));
			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.LEVIIMMUNE, f => hard ? 1 : 0.75F);
			EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.DEFENSECLAMP, f => hard ? 10 : 20F);
		}

		public static void injectLoad() {
			ReefbalanceMod.scanCountOverridesCalculation += map => {
				bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
				map[TechType.ConstructorFragment] = hard ? 12 : 8;
				map[TechType.LaserCutterFragment] = hard ? 10 : 5;
				map[TechType.WorkbenchFragment] = hard ? 8 : 4;
				map[TechType.SeaglideFragment] = hard ? 5 : 3;
				map[TechType.StasisRifleFragment] = hard ? 4 : 2;
				map[TechType.SeamothFragment] = hard ? 6 : 4; //normally 3

				map[TechType.BaseNuclearReactorFragment] = hard ? 6 : 4;
				map[TechType.BaseBioReactorFragment] = hard ? 6 : 4;
				map[TechType.MoonpoolFragment] = hard ? 6 : 4;
				if (hard)
					map[TechType.ScannerRoomFragment] = 5;
				map[TechType.BaseFiltrationMachineFragment] = hard ? 4 : 2;

				map[TechType.CyclopsHullFragment] = hard ? 6 : 4;
				map[TechType.CyclopsEngineFragment] = hard ? 6 : 4;
				map[TechType.CyclopsBridgeFragment] = hard ? 6 : 4;

				map[TechType.ExosuitDrillArmFragment] = hard ? 20 : 10; //these are EVERYWHERE
				map[TechType.ExosuitGrapplingArmFragment] = hard ? 12 : 6;
				map[TechType.ExosuitPropulsionArmFragment] = hard ? 12 : 6;
				map[TechType.ExosuitTorpedoArmFragment] = hard ? 12 : 6;

				seaVoyager = TechType.None;
				if (TechTypeHandler.TryGetModdedTechType("SeaVoyager", out seaVoyager)) {
					map[seaVoyager] = hard ? 18 : 12;
					seaVoyagerComponent = InstructionHandlers.getTypeBySimpleName("ShipMod.Ship.SeaVoyager");
				}
			};

			AuroresourceMod.detectorUnlock = TechType.None;
		}

		public static void prePostAdd() {
			BaseDrillableGrinder.uncraftingIngredientRatios[CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType] = 1;
			BaseDrillableGrinder.uncraftingIngredientRatios[CraftingItems.getItem(CraftingItems.Items.TraceMetals).TechType] = 1;
			BaseDrillableGrinder.uncraftingIngredientRatios[CraftingItems.getItem(CraftingItems.Items.GeyserMinerals).TechType] = 1;
			BaseDrillableGrinder.uncraftingIngredientRatios[CraftingItems.getItem(CraftingItems.Items.Electrolytes).TechType] = 1;
			BaseDrillableGrinder.uncraftingIngredientRatios[CraftingItems.getItem(CraftingItems.Items.Tungsten).TechType] = 1;
			BaseDrillableGrinder.uncraftingIngredientRatios[C2CItems.t2Battery.TechType] = 0;
			foreach (CustomMaterials.Materials m in Enum.GetValues(typeof(CustomMaterials.Materials))) {
				BaseDrillableGrinder.uncraftingIngredientRatios[CustomMaterials.getItem(m).TechType] = 1;
			}

			BaseDrillableGrinder.uncraftabilityFlags[CraftingItems.getItem(CraftingItems.Items.TraceMetals).TechType] = false;
			BaseDrillableGrinder.uncraftabilityFlags[CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).TechType] = false;
			BaseDrillableGrinder.uncraftabilityFlags[C2CItems.breathingFluid.TechType] = false;
		}

		public static void addPostCompat() {
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
			BioRecipe rec = Bioprocessor.getRecipe(TechType.SeaTreaderPoop);
			Bioprocessor.addRecipe(new TypeInput(AqueousEngineeringMod.poo), CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, rec.enzyCount, rec.processTime, rec.totalEnergyCost, rec.inputCount * 4, rec.outputCount);

			BiomeRegions.RegionType glassForest = new BiomeRegions.RegionType("GlassForest", UnderwaterIslandsFloorBiome.instance.displayName, 3.5F, 0F, 0.8F, 0.85F);
			//BiomeRegions.RegionType voidSpikes = new BiomeRegions.RegionType(VoidSpikesBiome.instance.displayName, 0.1F, 0.1F, 0.25F, 0.95F);
			BiomeRegions.RegionType sanctuary = new BiomeRegions.RegionType("Sanctuary", CrashZoneSanctuaryBiome.instance.displayName, 0.1F, 1.1F, 0.85F, 0.8F);
			ACUTheming.setFloorTexture(glassForest, TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/ACUFloor/GlassForest"));
			//ACUTheming.setFloorTexture(voidSpikes, TextureManager.getTexture(AqueousEngineeringMod.modDLL, "Textures/ACUFloor/VoidSpikes"));
			ACUTheming.setFloorTexture(sanctuary, TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/ACUFloor/Sanctuary"));
			ACUTheming.registerGrassProp(sanctuary, null, 25, 0.6F); //null is default texture
			ACUTheming.registerProp(sanctuary, SeaToSeaMod.crashSanctuaryFern.ClassID, 20, true, 0.7F);

			ACUEcosystems.addFood(new ACUEcosystems.PlantFood(C2CItems.alkali, 0.15F, BiomeRegions.Other));
			ACUEcosystems.addFood(new ACUEcosystems.PlantFood(C2CItems.healFlower, 0.15F, BiomeRegions.RedGrass));
			ACUEcosystems.addFood(new ACUEcosystems.PlantFood(C2CItems.kelp, 0.08F, glassForest));
			ACUEcosystems.addFood(new ACUEcosystems.PlantFood(C2CItems.mountainGlow, 0.3F, BiomeRegions.Other));
			ACUEcosystems.addFood(new ACUEcosystems.PlantFood(C2CItems.sanctuaryPlant, 0.2F, sanctuary));
			ACUEcosystems.addFood(new ACUEcosystems.AnimalFood(C2CItems.purpleBoomerang, ACUEcosystems.AnimalFood.calculateFoodValue(TechType.Boomerang), glassForest));
			ACUEcosystems.addFood(new ACUEcosystems.AnimalFood(C2CItems.purpleHoopfish, ACUEcosystems.AnimalFood.calculateFoodValue(TechType.Spinefish), glassForest));
			ACUEcosystems.ACUMetabolism met = ACUEcosystems.getMetabolismForAnimal(TechType.RabbitRay);
			ACUEcosystems.addPredatorType(C2CItems.purpleHolefish.TechType, met.relativeValue * 2F, met.metabolismPerSecond, met.normalizedPoopChance, false, glassForest);
			met = ACUEcosystems.getMetabolismForAnimal(TechType.Jellyray);
			ACUEcosystems.addPredatorType(C2CItems.sanctuaryray.TechType, met.relativeValue * 1.25F, met.metabolismPerSecond, met.normalizedPoopChance, false, sanctuary);
			met = ACUEcosystems.getMetabolismForAnimal(TechType.Stalker);
			ACUEcosystems.addPredatorType(C2CItems.deepStalker.TechType, met.relativeValue * 1.5F, met.metabolismPerSecond * 0.5F, met.normalizedPoopChance * 1.25F, true, BiomeRegions.GrandReef);
			ACUEcosystems.getMetabolismForAnimal(TechType.BoneShark).addBiome(glassForest);
			ACUEcosystems.getMetabolismForAnimal(TechType.Gasopod).addBiome(sanctuary);
			ACUEcosystems.getMetabolismForAnimal(TechType.Mesmer).addBiome(sanctuary);
			ACUEcosystems.getAnimalFood(TechType.Bladderfish).addBiome(sanctuary);
			ACUEcosystems.getAnimalFood(TechType.Boomerang).addBiome(sanctuary);
			ACUEcosystems.getAnimalFood(TechType.Hoopfish).addBiome(sanctuary);
			ACUEcosystems.getPlantFood(VanillaFlora.SPOTTED_DOCKLEAF.getPrefabID()).addBiome(sanctuary);
			ACUEcosystems.getPlantFood(VanillaFlora.PAPYRUS.getPrefabID()).addBiome(sanctuary);
			//ACUEcosystems.getAnimalFood(TechType.CaveCrawler).addBiome(sanctuary);

			ACUCallbackSystem.addStalkerToy(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1.0F);

			if (hard)
				RecipeUtil.addIngredient(TechType.RocketStage3, AqueousEngineeringMod.ionRod.TechType, RecipeUtil.removeIngredient(TechType.RocketStage3, TechType.ReactorRod).amount);

			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.HeatSealant).TechType, EcoceanMod.glowOil.TechType, hard ? 3 : 2);
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, EcoceanMod.glowOil.TechType, hard ? 6 : 3);
			RecipeUtil.addIngredient(C2CItems.powerSeal.TechType, EcoceanMod.glowOil.TechType, hard ? 8 : 5);
			RecipeUtil.addIngredient(TechType.PrecursorKey_White, EcoceanMod.glowOil.TechType, hard ? 6 : 4);
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.RocketFuel).TechType, EcoceanMod.glowOil.TechType, 3);
			//SeaTreaderTunnelLocker.addItem(glowOil.TechType, 2);

			//FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(EcoceanMod.glowOil.TechType, 3, "Oil containing frequency-discriminating chemoluminescent seeds");		
			FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(EcoceanMod.lavaShroom.seed.TechType, 2, "Flora adapted for and frequently becoming sources of extreme heat");
			//FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(EcoceanMod.mushroomStack.seed.TechType, 1, "Decorative, luminous flora ");
			FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(EcoceanMod.pinkLeaves.seed.TechType, 1, "A vibrantly pink plant almost wiped out by the aurora's impact");

			SeaTreaderTunnelLocker.addItem(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1);

			RecipeUtil.addIngredient(EcoceanMod.planktonScoop.TechType, CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1);
			GenUtil.registerWorldgen(new PositionedPrefab(GenUtil.getOrCreateDatabox(EcoceanMod.planktonScoop.TechType).ClassID, new Vector3(332.93F, -277.64F, -1435.6F)));

			RecipeUtil.addIngredient(AuroresourceMod.meteorDetector.TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, 1);
			RecipeUtil.addIngredient(AuroresourceMod.meteorDetector.TechType, CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType, 1);
			//RecipeUtil.addIngredient(AuroresourceMod.meteorDetector.TechType, TechType.MercuryOre, 2);

			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.BacterialSample).TechType, EcoceanMod.planktonItem.TechType, 1);
			RecipeUtil.addIngredient(C2CRecipes.getAlternateBacteria().TechType, EcoceanMod.planktonItem.TechType, 2);
			//RecipeUtil.addIngredient(TechType.Polyaniline, EcoceanMod.planktonItem.TechType, 2);

			C2CRecipes.replaceFiberMeshWithMicroFilter(AqueousEngineeringMod.acuCleanerBlock.TechType);
			C2CRecipes.replaceFiberMeshWithMicroFilter(AqueousEngineeringMod.acuBoosterBlock.TechType);
			C2CRecipes.replaceFiberMeshWithMicroFilter(AqueousEngineeringMod.planktonFeederBlock.TechType);
			C2CRecipes.replaceFiberMeshWithMicroFilter(EcoceanMod.planktonScoop.TechType);
			RecipeUtil.removeIngredient(EcoceanMod.planktonScoop.TechType, EcoceanMod.mushroomVaseStrand.seed.TechType); //since in the mesh
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.MicroFilter).TechType, EcoceanMod.mushroomVaseStrand.seed.TechType, 3);

			RecipeUtil.addIngredient(AqueousEngineeringMod.wirelessChargerBlock.TechType, CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 2);
			RecipeUtil.addIngredient(AqueousEngineeringMod.wirelessChargerBlock.TechType, CraftingItems.getItem(CraftingItems.Items.GeyserMinerals).TechType, 3);

			//GenUtil.registerWorldgen(new PositionedPrefab(ExscansionMod.alienBase.ClassID, new Vector3())); //step cave

			Spawnable baseglass = ItemRegistry.instance.getItem("BaseGlass");
			int amt = RecipeUtil.removeIngredient(TechType.BaseWaterPark, baseglass != null ? baseglass.TechType : TechType.Glass).amount;
			RecipeUtil.addIngredient(TechType.BaseWaterPark, TechType.EnameledGlass, amt);

			CustomEgg ghostRayEgg = CustomEgg.getEgg(TechType.GhostRayBlue);
			if (ghostRayEgg != null)
				FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(TechType.GhostRayBlue, 1, "A large poisonous herbivore adapted to deep water");
			CustomEgg blighterEgg = CustomEgg.getEgg(TechType.Blighter);
			if (blighterEgg != null)
				FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(TechType.Blighter, 2, "A small but aggressive carrion feeder, with limited visual sensation");

			EcoceanMod.planktonScoop.TechType.removeUnlockTrigger();
			AqueousEngineeringMod.wirelessChargerBlock.TechType.removeUnlockTrigger();
			AuroresourceMod.meteorDetector.TechType.removeUnlockTrigger();
			GenUtil.getOrCreateDatabox(AqueousEngineeringMod.wirelessChargerBlock.TechType); //needs to be created to be used at runtime

			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, new ItemDisplayRenderBehavior() {
				verticalOffset = 0.3F,
				getRenderObj = ItemDisplayRenderBehavior.getChildNamed("model/" + CraftingItems.LATHING_DRONE_RENDER_OBJ_NAME)
			});
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.CrystalLens).TechType, new ItemDisplayRenderBehavior() { verticalOffset = 0.2F });
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.RocketFuel).TechType, TechType.Benzene);
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType, TechType.HydrochloricAcid);
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.SulfurAcid).TechType, TechType.HydrochloricAcid);
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, TechType.Polyaniline);
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType, TechType.Polyaniline);
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType, TechType.Polyaniline);
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, TechType.Polyaniline);
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType, TechType.Polyaniline);
			ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, TechType.Polyaniline);

			ItemDisplay.setRendererBehavior(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, new ItemDisplayRenderBehavior() { verticalOffset = 0.2F });
			ItemDisplay.setRendererBehavior(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, new ItemDisplayRenderBehavior() {
				verticalOffset = 0.0F,
				rotationSpeedMultiplier = 1.5F
			});

			CompassDistortionSystem.instance.addRegionalDistortion(new CompassDistortionSystem.BiomeDistortion(UnderwaterIslandsFloorBiome.instance, 180F, 0.18F));
			CompassDistortionSystem.instance.addRegionalDistortion(new CompassDistortionSystem.ConditionalDistortion(pos => VoidSpikeLeviathanSystem.instance.isVoidFlashActive(true), 720F, 1.2F));

			FallingMaterialSystem.instance.clear();
			FallingMaterialSystem.instance.addMaterial(CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType, 100);
			GeyserMaterialSpawner.addBiomeRateMultiplier(UnderwaterIslandsFloorBiome.instance, 3);

			FoodEffectSystem.instance.addVomitingEffect(C2CItems.mountainGlow.seed.TechType, 60, 60, 8, 4F, 20);
			FoodEffectSystem.instance.addDamageOverTimeEffect(C2CItems.mountainGlow.seed.TechType, 50, 30, DamageType.Heat, SeaToSeaMod.itemLocale.getEntry(C2CItems.mountainGlow.ClassID).getField<string>("eateffect"));

			FoodEffectSystem.instance.addVomitingEffect(C2CItems.kelp.seed.TechType, 250, 250, 20, 1.5F, 2);
			FoodEffectSystem.instance.addPoisonEffect(C2CItems.kelp.seed.TechType, 250, 30);
			FoodEffectSystem.instance.addDamageOverTimeEffect(C2CItems.alkali.seed.TechType, 75, 30, DamageType.Acid, SeaToSeaMod.itemLocale.getEntry(C2CItems.alkali.ClassID).getField<string>("eateffect"));

			FoodEffectSystem.instance.addPoisonEffect(C2CItems.purpleHolefish.TechType, 60, 30);
			FoodEffectSystem.instance.addPoisonEffect(C2CItems.purpleBoomerang.TechType, 50, 30);
			FoodEffectSystem.instance.addPoisonEffect(C2CItems.purpleHoopfish.TechType, 80, 30);

			FoodEffectSystem.instance.addVomitingEffect(C2CItems.sanctuaryPlant.seed.TechType, 60, 40, 5, 2, 5);
			FoodEffectSystem.instance.addEffect(C2CItems.sanctuaryPlant.seed.TechType, (s, go) => PlayerMovementSpeedModifier.add(1.8F, 180), FoodEffectSystem.instance.getLocaleEntry("speed"));

			FoodEffectSystem.instance.addVomitingEffect(CraftingItems.getItem(CraftingItems.Items.AmoeboidSample).TechType, 100, 100, 20, 4, 10);

			FoodEffectSystem.instance.moraleCallback = MoraleSystem.instance.shiftMorale;

			MushroomVaseStrand.filterDrops.addEntry(CraftingItems.getItem(CraftingItems.Items.TraceMetals).TechType, 5);
			MushroomVaseStrand.filterDrops.addEntry(CraftingItems.getItem(CraftingItems.Items.Tungsten).TechType, 60);

			//GrowingPlantViabilityTracker.registerThresholds(C2CItems.mountainGlow.TechType, 3, false, 0.004F);

			BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.processor, 0, BaseRoomSpecializationSystem.RoomTypes.WORK, BaseRoomSpecializationSystem.RoomTypes.MECHANICAL);
			BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.rebreatherCharger, 0, BaseRoomSpecializationSystem.RoomTypes.MECHANICAL);
			BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.alkali, 0.1F);
			BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.healFlower, 0.1F);
			BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.kelp, 0.2F);
			BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.sanctuaryPlant, 1F);
			BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.mountainGlow, -0.125F);

			BaseRoomSpecializationSystem.instance.setDisplayValue(C2CItems.purpleBoomerang.TechType, 0.2F); //lava boomerang is 0.15
			BaseRoomSpecializationSystem.instance.setDisplayValue(C2CItems.purpleHoopfish.TechType, 0.2F); //hoopfish is 0.2

			BaseRoomSpecializationSystem.instance.setDisplayValue(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 1.25F);
			BaseRoomSpecializationSystem.instance.setDisplayValue(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 1.25F);
			BaseRoomSpecializationSystem.instance.setDisplayValue(CraftingItems.getItem(CraftingItems.Items.CrystalLens).TechType, 1.25F);
			BaseRoomSpecializationSystem.instance.setDisplayValue(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, 1.5F);
			BaseRoomSpecializationSystem.instance.setDisplayValue(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 1.5F);
			BaseRoomSpecializationSystem.instance.setDisplayValue(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 2F);

			foreach (C2CItems.IngotDefinition ingot in C2CItems.getIngots())
				BaseRoomSpecializationSystem.instance.setDisplayValue(ingot.ingot, BaseRoomSpecializationSystem.instance.getItemDecoValue(ingot.material) * ingot.count / 2F);

			MoraleSystem.instance.registerBiomeEffect(VoidSpikesBiome.instance, new MoraleSystem.AmbientMoraleInfluence(-5, -50, -1));
			MoraleSystem.instance.registerBiomeEffect(CrashZoneSanctuaryBiome.instance, new MoraleSystem.AmbientMoraleInfluence(10, 5, 5));

			foreach (string s in SeaToSeaMod.lrCoralClusters)
				LootDistributionHandler.EditLootDistributionData(s, BiomeType.ActiveLavaZone_Chamber_Ceiling, 0, 0);

			AuroresourceMod.dunesMeteor.addDrop(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 15);
			AuroresourceMod.lavaPitCenter.addDrop(CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).TechType, 40);

			Type t;
			if (TechTypeHandler.TryGetModdedTechType("ResourceMonitorBuildableSmall", out TechType tt)) {
				RecipeUtil.modifyIngredients(tt, cheapenResourceMonitor);
				RecipeUtil.addIngredient(tt, TechType.AluminumOxide, 1);
			}
			if (TechTypeHandler.TryGetModdedTechType("ResourceMonitorBuildableLarge", out tt)) {
				RecipeUtil.modifyIngredients(tt, cheapenResourceMonitor);
				RecipeUtil.addIngredient(tt, CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1);
				RecipeUtil.addIngredient(tt, TechType.AluminumOxide, 2);

				t = InstructionHandlers.getTypeBySimpleName("ResourceMonitor.Components.ResourceMonitorDisplay");
				FieldInfo fi = t.GetField("ITEMS_PER_PAGE", BindingFlags.Static | BindingFlags.NonPublic);
				fi.SetValue(null, 48); //originally 12 = 2x6, make 4x12

				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "CreateAndAddItemDisplay", SeaToSeaMod.modDLL, shrinkItemDisplay);

				t = t.Assembly.GetType("ResourceMonitor.Components.ResourceMonitorLogic");
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "TrackStorageContainer", SeaToSeaMod.modDLL, filterStorageContainerInteract);
			}

			//buggy with C2C apparently
			t = InstructionHandlers.getTypeBySimpleName("EasyCraft.Options");
			if (t != null) {
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "OnAutoCraftChanged", SeaToSeaMod.modDLL, codes => {
					codes[InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Ldarg_1)].opcode = OpCodes.Ldc_I4_0;
				});
				t = t.Assembly.GetType("EasyCraft.Main");
				object settingMain = t.GetProperty("Settings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
				settingMain.GetType().GetField("autoCraft", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(settingMain, false);
			}

			if (TechTypeHandler.TryGetModdedTechType("TechPistol", out tt)) {
				RecipeUtil.addIngredient(tt, CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 2);
				RecipeUtil.addIngredient(tt, CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 1);
				RecipeUtil.removeIngredient(tt, TechType.Battery);
				RecipeUtil.removeIngredient(tt, TechType.Lubricant);
				RecipeUtil.addIngredient(tt, C2CItems.t2Battery.TechType, 1);
				RecipeUtil.addIngredient(tt, CraftingItems.getItem(CraftingItems.Items.Electrolytes).TechType, 1);
			}
			ItemUnlockLegitimacySystem.instance.add("TechPistol", "TechPistol", () => Story.StoryGoalManager.main.completedGoals.Contains("Iridium"));

			if (TechTypeHandler.TryGetModdedTechType("SeamothDrillArmModule", out tt)) {
				RecipeUtil.addIngredient(tt, CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 4);
				RecipeUtil.addIngredient(tt, CraftingItems.getItem(CraftingItems.Items.HoneycombComposite).TechType, 2);
				RecipeUtil.addIngredient(tt, CraftingItems.getItem(CraftingItems.Items.SealFabric).TechType, 1);
			}
			if (TechTypeHandler.TryGetModdedTechType("SeamothPropulsionArmModule", out tt)) {
				RecipeUtil.addIngredient(tt, CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 2);
				RecipeUtil.addIngredient(tt, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, 1);
				RecipeUtil.addIngredient(tt, TechType.Polyaniline, 1);
				RecipeUtil.addIngredient(tt, CraftingItems.getItem(CraftingItems.Items.Electrolytes).TechType, 2);
				RecipeUtil.addIngredient(tt, TechType.MercuryOre, 2);
			}
			ItemUnlockLegitimacySystem.instance.add("SeamothArms", "SeamothDrillArmModule", () => Story.StoryGoalManager.main.completedGoals.Contains("Iridium"));
			//ItemUnlockLegitimacySystem.instance.add("SeamothArms", "SeamothPropulsionArmModule", () => Story.StoryGoalManager.main.completedGoals.Contains(?));

			t = InstructionHandlers.getTypeBySimpleName("SlotExtender.Configuration.SEConfig");
			if (t != null) {
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "Config_Load", SeaToSeaMod.modDLL, codes => {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stsfld, "SlotExtender.Configuration.SEConfig", "MAXSLOTS");
					codes.InsertRange(idx, new InsnList() {
						new CodeInstruction(OpCodes.Pop),
						new CodeInstruction(OpCodes.Ldc_I4_5)
					});
				});
			}

			t = InstructionHandlers.getTypeBySimpleName("DeathRun.DeathRun");
			if (t != null)
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "Patch", SeaToSeaMod.modDLL, filterDeathrun);

			t = InstructionHandlers.getTypeBySimpleName("Agony.RadialTabs.GhostMoving");
			if (t != null) {
				InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "OnUpdate", SeaToSeaMod.modDLL, codes => {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, t.FullName, "_speed");
					codes.Insert(idx + 1, InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "getRadialTabAnimSpeed", false, new Type[] { typeof(float) }));
				});
			}

			FCSIntegrationSystem.instance.applyPatches();
			DEIntegrationSystem.instance.applyPatches();
			ItemUnlockLegitimacySystem.instance.applyPatches();

			if (seaVoyager != TechType.None) {
				RecipeUtil.modifyIngredients(seaVoyager, i => {
					if (i.techType == TechType.TitaniumIngot) {
						i.techType = TechType.PlasteelIngot;
					}
					else if (i.techType == TechType.Glass) {
						i.amount *= 3;
					}
					else if (i.techType == TechType.Lubricant) {
						i.techType = CraftingItems.getItem(CraftingItems.Items.Motor).TechType;
						i.amount *= 5;
					}
					else if (i.techType == TechType.WiringKit) {
						i.techType = TechType.AdvancedWiringKit;
						i.amount *= 2;
					}
					return false;
				});
			}
		}

		private static void filterDeathrun(InsnList codes) {
			for (int i = 0; i < codes.Count; i++) {
				CodeInstruction ci = codes[i];
				if (ci.opcode == OpCodes.Call) {
					MethodInfo call = (MethodInfo)ci.operand;
					if (call.Name == "SetTechData") {
						codes[i] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "mergeDeathrunRecipeChange", false, new Type[] {
							typeof(TechType),
							typeof(TechData)
						});
					}
					else
					if (call.Name == "EditFragmentsToScan") {
						codes[i] = InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "mergeDeathrunFragmentScanCount", false, new Type[] {
							typeof(TechType),
							typeof(int)
						});
					}
				}
			}
			//int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "SMLHelper.V2.Handler.CraftDataHandler", "SetTechData");
			//
		}

		private static void filterStorageContainerInteract(InsnList codes) {
			int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldloc_1);
			codes.InsertRange(idx + 1, new InsnList {
				new CodeInstruction(OpCodes.Ldarg_1),
				InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "isStorageVisibleToDisplayMonitor", false, new Type[] {
					typeof(bool),
					typeof(StorageContainer)
				})
			});
		}

		private static void shrinkItemDisplay(InsnList codes) {
			int idx = InstructionHandlers.getLastOpcodeBefore(codes, codes.Count, OpCodes.Ret);
			codes.InsertRange(idx, new InsnList {
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldloc_2),
				InstructionHandlers.createMethodCall("ReikaKalseki.SeaToSea.C2CHooks", "buildDisplayMonitorButton", false, new Type[] {
					typeof(MonoBehaviour),
					typeof(uGUI_ItemIcon)
				})
			});
		}

		private static bool cheapenResourceMonitor(Ingredient i) {
			if (i.techType != TechType.Glass && i.amount > 0)
				i.amount /= 2;
			if (i.techType == TechType.AdvancedWiringKit)
				i.techType = TechType.WiringKit;
			return false;
		}

	}
}
