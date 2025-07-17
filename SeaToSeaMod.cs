using UnityEngine;
  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;
    //For data read/write methods
using System;
    //For data read/write methods
using System.Collections.Generic;
   //Working with Lists and Collections
using System.Reflection;
using System.Xml;
using System.Linq;
   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.Exscansion;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
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
		internal static readonly XMLLocale mouseoverLocale = new XMLLocale(modDLL, "XML/mouseover.xml");
		internal static readonly XMLLocale miscLocale = new XMLLocale(modDLL, "XML/misc.xml");
    
		public static readonly WorldgenDatabase worldgen = new WorldgenDatabase();
    
		private static Dictionary<string, Dictionary<string, Texture2D>> degasiBaseTextures = new Dictionary<string, Dictionary<string, Texture2D>>();
    
		public static readonly TechnologyFragment[] rebreatherChargerFragments = new TechnologyFragment[] {
			new TechnologyFragment("f350b8ae-9ee4-4349-a6de-d031b11c82b1", go => go.transform.localScale = new Vector3(1, 3, 1)),
			new TechnologyFragment("f744e6d9-f719-4653-906b-34ed5dbdb230", go => go.transform.localScale = new Vector3(1, 2, 1)),
			//new TechnologyFragment("589bf5a6-6866-4828-90b2-7266661bb6ed"),
			new TechnologyFragment("3c076458-505e-4683-90c1-34c1f7939a0f", go => go.transform.localScale = new Vector3(1, 1, 0.2F)),
		};
    
		public static readonly TechnologyFragment[] bioprocFragments = new TechnologyFragment[] {
			new TechnologyFragment("85259b00-2672-497e-bec9-b200a1ab012f"),
			//new TechnologyFragment("ba258aad-07e9-4c9b-b517-2ce7400db7b2"),
			//new TechnologyFragment("cf4ca320-bb13-45b6-b4c9-2a079023e787"),
			new TechnologyFragment("f4b3942e-02d8-4526-b384-677a2ad9ce58", go => go.transform.localScale = new Vector3(0.25F, 0.25F, 0.5F)),
			new TechnologyFragment("f744e6d9-f719-4653-906b-34ed5dbdb230"),
		};
    
		public static readonly HashSet<string> lrCoralClusters = new HashSet<string> {
			"a711c0fa-f31e-4426-9164-a9a65557a9a2",
			//"e0e3036d-93fc-4554-8a58-4efed1efbbd7",  not found under brine
			"e1022037-0897-4a64-b460-cda2a309d2f1",
		};
    
		public static TechnologyFragment lathingDroneFragment;
    
		public static MushroomTreeBacterialColony mushroomBioFragment;
		public static GeyserCoral geyserCoral;
		public static GelFountain gelFountain;
		public static GeoGel geogel;
		public static GeoGel geogelDrip;
		public static GeoGelFog geogelFog;
		public static GeoGelFog geogelFogDrip;
		public static PostCoveDome postCoveDome;
    
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
		public static GunPoolBarrier gunPoolBarrier;
		public static LockedPrecursorDoor stepCaveBarrier;
		public static PartialPurpleTablet purpleTabletPartA;
		public static PartialPurpleTablet purpleTabletPartB;
		//public static PartialPurpleTablet floatingIslandTablet;
		public static ExplodingGrabbable brokenAuroraDepthModule;
		public static BKelpBumpWorm bkelpBumpWorm;
		public static AcidSpit acidSpit;
    
		private static BloodKelpBaseNuclearReactorMelter reactorMelter;
		private static TrailerBaseConverter bioBreaker;
		private static TerrainLootSpawner mercuryLootSpawner;
		private static TerrainLootSpawner calciteLootSpawner;
		private static ObjectSpawner stepCaveTunnelSpawner;
		private static ObjectSpawner stepCaveTunnelSpawnerSmall;
		private static StepCaveTunnelAtmo stepCaveTunnelAtmo;
    
		internal static CrashZoneSanctuarySpawner crashSanctuarySpawner;
		internal static SanctuaryGrassSpawner sanctuaryGrassSpawner;
		internal static CrashZoneSanctuaryFern crashSanctuaryFern;
		//internal static CrashZoneSanctuaryGrassBump sanctuaryGrassBump;
		//internal static CrashZoneSanctuaryCoralSheet sanctuaryCoral;
    
		internal static LRNestGrass lrNestGrass;
    
		public static DataChit laserCutterBulkhead;
		public static DataChit bioProcessorBoost;
		public static DataChit seamothDepthUnlockChit;
		public static TechType seamothDepthUnlockTrackerTech;
		public static PDAScanner.EntryData seamothDepthUnlockTracker;
		// public static DataChit vehicleSpeedBoost;
   
		public static PrecursorFabricatorConsole prisonEnzymeConsole;
   
		public static TechType prisonPipeRoomTank;
		public static PDAManager.PDAPage enviroSimulation;
    
		//internal static VoidLeviElecSphere leviPulse;
    
		public static SignalManager.ModSignal treaderSignal;
		public static SignalManager.ModSignal voidSpikeDirectionHint;
		//public static SignalManager.ModSignal duneArchWreckSignal;
		public static SignalManager.ModSignal sanctuaryDirectionHint;
    
		internal static Story.StoryGoal crashMesaRadio;
		//public static Story.StoryGoal duneArchRadio;
		//public static Story.StoryGoal mountainPodRadio;
    
		internal static Story.StoryGoal auroraTerminal;
		//internal static Story.StoryGoal jellyPDATriggeredPDAPrompt;
    
		internal static Story.StoryGoal sunbeamCountdownTrigger;
    
		internal static Harmony harmony;
    
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
			harmony = new Harmony(MOD_KEY);
			Harmony.DEBUG = true;
			FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log.txt");
			try {
				if (File.Exists(FileLog.logPath))
					File.Delete(FileLog.logPath);
			}
			catch (Exception ex) {
				SNUtil.log("Could not clean up harmony log: "+ex);
			}
			FileLog.Log("Ran mod register, started harmony (harmony log)");
			SNUtil.log("Ran mod register, started harmony");
			try {
				InstructionHandlers.runPatchesIn(harmony, typeof(C2CPatches));
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
			mouseoverLocale.load();
			miscLocale.load();
        
			C2CItems.preAdd();
        
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(VoidSpike).TypeHandle);
	    
			// voidspikeLeviRoar = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_roar", "Sounds/voidlevi-roar.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
			//voidspikeLeviFX = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_fx", "Sounds/voidlevi-fx1.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
			// voidspikeLeviAmbient = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_amb", "Sounds/voidlevi-longamb2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
			//voidspikeLeviBite = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_bite", "Sounds/voidlevi-bite.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);

			C2CItems.addCreatures();
	    
			XMLLocale.LocaleEntry e = miscLocale.getEntry("bulkheadLaserCutterUpgrade");
			laserCutterBulkhead = new DataChit(e.key, e.name, e.desc, d => {
				d.controlText = e.pda;
				d.graphic = () => SNUtil.getTechPopupSprite(TechType.LaserCutter);
			});
			//laserCutterBulkhead.showOnScannerRoom = false;
			laserCutterBulkhead.Patch();
			e = miscLocale.getEntry("bioprocessorBoost");
			bioProcessorBoost = new DataChit(e.key, e.name, e.desc, d => {
				d.controlText = e.pda;
			});
			//bioProcessorBoost.showOnScannerRoom = false;
			bioProcessorBoost.Patch();
			e = miscLocale.getEntry("jellyshroomSeamothDepth");
			seamothDepthUnlockChit = new DataChit(e.key, e.name, e.desc, d => {
				d.onUnlock = C2CProgression.onSeamothDepthChit;
			});
			seamothDepthUnlockChit.showOnScannerRoom = true;
			seamothDepthUnlockChit.Patch();
	    
			seamothDepthUnlockTrackerTech = TechTypeHandler.AddTechType(modDLL, "SeamothDepthUnlockTracker", "", "");
			seamothDepthUnlockTracker = new PDAScanner.EntryData();
			seamothDepthUnlockTracker.key = seamothDepthUnlockTrackerTech;
			seamothDepthUnlockTracker.blueprint = TechType.VehicleHullModule1;
			seamothDepthUnlockTracker.destroyAfterScan = false;
			seamothDepthUnlockTracker.locked = true;
			seamothDepthUnlockTracker.totalFragments = 3;
			seamothDepthUnlockTracker.isFragment = true;
			PDAHandler.AddCustomScannerEntry(seamothDepthUnlockTracker);
	    
			e = itemLocale.getEntry("Geogel");
			geogel = new GeoGel(e, false);
			geogel.Patch();
			geogelDrip = new GeoGel(e, true);
			geogelDrip.Patch();
	    
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
			gunPoolBarrier = new GunPoolBarrier();
			gunPoolBarrier.Patch();
			stepCaveBarrier = new LockedPrecursorDoor("StepCaveDoor", PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Purple, new PositionedPrefab("", new Vector3(34.895F, -167F, -649.277F), Quaternion.Euler(0, 287.2F, 0)), new PositionedPrefab("", new Vector3(48.267F, -169.978F, -659.676F), Quaternion.Euler(3.335F, 273.717F, 0.541F)));
			stepCaveBarrier.Patch();
			purpleTabletPartA = new PartialPurpleTablet(true, false);
			purpleTabletPartA.Patch();
			purpleTabletPartB = new PartialPurpleTablet(false, true);
			purpleTabletPartB.Patch();
			brokenAuroraDepthModule = new ExplodingGrabbable("ExplodingAuroraModule", C2CHooks.auroraDepthModule.prefabName);
			brokenAuroraDepthModule.Patch();
			reactorMelter = new BloodKelpBaseNuclearReactorMelter();
			reactorMelter.Patch();
			bioBreaker = new TrailerBaseConverter();
			bioBreaker.Patch();
	    
			bkelpBumpWorm = new BKelpBumpWorm(itemLocale.getEntry("BKelpBumpWorm"));
			bkelpBumpWorm.Patch();
			acidSpit = new AcidSpit();
			acidSpit.Patch();
	    
			mushroomBioFragment = new MushroomTreeBacterialColony(itemLocale.getEntry("TREE_BACTERIA"));
			mushroomBioFragment.register();
	    
			mercuryLootSpawner = new TerrainLootSpawner("MercuryLootSpawner", VanillaResources.MERCURY.prefab);
			mercuryLootSpawner.Patch();
			calciteLootSpawner = new TerrainLootSpawner("CalciteLootSpawner", CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).ClassID);
			calciteLootSpawner.Patch();
	    
			WeightedRandom<PrefabReference> stepCavePlants = new WeightedRandom<PrefabReference>();	    
			stepCavePlants.addEntry(VanillaFlora.VIOLET_BEAU, 40);
			stepCavePlants.addEntry(VanillaFlora.PAPYRUS, 30);  
			stepCavePlants.addEntry(VanillaFlora.REDWORT, 30);  
			stepCavePlants.addEntry(new ModPrefabContainer(C2CItems.healFlower), 20);
			stepCaveTunnelSpawner = new ObjectSpawner("StepCaveTunnelPlantSpawner", new ObjectSpawner.SpawnSet(stepCavePlants));
			stepCaveTunnelSpawner.Patch();
	    
			stepCavePlants = new WeightedRandom<PrefabReference>();
			stepCavePlants.addEntry(VanillaFlora.ACID_MUSHROOM, 40);
			//often placed wrong stepCavePlants.addEntry(VanillaFlora.WRITHING_WEED.getPrefabID(), 20);
			stepCaveTunnelSpawnerSmall = new ObjectSpawner("StepCaveTunnelPlantSpawnerSmall", new ObjectSpawner.SpawnSet(stepCavePlants, (go, tt) => {
				go.transform.rotation = tt.rotation;
				go.transform.Rotate(go.transform.right, -90);
			}));
			stepCaveTunnelSpawnerSmall.Patch();
	    
			stepCaveTunnelAtmo = new StepCaveTunnelAtmo();
			stepCaveTunnelAtmo.Patch();
	    
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
	    
			//PrecursorFabricatorConsole.CraftingIdentifier ci = new PrecursorFabricatorConsole.RecipeID(TechType.HatchingEnzymes, C2CRecipes.getHatchingEnzymeRecipe(), "HatchEnzymes");
			prisonEnzymeConsole = new PrecursorFabricatorConsole(C2CRecipes.getHatchingEnzymeFab(), "PrecursorEnzymes", new Color(0.8F, 0.8F, 0.8F)).addStoryGate("PrecursorPrisonAquariumIncubatorActive", mouseoverLocale.getEntry("EnzymesNotKnown").desc);
			prisonEnzymeConsole.Patch();
	    
			CustomLocaleKeyDatabase.registerKeys(mouseoverLocale);
	   
			//leviPulse = new VoidLeviElecSphere();
			//leviPulse.Patch();
        
			BasicCraftingItem drone = CraftingItems.getItem(CraftingItems.Items.LathingDrone);
			lathingDroneFragment = TechnologyFragment.createFragment("6e0f4652-c439-4540-95be-e61384e27692", drone.TechType, drone.FriendlyName, 3, 2, true, go => {
				ObjectUtil.removeComponent<Pickupable>(go);
				ObjectUtil.removeComponent<Rigidbody>(go);
				go.EnsureComponent<LathingDroneSparker>();
			}); //it has its own model
        
			C2CItems.addMachines();
	    
			geyserCoral = new GeyserCoral(itemLocale.getEntry("GEYSER_CORAL"));
			geyserCoral.register();
	    
			gelFountain = new GelFountain(itemLocale.getEntry("GEL_FOUNTAIN"));
			gelFountain.register();
	    
			geogelFog = new GeoGelFog(false);
			geogelFog.Patch();
			geogelFogDrip = new GeoGelFog(true);
			geogelFogDrip.Patch();
	    
			postCoveDome = new PostCoveDome(itemLocale.getEntry("POST_COVE_DOME"));
			postCoveDome.Patch();
        
			addPDAEntries();
			/*
	    e = SeaToSeaMod.pdaLocale.getEntry("envirosim");
	    prisonPipeRoomTank = TechTypeHandler.AddTechType(modDLL, e.key, e.getField<string>("scanprompt"), e.desc);
	    //prisonPipeRoomTank = new TechType[4];
	    //for (int i = 0; i < prisonPipeRoomTank.Length; i++)
	    //	prisonPipeRoomTank[i] = TechTypeHandler.AddTechType(modDLL, e.key+"_"+i, e.name, e.desc);
	    //SNUtil.addPDAEntry(prisonPipeRoomTank, e.key, e.name, 1, e.getField<string>("category"), e.pda, e.getField<string>("header"));
	    enviroSimulation = PDAManager.getPage(e.key);
	    
		PDAScanner.EntryData se = new PDAScanner.EntryData();
		se.key = prisonPipeRoomTank;
		se.blueprint = TechType.None;
		se.destroyAfterScan = false;
		se.locked = true;
		se.totalFragments = 4;
		se.isFragment = true;
		se.scanTime = 1;
		se.encyclopedia = enviroSimulation.id;
		PDAHandler.AddCustomScannerEntry(se);
        */
			addCommands();
			addOreGen();
        
			GenUtil.registerWorldgen(new PositionedPrefab(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).ClassID, Azurite.mountainBaseAzurite, Quaternion.Euler(0, 202.4F, 33.2F)));
			//GenUtil.registerWorldgen(new PositionedPrefab("e85adb0d-665e-48f5-9fa2-2dd316776864", C2CHooks.bkelpBaseGeoCenter), go => go.transform.localScale = Vector3.one*60);
        
			addSignalsAndRadio();
		
			PDAMessages.addAll();
		
			auroraTerminal = new Story.StoryGoal("auroraringterminal_c2c", Story.GoalType.PDA, 0);
			e = miscLocale.getEntry(auroraTerminal.key);
			SNUtil.addVOLine(auroraTerminal, e.desc, SoundManager.registerPDASound(modDLL, e.key, e.pda).asset);
			//StoryHandler.instance.addListener(s => {if (s == auroraTerminal.key) {}});
		
			sunbeamCountdownTrigger = new Story.StoryGoal("c2cTriggerSunbeamCountdown", Story.GoalType.Story, 0);
		
			KnownTech.onAdd += onTechUnlocked;
       
			//DamageSystem.acidImmune = DamageSystem.acidImmune.AddToArray<TechType>(TechType.Seamoth);
		
			C2CItems.postAdd();
       
			VoidSpikesBiome.instance.register();
			UnderwaterIslandsFloorBiome.instance.register();
			CrashZoneSanctuaryBiome.instance.register();
			VoidSpike.register();
			AvoliteSpawner.instance.register();
			BiomeDiscoverySystem.instance.register();
			LifeformScanningSystem.instance.register();
		
			C2CItems.alkali.addNativeBiome(VanillaBiomes.MOUNTAINS, true).addNativeBiome(VanillaBiomes.TREADER, true).addNativeBiome(VanillaBiomes.KOOSH, true);
			C2CItems.kelp.addNativeBiome(UnderwaterIslandsFloorBiome.instance);
			C2CItems.healFlower.addNativeBiome(VanillaBiomes.REDGRASS, true);
			C2CItems.sanctuaryPlant.addNativeBiome(CrashZoneSanctuaryBiome.instance);
			C2CItems.mountainGlow.addNativeBiome(VanillaBiomes.MOUNTAINS);
			//C2CItems.underislandsCavePlant.addNativeBiome(VanillaBiomes.UNDERISLANDS);
		
			C2CItems.deepStalker.addNativeBiome(VanillaBiomes.GRANDREEF);
			C2CItems.purpleBoomerang.addNativeBiome(UnderwaterIslandsFloorBiome.instance);
			C2CItems.purpleHoopfish.addNativeBiome(UnderwaterIslandsFloorBiome.instance);
			C2CItems.purpleHolefish.addNativeBiome(UnderwaterIslandsFloorBiome.instance);
			C2CItems.broodmother.addNativeBiome(VanillaBiomes.BLOODKELPNORTH);
		
			initHandlers();
        
			Vector3 ang = new Vector3(0, 317, 0);
			Vector3 pos1 = new Vector3(-1226, -350, -1258);
			Vector3 pos2 = new Vector3(-1327, -350, -1105);
			Vector3 tgt = pos2 + (pos2 - pos1).setLength(40);
			for (int i = 0; i <= 4; i++) {
				Vector3 pos = Vector3.Lerp(pos1, pos2, i / 4F);
				GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.SEA_TREADER.prefab, pos, Quaternion.Euler(ang)), go => {
					go.GetComponent<TreaderMoveOnSurface>().timeNextTarget = Time.time + 120;
					go.GetComponent<SeaTreader>().MoveTo(tgt);
				});
			}
        
			SNUtil.addMultiScanUnlock(TechType.PowerTransmitter, 2, TechType.PowerTransmitter, 1, false);
			SNUtil.addMultiScanUnlock(TechType.LEDLight, 2, TechType.LEDLight, 1, false);
			SNUtil.addMultiScanUnlock(TechType.ThermalPlant, 4, TechType.ThermalPlant, 1, false);
			SNUtil.addMultiScanUnlock(TechType.NuclearReactor, 7, TechType.NuclearReactor, 1, false);
			
			SpriteHandler.RegisterSprite(C2CItems.brineCoral, TextureManager.getSprite(modDLL, "Textures/BrineCoralIcon"));
			
			C2CIntegration.prePostAdd();
		}
    
		private static void initHandlers() {
			Harmony h = harmony;
			SaveSystem.addPlayerSaveCallback(typeof(LiquidBreathingSystem).GetField("kharaaTreatmentRemainingTime", BindingFlags.Instance | BindingFlags.NonPublic), () => LiquidBreathingSystem.instance);
        	SaveSystem.addPlayerSaveCallback(typeof(EnvironmentalDamageSystem).GetField("recoveryWarningEndTime", BindingFlags.Instance | BindingFlags.NonPublic), () => EnvironmentalDamageSystem.instance);
        
			POITeleportSystem.instance.populate();
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(C2CUnlocks).TypeHandle);
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(C2CProgression).TypeHandle);			
			DataCollectionTracker.instance.register();
		}
    
		[QModPostPatch]
		public static void PostLoad() {
			new LavaCastleVentCrystalPlacer().Patch();
			worldgen.load(s => s != "fcswreck" || FCSIntegrationSystem.instance.isLoaded()); //load in post because some cross-mod TTs may not exist yet
			mushroomBioFragment.postRegister();
			geyserCoral.postRegister();
			gelFountain.postRegister();
			postCoveDome.postRegister();
		
			int n = C2CHooks.purpleTabletsToBreak.Count + 1; //+1 for the broken one in front of gun
			n += SeaToSeaMod.worldgen.getCount("83b61f89-1456-4ff5-815a-ecdc9b6cc9e4");
			n += SeaToSeaMod.worldgen.getCount("PartialPurpleTablet_A");
			n += SeaToSeaMod.worldgen.getCount("PartialPurpleTablet_B");
			PDAHandler.EditFragmentsToScan(TechType.PrecursorKey_PurpleFragment, n);//hard ? n : n-1; //allow missing one in not-hard
        
			foreach (Vector3 pos in C2CProgression.instance.bkelpNestBumps) {
				GenUtil.registerWorldgen(new BKelpBumpWormSpawner(pos + Vector3.down * 3));
			}
		
			AvoliteSpawner.instance.postRegister();
			DataboxTypingMap.instance.load();
			DataboxTypingMap.instance.addValue(-789.81, -216.10, -711.02, C2CItems.bandage.TechType);
			DataboxTypingMap.instance.addValue(-483.55, -504.69, 1326.64, C2CItems.tetherModule.TechType);
        
			ESHooks.addLeviathan(C2CItems.voidSpikeLevi.TechType);
			ESHooks.scannabilityEvent += C2CHooks.isItemMapRoomDetectable;
        
			foreach (BiomeType bb in Enum.GetValues(typeof(BiomeType))) {
				LootDistributionHandler.EditLootDistributionData(VanillaResources.SULFUR.prefab, bb, 0, 1);
				LootDistributionHandler.EditLootDistributionData(CustomEgg.getEgg(TechType.SpineEel).ClassID, bb, 0, 1);
				if (bb != BiomeType.BonesField_Lake_Floor && bb != BiomeType.BonesField_LakePit_Floor && bb != BiomeType.BonesField_LakePit_Wall && bb != BiomeType.BonesField_Cave_Ground) {
					foreach (string s in lrCoralClusters)
						LootDistributionHandler.EditLootDistributionData(s, bb, 0, 1);
				}
			}
			
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ExplorationTrackerPages).TypeHandle);
        
			C2CIntegration.addPostCompat();
    	
			dumpAbandonedBaseTextures();
    	
			Dictionary<string, bool> modsWithIssues = new Dictionary<string, bool>() {
				{ "CyclopsNuclearUpgrades", true },
				{ "CyclopsBioReactor", false },
				//{"AquariumBreeding", false},
				{ "RedBaron", true },
				//{"SeamothArms", true},
				{ "HabitatControlPanel", true },
				{ "MoreSeamothDepth", true },
				{ "CustomCraft2", true },
				//{"FCSAlterraHub", false},
				{ "SlotExtender", false },
				{ "WarpChip", false },
				//{"Socknautica", false},
				{ "Socksfor1Monsters", false },
				{ "DADTankSubPack", true },
				{ "DWEquipmentBonanza", false },
				{ "SeaVoyager", false },
				{ "SubnauticaRandomiser", true },
				{ "EquivalentExchange", true },
				{ "Deathrun", false },
				{ "DecorationsMod", true },
				{ "AnthCreatures", true },
				{ "SpyWatch", true },
				{ "SeamothEnergyShield", true },
				{ "SeamothThermal", false },
				{ "ArmorSuit", false },
				{ "ShieldSuit", false },
				{ "TimeControlSuit", true },
				{ "CameraDroneStasisUpgrade", true },
				//{"CameraDroneFlightUpgrade", false},
				{ "CustomizeYourSpawns", true },
				{ "StasisModule", true },
				{ "StasisTorpedo", true },
				{ "CyclopsLaserCannonModule", false },
				{ "DebrisRecycling", true },
				{ "AD3D_DeepEngineMod", false },
				{ "DeepEngineMod", false },
				{ "AD3D_TechFabricatorMod", false },
				{ "PassiveReapers", true },
				{ "PlasmaCannonArm", false }, //add scanner module?
				{ "AcceleratedStart", true },
				{ "CyclopsNuclearReactor", true },
				{ "LaserCannon", true },
				{ "PartsFromScanning", true },
				{ "StealthModule", true },
				{ "RPG_Framework", true },
				{ "CustomBatteries", true },
				{ "DropUpgradesOnDestroy", false },
				{ "All_Items_1x1", false },
				{ "Radiant Depths", true }, //TODO id might be wrong, also might be 2.0
				{ "SubnauticaAutosave", true },
				{ "SeaToSeaWorldGenFixer", true },
				{ "FCSIntegrationRemover", true },
				{ "aaaaaaaaaa", true },
			};
			foreach (QModManager.API.IQMod mod in QModManager.API.QModServices.Main.GetAllMods()) {
				SNUtil.log("Checking compat with 'mod " + mod.Id + "' (\"" + mod.DisplayName + "\")");
				if (modsWithIssues.ContainsKey(mod.Id)) {
					if (modsWithIssues[mod.Id]) {
						string msg = "Mod '" + mod.DisplayName + "' detected. This mod is not compatible with SeaToSea, and cannot be used alongside it.";
						SNUtil.createPopupWarning(msg, false/*, null, SNUtil.createPopupButton("OK")*/);
						throw new Exception(msg);
					}
					else {
						string msg = "SeaToSea: Mod '" + mod.DisplayName + "' detected. This mod will significantly alter the balance of your pack and risks completely breaking C2C progression.";
						SNUtil.createPopupWarning(msg, false/*, null, SNUtil.createPopupButton("OK")*/);
						SNUtil.log(msg + " You should remove this mod if possible when using SeaToSea.");
					}
				}
			}
			if (!QModManager.API.QModServices.Main.ModPresent("TerrainPatcher")) {
				string msg = "TerrainPatcher is a required dependency for SeaToSea!";
				SNUtil.createPopupWarning(msg, false/*, SNUtil.createPopupButton("Download", () => {
				System.Diagnostics.Process.Start("https://github.com/Esper89/Subnautica-TerrainPatcher/releases/download/v0.4/TerrainPatcher-v0.4.zip");
				Application.Quit(64);
			}), SNUtil.createPopupButton("Ignore")*/
				);
				throw new Exception(msg);
			}
			if (!QModManager.API.QModServices.Main.ModPresent("AgonyRadialCraftingTabs")) {
				string msg = "RadialTabs is recommended when using SeaToSea to ensure that all crafting nodes in fabricator UIs remain onscreen.";
				SNUtil.createPopupWarning(msg, true/*, SNUtil.createPopupButton("Download", () => {
				System.Diagnostics.Process.Start("https://www.nexusmods.com/Core/Libs/Common/Widgets/ModRequirementsPopUp?id=2624&game_id=1155");
				Application.Quit(64);
			}), SNUtil.createPopupButton("Ignore")*/
				);
				SNUtil.log(msg + " You should add this mod if at all possible.");
			}
			string fn = "generated.optoctreepatch";
			if (File.Exists(Path.Combine(Path.GetDirectoryName(modDLL.Location), fn))) {
				string msg = "Delete "+fn+" from your install directory. This is an old file from previous versions and will conflict with new terrain patches.";
				SNUtil.createPopupWarning(msg, false);
				throw new Exception(msg);
			}
		}
    
		private static void dumpAbandonedBaseTextures() {
			string[] prefabs = new string[] {
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
									SNUtil.log("Exporting degasi base textures from " + r.gameObject.GetFullHierarchyPath() + ": " + n, modDLL);
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
			if (FCSIntegrationSystem.instance.isLoaded()) {
				vent.registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 0.2F);
				vent.registerWorldgen(BiomeType.DeepGrandReef_ThermalVent, 1, 0.4F);
			}
    	
			BasicCustomOre irid = CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM);
			irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Ceiling, 1, 1.2F);
			irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor, 1, 0.3F);
			irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor_Far, 1, 0.67F);
			irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Wall, 1, 0.6F);
			irid.registerWorldgen(BiomeType.InactiveLavaZone_Chamber_Ceiling, 1, 0.5F);
			
			LootDistributionHandler.EditLootDistributionData(PostCoveDomeGenerator.hotResourceDome.ClassID, BiomeType.InactiveLavaZone_Corridor_Ceiling, 0.5F, 1);
    	
			BasicCustomOre calcite = CustomMaterials.getItem(CustomMaterials.Materials.CALCITE);
			calcite.registerWorldgen(BiomeType.BonesField_Cave_Ceiling, 1, 1.2F);
    	
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
    	
			LootDistributionHandler.EditLootDistributionData(VanillaResources.QUARTZ.prefab, BiomeType.UnderwaterIslands_IslandCaveFloor, 0.5F, 1);
			LootDistributionHandler.EditLootDistributionData(VanillaResources.QUARTZ.prefab, BiomeType.UnderwaterIslands_IslandCaveWall, 0.3F, 1);
			LootDistributionHandler.EditLootDistributionData(VanillaResources.MAGNETITE.prefab, BiomeType.UnderwaterIslands_IslandCaveFloor, 0.5F, 1);
			LootDistributionHandler.EditLootDistributionData(VanillaResources.MAGNETITE.prefab, BiomeType.UnderwaterIslands_IslandCaveWall, 0.3F, 1);
    	
			LootDistributionHandler.EditLootDistributionData(C2CItems.purpleHolefish.ClassID, BiomeType.UnderwaterIslands_OpenDeep_CreatureOnly, 2.75F, 1);
    	
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
    	
			foreach (KeyValuePair<Vector3, Tuple<float, int, float>> kvp in C2CUtil.mercurySpawners) {
				Tuple<float, int, float> vals = kvp.Value; //exclusion radius, target count, max range
				int count = vals.Item2;
				if (config.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
					count = Math.Max(1, count * 2 / 3);
				GenUtil.registerWorldgen(new PositionedPrefab(mercuryLootSpawner.ClassID, kvp.Key, Quaternion.identity, new Vector3(vals.Item1, count, vals.Item3)));
			}
    	
			foreach (KeyValuePair<Vector3, Tuple<float, int, float>> kvp in C2CUtil.calciteSpawners) {
				GenUtil.registerWorldgen(new PositionedPrefab(calciteLootSpawner.ClassID, kvp.Key, Quaternion.identity, new Vector3(kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3)));
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
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("relockRecipes", C2CUtil.relockRecipes);
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("dumpGameStats", () => GameStatistics.collect().writeToFile(Path.Combine(Path.GetDirectoryName(modDLL.Location), "statdump.xml")));
        
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cSMMAnyW", b => anywhereSeamothModuleCheatActive = b && SNUtil.canUseDebug());
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cTrackerAll", b => trackerShowAllCheatActive = b && SNUtil.canUseDebug());
			//ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("c2cTrackerSetAll", ExplorationTrackerPages.instance.markAllDiscovered);
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cSGSA", b => fastSeaglideCheatActive = b && SNUtil.canUseDebug());
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cFRHS", b => SeamothHeatSinkModule.FREE_CHEAT = b && SNUtil.canUseDebug());
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<float>>("c2cENVHEAT", b => {
				if (SNUtil.canUseDebug())
					EnvironmentalDamageSystem.instance.TEMPERATURE_OVERRIDE = b;
			});
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("c2cSMTempDebug", b => C2CMoth.temperatureDebugActive = b);
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string>>("c2cSignalUnlock", arg => {
				if (SNUtil.canUseDebug())
					unlockSignal(arg);
			});
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string>>("c2cpoi", POITeleportSystem.instance.jumpToPOI);
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("c2cRFLdebug", () => SNUtil.writeToChat("Rocket launch error: " + FinalLaunchAdditionalRequirementSystem.instance.hasAllCargo() + "; Missing scan=" + LifeformScanningSystem.instance.hasScannedEverything()));
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("c2cRFLForce", FinalLaunchAdditionalRequirementSystem.instance.forceLaunch);
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("c2cRecover", () => C2CUtil.rescue());
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
			switch (name) {
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
					page.setHeaderImage(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/PDA/" + e.getField<string>("header")));
				page.register();
			}
		}

	}
}
