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

namespace ReikaKalseki.SeaToSea
{
  [QModCore]
  public static class SeaToSeaMod {
  	
    public const string MOD_KEY = "ReikaKalseki.SeaToSea";
    
    //public static readonly ModLogger logger = new ModLogger();
	public static readonly Assembly modDLL = Assembly.GetExecutingAssembly();
    
    internal static readonly Config<C2CConfig.ConfigEntries> config = new Config<C2CConfig.ConfigEntries>();
    internal static readonly XMLLocale itemLocale = new XMLLocale("XML/items.xml");
    internal static readonly XMLLocale pdaLocale = new XMLLocale("XML/pda.xml");
    internal static readonly XMLLocale signalLocale = new XMLLocale("XML/signals.xml");
    internal static readonly XMLLocale miscLocale = new XMLLocale("XML/misc.xml");
    
    public static readonly TechnologyFragment[] rebreatherChargerFragments = new TechnologyFragment[]{
    	new TechnologyFragment("f350b8ae-9ee4-4349-a6de-d031b11c82b1", go => go.transform.localScale = new Vector3(1, 3, 1)),
    	new TechnologyFragment("f744e6d9-f719-4653-906b-34ed5dbdb230", go => go.transform.localScale = new Vector3(1, 2, 1)),
  		//new TechnologyFragment("589bf5a6-6866-4828-90b2-7266661bb6ed"),
  		new TechnologyFragment("3c076458-505e-4683-90c1-34c1f7939a0f", go => go.transform.localScale = new Vector3(1, 1, 0.2F)),
    };
    public static readonly TechnologyFragment[] bioprocFragments = new TechnologyFragment[]{
    	new TechnologyFragment("85259b00-2672-497e-bec9-b200a1ab012f"),
    	//new TechnologyFragment("ba258aad-07e9-4c9b-b517-2ce7400db7b2"),
    	//new TechnologyFragment("cf4ca320-bb13-45b6-b4c9-2a079023e787"),
    	new TechnologyFragment("f4b3942e-02d8-4526-b384-677a2ad9ce58", go => go.transform.localScale = new Vector3(0.25F, 0.25F, 0.5F)),
    	new TechnologyFragment("f744e6d9-f719-4653-906b-34ed5dbdb230"),
    };
    
    public static TechnologyFragment lathingDroneFragment;
    
    internal static DeepStalker deepStalker;
    internal static VoidSpikeLeviathan voidSpikeLevi;
    
    internal static Bioprocessor processor;
    internal static RebreatherRecharger rebreatherCharger;
    
    public static SignalManager.ModSignal treaderSignal;
    public static SignalManager.ModSignal voidSpikeDirectionHint;
    //public static SignalManager.ModSignal duneArchWreckSignal;
    
    public static Story.StoryGoal crashMesaRadio;
    //public static Story.StoryGoal duneArchRadio;
    //public static Story.StoryGoal mountainPodRadio;
    
    public static PowerSealModuleFragment powersealModuleFragment;
    /*
    public static SoundManager.SoundData voidspikeLeviRoar;
    public static SoundManager.SoundData voidspikeLeviBite;
    public static SoundManager.SoundData voidspikeLeviFX;
    public static SoundManager.SoundData voidspikeLeviAmbient;
    */
    
    [QModPrePatch]
    public static void PreLoad() {
        config.load();
        
        C2CIntegration.injectConfigValues();
    }

    [QModPatch]
    public static void Load() {        
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log.txt");
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SNUtil.log("Ran mod register, started harmony");
        try {
        	harmony.PatchAll(modDLL);
        }
        catch (Exception ex) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(ex.Message);
			FileLog.Log(ex.StackTrace);
			FileLog.Log(ex.ToString());
        }
        
        CustomPrefab.addPrefabNamespace("ReikaKalseki.SeaToSea");
                
        itemLocale.load();
        pdaLocale.load();
        signalLocale.load();
        miscLocale.load();
        
        C2CItems.preAdd();
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(VoidSpike).TypeHandle);
	    
	   // voidspikeLeviRoar = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_roar", "Sounds/voidlevi-roar.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
	    //voidspikeLeviFX = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_fx", "Sounds/voidlevi-fx1.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
	   // voidspikeLeviAmbient = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_amb", "Sounds/voidlevi-longamb2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
	    //voidspikeLeviBite = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_bite", "Sounds/voidlevi-bite.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
        
	    deepStalker = new DeepStalker(itemLocale.getEntry("DeepStalker"));
	    deepStalker.register();
	    
	    //voidSpikeLevi = new VoidSpikeLeviathan(itemLocale.getEntry("VoidSpikeLevi"));
	    //voidSpikeLevi.register();
	    
        C2CItems.addFlora();
        C2CRecipes.addItemsAndRecipes();	
        C2CItems.addTablets();
	    powersealModuleFragment = new PowerSealModuleFragment();
	    powersealModuleFragment.register();
        
        BasicCraftingItem drone = CraftingItems.getItem(CraftingItems.Items.LathingDrone);
        lathingDroneFragment = TechnologyFragment.createFragment("6e0f4652-c439-4540-95be-e61384e27692", drone.TechType, drone.FriendlyName, 3, 2, go => {
        	ObjectUtil.removeComponent<Pickupable>(go);
        	//ObjectUtil.removeComponent<Collider>(go);
        	ObjectUtil.removeComponent<Rigidbody>(go);
        }); //it has its own model
        
        processor = new Bioprocessor();
        processor.Patch();       
        SNUtil.log("Registered custom machine "+processor);
        processor.addFragments(4, 5, bioprocFragments);
        Bioprocessor.addRecipes();
        
        rebreatherCharger = new RebreatherRecharger();
        rebreatherCharger.Patch();
        SNUtil.log("Registered custom machine "+rebreatherCharger);
        rebreatherCharger.addFragments(4, 7.5F, rebreatherChargerFragments);
        
        addPDAEntries();
                 
        new WorldgenDatabase().load();
        DataboxTypingMap.instance.load();
        
        addCommands();
        addOreGen();
        
        addSignalsAndRadio();
		
		PDAMessages.addAll();
		
		KnownTech.onAdd += onTechUnlocked;
       
		//DamageSystem.acidImmune = DamageSystem.acidImmune.AddToArray<TechType>(TechType.Seamoth);
		
		C2CItems.postAdd();
       
		VoidSpikesBiome.instance.register();
		UnderwaterIslandsFloorBiome.instance.register();
		VoidSpike.register();
		AvoliteSpawner.instance.register();
		
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(C2CUnlocks).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(C2CProgression).TypeHandle);
    }
    
    [QModPostPatch]
    public static void PostLoad() {
    	C2CIntegration.addPostCompat();
    }
    
    private static void addSignalsAndRadio() {			
        XMLLocale.LocaleEntry e = SeaToSeaMod.signalLocale.getEntry("treaderpod");
		treaderSignal = SignalManager.createSignal(e);
		treaderSignal.addRadioTrigger(e.getField<string>("sound"));
		treaderSignal.register("32e48451-8e81-428e-9011-baca82e9cd32", new Vector3(-1239, -360, -1193));
		treaderSignal.addWorldgen();
		/*
        e = SeaToSeaMod.signalLocale.getEntry("dunearch");
		duneArchWreckSignal = SignalManager.createSignal(e);
		duneArchWreckSignal.addRadioTrigger(e.getField<string>("sound"));
		duneArchWreckSignal.register("32e48451-8e81-428e-9011-baca82e9cd32", new Vector3(-1623, -355.6, -98.5));
		duneArchWreckSignal.addWorldgen();
		*/
        e = SeaToSeaMod.signalLocale.getEntry("voidspike");
		voidSpikeDirectionHint = SignalManager.createSignal(e);
		voidSpikeDirectionHint.setStoryGate(PDAManager.getPage("voidpod").id);
		voidSpikeDirectionHint.register("4c10bbd6-5100-4632-962e-69306b09222f", SpriteManager.Get(SpriteManager.Group.Pings, "Sunbeam"), VoidSpikesBiome.end500m);
		voidSpikeDirectionHint.addWorldgen();
		
		e = pdaLocale.getEntry("crashmesahint");
		crashMesaRadio = SNUtil.addRadioMessage("crashmesaradio", e.getField<string>("radio"), e.getField<string>("radioSound"));
    }
    
    private static void onTechUnlocked(TechType tech, bool vb) {/*
    	if (tech == TechType.PrecursorKey_Orange) {
    		Story.StoryGoal.Execute(SeaToSeaMod.crashMesaRadio.key, SeaToSeaMod.crashMesaRadio.goalType);
    	}
    	if (tech == TechType.NuclearReactor || tech == TechType.HighCapacityTank || tech == TechType.PrecursorKey_Purple || tech == TechType.SnakeMushroom || tech == CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType) {
    		Story.StoryGoal.Execute("RadioKoosh26", Story.GoalType.Radio); //pod 12
    	}*/
    	C2CItems.onTechUnlocked(tech);
    }
    
    private static void addOreGen() {
    	BasicCustomOre vent = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL);
    	vent.registerWorldgen(BiomeType.Dunes_ThermalVent, 1, 3F);
    	vent.registerWorldgen(BiomeType.Mountains_ThermalVent, 1, 1.0F);
    	//vent.registerWorldgen(BiomeType.JellyshroomCaves_Geyser, 1, 0.5F);
    	//vent.registerWorldgen(BiomeType.KooshZone_Geyser, 1, 1F);
    	//vent.registerWorldgen(BiomeType.GrandReef_ThermalVent, 1, 3F);
    	//vent.registerWorldgen(BiomeType.DeepGrandReef_ThermalVent, 1, 4F);
    	//vent.registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 0.5F);
    	
    	BasicCustomOre irid = CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Ceiling, 1, 1.2F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor, 1, 0.3F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor_Far, 1, 0.67F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Wall, 1, 0.6F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Chamber_Ceiling, 1, 0.5F);
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MAGNETITE.prefab, BiomeType.UnderwaterIslands_Geyser, 2.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_MAGNETITE.prefab, BiomeType.UnderwaterIslands_Geyser, 0.4F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_DIAMOND.prefab, BiomeType.UnderwaterIslands_Geyser, 0.25F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LITHIUM.prefab, BiomeType.UnderwaterIslands_Geyser, 1.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.QUARTZ.prefab, BiomeType.UnderwaterIslands_Geyser, 2.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.DIAMOND.prefab, BiomeType.UnderwaterIslands_Geyser, 1.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.QUARTZ.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 2.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LITHIUM.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 1F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_QUARTZ.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 0.33F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_MAGNETITE.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 0.15F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_DIAMOND.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 0.2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.UnderwaterIslands_Geyser, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_MERCURY.prefab, BiomeType.UnderwaterIslands_Geyser, 0.15F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 0.25F, 1);
    	
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.LostRiverCorridor_LakeFloor, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.LostRiverJunction_LakeFloor, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_Corridor_Stream, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_Lake_Floor, 0.2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_LakePit_Floor, 0.4F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_LakePit_Wall, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.SkeletonCave_Lake_Floor, 0.2F, 1);
    	//CustomMaterials.getItem(CustomMaterials.Materials.).registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 8F);
    	/*
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Dunes_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Mountains_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.GrandReef_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.DeepGrandReef_ThermalVent, 2F, 1);*/
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_MERCURY.prefab, BiomeType.KooshZone_CaveSpecial, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.KooshZone_CaveSpecial, 4F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.KooshZone_CaveFloor, 0.75F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.KooshZone_CaveWall, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.KooshZone_Geyser, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_MERCURY.prefab, BiomeType.KooshZone_Geyser, 0.125F, 1);
    	
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.Dunes_CaveFloor, 0.05F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.Mountains_CaveFloor, 0.05F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.ActiveLavaZone_Falls_Wall, 0.25F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.ActiveLavaZone_Falls_Floor, 0.25F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.ActiveLavaZone_Falls_Floor_Far, 0.4F, 1);    	
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.SeaTreaderPath_Path, 0.33F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.SeaTreaderPath_Path, 0.33F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.SeaTreaderPath_Path, 0.33F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.SeaTreaderPath_Path, 0.33F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
    	
    	foreach (BiomeType bb in Enum.GetValues(typeof(BiomeType))) {
    		LootDistributionHandler.EditLootDistributionData(VanillaResources.SULFUR.prefab, bb, 0, 1);
    	}
    }
    
    private static void addCommands() {
       // ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("voidsig", VoidSpikesBiome.instance.activateSignal);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string>>("signalUnlock", unlockSignal);
        
        //ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<float>>("spawnVKelp", spawnVentKelp);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("triggerVoidFX", f => VoidSpikeLeviathanSystem.instance.doDistantRoar(Player.main, true, f));
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("triggerVoidFlash", VoidSpikeLeviathanSystem.instance.doFlash);
    }
    /*
    private static void spawnVentKelp(float dist) {
		  GameObject obj = ObjectUtil.createWorldObject(C2CItems.kelp.ClassID, true, false);
		  obj.SetActive(false);
		  obj.transform.position = Player.main.transform.position+MainCamera.camera.transform.forward.normalized*dist;
		  LargeWorld.main.streamer.cellManager.RegisterEntity(obj);
		  obj.SetActive(true);
    }*/
    
    private static void unlockSignal(string name) {
    	switch(name) {
    		case "treaderpod":
    			treaderSignal.fireRadio();
    			break;
    		case "crashmesa":
    			Story.StoryGoal.Execute(SeaToSeaMod.crashMesaRadio.key, SeaToSeaMod.crashMesaRadio.goalType);
    			break;
    		case "voidpod":
    			VoidSpikesBiome.instance.fireRadio();
    			break;
    	}
    }
    
    private static void addPDAEntries() {
    	foreach (XMLLocale.LocaleEntry e in pdaLocale.getEntries()) {
			PDAManager.PDAPage page = PDAManager.createPage(e);
			if (e.hasField("audio"))
				page.setVoiceover(e.getField<string>("audio"));
			if (e.hasField("header"))
				page.setHeaderImage(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/PDA/"+e.getField<string>("header")));
			page.register();
    	}
    }
   /*
	public static bool hasNoGasMask() {
   		return Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0;
	}*/

  }
}
