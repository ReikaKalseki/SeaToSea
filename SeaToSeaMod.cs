using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{
  [QModCore]
  public static class SeaToSeaMod
  {
    public const string MOD_KEY = "ReikaKalseki.SeaToSea";
    
    public static readonly Config<C2CConfig.ConfigEntries> config = new Config<C2CConfig.ConfigEntries>();
    public static readonly XMLLocale itemLocale = new XMLLocale("XML/items.xml");
    public static readonly XMLLocale pdaLocale = new XMLLocale("XML/pda.xml");
    public static readonly XMLLocale signalLocale = new XMLLocale("XML/signals.xml");
    public static readonly XMLLocale miscLocale = new XMLLocale("XML/misc.xml");
    
    public static SeamothVoidStealthModule voidStealth;
    public static CyclopsHeatModule cyclopsHeat;
    public static SeamothDepthModule depth1300;
    public static CustomEquipable sealSuit;
    public static CustomEquipable rebreatherV2;
    public static CustomBattery t2Battery;
    
    public static AlkaliPlant alkali;
    
    public static Bioprocessor processor;
    
    public static SignalManager.ModSignal treaderSignal;
    
    public static Story.StoryGoal crashMesaRadio;
    public static Story.StoryGoal voidSpikePDA;
    //public static Story.StoryGoal mountainPodRadio;
    
    public static BrokenTablet brokenRedTablet;
    public static BrokenTablet brokenWhiteTablet;
    public static BrokenTablet brokenOrangeTablet;
    public static BrokenTablet brokenBlueTablet;
    
    public static FMODAsset voidspikeLeviRoar;
    public static FMODAsset voidspikeLeviBite;
    public static FMODAsset voidspikeLeviFX;
    public static FMODAsset voidspikeLeviAmbient;

    [QModPatch]
    public static void Load()
    {
        config.load();
        
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SNUtil.log("Ran mod register, started harmony");
        try {
        	harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }
        catch (Exception ex) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(ex.Message);
			FileLog.Log(ex.StackTrace);
			FileLog.Log(ex.ToString());
        }
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WorldGenerator).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PlacedObject).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CustomPrefab).TypeHandle);
        
        itemLocale.load();
        pdaLocale.load();
        signalLocale.load();
        miscLocale.load();
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(VoidSpike).TypeHandle);
        
        processor = new Bioprocessor();
        processor.Patch();
        SNUtil.log("Registered custom machine "+processor);
        
	    brokenRedTablet = new BrokenTablet(TechType.PrecursorKey_Red);
	    brokenWhiteTablet = new BrokenTablet(TechType.PrecursorKey_White);
	    brokenOrangeTablet = new BrokenTablet(TechType.PrecursorKey_Orange);
	    brokenBlueTablet = new BrokenTablet(TechType.PrecursorKey_Blue);
	    brokenRedTablet.Patch();
	    brokenWhiteTablet.Patch();
	    brokenOrangeTablet.Patch();
	    brokenBlueTablet.Patch();
	    
	    voidspikeLeviRoar = SoundManager.registerSound("voidspikelevi_roar", "Sounds/voidlevi-roar.ogg", SoundSystem.masterBus);
	    voidspikeLeviFX = SoundManager.registerSound("voidspikelevi_fx", "Sounds/voidlevi-fx1.ogg", SoundSystem.masterBus);
	    voidspikeLeviAmbient = SoundManager.registerSound("voidspikelevi_amb", "Sounds/voidlevi-longamb2.ogg", SoundSystem.masterBus);
	    voidspikeLeviBite = SoundManager.registerSound("voidspikelevi_bite", "Sounds/voidlevi-bite.ogg", SoundSystem.masterBus);
        
        addFlora();
        addItemsAndRecipes();
        addPDAEntries();
                 
        WorldgenDatabase.instance.load();
        DataboxTypingMap.instance.load();
        
        addCommands();
        addOreGen();
			
        XMLLocale.LocaleEntry e = SeaToSeaMod.signalLocale.getEntry("treaderpod");
		treaderSignal = SignalManager.createSignal(e);
		//treaderSignal.pdaEntry.addSubcategory("AuroraSurvivors");
		treaderSignal.addRadioTrigger(e.getField<string>("sound"));
		treaderSignal.register("32e48451-8e81-428e-9011-baca82e9cd32", new Vector3(-1239, -360, -1193));
		treaderSignal.addWorldgen();
		
		e = pdaLocale.getEntry("crashmesahint");
		crashMesaRadio = SNUtil.addRadioMessage("crashmesaradio", e.getField<string>("radio"), e.getField<string>("radioSound"), 1200);
		
		e = miscLocale.getEntry("voidspikeenter");
		voidSpikePDA = SNUtil.addVOLine(e.key, Story.GoalType.PDA, e.desc, SoundManager.registerSound("prompt_"+e.key, e.pda, SoundSystem.voiceBus), 5);
		
		KnownTech.onAdd += onTechUnlocked;
       
		//DamageSystem.acidImmune = DamageSystem.acidImmune.AddToArray<TechType>(TechType.Seamoth);
       
		VoidSpikesBiome.instance.register();
		VoidSpike.register();
		//AvoliteSpawner.instance.register();
    }
    
    private static void onTechUnlocked(TechType tech, bool vb) {
    	if (tech == TechType.PrecursorKey_Orange) {
    		Story.StoryGoal.Execute(SeaToSeaMod.crashMesaRadio.key, SeaToSeaMod.crashMesaRadio.goalType);
    	}
    }
    
    private static void addFlora() {
		alkali = new AlkaliPlant();
		alkali.Patch();	
		alkali.addPDAEntry(itemLocale.getEntry(alkali.ClassID).pda, 3);
		SNUtil.log(" > "+alkali);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Mountains_IslandCaveFloor, 1, 1F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Mountains_CaveFloor, 1, 0.5F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Dunes_CaveFloor, 1, 0.5F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.KooshZone_CaveFloor, 1, 2F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.SeaTreaderPath_CaveFloor, 1, 1F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.UnderwaterIslands_ValleyFloor, 1, 0.5F);
    }
    
    private static void addOreGen() {
    	BasicCustomOre vent = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL);
    	vent.registerWorldgen(BiomeType.Dunes_ThermalVent, 1, 3F);
    	vent.registerWorldgen(BiomeType.Mountains_ThermalVent, 1, 1.2F);
    	//vent.registerWorldgen(BiomeType.JellyshroomCaves_Geyser, 1, 0.5F);
    	//vent.registerWorldgen(BiomeType.KooshZone_Geyser, 1, 1F);
    	//vent.registerWorldgen(BiomeType.GrandReef_ThermalVent, 1, 3F);
    	//vent.registerWorldgen(BiomeType.DeepGrandReef_ThermalVent, 1, 4F);
    	
    	BasicCustomOre irid = CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor, 1, 1.5F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor_Far, 1, 0.75F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Wall, 1, 0.25F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Chamber_Ceiling, 1, 2F);
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MAGNETITE.prefab, BiomeType.UnderwaterIslands_Geyser, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_MAGNETITE.prefab, BiomeType.UnderwaterIslands_Geyser, 0.2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LITHIUM.prefab, BiomeType.UnderwaterIslands_Geyser, 1.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.QUARTZ.prefab, BiomeType.UnderwaterIslands_Geyser, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.DIAMOND.prefab, BiomeType.UnderwaterIslands_Geyser, 1F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.QUARTZ.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LITHIUM.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 1F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_QUARTZ.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 0.2F, 1);
    	
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.LostRiverCorridor_LakeFloor, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.LostRiverJunction_LakeFloor, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_Corridor_Stream, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_Lake_Floor, 0.2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_LakePit_Floor, 0.4F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_LakePit_Wall, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.SkeletonCave_Lake_Floor, 0.2F, 1);
    	vent.registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 0.5F);
    	//CustomMaterials.getItem(CustomMaterials.Materials.).registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 8F);
    	/*
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Dunes_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Mountains_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.GrandReef_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.DeepGrandReef_ThermalVent, 2F, 1);*/
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_MERCURY.prefab, BiomeType.KooshZone_CaveSpecial, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.KooshZone_CaveSpecial, 4F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.KooshZone_CaveFloor, 0.75F, 1);
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    }
    
    private static void addCommands() {
        BuildingHandler.instance.addCommand<string>("pfb", BuildingHandler.instance.spawnPrefabAtLook);
        //BuildingHandler.instance.addCommand<string>("btt", BuildingHandler.instance.spawnTechTypeAtLook);
        BuildingHandler.instance.addCommand<bool>("bden", BuildingHandler.instance.setEnabled);  
        BuildingHandler.instance.addCommand("bdsa", BuildingHandler.instance.selectAll);
        BuildingHandler.instance.addCommand("bdslp", BuildingHandler.instance.selectLastPlaced);
        BuildingHandler.instance.addCommand<string>("bdexs", BuildingHandler.instance.saveSelection);
        BuildingHandler.instance.addCommand<string>("bdexa", BuildingHandler.instance.saveAll);
        BuildingHandler.instance.addCommand<string>("bdld", BuildingHandler.instance.loadFile);
        BuildingHandler.instance.addCommand("bdinfo", BuildingHandler.instance.selectedInfo);
        BuildingHandler.instance.addCommand("bdtex", BuildingHandler.instance.dumpTextures);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, bool>>("sound", SNUtil.playSound);
       // ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("voidsig", VoidSpikesBiome.instance.activateSignal);
        //ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, string, string>>("exec", DebugExec.run);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("execTemp", DebugExec.tempCode);
    }
    
    private static void addItemsAndRecipes() {
        BasicCraftingItem comb = CraftingItems.getItem(CraftingItems.Items.HoneycombComposite);
        comb.craftingTime = 12;
        comb.addIngredient(TechType.AramidFibers, 6).addIngredient(TechType.PlasteelIngot, 1);
        
        BasicCraftingItem gem = CraftingItems.getItem(CraftingItems.Items.DenseAzurite);
        gem.craftingTime = 4;
        gem.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL), 9).addIngredient(TechType.Diamond, 1).addIngredient(TechType.Magnetite, 5);
        
        BasicCraftingItem lens = CraftingItems.getItem(CraftingItems.Items.CrystalLens);
        lens.craftingTime = 20;
        lens.addIngredient(gem, 5).addIngredient(TechType.TitaniumIngot, 2).addIngredient(TechType.AdvancedWiringKit, 1).addIngredient(TechType.FiberMesh, 4);
        
        BasicCraftingItem sealedFabric = CraftingItems.getItem(CraftingItems.Items.SealFabric);
        sealedFabric.craftingTime = 4;
        sealedFabric.numberCrafted = 2;
        sealedFabric.addIngredient(CraftingItems.getItem(CraftingItems.Items.Sealant), 5).addIngredient(TechType.AramidFibers, 3).addIngredient(TechType.StalkerTooth, 1).addIngredient(TechType.Silicone, 2);
        
        BasicCraftingItem armor = CraftingItems.getItem(CraftingItems.Items.HullPlating);
        armor.craftingTime = 9;
        armor.addIngredient(TechType.PlasteelIngot, 2).addIngredient(TechType.Lead, 5).addIngredient(comb, 1).addIngredient(TechType.Nickel, 2);
        
        CraftingItems.addAll();
        
        voidStealth = new SeamothVoidStealthModule();
        voidStealth.addIngredient(lens, 1).addIngredient(comb, 2).addIngredient(TechType.Aerogel, 12);
        voidStealth.Patch();
        
        depth1300 = new SeamothDepthModule("SMDepth4", "Seamoth Depth Module MK4", "Increases crush depth to 1300m.", 1300);
        depth1300.addIngredient(TechType.VehicleHullModule3, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS), 4).addIngredient(CraftingItems.getItem(CraftingItems.Items.HullPlating), 3);
        depth1300.Patch();
        
        cyclopsHeat = new CyclopsHeatModule();
        cyclopsHeat.addIngredient(TechType.CyclopsThermalReactorModule, 1).addIngredient(TechType.CyclopsFireSuppressionModule, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 12).addIngredient(CraftingItems.getItem(CraftingItems.Items.Sealant), 4);
        cyclopsHeat.Patch();
        /*
        CraftData.itemSizes[TechType.AcidMushroom] = new Vector2int(1, 2);
        CraftData.itemSizes[TechType.HydrochloricAcid] = new Vector2int(2, 2);
        RecipeUtil.modifyIngredients(TechType.HydrochloricAcid, i => i.amount = 12);
        */
		RecipeUtil.removeRecipe(TechType.HydrochloricAcid);
		RecipeUtil.removeRecipe(TechType.Benzene);
		
        sealSuit = new SealedSuit();
        sealSuit.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 9).addIngredient(CraftingItems.getItem(CraftingItems.Items.SealFabric), 6).addIngredient(TechType.Titanium, 1).addIngredient(TechType.CrashPowder, 3);
        sealSuit.Patch();
		
		t2Battery = new CustomBattery(itemLocale.getEntry("t2battery"), 500);
		t2Battery.unlockRequirement = TechType.Kyanite;//CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType;
        t2Battery.addIngredient(CraftingItems.getItem(CraftingItems.Items.DenseAzurite), 1).addIngredient(TechType.Polyaniline, 1).addIngredient(TechType.MercuryOre, 2).addIngredient(TechType.Lithium, 2).addIngredient(TechType.Silicone, 1);
		t2Battery.Patch();
		
        rebreatherV2 = new RebreatherV2();
        rebreatherV2.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 6).addIngredient(TechType.Benzene, 12).addIngredient(TechType.Silicone, 3).addIngredient(TechType.Rebreather, 1).addIngredient(t2Battery, 1);
        rebreatherV2.Patch();
        
        RecipeUtil.addIngredient(TechType.Rebreather, TechType.Titanium, 3);
        RecipeUtil.addIngredient(TechType.Rebreather, TechType.AdvancedWiringKit, 1);
        RecipeUtil.addIngredient(TechType.Rebreather, TechType.EnameledGlass, 1);
        RecipeUtil.removeIngredient(TechType.Rebreather, TechType.WiringKit);
        
        //RecipeUtil.addIngredient(TechType.Polyaniline, TechType.Salt, 2);
        
        RecipeUtil.addIngredient(TechType.StasisRifle, CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 4);
        RecipeUtil.removeIngredient(TechType.StasisRifle, TechType.Battery);
        RecipeUtil.addIngredient(TechType.StasisRifle, t2Battery.TechType, 2);
        
        RecipeUtil.modifyIngredients(TechType.ReinforcedDiveSuit, i => {if (i.techType == TechType.Diamond) i.amount = 4; return i.techType == TechType.Titanium;});
        RecipeUtil.addIngredient(TechType.ReinforcedDiveSuit, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 9);
        RecipeUtil.addIngredient(TechType.ReinforcedDiveSuit, sealSuit.TechType, 1);
        
        RecipeUtil.addIngredient(TechType.Cyclops, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 3);
        RecipeUtil.addIngredient(TechType.Exosuit, CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 4);
        
        RecipeUtil.removeIngredient(TechType.ExoHullModule1, TechType.PlasteelIngot);
        RecipeUtil.addIngredient(TechType.ExoHullModule1, TechType.Kyanite, 3);
        RecipeUtil.addIngredient(TechType.ExoHullModule1, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 2);
        RecipeUtil.addIngredient(TechType.ExoHullModule2, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 4);
        RecipeUtil.addIngredient(TechType.ExoHullModule2, CraftingItems.getItem(CraftingItems.Items.SmartPolymer).TechType, 3);
        RecipeUtil.removeIngredient(TechType.ExoHullModule2, TechType.Kyanite);
        RecipeUtil.removeIngredient(TechType.ExoHullModule2, TechType.Titanium);
        
        RecipeUtil.addIngredient(TechType.LaserCutter, TechType.AluminumOxide, 2);
        RecipeUtil.removeIngredient(TechType.LaserCutter, TechType.Battery);
        RecipeUtil.addIngredient(TechType.LaserCutter, t2Battery.TechType, 1);
        
        RecipeUtil.addIngredient(TechType.VehicleHullModule2, CraftingItems.getItem(CraftingItems.Items.HoneycombComposite).TechType, 1);
        //RecipeUtil.addIngredient(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 2);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.HoneycombComposite).TechType, 2);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 1);
        RecipeUtil.removeIngredient(TechType.VehicleHullModule3, TechType.PlasteelIngot);
        //RecipeUtil.removeIngredient(TechType.VehicleHullModule3, TechType.AluminumOxide);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, TechType.Diamond, 1);
        
        RecipeUtil.addIngredient(TechType.PrecursorKey_Blue, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, 1);
        
        //RecipeUtil.modifyIngredients(TechType.EnameledGlass, i => {i.amount *= 2; return false;});
        //RecipeUtil.getRecipe(TechType.EnameledGlass).craftAmount *= 2;
        RecipeUtil.addIngredient(TechType.EnameledGlass, TechType.Lead, 2);
        RecipeUtil.addIngredient(TechType.EnameledGlass, TechType.Diamond, 1);
        RecipeUtil.addIngredient(TechType.AdvancedWiringKit, TechType.MercuryOre, 1);
        
        RecipeUtil.addIngredient(TechType.Bleach, CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType, 1);
        RecipeUtil.addIngredient(TechType.BaseFiltrationMachine, TechType.Bleach, 2);
        RecipeUtil.addIngredient(TechType.BaseFiltrationMachine, TechType.AdvancedWiringKit, 1);
        RecipeUtil.removeIngredient(TechType.BaseFiltrationMachine, TechType.CopperWire);
        RecipeUtil.addIngredient(TechType.BaseFiltrationMachine, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 1);
        
        RecipeUtil.addRecipe(TechType.PrecursorKey_Red, TechGroup.Personal, TechCategory.Equipment, 1, CraftTree.Type.Fabricator, new string[]{"Personal", "Equipment"});
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.PrecursorIonCrystal, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.MercuryOre, 3);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.Benzene, 1);
        CraftDataHandler.SetItemSize(TechType.PrecursorKey_Red, new Vector2int(2, 2));
        CraftDataHandler.SetCraftingTime(TechType.PrecursorKey_Red, 6);        
        //SNUtil.addSelfUnlock(TechType.PrecursorKey_Red, PDAManager.createPage(locale.getEntry("redkey")));
        
        RecipeUtil.addRecipe(TechType.PrecursorKey_White, TechGroup.Personal, TechCategory.Equipment, 1, CraftTree.Type.Fabricator, new string[]{"Personal", "Equipment"});
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, TechType.PrecursorIonCrystal, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, TechType.Magnetite, 3);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, TechType.UraniniteCrystal, 2);
        CraftDataHandler.SetCraftingTime(TechType.PrecursorKey_White, 8);        
        //SNUtil.addSelfUnlock(TechType.PrecursorKey_White, PDAManager.createPage(locale.getEntry("whitekey")));
        
        RecipeUtil.modifyIngredients(TechType.BaseReinforcement, i => true);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, TechType.PlasteelIngot, 1);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 1);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, TechType.Lead, 2);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, TechType.FiberMesh, 1);
        Base.FaceHullStrength[(int)Base.FaceType.Reinforcement] = 25; //from 7
        Base.FaceHullStrength[(int)Base.FaceType.BulkheadOpened] = 6; //from 3
        Base.FaceHullStrength[(int)Base.FaceType.BulkheadClosed] = 6; //from 3
        Base.CellHullStrength[(int)Base.CellType.Foundation] = 5; //from 2
        
        brokenBlueTablet.register();
        brokenRedTablet.register();
        brokenWhiteTablet.register();
        brokenOrangeTablet.register();
        
        KnownTechHandler.Main.RemoveAllCurrentAnalysisTechEntry(TechType.VehicleHullModule3);
        
        RecipeUtil.addIngredient(TechType.PrecursorKey_Purple, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Orange, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Blue, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 3);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 5);
        
        Bioprocessor.addRecipes();
    }
    
    public static void addPDAEntries() {
    	foreach (XMLLocale.LocaleEntry e in pdaLocale.getEntries()) {
			PDAManager.PDAPage page = PDAManager.createPage(e);
			if (e.hasField("audio"))
				page.setVoiceover(e.getField<string>("audio"));
			if (e.hasField("header"))
				page.setHeaderImage(TextureManager.getTexture("Textures/PDA/"+e.getField<string>("header")));
			page.register();
    	}
    }
   
	public static bool hasNoGasMask() {
   		return Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0;
	}

  }
}
