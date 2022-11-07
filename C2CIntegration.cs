﻿using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
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
    	
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.CHEAP_GLASS, false);
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.CHEAP_HUDCHIP, false);
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.CHEAP_SEABASE, true);
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.COMPACT_DECO, true);
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.COMPACT_KELP, true);
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.COMPACT_SEEDS, false);
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.REINF_GLASS, true);
    	ReefbalanceMod.config.attachOverride(RBConfig.ConfigEntries.LARGE_CYCLOCKER, true);
    	
    	AuroresourceMod.config.attachOverride(ARConfig.ConfigEntries.SPEED, f => Mathf.Clamp(f, 0.5F, 1F));
    	
    	AqueousEngineeringMod.config.attachOverride(AEConfig.ConfigEntries.POO_RATE, f => Mathf.Clamp(f, 0.25F, 4F));
    	
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.LEVISCAN, true);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.BASERANGE, 200);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.MAXRANGE, 600);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.RANGEAMT, 200);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.SPDAMT, 6);
    	ExscansionMod.config.attachOverride(ESConfig.ConfigEntries.BASESPEED, 18);
    	
    	EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.GLOWFIRERATE, f => Mathf.Clamp(f, 0.75F, 1F));
    	EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.GLOWLIFE, f => Mathf.Clamp(f, 0.5F, 2F));
    	EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.GLOWCOUNT, 3);
    	EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.BOMBDMG, f => Mathf.Clamp(f, 0.5F, 2F));
    	EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.ANCHORDMG, f => Mathf.Clamp(f, 0.25F, 1.5F));
    	EcoceanMod.config.attachOverride(ECConfig.ConfigEntries.BLOODDMG, f => Mathf.Clamp(f, 1F, 3F));
    }
    
    public static void addPostCompat() {
		Spawnable miniPoo = ItemRegistry.instance.getItem("MiniPoop");
		if (miniPoo != null)
			Bioprocessor.addRecipe(miniPoo.TechType, CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType, 1, 10, 6, 4);
		
		ACUEcosystems.addPredatorType(SeaToSeaMod.deepStalker.TechType, 0.5F, 0.3F, true, BiomeRegions.RegionType.GrandReef);
		
		Spawnable glowOil = ItemRegistry.instance.getItem("GlowOil");
		if (glowOil != null) {
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, glowOil.TechType, 2);
			RecipeUtil.addIngredient(C2CItems.cyclopsHeat.TechType, glowOil.TechType, 8);
			RecipeUtil.addIngredient(C2CItems.powerSeal.TechType, glowOil.TechType, 5);
			RecipeUtil.addIngredient(TechType.PrecursorKey_White, glowOil.TechType, 6);
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.RocketFuel).TechType, glowOil.TechType, 3);
			SeaTreaderTunnelLocker.addItem(glowOil.TechType, 2);
		}
		
		SeaTreaderTunnelLocker.addItem(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1);
    }

  }
}