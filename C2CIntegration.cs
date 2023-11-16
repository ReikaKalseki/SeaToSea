using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Reflection;
using System.Linq;   //More advanced manipulation of lists/collections
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;

using ReikaKalseki.Auroresource;
using ReikaKalseki.Reefbalance;
using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.Exscansion;
using ReikaKalseki.Ecocean;

namespace ReikaKalseki.SeaToSea {
	
  public static class C2CIntegration {
    
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
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.LARGE_CYCLOCKER, true);
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.LANTERN_SPEED, hard ? 0.2F : 0.4F);
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.NO_BUILDER_CLEAR, true);
    	
    	AuroresourceMod.config.attachOverride(ARConfig.ConfigEntries.SPEED, f => Mathf.Clamp(f, 0.5F, 1F));
    	AuroresourceMod.config.attachOverride(ARConfig.ConfigEntries.REENTRY_RATE, f => Mathf.Clamp(f, 0.5F, 2F));
    	AuroresourceMod.config.attachOverride(ARConfig.ConfigEntries.REENTRY_WARNING, f => Mathf.Clamp(f, 0.5F, 4F));
    	AuroresourceMod.config.attachOverride(ARConfig.ConfigEntries.GEYSER_RESOURCE_RATE, 0.5F);
    	
    	AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.POO_RATE, f => Mathf.Clamp(f, 0.25F, hard ? 3F : 4F));
    	AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.ATPTAPRATE, f => hard ? 10 : 15);
    	if (hard)
    		AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.LEISUREDECO, f => Mathf.Max(f, 18));
    	
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.LEVISCAN, true);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.RESSCAN, true);
    	if (hard)
    		ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.TOOTHSCAN, true);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.BASERANGE, 200);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.MAXRANGE, 600);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.RANGEAMT, 200);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.SPDAMT, 6);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.BASESPEED, 18);
    	
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
			
			TechType seaVoyager = TechType.None;
			if (TechTypeHandler.TryGetModdedTechType("SeaVoyager", out seaVoyager))
				map[seaVoyager] = hard ? 18 : 12;
		};
		
		AuroresourceMod.detectorUnlock = TechType.None;
	}
    
    public static void addPostCompat() {
		bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
		BioRecipe rec = Bioprocessor.getRecipe(TechType.SeaTreaderPoop);
		Bioprocessor.addRecipe(new TypeInput(AqueousEngineeringMod.poo), CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, rec.enzyCount, rec.processTime, rec.totalEnergyCost, rec.inputCount*4, rec.outputCount);
		
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
		ACUEcosystems.addFood(new ACUEcosystems.AnimalFood(SeaToSeaMod.purpleBoomerang, ACUEcosystems.AnimalFood.calculateFoodValue(TechType.Boomerang), glassForest));
		ACUEcosystems.addFood(new ACUEcosystems.AnimalFood(SeaToSeaMod.purpleHoopfish, ACUEcosystems.AnimalFood.calculateFoodValue(TechType.Spinefish), glassForest));
		ACUEcosystems.ACUMetabolism met = ACUEcosystems.getMetabolismForAnimal(TechType.RabbitRay);
		ACUEcosystems.addPredatorType(SeaToSeaMod.purpleHolefish.TechType, met.relativeValue*2F, met.metabolismPerSecond, met.normalizedPoopChance, false, glassForest);
		met = ACUEcosystems.getMetabolismForAnimal(TechType.Jellyray);
		ACUEcosystems.addPredatorType(SeaToSeaMod.sanctuaryray.TechType, met.relativeValue*1.25F, met.metabolismPerSecond, met.normalizedPoopChance, false, sanctuary);
		met = ACUEcosystems.getMetabolismForAnimal(TechType.Stalker);
		ACUEcosystems.addPredatorType(SeaToSeaMod.deepStalker.TechType, met.relativeValue*1.5F, met.metabolismPerSecond*0.5F, met.normalizedPoopChance*1.25F, true, BiomeRegions.GrandReef);
		ACUEcosystems.getMetabolismForAnimal(TechType.BoneShark).addBiome(glassForest);
		ACUEcosystems.getMetabolismForAnimal(TechType.Gasopod).addBiome(sanctuary);
		ACUEcosystems.getMetabolismForAnimal(TechType.Mesmer).addBiome(sanctuary);
		ACUEcosystems.getAnimalFood(TechType.Bladderfish).addBiome(sanctuary);
		ACUEcosystems.getAnimalFood(TechType.Boomerang).addBiome(sanctuary);
		ACUEcosystems.getAnimalFood(TechType.Hoopfish).addBiome(sanctuary);
		ACUEcosystems.getPlantFood(VanillaFlora.SPOTTED_DOCKLEAF.getPrefabID()).addBiome(sanctuary);
		ACUEcosystems.getPlantFood(VanillaFlora.PAPYRUS.getPrefabID()).addBiome(sanctuary);
		//ACUEcosystems.getAnimalFood(TechType.CaveCrawler).addBiome(sanctuary);
		
		ACUCallbackSystem.addStalkerToy(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 3);
		
		RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.HeatSealant).TechType, EcoceanMod.glowOil.TechType, hard ? 3 : 2);
		RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, EcoceanMod.glowOil.TechType, hard ? 6 : 3);
		RecipeUtil.addIngredient(C2CItems.powerSeal.TechType, EcoceanMod.glowOil.TechType, hard ? 8 : 5);
		RecipeUtil.addIngredient(TechType.PrecursorKey_White, EcoceanMod.glowOil.TechType, hard ? 6 : 4);
		RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.RocketFuel).TechType, EcoceanMod.glowOil.TechType, 3);
		//SeaTreaderTunnelLocker.addItem(glowOil.TechType, 2);
			
		FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(EcoceanMod.glowOil.TechType, 3, "Oil containing frequency-discriminating chemoluminescent seeds");		
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
		
		Spawnable baseglass = ItemRegistry.instance.getItem("BaseGlass");
		int amt = RecipeUtil.removeIngredient(TechType.BaseWaterPark, baseglass != null ? baseglass.TechType : TechType.Glass).amount;
		RecipeUtil.addIngredient(TechType.BaseWaterPark, TechType.EnameledGlass, amt);
		
		CustomEgg ghostRayEgg = CustomEgg.getEgg(TechType.GhostRayBlue);
		if (ghostRayEgg != null)
			FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(TechType.GhostRayBlue, 1, "A large poisonous herbivore adapted to deep water");
		CustomEgg blighterEgg = CustomEgg.getEgg(TechType.Blighter);
		if (blighterEgg != null)
			FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(TechType.Blighter, 2, "A small but aggressive carrion feeder, with limited visual sensation");
		
		C2CRecipes.removeVanillaUnlock(EcoceanMod.planktonScoop.TechType);
		
		C2CRecipes.removeVanillaUnlock(AuroresourceMod.meteorDetector.TechType);
		
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, new ItemDisplayRenderBehavior(){verticalOffset = 0.3F, getRenderObj = ItemDisplayRenderBehavior.getChildNamed("model/"+CraftingItems.LATHING_DRONE_RENDER_OBJ_NAME)});
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.CrystalLens).TechType, new ItemDisplayRenderBehavior(){verticalOffset = 0.2F});
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.RocketFuel).TechType, TechType.Benzene);
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType, TechType.HydrochloricAcid);
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.SulfurAcid).TechType, TechType.HydrochloricAcid);
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, TechType.Polyaniline);
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType, TechType.Polyaniline);
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.KelpEnzymes).TechType, TechType.Polyaniline);
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, TechType.Polyaniline);
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType, TechType.Polyaniline);
		ItemDisplay.setRendererBehavior(CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, TechType.Polyaniline);
		
		ItemDisplay.setRendererBehavior(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, new ItemDisplayRenderBehavior(){verticalOffset = 0.2F});
		ItemDisplay.setRendererBehavior(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, new ItemDisplayRenderBehavior(){verticalOffset = 0.0F, rotationSpeedMultiplier = 1.5F});
		
		CompassDistortionSystem.instance.addRegionalDistortion(new CompassDistortionSystem.BiomeDistortion(UnderwaterIslandsFloorBiome.instance, 180F, 0.18F));
		CompassDistortionSystem.instance.addRegionalDistortion(new CompassDistortionSystem.ConditionalDistortion(pos => VoidSpikeLeviathanSystem.instance.isVoidFlashActive(true), 720F, 1.2F));
		
		FallingMaterialSystem.instance.clear();
		FallingMaterialSystem.instance.addMaterial(CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType, 100);
		
		FoodEffectSystem.instance.addVomitingEffect(C2CItems.mountainGlow.seed.TechType, 60, 60, 8, 4F, 20);			
		FoodEffectSystem.instance.addDamageOverTimeEffect(C2CItems.mountainGlow.seed.TechType, 50, 30, DamageType.Heat, SeaToSeaMod.itemLocale.getEntry(C2CItems.mountainGlow.ClassID).getField<string>("eateffect"));
		
		FoodEffectSystem.instance.addVomitingEffect(C2CItems.kelp.seed.TechType, 250, 250, 20, 1.5F, 2);
		FoodEffectSystem.instance.addPoisonEffect(C2CItems.kelp.seed.TechType, 250, 30);
		FoodEffectSystem.instance.addDamageOverTimeEffect(C2CItems.alkali.seed.TechType, 75, 30, DamageType.Acid, SeaToSeaMod.itemLocale.getEntry(C2CItems.alkali.ClassID).getField<string>("eateffect"));
		
		FoodEffectSystem.instance.addPoisonEffect(SeaToSeaMod.purpleHolefish.TechType, 60, 30);
		FoodEffectSystem.instance.addPoisonEffect(SeaToSeaMod.purpleBoomerang.TechType, 50, 30);
		FoodEffectSystem.instance.addPoisonEffect(SeaToSeaMod.purpleHoopfish.TechType, 80, 30);
		
		FoodEffectSystem.instance.addVomitingEffect(C2CItems.sanctuaryPlant.seed.TechType, 60, 40, 5, 2, 5);
		FoodEffectSystem.instance.addEffect(C2CItems.sanctuaryPlant.seed.TechType, (s, go) => PlayerMovementSpeedModifier.add(1.8F, 180), FoodEffectSystem.instance.getLocaleEntry("speed"));
		
		FoodEffectSystem.instance.addVomitingEffect(CraftingItems.getItem(CraftingItems.Items.AmoeboidSample).TechType, 100, 100, 20, 4, 10);
		
		BaseRoomSpecializationSystem.instance.registerModdedObject(SeaToSeaMod.processor, 0, BaseRoomSpecializationSystem.RoomTypes.WORK, BaseRoomSpecializationSystem.RoomTypes.MECHANICAL);
		BaseRoomSpecializationSystem.instance.registerModdedObject(SeaToSeaMod.rebreatherCharger, 0, BaseRoomSpecializationSystem.RoomTypes.MECHANICAL);
		BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.alkali, 0.1F);
		BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.healFlower, 0.1F);
		BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.kelp, 0.2F);
		BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.sanctuaryPlant, 1F);
		BaseRoomSpecializationSystem.instance.registerModdedObject(C2CItems.mountainGlow, -0.25F);
		
		BaseRoomSpecializationSystem.instance.setDisplayValue(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 1.25F);
		BaseRoomSpecializationSystem.instance.setDisplayValue(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 1.25F);
		BaseRoomSpecializationSystem.instance.setDisplayValue(CraftingItems.getItem(CraftingItems.Items.CrystalLens).TechType, 1.25F);
		BaseRoomSpecializationSystem.instance.setDisplayValue(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, 1.5F);
		BaseRoomSpecializationSystem.instance.setDisplayValue(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 1.5F);
		BaseRoomSpecializationSystem.instance.setDisplayValue(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 2F);
		
		FCSIntegrationSystem.instance.applyPatches();
    }

  }
}
