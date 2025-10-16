using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.Auroresource;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.Exscansion;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public static class C2CHooks {

		internal static readonly Vector3 deepDegasiTablet = new Vector3(-638.9F, -506.0F, -941.3F);

		internal static readonly List<Vector3> purpleTabletsToBreak = new List<Vector3>() { //do not include one inside gun by control room
			new Vector3(291.19F, 30.94F, 848.86F), //south island shelf
			new Vector3(363.22F, 54.11F, 1015.80F), //internal island bridge
			new Vector3(383.18F, 18.21F, 1086.94F), //above gun entrance
			new Vector3(389.05F, -120.38F, 1150.36F), //this one is in that separate small cave loop
			new Vector3(320.14F, -91.85F, 1023.56F), //underwater near survivor cache
			new Vector3(-753.54F, 13.72F, -1107.50F), //floating island
		};

		//internal static readonly Dictionary<Vector3, bool[]> purpleTabletsToRemoveParts = new Dictionary<Vector3, bool[]>();

		internal static readonly Vector3 crashMesa = new Vector3(623.8F, -250.0F, -1105.2F);
		internal static readonly Vector3 mountainBaseGeoCenter = new Vector3(953, -344, 1453);
		internal static readonly Vector3 bkelpBaseGeoCenter = new Vector3(-1311.6F, -670.6F, -412.7F);
		internal static readonly Vector3 bkelpBaseNuclearReactor = new Vector3(-1325.67F, -660.60F, -392.70F);
		internal static readonly Vector3 trailerBaseBioreactor = new Vector3(1314.94F, -80.2F, -412.97F);
		internal static readonly Vector3 lrpowerSealSetpieceCenter = new Vector3(-713.45F, -766.37F, -262.74F);
		internal static readonly Vector3 auroraFront = new Vector3(1202.43F, -40.16F, 151.54F);
		internal static readonly Vector3 auroraRepulsionGunTerminal = new Vector3(1029.51F, -8.7F, 35.87F);
		internal static readonly Vector3 lostRiverCachePanel = new Vector3(-1119.5F, -684.4F, -709.7F);
		internal static readonly Vector3 voidWreckVoidPatch = new Vector3(-293.58F, -422.65F, -1753.40F);
		//internal static readonly Vector3 gunPoolBarrier = new Vector3(481.81F, -125.03F, 1257.85F);
		//internal static readonly Vector3 gunPoolBarrierTerminal = new Vector3();
		internal static readonly Vector3 gunCenter = new Vector3(460.6F, -99, 1208.4F);
		internal static readonly Vector3 mountainCenter = new Vector3(359.9F, 29F, 985.9F);

		internal static readonly Vector3 fcsWreckOpenableDoor = new Vector3(88.87F, -420.75F, 1449.10F);
		internal static readonly Vector3 fcsWreckBlockedDoor = new Vector3(93.01F, -421.27F, 1444.71F);

		internal static readonly PositionedPrefab auroraStorageModule = new PositionedPrefab("d290b5da-7370-4fb8-81bc-656c6bde78f8", new Vector3(991.5F, 3.21F, -30.99F), Quaternion.Euler(14.44F, 353.7F, 341.6F));
		internal static readonly PositionedPrefab auroraCyclopsModule = new PositionedPrefab("049d2afa-ae76-4eef-855d-3466828654c4", new Vector3(872.5F, 2.69F, -0.66F), Quaternion.Euler(357.4F, 224.9F, 21.38F));
		internal static readonly PositionedPrefab auroraDepthModule = new PositionedPrefab("74ec328c-e627-40ad-b373-97e384ec0385", new Vector3(903.52F, -0.16F, 16.06F), Quaternion.Euler(10.34F, 341.24F, 331.96F));

		private static readonly HashSet<TechType> scanToScannerRoom = new HashSet<TechType>();
		private static readonly HashSet<string> floaterRocks = new HashSet<string>() {
			"44396d05-0910-4b4d-a046-119fab3512a5",
			"7637d968-4878-46a5-adf5-aa9e21fe3ddc",
			"9a9cdb4e-f110-412d-b16b-b9ace904b569",
			"a7b35deb-1ac7-4fb8-8393-c0252cbf6d23",
			"d4ad48a9-67fa-4b34-8447-5cd6a69d1270",
			"e3d778b5-a81e-4b64-8dd6-910fb22772db",
			"f895696c-cdc6-4427-a87f-2b62666ea0cb"
		};

		private static readonly HashSet<string> auroraFires = new HashSet<string>() {
			"14bbf7f0-4276-48bf-868b-317b366edd16",
			"3877d31d-37a5-4c94-8eef-881a500c58bc",
			"afe53ea1-d2a8-4f76-8ffb-d41ff6046b52"
		};

		private static readonly Dictionary<string, Color> auroraPrawnFireColors = new Dictionary<string, Color> {
			{ "xFireFlame", new Color(0, 0.67F, 2) },
			{ "xFireCurl", new Color(1, 1, 1) },
			{ "xAmbiant_Sparks", new Color(0, 1, 1) },
			{ "xAmbiant_Ashes", new Color(0.1F, 0.1F, 1) },
			{ "x_Fire_CrossPlanes", new Color(0.67F, 0.43F, 1) },
			{ "x_Fire_GroundPlane", new Color(0.24F, 0.57F, 1) },
			{ "x_SmokeLight_Cylindrical", new Color(0.67F, 0.72F, 0.97F) },
			{ "x_Fire_Cylindrical", new Color(0.24F, 0.51F, 1) },
		};

		private static Oxygen playerBaseO2;

		private static float nextSanctuaryPromptCheckTime = -1;
		private static float nextBkelpBaseAmbCheckTime = -1;
		private static float nextBkelpBaseAmbTime = -1;
		private static float nextCameraEMPTime = -1;

		private static float foodToRestore;
		private static float waterToRestore;

		public static bool skipPlayerTick = false;
		public static bool skipBiomeCheck = false;
		public static bool skipTemperatureCheck = false;
		public static bool skipSkyApplierSpawn = false;
		public static bool skipRadiationLevel = false;
		public static bool skipFruitPlantTick = false;
		public static bool skipScannerTick = false;
		public static bool skipCompassCalc = false;
		public static bool skipPodTick = false;
		public static bool skipSeamothTick = false;
		public static bool skipCrawlerTick = false;
		public static bool skipTreaderTick = false;
		public static bool skipVoidLeviTick = false;
		public static bool skipMagnetic = false;
		public static bool skipWaveBob = false;
		public static bool skipRaytrace = false;
		public static bool skipReach = false;
		public static bool skipResourceSpawn = false;
		public static bool skipEnviroDamage = false;
		public static bool skipO2 = false;
		public static bool skipStalkerShiny = false;
		public static bool skipRocketTick = false;

		private static TechType techPistol = TechType.None;
		private static bool searchedTechPistol = false;

		private static float lastO2PipeTime = -1;

		private static bool playerDied;

		private static float lastSaveAlertTime = -1;

		private static float lastCuddlefishPlay = -1;

		private static TechType loadTechPistol() {
			if (techPistol == TechType.None && !searchedTechPistol) {
				techPistol = SNUtil.getTechType("TechPistol");
				if (DIHooks.isWorldLoaded())
					searchedTechPistol = true;
			}
			return techPistol;
		}

		static C2CHooks() {
			SNUtil.log("Initializing C2CHooks");
			DIHooks.onWorldLoadedEvent += onWorldLoaded;
			DIHooks.onDamageEvent += recalculateDamage;
			DIHooks.onItemPickedUpEvent += onItemPickedUp;
			DIHooks.onSkyApplierSpawnEvent += onSkyApplierSpawn;

			DIHooks.getBiomeEvent += getBiomeAt;
			DIHooks.getTemperatureEvent += getWaterTemperature;

			DIHooks.onPlayerTickEvent += tickPlayer;
			DIHooks.getPlayerInputEvent += controlPlayerInput;

			DIHooks.onSeamothModulesChangedEvent += updateSeamothModules;
			DIHooks.onCyclopsModulesChangedEvent += updateCyclopsModules;
			DIHooks.onPrawnModulesChangedEvent += updatePrawnModules;
			DIHooks.onSeamothModuleUsedEvent += useSeamothModule;

			DIHooks.seamothDischargeEvent += pulseSeamothDefence;
			DIHooks.onSeamothSonarUsedEvent += pingSeamothSonar;
			DIHooks.onTorpedoFireEvent += onTorpedoFired;
			DIHooks.onTorpedoExplodeEvent += onTorpedoExploded;

			DIHooks.onSonarUsedEvent += pingAnySonar;

			DIHooks.onEMPHitEvent += onEMPHit;

			DIHooks.constructabilityEvent += applyGeyserFilterBuildability;
			DIHooks.breathabilityEvent += canPlayerBreathe;

			DIHooks.getSwimSpeedEvent += getSwimSpeed;

			DIHooks.spawnTreaderChunk += onTreaderChunkSpawn;

			DIHooks.crashfishExplodeEvent += onCrashfishExplode;

			//DIHooks.fogCalculateEvent += interceptChosenFog;

			DIHooks.radiationCheckEvent += (ch) => {
				if (!skipRadiationLevel)
					ch.value = getRadiationLevel(ch);
			};

			DIHooks.itemTooltipEvent += generateItemTooltips;
			DIHooks.bulkheadLaserHoverEvent += interceptBulkheadLaserCutter;

			DIHooks.knifeAttemptEvent += tryKnife;
			DIHooks.onKnifedEvent += onKnifed;
			DIHooks.knifeHarvestEvent += interceptItemHarvest;

			DIHooks.onFruitPlantTickEvent += tickFruitPlant;

			DIHooks.reaperGrabVehicleEvent += onReaperGrab;
			DIHooks.cyclopsDamageEvent += onCyclopsDamage;

			DIHooks.vehicleEnterEvent += onVehicleEnter;

			DIHooks.scannerRoomTickEvent += AvoliteSpawner.instance.tickMapRoom;

			DIHooks.solarEfficiencyEvent += (ch) => ch.value = getSolarEfficiencyLevel(ch);
			DIHooks.depthCompassEvent += getCompassDepthLevel;
			DIHooks.propulsibilityEvent += modifyPropulsibility;
			//DIHooks.droppabilityEvent += modifyDroppability;	    	
			DIHooks.moduleFireCostEvent += (ch) => ch.value = getModuleFireCost(ch);

			DIHooks.equipmentTypeCheckEvent += changeEquipmentCompatibility;

			DIHooks.onStasisRifleFreezeEvent += (ch) => ch.applyKinematicChange = !onStasisFreeze(ch.sphere, ch.body);
			DIHooks.onStasisRifleUnfreezeEvent += (ch) => ch.applyKinematicChange = !onStasisUnFreeze(ch.sphere, ch.body);

			DIHooks.respawnEvent += onPlayerRespawned;
			DIHooks.itemsLostEvent += onItemsLost;
			DIHooks.selfScanEvent += onSelfScan;
			DIHooks.scanCompleteEvent += onScanComplete;

			DIHooks.tryEatEvent += tryEat;

			DIHooks.waterFilterSpawnEvent += onWaterFilterSpawn;

			DIHooks.onPlayWithCuddlefish += onCuddlefishPlay;
			DIHooks.onRocketStageCompletedEvent += onRocketStageComplete;
			DIHooks.onSleepEvent += onSleep;
			DIHooks.onEatEvent += onEat;
			DIHooks.getFoodRateEvent += affectFoodRate;

			DIHooks.targetabilityEvent += checkTargetingSkip;

			SNUtil.log("Finished registering main DI event callbacks");

			KnownTech.onAdd += onTechUnlocked;

			BaseSonarPinger.onBaseSonarPingedEvent += onBaseSonarPinged;
			BaseDrillableGrinder.onDrillableGrindEvent += getGrinderDrillableDrop;

			LavaBombTag.onLavaBombImpactEvent += onLavaBombHit;
			ExplodingAnchorPod.onExplodingAnchorPodDamageEvent += onAnchorPodExplode;
			PredatoryBloodvine.onBloodKelpGrabEvent += onBloodKelpGrab;
			VoidTongueTag.onVoidTongueGrabEvent += onVoidTongueGrab;
			VoidTongueTag.onVoidTongueReleaseEvent += onVoidTongueRelease;
			PlanktonCloudTag.onPlanktonActivationEvent += onPlanktonActivated;
			VoidBubble.voidBubbleSpawnerTickEvent += tickVoidBubbles;
			VoidBubble.voidBubbleTickEvent += tickVoidBubble;
			MushroomVaseStrand.vaseStrandFilterCollectEvent += onCollectFromVaseStrand;

			FallingMaterialSystem.impactEvent += onMeteorImpact;

			SNUtil.log("Finished registering event callbacks");

			scanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType);
			scanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType);
			scanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType);
			scanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.OBSIDIAN).TechType);
			scanToScannerRoom.Add(C2CItems.voidSpikeLevi.TechType);
			scanToScannerRoom.Add(C2CItems.alkali.TechType);
			scanToScannerRoom.Add(C2CItems.healFlower.TechType);
			scanToScannerRoom.Add(C2CItems.kelp.TechType);
			scanToScannerRoom.Add(C2CItems.broodmother.TechType);
		}

		//[System.Runtime.InteropServices.DllImport("WorldgenCheck.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
		//public static extern bool handleWorldgenIntegrity(string s);

		public static void onWorldLoaded() {
			if (WorldgenIntegrityChecks.checkWorldgenIntegrity(false))
				WorldgenIntegrityChecks.throwError();

			MoraleSystem.instance.reset();

			Inventory.main.equipment.onEquip += onEquipmentAdded;
			Inventory.main.equipment.onUnequip += onEquipmentRemoved;

			//remove all since field does not serialize
			Inventory.main.container.forEachOfType(C2CItems.emperorRootOil.TechType, ii => Inventory.main.container.forceRemoveItem(ii));

			BrokenTablet.updateLocale();

			PDAScanner.mapping[TechType.SeaEmperorJuvenile] = PDAScanner.mapping[TechType.SeaEmperorBaby]; //make juveniles scannable into baby page, so not missable

			PDAManager.getPage("rescuewarp").unlock(false);

			Player.main.playerRespawnEvent.AddHandler(Player.main, new UWE.Event<Player>.HandleFunction(ep => {
				if (!ep.lastValidSub && !ep.lastEscapePod && !EscapePod.main) {
					ep.SetPosition(new Vector3(0, -5, 0));
					ep.SetMotorMode(Player.MotorMode.Dive);
				}
			}));

			if (FCSIntegrationSystem.instance.isLoaded()) {
				FCSIntegrationSystem.instance.initializeTechUnlocks();
				BaseBioReactor.charge[FCSIntegrationSystem.instance.getBiofuel()] = 18000;
			}

			VoidSpikesBiome.instance.onWorldStart();
			UnderwaterIslandsFloorBiome.instance.onWorldStart();
			
			C2CProgression.instance.onWorldLoaded();

			moveToExploitable("SeaCrown");
			moveToExploitable("SpottedLeavesPlant");
			moveToExploitable("OrangeMushroom");
			moveToExploitable("SnakeMushroom");
			moveToExploitable("PurpleVasePlant");

			foreach (string k in new List<String>(Language.main.strings.Keys)) {
				//SNUtil.log(k+" :>");
				//SNUtil.log(Language.main.Get(k));
				string k2 = k.ToLowerInvariant();
				if (k2.Contains("tooltip") || k2.Contains("desc") || k2.Contains("ency"))
					continue;
				string s = Language.main.Get(k);
				if (s.ToLowerInvariant().Contains("creepvine"))
					continue;
				string s0 = s;
				s = s.Replace(" seed", " Sample");
				//s = s.Replace(" spore", " Sample");
				s = s.Replace(" Seed", " Sample");
				//s = s.Replace(" Spore", " Sample");
				if (s != s0)
					CustomLocaleKeyDatabase.registerKey(k, s);
			}

			CustomLocaleKeyDatabase.registerKey("EncyDesc_Aurora_DriveRoom_Terminal1", Language.main.Get("EncyDesc_Aurora_DriveRoom_Terminal1").Replace("from 8 lifepods", "from 14 lifepods").Replace("T+8hrs: 1", "T+8hrs: 7"));
			CustomLocaleKeyDatabase.registerKey("EncyDesc_WaterFilter", Language.main.Get("EncyDesc_WaterFilter") + "\n\nNote: In highly mineralized regions, salt collection is both accelerated and may yield additional byproducts.");

			string key = "EncyDesc_"+ TechType.SpottedLeavesPlant.AsString();
			CustomLocaleKeyDatabase.registerKey(key, Language.main.Get(key) + "\n\nThese compounds appear to be compatible with human digestion.");

			CustomLocaleKeyDatabase.registerKey("Need_laserCutterBulkhead_Chit", SeaToSeaMod.miscLocale.getEntry("bulkheadLaserCutterUpgrade").getField<string>("error"));
			CustomLocaleKeyDatabase.registerKey("Tooltip_" + TechType.MercuryOre.AsString(), SeaToSeaMod.miscLocale.getEntry("MercuryDesc").desc);
			CustomLocaleKeyDatabase.registerKey("EncyDesc_Mercury", SeaToSeaMod.miscLocale.getEntry("MercuryDesc").pda);
			CustomLocaleKeyDatabase.registerKey("Tooltip_" + TechType.PrecursorKey_Red.AsString(), SeaToSeaMod.itemLocale.getEntry("redkey").desc);
			CustomLocaleKeyDatabase.registerKey("Tooltip_" + TechType.PrecursorKey_White.AsString(), SeaToSeaMod.itemLocale.getEntry("whitekey").desc);

			Campfire.updateLocale(); //call after the above locale init
			KeypadCodeSwappingSystem.instance.patchEncyPages();

			CustomLocaleKeyDatabase.registerKey(SeaToSeaMod.tunnelLight.TechType.AsString(), Language.main.Get(TechType.LEDLight));
			CustomLocaleKeyDatabase.registerKey("Tooltip_" + SeaToSeaMod.tunnelLight.TechType.AsString(), Language.main.Get("Tooltip_" + TechType.LEDLight.AsString()));

			CustomLocaleKeyDatabase.registerKey(SeaToSeaMod.deadMelon.TechType.AsString(), Language.main.Get(TechType.MelonPlant));

			EcoceanMod.lavaShroom.pdaPage.append("\n\n" + SeaToSeaMod.miscLocale.getEntry("Appends").getField<string>("lavashroom"));

			CustomLocaleKeyDatabase.registerKey("Tooltip_" + TechType.VehicleHullModule3.AsString(), Language.main.Get("Tooltip_" + TechType.VehicleHullModule3.AsString()).Replace("maximum", "900m"));

			StoryGoalCustomEventHandler.main.sunbeamGoals[StoryGoalCustomEventHandler.main.sunbeamGoals.Length - 2].trigger = SeaToSeaMod.sunbeamCountdownTrigger.key;
		}

		private static void moveToExploitable(string key) {
			PDAEncyclopedia.EntryData data = PDAEncyclopedia.mapping[key];/*
	    	TreeNode root = PDAEncyclopedia.tree;
	    	TreeNode node = root;
	    	foreach (string s in data.path.Split('/')) {
	    		node = node[s];
	    	}
	    	if (node == null) {
	    		SNUtil.log("Found no ency node for "+key+" in "+data.path);
	    		return;
	    	}*/
			//node.parent.RemoveNode(node);
			//root[3][1][0].AddNode(node);
			data.path = data.path.Replace("Sea", "Exploitable").Replace("Land", "Exploitable");
			data.nodes = PDAEncyclopedia.ParsePath(data.path);
		}

		public static void tickPlayer(Player ep) {
			if (playerDied) {
				C2CUtil.setupDeathScreen();
				return;
			}
			if (skipPlayerTick || !ep || !DIHooks.isWorldLoaded())
				return;

			//SNUtil.writeToChat(WorldUtil.getRegionalDescription(ep.transform.position));

			if (playerBaseO2 == null) {
				foreach (Oxygen o in Player.main.oxygenMgr.sources) {
					if (o.isPlayer) {
						playerBaseO2 = o;
						break;
					}
				}
			}

			float time = DayNightCycle.main.timePassedAsFloat;

			if (ep.GetBiomeString() == "observatory") {
				ObservatoryDiscoverySystem.instance.tick(ep);
			}

			MoraleSystem.instance.tick(ep);

			if (KeyCodeUtils.GetKeyDown(SeaToSeaMod.keybinds.getBinding(C2CModOptions.PROPGUNSWAP))) {
				C2CUtil.swapRepulsionCannons();
			}

			if (IngameMenu.main && Time.timeSinceLevelLoad - IngameMenu.main.lastSavedStateTime >= SeaToSeaMod.config.getFloat(C2CConfig.ConfigEntries.SAVETHRESH) * 60F && time - lastSaveAlertTime >= SeaToSeaMod.config.getFloat(C2CConfig.ConfigEntries.SAVECOOL) * 60F && allowSaving(true)) {
				SNUtil.writeToChat("It has been " + Utils.PrettifyTime((int)(Time.timeSinceLevelLoad - IngameMenu.main.lastSavedStateTime)) + " since you last saved; you should do so again soon.");
				lastSaveAlertTime = time;
			}

			if (FCSIntegrationSystem.instance.isLoaded())
				FCSIntegrationSystem.instance.tickNotifications(time);
			if (DEIntegrationSystem.instance.isLoaded())
				DEIntegrationSystem.instance.tickVoidThalassaceanSpawner(ep);

			LifeformScanningSystem.instance.tick(time);
			DataCollectionTracker.instance.tick(time);
			EmperorRootOil.tickInventory(ep, time);

			if (Camera.main && Vector3.Distance(ep.transform.position, Camera.main.transform.position) > 5) {
				if (VoidSpikesBiome.instance.getDistanceToBiome(Camera.main.transform.position, true) < 200)
					WaterBiomeManager.main.GetComponent<WaterscapeVolume>().fogEnabled = true;
			}

			ItemUnlockLegitimacySystem.instance.tick(ep);

			if (Time.deltaTime > 0)
				BKelpBumpWormSpawner.tickSpawnValidation(ep);

			if (LiquidBreathingSystem.instance.hasTankButNoMask()) {
				Oxygen ox = Inventory.main.equipment.GetItemInSlot("Tank").item.gameObject.GetComponent<Oxygen>();
				ep.oxygenMgr.UnregisterSource(ox);
				ep.oxygenMgr.UnregisterSource(playerBaseO2);
			}
			else if (LiquidBreathingSystem.instance.hasLiquidBreathing()) {
				//SNUtil.writeToChat("Tick liquid breathing: "+LiquidBreathingSystem.instance.isLiquidBreathingActive(ep));
				Oxygen ox = Inventory.main.equipment.GetItemInSlot("Tank").item.gameObject.GetComponent<Oxygen>();
				if (LiquidBreathingSystem.instance.isLiquidBreathingActive(ep)) {
					LiquidBreathingSystem.instance.tickLiquidBreathing(true, true);
					ep.oxygenMgr.UnregisterSource(playerBaseO2);
					ep.oxygenMgr.RegisterSource(ox);
				}
				else {
					LiquidBreathingSystem.instance.tickLiquidBreathing(true, false);
					ep.oxygenMgr.UnregisterSource(ox);
					ep.oxygenMgr.RegisterSource(playerBaseO2);
					float add = Mathf.Min(ep.oxygenMgr.oxygenUnitsPerSecondSurface, ox.oxygenCapacity - ox.oxygenAvailable) * Time.deltaTime;
					if (add > 0.01) {
						if (LiquidBreathingSystem.instance.tryFillPlayerO2Bar(ep, ref add)) {
							ox.AddOxygen(add);
							//LiquidBreathingSystem.instance.onAddO2ToBar(add);
						}
					}
				}
			}
			else {
				LiquidBreathingSystem.instance.tickLiquidBreathing(false, false);
				ep.oxygenMgr.RegisterSource(playerBaseO2);
				if (time - LiquidBreathingSystem.instance.getLastUnequippedTime() < 0.5)
					ep.oxygenMgr.RemoveOxygen(ep.oxygenMgr.GetOxygenAvailable());
			}

			if (!PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key))
				SeaToSeaMod.sanctuaryDirectionHint.deactivate();
			if (!VoidSpikesBiome.instance.isRadioFired())
				SeaToSeaMod.voidSpikeDirectionHint.deactivate();
			if (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePrompt).key) || PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePromptLate).key))
				Story.StoryGoal.Execute("KelpCaveHint", Story.GoalType.Story);
			//	C2CProgression.spawnPOIMarker("kelpCavePOI", C2CProgression.instance.dronePDACaveEntrance.setY(-5));
			//if (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.DuneArchPrompt).key))
			//C2CProgression.spawnPOIMarker("duneArch", POITeleportSystem.getPOI("dunearch"));

			float distsq = (ep.transform.position - crashMesa).sqrMagnitude - 400;
			if (time >= nextSanctuaryPromptCheckTime && !PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key)) {
				nextSanctuaryPromptCheckTime = time + 1;
				if (distsq < 2500 || Vector3.Distance(ep.transform.position, auroraFront) < 144 || Vector3.Distance(ep.transform.position, trailerBaseBioreactor) < 200 || Vector3.Distance(ep.transform.position, CrashZoneSanctuaryBiome.biomeCenter) < 200) {
					Player.main.gameObject.EnsureComponent<DelayedPromptsCallback>().Invoke("triggerSanctuary", 20);
				}
			}

			if (distsq < 25 * 25 || (distsq <= 250 * 250 && UnityEngine.Random.Range(0F, 1F) <= 0.075F * Time.timeScale * (distsq <= 10000 ? 2.5F : 1))) {
				IEcoTarget tgt = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Leviathan, crashMesa, eco => eco.GetGameObject().GetComponent<ReaperLeviathan>(), 6);
				if (tgt != null && (tgt.GetPosition() - crashMesa).sqrMagnitude >= Mathf.Max(distsq, 225)) {
					GameObject go = tgt.GetGameObject();
					Vehicle v = ep.GetVehicle();
					GameObject hit = v ? v.gameObject : ep.gameObject;
					Vector3 pos = distsq <= 2500 ? hit.transform.position : MathUtil.getRandomVectorAround(crashMesa, 40).setY(crashMesa.y);
					go.EnsureComponent<C2CReaper>().forceAggression(hit, pos);
				}
			}

			VoidSpikeLeviathanSystem.instance.tick(ep);

			ExplorationTrackerPages.instance.tick();

			if (ep.currentSub == null && UnityEngine.Random.Range(0, (int)(10 / Time.timeScale)) == 0) {
				if (ep.GetVehicle() == null) {
					float ventDist = -1;
					IEcoTarget tgt = EcoRegionManager.main.FindNearestTarget(EcoTargetType.HeatArea, ep.transform.position, null, 3);
					if (tgt != null)
						ventDist = Vector3.Distance(tgt.GetPosition(), ep.transform.position);
					if (ventDist >= 0 && ventDist <= 25) {
						float f = Math.Min(1, (40 - ventDist) / 32F);
						foreach (InventoryItem item in Inventory.main.container) {
							if (item != null && item.item) {
								Battery b = item.item.gameObject.GetComponentInChildren<Battery>();
								if (b != null && Mathf.Approximately(b.capacity, C2CItems.t2Battery.capacity)) {
									b.charge = Math.Min(b.charge + (0.5F * f), b.capacity);
									continue;
								}
								EnergyMixin e = item.item.gameObject.GetComponentInChildren<EnergyMixin>();
								if (e != null && e.battery != null && Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity)) {
									//SNUtil.writeToChat("Charging "+item.item+" by factor "+f+", d="+ventDist);
									e.AddEnergy(0.5F * f);
								}
							}
						}
					}
				}
			}

			if (time >= nextBkelpBaseAmbCheckTime) {
				nextBkelpBaseAmbCheckTime = time + UnityEngine.Random.Range(0.5F, 2.5F);
				if (Vector3.Distance(ep.transform.position, bkelpBaseGeoCenter) <= 60) {
					StoryGoal.Execute("SeeBkelpBase", Story.GoalType.Story);
					if (time >= nextBkelpBaseAmbTime) {
						SNUtil.log("Queuing bkelp base ambience @ " + ep.transform.position);
						VanillaMusic.WRECK.play();
						nextBkelpBaseAmbTime = DayNightCycle.main.timePassedAsFloat + UnityEngine.Random.Range(60F, 90F);
					}
				}
				else {
					VanillaMusic.WRECK.disable();
				}
			}
		}

		public static void onEquipmentAdded(string slot, InventoryItem item) {
			if (item.item.GetTechType() == C2CItems.liquidTank.TechType)
				LiquidBreathingSystem.instance.onEquip();
		}

		public static void onEquipmentRemoved(string slot, InventoryItem item) {
			if (item.item.GetTechType() == C2CItems.liquidTank.TechType)
				LiquidBreathingSystem.instance.onUnequip();
		}

		public static void tickO2Bar(uGUI_OxygenBar gui) {
			if (skipO2)
				return;
			LiquidBreathingSystem.instance.updateOxygenGUI(gui);
		}

		public static float getO2RedPulseTime(float orig) {
			return skipO2 ? orig : LiquidBreathingSystem.instance.isO2BarFlashingRed() ? 6 : orig;
		}

		public static void canPlayerBreathe(DIHooks.BreathabilityCheck ch) {
			if (skipO2)
				return;
			//SNUtil.writeToChat(orig+": "+p.IsUnderwater()+" > "+Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType));
			if (!LiquidBreathingSystem.instance.isO2BarAbleToFill(ch.player))
				ch.breathable = false;
		}

		public static float addO2ToPlayer(OxygenManager mgr, float f) {
			if (skipO2)
				return f;
			if (!LiquidBreathingSystem.instance.isO2BarAbleToFill(Player.main))
				f = 0;
			return f;
		}

		public static void addOxygenAtSurfaceMaybe(OxygenManager mgr, float time) {
			if (skipO2)
				return;
			if (LiquidBreathingSystem.instance.isO2BarAbleToFill(Player.main)) {
				//SNUtil.writeToChat("Add surface O2");
				mgr.AddOxygenAtSurface(time);
			}
		}

		public static void getBiomeAt(DIHooks.BiomeCheck b) {
			if (skipBiomeCheck)
				return;
			if (VoidSpikesBiome.instance.isInBiome(b.position)) {
				b.setValue(VoidSpikesBiome.instance.biomeName);
				b.lockValue();
				//if (BiomeBase.logBiomeFetch)
				//	SNUtil.writeToChat("Biome WBM fetch overridden to "+VoidSpikesBiome.biomeName);
			}
			else if (UnderwaterIslandsFloorBiome.instance.isInBiome(b.originalValue, b.position)) {
				b.setValue(UnderwaterIslandsFloorBiome.instance.biomeName);
				b.lockValue();
				//if (BiomeBase.logBiomeFetch)
				//	SNUtil.writeToChat("Biome WBM fetch overridden to "+UnderwaterIslandsFloorBiome.biomeName);
			}/*
	   		if (Vector3.Distance(dmg.target.transform.position, bkelpBaseGeoCenter) <= 60 && !dmg.target.FindAncestor<Vehicle>()) {
	   			b.setValue(BKelpBaseBiome.biomeName);
	    		b.lockValue();
	   		}*/
			else if (CrashZoneSanctuaryBiome.instance.isInBiome(b.position)) {
				b.setValue(CrashZoneSanctuaryBiome.instance.biomeName);
				b.lockValue();
				//if (BiomeBase.logBiomeFetch)
				//	SNUtil.writeToChat("Biome WBM fetch overridden to "+UnderwaterIslandsFloorBiome.biomeName);
			}
			else if (Vector3.Distance(b.position, voidWreckVoidPatch) <= 40) {
				b.setValue(VanillaBiomes.VOID.mainID);
				b.lockValue();
			}
		}

		public static void getSwimSpeed(DIHooks.SwimSpeedCalculation ch) {
			float morale = MoraleSystem.instance.moralePercentage;
			if (morale < 25) {
				ch.setValue(ch.getValue() * Mathf.Lerp(0.5F, 1F, morale / 25F));
			}

			if (Player.main.motorMode != Player.MotorMode.Dive)
				return;
			//SNUtil.writeToChat("Get swim speed, was "+f+", has="+LiquidBreathingSystem.instance.hasLiquidBreathing());
			if (LiquidBreathingSystem.instance.hasLiquidBreathing())
				ch.setValue(ch.getValue() - 0.1F); //was 0.25
			if (WorldUtil.isInDRF(Player.main.transform.position))
				ch.setValue(ch.getValue() * 0.5F);
			if ((Player.main.transform.position - C2CHooks.crashMesa).sqrMagnitude <= 2500) {
				ch.setValue(ch.getValue() * 0.4F);
			}
		}

		public static float getSeaglideSpeed(float f) { //1.45 by default
			if (SeaToSeaMod.fastSeaglideCheatActive)
				return 40;
			//SNUtil.writeToChat("Get SG speed, was "+f+", has="+Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity));
			if (isHeldToolAzuritePowered()) {
				float bonus = 0.75F; //was 0.55 then 0.95
				float depth = Player.main.GetDepth();
				float depthFactor = depth <= 50 ? 1 : 1 - ((depth - 50) / 350F);
				if (depthFactor > 0) {
					f += bonus * depthFactor;
				}
			}
			if (WorldUtil.isInDRF(Player.main.transform.position))
				f *= 0.5F;
			return f;
		}

		public static float getScannerSpeed(float f) { //f is a divisor, scanTime
			if (isHeldToolAzuritePowered()) {
				f *= 0.5F; //double speed
			}
			return f;
		}
		/* DO NOT USE - RISKS VOIDING
	    public static float getBuilderSpeed(float f) { //f is a divisor, item count
	    	if (isHeldToolAzuritePowered()) {
	    		f *= 0.667F; //1.5x speed
	    	}
	    	return f;
	    }*/

		public static float getLaserCutterSpeed(LaserCutter lc) { //25 by default
			float amt = lc.healthPerWeld;
			if (isHeldToolAzuritePowered())
				amt *= 1.5F;
			return amt;
		}

		public static float getRepairSpeed(Welder lc) { //10 by default
			float amt = lc.healthPerWeld;
			if (isHeldToolAzuritePowered())
				amt *= 2F;
			return amt;
		}

		public static float getConstructableSpeed() {
			return NoCostConsoleCommand.main.fastBuildCheat ? 0.01F : !GameModeUtils.RequiresIngredients() ? 0.2F : StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.auroraTerminal.key) ? 0.67F : 1F;
		}

		public static float getVehicleConstructionSpeed(ConstructorInput inp, TechType made, float time) {
			if (StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.auroraTerminal.key))
				time *= made == TechType.RocketBase ? 0.8F : 0.5F;
			else
				time *= made == TechType.Seamoth ? 2F : 1.5F;
			return time;
		}

		public static float getRocketConstructionSpeed(float time) {
			time *= StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.auroraTerminal.key) ? 0.8F : 1.6F;
			return time;
		}

		public static bool getFabricatorTime(DIHooks.CraftTimeCalculation calc) {
			if (StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.auroraTerminal.key)) {
				calc.craftingDuration *= (float)MathUtil.linterpolate(calc.craftingDuration, 1, 2, 1, 0.5, true);
				calc.craftingDuration = Mathf.Min(calc.craftingDuration, 10);
			}
			else {
				calc.craftingDuration *= 1.5F;
			}
			if (!QModManager.API.QModServices.Main.ModPresent("AgonyRadialCraftingTabs")) {
				float morale = MoraleSystem.instance.moralePercentage;
				float f = 1;
				if (morale < 10) {
					f = Mathf.Lerp(6F, 3F, morale / 10F);
				}
				else if (morale < 25) {
					f = (float)MathUtil.linterpolate(morale, 10, 25, 3, 1.5, true);
				}
				else if (morale < 50) {
					f = (float)MathUtil.linterpolate(morale, 25, 50, 1.5, 1, true);
				}
				else if (morale >= 90) {
					f = (float)MathUtil.linterpolate(morale, 90, 100, 1, 0.5F, true);
				}
				calc.craftingDuration *= f;
				//SNUtil.writeToChat("Morale is " + morale.ToString("0.0") + " -> "+f.ToString("0.00")+"x duration");
			}
			return true;
		}

		public static float getRadialTabAnimSpeed(float orig) {
			float morale = MoraleSystem.instance.moralePercentage;
			float f = 1;
			if (morale < 10) {
				f = Mathf.Lerp(0.125F, 0.33F, morale / 10F);
			}
			else if (morale < 25) {
				f = (float)MathUtil.linterpolate(morale, 10, 25, 0.33, 0.67, true);
			}
			else if (morale < 50) {
				f = (float)MathUtil.linterpolate(morale, 25, 50, 0.67, 1, true);
			}
			return f * orig;
		}

		public static float getPropulsionCannonForce(PropulsionCannon prop) {
			float ret = prop.attractionForce;
			if (isHeldToolAzuritePowered())
				ret *= 3;
			float temp = WaterTemperatureSimulation.main.GetTemperature(Player.main.transform.position);
			if (temp >= 100)
				ret *= Mathf.Max(0.04F, 1F / ((temp - 99) / 50F));
			return ret;
		}

		public static float getPropulsionCannonThrowForce(PropulsionCannon prop) {
			float ret = prop.shootForce;
			if (isHeldToolAzuritePowered())
				ret *= 1.5F;
			return ret;
		}

		public static float getRepulsionCannonThrowForce(RepulsionCannon prop) {
			float ret = RepulsionCannon.shootForce;
			if (isHeldToolAzuritePowered())
				ret *= 4;
			return ret;
		}

		public static void onRepulsionCannonTryHit(RepulsionCannon prop, Rigidbody rb) {
			if (isHeldToolAzuritePowered() && rb.gameObject.GetFullHierarchyPath().Contains("CaptainsQuarters_Keypad")) {
				StarshipDoor s = rb.GetComponent<StarshipDoor>();
				//SNUtil.writeToChat("S: "+s);
				if (s) {
					GameObject go = s.gameObject;
					GameObject door = rb.gameObject.getChildObject("Starship_doors_manual_01/Starship_doors_automatic");
					Rigidbody rb2 = door.EnsureComponent<Rigidbody>();
					rb2.copyObject(rb);
					s.GetComponent<StarshipDoorLocked>().destroy(false); //need to do directly since removecomponent calls destroyImmediate, and this is an anim call
					s.destroy(false);
					//SNUtil.writeToChat("C: "+string.Join(", ", go.GetComponents<MonoBehaviour>().Select<Component, string>(c => c.GetType().Name).ToArray()));
					rb2.isKinematic = false;
					rb2.mass = 500;
					rb2.ResetCenterOfMass();
					rb2.ResetInertiaTensor();
					rb2.detectCollisions = true;
					rb2.transform.SetParent(null);
					foreach (Collider c in rb2.GetComponentsInChildren<Collider>()) {
						c.enabled = false;
					}
					rb2.velocity = (MainCamera.camera.transform.forward * 30F) + (Vector3.up * 7.5F);
					rb2.angularVelocity = MathUtil.getRandomVectorAround(Vector3.zero, 2.5F);
					FlyingDoor fd = door.EnsureComponent<FlyingDoor>();
					fd.Invoke("solidify", 0.05F);
					fd.Invoke("thump", 0.15F);
				}
			}
		}

		private class FlyingDoor : MonoBehaviour {

			private static readonly SoundManager.SoundData impactSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "doorhit", "Sounds/doorhit.ogg", SoundManager.soundMode3D, s => {
				SoundManager.setup3D(s, 200);
			}, SoundSystem.masterBus);

			void solidify() {
				foreach (Collider c in this.GetComponentsInChildren<Collider>()) {
					c.enabled = true;
				}
			}

			void thump() {
				SoundManager.playSoundAt(impactSound, transform.position, false, 40, 2);
				SoundManager.playSoundAt(impactSound, transform.position, false, 40, 2);
			}

		}

		public static void modifyPropulsibility(DIHooks.PropulsibilityCheck ch) {
			if (ch.obj.FindAncestor<Rigidbody>().name.StartsWith("ExplorableWreck", StringComparison.InvariantCultureIgnoreCase)) {
				ch.value = 1;
				return;
			}
			Drillable d = ch.obj.FindAncestor<Drillable>();
			if (d) {
				SpecialDrillable s = ch.obj.FindAncestor<SpecialDrillable>();
				if (!s || s.canBeMoved())
					ch.value = 99999999;
			}
			if (isHeldToolAzuritePowered())
				ch.value *= ch.isMass ? 6 : 4;
		}

		public static bool isHeldToolAzuritePowered() {
			if (Inventory.main == null)
				return false;
			Pickupable held = Inventory.main.GetHeld();
			if (!held || !held.gameObject)
				return false;
			EnergyMixin e = held.gameObject.GetComponent<EnergyMixin>();
			return e && (e.battery != null && Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity));
		}

		public static void onThingInO2Area(OxygenArea a, Collider obj) {
			if (obj.isPlayer()) {
				lastO2PipeTime = DayNightCycle.main.timePassedAsFloat;
				float o2ToAdd = Math.Min(a.oxygenPerSecond * Time.deltaTime, Player.main.GetOxygenCapacity() - Player.main.GetOxygenAvailable());
				if (o2ToAdd > 0)
					LiquidBreathingSystem.instance.tryFillPlayerO2Bar(Player.main, ref o2ToAdd, true);
				if (LiquidBreathingSystem.instance.hasLiquidBreathing()) {
					LiquidBreathingSystem.instance.checkLiquidBreathingSupport(a);
				}
			}
		}

		public static void updateToolDefaultBattery(EnergyMixin mix) {
			Pickupable p = mix.gameObject.GetComponent<Pickupable>();
			//SNUtil.writeToChat("update tool default battery: "+p+" > "+(p == null ? "" : ""+p.GetTechType()));
			if (p == null)
				return;
			addT2BatteryAllowance(mix);
			if (p.GetTechType() == loadTechPistol()) {
				mix.defaultBattery = C2CItems.t2Battery.TechType;
				return;
			}
			switch (p.GetTechType()) {
				case TechType.StasisRifle:
				case TechType.LaserCutter:
					mix.defaultBattery = C2CItems.t2Battery.TechType;
					break;
			}
		}

		public static void addT2BatteryAllowance(EnergyMixin mix) {
			if (mix.compatibleBatteries.Contains(TechType.Battery) && !mix.compatibleBatteries.Contains(C2CItems.t2Battery.TechType)) {
				mix.compatibleBatteries.Add(C2CItems.t2Battery.TechType);/*
	    		List<EnergyMixin.BatteryModels> arr = mix.batteryModels.ToList();
	    		GameObject go = C2CItems.t2Battery.GetGameObject();
	    		go.SetActive(false);
	    		arr.Add(new EnergyMixin.BatteryModels{model = go, techType = C2CItems.t2Battery.TechType});
	    		mix.batteryModels = arr.ToArray();*/
			}
		}

		public static GameObject onSpawnBatteryForEnergyMixin(GameObject go) {
			//SNUtil.writeToChat("Spawned a "+go);
			go.SetActive(false);
			return go;
		}

		public static void collectTimeCapsule(TimeCapsule tc) {
			bool someBlocked = false;
			try {
				PDAEncyclopedia.AddTimeCapsule(tc.id, true);
				PlayerTimeCapsule.main.RegisterOpen(tc.instanceId);
				List<TimeCapsuleItem> items = TimeCapsuleContentProvider.GetItems(tc.id);
				if (items != null) {
					foreach (TimeCapsuleItem tci in items) {
						if (C2CProgression.instance.isTechGated(tci.techType) || C2CProgression.instance.isTechGated(tci.batteryType)) {
							someBlocked = true;
							continue;
						}
						Pickupable pickupable = tci.Spawn();
						if (pickupable != null) {
							Inventory.main.ForcePickup(pickupable);
						}
					}
				}
			}
			finally {
				tc.gameObject.destroy(false);
			}
			if (someBlocked) {

			}
		}

		public static void setPingAlpha(uGUI_Ping ico, float orig, PingInstance inst, bool text) {
			/*
	    	if (Player.main != null && VoidSpikesBiome.instance.isInBiome(Player.main.transform.position)) {
	    		return inst.pingType == PingType.Seamoth;
	    	}*/
			float a = Mathf.Min(VoidSpikeLeviathanSystem.instance.getNetScreenVisibilityAfterFlash(), orig);
			if (text)
				ico.SetTextAlpha(a);
			else
				ico.SetIconAlpha(a);
		}

		public static Vector3 getApparentPingPosition(PingInstance inst) {
			if (!inst || !inst.origin)
				return Vector3.zero;
			Vector3 pos = inst.origin.position;
			if (inst.pingType == SeaToSeaMod.voidSpikeDirectionHint.signalType) {
				pos = VoidSpikesBiome.instance.getPDALocation() + VoidSpikesBiome.voidEndpoint500m - VoidSpikesBiome.end500m;//VoidSpikesBiome.voidEndpoint500m;
			}
			if (Player.main != null && VoidSpikesBiome.instance.isInBiome(Player.main.transform.position) && !VoidSpikesBiome.instance.isInBiome(pos) && Vector3.Distance(Player.main.transform.position, pos) > 2) {
				pos += VoidSpikesBiome.end500m - VoidSpikesBiome.voidEndpoint500m;
			}
			return pos;
		}

		public static void recalculateDamage(DIHooks.DamageToDeal dmg) {
			//if (type == DamageType.Acid && dealer == null && target.GetComponentInParent<SeaMoth>() != null)
			//	return 0;
			//SNUtil.writeToChat(dmg.target.name);
			Player p = dmg.target.FindAncestor<Player>();
			if (p != null) {
				if (dmg.type == DamageType.Heat && Vector3.Distance(p.transform.position, mountainBaseGeoCenter) <= 27) {
					dmg.setValue(0);
				}
				else {
					bool flag = C2CItems.hasSealedOrReinforcedSuit(out bool seal, out bool reinf);
					if (!reinf && dmg.type == DamageType.Heat && WaterTemperatureSimulation.main.GetTemperature(p.transform.position) > 270) {
						dmg.setValue(dmg.getAmount() * 1.25F);
					}
					else if (flag) {
						if ((dmg.type == DamageType.Poison || dmg.type == DamageType.Acid || dmg.type == DamageType.Electrical) && dmg.dealer != Player.main.gameObject) {
							//this means something has to deal at least 50 damage to do anything with seal suit, and 20 with reinf (yet most poison is DoT and so does less per)
							//and lots of other damage is *= Time.deltaTime too, so is tiny per
							//even LR brine damage is 10 in 1s increments, though is caught by the upper case instead
							if (dmg.type == DamageType.Acid && dmg.target.GetComponent<AcidicBrineDamage>()) {
								dmg.setValue(dmg.getAmount() * (seal ? 0.4F : 0.8F)); //from 10 to 4 or 8
							}
							else {
								dmg.setValue(dmg.getAmount() * (seal ? 0.2F : 0.5F));
								bool skipFlat = false;
								foreach (DamageOverTime dot in dmg.target.GetComponents<DamageOverTime>()) { //assume is DoT, do not do the flat reduction, just a -80% or -50%
									if (dot.damageType == dmg.type) {
										skipFlat = true;
										break;
									}
								}
								if (!skipFlat) //only do flat reduction on singular hits, which does include Update *= dT, making you immune to gradual health loss from things 
									dmg.setValue(dmg.getAmount() - 10);
							}
						}
					}
				}
				float amt = dmg.getAmount();
				if (amt > 0.01 && !IntroVignette.isIntroActive) { //the panel to the face actually DOES DAMAGE...
					float hit = 0;
					if (amt <= 10) {
						hit = Mathf.Lerp(2, 5, amt/10F);
					}
					else {
						float dmgRef = Mathf.Clamp(amt, 10, 50)-10; //0-40
						hit = Mathf.Lerp(5, 75, dmgRef / 40F);
					}
					MoraleSystem.instance.shiftMorale(-hit*MoraleSystem.MORALE_DAMAGE_COEFFICIENT);
				}
			}
			else {
				//SubRoot sub = dmg.target.FindAncestor<SubRoot>();
				//if (sub && sub.isCyclops)
				//	SNUtil.writeToChat("Cyclops ["+dmg.target.GetFullHierarchyPath()+"] took "+dmg.amount+" of "+dmg.type+" from '"+dmg.dealer+"'");
				if (dmg.type == DamageType.Normal || dmg.type == DamageType.Drill || dmg.type == DamageType.Puncture || dmg.type == DamageType.Electrical) {
					DeepStalkerTag s = dmg.target.FindAncestor<DeepStalkerTag>();
					if (s) {
						if (dmg.type == DamageType.Electrical)
							s.onHitWithElectricDefense();
						dmg.setValue(dmg.getAmount() * 0.5F); //50% resistance to "factorio physical" damage, plus electric to avoid PD killing them
					}
				}
				if (dmg.type == DamageType.Electrical) {
					VoidSpikeLeviathan.VoidSpikeLeviathanAI s = dmg.target.FindAncestor<VoidSpikeLeviathan.VoidSpikeLeviathanAI>();
					if (s) {
						dmg.setValue(0);
						dmg.lockValue();
					}
					if (Vector3.Distance(dmg.target.transform.position, bkelpBaseGeoCenter) <= 60 && !dmg.target.FindAncestor<Vehicle>()) {
						dmg.setValue(0);
					}
				}
				if (dmg.type == DamageType.Heat && DEIntegrationSystem.instance.isLoaded() && CraftData.GetTechType(dmg.target) == DEIntegrationSystem.instance.getRubyPincher()) {
					dmg.setValue(dmg.getAmount() * 0.5F);
				}
				if (dmg.type == DamageType.Normal && VanillaBiomes.VOID.isInBiome(dmg.target.transform.position)) {
					SeaMoth sm = dmg.target.FindAncestor<SeaMoth>();
					if (sm && !sm.vehicleHasUpgrade(C2CItems.voidStealth.TechType))
						dmg.setValue(dmg.getAmount() * 1.5F);
				}
				if (dmg.type == DamageType.Poison || !dmg.target.isFarmedPlant()) {
					if (dmg.target.FindAncestor<GlowKelpTag>()) {
						dmg.setValue(0);
					}
				}
			}
		}

		public static float getVehicleRechargeAmount(Vehicle v) {
			float baseline = 0.0025F;
			SubRoot b = v.GetComponentInParent<SubRoot>();
			if (b && b.isBase && b.currPowerRating > 0) {
				baseline *= 4;
			}
			return baseline;
		}

		public static float getPlayerO2Rate(Player ep) {
			return EnvironmentalDamageSystem.instance.getPlayerO2Rate(ep);
		}

		public static float getPlayerO2Use(Player ep, float breathingInterval, int depthClass) {
			return EnvironmentalDamageSystem.instance.getPlayerO2Use(ep, breathingInterval, depthClass);
		}

		public static void tickPlayerEnviroAlerts(RebreatherDepthWarnings warn) {
			EnvironmentalDamageSystem.instance.tickPlayerEnviroAlerts(warn);
		}

		public static void doEnvironmentalDamage(TemperatureDamage dmg) {
			EnvironmentalDamageSystem.instance.tickTemperatureDamages(dmg);
		}

		public static void onSetPlayerACU(Player ep, WaterPark w) {
			if (w) {
				foreach (WaterParkItem wp in w.items) {
					LifeformScanningSystem.instance.onObjectSeen(wp.gameObject, true, true);
				}
			}
		}

		public static void onCrashfishExplode(Crash c) {
			LifeformScanningSystem.instance.onObjectSeen(c.gameObject, false);
		}

		public static void onItemPickedUp(DIHooks.ItemPickup ip) {
			Pickupable p = ip.item;
			AvoliteSpawner.instance.cleanPickedUp(p);
			p.gameObject.removeComponent<SinkingGroundChunk>();
			FCSIntegrationSystem.instance.modifyPeeperFood(p);
			LifeformScanningSystem.instance.onObjectSeen(p.gameObject, true);
			TechType tt = p.GetTechType();
			C2CItems.IngotDefinition ingot = C2CItems.getIngotByUnpack(tt);
			if (ingot != null) {
				ingot.pickupUnpacked();
				ip.destroy = true;
			}
			else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType) {
				Story.StoryGoal.Execute("Azurite", Story.GoalType.Story);
				if (VanillaBiomes.ILZ.isInBiome(p.transform.position))
					Story.StoryGoal.Execute("ILZAzurite", Story.GoalType.Story);
				if (ip.prawn || !C2CItems.hasSealedOrReinforcedSuit(out bool seal, out bool reinf)) {
					LiveMixin lv = ip.prawn ? ip.prawn.liveMixin : Player.main.gameObject.GetComponentInParent<LiveMixin>();
					float dmg = lv.maxHealth * (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.3F : 0.2F);
					if (Vector3.Distance(p.transform.position, Azurite.mountainBaseAzurite) <= 8)
						dmg *= 0.75F;
					if (ip.prawn)
						dmg *= 0.04F; //do about 2% damage
					lv.TakeDamage(dmg, Player.main.gameObject.transform.position, DamageType.Electrical, p.gameObject);
				}
			}
			else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType) {
				Story.StoryGoal.Execute("Platinum", Story.GoalType.Story);
				HashSet<DeepStalkerTag> set = WorldUtil.getObjectsNearWithComponent<DeepStalkerTag>(p.transform.position, 60);
				foreach (DeepStalkerTag c in set) {
					if (!c.currentlyHasPlatinum() && !c.GetComponent<WaterParkCreature>()) {
						float chance = Mathf.Clamp01(1F - (Vector3.Distance(c.transform.position, p.transform.position) / 90F));
						if (UnityEngine.Random.Range(0F, 1F) <= chance)
							c.triggerPtAggro(ip.prawn ? ip.prawn.gameObject : Player.main.gameObject);
					}
				}
			}
			else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType) {
				Story.StoryGoal.Execute("PressureCrystals", Story.GoalType.Story);
			}
			else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType) {
				Story.StoryGoal.Execute("Avolite", Story.GoalType.Story);
			}
			else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).TechType) {
				Story.StoryGoal.Execute("Calcite", Story.GoalType.Story);
			}
			else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.OBSIDIAN).TechType) {
				Story.StoryGoal.Execute("Obsidian", Story.GoalType.Story);
			}
			else if (tt == C2CItems.alkali.seed.TechType) {
				Story.StoryGoal.Execute("AlkaliVine", Story.GoalType.Story);
			}
			else if (tt == C2CItems.kelp.seed.TechType) {
				Story.StoryGoal.Execute("DeepvineSamples", Story.GoalType.Story);
			}
			else if (tt == C2CItems.mountainGlow.seed.TechType) {
				Story.StoryGoal.Execute("Pyropod", Story.GoalType.Story);
			}
			else if (tt == C2CItems.voltaicBladderfish.TechType) {
				if (ip.prawn || !C2CItems.hasSealedOrReinforcedSuit(out bool seal, out bool reinf)) {
					LiveMixin lv = ip.prawn ? ip.prawn.liveMixin : Player.main.gameObject.GetComponentInParent<LiveMixin>();
					float dmg = lv.maxHealth * (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.15F : 0.1F);
					if (ip.prawn)
						dmg *= 0.08F; //do about 2% damage
					lv.TakeDamage(dmg, Player.main.gameObject.transform.position, DamageType.Electrical, p.gameObject);
				}
			}
			else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType && VanillaBiomes.ILZ.isInBiome(Player.main.transform.position)) {
				Story.StoryGoal.Execute("Iridium", Story.GoalType.Story);
				C2CItems.hasSealedOrReinforcedSuit(out bool seal, out bool reinf);
				if (!ip.prawn && !reinf) {
					LiveMixin lv = Player.main.gameObject.GetComponentInParent<LiveMixin>();
					float dmg = 40 + ((WaterTemperatureSimulation.main.GetTemperature(Player.main.transform.position) - 90) / 3);
					lv.TakeDamage(dmg, Player.main.gameObject.transform.position, DamageType.Heat, Player.main.gameObject);
				}
			}
			else if (tt == TechType.Kyanite) {
				Story.StoryGoal.Execute("Kyanite", Story.GoalType.Story);
			}
			else if (tt == TechType.Sulphur) {
				Story.StoryGoal.Execute("Sulfur", Story.GoalType.Story);
			}
			else if (tt == TechType.UraniniteCrystal) {
				Story.StoryGoal.Execute("Uranium", Story.GoalType.Story);
			}
			else if (tt == TechType.Nickel) {
				Story.StoryGoal.Execute("Nickel", Story.GoalType.Story);
			}
			else if (tt == TechType.MercuryOre) {
				Story.StoryGoal.Execute("Mercury", Story.GoalType.Story);
			}
			else if (tt == TechType.AdvancedWiringKit/* && Vector3.Distance(Player.main.transform.position, SeaToSeaMod.ADV_WIRING_POS) <= 10*/) {
				Story.StoryGoal.Execute(SeaToSeaMod.ADV_WIRING_GOAL, Story.GoalType.Story);
			}
			else if (tt == CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType) {
				p.GetComponent<NanocarbonTag>().reset();
				Story.StoryGoal.Execute("Nanocarbon", Story.GoalType.Story);
			}
			else if (tt == C2CItems.emperorRootOil.TechType) {
				EmperorRootOil.EmperorRootOilTag tag = p.gameObject.EnsureComponent<EmperorRootOil.EmperorRootOilTag>();
				if (tag.pickupTime < 0)
					tag.pickupTime = DayNightCycle.main.timePassedAsFloat;
			}
			else if (DEIntegrationSystem.instance.isLoaded() && tt == DEIntegrationSystem.instance.thalassaceanCud.TechType) {
				DEIntegrationSystem.C2CThalassacean thala = p.gameObject.FindAncestor<DEIntegrationSystem.C2CThalassacean>();
				if (!thala)
					thala = WorldUtil.getClosest<DEIntegrationSystem.C2CThalassacean>(p.transform.position);
				if (thala && Vector3.Distance(thala.transform.position, p.transform.position) < 30)
					thala.lastCollect = DayNightCycle.main.timePassedAsFloat;
				if (!thala)
					ip.destroy = true; //destroy collected from 000
			}
		}

		public static float getReachDistance() {
			return skipRaytrace || Player.main.GetVehicle() ? 2 : (Player.main.transform.position - lostRiverCachePanel).sqrMagnitude <= 100 ? 4F : VoidSpikesBiome.instance.isInBiome(Player.main.transform.position) ? 3.5F : 2;
		}

		public static void checkTargetingSkip(DIHooks.TargetabilityCheck ch) {
			if (skipRaytrace)
				return;
			//SNUtil.log("Checking targeting skip of "+id+" > "+id.ClassId);
			if (ch.prefab.ClassId == "b250309e-5ad0-43ca-9297-f79e22915db6" && Vector3.Distance(Player.main.transform.position, lrpowerSealSetpieceCenter) <= 8) { //to allow to hit the things inside the mouth
				ch.allowTargeting = false;
			}
			else if (VoidSpike.isSpike(ch.prefab.ClassId) && VoidSpikesBiome.instance.isInBiome(ch.transform.position)) {
				ch.allowTargeting = false;
			}
		}

		public static EntityCell getEntityCellForInt3(Array3<EntityCell> data, Int3 raw, BatchCells batch) {
			int n = data.GetLength(0) / 2;
			Int3 real = raw + new Int3(n, n, n);
			return data.Get(real);
		}

		public static void setEntityCellForInt3(Array3<EntityCell> data, Int3 raw, EntityCell put, BatchCells batch) {
			int n = data.GetLength(0) / 2;
			Int3 real = raw + new Int3(n, n, n);
			data.Set(real, put);
		}

		public static void initBatchCells(BatchCells b) { //default 10 5 5 5
			b.cellsTier0 = new Array3<EntityCell>(20);
			b.cellsTier1 = new Array3<EntityCell>(10);
			b.cellsTier2 = new Array3<EntityCell>(10);
			b.cellsTier3 = new Array3<EntityCell>(10);
		}

		public static void onDataboxActivate(BlueprintHandTarget c) {
			TechType over = DataboxTypingMap.instance.getOverride(c);
			if (over == TechType.None && c.unlockTechType == TechType.RepulsionCannon)
				over = AqueousEngineeringMod.wirelessChargerBlock.TechType;
			if (over != TechType.None && over != c.unlockTechType) {
				SNUtil.log("Blueprint @ " + c.gameObject.transform.position + ", previously " + c.unlockTechType + ", found an override to " + over);
				GameObject go = ObjectUtil.createWorldObject(GenUtil.getOrCreateDatabox(over).TechType);
				if (!go) {
					SNUtil.log("Could not find prefab for databox for " + over + "!");
					return;
				}
				go.transform.SetParent(c.transform.parent);
				go.transform.position = c.transform.position;
				go.transform.rotation = c.transform.rotation;
				go.transform.localScale = c.transform.localScale;
				c.gameObject.destroy(false);
			}
			else if (c.unlockTechType == C2CItems.breathingFluid.TechType && c.transform.position.y < -500) { //clear old databox placement
				c.gameObject.destroy(false);
			}
			else if (c.unlockTechType == C2CItems.liquidTank.TechType && c.transform.position.y > -500) {
				c.gameObject.destroy(false);
			}
			else if (c.gameObject.name == "FCSDataBox(Clone)") { //lock to C2C way
				c.gameObject.destroy(false);
			}
		}

		public static void onTreaderChunkSpawn(SinkingGroundChunk chunk) {
			if (UnityEngine.Random.Range(0F, 1F) < (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.92 : 0.88))
				return;
			int near = 0;
			foreach (Collider c in Physics.OverlapSphere(chunk.gameObject.transform.position, 40F)) {
				if (!c || !c.gameObject) {
					continue;
				}
				TechTag p = c.gameObject.GetComponentInParent<TechTag>();
				if (p != null && p.type == CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType)
					near++;
			}
			if (near > 2)
				return;
			GameObject owner = chunk.gameObject;
			GameObject placed = ObjectUtil.createWorldObject(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType.ToString());
			placed.transform.position = owner.transform.position + (Vector3.up * 0.08F);
			placed.transform.rotation = owner.transform.rotation;
			owner.destroy(false);
		}

		public static void onResourceSpawn(ResourceTracker p) {
			if (skipResourceSpawn)
				return;
			PrefabIdentifier pi = p.GetComponent<PrefabIdentifier>();
			if (pi && pi.ClassId == VanillaResources.LARGE_SULFUR.prefab) {
				p.overrideTechType = TechType.Sulphur;
				p.techType = TechType.Sulphur;
			}
		}

		public static void doEnviroVehicleDamage(CrushDamage dmg) {
			EnvironmentalDamageSystem.instance.tickCyclopsDamage(dmg);
		}

		public static void getWaterTemperature(DIHooks.WaterTemperatureCalculation calc) {
			if (skipTemperatureCheck)
				return;
			if (EnvironmentalDamageSystem.instance.TEMPERATURE_OVERRIDE >= 0) {
				calc.setValue(EnvironmentalDamageSystem.instance.TEMPERATURE_OVERRIDE);
				calc.lockValue();
				return;
			}
			//SNUtil.writeToChat("C2C: Checking water temp @ "+calc.position+" def="+calc.originalValue);
			if (Vector3.Distance(calc.position, mountainBaseGeoCenter) <= 20) {
				calc.setValue(Mathf.Min(calc.getTemperature(), 45));
			}
			else {
				float bdist = Vector3.Distance(calc.position, bkelpBaseNuclearReactor);
				if (bdist <= 12)
					calc.setValue(Mathf.Max(calc.getTemperature(), 90 - (bdist * 6F)));
			}
			string biome = EnvironmentalDamageSystem.instance.getBiome(calc.position);
			float poison = EnvironmentalDamageSystem.instance.getLRPoison(biome);
			if (poison > 0) { //make LR cold, down to -10C (4C is max water density point, but not for saltwater), except around vents
				float temp = calc.getTemperature();
				float cooling = poison * Mathf.Max(0, 3F - (Mathf.Max(0, temp - 30) / 10F));
				calc.setValue(Mathf.Max(-10, temp - cooling));
			}
			else if (VanillaBiomes.COVE.isInBiome(calc.position))
				calc.setValue(calc.getTemperature() - 10);
			if (biome == null || (biome.ToLowerInvariant().Contains("void") && calc.position.y <= -50))
				calc.setValue(Mathf.Max(4, calc.getTemperature() + ((calc.position.y + 50) / 20F))); //drop 1C per 20m below 50m, down to 4C around 550m
			double dist = VoidSpikesBiome.instance.getDistanceToBiome(calc.position, true);
			if (dist <= 500)
				calc.setValue((float)MathUtil.linterpolate(dist, 200, 500, VoidSpikesBiome.waterTemperature, calc.getTemperature(), true));
			if (VoidSpikesBiome.instance.isInBiome(calc.position)) {
				calc.setValue(VoidSpikesBiome.waterTemperature);
			}
			dist = UnderwaterIslandsFloorBiome.instance.getDistanceToBiome(calc.position);
			if (dist <= 150)
				calc.setValue((float)MathUtil.linterpolate(dist, 0, 150, UnderwaterIslandsFloorBiome.waterTemperature, calc.getTemperature(), true));
			if (UnderwaterIslandsFloorBiome.instance.isInBiome(calc.position))
				calc.setValue(calc.getTemperature() + UnderwaterIslandsFloorBiome.instance.getTemperatureBoost(calc.getTemperature(), calc.position));
			calc.setValue(Mathf.Max(calc.getTemperature(), EnvironmentalDamageSystem.instance.getWaterTemperature(calc.position)));
			EjectedHeatSink.iterateHeatSinks(h => {
				if (h) {
					dist = Vector3.Distance(h.transform.position, calc.position);
					if (dist <= EjectedHeatSink.HEAT_RADIUS) {
						float f = 1F - (float)(dist / EjectedHeatSink.HEAT_RADIUS);
						//SNUtil.writeToChat("Found heat sink "+lb.transform.position+" at dist "+dist+" > "+f+" > "+(f*lb.getTemperature()));
						calc.setValue(Mathf.Max(calc.getTemperature(), f * h.getTemperature()));
					}
				}
			});/* Too expensive
	    	Geyser g = WorldUtil.getClosest<Geyser>(calc.position);
	    	if (g && g.erupting && calc.position.y > g.transform.position.y) {
	    		calc.setValue(Mathf.Max(calc.getTemperature(), 800-10*Vector3.Distance(g.transform.position, calc.position)));
	    	}
	    	calc.setValue(C2CMoth.getOverrideTemperature(calc.getTemperature()));*/
		}

		public static void onPrecursorDoorSpawn(PrecursorKeyTerminal pk) {
			try {
				Transform parent = pk.transform.parent;
				PrefabIdentifier pi = parent == null ? null : parent.GetComponent<PrefabIdentifier>();
				if (!pi) {/*
		    		if (Vector3.Distance(pi.transform.position, gunPoolBarrierTerminal) < 0.5F) {
		    			pk.acceptKeyType = PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Orange;
		    			HashSet<GameObject> barrier = WorldUtil.getObjectsNearMatching(pk.transform.position, 90, isGunBarrier);
		    			if (barrier.Count == 0) {
		    			pk.transform.parent = barrier.First().transform;
		    		}*/
					return;
				}
				switch (pi.classId) {
					case "0524596f-7f14-4bc2-a784-621fdb23971f":
					case "47027cf0-dca8-4040-94bd-7e20ae1ca086":
						new ChangePrecursorDoor(PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_White).applyToObject(pk);
						break;
					case "fdb2bcbb-288a-40b6-bd7a-5585445eb43f":
						if (parent.position.y > -100) {
							new ChangePrecursorDoor(PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Purple).applyToObject(pk);
							//does not exist parent.GetComponent<PrecursorGlobalKeyActivator>().doorActivationKey = "GunGateDoor";
						}
						else if (Math.Abs(parent.position.y + 803.8) < 0.25) {
							new ChangePrecursorDoor(PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Red).applyToObject(pk);
							//parent.GetComponent<PrecursorGlobalKeyActivator>().doorActivationKey = "DRFGateDoor";
						}
						else if (Math.Abs(parent.position.y + 803.8) < 15) { //the original
							new ChangePrecursorDoor(PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Orange).applyToObject(pk);
						}
						break;/*
		    		case "d26276ab-0c29-4642-bcb8-1a5f8ee42cb2":
		    			break;*/
				}
			}
			catch (Exception e) {
				SNUtil.log("Caught exception processing precursor door " + pk.gameObject.GetFullHierarchyPath() + " @ " + pk.transform.position + ": " + e.ToString());
			}
		}
		/*
	    private static bool isGunBarrier(GameObject go) {
	    	PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
	    	return pi && pi.ClassId == "d26276ab-0c29-4642-bcb8-1a5f8ee42cb2" && Vector3.Distance(pi.transform.position, gunPoolBarrier) < 0.5F;
	    }
	    */
		public static void OnInspectableSpawn(InspectOnFirstPickup pk) {/*
	    	PrefabIdentifier pi = pk.gameObject.GetComponentInParent<PrefabIdentifier>();
	    	if (pi != null && (pi.ClassId == "7d19f47b-6ec6-4a25-9b28-b3fd7f5661b7" || pi.ClassId == "066e533d-f854-435d-82c6-b28ba59858e0")) {
	    		VFXFabricating fab = pi.gameObject.transform.Find("Model").gameObject.EnsureComponent<VFXFabricating>();
	    		fab.localMaxY = 0.1F;
	    		fab.localMinY = -0.1F;
	    	}*/
		}

		public static GameObject getCrafterGhostModel(GameObject ret, TechType tech) {
			SNUtil.log("Crafterghost for " + tech + ": " + ret);
			if (tech == TechType.PrecursorKey_Red || tech == TechType.PrecursorKey_White) {
				ret = ObjectUtil.lookupPrefab(CraftData.GetClassIdForTechType(tech));
				ret = ret.clone();
				ret = ret.getChildObject("Model");
				VFXFabricating fab = ret.EnsureComponent<VFXFabricating>();
				fab.localMaxY = 0.1F;
				fab.localMinY = -0.1F;
				fab.enabled = true;
				fab.gameObject.SetActive(true);
			}
			return ret;
		}

		public static void onSpawnLifepod(EscapePod pod) {
			pod.gameObject.EnsureComponent<C2CLifepod>();
			pod.gameObject.EnsureComponent<Magnetic>();
		}

		public static void onSkyApplierSpawn(SkyApplier pk) {
			if (skipSkyApplierSpawn)
				return;
			GameObject go = pk.gameObject;
			if (go.name.StartsWith("Seamoth", StringComparison.InvariantCultureIgnoreCase) && go.name.EndsWith("Arm(Clone)", StringComparison.InvariantCultureIgnoreCase))
				return;
			//if (DIHooks.isWorldLoaded())
			//	LifeformScanningSystem.instance.onObjectCreated(go);
			if (go.name.StartsWith("ExplorableWreck", StringComparison.InvariantCultureIgnoreCase)) {
				go.EnsureComponent<ImmuneToPropulsioncannon>(); //also implements IObstacle to prevent building
			}
			PrefabIdentifier pi = go.FindAncestor<PrefabIdentifier>();
			if (SNUtil.match(pi, "d79ab37f-23b6-42b9-958c-9a1f4fc64cfd") && Vector3.Distance(fcsWreckOpenableDoor, go.transform.position) <= 0.5) {
				new WreckDoorSwaps.DoorSwap(go.transform.position, "Handle").applyTo(go);
			}
			else if (SNUtil.match(pi, "055b3160-f57b-46ba-80f5-b708d0c8180e") && Vector3.Distance(fcsWreckBlockedDoor, go.transform.position) <= 0.5) {
				new WreckDoorSwaps.DoorSwap(go.transform.position, "Blocked").applyTo(go);
			}
			else if (SNUtil.match(pi, VanillaCreatures.SEA_TREADER.prefab)) {
				//go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
				go.EnsureComponent<C2CTreader>();
			}
			else if (SNUtil.match(pi, VanillaCreatures.CAVECRAWLER.prefab)) {
				go.EnsureComponent<C2Crawler>();
			}
			else if (SNUtil.match(pi, VanillaCreatures.REAPER.prefab)) {
				go.EnsureComponent<C2CReaper>();
			}
			else if (DEIntegrationSystem.instance.isLoaded() && !go.GetComponent<WaterParkCreature>() && SNUtil.match(go, DEIntegrationSystem.instance.getThalassacean(), DEIntegrationSystem.instance.getLRThalassacean())) {
				go.EnsureComponent<DEIntegrationSystem.C2CThalassacean>();
			}
			else if (SNUtil.match(pi, "61ac1241-e990-4646-a618-bddb6960325b")) {
				if (Vector3.Distance(go.transform.position, Player.main.transform.position) <= 80 && go.transform.position.y < -200) {
					PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.TreaderPooPrompt).key);
				}
			}/*
			else if (SNUtil.match(pi, "e1d8b721-0edb-466e-93d3-074dc90d57f2")) {
	    		if (go.transform.position.y < -1200) {
	    			go.EnsureComponent<TechTag>().type = SeaToSeaMod.prisonPipeRoomTank;
		    	}
	    	}*/
			/*
			else if (SNUtil.match(pi, "407e40cf-69f2-4412-8ab6-45faac5c4ea2")) {
	    		
	    	}*//*
	    	else if (SNUtil.match(pi, "SeaVoyager")) {
	    		go.EnsureComponent<C2CVoyager>();
	    	}*/
			else if (SNUtil.match(pi, "172d9440-2670-45a3-93c7-104fee6da6bc")) {
				if (Vector3.Distance(go.transform.position, lostRiverCachePanel) < 2) {
					Renderer r = go.getChildObject("Precursor_Lab_infoframe/Precursor_Lab_infoframe_glass").GetComponent<Renderer>();
					r.materials[0].SetColor("_Color", new Color(1, 1, 1, /*0.43F*/0.24F));
					r.materials[0].SetColor("_SpecColor", new Color(0.38F, 1, 0.52F, 1));
					RenderUtil.setGlossiness(r.materials[0], 50, 0, 0);
					GameObject copy = r.gameObject.clone();
					copy.transform.SetParent(r.transform.parent);
					copy.transform.position = r.transform.position;
					copy.transform.rotation = r.transform.rotation;
					copy.transform.localScale = r.transform.localScale;
					Renderer r2 = copy.GetComponent<Renderer>();
					r2.materials[0].shader = Shader.Find("UWE/Marmoset/IonCrystal");
					r2.materials[0].SetInt("_ZWrite", 1);
					r2.materials[0].SetColor("_DetailsColor", Color.white);
					r2.materials[0].SetColor("_SquaresColor", new Color(1, 4, 1.5F, 2));
					r2.materials[0].SetFloat("_SquaresTile", 200F);
					r2.materials[0].SetFloat("_SquaresSpeed", 12F);
					r2.materials[0].SetFloat("_SquaresIntensityPow", 20F);
					r2.materials[0].SetVector("_NoiseSpeed", new Vector4(1, 1, 1, 1));
					r2.materials[0].SetVector("_FakeSSSparams", new Vector4(1, 15, 1, 1));
					r2.materials[0].SetVector("_FakeSSSSpeed", new Vector4(1, 1, 1, 1));
					RenderUtil.setGlossiness(r2.materials[0], 0, 0, 0);
					r.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -709.79F);
					r2.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -709.80F);
					GenericHandTarget ht = go.EnsureComponent<GenericHandTarget>();
					ht.onHandHover = new HandTargetEvent();
					ht.onHandClick = new HandTargetEvent();
					ht.onHandHover.AddListener(hte => {
						if (!KnownTech.knownTech.Contains(C2CItems.treatment.TechType)) {
							HandReticle.main.targetDistance = 15;
							HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
							HandReticle.main.SetInteractText("LostRiverCachePanel");
						}
					});
					ht.onHandClick.AddListener(hte => {
						if (!KnownTech.knownTech.Contains(C2CItems.treatment.TechType)) {
							KnownTech.Add(C2CItems.treatment.TechType);
							SNUtil.triggerTechPopup(C2CItems.treatment.TechType);
						}
					});
				}
			}/*
	    	else if (SNUtil.match(pi, VanillaCreatures.GHOST_LEVIATHAN && pi.GetComponentInChildren<GhostLeviatanVoid>()) {
	    		***
	    	}*/
			else if (pi && auroraFires.Contains(pi.ClassId) && EnvironmentalDamageSystem.instance.isPositionInAuroraPrawnBay(pi.transform.position)) {
				blueAuroraPrawnFire(go);
			}
			else if (SNUtil.match(pi, "b86d345e-0517-4f6e-bea4-2c5b40f623b4") && pi.transform.parent && pi.transform.parent.name.Contains("ExoRoom_Weldable")) {
				GameObject inner = go.getChildObject("Starship_doors_manual_01/Starship_doors_automatic");
				StarshipDoorLocked d = go.transform.parent.GetComponentInChildren<StarshipDoorLocked>();
				Renderer r = inner.GetComponentInChildren<Renderer>();
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/", new Dictionary<int, string>() { {
						0,
						"FireDoor"
					}, {
						1,
						"FireDoor"
					}
				});
				d.lockedTexture = (Texture2D)r.materials[0].GetTexture(Shader.PropertyToID("_Illum")); //replace all since replaced the base texture too
				d.unlockedTexture = TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/FireDoor2_Illum");
				//WeldableWallPanelGeneric panel = go.transform.parent.GetComponentInChildren<WeldableWallPanelGeneric>();
				PrawnBayDoorTriggers pt = /*panel.sendMessageFrom*/go.transform.parent.gameObject.EnsureComponent<PrawnBayDoorTriggers>();
				pt.door = d.GetComponent<StarshipDoor>();
				GenericHandTarget ht = inner.EnsureComponent<GenericHandTarget>();
				pt.hoverHint = ht;
				ht.onHandHover = new HandTargetEvent();
				ht.onHandHover.AddListener(hte => {
					HandReticle.main.SetIcon(HandReticle.IconType.Info, 1f);
					HandReticle.main.SetInteractText("PrawnBayDoorHeatWarn");
					HandReticle.main.SetTargetDistance(8);
				});
				Vector3 p1 = new Vector3(991.1F, 1F, -3.2F);
				Vector3 p2 = new Vector3(991.7F, 1F, -2.8F);/*
				GameObject rippleHolder = new GameObject("ripples");
				rippleHolder.transform.parent = go.transform.parent;
				rippleHolder.transform.localPosition = Vector3.zero;
				GameObject vent = ObjectUtil.lookupPrefab("5bbd405c-ca10-4da8-832b-87558c42f4dc");
				GameObject bubble = vent.getChildObject("xThermalVent_Dark_Big/xBubbles");
				int n = 5;
				for (int i = 0; i <= n; i++) {
					GameObject p = bubble.clone();
					p.transform.parent = rippleHolder.transform;
					p.transform.position = Vector3.Lerp(p1, p2, i/(float)n);
					p.GetComponentInChildren<Renderer>().materials[0].color = new Color(-8, -8, -8, 0.3F);
				}*/
				GameObject fire = ObjectUtil.createWorldObject("3877d31d-37a5-4c94-8eef-881a500c58bc");
				fire.transform.parent = go.transform;
				fire.transform.position = Vector3.Lerp(p1, p2, 0.5F) + new Vector3(1.3F, -0.05F, -1.7F);
				fire.transform.localScale = new Vector3(1.8F, 1, 1.8F);
				blueAuroraPrawnFire(fire);
				//fire.removeComponent<VFXExtinguishableFire>();
				LiveMixin lv = fire.GetComponent<LiveMixin>();
				lv.invincible = true;
				lv.data.maxHealth = 40000;
				lv.health = lv.data.maxHealth;
			}
			else if (pi && SeaToSeaMod.lrCoralClusters.Contains(pi.ClassId)) {
				string name = go.name;
				go.EnsureComponent<TechTag>().type = C2CItems.brineCoral;
				//if (!hard) require azurite battery if not set from 1600 to 750
				//	go.EnsureComponent<Rigidbody>().mass = 750;
				GameObject pfb = ObjectUtil.lookupPrefab(VanillaResources.LARGE_QUARTZ.prefab);
				go.makeMapRoomScannable(C2CItems.brineCoral);
				Drillable d = go.EnsureComponent<Drillable>();
				d.copyObject(pfb.GetComponent<Drillable>());
				go.name = name;
				/*
	    		d.deleteWhenDrilled = true;
	    		d.kChanceToSpawnResources = 1;
	    		d.lootPinataOnSpawn = true;
	    		d.minResourcesToSpawn = 1;
	    		d.maxResourcesToSpawn = 2;*/
				d.resources = new Drillable.ResourceType[] { new Drillable.ResourceType() {
						techType = C2CItems.brineCoralPiece.TechType,
						chance = 1
					}
				};
				bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
				d.kChanceToSpawnResources = 1;
				d.minResourcesToSpawn = hard ? 1 : 2;
				d.maxResourcesToSpawn = hard ? 3 : 4;
				go.EnsureComponent<BrineCoralTag>();
				d.onDrilled += (dr) => {
					dr.GetComponent<BrineCoralTag>().onDrilled();
				};
			}
			else if (SNUtil.match(pi, "58247109-68b9-411f-b90f-63461df9753a") && Vector3.Distance(deepDegasiTablet, go.transform.position) <= 0.2) {
				GameObject go2 = ObjectUtil.createWorldObject(C2CItems.brokenOrangeTablet.ClassID);
				go2.transform.position = go.transform.position;
				go2.transform.rotation = go.transform.rotation;
				go.destroy(false);
			}
			else if (pi && (pi.ClassId == "92fb421e-a3f6-4b0b-8542-fd4faee4202a" || pi.classId == "53ffa3e8-f2f7-43b8-a5c7-946e766aff64")) {
				for (int i = 0; i < purpleTabletsToBreak.Count; i++) {
					Vector3 pos = purpleTabletsToBreak[i];
					if (Vector3.Distance(pos, go.transform.position) <= 0.2) {
						GameObject go2 = ObjectUtil.createWorldObject(i == 2 ? SeaToSeaMod.purpleTabletPartA.ClassID : "83b61f89-1456-4ff5-815a-ecdc9b6cc9e4");
						go2.transform.position = go.transform.position;
						go2.transform.rotation = go.transform.rotation;
						go.destroy(false);
					}
				}
			}
			else if (SNUtil.match(pi, "83b61f89-1456-4ff5-815a-ecdc9b6cc9e4")) { //broken purple tablet
				GameObject light = ObjectUtil.lookupPrefab("53ffa3e8-f2f7-43b8-a5c7-946e766aff64").GetComponentInChildren<Light>().gameObject;
				Vector3 rel = light.transform.localPosition;
				light = light.clone();
				light.transform.parent = pi.transform;
				light.transform.localPosition = rel;
				Light l = light.GetComponent<Light>();
				l.intensity *= 0.67F;
				//l.intensity = 0.4F;
				//l.range = 18F;
				//l.color = new Color(0.573, 0.404, 1.000, 1.000);
				l.shadows = LightShadows.Soft;
				FlickeringLight f = light.EnsureComponent<FlickeringLight>();
				f.dutyCycle = 0.5F;
				f.updateRate = 0.3F;
				f.fadeRate = 5F;
				go.EnsureComponent<TabletFragmentTag>();
				/*
				GameObject models = pi.gameObject.getChildObject("precursor_key_cracked_01");
				MeshRenderer[] parts = models.GetComponentsInChildren<MeshRenderer>();
				SNUtil.log("Checking fragmentation of purple tablet @ "+go.transform.position+": "+parts.Length+" in map "+purpleTabletsToRemoveParts.toDebugString());
				if (parts.Length == 2) {
					foreach (KeyValuePair<Vector3, bool[]> kvp in purpleTabletsToRemoveParts) {
		    			if (Vector3.Distance(kvp.Key, go.transform.position) <= 0.2) {
							SNUtil.log("Found match "+kvp.Value.toDebugString());
							for (int i = 0; i < parts.Length; i++) {
								parts[i].gameObject.SetActive(i >= kvp.Value.Length || kvp.Value[i]);
							}
							break;
		    			}
					}
				}*/
			}
			else if (SNUtil.match(pi, "1c34945a-656d-4f70-bf86-8bc101a27eee")) {
				go.EnsureComponent<C2CMoth>();
				go.EnsureComponent<BrightLightController>().setLightValues(120, 1.75F, 135, 180, 2.5F).setPowerValues(0.15F, 0.5F);
				go.EnsureComponent<SeamothTetherController>();
				//go.EnsureComponent<VoidSpikeLeviathanSystem.SeamothStealthManager>();
			}
			else if (SNUtil.match(pi, "ba3fb98d-e408-47eb-aa6c-12e14516446b")) { //prawn
				TemperatureDamage td = go.EnsureComponent<TemperatureDamage>();
				td.minDamageTemperature = 350;
				td.baseDamagePerSecond = Mathf.Max(10, td.baseDamagePerSecond) * 0.33F;
				td.onlyLavaDamage = false;
				td.InvokeRepeating("UpdateDamage", 1f, 1f);
				//go.removeComponent<ImmuneToPropulsioncannon>();
				go.EnsureComponent<BrightLightController>().setLightValues(120, 1.6F, 120, 150, 2.25F).setPowerValues(0.25F, 0.67F);
			}
			else if (SNUtil.match(pi, "8b113c46-c273-4112-b7ef-65c50d2591ed")) { //rocket
				go.EnsureComponent<C2CRocket>();
			}
			else if (SNUtil.match(pi, "d4be3a5d-67c3-4345-af25-7663da2d2898")) { //cuddlefish
				Pickupable p = go.EnsureComponent<Pickupable>();
				p.isPickupable = true;
				p.overrideTechType = TechType.Cutefish;
			}
			/*
	    	else if (SNUtil.match(pi, auroraStorageModule.prefabName && Vector3.Distance(auroraStorageModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraCyclopsModule.position;
	    		go.transform.rotation = auroraCyclopsModule.rotation;
	    	}
	    	else if (SNUtil.match(pi, auroraCyclopsModule.prefabName && Vector3.Distance(auroraCyclopsModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraStorageModule.position;
	    		go.transform.rotation = auroraStorageModule.rotation;
	    	}*/
			else if (SNUtil.match(pi, auroraDepthModule.prefabName) && Vector3.Distance(auroraDepthModule.position, go.transform.position) <= 0.2) {
				GameObject go2 = ObjectUtil.createWorldObject(SeaToSeaMod.brokenAuroraDepthModule.ClassID);
				go2.transform.position = go.transform.position;
				go2.transform.rotation = go.transform.rotation;
				go.destroy(false);
			}
			else if (SNUtil.match(pi, "bc9354f8-2377-411b-be1f-01ea1914ec49") && Vector3.Distance(auroraRepulsionGunTerminal, go.transform.position) <= 0.2) {
				pi.GetComponent<StoryHandTarget>().goal = SeaToSeaMod.auroraTerminal;
			}
			else if (pi && pi.GetComponent<BlueprintHandTarget>()) {
				DamagedDataboxSystem.instance.onDataboxSpawn(go);
				go.EnsureComponent<ImmuneToPropulsioncannon>();
			}
			else if (pi && (pi.ClassId == VanillaResources.MAGNETITE.prefab || pi.ClassId == VanillaResources.LARGE_MAGNETITE.prefab)) {
				go.EnsureComponent<Magnetic>();
			}
			else if (SNUtil.match(pi, "160e99a7-cb46-409d-98e2-360a76ff92da")) {
				go.EnsureComponent<C2CStasisRifle>();
			}

			SubRoot sub = (bool)pi ? pi.GetComponent<SubRoot>() : go.GetComponent<SubRoot>();
			if (sub) {
				go.EnsureComponent<Magnetic>();
				if (sub.isCyclops)
					go.EnsureComponent<BrightLightController>().setLightValues(0, 0, 135, 200, 2.0F).setPowerValues(0, /*0.4F1.6F*/0.6F);
			}
			if (go.GetComponent<BaseCell>() || go.GetComponent<Constructable>() || go.FindAncestor<Vehicle>()) {
				go.EnsureComponent<Magnetic>();
			}
			if (go.GetComponent<MeleeAttack>()) {
				go.EnsureComponent<AttackRelay>();
			}
			if (pi && !floaterRocks.Contains(pi.ClassId) && CraftData.GetTechType(go) != TechType.FloatingStone && go.GetComponent<Drillable>()) {
				Rigidbody rb = go.FindAncestor<Rigidbody>();
				if (rb)
					rb.mass = Mathf.Max(2400, rb.mass);
			}
			if (pi)
				KeypadCodeSwappingSystem.instance.handleDoor(pi);

			WeldableWallPanelGeneric panel = go.GetComponent<WeldableWallPanelGeneric>();
			if (panel && panel.liveMixin)
				panel.liveMixin.data.canResurrect = true;
		}

		public static void onFireSpawn(VFXExtinguishableFire fire) {/*
	    	SNUtil.log("Spawned fire "+fire+" @ "+fire.transform.position);
	    	PrefabIdentifier pi = fire.gameObject.FindAncestor<PrefabIdentifier>();
	    	SNUtil.log("pi: "+(pi ? pi.classId : "null"));
	    	if (pi && auroraFires.Contains(pi.ClassId)) {
	    		blueAuroraPrawnFire(pi.gameObject);
	    	}*/
			fire.gameObject.EnsureComponent<AuroraFireChecker>();
		}

		private static void blueAuroraPrawnFire(GameObject fire) {
			fire.EnsureComponent<AuroraFireBluer>();
		}

		private class AuroraFireChecker : MonoBehaviour {

			void Update() {
				PrefabIdentifier pi = gameObject.FindAncestor<PrefabIdentifier>();
				if (pi) {
					if (auroraFires.Contains(pi.ClassId) && EnvironmentalDamageSystem.instance.isPositionInAuroraPrawnBay(pi.transform.position)) {
						pi.gameObject.EnsureComponent<AuroraFireBluer>();
					}
					this.destroy(false);
				}
			}

		}

		private class AuroraFireBluer : MonoBehaviour {

			private float age;

			void Update() {
				age += Time.deltaTime;
				bool flag = false;
				//SNUtil.log("Trying to blue prawn bay fire "+gameObject.name+" @ "+transform.position);
				foreach (Renderer r in this.GetComponentsInChildren<Renderer>(true)) {
					if (!r || r.name == null || r.materials == null || r.materials.Length == 0)
						continue;
					//SNUtil.log("Checking renderer "+r.name+" in "+r.gameObject.GetFullHierarchyPath());
					if (auroraPrawnFireColors.ContainsKey(r.name)) {
						foreach (Material m in r.materials) {
							//SNUtil.log("Setting color to "+auroraPrawnFireColors[r.name]);
							if (!m)
								continue;
							m.color = auroraPrawnFireColors[r.name];
							flag = true;
						}
					}
				}
				Light l = this.GetComponentInChildren<Light>();
				if (l)
					l.color = new Color(0.55F, 0.67F, 1F);
				if (flag && age >= 0.5F) {
					Light l2 = gameObject.addLight(0.4F, 32F, l.color).setName("BlueFireLight");
					//SNUtil.log("Bluing complete. Destroying component.");
					this.destroy(false);
				}
			}

		}

		public static void onStartWaterFilter(FiltrationMachine fm) {
			fm.storageContainer.Resize(2, 3); //add another row for byproducts
			fm.gameObject.EnsureComponent<C2CWaterFilter>().machine = fm;
		}

		class C2CWaterFilter : MonoBehaviour {

			internal FiltrationMachine machine;

			private float lastBiomeCheck;

			private BiomeBase biome;

			void Update() {
				if (!machine)
					machine = this.GetComponent<FiltrationMachine>();
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time - lastBiomeCheck >= 30) {
					biome = BiomeBase.getBiome(machine.transform.position);
					lastBiomeCheck = time;
				}
				if (biome == VanillaBiomes.LOSTRIVER || biome == VanillaBiomes.COVE) {
					machine.timeRemainingSalt -= Time.deltaTime * 1.5F; //2.5x salt rate in lost river
					if (machine.timeRemainingSalt <= 0 && machine.storageContainer.container.GetCount(TechType.Salt) < machine.maxSalt) { //need to recheck
						machine.timeRemainingSalt = -1f;
						machine.Spawn(machine.saltPrefab);
						machine.TryFilterSalt();
					}
				}
			}

		}

		/*
	    
	    public static void onPingAdd(uGUI_PingEntry e, PingType type, string name, string text) {
	    	SNUtil.log("Ping ID type "+type+" = "+name+"|"+text+" > "+e.label.text);
	    }*/

		class AttackRelay : MonoBehaviour {

			void OnMeleeAttack(GameObject target) {
				if (target == Player.main.gameObject)
					LifeformScanningSystem.instance.onObjectSeen(gameObject, false);
			}

		}

		public static void tickFruitPlant(DIHooks.FruitPlantTag fpt) {
			if (skipFruitPlantTick)
				return;
			FruitPlant fp = fpt.getPlant();
			if (fp && fp.gameObject.isFarmedPlant() && WorldUtil.isPlantInNativeBiome(fp.gameObject)) {
				fp.fruitSpawnInterval = fpt.getBaseGrowthTime() / 1.5F;
			}
		}

		class PrawnBayDoorTriggers : MonoBehaviour {

			internal GenericHandTarget hoverHint;

			internal StarshipDoor door;

			private bool wasOpen;

			public void UnlockDoor() {
				if (hoverHint)
					hoverHint.destroy();
			}

			private void Update() {
				if (door && door.doorOpen && !wasOpen) {
					wasOpen = true;
					EnvironmentalDamageSystem.instance.triggerAuroraPrawnBayWarning();
					Player.main.liveMixin.TakeDamage(5, Player.main.transform.position, DamageType.Heat, gameObject);
				}
			}

		}

		public static void updateSeamothModules(SeaMoth sm, int slotID, TechType tt, bool added) {
			sm.gameObject.EnsureComponent<C2CMoth>().recalculateModules();
			sm.gameObject.EnsureComponent<BrightLightController>().recalculateModule();
			sm.gameObject.EnsureComponent<SeamothTetherController>().recalculateModule();
			if (added && GameModeUtils.currentEffectiveMode != GameModeOption.Creative && !SNUtil.canUseDebug())
				ItemUnlockLegitimacySystem.instance.validateModule(sm, slotID, tt);
		}

		public static void updateCyclopsModules(SubRoot sm) {
			if (C2CIntegration.seaVoyager != TechType.None && sm.GetType() == C2CIntegration.seaVoyagerComponent) { //this is the load hook as it has no SkyAppliers
				sm.gameObject.EnsureComponent<C2CVoyager>();
				return;
			}
			sm.gameObject.EnsureComponent<BrightLightController>().recalculateModule();
			C2CUtil.resizeCyclopsStorage(sm);
			if (GameModeUtils.currentEffectiveMode != GameModeOption.Creative && !SNUtil.canUseDebug())
				ItemUnlockLegitimacySystem.instance.validateModules(sm);
		}

		public static void updatePrawnModules(Exosuit sm, int slotID, TechType tt, bool added) {
			sm.gameObject.EnsureComponent<BrightLightController>().recalculateModule();
			if (added && GameModeUtils.currentEffectiveMode != GameModeOption.Creative && !SNUtil.canUseDebug())
				ItemUnlockLegitimacySystem.instance.validateModule(sm, slotID, tt);
		}

		public static void useSeamothModule(SeaMoth sm, TechType tt, int slotID) {

		}

		public static float getVehicleTemperature(Vehicle v) {
			return C2CMoth.getOverrideTemperature(v, WaterTemperatureSimulation.main.GetTemperature(v.transform.position));
		}

		public static bool isSpawnableVoid(string biome) {
			bool ret = VoidSpikeLeviathanSystem.instance.isSpawnableVoid(biome);
			if (ret && Player.main.IsSwimming() && !Player.main.GetVehicle() && VoidGhostLeviathansSpawner.main.spawnedCreatures.Count < 3 && !VoidSpikesBiome.instance.isInBiome(Player.main.transform.position)) {
				VoidGhostLeviathansSpawner.main.timeNextSpawn = Time.time - 1;
			}
			return ret;
		}

		public static GameObject getVoidLeviathan(VoidGhostLeviathansSpawner spawner, Vector3 pos) {
			return VoidSpikeLeviathanSystem.instance.getVoidLeviathan(spawner, pos);
		}

		public static void tickVoidLeviathan(GhostLeviatanVoid gv) {
			if (skipVoidLeviTick)
				return;
			VoidSpikeLeviathanSystem.instance.tickVoidLeviathan(gv);
		}

		public static void pingSeamothSonar(SeaMoth sm) {
			bool vv = VanillaBiomes.VOID.isInBiome(sm.transform.position);
			VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, vv ? 30 : 10);
			if (vv) {
				for (int i = VoidGhostLeviathansSpawner.main.spawnedCreatures.Count; i < VoidGhostLeviathansSpawner.main.maxSpawns; i++) {
					VoidGhostLeviathansSpawner.main.timeNextSpawn = 0.1F;
					VoidGhostLeviathansSpawner.main.UpdateSpawn(); //trigger spawn and time recalc
				}
			}
		}

		public static void onTorpedoFired(Bullet b, Vehicle v) {
			if (v is SeaMoth)
				VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(v as SeaMoth, SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 30 : 15);
		}

		public static void onTorpedoExploded(SeamothTorpedo p, Transform result) {
			Vehicle v = Player.main.GetVehicle();
			if (v is SeaMoth)
				VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(v as SeaMoth, SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 60 : 30);
		}

		public static void pingAnySonar(SNCameraRoot cam) {
			if (VoidSpikesBiome.instance.isInBiome(cam.transform.position)) {
				VoidSpikeLeviathanSystem.instance.triggerEMInterference();
			}
		}

		public static void pulseSeamothDefence(SeaMoth sm) {
			VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, 12);
		}

		public static void onBaseSonarPinged(GameObject go) {
			if (VoidSpikesBiome.instance.isInBiome(go.transform.position)) {
				Player ep = Player.main;
				Vehicle v = ep.GetVehicle();
				if (v && v is SeaMoth sm && VoidSpikesBiome.instance.isInBiome(ep.transform.position))
					VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, 40);
			}
		}

		public static void getGrinderDrillableDrop(DrillableGrindingResult res) {
			if (res.materialTech == TechType.Sulphur) {
				//SNUtil.writeToChat("Intercepting grinding sulfur");
				Story.StoryGoal.Execute("GrabSulfur", Story.GoalType.Story);
				res.drop = ObjectUtil.lookupPrefab(CraftingItems.getItem(CraftingItems.Items.SulfurAcid).ClassID);
				res.dropCount = UnityEngine.Random.Range(0F, 1F) < 0.33F ? 2 : 1;
			}
		}

		public static void onLavaBombHit(LavaBombTag bomb, GameObject hit) {
			if (hit) {
				C2CMoth cm = hit.FindAncestor<C2CMoth>();
				if (cm)
					cm.onHitByLavaBomb(bomb);
				if (hit.layer == Voxeland.GetTerrainLayerMask() || hit.layer == 30) { //for some reason this sometimes causes multiple (1-3!) to drop but that is actually a good thing
					GameObject bs = ObjectUtil.createWorldObject(CustomMaterials.getItem(CustomMaterials.Materials.OBSIDIAN).ClassID);
					bs.transform.position = bomb.transform.position;
					/*
		    		SinkingGroundChunk s = bs.EnsureComponent<SinkingGroundChunk>();
		    		s.modelTransform = bs.GetComponentInChildren<Renderer>().transform;
		    		s.sinkHeight = 1;
		    		s.sinkTime = 10;
		    		*/
					bs.applyGravity();
				}
			}
		}

		public static void onAnchorPodExplode(ExplodingAnchorPodDamage dmg) {
			if (VoidSpikesBiome.instance.isInBiome(dmg.toDamage.transform.position) && dmg.toDamage.gameObject.FindAncestor<Player>()) {
				dmg.damageAmount *= 0.67F;
			}
		}

		public static void onBloodKelpGrab(PredatoryBloodvine kelp, GameObject tgt) {
			MoraleSystem.instance.shiftMorale(tgt.isPlayer() ? -40 : -10);
		}

		public static void onVoidTongueGrab(VoidTongueTag tag, Rigidbody rb) {
			if (rb.isPlayer() || rb.GetComponent<Vehicle>() || rb.GetComponent<SubRoot>())
				MoraleSystem.instance.shiftMorale(-200);
			else if (rb.GetComponent<GhostLeviatanVoid>())
				MoraleSystem.instance.shiftMorale(-10);
		}

		public static void onVoidTongueRelease(VoidTongueTag tag, Rigidbody rb) {
			if (rb.isPlayer() || rb.GetComponent<Vehicle>() || rb.GetComponent<SubRoot>())
				MoraleSystem.instance.shiftMorale(50);
		}

		public static void onPlanktonActivated(PlanktonCloudTag cloud, GameObject hit) {
			SeaMoth sm = hit.GetComponent<SeaMoth>();
			if (sm) {
				bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
				float amt = UnityEngine.Random.Range(hard ? 15 : 8, hard ? 25 : 15);
				if (VanillaBiomes.VOID.isInBiome(sm.transform.position))
					VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, amt);
			}
		}

		public static void tickVoidBubbles(VoidBubbleSpawnerTick t) {
			double dist = VoidSpikesBiome.instance.getDistanceToBiome(t.player.transform.position, true) - VoidSpikesBiome.biomeVolumeRadius;
			float f = (float)MathUtil.linterpolate(dist, 50, 300, 0, 1, true);
			//SNUtil.writeToChat(dist.ToString("0.0")+" > "+f.ToString("0.0000"));
			t.spawnChance *= f;
		}

		public static void tickVoidBubble(VoidBubbleTag t) {
			double dist = VoidSpikesBiome.instance.getDistanceToBiome(Player.main.transform.position, true) - VoidSpikesBiome.biomeVolumeRadius;
			if (dist <= 120) {
				t.fade(dist <= 80 ? 2 : (dist <= 80 ? 5 : 10));
			}
		}

		public static ClipMapManager.Settings modifyWorldMeshSettings(ClipMapManager.Settings values) {
			ClipMapManager.LevelSettings baseline = values.levels[0];

			for (int i = 1; i < values.levels.Length - 2; i++) {
				ClipMapManager.LevelSettings lvl = values.levels[i];

				if (lvl.entities) {
					//lvl.downsamples = baseline.downsamples;
					lvl.colliders = true;
					//lvl.grass = true;
					//lvl.grassSettings = baseline.grassSettings;
				}
			}
			return values;
		}

		public static string getO2Tooltip(Oxygen ox) {
			return ox.GetComponent<Pickupable>().GetTechType() == C2CItems.liquidTank.TechType
				? ox.GetSecondsLeft() + "s fluid stored in supply tank"
				: LanguageCache.GetOxygenText(ox.GetSecondsLeft());
		}

		public static string getBatteryTooltip(Battery ox) {
			return ox.GetComponent<Pickupable>().GetTechType() == C2CItems.liquidTank.TechType
				? Mathf.RoundToInt(ox.charge) + "s fluid stored in primary tank"
				: Language.main.GetFormat<float, int, float>("BatteryCharge", ox.charge / ox.capacity, Mathf.RoundToInt(ox.charge), ox.capacity);
		}

		public static void onClickedVehicleUpgrades(VehicleUpgradeConsoleInput v) {
			if (v.docked || SeaToSeaMod.anywhereSeamothModuleCheatActive || GameModeUtils.currentEffectiveMode == GameModeOption.Creative)
				v.OpenPDA();
		}

		public static void onHoverVehicleUpgrades(VehicleUpgradeConsoleInput v) {
			HandReticle main = HandReticle.main;
			if (!v.docked && !SeaToSeaMod.anywhereSeamothModuleCheatActive && GameModeUtils.currentEffectiveMode != GameModeOption.Creative) {
				main.SetInteractText("DockToChangeVehicleUpgrades"); //locale key
				main.SetIcon(HandReticle.IconType.HandDeny, 1f);
			}
			else if (v.equipment != null) {
				main.SetInteractText(v.interactText);
				main.SetIcon(HandReticle.IconType.Hand, 1f);
			}
		}

		public static void tryKnife(DIHooks.KnifeAttempt k) {
			LifeformScanningSystem.instance.onObjectSeen(k.target.gameObject, false);
			TechType tt = CraftData.GetTechType(k.target.gameObject);
			if (tt == TechType.BlueAmoeba || tt == SeaToSeaMod.gelFountain.TechType) {
				k.allowKnife = true;
				return;
			}
			AlkaliPlantTag a = k.target.GetComponent<AlkaliPlantTag>();
			if (a) {
				k.allowKnife = a.isHarvestable();
				return;
			}
		}

		public static GameObject getStalkerShinyTarget(GameObject def, CollectShiny cc) {
			if (skipStalkerShiny)
				return def;
			if (cc.shinyTarget && cc.GetComponent<DeepStalkerTag>()) {
				bool hasPlat = cc.shinyTarget.GetComponent<PlatinumTag>();
				bool lookingAtPlat = def.GetComponent<PlatinumTag>();
				return hasPlat == lookingAtPlat ? def : hasPlat ? cc.shinyTarget : def;
			}
			return def;
		}

		public static void onShinyTargetIsCurrentlyHeldByStalker(CollectShiny cc) {
			if (skipStalkerShiny)
				return;
			if (cc.shinyTarget && cc.shinyTarget.GetComponent<PlatinumTag>()) {
				DeepStalkerTag ds = cc.GetComponent<DeepStalkerTag>();
				ds.tryStealFrom(cc.shinyTarget.GetComponentInParent<Stalker>());
			}
			else {
				cc.targetPickedUp = false;
				cc.shinyTarget = null;
			}
		}

		public static bool stalkerTryDropTooth(Stalker s) {
			return (!s.GetComponent<DeepStalkerTag>() || UnityEngine.Random.Range(0F, 1F) > 0.8) && (!s.GetComponent<WaterParkCreature>() || PDAScanner.complete.Contains(TechType.StalkerTooth)) && s.LoseTooth();
		}

		public static void tryEat(DIHooks.EatAttempt ea) {
			if (LiquidBreathingSystem.instance.hasLiquidBreathing())
				ea.allowEat = false;
		}

		public static void tryLaunchRocket(LaunchRocket r) {
			if (!r.IsRocketReady())
				return;
			if (LaunchRocket.launchStarted)
				return;
			if (!StoryGoalCustomEventHandler.main.gunDisabled && !r.forcedRocketReady) {
				r.gunNotDisabled.Play();
				return;
			}
			if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
				if (!C2CUtil.checkConditionAndShowPDAAndVoicelogIfNot(ExplorationTrackerPages.instance.isFullyComplete(false), ExplorationTrackerPages.INCOMPLETE_PDA, PDAMessages.Messages.NeedFinishExploreTrackerMessage)) {
					ExplorationTrackerPages.instance.showAllPages();
					return;
				}
			}
			if (!FinalLaunchAdditionalRequirementSystem.instance.checkIfScannedAllLifeforms()) {
				return;
			}
			if (!FinalLaunchAdditionalRequirementSystem.instance.checkIfCollectedAllEncyData()) {
				return;
			}
			if (!FinalLaunchAdditionalRequirementSystem.instance.checkIfFullyLoaded()) {
				return;
			}
			//if (!FinalLaunchAdditionalRequirementSystem.instance.checkIfVisitedAllBiomes()) {
			//	return;
			//}
			if (!C2CProgression.instance.isRequiredProgressionComplete()) {
				SNUtil.writeToChat("Missing progression, cannot launch");
				return;
			}
			FinalLaunchAdditionalRequirementSystem.instance.forceLaunch(r);
		}

		public static void onEMPHit(EMPBlast e, GameObject go) {
			VoidSpikeLeviathanSystem.instance.onObjectEMPHit(e, go);
		}
		/*
	    public static void interceptChosenFog(DIHooks.WaterFogValues fog) {
	    	double d = VoidSpikesBiome.instance.getDistanceToBiome(Camera.main.transform.position, true)-VoidSpikesBiome.biomeVolumeRadius;
	    	if (d <= 50 && d > 0) {
	    		float f = (float)(1-d/50F);
	    		fog.density = (float)MathUtil.linterpolate(f, 0, 1, fog.originalDensity, VoidSpikesBiome.fogDensity, true);
	    		fog.color = Color.Lerp(fog.originalColor, VoidSpikesBiome.waterColor, f);
	    		return;
	    	}
	    	d = UnderwaterIslandsFloorBiome.instance.getDistanceToBiome(Camera.main.transform.position);
	    	//SNUtil.writeToChat(d.ToString("0.000"));
	    	if (d <= 100 && d > 0) {
	    		float f = (float)(1-d/100F);
	    		fog.density = (float)MathUtil.linterpolate(f, 0, 1, fog.originalDensity, UnderwaterIslandsFloorBiome.fogDensity, true);
	    		fog.sunValue = (float)MathUtil.linterpolate(f, 0, 1, fog.originalSunValue, UnderwaterIslandsFloorBiome.sunIntensity, true);
	    		fog.color = Color.Lerp(fog.originalColor, UnderwaterIslandsFloorBiome.waterColor, f);
	    		return;
	    	}
	    }*/

		public static float getRadiationLevel(DIHooks.RadiationCheck ch) {
			//SNUtil.writeToChat(ch.originalValue+" @ "+VoidSpikesBiome.instance.getDistanceToBiome(ch.position));
			if (VoidSpikesBiome.instance.getDistanceToBiome(ch.position) <= VoidSpikesBiome.biomeVolumeRadius + 75)
				return 0;
			float dd = Vector3.Distance(ch.position, bkelpBaseGeoCenter);
			if (dd <= 80) {
				float ret = (float)MathUtil.linterpolate(dd, 60, 80, 0.25F, 0, true);
				if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
					ret -= 0.17F;
				//do not require, as need rebreather v2 if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
				//	ret -= 0.12F;
				if (Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0)
					ret -= 0.08F;
				if (ret > 0)
					return ret;
			}
			return ch.value;
		}

		public static float getSolarEfficiencyLevel(DIHooks.SolarEfficiencyCheck ch) {
			if (!SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
				return ch.value;
			float depth = Mathf.Max(0F, Ocean.main.GetDepthOf(ch.panel.gameObject));
			float effectiveDepth = depth;
			if (depth > 150)
				effectiveDepth = Mathf.Max(depth, 250);
			else if (depth > 100)
				effectiveDepth = (float)MathUtil.linterpolate(depth, 100, 150, 125, 250, true);
			else if (depth > 50)
				effectiveDepth = (float)MathUtil.linterpolate(depth, 50, 100, 50, 125, true);
			float f = Mathf.Clamp01((ch.panel.maxDepth - effectiveDepth) / ch.panel.maxDepth);
			//SNUtil.writeToChat(depth+" > "+effectiveDepth+" > "+f+" > "+ch.panel.depthCurve.Evaluate(f));
			return ch.panel.depthCurve.Evaluate(f) * ch.panel.GetSunScalar();
		}

		public static float getModuleFireCost(DIHooks.ModuleFireCostCheck ch) {
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
			if (hard)
				ch.value *= 1.5F;
			if (ch.module == TechType.SeamothSonarModule)
				ch.value *= hard ? 8 / 3F : 4 / 3F;
			return ch.value;
		}

		public static void fireSeamothDefence(SeaMoth sm) {
			VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, 10); //x1.5 on hard already
			sm.energyInterface.ConsumeEnergy(SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 5 : 3);
		}

		public static void generateItemTooltips(StringBuilder sb, TechType tt, GameObject go) {
			if (tt == TechType.LaserCutter && hasLaserCutterUpgrade()) {
				TooltipFactory.WriteDescription(sb, "\nCutting Temperature upgraded to allow cutting selected seabase structural elements");
			}
			else if (tt == C2CItems.emperorRootOil.TechType) {
				EmperorRootOil.EmperorRootOilTag tag = go.GetComponent<EmperorRootOil.EmperorRootOilTag>();
				if (tag && tag.pickupTime >= 0) {
					float age = DayNightCycle.main.timePassedAsFloat - tag.pickupTime;
					float pct = (float)MathUtil.linterpolate(age, 0, EmperorRootOil.LIFESPAN, 100, 0, true);
					TooltipFactory.WriteDescription(sb, "\n" + pct.ToString("0.0") + "% freshness remaining");
				}
			}
		}

		public static void interceptBulkheadLaserCutter(DIHooks.BulkheadLaserCutterHoverCheck ch) {
			if (!hasLaserCutterUpgrade())
				ch.refusalLocaleKey = "Need_laserCutterBulkhead_Chit";
		}

		public static bool hasLaserCutterUpgrade() {
			return StoryGoalManager.main.completedGoals.Contains(SeaToSeaMod.laserCutterBulkhead.goal.key);
		}

		public static void onKnifed(GameObject go) {
			TechType tt = CraftData.GetTechType(go);
			if (tt == TechType.BlueAmoeba)
				DIHooks.fireKnifeHarvest(go, new Dictionary<TechType, int> { {
						CraftingItems.getItem(CraftingItems.Items.AmoeboidSample).TechType,
						1
					}
				});
			else if (tt == SeaToSeaMod.gelFountain.TechType)
				go.GetComponent<GelFountainTag>().onKnifed();
		}

		public static void interceptItemHarvest(DIHooks.KnifeHarvest h) {
			if (h.drops.Count > 0) {
				if (h.objectType == C2CItems.kelp.TechType) {
					GlowKelpTag tag = h.hit.FindAncestor<GlowKelpTag>();
					h.drops[h.defaultDrop] = 2;
					float f = tag.isFarmed() ? 0 : 0.25F;
					TechType egg = CustomEgg.getEgg(C2CItems.purpleHolefish.TechType).TechType;
					f -= Inventory.main.GetPickupCount(egg) * 0.2F; //100% chance if 0, 80% chance if 1, down to 0% at >= 5
					/*
	    			WaterPark wp = tag.getACU();
	    			if (wp && wp.GetComponentInChildren<PurpleHolefishTag>())
	    				f = 0.06F;
	    				*/
					if (f > 0 && UnityEngine.Random.Range(0F, 1F) <= f)
						h.drops[egg] = 1;
				}
				if (h.hit.isFarmedPlant() && WorldUtil.isPlantInNativeBiome(h.hit)) {
					h.drops[h.defaultDrop] = h.drops[h.defaultDrop] * 2;
				}
			}
		}

		public static void onReaperGrab(ReaperLeviathan r, Vehicle v) {
			MoraleSystem.instance.shiftMorale(v == Player.main.GetVehicle() ? -40 : -20);
			if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) && KnownTech.Contains(TechType.BaseUpgradeConsole) && !KnownTech.Contains(TechType.SeamothElectricalDefense)) {
				KnownTech.Add(TechType.SeamothElectricalDefense);
				SNUtil.triggerTechPopup(TechType.SeamothElectricalDefense);
			}
		}

		public static void onCyclopsDamage(SubRoot r, DamageInfo di) {/*
	    	if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) && !KnownTech.Contains(TechType.CyclopsShieldModule)) {
				float healthFraction = r.live.GetHealthFraction();
				float num = (100f - r.damageManager.overshieldPercentage) / 100f;
	    		if (healthFraction < num) { //health below auto regen level
			       	KnownTech.Add(TechType.CyclopsShieldModule);
		    		SNUtil.triggerTechPopup(TechType.CyclopsShieldModule);
	    		}
		    }*/
		}

		public static bool chargerConsumeEnergy(IPowerInterface pi, float amt, out float consumed, Charger c) {
			if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) && (c is PowerCellCharger || c.GetType().Name.Contains("FCS")))
				amt *= 1.5F;
			return pi.ConsumeEnergy(amt, out consumed);
		}

		public static void tickScannerCamera(MapRoomCamera cam) {
			cam.gameObject.EnsureComponent<CameraLeviathanAttractor>();
			Vector3 campos = cam.transform.position;
			if (VoidSpikesBiome.instance.getDistanceToBiome(campos, true) < 200) {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time > nextCameraEMPTime) {
					float d = UnityEngine.Random.Range(96F, 150F);
					Vector3 pos = campos + (cam.transform.forward * d);
					pos = MathUtil.getRandomVectorAround(pos, 45);
					pos = campos + (pos - campos).setLength(d);
					VoidSpikeLeviathanSystem.instance.spawnEMPBlast(pos);
					nextCameraEMPTime = time + UnityEngine.Random.Range(1.2F, 2.5F);
				}
			}
			float temp = EnvironmentalDamageSystem.instance.getWaterTemperature(campos);
			if (temp >= 100) {
				float amt = 5 * (1 + ((temp - 100) / 100F));
				cam.liveMixin.TakeDamage(amt * Time.deltaTime, campos, DamageType.Heat);
			}
			if (!cam.dockingPoint) {
				float leak = EnvironmentalDamageSystem.instance.getLRPowerLeakage(cam.gameObject);
				if (leak >= 0) {
					cam.energyMixin.ConsumeEnergy(leak * Time.deltaTime * 0.5F);
				}
			}
		}

		public static float getCrushDamage(CrushDamage dmg) {
			float f = 1;
			if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
				float ratio = dmg.GetDepth() / dmg.crushDepth;
				if (ratio > 1) {
					f += Mathf.Pow(ratio, 4) - 1; //so at 1700 with a limit of 1300 it is ~3x as much damage; at 1200 with a 900 limit it is 3.2x, at 900 with 500 it is 10.5x
					ratio = (dmg.GetDepth() - 900) / 300F; //add another +33% per 100m over 900m
					if (ratio > 0)
						f += ratio;
				} //net result: 1700 @ 1300 = 5.6x, 1200 @ 900 = 2.8x, 900 @ 500 = 7x, 300 @ 200 = 3.3x
			}
			return dmg.damagePerCrush * f;
		}

		internal static void isItemMapRoomDetectable(ESHooks.ResourceScanCheck rt) {
			if (rt.resource.techType == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType) {
				rt.isDetectable = PDAScanner.complete.Contains(rt.resource.techType) || StoryGoalManager.main.completedGoals.Contains("Precursor_LavaCastle_Log2"); //mentions lava castle
			}
			else if (rt.resource.techType == CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType) {
				rt.isDetectable = PDAScanner.complete.Contains(rt.resource.techType) || PDAManager.getPage("sunbeamdebrishint").isUnlocked();
			}
			else if (scanToScannerRoom.Contains(rt.resource.techType)) {
				rt.isDetectable = PDAScanner.complete.Contains(rt.resource.techType);
			}
			else if (rt.resource.techType == SeaToSeaMod.mushroomBioFragment.TechType) {
				rt.isDetectable = SNUtil.getFragmentScanCount(rt.resource.techType) > SeaToSeaMod.mushroomBioFragment.getFragmentCount() - 2;
			}
			else if (rt.resource.techType == SeaToSeaMod.geyserCoral.TechType) {
				rt.isDetectable = SNUtil.getFragmentScanCount(rt.resource.techType) > SeaToSeaMod.geyserCoral.getFragmentCount() - 4;
			}
			if (rt.resource.GetComponent<Drillable>()) {
				rt.isDetectable = StoryGoalManager.main.completedGoals.Contains("OnConstructExosuit") || KnownTech.knownTech.Contains(AqueousEngineeringMod.grinderBlock.TechType);
			}
		}

		static void onVehicleEnter(Vehicle v, Player ep) {/*
			if (v is SeaMoth) {
				VoidSpikesBiome.instance.onSeamothEntered((SeaMoth)v, ep);
			}*/
		}

		public static void getCompassDepthLevel(DIHooks.DepthCompassCheck ch) {
			if (skipCompassCalc)
				return;
			if (VoidSpikeLeviathanSystem.instance.isVoidFlashActive(true)) {
				ch.value = VoidSpikeLeviathanSystem.instance.getRandomDepthForDisplay();
				ch.crushValue = 1000 - ch.value;
			}
		}

		public static bool onStasisFreeze(StasisSphere s, Rigidbody c) {
			PrefabIdentifier pi = c.GetComponent<PrefabIdentifier>();
			//SNUtil.writeToChat("Froze "+pi??pi.ClassId);
			if (pi && pi.ClassId == C2CItems.alkali.ClassID) {
				pi.GetComponentInChildren<AlkaliPlantTag>().OnFreeze(/*s.time*/);
				return true;
			}
			return false;
		}

		public static bool onStasisUnFreeze(StasisSphere s, Rigidbody c) {
			PrefabIdentifier pi = c.GetComponent<PrefabIdentifier>();
			//SNUtil.writeToChat("Unfroze "+pi??pi.ClassId);
			if (pi && pi.ClassId == C2CItems.alkali.ClassID) {
				pi.GetComponentInChildren<AlkaliPlantTag>().OnUnfreeze();
				return true;
			}
			return false;
		}

		public static float get3AxisSpeed(float orig, Vehicle v, Vector3 input) {
			if (orig <= 0 || input.magnitude < 0.01F)
				return orig;
			//vanilla is float d = Mathf.Abs(vector.x) * this.sidewardForce + Mathf.Max(0f, vector.z) * this.forwardForce + Mathf.Max(0f, -vector.z) * this.backwardForce + Mathf.Abs(vector.y * this.verticalForce);
			float netForward = (Mathf.Max(0, input.z) * v.forwardForce) + (Mathf.Max(0, -input.z) * v.backwardForce);
			float inputFracX = Mathf.Pow(Mathf.Abs(input.x / input.magnitude), 0.75F);
			float inputFracY = Mathf.Pow(Mathf.Abs(input.y / input.magnitude), 0.75F);
			float inputFracZ = Mathf.Pow(Mathf.Abs(input.z / input.magnitude), 0.75F);
			float origX = Mathf.Abs(input.x) * v.sidewardForce;
			float origY = Mathf.Abs(input.y * v.verticalForce);
			float ret = (netForward * inputFracZ) + (origX * inputFracX) + (origY * inputFracY); //multiply each component by its component of the input vector rather than a blind sum
																								 //SNUtil.writeToChat("Input vector "+input+" > speeds "+orig.ToString("00.0000")+" & "+ret.ToString("00.0000"));
			return ret;
		}

		//Not called anymore, because kick to main menu when die now
		public static void onPlayerRespawned(Survival s, Player ep, bool post) {
			if (post) {
				bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
				s.water = Mathf.Max(hard ? 5 : 15, waterToRestore);
				s.food = Mathf.Max(hard ? 5 : 15, foodToRestore);
				MoraleSystem.instance.reset();
			}
			else {
				waterToRestore = s.water;
				foodToRestore = s.food;
				EnvironmentalDamageSystem.instance.resetCooldowns();
			}
		}

		public static void onItemsLost() {
			/* no longer necessary because kick to main menu instead
	    	foreach (InventoryItem ii in ((IEnumerable<InventoryItem>)Inventory.main.container)) {
	    		if (ii != null && ii.item && ii.item.GetTechType() == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType) {
	    			ii.item.destroyOnDeath = true;
	    		}
	    	}*/
		}

		public static void onDeath() {
			//SNUtil.writeToChat("You died);
			//IngameMenu.main.QuitGame(true);
			playerDied = true;
			C2CUtil.setupDeathScreen();
		}

		public static void onSelfScan() {
			PDAMessages.Messages msg = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? PDAMessages.Messages.LiquidBreathingSelfScanHard : PDAMessages.Messages.LiquidBreathingSelfScanEasy;
			if (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(msg).key)) {
				PDAManager.getPage("liqbrefficiency").unlock();
			}
		}

		public static void onScanComplete(PDAScanner.EntryData data) {
			C2CProgression.instance.onScanComplete(data);
			LifeformScanningSystem.instance.onScanComplete(data);
			DataCollectionTracker.instance.onScanComplete(data);
			MoraleSystem.instance.shiftMorale(1);
		}

		public static void onTechUnlocked(TechType tech, bool vb) {/*
    	if (tech == TechType.PrecursorKey_Orange) {
    		Story.StoryGoal.Execute(SeaToSeaMod.crashMesaRadio.key, SeaToSeaMod.crashMesaRadio.goalType);
    	}
    	if (tech == TechType.NuclearReactor || tech == TechType.HighCapacityTank || tech == TechType.PrecursorKey_Purple || tech == TechType.SnakeMushroom || tech == CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType) {
    		Story.StoryGoal.Execute("RadioKoosh26", Story.GoalType.Radio); //pod 12
    	}*/
			C2CItems.onTechUnlocked(tech);
			MoraleSystem.instance.shiftMorale(2.5F);
		}

		public static void onDataboxTooltipCalculate(BlueprintHandTarget tgt) {
			LiveMixin lv = tgt.GetComponent<LiveMixin>();
			if (lv && lv.health < lv.maxHealth) {
				HandReticle.main.SetInteractText("NeedRepairDataBox");
				HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
			}
		}

		public static bool onDataboxClick(BlueprintHandTarget tgt) { //return true to prevent use
			if (tgt.used)
				return true;
			if (tgt.unlockTechType == TechType.BaseReinforcement) {
				StoryGoal.Execute(SeaToSeaMod.REINF_DB_GOAL, Story.GoalType.Story);
			}
			LiveMixin lv = tgt.GetComponent<LiveMixin>();
			return lv && lv.health < lv.maxHealth;
		}

		public static void applyGeyserFilterBuildability(DIHooks.BuildabilityCheck check) {
			if (VoidSpikesBiome.instance.isInBiome(Player.main.transform.position) || (Player.main.transform.position - VoidSpikesBiome.signalLocation).sqrMagnitude <= 40000) {
				check.placeable = false;
				return;
			}
			if (Builder.constructableTechType == C2CItems.geyserFilter.TechType) {
				check.placeable = !check.placeOn && GeyserFilterLogic.findGeyser(Builder.GetGhostModel().transform.position);
				check.ignoreSpaceRequirements = check.placeable;
				//check.ignoreSpaceRequirements = true;
			}
			if (C2CIntegration.seaVoyager != TechType.None && check.placeOn && check.placeOn.gameObject.GetComponentInParent(C2CIntegration.seaVoyagerComponent))
				check.placeable = false;
		}

		public static void onHandSend(GameObject target, HandTargetEventType e, GUIHand hand) {/*
	    	SNUtil.writeToChat("Hand send fired for GO "+target+"$"+target.activeInHierarchy+"::"+target.GetFullHierarchyPath()+" @ "+target.transform.position+"#"+target.GetInstanceID()+" of type "+e+", on hand "+hand+", TT="+target.GetComponent<IHandTarget>());
	    	if (KeyCodeUtils.GetKeyHeld(KeyCode.LeftAlt)) {
	    		IHandTarget iht = target.GetComponent<IHandTarget>();
	    		if (iht != null)
	    			iht.OnHandClick(hand);
	    	}*/
			if (e == HandTargetEventType.Hover && target)
				LifeformScanningSystem.instance.onObjectSeen(target, true);
			SanctuaryPlantTag spt = target.GetComponent<SanctuaryPlantTag>();
			if (spt) {
				if (e == HandTargetEventType.Hover)
					spt.OnHandHover(hand);
				else if (e == HandTargetEventType.Click)
					spt.OnHandClick(hand);
			}
		}

		public static void onKeypadFailed(KeypadDoorConsole con) {
			KeypadCodeSwappingSystem.instance.onCodeFailed(con);
		}

		public static void changeEquipmentCompatibility(DIHooks.EquipmentTypeCheck ch) {
			if (ch.item == C2CItems.lightModule.TechType && Player.main.currentSub && Player.main.currentSub.isCyclops && Vector3.Distance(Player.main.currentSub.GetComponentInChildren<CyclopsVehicleStorageTerminalManager>().transform.position, Player.main.transform.position) >= 4.5F) {
				ch.type = EquipmentType.CyclopsModule;
			}
		}

		public static List<SMLHelper.V2.Crafting.Ingredient> filterFCSRecyclerOutput(List<SMLHelper.V2.Crafting.Ingredient> li) {
			li.RemoveAll(i => C2CProgression.instance.isTechGated(i.techType));
			return li;
		}

		public static List<TechType> filterFCSDrillerOutput(List<TechType> li) {
			li.RemoveAll(C2CProgression.instance.isTechGated);
			return li;
		}

		public static bool canFCSDrillOperate(bool orig, MonoBehaviour drill) { //orig is actually hasOil in some cases
			return orig && canFCSDrillOperate(drill);
		}

		private static float lastDrillDepletionTime = -1;

		public static bool canFCSDrillOperate(MonoBehaviour drill) {
			//SNUtil.writeToChat("Drill "+drill+" @ "+drill.transform.position+" is trying to mine: "+orig);
			bool ret = DrillDepletionSystem.instance.hasRemainingLife(drill);
			if (!ret) {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time - lastDrillDepletionTime >= 1) {
					lastDrillDepletionTime = time;
					SNUtil.writeToChat("Drill in " + WorldUtil.getRegionalDescription(drill.transform.position, true) + " has depleted the local resources.");
					Component com = drill.GetComponent(FCSIntegrationSystem.instance.getFCSDrillOreManager());
					if (com) {
						PropertyInfo p = FCSIntegrationSystem.instance.getFCSDrillOreManager().GetProperty("AllowedOres", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						p.SetValue(com, new List<TechType> { });
					}
				}
			}
			return ret;
		}

		public static void tickFCSDrill(MonoBehaviour drill) {
			//SNUtil.writeToChat("Ticking drill "+drill+" @ "+drill.transform.position);
			if (DayNightCycle.main.deltaTime > 0)
				DrillDepletionSystem.instance.deplete(drill);
		}

		public static TechType getFCSDrillFuel() {
			return FCSIntegrationSystem.instance.fcsDrillFuel.TechType;
		}

		public static TechType pickFCSDrillOre(TechType orig, MonoBehaviour drill, bool filtering, bool blacklist, HashSet<TechType> filters, List<TechType> defaultSet) {
			DrillableResourceArea d = DrillDepletionSystem.instance.getMotherlode(drill);
			if (d != null) {
				TechType ret = getRandomValidMotherlodeDrillYield(d);
				if (filtering && filters.Contains(ret) == blacklist)
					ret = TechType.None;
				//SNUtil.writeToChat("picking new drop for drill "+drill+" on "+d.ClassID+": "+ret);
				return ret;
			}
			return orig;
		}

		private static TechType getRandomValidMotherlodeDrillYield(DrillableResourceArea d) {
			TechType ret = d.getRandomResourceType();
			while (!isFCSDrillMaterialAllowed(ret, false))
				ret = d.getRandomResourceType();
			return ret;
		}

		internal static bool isFCSDrillMaterialAllowed(TechType tt, bool skipChance) {
			return tt == TechType.Nickel
				? StoryGoalManager.main.completedGoals.Contains("Nickel") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.4F)
				: tt == TechType.MercuryOre
				? StoryGoalManager.main.completedGoals.Contains("Mercury") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.2F)
				: tt == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType
				? StoryGoalManager.main.completedGoals.Contains("Iridium") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.2F)
				: tt == TechType.Kyanite
				? StoryGoalManager.main.completedGoals.Contains("Kyanite") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.25F)
				: tt == TechType.Sulphur
				? StoryGoalManager.main.completedGoals.Contains("Sulfur") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.5F)
				: tt != TechType.UraniniteCrystal || (StoryGoalManager.main.completedGoals.Contains("Uranium") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.5F));
		}

		public static Action<int, int> cleanupFCSContainer(Action<int, int> notify, MonoBehaviour drill, Dictionary<TechType, int> dict) {
			if (dict.ContainsKey(TechType.None)) {
				SNUtil.writeToChat("Removed TechType.None from drill inventory");
				dict.Remove(TechType.None);
			}
			int removed = 0;
			if (dict.ContainsKey(TechType.MercuryOre) && !StoryGoalManager.main.completedGoals.Contains("Mercury")) {
				removed += dict[TechType.MercuryOre];
				dict.Remove(TechType.MercuryOre);
			}
			if (dict.ContainsKey(TechType.Nickel) && !StoryGoalManager.main.completedGoals.Contains("Nickel")) {
				removed += dict[TechType.Nickel];
				dict.Remove(TechType.Nickel);
			}
			if (dict.ContainsKey(TechType.Kyanite) && !StoryGoalManager.main.completedGoals.Contains("Kyanite")) {
				removed += dict[TechType.Kyanite];
				dict.Remove(TechType.Kyanite);
			}
			if (dict.ContainsKey(TechType.Sulphur) && !StoryGoalManager.main.completedGoals.Contains("Sulfur")) {
				removed += dict[TechType.Sulphur];
				dict.Remove(TechType.Sulphur);
			}
			if (dict.ContainsKey(TechType.UraniniteCrystal) && !StoryGoalManager.main.completedGoals.Contains("Uranium")) {
				removed += dict[TechType.UraniniteCrystal];
				dict.Remove(TechType.UraniniteCrystal);
			}
			if (dict.ContainsKey(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType) && !StoryGoalManager.main.completedGoals.Contains("Iridium")) {
				removed += dict[CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType];
				dict.Remove(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType);
			}
			if (removed > 0) {
				DrillableResourceArea d = DrillDepletionSystem.instance.getMotherlode(drill);
				SNUtil.writeToChat("Removing " + removed + " progression-gated resources from drill yield @ " + d);
				for (int i = 0; i < removed; i++) {
					TechType tt = d == null ? (UnityEngine.Random.Range(0F, 1F) <= 0.3 ? TechType.Copper : TechType.Titanium) : getRandomValidMotherlodeDrillYield(d);
					dict[tt] = dict.ContainsKey(tt) ? dict[tt] + 1 : 1;
				}
			}
			return notify;
		}

		public static float getFCSBioGenPowerFactor(float val, MonoBehaviour power, TechType item) {
			if (item == FCSIntegrationSystem.instance.getBiofuel())
				val *= 4;
			return val;
		}

		public static void controlPlayerInput(DIHooks.PlayerInput pi) {
			Drunk.manageDrunkenness(pi);
		}

		public static void onFCSPurchasedTech(TechType tt) {
			FCSIntegrationSystem.instance.onPlayerBuy(tt);
		}

		public static bool isFCSItemBuyable(TechType tt) {
			//SNUtil.writeToChat("checking if "+tt.AsString()+" is buyable: unlocked="+CrafterLogic.IsCraftRecipeUnlocked(tt));
			return tt != TechType.None && !KnownTech.Contains(tt) && tt != FCSIntegrationSystem.instance.getTeleportCard() && tt != FCSIntegrationSystem.instance.getVehiclePad();
		}

		public static int filterFCSCartAdd(int origLimit, System.Collections.IList cart, TechType adding) { //cart is a List<CartItem>, each of which has a TechType property which might == adding
			if (cart.Count <= 0 || !FCSIntegrationSystem.instance.isUnlockingTypePurchase(adding)) //always allow first item or as many non-unlocking purchases as you want
				return origLimit;
			PropertyInfo pi = cart[0].GetType().GetProperty("TechType", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach (object obj in cart) {
				TechType tt = (TechType)pi.GetValue(obj);
				if (tt == adding)
					return -1;
			}
			return origLimit;
		}

		public static bool isTeleporterFunctional(bool orig, MonoBehaviour teleporter) {
			//SNUtil.writeToChat("Testing teleporter "+teleporter+" @ "+teleporter.transform.position);
			return orig && FCSIntegrationSystem.instance.checkTeleporterFunction(teleporter);
		}

		public static float getCurrentGeneratorPower(float orig, MonoBehaviour generator) {
			float sp = DayNightCycle.main.dayNightSpeed * 2;
			return sp * 1.2F * FCSIntegrationSystem.instance.getCurrentGeneratorPowerFactor(generator);
		}

		public static void onMeteorImpact(GameObject meteor, Pickupable drop) {
			if (!PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.MeteorPrompt).key)) {
				Story.StoryGoal.Execute(C2CProgression.METEOR_GOAL, Story.GoalType.Story);
			}
		}

		public static void buildDisplayMonitorButton(MonoBehaviour screen, uGUI_ItemIcon icon) {
			icon.transform.localScale = new Vector3(0.5F, 0.45F, 1);
			GameObject grid = screen.gameObject.getChildObject("Canvas/Screens/MainScreen/ActualScreen/MainGrid");
			UnityEngine.UI.GridLayoutGroup grp = grid.GetComponent<UnityEngine.UI.GridLayoutGroup>();
			grp.cellSize = new Vector2(100, 90);
		}

		public static bool isStorageVisibleToDisplayMonitor(bool skip, StorageContainer sc) {
			//SNUtil.writeToChat("checking SC="+sc+": "+skip);
			skip |= sc && sc.gameObject.FindAncestor<MapRoomFunctionality>();
			skip |= sc && sc.GetComponent<BioprocessorLogic>();
			skip |= sc && sc.GetComponent<Planter>();
			return skip;
		}

		public static void mergeDeathrunRecipeChange(TechType tt, TechData td) {
			TechData real = RecipeUtil.getRecipe(tt);
			if (real == null) {
				SNUtil.log("Discarding deathrun " + tt + " recipe, as there is no vanilla recipe");
				return;
			}
			SNUtil.log("Integrating deathrun recipe change: " + tt + " = " + RecipeUtil.toString(td) + " into " + RecipeUtil.toString(real));
			Dictionary<TechType, int> cost = RecipeUtil.getIngredientsDict(real);
			foreach (Ingredient i in td.Ingredients) {
				if (cost.ContainsKey(i.techType)) {
					cost[i.techType] = Math.Max(cost[i.techType], i.amount);
				}
			}
			RecipeUtil.modifyIngredients(tt, i => {
				i.amount = cost[i.techType];
				return false;
			});
		}

		public static void mergeDeathrunFragmentScanCount(TechType tt, int amt) {
			PDAHandler.EditFragmentsToScan(tt, Math.Max(amt, ReikaKalseki.Reefbalance.ReefbalanceMod.getScanCountOverride(tt)));
		}

		public static bool allowSaving(bool orig) {
			if (!orig)
				return false;
			if (GameModeUtils.currentEffectiveMode == GameModeOption.Creative)
				return true;
			Player ep = Player.main;
			Survival s = ep.GetComponent<Survival>();
			if (GameModeUtils.RequiresSurvival() && (s.water < 10 || s.food < 10))
				return false;
			if (VoidSpikesBiome.instance.getDistanceToBiome(ep.transform.position) < 400)
				return false;
			if (ep.currentEscapePod)
				return true;
			if (ep.radiationAmount > 0 && !(Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0 && Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0 && Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0))
				return false;
			if (DayNightCycle.main && DayNightCycle.main.timePassedAsFloat - lastO2PipeTime <= 0.5)
				return true;
			if (ep.IsSwimming() && ep.transform.position.y < 0)
				return false;
			if (ep.currentWaterPark)
				return false;
			if (ep.currentSub && ep.currentSub.powerRelay && ep.currentSub.powerRelay.GetPower() > 0)
				return !ep.currentSub.isFlooded;
			if (ep.precursorOutOfWater)
				return true;
			if (BiomeBase.getBiome(ep.transform.position) == VanillaBiomes.ALZ || WaterTemperatureSimulation.main.GetTemperature(ep.transform.position) > 150)
				return false;
			if (!SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
				Vehicle v = ep.GetVehicle();
				if (v && v.IsPowered())
					return true;
			}
			return false;
		}

		public static void onWaterFilterSpawn(DIHooks.WaterFilterSpawn sp) {
			TechType id = TechType.None;
			Vector3 refpt = sp.filter.transform.position; //basically right above the brine
			BiomeBase bb = BiomeBase.getBiome(refpt);
			if (bb == VanillaBiomes.COVE && sp.item.GetTechType() == TechType.Salt) {
				bool inBrine = false;
				bool trig = Physics.queriesHitTriggers;
				Physics.queriesHitTriggers = true;
				foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(refpt, 4)) {
					if (SNUtil.match(pi, "32e0b9a0-236b-4e03-81cf-921a92ef735d")) {
						inBrine = true;
						break;
					}
				}
				Physics.queriesHitTriggers = trig;
				if (inBrine) {
					id = SeaToSeaMod.geogel.TechType;
				}
			}
			else if (bb == VanillaBiomes.LOSTRIVER && sp.item.GetTechType() == TechType.Salt) {
				bool inBrine = false;
				foreach (RaycastHit hit in Physics.SphereCastAll(refpt, 4, Vector3.up, 0.1F, 1, QueryTriggerInteraction.Collide)) {
					if (hit.transform && hit.transform.GetComponent<AcidicBrineDamageTrigger>()) {
						inBrine = true;
						break;
					}
				}
				if (inBrine) {
					id = CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType;
				}
			}
			else if (bb == UnderwaterIslandsFloorBiome.instance && refpt.y < -500 && sp.item.GetTechType() == TechType.Salt) {
				id = GeyserMaterialSpawner.getRandomMineral(UnderwaterIslandsFloorBiome.instance);
			}
			else if (bb == VanillaBiomes.ILZ && sp.item.GetTechType() == TechType.Salt) {
				id = CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).TechType;
			}
			if (id != TechType.None) {
				Vector2int sz = CraftData.GetItemSize(id);
				if (sp.filter.storageContainer.container.HasRoomFor(sz.x, sz.y)) {
					InventoryItem ii = new InventoryItem(ObjectUtil.createWorldObject(id).GetComponent<Pickupable>().Pickup(false));
					sp.filter.storageContainer.container.UnsafeAdd(ii);
				}
			}
		}

		public static void tickSwimCharge(UpdateSwimCharge ch) {
			bool active = Inventory.main.equipment.GetCount(TechType.SwimChargeFins) > 0;
			bool relay = active && Inventory.main.equipment.GetCount(C2CItems.chargeFinRelay.TechType) > 0;
			bool charging = false;
			if (active && Player.main.IsUnderwater()) {
				float vel = Player.main.GetComponent<Rigidbody>().velocity.magnitude;
				if (vel > 2F) {
					float chargeAmount = (float)MathUtil.linterpolate(vel, 10, 20, 0.005, 0.04, true); //0.005 in vanilla, give bonus if going > 10 (seaglide), azurite seaglide peaks about 17
																									   //SNUtil.writeToChat(vel+" > "+Mathf.Sqrt(vel)+" > "+chargeAmount);
					PlayerTool held = Inventory.main.GetHeldTool();
					if (relay) {
						foreach (EnergyMixin e in InventoryUtil.getAllHeldChargeables()) {
							PlayerTool tool = e.GetComponent<PlayerTool>();
							if (tool) {
								float add = tool == held ? chargeAmount*0.9F : chargeAmount*0.33F; //90% efficiency on held, 33% efficiency on non-helds
								if (e && e.AddEnergy(add))
									charging = true;
							}
						}
					}
					else if (held != null) {
						EnergyMixin e = held.GetComponent<EnergyMixin>();
						if (e && e.AddEnergy(chargeAmount))
							charging = true;
					}
				}
			}
			if (charging)
				ch.swimChargeLoop.Play();
			else
				ch.swimChargeLoop.Stop();

			if (charging && relay)
				BatteryChargeIndicatorHandler.resyncChargeIndicators();
		}

		public static void onStartInvUI(uGUI_InventoryTab gui) {
			C2CUtil.createRescuePDAButton();
		}

		/*
		class DelayedBatterySwapCallback : MonoBehaviour {

			internal TechType battery;
            internal float charge;
			internal EnergyMixin mixin;

			public DelayedBatterySwapCallback init(TechType tt, float f, EnergyMixin e) {
				battery = tt;
				charge = f;
				mixin = e;
				return this;
			}

			public void apply() {
				if (mixin)
					mixin.SetBattery(battery, charge);
				this.destroy(false);
            }

		}
		*/
		public static void onCollectFromVaseStrand(MushroomVaseStrand.MushroomVaseStrandTag plant, TechType item) {
			if (item == CraftingItems.getItem(CraftingItems.Items.Tungsten).TechType) {
				Story.StoryGoal.Execute(C2CProgression.TUNGSTEN_GOAL, Story.GoalType.Story);
			}
		}

		private static void onRocketStageComplete(Rocket r, int stage, bool anyComplete) {
			MoraleSystem.instance.shiftMorale(anyComplete ? 20 : 5);
		}

		private static void onCuddlefishPlay(CuteFishHandTarget target, Player player, CuteFishHandTarget.CuteFishCinematic cinematic) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastCuddlefishPlay < 600) //10 min
				return;
			lastCuddlefishPlay = time;
			MoraleSystem.instance.shiftMorale(25);
		}

		public static void onSleep(Bed bed) {
			float f = 1;
			switch (bed.GetComponent<PrefabIdentifier>().ClassId) {
				case "c3994649-d0da-4f8c-bb77-1590f50838b9":
					f = 0.8F;
					break;
				case "cdb374fd-4f38-4bef-86a3-100cc87155b6":
					f = 1.25F;
					break;
			}
			MoraleSystem.instance.shiftMorale(f*(SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 10 : 20));
		}

		public static void onEat(Survival s, GameObject go) {
			if (go) {
				Pickupable pp = go.GetComponent<Pickupable>();
				if (pp) {
					TechType tt = pp.GetTechType();
					if (tt == TechType.BigFilteredWater || tt == TechType.DisinfectedWater || tt == TechType.FilteredWater)
						return;
					int morale;
					if (tt == TechType.Coffee) {
						MoraleSystem.instance.onDrinkCoffee();
						return;
					}
					else if (tt == TechType.Snack1 || tt == TechType.Snack2 || tt == TechType.Snack3) {
						morale = 20;
						PlayerMovementSpeedModifier.add(0.9F, 60*10);
					}
					else if (tt == TechType.StillsuitWater) {
						morale = -50;
					}
					else if (tt == TechType.Bladderfish) {
						morale = -40;
					}
					else if (tt == TechType.Hoverfish || tt == TechType.CookedHoverfish || tt == TechType.CuredHoverfish || tt == Campfire.cookMap[TechType.Hoverfish].output.TechType) {
						morale = -10;
					}
					else if (tt.isRawFish()) {
						morale = -25;
					}
					else {
						ReadOnlyCollection<ConsumableTracker.ConsumeItemEvent> li = ConsumableTracker.instance.getEvents();
						int eatsSinceDifferent = 999999;
						int back = 1;
						for (int i = li.Count - 2; i >= 0; i--) { //this event is already in the list so start an extra item back
							ConsumableTracker.ConsumeItemEvent evt = li[i];
							if (!evt.isEating)
								continue;
							if (tt == TechType.BigFilteredWater || tt == TechType.DisinfectedWater || tt == TechType.FilteredWater || tt == TechType.StillsuitWater || tt == TechType.Coffee)
								continue;
							//SNUtil.writeToChat("ate "+evt.itemType+" @ "+evt.eventTime);
							if (MoraleSystem.instance.areFoodsDifferent(evt.itemType, tt)) {
								eatsSinceDifferent = back;
								break;
							}
							back++;
						}
						string msg;
						switch (back) {
							case 1: //different from last item -> boost
								morale = 10;
								msg = "Morale boost from dietary variety";
								break;
							case 2: //if same as last two items then no effect
							case 3:
								morale = 0;
								msg = "Dietary variety recommended for optimum morale";
								break;
							case 4: //if have to go back five items then small penalty
							case 5:
								morale = -10;
								msg = "Lack of dietary variety slightly harming morale";
								break;
							case 6: //if have to go back five items then moderate penalty
							case 7:
							case 8:
								morale = -20;
								msg = "Lack of dietary variety substantially harming morale";
								break;
							default: //eight or more and you are always eating the same thing, so big penalty
								morale = -40;
								msg = "Lack of dietary variety severely harming morale";
								break;
						}
						SNUtil.writeToChat(msg);
					}
					MoraleSystem.instance.shiftMorale(morale);
				}
			}
		}

		public static void affectFoodRate(DIHooks.FoodRateCalculation calc) {
			float morale = MoraleSystem.instance.moralePercentage;
			if (morale < 40) {
				calc.rate *= Mathf.Lerp(2.5F, 1, morale / 40F);
			}
			else if (morale > 80) {
				calc.rate *= Mathf.Lerp(1, 0.5F, (morale - 80F) / 20F);
			}
		}
	}
}
