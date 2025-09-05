using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Collections.ObjectModel;
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
	public static class C2CRecipes {

		private static DuplicateRecipeDelegateWithRecipe quartzIngotToGlass;
		private static DuplicateRecipeDelegateWithRecipe bacteriaAlternate;
		private static DuplicateRecipeDelegateWithRecipe enzymeAlternate;
		private static DuplicateRecipeDelegateWithRecipe traceMetalAlternate;
		private static DuplicateRecipeDelegateWithRecipe altFiberMesh;
		private static DuplicateRecipeDelegateWithRecipe altSulfurAcid;
		//private static DuplicateRecipeDelegateWithRecipe altBleach;
		private static DuplicateRecipeDelegateWithRecipe t2BatteryRepair;
		//private static DuplicateRecipeDelegateWithRecipe altLuminol;
		//private static DuplicateRecipeDelegateWithRecipe replGunDeConversion;

		//private static DuplicateRecipeDelegateWithRecipe brineSaltConversion;
		//private static DuplicateRecipeDelegateWithRecipe gelWaterConversion;

		//private static TechData hatchingEnzymesReplacement;

		private static CraftTree.Type precursorEnzymeFab;
		private static ModCraftTreeRoot precursorEnzymeTree;

		private static readonly List<TechType> removedVanillaUnlocks = new List<TechType>();
		private static readonly List<TechType> specialRecipes = new List<TechType>();

		internal static void addItemsAndRecipes() {
			SNUtil.log("Applying recipe changes");

			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);

			TechData rec = new TechData();
			rec.Ingredients.Add(new Ingredient(TechType.TitaniumIngot, 1));
			DuplicateRecipeDelegateWithRecipe item = new DuplicateRecipeDelegateWithRecipe(TechType.Titanium, rec);
			item.craftTime = 3;
			item.craftingType = CraftTree.Type.Fabricator;
			//item.category = TechCategory.BasicMaterials;
			//item.group = TechGroup.Resources;
			item.craftingMenuTree = new string[] { "Resources", "C2CIngots2" };
			item.ownerMod = SeaToSeaMod.modDLL;
			item.setRecipe(10);
			item.Patch();

			C2CItems.addIngot(TechType.Titanium, TechType.TitaniumIngot, item, 10);

			string tiingot = "41919ae1-1471-4841-a524-705feb9c2d20";
			createCompressedIngot(TechType.Quartz, tiingot, 18, 6, 0.6F, 5, "Boule");
			createCompressedIngot(TechType.AluminumOxide, tiingot, 30, 3, 0.4F, "Ruby", 8, "Boule");
			createCompressedIngot(TechType.Copper, tiingot, 6.5F, 4, 0.4F);
			createCompressedIngot(TechType.Silver, tiingot, 24, 8, 0);
			createCompressedIngot(TechType.Gold, "4ae90608-40da-45ce-8480-e2f0133f96b2", 12, 5, 0);
			createCompressedIngot(TechType.Lead, "4ae90608-40da-45ce-8480-e2f0133f96b2", 25, 2, 0);
			createCompressedIngot(TechType.Lithium, "c483f597-c78a-42e9-bad5-3be9ef47aa81", 2.5F, 6, 0.5F, 10, "Plate");
			createCompressedIngot(TechType.Magnetite, "a06157cc-8de8-4fec-85a6-76b2aee1e263", 30, 7, 0.5F, 6, "Bar");
			createCompressedIngot(TechType.Nickel, tiingot, 8, 8, 0.6F);
			createCompressedIngot(TechType.Kyanite, tiingot, 30, 3, 0.7F, 6, "Boule");
			createCompressedIngot(TechType.Salt, VanillaResources.LEAD.prefab, 6, 1, 0.6F, 8, "Block");
			createCompressedIngot(TechType.Sulphur, VanillaResources.LEAD.prefab, 0.5F, 0, 1.0F, 6, "Block");
			createCompressedIngot(TechType.Diamond, tiingot, 24, 0, 1.2F, 5, "Boule");
			createCompressedIngot(TechType.MercuryOre, VanillaResources.TITANIUM.prefab, 0.75F, 0, 0F, "Mercury", 8, "Block");
			createCompressedIngot(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), "4ae90608-40da-45ce-8480-e2f0133f96b2", 18, 2, 0.25F);
			createCompressedIngot(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), "a06157cc-8de8-4fec-85a6-76b2aee1e263", 40, 5, 0.8F, 8);

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
			quartzIngotToGlass.craftingMenuTree = new string[] { "Resources", "C2CIngots2" };
			quartzIngotToGlass.sprite = SpriteManager.Get(TechType.Glass);
			quartzIngotToGlass.ownerMod = SeaToSeaMod.modDLL;
			quartzIngotToGlass.Patch();

			BasicCraftingItem acid = CraftingItems.getItem(CraftingItems.Items.WeakAcid);
			acid.craftingTime = 0.5F;
			acid.addIngredient(TechType.AcidMushroom, 4);

			BasicCraftingItem enzyT = CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes);
			enzyT.craftingTime = 2;
			enzyT.addIngredient(TechType.SeaTreaderPoop, 1).addIngredient(acid, 2);
			/*
             int kelpamt = 2;
             BasicCraftingItem enzyK = CraftingItems.getItem(CraftingItems.Items.KelpEnzymes);
             enzyK.craftingTime = 3;
             enzyK.addIngredient(C2CItems.kelp.seed.TechType, kelpamt);
             */
			BasicCraftingItem bacteria = CraftingItems.getItem(CraftingItems.Items.BacterialSample);
			bacteria.craftingTime = 1;
			bacteria.numberCrafted = hard ? 2 : 3;
			bacteria.addIngredient(TechType.SeaCrownSeed, 2).addIngredient(enzyT, 2);//.addIngredient(TechType.TreeMushroomPiece, 1);

			BasicCraftingItem enzy = CraftingItems.getItem(CraftingItems.Items.BioEnzymes);
			enzy.craftingTime = 4;
			enzy.numberCrafted = 4;
			enzy.addIngredient(TechType.Salt, 1).addIngredient(bacteria, 2).addIngredient(TechType.DisinfectedWater, 1);

			BasicCraftingItem comb = CraftingItems.getItem(CraftingItems.Items.HoneycombComposite);
			comb.craftingTime = 12;
			comb.addIngredient(TechType.AramidFibers, hard ? 3 : 2).addIngredient(TechType.PlasteelIngot, 1);

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
			armor.addIngredient(TechType.PlasteelIngot, 2).addIngredient(TechType.Lead, 3).addIngredient(comb, 1).addIngredient(TechType.Nickel, 6).addIngredient(CraftingItems.getItem(CraftingItems.Items.Nanocarbon), 1);

			BasicCraftingItem motor = CraftingItems.getItem(CraftingItems.Items.Motor);
			motor.craftingTime = 1;
			motor.addIngredient(TechType.CopperWire, 1).addIngredient(TechType.Titanium, 2).addIngredient(TechType.Lubricant, 1).addIngredient(TechType.Gold, 1);

			BasicCraftingItem traceMetal = CraftingItems.getItem(CraftingItems.Items.TraceMetals);
			traceMetal.craftingTime = 1;
			traceMetal.numberCrafted = 3;
			traceMetal.addIngredient(TechType.JeweledDiskPiece, 4);

			BasicCraftingItem drone = CraftingItems.getItem(CraftingItems.Items.LathingDrone);
			drone.craftingTime = 4;
			drone.addIngredient(motor, 1).addIngredient(TechType.Titanium, 1).addIngredient(TechType.ComputerChip, 1).addIngredient(TechType.PowerCell, 1);

			BasicCraftingItem chlorine = CraftingItems.getItem(CraftingItems.Items.Chlorine);
			chlorine.craftingTime = 3;
			chlorine.numberCrafted = hard ? 2 : 3;
			chlorine.addIngredient(TechType.Salt, 3).addIngredient(TechType.GasPod, 3);

			BasicCraftingItem sulfurAcid = CraftingItems.getItem(CraftingItems.Items.SulfurAcid);

			BasicCraftingItem heatSeal = CraftingItems.getItem(CraftingItems.Items.HeatSealant);
			heatSeal.craftingTime = 5;
			heatSeal.numberCrafted = 1;
			heatSeal.addIngredient(sealedFabric, 1).addIngredient(sulfurAcid, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.Tungsten), 5).addIngredient(C2CItems.mountainGlow.seed, 3);

			BasicCraftingItem tankWall = CraftingItems.getItem(CraftingItems.Items.FuelTankWall);
			tankWall.craftingTime = 2.5F;
			tankWall.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS), 9).addIngredient(heatSeal, 2).addIngredient(CraftingItems.getItem(CraftingItems.Items.SmartPolymer), 1);

			BasicCraftingItem microfilter = CraftingItems.getItem(CraftingItems.Items.MicroFilter);
			microfilter.craftingTime = 6F;
			microfilter.addIngredient(TechType.Titanium, 2).addIngredient(C2CItems.mountainGlow.seed, hard ? 2 : 1).addIngredient(TechType.FiberMesh, 5).addIngredient(TechType.Diamond, 1).addIngredient(TechType.AluminumOxide, hard ? 3 : 2);

			BasicCraftingItem fuel = CraftingItems.getItem(CraftingItems.Items.RocketFuel);
			fuel.craftingTime = 6;
			fuel.addIngredient(TechType.Sulphur, 8).addIngredient(TechType.Kyanite, hard ? 4 : 3).addIngredient(TechType.CrashPowder, hard ? 3 : 2).addIngredient(TechType.PrecursorIonCrystal, hard ? 2 : 1).addIngredient(microfilter, 1);

			BasicCraftingItem electro = CraftingItems.getItem(CraftingItems.Items.Electrolytes);
			electro.craftingTime = 1;
			electro.numberCrafted = 2;
			electro.addIngredient(C2CItems.sanctuaryPlant.seed, 4).addIngredient(C2CItems.brineCoralPiece, 3).addIngredient(sulfurAcid, 1);

			//BasicCraftingItem pump = CraftingItems.getItem(CraftingItems.Items.FluidPump);
			//pump.craftingTime = 5;
			//pump.addIngredient(TechType.PlasteelIngot, 1).addIngredient(TechType.AdvancedWiringKit, 1).addIngredient(motor, 4).addIngredient(TechType.Pipe, 10);

			BasicCraftingItem dimlum = CraftingItems.getItem(CraftingItems.Items.DimLuminol);
			dimlum.craftingTime = 6F;
			dimlum.addIngredient(TechType.DisinfectedWater, 1).addIngredient(TechType.PurpleStalkSeed, 2).addIngredient(TechType.EyesPlantSeed, 1).addIngredient(TechType.RedBasketPlantSeed, 1).addIngredient(TechType.SnakeMushroomSpore, 2).addIngredient(TechType.RedConePlantSeed, 1);

			BasicCraftingItem obsidiglass = CraftingItems.getItem(CraftingItems.Items.ObsidianGlass);
			obsidiglass.craftingTime = 3F;
			obsidiglass.addIngredient(TechType.EnameledGlass, 1).addIngredient(TechType.Aerogel, 2).addIngredient(C2CItems.getIngot(TechType.Quartz).ingot, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.OBSIDIAN), 3);

			C2CItems.addCraftingItems();

			rec = RecipeUtil.copyRecipe(bacteria.getRecipe());
			foreach (Ingredient i in rec.Ingredients) {
				if (i.techType == enzyT.TechType) {
					i.amount *= 2;
				}
				else if (i.techType == TechType.SeaCrownSeed) {
					i.amount = 3;
				}
			}
			rec.Ingredients.Add(new Ingredient(CraftingItems.getItem(CraftingItems.Items.AmoeboidSample).TechType, 2));
			bacteriaAlternate = new DuplicateRecipeDelegateWithRecipe(bacteria, rec);
			bacteriaAlternate.ownerMod = SeaToSeaMod.modDLL;
			bacteriaAlternate.craftTime = bacteria.craftingTime * 2F;
			bacteriaAlternate.setRecipe(bacteria.numberCrafted * 3);
			bacteriaAlternate.unlock = TechType.Unobtanium;
			bacteriaAlternate.Patch();

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
			rec.Ingredients.Add(new Ingredient(sulfurAcid.TechType, 1));
			rec.Ingredients.Add(new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).TechType, 1));
			enzymeAlternate = new DuplicateRecipeDelegateWithRecipe(enzy, rec);
			enzymeAlternate.ownerMod = SeaToSeaMod.modDLL;
			enzymeAlternate.craftTime = enzy.craftingTime * 2F;
			enzymeAlternate.setRecipe(enzy.numberCrafted * 3);
			enzymeAlternate.unlock = TechType.Unobtanium;
			enzymeAlternate.Patch();

			rec = new TechData();
			rec.Ingredients.Add(new Ingredient(TechType.JeweledDiskPiece, 4));
			rec.Ingredients.Add(new Ingredient(sulfurAcid.TechType, 1));
			traceMetalAlternate = new DuplicateRecipeDelegateWithRecipe(traceMetal, rec);
			traceMetalAlternate.ownerMod = SeaToSeaMod.modDLL;
			traceMetalAlternate.craftTime = 2.5F;
			traceMetalAlternate.setRecipe(5);
			traceMetalAlternate.unlock = TechType.Unobtanium;
			traceMetalAlternate.allowUnlockPopups = true;
			traceMetalAlternate.Patch();

			rec = RecipeUtil.copyRecipe(RecipeUtil.getRecipe(TechType.FiberMesh));
			rec.Ingredients[0].amount = 3;
			rec.Ingredients.Add(new Ingredient(C2CItems.mountainGlow.seed.TechType, 2));
			altFiberMesh = new DuplicateRecipeDelegateWithRecipe(TechType.FiberMesh, rec);
			altFiberMesh.category = TechCategory.BasicMaterials;
			altFiberMesh.group = TechGroup.Resources;
			altFiberMesh.craftingType = CraftTree.Type.Fabricator;
			altFiberMesh.craftingMenuTree = new string[] { "Resources", "BasicMaterials" };
			altFiberMesh.ownerMod = SeaToSeaMod.modDLL;
			altFiberMesh.craftTime = 6;
			altFiberMesh.setRecipe(4);
			altFiberMesh.unlock = TechType.Unobtanium;
			altFiberMesh.allowUnlockPopups = true;
			altFiberMesh.Patch();

			rec = new TechData();
			rec.Ingredients.Add(new Ingredient(TechType.Sulphur, 4));
			rec.Ingredients.Add(new Ingredient(TechType.BigFilteredWater, 2));
			rec.Ingredients.Add(new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1));
			altSulfurAcid = new DuplicateRecipeDelegateWithRecipe(CraftingItems.getItem(CraftingItems.Items.SulfurAcid), rec);
			altSulfurAcid.category = C2CItems.chemistryCategory;
			altSulfurAcid.group = TechGroup.Resources;
			altSulfurAcid.craftingType = CraftTree.Type.Fabricator;
			altSulfurAcid.craftingMenuTree = new string[] { "Resources", "C2Chemistry" };
			altSulfurAcid.ownerMod = SeaToSeaMod.modDLL;
			altSulfurAcid.craftTime = 9;
			altSulfurAcid.setRecipe(5);
			altSulfurAcid.unlock = TechType.Sulphur;
			altSulfurAcid.allowUnlockPopups = true;
			altSulfurAcid.Patch();
			/*
            rec = new TechData();
            rec.Ingredients.Add(new Ingredient(TechType.PurpleStalkSeed, 2));
            rec.Ingredients.Add(new Ingredient(TechType.EyesPlantSeed, 1));
            rec.Ingredients.Add(new Ingredient(TechType.RedBasketPlantSeed, 1));
            rec.Ingredients.Add(new Ingredient(TechType.SnakeMushroomSpore, 2));
            rec.Ingredients.Add(new Ingredient(TechType.RedConePlantSeed, 1));
            altLuminol = new DuplicateRecipeDelegateWithRecipe(CraftingItems.getItem(CraftingItems.Items.Luminol), rec);
            altLuminol.category = C2CItems.chemistryCategory;
            altLuminol.group = TechGroup.Resources;
            altLuminol.craftingType = CraftTree.Type.Fabricator;
            altLuminol.craftingMenuTree = new string[]{"Resources", "C2Chemistry"};
            altLuminol.ownerMod = SeaToSeaMod.modDLL;
            altLuminol.craftTime = 30;
            altLuminol.setRecipe(1);
            altLuminol.unlock = TechType.SnakeMushroomSpore;
            altLuminol.allowUnlockPopups = true;
            altLuminol.Patch();*/
			/*
            rec = new TechData();
            rec.Ingredients.Add(new Ingredient(TechType.Salt, 2));
            rec.Ingredients.Add(new Ingredient(TechType.HydrochloricAcid, 1));
            altBleach = new DuplicateRecipeDelegateWithRecipe(TechType.Bleach, rec);
            altBleach.category = C2CItems.chemistryCategory;
            altBleach.group = TechGroup.Resources;
            altBleach.craftingType = CraftTree.Type.Fabricator;
            altBleach.craftingMenuTree = new string[]{"Resources", "C2Chemistry"};
            altBleach.ownerMod = SeaToSeaMod.modDLL;
            altBleach.craftTime = 4;
            altBleach.setRecipe(2);
            altBleach.unlock = TechType.HydrochloricAcid;
            altBleach.allowUnlockPopups = true;
            altBleach.Patch();*/
			/*
            int s = 3;
            rec = new TechData();
            rec.Ingredients.Add(new Ingredient(C2CItems.kelp.seed.TechType, Mathf.CeilToInt(kelpamt*s*0.5F)));
            rec.Ingredients.Add(new Ingredient(TechType.TreeMushroomPiece, 1));
            item = new DuplicateRecipeDelegateWithRecipe(enzyK, rec);
            item.setRecipe(enzyK.numberCrafted*s);
            item.craftTime = enzyK.craftingTime*s/2F;
            item.ownerMod = SeaToSeaMod.modDLL;
            item.Patch();*/
			/*
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
            item.Patch();*/

			/*
            CraftData.itemSizes[TechType.AcidMushroom] = new Vector2int(1, 2);
            CraftData.itemSizes[TechType.HydrochloricAcid] = new Vector2int(2, 2);
            RecipeUtil.modifyIngredients(TechType.HydrochloricAcid, i => i.amount = 12);
            */

			RecipeUtil.removeRecipe(TechType.HydrochloricAcid, true);
			RecipeUtil.removeRecipe(TechType.Benzene, true);

			RecipeUtil.removeRecipe(TechType.RepulsionCannon, true);

			//hatchingEnzymesReplacement = RecipeUtil.copyRecipe(RecipeUtil.getRecipe(TechType.HatchingEnzymes));
			//RecipeUtil.removeRecipe(TechType.HatchingEnzymes, true);
			RecipeUtil.RecipeNode node = RecipeUtil.getRecipeNode(TechType.HatchingEnzymes);
			CraftTreeHandler.Main.RemoveNode(node.recipeType, node.path.Split('\\'));
			precursorEnzymeTree = CraftTreeHandler.CreateCustomCraftTreeAndType("PrecursorEnzymes", out precursorEnzymeFab);
			precursorEnzymeTree.AddCraftingNode(TechType.HatchingEnzymes);
			//RecipeUtil.changeRecipePath(TechType.HatchingEnzymes, "Resources", "C2Chemistry");
			RecipeUtil.setItemCategory(TechType.HatchingEnzymes, TechGroup.Uncategorized, TechCategory.Misc); //this removes from PDA
			C2CItems.setChemistry(TechType.Bleach);
			C2CItems.setChemistry(TechType.Polyaniline);
			//C2CItems.setChemistry(TechType.HatchingEnzymes);

			RecipeUtil.changeRecipePath(TechType.TitaniumIngot, "Resources", "C2CIngots");
			RecipeUtil.setItemCategory(TechType.TitaniumIngot, TechGroup.Resources, C2CItems.ingotCategory);
			//do not remove creepvine, as lubricant is needed earlier than this

			C2CItems.voidStealth.addIngredient(lens, 1).addIngredient(comb, 2).addIngredient(TechType.Aerogel, 12);
			C2CItems.depth1300.addIngredient(TechType.VehicleHullModule3, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS), 12).addIngredient(obsidiglass, 3).addIngredient(armor, 2).addIngredient(CraftingItems.getItem(CraftingItems.Items.Tungsten), 8);
			C2CItems.powerSeal.addIngredient(TechType.Polyaniline, 3).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 6).addIngredient(CraftingItems.getItem(CraftingItems.Items.Electrolytes), 2).addIngredient(CraftingItems.getItem(CraftingItems.Items.Sealant), 5);
			C2CItems.heatSinkModule.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 2).addIngredient(TechType.Pipe, 5).addIngredient(motor, 1).addIngredient(TechType.AdvancedWiringKit, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.Tungsten), 5);
			C2CItems.speedModule.addIngredient(TechType.Nickel, 5).addIngredient(motor, 4).addIngredient(TechType.AdvancedWiringKit, 1).addIngredient(electro, hard ? 3 : 2).addIngredient(TechType.Gravsphere, 1);
			C2CItems.lightModule.addIngredient(TechType.LEDLight, 3).addIngredient(TechType.WiringKit, 1).addIngredient(TechType.CopperWire, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.Luminol), 2).addIngredient(CraftingItems.getItem(CraftingItems.Items.Tungsten), 1);
			C2CItems.tetherModule.addIngredient(TechType.ExosuitPropulsionArmModule, 1).addIngredient(TechType.WiringKit, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.Motor), 1);
			C2CItems.cyclopsHeat.addIngredient(TechType.CyclopsThermalReactorModule, 1).addIngredient(TechType.CyclopsFireSuppressionModule, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 12).addIngredient(heatSeal, 4);
			C2CItems.cyclopsStorage.addIngredient(TechType.VehicleStorageModule, 2).addIngredient(TechType.ComputerChip, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 1);
			C2CItems.sealSuit.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 3).addIngredient(sealedFabric, 5).addIngredient(TechType.Titanium, 1).addIngredient(TechType.CrashPowder, 2);
			C2CItems.t2Battery.addIngredient(TechType.Battery, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.DenseAzurite), 1).addIngredient(TechType.Polyaniline, 1).addIngredient(TechType.MercuryOre, 2).addIngredient(TechType.Lithium, 2).addIngredient(TechType.Silicone, 1);
			C2CItems.rebreatherV2.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 4).addIngredient(microfilter, 2).addIngredient(sealedFabric, 3).addIngredient(TechType.Rebreather, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.Motor), 1).addIngredient(C2CItems.t2Battery, 1);
			C2CItems.breathingFluid.addIngredient(TechType.Benzene, 1).addIngredient(SeaToSeaMod.geogel, 4).addIngredient(TechType.MembrainTreeSeed, 2).addIngredient(TechType.Eyeye, 2).addIngredient(C2CItems.bkelpBumpWormItem.TechType, 1).addIngredient(TechType.OrangeMushroomSpore, 1).addIngredient(TechType.SpottedLeavesPlantSeed, 2);
			C2CItems.liquidTank.addIngredient(TechType.HighCapacityTank, 1).addIngredient(C2CItems.breathingFluid, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.HoneycombComposite), 1).addIngredient(sealedFabric, 2);
			C2CItems.heatSink.addIngredient(TechType.Titanium, 4).addIngredient(TechType.Lithium, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.Tungsten), 3);
			C2CItems.bandage.addIngredient(TechType.FirstAidKit, 1).addIngredient(SeaToSeaMod.geogel, 1).addIngredient(C2CItems.healFlower.seed, 3).addIngredient(TechType.JellyPlant, 1).addIngredient(TechType.Bleach, 1);
			C2CItems.treatment.addIngredient(C2CItems.bandage, 1).addIngredient(TechType.Glass, 1).addIngredient(TechType.Titanium, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42), 2);
			C2CItems.chargeFinRelay.addIngredient(CraftingItems.getItem(CraftingItems.Items.Electrolytes), 4).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL), 1).addIngredient(TechType.AdvancedWiringKit, 1).addIngredient(TechType.Magnetite, 4);
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

			rec = RecipeUtil.copyRecipe(C2CItems.t2Battery.getRecipe());
			for (int idx = rec.Ingredients.Count - 1; idx >= 0; idx--) {
				Ingredient i = rec.Ingredients[idx];
				if (i.techType == TechType.Polyaniline || i.techType == TechType.Battery || i.techType == TechType.Silicone) {
					rec.Ingredients.RemoveAt(idx);
				}
				else if (i.techType == CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType) {
					i.techType = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType;
					i.amount = 3;
				}
			}
			rec.Ingredients.Insert(0, new Ingredient(CraftingItems.getItem(CraftingItems.Items.BrokenT2Battery).TechType, 1));
			t2BatteryRepair = new DuplicateRecipeDelegateWithRecipe(C2CItems.t2Battery, rec);
			t2BatteryRepair.setRecipe();
			CraftData.GetCraftTime(C2CItems.t2Battery.TechType, out t2BatteryRepair.craftTime);
			t2BatteryRepair.category = C2CItems.t2Battery.CategoryForPDA;
			t2BatteryRepair.group = C2CItems.t2Battery.GroupForPDA;
			t2BatteryRepair.craftingType = C2CItems.t2Battery.FabricatorType;
			t2BatteryRepair.craftingMenuTree = C2CItems.t2Battery.StepsToFabricatorTab;
			t2BatteryRepair.unlock = TechType.Unobtanium;
			t2BatteryRepair.ownerMod = SeaToSeaMod.modDLL;
			t2BatteryRepair.suffixName = " Repair";
			t2BatteryRepair.Patch();

			/*
            rec = new TechData();
            rec.Ingredients.Add(new Ingredient(C2CItems.brineSalt.TechType, 1));
            brineSaltConversion = new DuplicateRecipeDelegateWithRecipe(C2CItems.t2Battery, rec);
            brineSaltConversion.setRecipe();
            CraftData.GetCraftTime(C2CItems.t2Battery.TechType, out t2BatteryRepair.craftTime);
            brineSaltConversion.category = C2CItems.t2Battery.CategoryForPDA;
            brineSaltConversion.group = C2CItems.t2Battery.GroupForPDA;
            brineSaltConversion.craftingType = C2CItems.t2Battery.FabricatorType;
            brineSaltConversion.craftingMenuTree = C2CItems.t2Battery.StepsToFabricatorTab;
            brineSaltConversion.unlock = TechType.Unobtanium;
            brineSaltConversion.ownerMod = SeaToSeaMod.modDLL;
            brineSaltConversion.suffixName = " Conversion";
            brineSaltConversion.Patch();
            */
			/*
            rec = new TechData();
            rec.Ingredients.Add(new Ingredient(TechType.RepulsionCannon, 1));
            replGunDeConversion = new DuplicateRecipeDelegateWithRecipe(TechType.PropulsionCannon, rec);
            replGunDeConversion.setRecipe();
            replGunDeConversion.craftTime = 5;
            replGunDeConversion.category = TechCategory.Workbench;
            replGunDeConversion.group = TechGroup.Workbench;
            replGunDeConversion.craftingType = CraftTree.Type.Workbench;
            replGunDeConversion.craftingMenuTree = new string[]{"PropulsionCannonMenu"};
            replGunDeConversion.unlock = TechType.RepulsionCannon;
            replGunDeConversion.ownerMod = SeaToSeaMod.modDLL;
            replGunDeConversion.suffixName = " conversion";
            replGunDeConversion.Patch();
            */
			//SurvivalHandler.GiveHealthOnConsume(bandage.TechType, 50, false);

			RecipeUtil.startLoggingRecipeChanges();

			RecipeUtil.modifyIngredients(TechType.Lubricant, i => { i.amount = 4; return false; });

			addItemToRecipe(TechType.Rebreather, TechType.Titanium, 3);
			//addItemToRecipe(TechType.Rebreather, TechType.AdvancedWiringKit, 1);
			addItemToRecipe(TechType.Rebreather, TechType.EnameledGlass, 1);
			// RecipeUtil.removeIngredient(TechType.Rebreather, TechType.WiringKit);

			RecipeUtil.modifyIngredients(TechType.Constructor, i => i.techType != TechType.TitaniumIngot);
			addItemToRecipe(TechType.Constructor, TechType.WiringKit, 1);
			addItemToRecipe(TechType.Constructor, TechType.Silicone, 3);
			addItemToRecipe(TechType.Constructor, drone.TechType, 4);

			//addItemToRecipe(TechType.Polyaniline, TechType.Salt, 2);
			addItemToRecipe(TechType.Polyaniline, C2CItems.sanctuaryPlant.seed.TechType, 1);

			addItemToRecipe(TechType.StasisRifle, CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 6);
			RecipeUtil.removeIngredient(TechType.StasisRifle, TechType.Battery);
			addItemToRecipe(TechType.StasisRifle, C2CItems.t2Battery.TechType, 2);

			RecipeUtil.modifyIngredients(TechType.ReinforcedDiveSuit, i => { if (i.techType == TechType.Diamond) i.amount = 4; return i.techType == TechType.Titanium || i.techType == TechType.AramidFibers; });
			addItemToRecipe(TechType.ReinforcedDiveSuit, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 9);
			addItemToRecipe(TechType.ReinforcedDiveSuit, C2CItems.sealSuit.TechType, 1);
			addItemToRecipe(TechType.ReinforcedDiveSuit, C2CItems.sealGloves.TechType, 1);
			addItemToRecipe(TechType.ReinforcedDiveSuit, heatSeal.TechType, 2);

			addItemToRecipe(TechType.Stillsuit, C2CItems.mountainGlow.seed.TechType, 3);

			addItemToRecipe(TechType.SwimChargeFins, electro.TechType, 1);

			addItemToRecipe(TechType.BaseNuclearReactor, electro.TechType, 2);
			addItemToRecipe(TechType.BaseNuclearReactor, armor.TechType, 2);
			RecipeUtil.removeIngredient(TechType.BaseNuclearReactor, TechType.PlasteelIngot);
			RecipeUtil.removeIngredient(TechType.BaseNuclearReactor, TechType.Lead);

			RecipeUtil.modifyIngredients(TechType.AramidFibers, i => { if (i.techType == TechType.FiberMesh) i.amount = 2; return false; });

			RecipeUtil.modifyIngredients(TechType.PlasteelIngot, i => { if (i.techType == TechType.Lithium) i.amount = i.amount * 3 / 2; return false; });
			addItemToRecipe(TechType.PlasteelIngot, CraftingItems.getItem(CraftingItems.Items.GeyserMinerals).TechType, 1);

			RecipeUtil.removeIngredient(TechType.Battery, TechType.AcidMushroom);
			addItemToRecipe(TechType.Battery, acid.TechType, 3);

			addItemToRecipe(TechType.WiringKit, acid.TechType, 1);
			addItemToRecipe(TechType.ComputerChip, acid.TechType, 2);

			addItemToRecipe(TechType.ReactorRod, sulfurAcid.TechType, 1);

			RecipeUtil.modifyIngredients(TechType.PrecursorIonBattery, i => { i.amount *= i.techType == TechType.Gold ? 5 : 2; return i.techType == TechType.Silver; });
			addItemToRecipe(TechType.PrecursorIonBattery, TechType.PowerCell, 1);
			addItemToRecipe(TechType.PrecursorIonBattery, CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 4);
			addItemToRecipe(TechType.PrecursorIonBattery, electro.TechType, 2);
			//RecipeUtil.getRecipe(TechType.PrecursorIonBattery).craftAmount = 2;
			RecipeUtil.modifyIngredients(TechType.PrecursorIonPowerCell, i => { if (i.techType == TechType.PrecursorIonBattery) i.amount = 1; return i.techType == TechType.Silicone; });
			addItemToRecipe(TechType.PrecursorIonPowerCell, CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 4);
			addItemToRecipe(TechType.PrecursorIonPowerCell, TechType.MercuryOre, 2);
			addItemToRecipe(TechType.PrecursorIonPowerCell, CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 5);

			C2CItems.setModElectronics(TechType.PrecursorIonBattery);
			C2CItems.setModElectronics(TechType.PrecursorIonPowerCell);

			addItemToRecipe(TechType.CyclopsShieldModule, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, 1);
			addItemToRecipe(TechType.CyclopsShieldModule, electro.TechType, 1);

			addItemToRecipe(TechType.CyclopsThermalReactorModule, electro.TechType, 4);
			addItemToRecipe(TechType.ExosuitThermalReactorModule, electro.TechType, 3);

			addItemToRecipe(TechType.RocketBase, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 4);
			addItemToRecipe(TechType.RocketBase, TechType.Silicone, 8);
			addItemToRecipe(TechType.RocketBase, CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 4);
			RecipeUtil.modifyIngredients(TechType.RocketBase, i => {
				if (i.techType == TechType.Lead)
					i.amount = 6;
				return i.techType == TechType.TitaniumIngot || i.techType == TechType.ComputerChip;
			});
			addItemToRecipe(TechType.RocketBaseLadder, TechType.WiringKit, 4);
			RecipeUtil.modifyIngredients(TechType.RocketStage1, i => i.techType != TechType.PlasteelIngot);
			addItemToRecipe(TechType.RocketStage1, CustomMaterials.getIngot(CustomMaterials.Materials.IRIDIUM), hard ? 2 : 1);
			addItemToRecipe(TechType.RocketStage1, CustomMaterials.getIngot(CustomMaterials.Materials.PLATINUM), hard ? 3 : 2);
			addItemToRecipe(TechType.RocketStage1, C2CItems.getIngot(TechType.Diamond).ingot, hard ? 3 : 2);
			addItemToRecipe(TechType.RocketStage1, CraftingItems.getItem(CraftingItems.Items.Tungsten).TechType, hard ? 12 : 6);
			RecipeUtil.modifyIngredients(TechType.RocketStage2, i => i.techType == TechType.Kyanite || i.techType == TechType.Sulphur);
			addItemToRecipe(TechType.RocketStage2, tankWall.TechType, hard ? 3 : 2);
			addItemToRecipe(TechType.RocketStage2, fuel.TechType, hard ? 6 : 4);
			addItemToRecipe(TechType.RocketStage2, CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType, hard ? 9 : 6);
			RecipeUtil.modifyIngredients(TechType.RocketStage3, i => { if (i.techType == TechType.EnameledGlass) { i.amount = 5; i.techType = obsidiglass.TechType; } return i.techType == TechType.ComputerChip; });
			addItemToRecipe(TechType.RocketStage3, C2CItems.t2Battery.TechType, hard ? 3 : 2);
			addItemToRecipe(TechType.RocketStage3, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 8);
			addItemToRecipe(TechType.RocketStage3, TechType.AdvancedWiringKit, 3);
			addItemToRecipe(TechType.RocketStage3, TechType.ReactorRod, 4);
			addItemToRecipe(TechType.RocketStage3, electro.TechType, hard ? 6 : 4);
			addItemToRecipe(TechType.RocketStage3, C2CItems.getIngot(TechType.Magnetite).ingot, hard ? 5 : 3);

			addItemToRecipe(TechType.HighCapacityTank, TechType.Aerogel, 1);

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
			Dictionary<TechType, int> replaceTableCoral = new Dictionary<TechType, int>(){
			{TechType.ComputerChip, 2},
			{TechType.Fabricator, 1},
			{TechType.BaseMapRoom, hard ? 2 : 1},
		};

			foreach (KeyValuePair<TechType, int> kvp in addMotors) {
				int amt = -1;
				RecipeUtil.modifyIngredients(kvp.Key, i => { if (i.techType == TechType.Lubricant) { amt = i.amount; return true; } else { return false; } });
				addItemToRecipe(kvp.Key, motor.TechType, Math.Max(kvp.Value, amt));
			}

			foreach (KeyValuePair<TechType, int> kvp in replaceTableCoral) {
				int amt = -1;
				RecipeUtil.modifyIngredients(kvp.Key, i => { if (i.techType == TechType.JeweledDiskPiece) { amt = i.amount; return true; } else { return false; } });
				addItemToRecipe(kvp.Key, traceMetal.TechType, Math.Max(kvp.Value, amt));
			}

			addItemToRecipe(TechType.Cyclops, armor.TechType, 4);
			addItemToRecipe(TechType.Cyclops, electro.TechType, 3);
			addItemToRecipe(TechType.Exosuit, CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 4);
			addItemToRecipe(TechType.Exosuit, obsidiglass.TechType, 4);
			RecipeUtil.removeIngredient(TechType.Exosuit, TechType.EnameledGlass);

			RecipeUtil.removeIngredient(TechType.ExoHullModule1, TechType.PlasteelIngot);
			addItemToRecipe(TechType.ExoHullModule1, TechType.Kyanite, 3);
			addItemToRecipe(TechType.ExoHullModule1, armor.TechType, 2);
			addItemToRecipe(TechType.ExoHullModule2, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 4);
			addItemToRecipe(TechType.ExoHullModule2, CraftingItems.getItem(CraftingItems.Items.SmartPolymer).TechType, 2);
			RecipeUtil.removeIngredient(TechType.ExoHullModule2, TechType.Kyanite);
			RecipeUtil.removeIngredient(TechType.ExoHullModule2, TechType.Titanium);

			//addItemToRecipe(TechType.ExosuitJetUpgradeModule, CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).TechType, 4);
			addItemToRecipe(TechType.ExosuitJetUpgradeModule, CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 5);

			addItemToRecipe(TechType.CyclopsHullModule1, CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).TechType, 8);
			addItemToRecipe(TechType.CyclopsHullModule2, C2CItems.mountainGlow.seed.TechType, 3);
			addItemToRecipe(TechType.CyclopsHullModule2, obsidiglass.TechType, 4);

			addItemToRecipe(TechType.LaserCutter, TechType.AluminumOxide, 2);
			RecipeUtil.removeIngredient(TechType.LaserCutter, TechType.Battery);
			addItemToRecipe(TechType.LaserCutter, C2CItems.t2Battery.TechType, 1);

			RecipeUtil.modifyIngredients(TechType.VehicleHullModule1, i => { if (i.techType == TechType.Glass) i.techType = ReefbalanceMod.baseGlass.TechType; return false; });
			addItemToRecipe(TechType.VehicleHullModule1, CraftingItems.getItem(CraftingItems.Items.MicroFilter).TechType, 1);
			addItemToRecipe(TechType.VehicleHullModule1, TechType.Aerogel, 2);
			RecipeUtil.modifyIngredients(TechType.VehicleHullModule2, i => { if (i.techType == TechType.EnameledGlass || i.techType == TechType.Magnetite) i.amount *= 4; return false; });
			addItemToRecipe(TechType.VehicleHullModule1, TechType.Silicone, 2);
			addItemToRecipe(TechType.VehicleHullModule2, TechType.AdvancedWiringKit, 1);
			addItemToRecipe(TechType.VehicleHullModule2, TechType.BloodOil, 3);
			addItemToRecipe(TechType.VehicleHullModule2, TechType.AluminumOxide, 9);
			addItemToRecipe(TechType.VehicleHullModule2, CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 4);
			//addItemToRecipe(TechType.VehicleHullModule3, armor.TechType, 2);
			addItemToRecipe(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType, 1);
			addItemToRecipe(TechType.VehicleHullModule3, comb.TechType, hard ? 2 : 1); //down from 2 because of all the new stuff
			addItemToRecipe(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 2);
			addItemToRecipe(TechType.VehicleHullModule3, CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).TechType, 9);
			//if (hard)
			addItemToRecipe(TechType.VehicleHullModule3, TechType.Nickel, 4);
			RecipeUtil.removeIngredient(TechType.VehicleHullModule3, TechType.PlasteelIngot);
			RecipeUtil.removeIngredient(TechType.VehicleHullModule3, TechType.AluminumOxide);
			addItemToRecipe(TechType.VehicleHullModule3, TechType.Diamond, 3);
			addItemToRecipe(TechType.VehicleHullModule3, TechType.Lubricant, 6);

			addItemToRecipe(TechType.PrecursorKey_Blue, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, 2);

			RecipeUtil.modifyIngredients(TechType.EnameledGlass, i => { if (i.techType == TechType.Glass) i.amount *= 2; return false; });
			RecipeUtil.getRecipe(TechType.EnameledGlass).craftAmount *= 2;
			addItemToRecipe(TechType.EnameledGlass, TechType.Lead, 2);
			addItemToRecipe(TechType.EnameledGlass, TechType.Diamond, 1);
			addItemToRecipe(TechType.AdvancedWiringKit, TechType.MercuryOre, 1);

			RecipeUtil.modifyIngredients(TechType.AdvancedWiringKit, i => { if (i.techType == TechType.WiringKit) i.amount *= 2; return false; });
			RecipeUtil.modifyIngredients(TechType.WiringKit, i => { if (i.techType == TechType.Silver) i.amount = 3; return false; });

			RecipeUtil.getRecipe(TechType.DisinfectedWater).craftAmount = 3;
			addItemToRecipe(TechType.Bleach, chlorine.TechType, 1);
			addItemToRecipe(TechType.BaseFiltrationMachine, TechType.Bleach, 2);
			addItemToRecipe(TechType.BaseFiltrationMachine, TechType.AdvancedWiringKit, 1);
			RecipeUtil.removeIngredient(TechType.BaseFiltrationMachine, TechType.CopperWire);
			addItemToRecipe(TechType.BaseFiltrationMachine, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 1);

			//replace fiber mesh with microbial filter

			RecipeUtil.addRecipe(TechType.PrecursorKey_Red, TechGroup.Personal, TechCategory.Equipment, new string[] { "Machines" });
			addItemToRecipe(TechType.PrecursorKey_Red, TechType.PrecursorIonCrystal, 1);
			addItemToRecipe(TechType.PrecursorKey_Red, TechType.MercuryOre, 6);
			addItemToRecipe(TechType.PrecursorKey_Red, TechType.AluminumOxide, 4);
			addItemToRecipe(TechType.PrecursorKey_Red, TechType.Benzene, 1);
			addItemToRecipe(TechType.PrecursorKey_Red, C2CItems.mountainGlow.seed.TechType, 2);
			addItemToRecipe(TechType.PrecursorKey_Red, CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 1);
			CraftDataHandler.SetItemSize(TechType.PrecursorKey_Red, new Vector2int(3, 3));
			CraftDataHandler.SetCraftingTime(TechType.PrecursorKey_Red, 6);
			CraftDataHandler.SetItemSize(TechType.PrecursorKey_Blue, new Vector2int(2, 2));

			RecipeUtil.addRecipe(TechType.PrecursorKey_White, TechGroup.Personal, TechCategory.Equipment, new string[] { "Machines" });
			addItemToRecipe(TechType.PrecursorKey_White, TechType.PrecursorIonCrystal, 1);
			addItemToRecipe(TechType.PrecursorKey_White, C2CItems.getIngot(TechType.Magnetite).ingot, 3);
			addItemToRecipe(TechType.PrecursorKey_White, TechType.UraniniteCrystal, 3);
			addItemToRecipe(TechType.PrecursorKey_White, TechType.Diamond, 6);
			CraftDataHandler.SetCraftingTime(TechType.PrecursorKey_White, 8);

			RecipeUtil.modifyIngredients(TechType.HatchingEnzymes, i => {
				switch (i.techType) {
					case TechType.KooshChunk:
						i.amount = 2;
						break;
					case TechType.TreeMushroomPiece:
						i.amount = 4;
						break;
					case TechType.RedGreenTentacleSeed:
						i.amount = 3;
						break;
				}
				return false;
			});
			addItemToRecipe(TechType.HatchingEnzymes, TechType.ShellGrassSeed, 1);
			addItemToRecipe(TechType.HatchingEnzymes, C2CItems.emperorRootOil.TechType, 3);

			CraftDataHandler.SetItemSize(TechType.ShellGrassSeed, new Vector2int(1, 1));
			CraftDataHandler.SetItemSize(TechType.RedGreenTentacleSeed, new Vector2int(1, 2));

			RecipeUtil.ensureIngredient(TechType.Seamoth, TechType.PowerCell, 1);
			RecipeUtil.ensureIngredient(TechType.Exosuit, TechType.PowerCell, 2);
			RecipeUtil.ensureIngredient(TechType.Cyclops, TechType.PowerCell, 6);

			if (hard)
				RecipeUtil.addIngredient(TechType.Seamoth, TechType.EnameledGlass, RecipeUtil.removeIngredient(TechType.Seamoth, TechType.Glass).amount);

			RecipeUtil.modifyIngredients(TechType.BaseReinforcement, i => true);
			addItemToRecipe(TechType.BaseReinforcement, TechType.PlasteelIngot, 1);
			addItemToRecipe(TechType.BaseReinforcement, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 1);
			addItemToRecipe(TechType.BaseReinforcement, TechType.Lead, 2);
			addItemToRecipe(TechType.BaseReinforcement, TechType.FiberMesh, 1);

			Base.FaceHullStrength[(int)Base.FaceType.Reinforcement] = hard ? 30 : 40; //from 7
			Base.FaceHullStrength[(int)Base.FaceType.BulkheadOpened] = hard ? 8 : 12; //from 3
			Base.FaceHullStrength[(int)Base.FaceType.BulkheadClosed] = hard ? 8 : 12; //from 3
			Base.CellHullStrength[(int)Base.CellType.Foundation] = hard ? 5 : 6; //from 2

			TechType.VehicleHullModule1.removeUnlockTrigger();
			TechType.VehicleHullModule2.removeUnlockTrigger();
			TechType.VehicleHullModule3.removeUnlockTrigger();
			TechType.BaseReinforcement.removeUnlockTrigger();
			TechType.HeatBlade.removeUnlockTrigger(); //force you to learn it from the mountain cave base
			if (hard)
				TechType.AdvancedWiringKit.removeUnlockTrigger();

			addItemToRecipe(TechType.PrecursorKey_Purple, CraftingItems.getItem(CraftingItems.Items.DimLuminol).TechType, 1);
			//addItemToRecipe(TechType.PrecursorKey_Purple, TechType.PurpleStalkSeed, 2);
			addItemToRecipe(TechType.PrecursorKey_Orange, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2);
			addItemToRecipe(TechType.PrecursorKey_Blue, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 3);
			addItemToRecipe(TechType.PrecursorKey_Red, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1);
			addItemToRecipe(TechType.PrecursorKey_White, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 5);
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

			if (hard) {
				RecipeUtil.removeRecipe(TechType.SeamothSolarCharge, true);
				TechType.SeamothElectricalDefense.removeUnlockTrigger();
				RecipeUtil.clearIngredients(TechType.SeamothElectricalDefense);
				addItemToRecipe(TechType.SeamothElectricalDefense, C2CItems.t2Battery.TechType, 1);
				addItemToRecipe(TechType.SeamothElectricalDefense, TechType.AdvancedWiringKit, 1);
				addItemToRecipe(TechType.SeamothElectricalDefense, TechType.CopperWire, 3);
				CraftDataHandler.SetItemSize(TechType.PowerCell, new Vector2int(1, 2));
				CraftDataHandler.SetItemSize(TechType.LaserCutter, new Vector2int(2, 1));

				//removeVanillaUnlock(TechType.CyclopsShieldModule);
			}
			CraftDataHandler.SetItemSize(TechType.PrecursorIonPowerCell, new Vector2int(1, 2));
			CraftDataHandler.SetItemSize(TechType.Jumper, new Vector2int(1, 1));
			CraftDataHandler.SetItemSize(TechType.ExosuitDrillArmModule, new Vector2int(2, 2));
			CraftDataHandler.SetItemSize(TechType.ExosuitPropulsionArmModule, new Vector2int(2, 2));
			CraftDataHandler.SetItemSize(TechType.ExosuitGrapplingArmModule, new Vector2int(2, 2));
			CraftDataHandler.SetItemSize(C2CItems.depth1300.TechType, new Vector2int(2, 2));

			CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, "Personal", "Equipment", "PrecursorKey_Purple");
			CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorKey_Purple, "Machines");
			CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, "Personal", "Equipment", "PrecursorKey_Orange");
			CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorKey_Orange, "Machines");
			CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, "Personal", "Equipment", "PrecursorKey_Blue");
			CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorKey_Blue, "Machines");

			foreach (FieldInfo f in typeof(C2CItems).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)) {
				if (f.FieldType.IsSubclassOf(typeof(ModPrefab))) {
					ModPrefab pfb = (ModPrefab)f.GetValue(null);
					if (pfb == null)
						continue;
					specialRecipes.Add(pfb.TechType);
					lockRecipe(pfb.TechType, false);
				}
			}

			foreach (CraftingItems.Items tt in Enum.GetValues(typeof(CraftingItems.Items)))
				lockRecipe(CraftingItems.getItem(tt).TechType, false);

			foreach (TechType tt in specialRecipes)
				lockRecipe(tt);

			lockRecipe(TechType.Seamoth, false);
			lockRecipe(TechType.Cyclops, false);
			lockRecipe(TechType.Exosuit, false);
			lockRecipe(TechType.LaserCutter, false);
			lockRecipe(TechType.PowerCell, false);
			lockRecipe(TechType.ReinforcedDiveSuit, false);

			//RecipeUtil.logChangedRecipes();
		}

		public static void replaceFiberMeshWithMicroFilter(TechType tt) {
			RecipeUtil.addIngredient(tt, CraftingItems.getItem(CraftingItems.Items.MicroFilter).TechType, RecipeUtil.removeIngredient(tt, TechType.FiberMesh).amount);
		}

		private static void addItemToRecipe(TechType tt, TechType add, int amt = 1) {
			RecipeUtil.addIngredient(tt, add, amt);
			specialRecipes.Add(tt);
		}

		private static void addItemToRecipe(TechType tt, TechData td, TechType add, int amt = 1) {
			RecipeUtil.addIngredient(td, add, amt);
			specialRecipes.Add(tt);
		}

		public static void lockRecipe(TechType tt, bool allowAdd = true) {
			TechData r = CraftDataHandler.GetTechData(tt);
			if (r == null || r.Ingredients is LockedRecipeList)
				return;
			r.Ingredients = new LockedRecipeList(r, allowAdd);
		}

		private static void createCompressedIngot(DIPrefab<VanillaResources> item, string pfbMdl, float specInt, float shiny, float fresnel, int amt = 10, string name = "Ingot") {
			string n = ((ModPrefab)item).ClassID;
			createCompressedIngot(((ModPrefab)item).TechType, pfbMdl, specInt, shiny, fresnel, n.Substring(0, 1) + n.Substring(1).ToLowerInvariant(), amt, name, item.getIcon());
		}

		private static void createCompressedIngot(TechType item, string pfbMdl, float specInt, float shiny, float fresnel, int amt = 10, string name = "Ingot") {
			createCompressedIngot(item, pfbMdl, specInt, shiny, fresnel, "" + item, amt, name);
		}

		private static void createCompressedIngot(TechType item, string pfbMdl, float specInt, float shiny, float fresnel, string refName, int amt = 10, string name = "Ingot", Atlas.Sprite spr = null) {
			string pref = name[0] == 'A' || name[0] == 'E' || name[0] == 'I' || name[0] == 'O' || name[0] == 'U' ? "An " : "A ";
			BasicCraftingItem ingot = new BasicCraftingItem("ingot_"+item, refName+" "+name, pref+name.ToLowerInvariant()+" of compressed "+refName.ToLowerInvariant()+".", pfbMdl);
			ingot.addIngredient(item, amt);
			ingot.craftingSubCategory = "C2CIngots";
			ingot.craftingTime = CraftData.craftingTimes[TechType.TitaniumIngot];
			ingot.unlockRequirement = TechType.Unobtanium;
			ingot.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/Ingot/" + refName.ToLowerInvariant());
			ingot.renderModify = r => {
				//if (item == TechType.MercuryOre)
				//	RenderUtil.copyTextures(VanillaResources.LARGE_MERCURY.prefab, r);
				//else
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Items/World/Ingot/" + item);
				RenderUtil.setGlossiness(r, specInt, shiny, fresnel);
				if (item == TechType.Quartz || item == TechType.Diamond) {
					RenderUtil.makeTransparent(r);
					r.materials[0].EnableKeyword("UWE_DETAILMAP");
				}
				if (item == TechType.Quartz || item == TechType.AluminumOxide || item == TechType.Kyanite) {
					float f = item == TechType.Quartz ? 0.3F : (item == TechType.Kyanite ? 9 : 6);
					RenderUtil.setEmissivity(r, f * 0.67F, f);
				}
				if (item == TechType.Magnetite) {
					GameObject root = r.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
					root.EnsureComponent<Magnetic>();
					root.GetComponent<WorldForces>().underwaterGravity *= 0.67F;
				}
				if (item == TechType.Salt) {
					GameObject root = r.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
					SaltGrindable cg = root.EnsureComponent<SaltGrindable>();
					cg.techType = C2CItems.getIngot(TechType.Salt).ingot; //fetch not use local var because patch() is not called yet
																		  //cg.chooseRandomResource = (cg0) => ObjectUtil.lookupPrefab(Ecocean.MushroomVaseStrand.filterDrops.getRandomEntry());
																		  //cg.dropTable = Ecocean.MushroomVaseStrand.filterDrops;
					/*
                    cg.resourceChoice.AddListener(res => {
                        res.drop = ObjectUtil.lookupPrefab(Ecocean.MushroomVaseStrand.filterDrops.getRandomEntry());
                    });*/
					cg.numberToYieldMin = 1;
					cg.numberToYieldMax = 2;
				}
				PrefabIdentifier pi = r.gameObject.FindAncestor<PrefabIdentifier>();
				pi.gameObject.removeComponent<Eatable>();
				if (item == TechType.MercuryOre)
					r.transform.localScale *= 1.5F;
			};
			//ingot.ownerMod = modDLL;
			ingot.Patch();
			SNUtil.log("Added compressed ingot for " + refName + ": " + ingot.TechType + " @ " + ingot.FabricatorType + " > " + string.Join("/", ingot.StepsToFabricatorTab));

			TechData rec = new TechData();
			rec.Ingredients.Add(new Ingredient(ingot.TechType, 1));
			DuplicateRecipeDelegateWithRecipe unpack = new DuplicateRecipeDelegateWithRecipe(item, rec);
			unpack.craftTime = 3;
			unpack.craftingType = CraftTree.Type.Fabricator;
			unpack.category = C2CItems.ingotCategory;
			unpack.group = TechGroup.Resources;
			unpack.unlock = TechType.Unobtanium;
			unpack.craftingMenuTree = new string[] { "Resources", "C2CIngots2" };
			unpack.ownerMod = SeaToSeaMod.modDLL;
			if (spr != null)
				unpack.sprite = spr;
			unpack.setRecipe(amt);
			unpack.Patch();

			C2CItems.addIngot(item, ingot, unpack, amt);
		}

		class SaltGrindable : CustomGrindable {

			public override GameObject chooseRandomResource() {
				TechType tt = Ecocean.MushroomVaseStrand.filterDrops.getRandomEntry();
				while (tt == CraftingItems.getItem(CraftingItems.Items.Tungsten).TechType && !Story.StoryGoalManager.main.IsGoalComplete(C2CProgression.TUNGSTEN_GOAL))
					tt = Ecocean.MushroomVaseStrand.filterDrops.getRandomEntry();
				return ObjectUtil.lookupPrefab(tt);
			}

		}

		public static DuplicateRecipeDelegateWithRecipe getAlternateEnzyme() {
			return enzymeAlternate;
		}

		public static DuplicateRecipeDelegateWithRecipe getAlternateFiber() {
			return altFiberMesh;
		}

		public static DuplicateRecipeDelegateWithRecipe getAltSulfurAcid() {
			return altSulfurAcid;
		}
		/*
        public static DuplicateRecipeDelegateWithRecipe getAltLuminol() {
            return altLuminol;
        }
        */
		public static DuplicateRecipeDelegateWithRecipe getAltTraceMetal() {
			return traceMetalAlternate;
		}

		public static DuplicateRecipeDelegateWithRecipe getAlternateBacteria() {
			return bacteriaAlternate;
		}

		public static DuplicateRecipeDelegateWithRecipe getQuartzIngotToGlass() {
			return quartzIngotToGlass;
		}

		public static DuplicateRecipeDelegateWithRecipe getT2BatteryRepair() {
			return t2BatteryRepair;
		}
		/*
        public static DuplicateRecipeDelegateWithRecipe getPropGunDeConversion() {
            return replGunDeConversion;
        }*/
		/*
        public static TechData getHatchingEnzymeRecipe() {
            return hatchingEnzymesReplacement;
        }*/

		public static CraftTree.Type getHatchingEnzymeFab() {
			return precursorEnzymeFab;
		}

		class LockedRecipeList : List<Ingredient> {

			public bool allowAdd { get; private set; }

			internal LockedRecipeList(TechData r, bool add = false) : base(r.Ingredients) {
				allowAdd = add;
			}

			new void Add(Ingredient i) {
				if (allowAdd)
					base.Add(i);
				else
					throw new Exception("Unsupported operation");
			}

			new void Insert(int idx, Ingredient i) {
				if (allowAdd)
					base.Insert(idx, i);
				else
					throw new Exception("Unsupported operation");
			}

			new void Remove(Ingredient i) {
				throw new Exception("Unsupported operation");
			}

			new void RemoveAt(int idx) {
				throw new Exception("Unsupported operation");
			}

			new void Clear() {
				throw new Exception("Unsupported operation");
			}

			public new Ingredient this[int index] {
				get {
					return base[index];
				}
				set {
					throw new Exception("Unsupported operation");
				}
			}

		}

	}
}
