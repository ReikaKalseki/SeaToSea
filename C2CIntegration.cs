using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Reflection;
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
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
    	
    	AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.POO_RATE, f => Mathf.Clamp(f, 0.25F, hard ? 3F : 4F));
    	
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
		};
		
	}
    
    public static void addPostCompat() {
		bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
		BioRecipe rec = Bioprocessor.getRecipe(TechType.SeaTreaderPoop);
		Bioprocessor.addRecipe(new TypeInput(AqueousEngineeringMod.poo), CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, rec.enzyCount, rec.processTime, rec.totalEnergyCost, rec.inputCount*4, rec.outputCount);
		
		ACUEcosystems.addPredatorType(SeaToSeaMod.deepStalker.TechType, 0.5F, 0.3F, true, BiomeRegions.RegionType.GrandReef);
		ACUCallbackSystem.addStalkerToy(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 3);
		
		RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.HeatSealant).TechType, EcoceanMod.glowOil.TechType, hard ? 3 : 2);
		RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, EcoceanMod.glowOil.TechType, hard ? 6 : 3);
		RecipeUtil.addIngredient(C2CItems.powerSeal.TechType, EcoceanMod.glowOil.TechType, hard ? 8 : 5);
		RecipeUtil.addIngredient(TechType.PrecursorKey_White, EcoceanMod.glowOil.TechType, hard ? 6 : 4);
		RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.RocketFuel).TechType, EcoceanMod.glowOil.TechType, 3);
		//SeaTreaderTunnelLocker.addItem(glowOil.TechType, 2);
			
		FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(EcoceanMod.glowOil.TechType, 3, "Oil containing frequency-discriminating chemoluminescent seeds");		
		FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(EcoceanMod.lavaShroom.seed.TechType, 2, "Flora adapted for and frequently becoming sources of extreme heat");
		
		SeaTreaderTunnelLocker.addItem(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1);
		
		RecipeUtil.addIngredient(EcoceanMod.planktonScoop.TechType, CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1);
		GenUtil.registerWorldgen(new PositionedPrefab(GenUtil.getOrCreateDatabox(EcoceanMod.planktonScoop.TechType).ClassID, new Vector3(332.93F, -277.64F, -1435.6F)));
		
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
    }

  }
}
