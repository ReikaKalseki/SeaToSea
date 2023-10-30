using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Reflection;
using System.Xml;
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea
{
  [QModCore]
  public static class SeaToSeaMod {
  	
    public const string MOD_KEY = "ReikaKalseki.SeaToSea";
    
    //public static readonly ModLogger logger = new ModLogger();
	public static readonly Assembly modDLL = Assembly.GetExecutingAssembly();
    
    internal static readonly Config<C2CConfig.ConfigEntries> config = new Config<C2CConfig.ConfigEntries>(modDLL);
    internal static readonly XMLLocale itemLocale = new XMLLocale(modDLL, "XML/items.xml");
    internal static readonly XMLLocale pdaLocale = new XMLLocale(modDLL, "XML/pda.xml");
    internal static readonly XMLLocale signalLocale = new XMLLocale(modDLL, "XML/signals.xml");
    internal static readonly XMLLocale trackerLocale = new XMLLocale(modDLL, "XML/tracker.xml");
    internal static readonly XMLLocale miscLocale = new XMLLocale(modDLL, "XML/misc.xml");
    
    public static readonly WorldgenDatabase worldgen = new WorldgenDatabase();
    
    private static Dictionary<string, Dictionary<string, Texture2D>> degasiBaseTextures = new Dictionary<string, Dictionary<string, Texture2D>>();
    
    private static readonly Dictionary<Vector3, Tuple<float, int, float>> mercurySpawners = new Dictionary<Vector3, Tuple<float, int, float>>(){
    	{new Vector3(908.7F, -235.1F, 615.7F), Tuple.Create(2F, 4, 32F)},
    	{new Vector3(904.3F, -247F, 668.8F), Tuple.Create(1F, 3, 32F)},
    	{new Vector3(915.1F, -246.8F, 651.2F), Tuple.Create(2F, 6, 32F)},
    	{new Vector3(1273, -290, 604.3F), Tuple.Create(2F, 3, 32F)},
    	{new Vector3(1254, -293.3F, 606.3F), Tuple.Create(2F, 3, 32F)},
    	{new Vector3(1239, -286.4F, 617), Tuple.Create(2F, 3, 32F)},
    	{new Vector3(1245, -308.2F, 555.8F), Tuple.Create(2F, 3, 32F)},
    	{new Vector3(-1216, -299.1F, 510.3F), Tuple.Create(2F, 3, 32F)},
    	{new Vector3(1278, -276.4F, 497.5F), Tuple.Create(2F, 3, 32F)},
    	{new Vector3(1228, -275.6F, 483.9F), Tuple.Create(2F, 3, 32F)}
    };
    
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
    
    public static MushroomTreeBacterialColony mushroomBioFragment;    
    public static GeyserCoral geyserCoral;    
    public static PowerSealModuleFragment powersealModuleFragment;    
    public static EjectedHeatSink ejectedHeatSink;
    
    public static UnmovingHeatBlade thermoblade;
    public static MountainBaseCuredPeeper peeper;
    //public static SeaTreaderTunnelLocker locker;
    public static SeaTreaderTunnelLight tunnelLight;
    public static FallingGlassForestWreck gfWreckProp;
    public static DeadMelon deadMelon;
    public static Campfire campfire;
    public static Mattress mattress;
    public static MarshmallowCan marshCan;
    
    private static BloodKelpBaseNuclearReactorMelter reactorMelter;
    private static TrailerBaseConverter bioBreaker;
    private static MercuryLootSpawner mercuryLootSpawner;
    
    internal static CrashZoneSanctuarySpawner crashSanctuarySpawner;
    internal static SanctuaryGrassSpawner sanctuaryGrassSpawner;
    internal static CrashZoneSanctuaryFern crashSanctuaryFern;
    //internal static CrashZoneSanctuaryGrassBump sanctuaryGrassBump;
    //internal static CrashZoneSanctuaryCoralSheet sanctuaryCoral;
    
    internal static LRNestGrass lrNestGrass;
    
    public static DataChit laserCutterBulkhead;
    public static DataChit bioProcessorBoost;
   // public static DataChit vehicleSpeedBoost;
    
    //internal static VoidLeviElecSphere leviPulse;
    internal static DeepStalker deepStalker;
    internal static VoidSpikeLeviathan voidSpikeLevi;
    
    internal static Bioprocessor processor;
    internal static RebreatherRecharger rebreatherCharger;
    internal static GeyserFilter geyserFilter;
    
    public static SignalManager.ModSignal treaderSignal;
    public static SignalManager.ModSignal voidSpikeDirectionHint;
    //public static SignalManager.ModSignal duneArchWreckSignal;
    public static SignalManager.ModSignal sanctuaryDirectionHint;
    
    internal static Story.StoryGoal crashMesaRadio;
    //public static Story.StoryGoal duneArchRadio;
    //public static Story.StoryGoal mountainPodRadio;
    
    internal static Story.StoryGoal auroraTerminal;
    
    internal static Story.StoryGoal sunbeamCountdownTrigger;
    
    /*
    public static SoundManager.SoundData voidspikeLeviRoar;
    public static SoundManager.SoundData voidspikeLeviBite;
    public static SoundManager.SoundData voidspikeLeviFX;
    public static SoundManager.SoundData voidspikeLeviAmbient;
    */
   
   internal static bool anywhereSeamothModuleCheatActive = false;
   internal static bool trackerShowAllCheatActive = false;
   internal static bool fastSeaglideCheatActive = false;
    
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
        
        ModVersionCheck.getFromGitVsInstall("Sea To Sea", modDLL, "SeaToSea").register();
        SNUtil.checkModHash(modDLL);
        
        CustomPrefab.addPrefabNamespace("ReikaKalseki.SeaToSea");
        
        C2CIntegration.injectLoad();
                
        itemLocale.load();
        pdaLocale.load();
        signalLocale.load();
        trackerLocale.load();
        miscLocale.load();
        
        C2CItems.preAdd();
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(VoidSpike).TypeHandle);
	    
	   // voidspikeLeviRoar = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_roar", "Sounds/voidlevi-roar.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
	    //voidspikeLeviFX = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_fx", "Sounds/voidlevi-fx1.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
	   // voidspikeLeviAmbient = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_amb", "Sounds/voidlevi-longamb2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
	    //voidspikeLeviBite = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_bite", "Sounds/voidlevi-bite.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
        
	    deepStalker = new DeepStalker(itemLocale.getEntry("DeepStalker"));
	    deepStalker.register();
	    
	    voidSpikeLevi = new VoidSpikeLeviathan(itemLocale.getEntry("VoidSpikeLevi"));
	    voidSpikeLevi.register();
	    
	    XMLLocale.LocaleEntry e = miscLocale.getEntry("bulkheadLaserCutterUpgrade");
	    laserCutterBulkhead = new DataChit(e.key, e.name, e.desc, d => {d.controlText = e.pda; d.graphic = () => SNUtil.getTechPopupSprite(TechType.LaserCutter);});
	    laserCutterBulkhead.Patch();
	    e = miscLocale.getEntry("bioprocessorBoost");
	    bioProcessorBoost = new DataChit(e.key, e.name, e.desc, d => {d.controlText = e.pda;});
	    bioProcessorBoost.Patch();
	    
        C2CItems.addFlora();
        C2CRecipes.addItemsAndRecipes();	
        C2CItems.addTablets();
	    powersealModuleFragment = new PowerSealModuleFragment();
	    powersealModuleFragment.register();
	    ejectedHeatSink = new EjectedHeatSink();
	    ejectedHeatSink.Patch();
	    thermoblade = new UnmovingHeatBlade();
	    thermoblade.Patch();
	    peeper = new MountainBaseCuredPeeper();
	    peeper.Patch();
	    //locker = new SeaTreaderTunnelLocker();
	    //locker.Patch();
	    tunnelLight = new SeaTreaderTunnelLight();
	    tunnelLight.Patch();
	    gfWreckProp = new FallingGlassForestWreck();
	    gfWreckProp.Patch();
	    deadMelon = new DeadMelon();
	    deadMelon.Patch();
	    campfire = new Campfire();
	    campfire.Patch();
	    mattress = new Mattress();
	    mattress.Patch();
	    marshCan = new MarshmallowCan();
	    marshCan.Patch();
	    reactorMelter = new BloodKelpBaseNuclearReactorMelter();
	    reactorMelter.Patch();
	    bioBreaker = new TrailerBaseConverter();
	    bioBreaker.Patch();
	    
	    mushroomBioFragment = new MushroomTreeBacterialColony(itemLocale.getEntry("TREE_BACTERIA"));
	    mushroomBioFragment.register();
	    
	    mercuryLootSpawner = new MercuryLootSpawner();
	    mercuryLootSpawner.Patch();
	    crashSanctuarySpawner = new CrashZoneSanctuarySpawner();
	    crashSanctuarySpawner.Patch();
	    sanctuaryGrassSpawner = new SanctuaryGrassSpawner();
	    sanctuaryGrassSpawner.Patch();
	    crashSanctuaryFern = new CrashZoneSanctuaryFern();
	    crashSanctuaryFern.Patch();
	    //sanctuaryGrassBump = new CrashZoneSanctuaryGrassBump();
	    //sanctuaryGrassBump.Patch();
	    //sanctuaryCoral = new CrashZoneSanctuaryCoralSheet();
	    //sanctuaryCoral.Patch();
	    
	    lrNestGrass = new LRNestGrass();
	    lrNestGrass.Patch();
	    
	    //leviPulse = new VoidLeviElecSphere();
	    //leviPulse.Patch();
        
        BasicCraftingItem drone = CraftingItems.getItem(CraftingItems.Items.LathingDrone);
        lathingDroneFragment = TechnologyFragment.createFragment("6e0f4652-c439-4540-95be-e61384e27692", drone.TechType, drone.FriendlyName, 3, 2, true, go => {
        	ObjectUtil.removeComponent<Pickupable>(go);
        	ObjectUtil.removeComponent<Rigidbody>(go);
			go.EnsureComponent<LathingDroneSparker>();
        }); //it has its own model
        
       	e = itemLocale.getEntry("bioprocessor");
        processor = new Bioprocessor(e);
        processor.Patch();       
        SNUtil.log("Registered custom machine "+processor);
        processor.addPDAPage(e.pda, "Bioprocessor");
        processor.addFragments(4, 5, bioprocFragments);
        Bioprocessor.addRecipes();
        
        e = itemLocale.getEntry("rebreathercharger");
        rebreatherCharger = new RebreatherRecharger(e);
        rebreatherCharger.Patch();
        SNUtil.log("Registered custom machine "+rebreatherCharger);
        rebreatherCharger.addPDAPage(e.pda, "RebreatherCharger");
        rebreatherCharger.addFragments(4, 7.5F, rebreatherChargerFragments);
        
        e = itemLocale.getEntry("geyserfilter");
        geyserFilter = new GeyserFilter(e);
        geyserFilter.Patch();
        SNUtil.log("Registered custom machine "+geyserFilter);
        geyserFilter.addPDAPage(e.pda, "GeyserFilter");
	    
	    geyserCoral = new GeyserCoral(itemLocale.getEntry("GEYSER_CORAL"));
	    geyserCoral.register();
        
        addPDAEntries();
        
        addCommands();
        addOreGen();
        
        GenUtil.registerWorldgen(new PositionedPrefab(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).ClassID, Azurite.mountainBaseAzurite, Quaternion.Euler(0, 202.4F, 33.2F)));
        //GenUtil.registerWorldgen(new PositionedPrefab("e85adb0d-665e-48f5-9fa2-2dd316776864", C2CHooks.bkelpBaseGeoCenter), go => go.transform.localScale = Vector3.one*60);
        
        addSignalsAndRadio();
		
		PDAMessages.addAll();
		
		auroraTerminal = SNUtil.addVOLine("auroraringterminal_c2c", Story.GoalType.PDA, miscLocale.getEntry("auroraringterminal").desc, SoundManager.getSound("event:/player/story/Aurora_RingRoom_Terminal2"));
		
		sunbeamCountdownTrigger = new Story.StoryGoal("c2cTriggerSunbeamCountdown", Story.GoalType.Story, 0);
		
		KnownTech.onAdd += onTechUnlocked;
       
		//DamageSystem.acidImmune = DamageSystem.acidImmune.AddToArray<TechType>(TechType.Seamoth);
		
		C2CItems.postAdd();
       
		VoidSpikesBiome.instance.register();
		UnderwaterIslandsFloorBiome.instance.register();
		CrashZoneSanctuaryBiome.instance.register();
		VoidSpike.register();
		AvoliteSpawner.instance.register();
		
		C2CItems.alkali.addNativeBiome(VanillaBiomes.MOUNTAINS, true).addNativeBiome(VanillaBiomes.TREADER, true).addNativeBiome(VanillaBiomes.KOOSH, true);
		C2CItems.kelp.addNativeBiome(UnderwaterIslandsFloorBiome.instance);
		C2CItems.healFlower.addNativeBiome(VanillaBiomes.REDGRASS, true);
		C2CItems.sanctuaryPlant.addNativeBiome(CrashZoneSanctuaryBiome.instance);
		
		initHandlers();
        
        Vector3 ang = new Vector3(0, 317, 0);
        Vector3 pos1 = new Vector3(-1226, -350, -1258);
        Vector3 pos2 = new Vector3(-1327, -350, -1105);
        Vector3 tgt = pos2+(pos2-pos1).setLength(40);
        for (int i = 0; i <= 4; i++) {
        	Vector3 pos = Vector3.Lerp(pos1, pos2, i/4F);
        	GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.SEA_TREADER.prefab, pos, Quaternion.Euler(ang)), go => {
        		go.GetComponent<TreaderMoveOnSurface>().timeNextTarget = Time.time+120;
        		go.GetComponent<SeaTreader>().MoveTo(tgt);
        	});
        }
        
        SaveSystem.addPlayerSaveCallback(typeof(LiquidBreathingSystem).GetField("lastKharaaTreatmentTime", BindingFlags.Instance | BindingFlags.NonPublic), () => LiquidBreathingSystem.instance);
        
        SNUtil.addScanUnlock(TechType.PowerTransmitter, 2, TechType.PowerTransmitter, 1, false);
        SNUtil.addScanUnlock(TechType.LEDLight, 2, TechType.LEDLight, 1, false);
        SNUtil.addScanUnlock(TechType.ThermalPlant, 4, TechType.ThermalPlant, 1, false);
        SNUtil.addScanUnlock(TechType.NuclearReactor, 7, TechType.NuclearReactor, 1, false);
    }
    
    private static void initHandlers() {
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(C2CUnlocks).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(C2CProgression).TypeHandle);
    }
    
    [QModPostPatch]
    public static void PostLoad() {
        worldgen.load(); //load in post because some cross-mod TTs may not exist yet
		mushroomBioFragment.postRegister();
		geyserCoral.postRegister();
        DataboxTypingMap.instance.load();
        DataboxTypingMap.instance.addValue(-789.81, -216.10, -711.02, C2CItems.bandage.TechType);
        
    	foreach (BiomeType bb in Enum.GetValues(typeof(BiomeType))) {
    		LootDistributionHandler.EditLootDistributionData(VanillaResources.SULFUR.prefab, bb, 0, 1);
    		LootDistributionHandler.EditLootDistributionData(CustomEgg.getEgg(TechType.SpineEel).ClassID, bb, 0, 1);
    	}
        
        POITeleportSystem.instance.populate();
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ExplorationTrackerPages).TypeHandle);
        
    	C2CIntegration.addPostCompat();
    	
    	dumpAbandonedBaseTextures();
    	
    	Dictionary<string, bool> modsWithIssues = new Dictionary<string, bool>(){
    		{"CyclopsNuclearUpgrades", false},
    		{"CyclopsBioReactor", false},
    		{"RedBaron", true},
    		{"SeamothArms", true},
    		{"HabitatControlPanel", true},
    		{"MoreSeamothDepth", true},
    		{"CustomCraft2", true},
    		{"FCSAlterraHub", false},
    		{"SlotExtender", false},
    		{"WarpChip", false},
    		//{"Socknautica", false},
    		{"Socksfor1Monsters", false},
    		{"DADTankSubPack", true},
    		{"DWEquipmentBonanza", false},
    		{"SeaVoyager", false},
    		{"SubnauticaRandomiser", true},
    		{"EquivalentExchange", true},
    		{"Deathrun", false},
    	};
    	foreach (KeyValuePair<string, bool> kvp in modsWithIssues) {
    		if (QModManager.API.QModServices.Main.ModPresent(kvp.Key)) {
    			if (kvp.Value) {
    				string msg = "Mod '"+kvp.Key+"' detected. This mod is not compatible with SeaToSea, and cannot be used alongside it.";
    				SNUtil.createPopupWarning(msg, false/*, null, SNUtil.createPopupButton("OK")*/);
    				throw new Exception(msg);
    			}
    			else {
    				string msg = "SeaToSea: Mod '"+kvp.Key+"' detected. This mod will significantly alter the balance of your pack and risks completely breaking C2C progression.";
    				SNUtil.createPopupWarning(msg, false/*, null, SNUtil.createPopupButton("OK")*/);
    				SNUtil.log(msg+" You should remove this mod if possible when using SeaToSea.");
    			}
    		}
    	}
    	if (!QModManager.API.QModServices.Main.ModPresent("TerrainPatcher")) {
    		string msg = "TerrainPatcher may be an optional dependency for SeaToSea, but its use is strongly recommended to ensure biomes appear as intended.";
    		SNUtil.createPopupWarning(msg, true/*, SNUtil.createPopupButton("Download", () => {
				System.Diagnostics.Process.Start("https://github.com/Esper89/Subnautica-TerrainPatcher/releases/download/v0.4/TerrainPatcher-v0.4.zip");
				Application.Quit(64);
			}), SNUtil.createPopupButton("Ignore")*/);
    		SNUtil.log(msg+" You should add this mod if at all possible.");
    	}
    	if (!QModManager.API.QModServices.Main.ModPresent("AgonyRadialCraftingTabs")) {
    		string msg = "RadialTabs is recommended when using SeaToSea to ensure that all crafting nodes in fabricator UIs remain onscreen.";
    		SNUtil.createPopupWarning(msg, true/*, SNUtil.createPopupButton("Download", () => {
				System.Diagnostics.Process.Start("https://www.nexusmods.com/Core/Libs/Common/Widgets/ModRequirementsPopUp?id=2624&game_id=1155");
				Application.Quit(64);
			}), SNUtil.createPopupButton("Ignore")*/);
    		SNUtil.log(msg+" You should add this mod if at all possible.");
    	}
    }
    
    private static void dumpAbandonedBaseTextures() {
    	string[] prefabs = new string[]{
    		"026c39c1-d0cc-442c-aa42-e574c9c281b2",
			"0e394d55-da8c-4b3e-b038-979477ce77c1",
			"255ed3c3-1973-40c0-9917-d16dd9a7018d",
			"256a06d3-b861-487a-b8ac-050daa0d683d",
			"2921736c-c898-4213-9615-ea1a72e28178",
			"569f22e0-274d-49b0-ae5e-21ef0ce907ca",
			"99b164ac-dfb4-4a14-b305-8666fa227717",
			"c1139534-b3b9-4750-b60b-a77ca054b3dd",
			"dd923ae3-20f6-47e0-87c0-ae2bc386607a"
    	};
    	HashSet<string> exported = new HashSet<string>();
	    foreach (string s in prefabs) {
	    	GameObject go = ObjectUtil.lookupPrefab(s);
	    	if (go) {
	    		Renderer[] rr = go.GetComponentsInChildren<Renderer>(true);
	    		//SNUtil.log("Exporting degasi base textures from "+s+": "+rr.Length+":"+string.Join(", ", rr.Select(r2 => r2.name)), modDLL);
	    		foreach (Renderer r in rr) {
					foreach (Material m in r.materials) {
	    				if (m && m.mainTexture != null && m.mainTexture.name != null) {
		    				string n = m.mainTexture.name.Replace(" (Instance)", "").ToLowerInvariant();
							if (!exported.Contains(n)) {
								SNUtil.log("Exporting degasi base textures from "+r.gameObject.GetFullHierarchyPath()+": "+n, modDLL);
								degasiBaseTextures[n] = new Dictionary<string, Texture2D>();
								foreach (string tex in m.GetTexturePropertyNames())
									degasiBaseTextures[n][tex] = (Texture2D)m.GetTexture(tex);
								if (degasiBaseTextures[n].Count > 0)
									exported.Add(n);
							}
	    				}
					}
	    		}
	    	}
	    }
    }
    
    public static bool hasDegasiBaseTextures(string n) {
    	return degasiBaseTextures.ContainsKey(n);
    }
    
    public static Texture2D getDegasiBaseTexture(string n, string type) {
    	return degasiBaseTextures[n].ContainsKey(type) ? degasiBaseTextures[n][type] : null;
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
		
        e = SeaToSeaMod.signalLocale.getEntry("sanctuary");
		sanctuaryDirectionHint = SignalManager.createSignal(e);
		sanctuaryDirectionHint.register("4c10bbd6-5100-4632-962e-69306b09222f", SpriteManager.Get(SpriteManager.Group.Pings, "Sunbeam"), CrashZoneSanctuaryBiome.biomeCenter.setY(-360));
		sanctuaryDirectionHint.addWorldgen();
		
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
    	
    	foreach (KeyValuePair<Vector3, Tuple<float, int, float>> kvp in mercurySpawners) {
    		Tuple<float, int, float> vals = kvp.Value; //exclusion radius, target count, max range
    		int count = vals.Item2;
    		if (config.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
    			count = Math.Max(1, count*2/3);
    		GenUtil.registerWorldgen(new PositionedPrefab(mercuryLootSpawner.ClassID, kvp.Key, Quaternion.identity, new Vector3(vals.Item1, count, vals.Item3)));
    	}
    	
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.Dunes_CaveFloor, 0.05F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.Mountains_CaveFloor, 0.05F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.ActiveLavaZone_Falls_Wall, 0.25F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.ActiveLavaZone_Falls_Floor, 0.25F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.ActiveLavaZone_Falls_Floor_Far, 0.4F, 1);    	
    	/*
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	*/
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.SeaTreaderPath_Path, 0.33F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.SeaTreaderPath_Path, 0.33F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.SeaTreaderPath_Path, 0.33F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.SeaTreaderPath_Path, 0.33F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
    	
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_DIAMOND.prefab, BiomeType.Mountains_IslandCaveFloor, 0.33F, 1);
    }
    
    private static void addCommands() {
       // ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("voidsig", VoidSpikesBiome.instance.activateSignal);
        
        //ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<float>>("spawnVKelp", spawnVentKelp);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("triggerVoidFX", f => VoidSpikeLeviathanSystem.instance.doDistantRoar(Player.main, true, f));
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("triggerVoidFlash", VoidSpikeLeviathanSystem.instance.doDebugFlash);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("voidLeviReefback", VoidSpikeLeviathan.makeReefbackTest);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("relockRecipes", relockRecipes);
        
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cSMMAnyW", b => anywhereSeamothModuleCheatActive = b);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cTrackerAll", b => trackerShowAllCheatActive = b);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cSGSA", b => fastSeaglideCheatActive = b);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cFRHS", b => SeamothHeatSinkModule.FREE_CHEAT = b);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<float>>("c2cENVHEAT", b => EnvironmentalDamageSystem.instance.TEMPERATURE_OVERRIDE = b);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cSMTempDebug", b => C2CMoth.temperatureDebugActive = b);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string>>("c2cSignalUnlock", unlockSignal);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string>>("c2cpoi", POITeleportSystem.instance.jumpToPOI);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("c2cRFLdebug", () => SNUtil.writeToChat("Rocket launch error: "+FinalLaunchAdditionalRequirementSystem.instance.hasAllCargo()));
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("c2cRFLForce", FinalLaunchAdditionalRequirementSystem.instance.forceLaunch);
    }
    /*
    private static void spawnVentKelp(float dist) {
		  GameObject obj = ObjectUtil.createWorldObject(C2CItems.kelp.ClassID, true, false);
		  obj.SetActive(false);
		  obj.transform.position = Player.main.transform.position+MainCamera.camera.transform.forward.normalized*dist;
		  LargeWorld.main.streamer.cellManager.RegisterEntity(obj);
		  obj.SetActive(true);
    }*/
    
    private static void relockRecipes() {
    	foreach (TechType tt in C2CRecipes.getRemovedVanillaUnlocks()) {
    		KnownTech.knownTech.Remove(tt);
    	}
    }
    
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
	public static bool checkConditionAndShowPDAAndVoicelogIfNot(bool check, string page, PDAMessages.Messages msg) {
		if (check) {
			return true;
		}
		else {
			if (PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(msg).key)) {
   				if (!string.IsNullOrEmpty(page))
					PDAManager.getPage(page).unlock(false);
			}
			return false;
		}
	}
   
   public static bool playerCanHeal() {
   	Player ep = Player.main;
	if (EnvironmentalDamageSystem.instance.isPlayerRecoveringFromPressure())
		return false;
   	if (ep.IsSwimming() && ep.GetDepth() >= EnvironmentalDamageSystem.depthDamageStart && !LiquidBreathingSystem.instance.hasLiquidBreathing())
   		return false;
   	return true;
   }
	   
	   public static void generateLRNestPlants() {
		   	Vector3 p1 = new Vector3(-786, -762.6F, -321);
		   	Vector3 p2 = new Vector3(-801, -764.9F, -280);
		   	
		   	List<GameObject> plants = new List<GameObject>();
		   	
		   	for (float f = 0; f <= 1; f += 0.05F) {
		   		Vector3 vec = Vector3.Lerp(p1, p2, f);
		   		for (int i = 0; i < 9; i++) {
					Vector3 rot = UnityEngine.Random.rotationUniform.eulerAngles.normalized;
					Ray ray = new Ray(vec, rot);
					if (UWE.Utils.RaycastIntoSharedBuffer(ray, 6, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore) > 0) {
						RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
						if (hit.transform != null) {
							bool flag = true;
							foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(hit.point, 0.2F)) {
								if (pi.ClassId == lrNestGrass.ClassID) {
									flag = false;
									break;
								}
							}
							if (!flag)
								continue;
							GameObject go = ObjectUtil.createWorldObject(lrNestGrass.ClassID);
							go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
							go.transform.position = hit.point;
							plants.Add(go);
						}
					}
		   		}
		   	}
	   	
			string path = BuildingHandler.instance.getDumpFile("lr_nest");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			
			CustomEgg egg = CustomEgg.getEgg(TechType.SpineEel);
			
			foreach (GameObject go in plants) {
				PositionedPrefab pfb = new PositionedPrefab(go.GetComponent<PrefabIdentifier>());
				XmlElement e = doc.CreateElement("customprefab");
				pfb.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
			
			doc.Save(path);
	   }

  }
}
