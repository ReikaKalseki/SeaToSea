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

namespace ReikaKalseki.SeaToSea
{
  public static class C2CRecipes {	  
    
    private static DuplicateRecipeDelegateWithRecipe quartzIngotToGlass;    
    private static DuplicateRecipeDelegateWithRecipe enzymeAlternate;
    
    internal static void addItemsAndRecipes() {
       	TechData rec = new TechData();
      	rec.Ingredients.Add(new Ingredient(TechType.TitaniumIngot, 1));
       	DuplicateRecipeDelegateWithRecipe item = new DuplicateRecipeDelegateWithRecipe(TechType.Titanium, rec);
       	item.craftTime = 3;
       	item.craftingType = CraftTree.Type.Fabricator;
       	//item.category = TechCategory.BasicMaterials;
       	//item.group = TechGroup.Resources;
       	item.craftingMenuTree = new string[]{"Resources", "C2CIngots2"};
       	item.ownerMod = SeaToSeaMod.modDLL;
       	item.setRecipe(10);
       	item.Patch();
       	
       	C2CItems.addIngot(TechType.Titanium, TechType.TitaniumIngot, item, 10);
       	
       	createCompressedIngot(TechType.Quartz, 5, "Boule");
       	createCompressedIngot(TechType.AluminumOxide, "Ruby", 8, "Boule");
       	createCompressedIngot(TechType.Copper);
       	createCompressedIngot(TechType.Silver);
       	createCompressedIngot(TechType.Gold);
       	createCompressedIngot(TechType.Lead);
       	createCompressedIngot(TechType.Lithium, 10, "Plate");
       	createCompressedIngot(TechType.Magnetite, 6, "Bar");
       	createCompressedIngot(TechType.Nickel);
       	createCompressedIngot(TechType.Kyanite, 6, "Boule");
       	createCompressedIngot(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM));
       	createCompressedIngot(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 8);
       	
       	C2CItems.IngotDefinition qi = C2CItems.getIngot(TechType.Quartz);
       	TechData glassRec = RecipeUtil.getRecipe(TechType.Glass);
       	rec = new TechData();
       	rec.Ingredients.Add(new Ingredient(qi.ingot, glassRec.Ingredients[0].amount));
       	quartzIngotToGlass = new DuplicateRecipeDelegateWithRecipe(TechType.Glass, rec);
       	quartzIngotToGlass.setRecipe(qi.count);
       	CraftData.GetCraftTime(TechType.Glass, out quartzIngotToGlass.craftTime);
       	quartzIngotToGlass.craftTime *= qi.count;
       	quartzIngotToGlass.craftingType = CraftTree.Type.Fabricator;
       	quartzIngotToGlass.category = C2CItems.ingotCategory;
       	quartzIngotToGlass.group = TechGroup.Resources;
       	quartzIngotToGlass.unlock = TechType.Unobtanium;
       	quartzIngotToGlass.craftingMenuTree = new string[]{"Resources", "C2CIngots2"};
    	quartzIngotToGlass.sprite = SpriteManager.Get(TechType.Glass);
    	quartzIngotToGlass.ownerMod = SeaToSeaMod.modDLL;
    	quartzIngotToGlass.Patch();
       
        BasicCraftingItem enzyT = CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes);
        enzyT.craftingTime = 2;
        enzyT.addIngredient(TechType.SeaTreaderPoop, 1);
       
        int kelpamt = 2;
        BasicCraftingItem enzyK = CraftingItems.getItem(CraftingItems.Items.KelpEnzymes);
        enzyK.craftingTime = 3;
        enzyK.addIngredient(C2CItems.kelp.seed.TechType, kelpamt);
       
        BasicCraftingItem enzy = CraftingItems.getItem(CraftingItems.Items.BioEnzymes);
        enzy.craftingTime = 4;
        enzy.numberCrafted = 4;
        enzy.addIngredient(TechType.Salt, 1).addIngredient(enzyT, 1).addIngredient(TechType.SeaCrownSeed, 2).addIngredient(TechType.DisinfectedWater, 1);
       
        BasicCraftingItem comb = CraftingItems.getItem(CraftingItems.Items.HoneycombComposite);
        comb.craftingTime = 12;
        comb.addIngredient(TechType.AramidFibers, 3).addIngredient(TechType.PlasteelIngot, 1);
        
        BasicCraftingItem gem = CraftingItems.getItem(CraftingItems.Items.DenseAzurite);
        gem.craftingTime = 4;
        gem.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL), 9).addIngredient(TechType.Diamond, 1).addIngredient(TechType.Magnetite, 5);
        
        BasicCraftingItem lens = CraftingItems.getItem(CraftingItems.Items.CrystalLens);
        lens.craftingTime = 20;
        lens.addIngredient(gem, 5).addIngredient(TechType.TitaniumIngot, 2).addIngredient(TechType.AdvancedWiringKit, 1).addIngredient(TechType.FiberMesh, 4);
        
        BasicCraftingItem sealedFabric = CraftingItems.getItem(CraftingItems.Items.SealFabric);
        sealedFabric.craftingTime = 4;
        sealedFabric.numberCrafted = 2;
        sealedFabric.addIngredient(CraftingItems.getItem(CraftingItems.Items.Sealant), 3).addIngredient(TechType.AramidFibers, 1).addIngredient(TechType.StalkerTooth, 1).addIngredient(TechType.Silicone, 2);
        
        BasicCraftingItem armor = CraftingItems.getItem(CraftingItems.Items.HullPlating);
        armor.craftingTime = 9;
        armor.numberCrafted = 2;
        armor.addIngredient(TechType.PlasteelIngot, 2).addIngredient(TechType.Lead, 3).addIngredient(comb, 1).addIngredient(TechType.Nickel, 6);
        
        BasicCraftingItem acid = CraftingItems.getItem(CraftingItems.Items.WeakAcid);
        acid.craftingTime = 0.5F;
        acid.addIngredient(TechType.AcidMushroom, 4);
        
        BasicCraftingItem motor = CraftingItems.getItem(CraftingItems.Items.Motor);
        motor.craftingTime = 1;
        motor.addIngredient(TechType.CopperWire, 1).addIngredient(TechType.Titanium, 2).addIngredient(TechType.Lubricant, 1).addIngredient(TechType.Gold, 1);
        
        BasicCraftingItem drone = CraftingItems.getItem(CraftingItems.Items.LathingDrone);
        drone.craftingTime = 4;
        drone.addIngredient(motor, 1).addIngredient(TechType.Titanium, 1).addIngredient(TechType.ComputerChip, 1).addIngredient(TechType.PowerCell, 1);
        
        BasicCraftingItem chlorine = CraftingItems.getItem(CraftingItems.Items.Chlorine);
        chlorine.craftingTime = 3;
        chlorine.numberCrafted = 2;
        chlorine.addIngredient(TechType.Salt, 3).addIngredient(TechType.GasPod, 3);
        
        BasicCraftingItem tankWall = CraftingItems.getItem(CraftingItems.Items.FuelTankWall);
        tankWall.craftingTime = 2.5F;
        tankWall.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS), 9).addIngredient(sealedFabric, 2).addIngredient(CraftingItems.getItem(CraftingItems.Items.SmartPolymer), 1);
        
        BasicCraftingItem fuel = CraftingItems.getItem(CraftingItems.Items.RocketFuel);
        fuel.craftingTime = 6;
        fuel.addIngredient(TechType.Sulphur, 8).addIngredient(TechType.Kyanite, 2).addIngredient(TechType.PrecursorIonCrystal, 1);
        
        C2CItems.addCraftingItems();
        
        rec = RecipeUtil.copyRecipe(enzy.getRecipe());
        foreach (Ingredient i in rec.Ingredients) {
        	if (i.techType == TechType.DisinfectedWater) {
        		i.techType = TechType.BigFilteredWater;
        		i.amount *= 2;
        	}
        	else {
        		i.amount *= 3;
        	}
        }
       	enzymeAlternate = new DuplicateRecipeDelegateWithRecipe(enzy, rec);
       	enzymeAlternate.ownerMod = SeaToSeaMod.modDLL;
       	enzymeAlternate.craftTime = enzy.craftingTime*2F;
       	enzymeAlternate.setRecipe(enzy.numberCrafted*3);
       	enzymeAlternate.unlock = TechType.Unobtanium;
       	enzymeAlternate.Patch();
       	
        int s = 3;
        rec = new TechData();
        rec.Ingredients.Add(new Ingredient(C2CItems.kelp.seed.TechType, Mathf.CeilToInt(kelpamt*s*0.5F)));
      	rec.Ingredients.Add(new Ingredient(TechType.TreeMushroomPiece, 1));
       	item = new DuplicateRecipeDelegateWithRecipe(enzyK, rec);
       	item.setRecipe(enzyK.numberCrafted*s);
       	item.craftTime = enzyK.craftingTime*s/2F;
       	item.ownerMod = SeaToSeaMod.modDLL;
       	item.Patch();
       	
        rec = new TechData();
        rec.Ingredients.Add(new Ingredient(acid.TechType, 9));
      	rec.Ingredients.Add(new Ingredient(TechType.Gold, 2));
      	rec.Ingredients.Add(new Ingredient(TechType.SpottedLeavesPlantSeed, 4));
      	rec.Ingredients.Add(new Ingredient(TechType.Lubricant, 4));
       	item = new DuplicateRecipeDelegateWithRecipe(TechType.Polyaniline, rec);
       	item.setRecipe();
       	CraftData.GetCraftTime(TechType.Polyaniline, out item.craftTime);
       	item.craftTime *= 4;
       	item.category = C2CItems.chemistryCategory;
       	item.group = TechGroup.Resources;
       	item.craftingType = CraftTree.Type.Fabricator;
       	item.craftingMenuTree = new string[]{"Resources", "C2Chemistry"};
       	item.unlock = TechType.AcidMushroom;
       	//item.suffixName = " Traces";
       	item.ownerMod = SeaToSeaMod.modDLL;
       	item.Patch();
       	
       	/*
        CraftData.itemSizes[TechType.AcidMushroom] = new Vector2int(1, 2);
        CraftData.itemSizes[TechType.HydrochloricAcid] = new Vector2int(2, 2);
        RecipeUtil.modifyIngredients(TechType.HydrochloricAcid, i => i.amount = 12);
        */
        
        RecipeUtil.removeRecipe(TechType.HydrochloricAcid, true);
		RecipeUtil.removeRecipe(TechType.Benzene, true);
		C2CItems.setChemistry(TechType.Bleach);
		C2CItems.setChemistry(TechType.Polyaniline);
		C2CItems.setChemistry(TechType.HatchingEnzymes);
       	
		RecipeUtil.changeRecipePath(TechType.TitaniumIngot, "Resources", "C2CIngots");
		RecipeUtil.setItemCategory(TechType.TitaniumIngot, TechGroup.Resources, C2CItems.ingotCategory);
		//do not remove creepvine, as lubricant is needed earlier than this
		
        C2CItems.voidStealth.addIngredient(lens, 1).addIngredient(comb, 2).addIngredient(TechType.Aerogel, 12);
        C2CItems.depth1300.addIngredient(TechType.VehicleHullModule3, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS), 12).addIngredient(armor, 2);
        C2CItems.powerSeal.addIngredient(TechType.Aerogel, 1).addIngredient(TechType.Polyaniline, 3).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 6).addIngredient(CraftingItems.getItem(CraftingItems.Items.Sealant), 5);
        C2CItems.heatSinkModule.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 2).addIngredient(TechType.Pipe, 5).addIngredient(motor, 1).addIngredient(TechType.Benzene, 2);
        C2CItems.cyclopsHeat.addIngredient(TechType.CyclopsThermalReactorModule, 1).addIngredient(TechType.CyclopsFireSuppressionModule, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 12).addIngredient(CraftingItems.getItem(CraftingItems.Items.Sealant), 4);
        C2CItems.sealSuit.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 3).addIngredient(sealedFabric, 5).addIngredient(TechType.Titanium, 1).addIngredient(TechType.CrashPowder, 2);
        C2CItems.t2Battery.addIngredient(TechType.Battery, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.DenseAzurite), 1).addIngredient(TechType.Polyaniline, 1).addIngredient(TechType.MercuryOre, 2).addIngredient(TechType.Lithium, 2).addIngredient(TechType.Silicone, 1);
		C2CItems.rebreatherV2.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 4).addIngredient(sealedFabric, 3).addIngredient(TechType.Rebreather, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.Motor), 1).addIngredient(C2CItems.t2Battery, 1);
        C2CItems.liquidTank.addIngredient(TechType.HighCapacityTank, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.HoneycombComposite), 1).addIngredient(sealedFabric, 2);
        C2CItems.heatSink.addIngredient(TechType.Titanium, 4).addIngredient(TechType.CopperWire, 1).addIngredient(TechType.Lithium, 2);
        C2CItems.breathingFluid.addIngredient(TechType.Benzene, 1).addIngredient(TechType.MembrainTreeSeed, 2).addIngredient(TechType.Eyeye, 2).addIngredient(TechType.PurpleVasePlantSeed, 1).addIngredient(TechType.OrangeMushroomSpore, 1).addIngredient(TechType.SpottedLeavesPlantSeed, 2);
		C2CItems.bandage.addIngredient(TechType.FirstAidKit, 1).addIngredient(C2CItems.healFlower.seed.TechType, 3).addIngredient(TechType.JellyPlant, 1);
		C2CItems.addMainItems();
        /*
		rec = RecipeUtil.createUncrafting(t2Battery.TechType, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType);
      	rec.Ingredients.Add(new Ingredient(t2Battery.TechType, 1));
      	item = new DuplicateRecipeDelegateWithRecipe(CraftingItems.getItem(CraftingItems.Items.DenseAzurite), rec);
       	item.craftTime = 6;
       	item.craftingType = CraftTree.Type.Fabricator;
       	item.category = TechCategory.Electronics;
       	item.group = TechGroup.Resources;
       	item.craftingMenuTree = new string[]{"Resources", "Electronics"};
       	item.Patch();*/
		UncraftingRecipeItem t2un = new UncraftingRecipeItem(C2CItems.t2Battery);
       	t2un.craftTime = 6;
       	t2un.ownerMod = SeaToSeaMod.modDLL;
		t2un.Patch();
		//SurvivalHandler.GiveHealthOnConsume(bandage.TechType, 50, false);
        
        RecipeUtil.startLoggingRecipeChanges();
        
        RecipeUtil.modifyIngredients(TechType.Lubricant, i => {i.amount = 4; return false;});
        
        RecipeUtil.addIngredient(TechType.Rebreather, TechType.Titanium, 3);
        //RecipeUtil.addIngredient(TechType.Rebreather, TechType.AdvancedWiringKit, 1);
        RecipeUtil.addIngredient(TechType.Rebreather, TechType.EnameledGlass, 1);
       // RecipeUtil.removeIngredient(TechType.Rebreather, TechType.WiringKit);
        
        RecipeUtil.modifyIngredients(TechType.Constructor, i => i.techType != TechType.TitaniumIngot);
        RecipeUtil.addIngredient(TechType.Constructor, TechType.WiringKit, 1);
        RecipeUtil.addIngredient(TechType.Constructor, TechType.Silicone, 3);
        RecipeUtil.addIngredient(TechType.Constructor, drone.TechType, 4);
        
        //RecipeUtil.addIngredient(TechType.Polyaniline, TechType.Salt, 2);
        
        RecipeUtil.addIngredient(TechType.StasisRifle, CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 6);
        RecipeUtil.removeIngredient(TechType.StasisRifle, TechType.Battery);
        RecipeUtil.addIngredient(TechType.StasisRifle, C2CItems.t2Battery.TechType, 2);
        
        RecipeUtil.modifyIngredients(TechType.ReinforcedDiveSuit, i => {if (i.techType == TechType.Diamond) i.amount = 4; return i.techType == TechType.Titanium;});
        RecipeUtil.addIngredient(TechType.ReinforcedDiveSuit, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 9);
        RecipeUtil.addIngredient(TechType.ReinforcedDiveSuit, C2CItems.sealSuit.TechType, 1);
        
        RecipeUtil.modifyIngredients(TechType.AramidFibers, i => {if (i.techType == TechType.FiberMesh) i.amount = 2; return false;});
        
        RecipeUtil.modifyIngredients(TechType.PlasteelIngot, i => {if (i.techType == TechType.Lithium) i.amount = i.amount*3/2; return false;});
        
        RecipeUtil.removeIngredient(TechType.Battery, TechType.AcidMushroom);
        RecipeUtil.addIngredient(TechType.Battery, acid.TechType, 3);
        
        RecipeUtil.addIngredient(TechType.PrecursorIonBattery, TechType.Battery, 1);
        RecipeUtil.addIngredient(TechType.PrecursorIonBattery, CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 1);
        RecipeUtil.addIngredient(TechType.PrecursorIonBattery, CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 2);
        RecipeUtil.addIngredient(TechType.PrecursorIonPowerCell, CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 4);
        
        RecipeUtil.addIngredient(TechType.RocketBase, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 4);
        RecipeUtil.addIngredient(TechType.RocketBase, TechType.Silicone, 8);
        RecipeUtil.addIngredient(TechType.RocketBase, CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 4);
        RecipeUtil.modifyIngredients(TechType.RocketBase, i => {
      		if (i.techType == TechType.TitaniumIngot)
      			i.techType = TechType.PlasteelIngot;
      		else if (i.techType == TechType.Lead)
      			i.amount = 6;
      		return i.techType == TechType.ComputerChip;
        });
        RecipeUtil.addIngredient(TechType.RocketBaseLadder, TechType.WiringKit, 4);
        RecipeUtil.modifyIngredients(TechType.RocketStage1, i => i.techType != TechType.PlasteelIngot);
        RecipeUtil.addIngredient(TechType.RocketStage1, CustomMaterials.getIngot(CustomMaterials.Materials.IRIDIUM), 1);
        RecipeUtil.addIngredient(TechType.RocketStage1, CustomMaterials.getIngot(CustomMaterials.Materials.PLATINUM), 1);
        RecipeUtil.addIngredient(TechType.RocketStage1, TechType.CrashPowder, 3);
        RecipeUtil.addIngredient(TechType.RocketStage1, TechType.Diamond, 4);
        RecipeUtil.modifyIngredients(TechType.RocketStage2, i => i.techType == TechType.Kyanite || i.techType == TechType.Sulphur);
        RecipeUtil.addIngredient(TechType.RocketStage2, tankWall.TechType, 2);
        RecipeUtil.addIngredient(TechType.RocketStage2, fuel.TechType, 4);
        RecipeUtil.addIngredient(TechType.RocketStage2, CraftingItems.getItem(CraftingItems.Items.HoneycombComposite).TechType, 2);
        RecipeUtil.modifyIngredients(TechType.RocketStage3, i => {if (i.techType == TechType.EnameledGlass) i.amount = 8; return i.techType == TechType.ComputerChip;});
        RecipeUtil.addIngredient(TechType.RocketStage3, C2CItems.t2Battery.TechType, 1);
        RecipeUtil.addIngredient(TechType.RocketStage3, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 8);
        RecipeUtil.addIngredient(TechType.RocketStage3, TechType.AdvancedWiringKit, 3);
        RecipeUtil.addIngredient(TechType.RocketStage3, TechType.ReactorRod, 4);
        
        RecipeUtil.addIngredient(TechType.HighCapacityTank, TechType.Aerogel, 1);
        
        Dictionary<TechType, int> addMotors = new Dictionary<TechType, int>(){
        	{TechType.BaseMoonpool, 2},
        	{TechType.Seamoth, 2},
        	{TechType.Seaglide, 1},
        	{TechType.Cyclops, 4},
        	{TechType.PipeSurfaceFloater, 1},
        	{TechType.BasePipeConnector, 1},
        	{TechType.RocketBaseLadder, 1},
        	{TechType.VendingMachine, 1},
        	{TechType.ExosuitDrillArmModule, 2},
        	{TechType.Exosuit, 3},
        };
        
        foreach (KeyValuePair<TechType, int> kvp in addMotors) {
        	int amt = -1;
        	RecipeUtil.modifyIngredients(kvp.Key, i => {if (i.techType == TechType.Lubricant){amt = i.amount; return true;} else {return false;}});
        	RecipeUtil.addIngredient(kvp.Key, motor.TechType, Math.Max(kvp.Value, amt));
        }
        
        RecipeUtil.addIngredient(TechType.Cyclops, armor.TechType, 4);
        RecipeUtil.addIngredient(TechType.Exosuit, CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 4);
        
        RecipeUtil.removeIngredient(TechType.ExoHullModule1, TechType.PlasteelIngot);
        RecipeUtil.addIngredient(TechType.ExoHullModule1, TechType.Kyanite, 3);
        RecipeUtil.addIngredient(TechType.ExoHullModule1, armor.TechType, 2);
        RecipeUtil.addIngredient(TechType.ExoHullModule2, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 4);
        RecipeUtil.addIngredient(TechType.ExoHullModule2, CraftingItems.getItem(CraftingItems.Items.SmartPolymer).TechType, 3);
        RecipeUtil.removeIngredient(TechType.ExoHullModule2, TechType.Kyanite);
        RecipeUtil.removeIngredient(TechType.ExoHullModule2, TechType.Titanium);
        
        RecipeUtil.addIngredient(TechType.LaserCutter, TechType.AluminumOxide, 2);
        RecipeUtil.removeIngredient(TechType.LaserCutter, TechType.Battery);
        RecipeUtil.addIngredient(TechType.LaserCutter, C2CItems.t2Battery.TechType, 1);
        
        RecipeUtil.modifyIngredients(TechType.VehicleHullModule2, i => {if (i.techType == TechType.EnameledGlass || i.techType == TechType.Magnetite) i.amount *= 4; return false;});
        RecipeUtil.addIngredient(TechType.VehicleHullModule2, TechType.Silicone, 2);
        RecipeUtil.addIngredient(TechType.VehicleHullModule2, TechType.AdvancedWiringKit, 1);
        RecipeUtil.addIngredient(TechType.VehicleHullModule2, CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 4);
        //RecipeUtil.addIngredient(TechType.VehicleHullModule3, armor.TechType, 2);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.HoneycombComposite).TechType, 2);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 2);
        RecipeUtil.removeIngredient(TechType.VehicleHullModule3, TechType.PlasteelIngot);
        RecipeUtil.removeIngredient(TechType.VehicleHullModule3, TechType.AluminumOxide);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, TechType.Diamond, 4);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, TechType.Lubricant, 6);
        
        RecipeUtil.addIngredient(TechType.PrecursorKey_Blue, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, 1);
        
        RecipeUtil.modifyIngredients(TechType.EnameledGlass, i => {if (i.techType == TechType.Glass) i.amount *= 2; return false;});
       	RecipeUtil.getRecipe(TechType.EnameledGlass).craftAmount *= 2;
        RecipeUtil.addIngredient(TechType.EnameledGlass, TechType.Lead, 2);
        RecipeUtil.addIngredient(TechType.EnameledGlass, TechType.Diamond, 1);
        RecipeUtil.addIngredient(TechType.AdvancedWiringKit, TechType.MercuryOre, 1);
        
        RecipeUtil.modifyIngredients(TechType.AdvancedWiringKit, i => {if (i.techType == TechType.WiringKit) i.amount *= 2; return false;});
        RecipeUtil.modifyIngredients(TechType.WiringKit, i => {i.amount = 3; return false;});
        
        RecipeUtil.getRecipe(TechType.DisinfectedWater).craftAmount = 3;
        RecipeUtil.addIngredient(TechType.Bleach, chlorine.TechType, 1);
        RecipeUtil.addIngredient(TechType.BaseFiltrationMachine, TechType.Bleach, 2);
        RecipeUtil.addIngredient(TechType.BaseFiltrationMachine, TechType.AdvancedWiringKit, 1);
        RecipeUtil.removeIngredient(TechType.BaseFiltrationMachine, TechType.CopperWire);
        RecipeUtil.addIngredient(TechType.BaseFiltrationMachine, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 1);
        
        RecipeUtil.addRecipe(TechType.PrecursorKey_Red, TechGroup.Personal, TechCategory.Equipment, 1, CraftTree.Type.Fabricator, new string[]{"Personal", "Equipment"});
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.PrecursorIonCrystal, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.MercuryOre, 6);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.AluminumOxide, 4);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.Benzene, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 1);
        CraftDataHandler.SetItemSize(TechType.PrecursorKey_Red, new Vector2int(2, 2));
        CraftDataHandler.SetCraftingTime(TechType.PrecursorKey_Red, 6);        
        
        RecipeUtil.addRecipe(TechType.PrecursorKey_White, TechGroup.Personal, TechCategory.Equipment, 1, CraftTree.Type.Fabricator, new string[]{"Personal", "Equipment"});
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, TechType.PrecursorIonCrystal, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, C2CItems.getIngot(TechType.Magnetite).ingot, 3);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, TechType.UraniniteCrystal, 3);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, TechType.Diamond, 6);
        CraftDataHandler.SetCraftingTime(TechType.PrecursorKey_White, 8);       

        RecipeUtil.ensureIngredient(TechType.Seamoth, TechType.PowerCell, 1);
        RecipeUtil.ensureIngredient(TechType.Exosuit, TechType.PowerCell, 2);
        RecipeUtil.ensureIngredient(TechType.Cyclops, TechType.PowerCell, 6);
        
        RecipeUtil.modifyIngredients(TechType.BaseReinforcement, i => true);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, TechType.PlasteelIngot, 1);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 1);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, TechType.Lead, 2);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, TechType.FiberMesh, 1);
        Base.FaceHullStrength[(int)Base.FaceType.Reinforcement] = 25; //from 7
        Base.FaceHullStrength[(int)Base.FaceType.BulkheadOpened] = 6; //from 3
        Base.FaceHullStrength[(int)Base.FaceType.BulkheadClosed] = 6; //from 3
        Base.CellHullStrength[(int)Base.CellType.Foundation] = 5; //from 2
        
        KnownTechHandler.Main.RemoveAllCurrentAnalysisTechEntry(TechType.VehicleHullModule2);
        KnownTechHandler.Main.RemoveAllCurrentAnalysisTechEntry(TechType.VehicleHullModule3);
        KnownTechHandler.Main.RemoveAllCurrentAnalysisTechEntry(TechType.BaseReinforcement);
        KnownTechHandler.Main.RemoveAllCurrentAnalysisTechEntry(TechType.HeatBlade); //force you to learn it from the mountain cave base
        //KnownTechHandler.Main.RemoveAllCurrentAnalysisTechEntry(TechType.SeamothElectricalDefense);
        
        RecipeUtil.addIngredient(TechType.PrecursorKey_Purple, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Orange, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Blue, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 3);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 5);
        /*
        CraftDataHandler.SetItemSize(TechType.CreepvinePiece, new Vector2int(2, 2));
        CraftDataHandler.SetItemSize(TechType.CreepvineSeedCluster, new Vector2int(3, 3));
        CraftDataHandler.SetItemSize(TechType.BloodOil, new Vector2int(3, 3));
        CraftDataHandler.SetItemSize(TechType.AcidMushroom, new Vector2int(1, 2));
        CraftDataHandler.SetItemSize(TechType.WhiteMushroom, new Vector2int(1, 2));
        CraftDataHandler.SetItemSize(TechType.WhiteMushroomSpore, new Vector2int(1, 2));
        CraftDataHandler.SetItemSize(TechType.JellyPlant, new Vector2int(2, 2));
        CraftDataHandler.SetItemSize(TechType.JellyPlantSeed, new Vector2int(2, 2));
        */
       
       	//RecipeUtil.logChangedRecipes();
    }
    
    private static void createCompressedIngot(DIPrefab<VanillaResources> item, int amt = 10, string name = "Ingot") {
    	string n = ((ModPrefab)item).ClassID;
    	createCompressedIngot(((ModPrefab)item).TechType, n.Substring(0, 1)+n.Substring(1).ToLowerInvariant(), amt, name, item.getIcon());
    }
    
    private static void createCompressedIngot(TechType item, int amt = 10, string name = "Ingot") {
    	createCompressedIngot(item, ""+item, amt, name);
    }
    
    private static void createCompressedIngot(TechType item, string refName, int amt = 10, string name = "Ingot", Atlas.Sprite spr = null) {
    	BasicCraftingItem ingot = new BasicCraftingItem("ingot_"+item, refName+" "+name, "An ingot of compressed "+refName.ToLowerInvariant(), "41919ae1-1471-4841-a524-705feb9c2d20");
    	ingot.addIngredient(item, amt);
    	ingot.craftingSubCategory = "C2CIngots";
    	ingot.craftingTime = CraftData.craftingTimes[TechType.TitaniumIngot];
    	ingot.unlockRequirement = TechType.Unobtanium;
    	ingot.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/ingot_"+refName.ToLowerInvariant());
       	//ingot.ownerMod = modDLL;
    	ingot.Patch();
    	SNUtil.log("Added compressed ingot for "+refName+": "+ingot.TechType+" @ "+ingot.FabricatorType+" > "+string.Join("/", ingot.StepsToFabricatorTab));
    	
       	TechData rec = new TechData();
      	rec.Ingredients.Add(new Ingredient(ingot.TechType, 1));
       	DuplicateRecipeDelegateWithRecipe unpack = new DuplicateRecipeDelegateWithRecipe(item, rec);
       	unpack.craftTime = 3;
       	unpack.craftingType = CraftTree.Type.Fabricator;
       	unpack.category = C2CItems.ingotCategory;
       	unpack.group = TechGroup.Resources;
       	unpack.unlock = TechType.Unobtanium;
       	unpack.craftingMenuTree = new string[]{"Resources", "C2CIngots2"};
       	unpack.ownerMod = SeaToSeaMod.modDLL;
       	if (spr != null)
       		unpack.sprite = spr;
       	unpack.setRecipe(amt);
       	unpack.Patch();
       	
       	C2CItems.addIngot(item, ingot, unpack, amt);
    }
    
    public static DuplicateRecipeDelegateWithRecipe getAlternateEnzyme() {
    	return enzymeAlternate;
    }
    
    public static DuplicateRecipeDelegateWithRecipe getQuartzIngotToGlass() {
    	return quartzIngotToGlass;
    }

  }
}