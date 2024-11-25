using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Text;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.Auroresource;
using ReikaKalseki.Ecocean;
using ReikaKalseki.Exscansion;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public static class C2CHooks {
	    
	    internal static readonly Vector3 deepDegasiTablet = new Vector3(-638.9F, -506.0F, -941.3F);
	    internal static readonly Vector3 crashMesa = new Vector3(623.8F, -250.0F, -1105.2F);
	    internal static readonly Vector3 mountainBaseGeoCenter = new Vector3(953, -344, 1453);
	    internal static readonly Vector3 bkelpBaseGeoCenter = new Vector3(-1311.6F, -670.6F, -412.7F);
	    internal static readonly Vector3 bkelpBaseNuclearReactor = new Vector3(-1325.67F, -660.60F, -392.70F);
	    internal static readonly Vector3 trailerBaseBioreactor = new Vector3(1314.94F, -80.2F, -412.97F);
	    internal static readonly Vector3 lrpowerSealSetpieceCenter = new Vector3(-713.45F, -766.37F, -262.74F);
	    internal static readonly Vector3 auroraFront = new Vector3(1202.43F, -40.16F, 151.54F);
	    internal static readonly Vector3 auroraRepulsionGunTerminal = new Vector3(1029.51F, -8.7F, 35.87F);
	    internal static readonly Vector3 lostRiverCachePanel = new Vector3(-1119.5F, -684.4F, -709.7F);
	    
	    internal static readonly Vector3 fcsWreckOpenableDoor = new Vector3(88.87F, -420.75F, 1449.10F);
	    internal static readonly Vector3 fcsWreckBlockedDoor = new Vector3(93.01F, -421.27F, 1444.71F);    
	    
	    internal static readonly Vector3 OZZY_FORK_DEEP_ROOM_POS = new Vector3(-645.6F, -102.7F, -16.2F);
	    internal static readonly string OZZY_FORK_DEEP_ROOM_GOAL = "ozzyforkdeeproom";
	    internal static readonly Vector3 UNDERISLANDS_BLOCKED_ROOM_POS = new Vector3(-124.38F, -200.69F, 855F);
	    internal static readonly string UNDERISLANDS_BLOCKED_ROOM_GOAL = "underislandsblockedroom";
	    internal static readonly Vector3 FLOATING_ARCH_POS = new Vector3(-662.55F, 5.50F, -1064.35F);
	    internal static readonly string FLOATING_ARCH_GOAL = "floatarch";
	    internal static readonly Vector3 PLANT_ALCOVE_POS = new Vector3(375, 22, 870);
	    internal static readonly string PLANT_ALCOVE_GOAL = "islandalcove";
	    internal static readonly Vector3 MUSHTREE_POS = new Vector3(-879.7F, -138, 590.4F);
	    internal static readonly string MUSHTREE_GOAL = "mushtree";
	    internal static readonly Vector3 MUSHTREE_ARCH_POS = new Vector3(-777.5F, -229.8F, 404.8F);
	    internal static readonly string MUSHTREE_ARCH_GOAL = "musharch";
	    internal static readonly Vector3 CRAG_ARCH_POS = new Vector3(-90.2F, -287.4F, -1261.5F);
	    internal static readonly string CRAG_ARCH_GOAL = "cragarch";
	    internal static readonly Vector3 KOOSH_ARCH_POS = new Vector3(1344.8F, -309.2F, 730.7F);
	    internal static readonly string KOOSH_ARCH_GOAL = "koosharch";
	    internal static readonly Vector3 LR_ARCH_POS = new Vector3(-914.7F, -621.2F, 1078.4F);
	    internal static readonly string LR_ARCH_GOAL = "lrarch";
	    internal static readonly string METEOR_GOAL = "meteorhit";
	    
	    private static readonly PositionedPrefab auroraStorageModule = new PositionedPrefab("d290b5da-7370-4fb8-81bc-656c6bde78f8", new Vector3(991.5F, 3.21F, -30.99F), Quaternion.Euler(14.44F, 353.7F, 341.6F));
	    private static readonly PositionedPrefab auroraCyclopsModule = new PositionedPrefab("049d2afa-ae76-4eef-855d-3466828654c4", new Vector3(872.5F, 2.69F, -0.66F), Quaternion.Euler(357.4F, 224.9F, 21.38F));
	    
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
	    
	    private static readonly Dictionary<string, Color> auroraPrawnFireColors = new Dictionary<string, Color>{
					{"xFireFlame", new Color(0, 0.67F, 2)},
					{"xFireCurl", new Color(1, 1, 1)},
					{"xAmbiant_Sparks", new Color(0, 1, 1)},
					{"xAmbiant_Ashes", new Color(0.1F, 0.1F, 1)},
					{"x_Fire_CrossPlanes", new Color(0.67F, 0.43F, 1)},
					{"x_Fire_GroundPlane", new Color(0.24F, 0.57F, 1)},
					{"x_SmokeLight_Cylindrical", new Color(0.67F, 0.72F, 0.97F)},
					{"x_Fire_Cylindrical", new Color(0.24F, 0.51F, 1)},
				};
	    
	    private static Oxygen playerBaseO2;
	    
	    private static float nextSanctuaryPromptCheckTime = -1;
	    private static float nextBkelpBaseAmbCheckTime = -1;
	    private static float nextBkelpBaseAmbTime = -1;
	    private static float nextCameraEMPTime = -1;
	    
	    private static float foodToRestore;
	    private static float waterToRestore;
	    
	    public static readonly string prawnBayLocaleKey = "PrawnBayDoorHeatWarn";
	    public static readonly string lrCachePanelLocaleKey = "LostRiverCachePanel";
	    public static readonly string dockUpgradesLocaleKey = "DockToChangeVehicleUpgrades";
	    public static readonly string needRepairDataboxLocaleKey = "NeedRepairDataBox";
	    public static readonly string sanctuaryPlantGrowingLocaleKey = "SanctuaryPlantGrowing";
	    public static readonly string sanctuaryPlantClickLocaleKey = "SanctuaryPlantClick";
	    public static readonly string campfireCookingLocaleKey = "CampfireCooking";
	    public static readonly string campfireUseLocaleKey = "CampfireClick";
	    public static readonly string smokedNoExpireLocaleKey = "SmokedNoExpire";
	    
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
	    	
	    	DIHooks.onSeamothSonarUsedEvent += pingSeamothSonar;
	    	DIHooks.onTorpedoFireEvent += onTorpedoFired;
	    	DIHooks.onTorpedoExplodeEvent += onTorpedoExploded;
	    	
	    	DIHooks.onSonarUsedEvent += pingAnySonar;
	    	
	    	DIHooks.onEMPHitEvent += onEMPHit;
	    	
	    	DIHooks.constructabilityEvent += applyGeyserFilterBuildability;
	    	DIHooks.breathabilityEvent += canPlayerBreathe;
	    	
	    	DIHooks.getSwimSpeedEvent += getSwimSpeed;
	    	
	    	//DIHooks.fogCalculateEvent += interceptChosenFog;
	    	
	    	DIHooks.radiationCheckEvent += (ch) => {if (!skipRadiationLevel) ch.value = getRadiationLevel(ch);};
	    	
	    	DIHooks.itemTooltipEvent += generateItemTooltips;
	    	DIHooks.bulkheadLaserHoverEvent += interceptBulkheadLaserCutter;
        
	    	DIHooks.knifeAttemptEvent += tryKnife;
	    	DIHooks.onKnifedEvent += onKnifed;
	    	DIHooks.knifeHarvestEvent += interceptItemHarvest;
	    	
	    	DIHooks.onFruitPlantTickEvent += tickFruitPlant;
	    	
	    	DIHooks.reaperGrabVehicleEvent += onReaperGrab;
	    	
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
	    	
	    	BaseSonarPinger.onBaseSonarPingedEvent += onBaseSonarPinged;
	    	BaseDrillableGrinder.onDrillableGrindEvent += getGrinderDrillableDrop;
	    	
	    	LavaBombTag.onLavaBombImpactEvent += onLavaBombHit;
	    	ExplodingAnchorPod.onExplodingAnchorPodDamageEvent += onAnchorPodExplode;
	    	PlanktonCloudTag.onPlanktonActivationEvent += onPlanktonActivated;
	    	VoidBubble.voidBubbleSpawnerTickEvent += tickVoidBubbles;
	    	VoidBubble.voidBubbleTickEvent += tickVoidBubble;
	    	
	    	FallingMaterialSystem.impactEvent += onMeteorImpact;
	    	
	    	scanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType);
	    	scanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType);
	    	scanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType);
	    	scanToScannerRoom.Add(SeaToSeaMod.voidSpikeLevi.TechType);
	    	scanToScannerRoom.Add(C2CItems.alkali.TechType);
	    	scanToScannerRoom.Add(C2CItems.healFlower.TechType);
	    	scanToScannerRoom.Add(C2CItems.kelp.TechType);
	    }
	    
	    public static void onWorldLoaded() {	    	
	    	Inventory.main.equipment.onEquip += onEquipmentAdded;
	    	Inventory.main.equipment.onUnequip += onEquipmentRemoved;
	        
	    	BrokenTablet.updateLocale();
	    	
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
	    		s = s.Replace(" seed", " Sample");
	    		s = s.Replace(" spore", " Sample");
	    		s = s.Replace(" Seed", " Sample");
	    		s = s.Replace(" Spore", " Sample");
	    		LanguageHandler.SetLanguageLine(k, s);
		    }
	    	
	    	LanguageHandler.SetLanguageLine("EncyDesc_Aurora_DriveRoom_Terminal1", Language.main.Get("EncyDesc_Aurora_DriveRoom_Terminal1").Replace("from 8 lifepods", "from 14 lifepods").Replace("T+8hrs: 1", "T+8hrs: 7"));
	    		    	
	    	LanguageHandler.SetLanguageLine("Need_laserCutterBulkhead_Chit", SeaToSeaMod.miscLocale.getEntry("bulkheadLaserCutterUpgrade").getField<string>("error"));
			LanguageHandler.SetLanguageLine(prawnBayLocaleKey, SeaToSeaMod.miscLocale.getEntry(prawnBayLocaleKey).desc);
			LanguageHandler.SetLanguageLine(lrCachePanelLocaleKey, SeaToSeaMod.miscLocale.getEntry(lrCachePanelLocaleKey).desc);
			LanguageHandler.SetLanguageLine(dockUpgradesLocaleKey, SeaToSeaMod.miscLocale.getEntry(dockUpgradesLocaleKey).desc);
			LanguageHandler.SetLanguageLine(needRepairDataboxLocaleKey, SeaToSeaMod.miscLocale.getEntry(needRepairDataboxLocaleKey).desc);
			LanguageHandler.SetLanguageLine(sanctuaryPlantClickLocaleKey, SeaToSeaMod.miscLocale.getEntry(sanctuaryPlantClickLocaleKey).desc);
			LanguageHandler.SetLanguageLine(sanctuaryPlantGrowingLocaleKey, SeaToSeaMod.miscLocale.getEntry(sanctuaryPlantGrowingLocaleKey).desc);
			LanguageHandler.SetLanguageLine(campfireUseLocaleKey, SeaToSeaMod.miscLocale.getEntry(campfireUseLocaleKey).desc);
			LanguageHandler.SetLanguageLine(campfireCookingLocaleKey, SeaToSeaMod.miscLocale.getEntry(campfireCookingLocaleKey).desc);
			LanguageHandler.SetLanguageLine(smokedNoExpireLocaleKey, SeaToSeaMod.miscLocale.getEntry(smokedNoExpireLocaleKey).desc);
	    	LanguageHandler.SetLanguageLine("Tooltip_"+TechType.MercuryOre.AsString(), SeaToSeaMod.miscLocale.getEntry("MercuryDesc").desc);
	    	LanguageHandler.SetLanguageLine("EncyDesc_Mercury", SeaToSeaMod.miscLocale.getEntry("MercuryDesc").pda);
	    	LanguageHandler.SetLanguageLine("Tooltip_"+TechType.PrecursorKey_Red.AsString(), SeaToSeaMod.itemLocale.getEntry("redkey").desc);
	    	LanguageHandler.SetLanguageLine("Tooltip_"+TechType.PrecursorKey_White.AsString(), SeaToSeaMod.itemLocale.getEntry("whitekey").desc);
	    	
	    	Campfire.updateLocale(); //call after the above locale init
	    	KeypadCodeSwappingSystem.instance.patchEncyPages();
	    	
	    	LanguageHandler.SetLanguageLine(SeaToSeaMod.tunnelLight.TechType.AsString(), Language.main.Get(TechType.LEDLight));
	    	LanguageHandler.SetLanguageLine("Tooltip_"+SeaToSeaMod.tunnelLight.TechType.AsString(), Language.main.Get("Tooltip_"+TechType.LEDLight.AsString()));
	    	
	    	LanguageHandler.SetLanguageLine(SeaToSeaMod.deadMelon.TechType.AsString(), Language.main.Get(TechType.MelonPlant));
	    	
	    	LanguageHandler.SetLanguageLine("Tooltip_"+TechType.VehicleHullModule3.AsString(), Language.main.Get("Tooltip_"+TechType.VehicleHullModule3.AsString().Replace("maximum", "900m")));
	    	
	    	StoryGoalCustomEventHandler.main.sunbeamGoals[StoryGoalCustomEventHandler.main.sunbeamGoals.Length-2].trigger = SeaToSeaMod.sunbeamCountdownTrigger.key;
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
	    	
	    	if (FCSIntegrationSystem.instance.isLoaded())
	    		FCSIntegrationSystem.instance.tickNotifications(time);
	    	
	    	LifeformScanningSystem.instance.tick(time);
	    	
	    	if (Camera.main && Vector3.Distance(ep.transform.position, Camera.main.transform.position) > 5) {
	    		if (VoidSpikesBiome.instance.getDistanceToBiome(Camera.main.transform.position, true) < 200)
	    			WaterBiomeManager.main.GetComponent<WaterscapeVolume>().fogEnabled = true;
	    	}
	    	
	    	ItemUnlockLegitimacySystem.instance.tick(ep);
	    	
	    	if (LiquidBreathingSystem.instance.hasTankButNoMask()) {
	    		Oxygen ox = Inventory.main.equipment.GetItemInSlot("Tank").item.gameObject.GetComponent<Oxygen>();
	    		ep.oxygenMgr.UnregisterSource(ox);
	    		ep.oxygenMgr.UnregisterSource(playerBaseO2);
	    	}	    	
	    	else if (LiquidBreathingSystem.instance.hasLiquidBreathing()) {
	    		//SNUtil.writeToChat("Tick liquid breathing: "+LiquidBreathingSystem.instance.isLiquidBreathingActive(ep));
	    		Oxygen ox = Inventory.main.equipment.GetItemInSlot("Tank").item.gameObject.GetComponent<Oxygen>();
	    		if (LiquidBreathingSystem.instance.isLiquidBreathingActive(ep)) {
	    			LiquidBreathingSystem.instance.tickKharaaTreatment();
	    			ep.oxygenMgr.UnregisterSource(playerBaseO2);
	    			ep.oxygenMgr.RegisterSource(ox);
	    		}
	    		else {
	    			ep.oxygenMgr.UnregisterSource(ox);
	    			ep.oxygenMgr.RegisterSource(playerBaseO2);
	    			float add = Mathf.Min(ep.oxygenMgr.oxygenUnitsPerSecondSurface, ox.oxygenCapacity-ox.oxygenAvailable)*Time.deltaTime;
	    			if (add > 0.01) {
	    				if (LiquidBreathingSystem.instance.tryFillPlayerO2Bar(ep, ref add)) {
	    					ox.AddOxygen(add);
	    					//LiquidBreathingSystem.instance.onAddO2ToBar(add);
	    				}
	    			}
	    		}
	    	}
	    	else {
	    		ep.oxygenMgr.RegisterSource(playerBaseO2);
	    		if (time-LiquidBreathingSystem.instance.getLastUnequippedTime() < 0.5)
	    			ep.oxygenMgr.RemoveOxygen(ep.oxygenMgr.GetOxygenAvailable());
	    	}
	    	
	    	if (!PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key))
	    		SeaToSeaMod.sanctuaryDirectionHint.deactivate();
	    	if (!VoidSpikesBiome.instance.isRadioFired())
	    		SeaToSeaMod.voidSpikeDirectionHint.deactivate();
	    	
	    	float distsq = (ep.transform.position-crashMesa).sqrMagnitude-400;
	    	if (time >= nextSanctuaryPromptCheckTime && !PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key)) {
	    		nextSanctuaryPromptCheckTime = time+1;
	    		if (distsq < 2500 || Vector3.Distance(ep.transform.position, auroraFront) < 144 || Vector3.Distance(ep.transform.position, trailerBaseBioreactor) < 200 || Vector3.Distance(ep.transform.position, CrashZoneSanctuaryBiome.biomeCenter) < 200) {
	    			Player.main.gameObject.EnsureComponent<CrashMesaCallback>().Invoke("triggerSanctuary", 20);
	    		}
	    	}
	    	
	    	if (distsq < 25*25 || (distsq <= 250*250 && UnityEngine.Random.Range(0F, 1F) <= 0.075F*Time.timeScale*(distsq <= 10000 ? 2.5F : 1))) {
	    		IEcoTarget tgt = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Leviathan, crashMesa, eco => eco.GetGameObject().GetComponent<ReaperLeviathan>(), 6);
	    		if (tgt != null && (tgt.GetPosition()-crashMesa).sqrMagnitude >= Mathf.Max(distsq, 225)) {
	    			GameObject go = tgt.GetGameObject();
	    			Vehicle v = ep.GetVehicle();
	    			GameObject hit = v ? v.gameObject : ep.gameObject;
	    			Vector3 pos = distsq <= 2500 ? hit.transform.position : MathUtil.getRandomVectorAround(crashMesa, 40).setY(crashMesa.y);
	    			if (Vector3.Distance(go.transform.position, pos) >= 40)
	    				go.GetComponent<SwimBehaviour>().SwimTo(pos, 20);
	    			ReaperLeviathan r = go.GetComponent<ReaperLeviathan>();
	    			r.Aggression.Add(0.75F);
	    			r.leashPosition = pos;
	    			go.GetComponent<ReaperMeleeAttack>().lastTarget.SetTarget(hit);
	    			foreach (AggressiveWhenSeeTarget a in go.GetComponents<AggressiveWhenSeeTarget>())
	    				a.lastTarget.SetTarget(hit);
	    		}
	    	}
	    	
	    	VoidSpikesBiome.instance.tickPlayer(ep);
	    	UnderwaterIslandsFloorBiome.instance.tickPlayer(ep);
	    	
	    	ExplorationTrackerPages.instance.tick();
	    	
	    	if (ep.currentSub == null && UnityEngine.Random.Range(0, (int)(10/Time.timeScale)) == 0) {
	    		if (ep.GetVehicle() == null) {
	    			float ventDist = -1;
					IEcoTarget tgt = EcoRegionManager.main.FindNearestTarget(EcoTargetType.HeatArea, ep.transform.position, null, 3);
					if (tgt != null)
						ventDist = Vector3.Distance(tgt.GetPosition(), ep.transform.position);
					if (ventDist >= 0 && ventDist <= 25) {
						float f = Math.Min(1, (40-ventDist)/32F);
			    		foreach (InventoryItem item in Inventory.main.container) {
			    			if (item != null) {
			    				Battery b = item.item.gameObject.GetComponentInChildren<Battery>();
			    				if (b != null && Mathf.Approximately(b.capacity, C2CItems.t2Battery.capacity)) {
			    					b.charge = Math.Min(b.charge+0.5F*f, b.capacity);
			    					continue;
			    				}
			    				EnergyMixin e = item.item.gameObject.GetComponentInChildren<EnergyMixin>();
			    				if (e != null && e.battery != null && Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity)) {
			    					//SNUtil.writeToChat("Charging "+item.item+" by factor "+f+", d="+ventDist);
			    					e.AddEnergy(0.5F*f);
			    				}
			    			}
			    		}
					}
	    		}
	    		
	    		//TODO cleanup
	    		if (!Story.StoryGoalManager.main.completedGoals.Contains(OZZY_FORK_DEEP_ROOM_GOAL) && Vector3.Distance(OZZY_FORK_DEEP_ROOM_POS, ep.transform.position) <= 12) {
	    			Story.StoryGoal.Execute(OZZY_FORK_DEEP_ROOM_GOAL, Story.GoalType.Story);
	    		}
	    		if (!Story.StoryGoalManager.main.completedGoals.Contains(UNDERISLANDS_BLOCKED_ROOM_GOAL) && Vector3.Distance(UNDERISLANDS_BLOCKED_ROOM_POS, ep.transform.position) <= 5) {
	    			Story.StoryGoal.Execute(UNDERISLANDS_BLOCKED_ROOM_GOAL, Story.GoalType.Story);
	    		}
	    		if (!Story.StoryGoalManager.main.completedGoals.Contains(FLOATING_ARCH_GOAL) && ep.transform.position.y > 0 && ep.transform.position.y < 22.5F && Vector3.Distance(FLOATING_ARCH_POS, ep.transform.position) <= 25) {
	    			Story.StoryGoal.Execute(FLOATING_ARCH_GOAL, Story.GoalType.Story);
	    		}
	    		if (!Story.StoryGoalManager.main.completedGoals.Contains(PLANT_ALCOVE_GOAL) && ep.transform.position.y > 15 && ep.transform.position.y < 30F && Vector3.Distance(PLANT_ALCOVE_POS, ep.transform.position) <= 15) {
	    			Story.StoryGoal.Execute(PLANT_ALCOVE_GOAL, Story.GoalType.Story);
	    		}
	    		if (!Story.StoryGoalManager.main.completedGoals.Contains(MUSHTREE_GOAL) && Vector3.Distance(MUSHTREE_POS, ep.transform.position) <= 4) {
	    			Story.StoryGoal.Execute(MUSHTREE_GOAL, Story.GoalType.Story);
	    		}
	    		if (!Story.StoryGoalManager.main.completedGoals.Contains(MUSHTREE_ARCH_GOAL) && Vector3.Distance(MUSHTREE_ARCH_POS, ep.transform.position) <= 12) {
	    			Story.StoryGoal.Execute(MUSHTREE_ARCH_GOAL, Story.GoalType.Story);
	    		}
	    		if (!Story.StoryGoalManager.main.completedGoals.Contains(KOOSH_ARCH_GOAL) && Vector3.Distance(KOOSH_ARCH_POS, ep.transform.position) <= 8) {
	    			Story.StoryGoal.Execute(KOOSH_ARCH_GOAL, Story.GoalType.Story);
	    		}
	    		if (!Story.StoryGoalManager.main.completedGoals.Contains(LR_ARCH_GOAL) && Vector3.Distance(LR_ARCH_POS, ep.transform.position) <= 6) {
	    			Story.StoryGoal.Execute(LR_ARCH_GOAL, Story.GoalType.Story);
	    		}
	    		if (!Story.StoryGoalManager.main.completedGoals.Contains(CRAG_ARCH_GOAL) && Vector3.Distance(CRAG_ARCH_POS, ep.transform.position) <= 6) {
	    			Story.StoryGoal.Execute(CRAG_ARCH_GOAL, Story.GoalType.Story);
	    		}
	    	}
	    	
	    	if (time >= nextBkelpBaseAmbCheckTime) {
	    		nextBkelpBaseAmbCheckTime = time+UnityEngine.Random.Range(0.5F, 2.5F);
		    	if (Vector3.Distance(ep.transform.position, bkelpBaseGeoCenter) <= 60) {
			    	if (time >= nextBkelpBaseAmbTime) {
			    		SNUtil.log("Queuing bkelp base ambience @ "+ep.transform.position);
			    		VanillaMusic.WRECK.play();
			    		nextBkelpBaseAmbTime = DayNightCycle.main.timePassedAsFloat+UnityEngine.Random.Range(60F, 90F);
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
	    	if (skipO2)
	    		return orig;
	    	return LiquidBreathingSystem.instance.isO2BarFlashingRed() ? 6 : orig;
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
	    		b.setValue(VoidSpikesBiome.biomeName);
	    		b.lockValue();
	    		//if (BiomeBase.logBiomeFetch)
	    		//	SNUtil.writeToChat("Biome WBM fetch overridden to "+VoidSpikesBiome.biomeName);
	    	}
	    	else if (UnderwaterIslandsFloorBiome.instance.isInBiome(b.originalValue, b.position)) {
	    		b.setValue(UnderwaterIslandsFloorBiome.biomeName);
	    		b.lockValue();
	    		//if (BiomeBase.logBiomeFetch)
	    		//	SNUtil.writeToChat("Biome WBM fetch overridden to "+UnderwaterIslandsFloorBiome.biomeName);
	    	}/*
	   		if (Vector3.Distance(dmg.target.transform.position, bkelpBaseGeoCenter) <= 60 && !dmg.target.FindAncestor<Vehicle>()) {
	   			b.setValue(BKelpBaseBiome.biomeName);
	    		b.lockValue();
	   		}*/
	    	else if (CrashZoneSanctuaryBiome.instance.isInBiome(b.position)) {
	    		b.setValue(CrashZoneSanctuaryBiome.biomeName);
	    		b.lockValue();
	    		//if (BiomeBase.logBiomeFetch)
	    		//	SNUtil.writeToChat("Biome WBM fetch overridden to "+UnderwaterIslandsFloorBiome.biomeName);
	    	}
	    }
	    
	    public static void getSwimSpeed(DIHooks.SwimSpeedCalculation ch) {
	    	if (Player.main.motorMode != Player.MotorMode.Dive)
	    		return;
	    	//SNUtil.writeToChat("Get swim speed, was "+f+", has="+LiquidBreathingSystem.instance.hasLiquidBreathing());
	    	if (LiquidBreathingSystem.instance.hasLiquidBreathing())
	    		ch.setValue(ch.getValue()-0.1F); //was 0.25
	    	if (WorldUtil.isInDRF(Player.main.transform.position))
	    		ch.setValue(ch.getValue()*0.5F);
	    }
	    
	    public static float getSeaglideSpeed(float f) { //1.45 by default
	    	if (SeaToSeaMod.fastSeaglideCheatActive)
	    		return 40;
	    	//SNUtil.writeToChat("Get SG speed, was "+f+", has="+Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity));
			if (isHeldToolAzuritePowered()) {
	    		float bonus = 0.75F; //was 0.55 then 0.95
	    		float depth = Player.main.GetDepth();
	    		float depthFactor = depth <= 50 ? 1 : 1-((depth-50)/350F);
	    		if (depthFactor > 0) {
	    			f += bonus*depthFactor;
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
			if (NoCostConsoleCommand.main.fastBuildCheat)
				return 0.01F;
			if (!GameModeUtils.RequiresIngredients())
				return 0.2F;
			return Story.StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.auroraTerminal.key) ? 0.67F : 1F;
	    }
	    
	    public static float getVehicleConstructionSpeed(ConstructorInput inp, TechType made, float time) {
	    	if (Story.StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.auroraTerminal.key))
	    		time *= made == TechType.RocketBase ? 0.8F : 0.5F;
	    	else
	    		time *= made == TechType.Seamoth ? 2F : 1.5F;
	    	return time;
	    }
	    
	    public static float getRocketConstructionSpeed(float time) {
	    	time *= Story.StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.auroraTerminal.key) ? 0.8F : 1.6F;
	    	return time;
	    }
	    
	    public static bool getFabricatorTime(TechType recipe, out float time) {
	    	//bool ret = CraftData.GetCraftTime(recipe, out time);
	    	
	    	bool ret = CraftData.craftingTimes.TryGetValue(recipe, out time);
	    	if (!ret) {
	    		time = 0;
	    		return false;
	    	}
	    	
	    	if (Story.StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.auroraTerminal.key)) {
	    		time *= (float)MathUtil.linterpolate(time, 1, 2, 1, 0.5, true);
	    		time = Mathf.Min(time, 10);
	    	}
	    	else {
	    		time *= 1.5F;
	    	}
	    	return true;
	    }
	    
	    public static float getPropulsionCannonForce(PropulsionCannon prop) {
	    	float ret = prop.attractionForce;
	    	if (isHeldToolAzuritePowered())
	    		ret *= 3;
	    	float temp = WaterTemperatureSimulation.main.GetTemperature(Player.main.transform.position);
	    	if (temp >= 100)
	    		ret *= Mathf.Max(0.04F, 1F/((temp-99)/50F));
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
		    		GameObject door = ObjectUtil.getChildObject(rb.gameObject, "Starship_doors_manual_01/Starship_doors_automatic");
		    		Rigidbody rb2 = door.EnsureComponent<Rigidbody>();
		    		rb2.copyObject(rb);
	    			UnityEngine.Object.Destroy(s.GetComponent<StarshipDoorLocked>()); //need to do directly since removecomponent calls destroyImmediate, and this is an anim call
	    			UnityEngine.Object.Destroy(s);
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
		    		rb2.velocity = MainCamera.camera.transform.forward*30F+Vector3.up*7.5F;
		    		rb2.angularVelocity = MathUtil.getRandomVectorAround(Vector3.zero, 2.5F);
		    		FlyingDoor fd = door.EnsureComponent<FlyingDoor>();
		    		fd.Invoke("solidify", 0.05F);
		    		fd.Invoke("thump", 0.15F);
	    		}
	    	}
	    }
	    
	    private class FlyingDoor : MonoBehaviour {
	    	
	    	private static readonly SoundManager.SoundData impactSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "doorhit", "Sounds/doorhit.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
	    	
	    	void solidify() {
	    		foreach (Collider c in GetComponentsInChildren<Collider>()) {
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
	    		ch.value *= (ch.isMass ? 6 : 4);
	    }
	    
	    public static bool isHeldToolAzuritePowered() {
	    	if (Inventory.main == null)
	    		return false;
	    	Pickupable held = Inventory.main.GetHeld();
	    	if (!held || !held.gameObject)
	    		return false;
	    	EnergyMixin e = held.gameObject.GetComponent<EnergyMixin>();
	    	if (!e)
	    		return false;
	    	return e.battery != null && Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity);
	    }
	    
	    public static void onThingInO2Area(OxygenArea a, Collider obj) {
	    	if (obj.gameObject.FindAncestor<Player>() == Utils.GetLocalPlayerComp()) {
		    	float o2ToAdd = Math.Min(a.oxygenPerSecond*Time.deltaTime, Player.main.GetOxygenCapacity()-Player.main.GetOxygenAvailable());
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
	    	switch(p.GetTechType()) {
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
			try
			{
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
			finally
			{
				UnityEngine.Object.Destroy(tc.gameObject);
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
	    		pos = VoidSpikesBiome.instance.getPDALocation()+VoidSpikesBiome.voidEndpoint500m-VoidSpikesBiome.end500m;//VoidSpikesBiome.voidEndpoint500m;
	    	}
	    	if (Player.main != null && VoidSpikesBiome.instance.isInBiome(Player.main.transform.position) && !VoidSpikesBiome.instance.isInBiome(pos) && Vector3.Distance(Player.main.transform.position, pos) > 2) {
	    		pos += VoidSpikesBiome.end500m-VoidSpikesBiome.voidEndpoint500m;
	    	}
	    	return pos;
	    }
	   
		public static void doEnvironmentalDamage(TemperatureDamage dmg) {
	   		EnvironmentalDamageSystem.instance.tickTemperatureDamages(dmg);
	 	}
   
		public static void recalculateDamage(DIHooks.DamageToDeal dmg) {
	   		//if (type == DamageType.Acid && dealer == null && target.GetComponentInParent<SeaMoth>() != null)
	   		//	return 0;
	   		//SNUtil.writeToChat(dmg.target.name);
	   		Player p = dmg.target.FindAncestor<Player>();
	   		if (p != null) {
	   			if (dmg.type == DamageType.Heat && Vector3.Distance(p.transform.position, mountainBaseGeoCenter) <= 27) {
	   				dmg.setValue(0);
	   				return;
	   			}
	   			bool seal;
	   			bool reinf;
	   			bool flag = C2CItems.hasSealedOrReinforcedSuit(out seal, out reinf);
	   			if (!reinf && dmg.type == DamageType.Heat && WaterTemperatureSimulation.main.GetTemperature(p.transform.position) > 270) {
	   				dmg.setValue(dmg.getAmount()*1.25F);
	   				return;
	   			}
	   			else if (flag) {
	   				if ((dmg.type == DamageType.Poison || dmg.type == DamageType.Acid || dmg.type == DamageType.Electrical) && dmg.dealer != Player.main.gameObject) {
	   					string biome = WaterBiomeManager.main.GetBiome(p.transform.position, false);
	   					bool brine = biome != null && biome.Contains("LostRiver") && (biome.Contains("Lake") || biome.Contains("Stream") || biome.Contains("Water"));
	   					if (brine) {
	   						dmg.setValue(dmg.getAmount() * (seal ? 0.5F : 0.75F));
	   					}
	   					else {
		   					dmg.setValue(dmg.getAmount() * (seal ? 0.2F : 0.4F));
		   					dmg.setValue(dmg.getAmount() - (seal ? 10 : 7.5F));
	   					}
		   			}
	   			}
	   		}
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
	   			if (!p && Vector3.Distance(dmg.target.transform.position, bkelpBaseGeoCenter) <= 60 && !dmg.target.FindAncestor<Vehicle>()) {
	   				dmg.setValue(0);
	   			}
	   		}
	   		if (dmg.type == DamageType.Normal && VanillaBiomes.VOID.isInBiome(dmg.target.transform.position)) {
	   			SeaMoth sm = dmg.target.FindAncestor<SeaMoth>();
	   			if (sm && !InventoryUtil.vehicleHasUpgrade(sm, C2CItems.voidStealth.TechType))
	   				dmg.setValue(dmg.getAmount()*1.5F);
	   		}
	   		GlowKelpTag tag = dmg.target.FindAncestor<GlowKelpTag>();
	   		if (tag && (dmg.type == DamageType.Poison || !ObjectUtil.isFarmedPlant(dmg.target))) {
	   			dmg.setValue(0);
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
    
	    public static void onItemPickedUp(Pickupable p, Exosuit prawn, bool isKnife) {
	    	AvoliteSpawner.instance.cleanPickedUp(p);
	    	FCSIntegrationSystem.instance.modifyPeeperFood(p);
	    	LifeformScanningSystem.instance.onObjectSeen(p.gameObject, true);
	    	TechType tt = p.GetTechType();
	    	if (tt == CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType) {
	    		Story.StoryGoal.Execute("Azurite", Story.GoalType.Story);
	   			bool seal;
	   			bool reinf;
	   			if (prawn || !C2CItems.hasSealedOrReinforcedSuit(out seal, out reinf)) {
	   				LiveMixin lv = prawn ? prawn.liveMixin : Player.main.gameObject.GetComponentInParent<LiveMixin>();
	    			float dmg = lv.maxHealth*(SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.3F : 0.2F);
	    			if (Vector3.Distance(p.transform.position, Azurite.mountainBaseAzurite) <= 8)
	    				dmg *= 0.75F;
	    			if (prawn)
	    				dmg *= 0.04F; //do about 2% damage
					lv.TakeDamage(dmg, Player.main.gameObject.transform.position, DamageType.Electrical, p.gameObject);
				}
	    	}
	    	else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType) {
	    		Story.StoryGoal.Execute("Platinum", Story.GoalType.Story);
	    		HashSet<DeepStalkerTag> set = WorldUtil.getObjectsNearWithComponent<DeepStalkerTag>(p.transform.position, 60);
				foreach (DeepStalkerTag c in set) {
					if (!c.currentlyHasPlatinum() && !c.GetComponent<WaterParkCreature>()) {
						float chance = Mathf.Clamp01(1F-Vector3.Distance(c.transform.position, p.transform.position)/90F);
						if (UnityEngine.Random.Range(0F, 1F) <= chance)
							c.triggerPtAggro(prawn ? prawn.gameObject : Player.main.gameObject);
					}
				}
	    	}
	    	else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType) {
	    		Story.StoryGoal.Execute("PressureCrystals", Story.GoalType.Story);
	    	}
	    	else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType) {
	    		Story.StoryGoal.Execute("Avolite", Story.GoalType.Story);
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
	    	else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType && VanillaBiomes.ILZ.isInBiome(Player.main.transform.position)) {
	    		Story.StoryGoal.Execute("Iridium", Story.GoalType.Story);
	    		bool reinf;
	    		bool seal;
	    		C2CItems.hasSealedOrReinforcedSuit(out seal, out reinf);
	    		if (!prawn && !reinf) {
		    		LiveMixin lv = Player.main.gameObject.GetComponentInParent<LiveMixin>();
		    		float dmg = 40+(WaterTemperatureSimulation.main.GetTemperature(Player.main.transform.position)-90)/3;
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
	    	else if (tt == CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType) {
	    		p.GetComponent<NanocarbonTag>().reset();
	    		Story.StoryGoal.Execute("Nanocarbon", Story.GoalType.Story);
	    	}
	    }
    
	    public static float getReachDistance() {
	    	if (skipRaytrace || Player.main.GetVehicle())
	    		return 2;
	    	if ((Player.main.transform.position-lostRiverCachePanel).sqrMagnitude <= 100)
	    		return 4F;
	    	if (VoidSpikesBiome.instance.isInBiome(Player.main.transform.position))
	    		return 3.5F;
	    	return 2;
	    }
	    
	    public static bool checkTargetingSkip(bool orig, Transform obj) {
	    	if (skipRaytrace)
	    		return orig;
	    	if (!obj || !obj.gameObject)
	    		return orig;
	    	PrefabIdentifier id = obj.gameObject.FindAncestor<PrefabIdentifier>();
	    	if (!id)
	    		return orig;
	    	//SNUtil.log("Checking targeting skip of "+id+" > "+id.ClassId);
	    	if (id.ClassId == "b250309e-5ad0-43ca-9297-f79e22915db6" && Vector3.Distance(Player.main.transform.position, lrpowerSealSetpieceCenter) <= 8) { //to allow to hit the things inside the mouth
	    		//SNUtil.writeToChat("Is lr setpiece");
	    		return true;
	    	}
	    	if (VoidSpike.isSpike(id.ClassId) && VoidSpikesBiome.instance.isInBiome(obj.position)) {
	    		//SNUtil.writeToChat("Is void spike");
	    		return true;
	    	}
	    	return orig;
	    }
	    
	    public static EntityCell getEntityCellForInt3(Array3<EntityCell> data, Int3 raw, BatchCells batch) {
	    	int n = data.GetLength(0)/2;
	    	Int3 real = raw+new Int3(n, n, n);
	    	return data.Get(real);
	    }
	    
	     public static void setEntityCellForInt3(Array3<EntityCell> data, Int3 raw, EntityCell put, BatchCells batch) {
	    	int n = data.GetLength(0)/2;
	    	Int3 real = raw+new Int3(n, n, n);
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
	    	if (over != TechType.None && over != c.unlockTechType) {
	    		SNUtil.log("Blueprint @ "+c.gameObject.transform.ToString()+", previously "+c.unlockTechType+", found an override to "+over);
	    		GameObject go = ObjectUtil.createWorldObject(GenUtil.getOrCreateDatabox(over).TechType);
	    		go.transform.SetParent(c.transform.parent);
	    		go.transform.position = c.transform.position;
	    		go.transform.rotation = c.transform.rotation;
	    		go.transform.localScale = c.transform.localScale;
	    		UnityEngine.Object.Destroy(c.gameObject);
	    	}
	    	else if (c.unlockTechType == C2CItems.breathingFluid.TechType && c.transform.position.y < -500) { //clear old databox placement
	    		UnityEngine.Object.Destroy(c.gameObject);
	    	}
	    	else if (c.unlockTechType == C2CItems.liquidTank.TechType && c.transform.position.y > -500) {
	    		UnityEngine.Object.Destroy(c.gameObject);
	    	}
	    }
	    
	    public static GameObject interceptScannerTarget(GameObject original, ref PDAScanner.ScanTarget tgt) { //the GO is the collider, NOT the parent
	    	return original;
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
	    	placed.transform.position = owner.transform.position+Vector3.up*0.08F;
	    	placed.transform.rotation = owner.transform.rotation;
	    	UnityEngine.Object.Destroy(owner);
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
	    			calc.setValue(Mathf.Max(calc.getTemperature(), 90-bdist*6F));
	    	}
	    	string biome = EnvironmentalDamageSystem.instance.getBiome(calc.position);
	    	float poison = EnvironmentalDamageSystem.instance.getLRPoison(biome);
	    	if (poison > 0) { //make LR cold, down to -10C (4C is max water density point, but not for saltwater), except around vents
	    		float temp = calc.getTemperature();
	    		float cooling = poison*Mathf.Max(0, 3F-Mathf.Max(0, temp-30)/10F);
	    		calc.setValue(Mathf.Max(-10, temp-cooling));
	    	}
	    	else if (VanillaBiomes.COVE.isInBiome(calc.position))
	    		calc.setValue(calc.getTemperature()-10);
	    	if (biome == null || biome.ToLowerInvariant().Contains("void") && calc.position.y <= -50)
	    		calc.setValue(Mathf.Max(4, calc.getTemperature()+(calc.position.y+50)/20F)); //drop 1C per 20m below 50m, down to 4C around 550m
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
	    		calc.setValue(calc.getTemperature()+UnderwaterIslandsFloorBiome.instance.getTemperatureBoost(calc.getTemperature(), calc.position));
	    	calc.setValue(Mathf.Max(calc.getTemperature(), EnvironmentalDamageSystem.instance.getWaterTemperature(calc.position)));
	    	EjectedHeatSink.iterateHeatSinks(h => {
				if (h) {
					dist = Vector3.Distance(h.transform.position, calc.position);
					if (dist <= EjectedHeatSink.HEAT_RADIUS) {
						float f = 1F-(float)(dist/EjectedHeatSink.HEAT_RADIUS);
						//SNUtil.writeToChat("Found heat sink "+lb.transform.position+" at dist "+dist+" > "+f+" > "+(f*lb.getTemperature()));
						calc.setValue(Mathf.Max(calc.getTemperature(), f*h.getTemperature()));
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
	    	GameObject parent = pk.transform.parent.gameObject;
	    	PrefabIdentifier pi = parent.GetComponent<PrefabIdentifier>();
	    	switch(pi.classId) {
	    		case "0524596f-7f14-4bc2-a784-621fdb23971f":
	    		case "47027cf0-dca8-4040-94bd-7e20ae1ca086":
	    			pk.acceptKeyType = PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_White;
	    			break;
	    		case "fdb2bcbb-288a-40b6-bd7a-5585445eb43f":
	    			bool gate = Math.Abs(parent.transform.position.y+803.8) < 0.25;
	    			pk.acceptKeyType = gate ? PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Red : PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Orange;
	    			break;
	    	}
	    }
	    
	    public static void OnInspectableSpawn(InspectOnFirstPickup pk) {/*
	    	PrefabIdentifier pi = pk.gameObject.GetComponentInParent<PrefabIdentifier>();
	    	if (pi != null && (pi.ClassId == "7d19f47b-6ec6-4a25-9b28-b3fd7f5661b7" || pi.ClassId == "066e533d-f854-435d-82c6-b28ba59858e0")) {
	    		VFXFabricating fab = pi.gameObject.transform.Find("Model").gameObject.EnsureComponent<VFXFabricating>();
	    		fab.localMaxY = 0.1F;
	    		fab.localMinY = -0.1F;
	    	}*/
	    }
	    
	    public static GameObject getCrafterGhostModel(GameObject ret, TechType tech) {
	    	SNUtil.log("Crafterghost for "+tech+": "+ret);
	    	if (tech == TechType.PrecursorKey_Red || tech == TechType.PrecursorKey_White) {
	    		ret = ObjectUtil.lookupPrefab(CraftData.GetClassIdForTechType(tech));
	    		ret = UnityEngine.Object.Instantiate(ret);
	    		ret = ObjectUtil.getChildObject(ret, "Model");
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
	    	if (pi && pi.ClassId == "d79ab37f-23b6-42b9-958c-9a1f4fc64cfd" && Vector3.Distance(fcsWreckOpenableDoor, go.transform.position) <= 0.5) {
				new WreckDoorSwaps.DoorSwap(go.transform.position, "Handle").applyTo(go);
	    	}
	    	else if (pi && pi.ClassId == "055b3160-f57b-46ba-80f5-b708d0c8180e" && Vector3.Distance(fcsWreckBlockedDoor, go.transform.position) <= 0.5) {
				new WreckDoorSwaps.DoorSwap(go.transform.position, "Blocked").applyTo(go);
	    	}			
			else if (pi && pi.ClassId == VanillaCreatures.SEA_TREADER.prefab) {
				//go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
				go.EnsureComponent<C2CTreader>();
	    	}
			else if (pi && pi.ClassId == VanillaCreatures.CAVECRAWLER.prefab) {
	    		go.EnsureComponent<C2Crawler>();
	    	}
			else if (pi && pi.ClassId == "61ac1241-e990-4646-a618-bddb6960325b") {
	    		if (Vector3.Distance(go.transform.position, Player.main.transform.position) <= 40 && go.transform.position.y < -200) {
					PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.TreaderPooPrompt).key);
		    	}
	    	}/*
			else if (pi && pi.ClassId == "407e40cf-69f2-4412-8ab6-45faac5c4ea2") {
	    		
	    	}*//*
			else if (pi && pi.ClassId == "SeaVoyager") {
	    		go.EnsureComponent<C2CVoyager>();
	    	}*/
			else if (pi && pi.ClassId == "172d9440-2670-45a3-93c7-104fee6da6bc") {
	    		if (Vector3.Distance(go.transform.position, lostRiverCachePanel) < 2) {
	    			Renderer r = ObjectUtil.getChildObject(go, "Precursor_Lab_infoframe/Precursor_Lab_infoframe_glass").GetComponent<Renderer>();
	    			r.materials[0].SetColor("_Color", new Color(1, 1, 1, /*0.43F*/0.24F));
	    			r.materials[0].SetColor("_SpecColor", new Color(0.38F, 1, 0.52F, 1));
	    			RenderUtil.setGlossiness(r.materials[0], 50, 0, 0);
	    			GameObject copy = UnityEngine.Object.Instantiate(r.gameObject);
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
						   	HandReticle.main.SetInteractText(lrCachePanelLocaleKey);
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
	    	else if (pi && pi.ClassId == VanillaCreatures.GHOST_LEVIATHAN && pi.GetComponentInChildren<GhostLeviatanVoid>()) {
	    		***
	    	}*/
	    	else if (pi && auroraFires.Contains(pi.ClassId) && EnvironmentalDamageSystem.instance.isPositionInAuroraPrawnBay(pi.transform.position)) {
	    		blueAuroraPrawnFire(go);
	    	}
	    	else if (pi && pi.ClassId == "b86d345e-0517-4f6e-bea4-2c5b40f623b4" && pi.transform.parent && pi.transform.parent.name.Contains("ExoRoom_Weldable")) {
	    		GameObject inner = ObjectUtil.getChildObject(go, "Starship_doors_manual_01/Starship_doors_automatic");
	    		StarshipDoorLocked d = go.transform.parent.GetComponentInChildren<StarshipDoorLocked>();
	    		Renderer r = inner.GetComponentInChildren<Renderer>();
	    		RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/", new Dictionary<int, string>(){{0, "FireDoor"}, {1, "FireDoor"}});
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
				   	HandReticle.main.SetInteractText(prawnBayLocaleKey);
				   	HandReticle.main.SetTargetDistance(8);
				});
				Vector3 p1 = new Vector3(991.1F, 1F, -3.2F);
				Vector3 p2 = new Vector3(991.7F, 1F, -2.8F);/*
				GameObject rippleHolder = new GameObject("ripples");
				rippleHolder.transform.parent = go.transform.parent;
				rippleHolder.transform.localPosition = Vector3.zero;
				GameObject vent = ObjectUtil.lookupPrefab("5bbd405c-ca10-4da8-832b-87558c42f4dc");
				GameObject bubble = ObjectUtil.getChildObject(vent, "xThermalVent_Dark_Big/xBubbles");
				int n = 5;
				for (int i = 0; i <= n; i++) {
					GameObject p = UnityEngine.Object.Instantiate(bubble);
					p.transform.parent = rippleHolder.transform;
					p.transform.position = Vector3.Lerp(p1, p2, i/(float)n);
					p.GetComponentInChildren<Renderer>().materials[0].color = new Color(-8, -8, -8, 0.3F);
				}*/
				GameObject fire = ObjectUtil.createWorldObject("3877d31d-37a5-4c94-8eef-881a500c58bc");
				fire.transform.parent = go.transform;
				fire.transform.position = Vector3.Lerp(p1, p2, 0.5F)+new Vector3(1.3F, -0.05F, -1.7F);
				fire.transform.localScale = new Vector3(1.8F, 1, 1.8F);
				blueAuroraPrawnFire(fire);
				//ObjectUtil.removeComponent<VFXExtinguishableFire>(fire);
				LiveMixin lv = fire.GetComponent<LiveMixin>();
				lv.invincible = true;
				lv.data.maxHealth = 40000;
				lv.health = lv.data.maxHealth;
	    		return;
	    	}
	    	else if (pi && pi.ClassId == "58247109-68b9-411f-b90f-63461df9753a" && Vector3.Distance(deepDegasiTablet, go.transform.position) <= 0.2) {
	    		GameObject go2 = ObjectUtil.createWorldObject(C2CItems.brokenOrangeTablet.ClassID);
	    		go2.transform.position = go.transform.position;
	    		go2.transform.rotation = go.transform.rotation;
	    		UnityEngine.Object.Destroy(go);
	    		return;
	    	}
	    	else if (pi && pi.ClassId == "1c34945a-656d-4f70-bf86-8bc101a27eee") {
	    		go.EnsureComponent<C2CMoth>();
	    		go.EnsureComponent<BrightLightController>().setLightValues(120, 1.75F, 135, 180, 2.5F).setPowerValues(0.15F, 0.5F);
	    		go.EnsureComponent<SeamothTetherController>();
	    		//go.EnsureComponent<VoidSpikeLeviathanSystem.SeamothStealthManager>();
	    	}
	    	else if (pi && pi.ClassId == "ba3fb98d-e408-47eb-aa6c-12e14516446b") { //prawn
	    		TemperatureDamage td = go.EnsureComponent<TemperatureDamage>();
	    		td.minDamageTemperature = 350;
	    		td.baseDamagePerSecond = Mathf.Max(10, td.baseDamagePerSecond)*0.33F;
	    		td.onlyLavaDamage = false;
	    		td.InvokeRepeating("UpdateDamage", 1f, 1f);
	    		//ObjectUtil.removeComponent<ImmuneToPropulsioncannon>(go);
	    		go.EnsureComponent<BrightLightController>().setLightValues(120, 1.6F, 120, 150, 2.25F).setPowerValues(0.25F, 0.67F);
	    	}
        	else if (pi && pi.classId == "8b113c46-c273-4112-b7ef-65c50d2591ed") { //rocket
	    		go.EnsureComponent<C2CRocket>();
        	}
	    	else if (pi && pi.classId == "d4be3a5d-67c3-4345-af25-7663da2d2898") { //cuddlefish
	    		Pickupable p = go.EnsureComponent<Pickupable>();
	    		p.isPickupable = true;
	    		p.overrideTechType = TechType.Cutefish;
	    	}
	    	/*
	    	else if (pi && pi.ClassId == auroraStorageModule.prefabName && Vector3.Distance(auroraStorageModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraCyclopsModule.position;
	    		go.transform.rotation = auroraCyclopsModule.rotation;
	    	}
	    	else if (pi && pi.ClassId == auroraCyclopsModule.prefabName && Vector3.Distance(auroraCyclopsModule.position, go.transform.position) <= 0.2) {
	    		go.transform.position = auroraStorageModule.position;
	    		go.transform.rotation = auroraStorageModule.rotation;
	    	}*/
	    	else if (pi && pi.ClassId == "bc9354f8-2377-411b-be1f-01ea1914ec49" && Vector3.Distance(auroraRepulsionGunTerminal, go.transform.position) <= 0.2) {
	    		pi.GetComponent<StoryHandTarget>().goal = SeaToSeaMod.auroraTerminal;
	    	}
	    	else if (pi && pi.GetComponent<BlueprintHandTarget>()) {
	    		DamagedDataboxSystem.instance.onDataboxSpawn(go);
	    		go.EnsureComponent<ImmuneToPropulsioncannon>();
	    	}
	    	else if (pi && (pi.ClassId == VanillaResources.MAGNETITE.prefab || pi.ClassId == VanillaResources.LARGE_MAGNETITE.prefab)) {
	    		go.EnsureComponent<Magnetic>();
	    	}
	    	
	    	SubRoot sub = go.GetComponent<SubRoot>();
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
					UnityEngine.Object.Destroy(this);
	    		}
	    	}
	    	
	    }
	    
	    private class AuroraFireBluer : MonoBehaviour {
	    	
	    	private float age;
	    	
	    	void Update() {
	    		age += Time.deltaTime;
	    		bool flag = false;
		    	//SNUtil.log("Trying to blue prawn bay fire "+gameObject.name+" @ "+transform.position);
				foreach (Renderer r in GetComponentsInChildren<Renderer>(true)) {
		    		if (!r)
		    			continue;
		    		//SNUtil.log("Checking renderer "+r.name+" in "+r.gameObject.GetFullHierarchyPath());
					if (auroraPrawnFireColors.ContainsKey(r.name)) {
		    			foreach (Material m in r.materials) {
			    			//SNUtil.log("Setting color to "+auroraPrawnFireColors[r.name]);
							m.color = auroraPrawnFireColors[r.name];
							flag = true;
						}
		    		}
				}
				Light l = GetComponentInChildren<Light>();
				if (l)
					l.color = new Color(0.55F, 0.67F, 1F);
				if (flag && age >= 0.5F) {
					Light l2 = ObjectUtil.addLight(gameObject);
					l2.color = l.color;
					l2.intensity = 0.4F;
					l2.range = 32F;
					l2.name = "BlueFireLight";
				
		    		//SNUtil.log("Bluing complete. Destroying component.");
					UnityEngine.Object.Destroy(this);
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
	    	if (fp && ObjectUtil.isFarmedPlant(fp.gameObject) && WorldUtil.isPlantInNativeBiome(fp.gameObject)) {
	        	fp.fruitSpawnInterval = fpt.getBaseGrowthTime()/1.5F;
	        }
	    }
	    
	    class PrawnBayDoorTriggers : MonoBehaviour {
	    	
	    	internal GenericHandTarget hoverHint;
	    	
	    	internal StarshipDoor door;
	    	
	    	private bool wasOpen;
	    	
			public void UnlockDoor() {
	    		if (hoverHint)
	    			UnityEngine.Object.DestroyImmediate(hoverHint);
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
	    	sm.gameObject.EnsureComponent<BrightLightController>().recalculateModule();
	    	sm.gameObject.EnsureComponent<SeamothTetherController>().recalculateModule();
	    	if (added && slotID < sm.torpedoSilos.Length && tt == C2CItems.heatSinkModule.TechType) {
	    		sm.torpedoSilos[slotID].SetActive(true);
	    	}
	    }
	    
	    public static void updateCyclopsModules(SubRoot sm) {
	    	sm.gameObject.EnsureComponent<BrightLightController>().recalculateModule();
	    }
	    
	    public static void updatePrawnModules(Exosuit sm, int slotID, TechType tt, bool added) {
	    	sm.gameObject.EnsureComponent<BrightLightController>().recalculateModule();
	    }
	    
	    public static void useSeamothModule(SeaMoth sm, TechType tt, int slotID) {
			
	    }
	    
	    public static float getVehicleTemperature(Vehicle v) {
	    	return C2CMoth.getOverrideTemperature(v, WaterTemperatureSimulation.main.GetTemperature(v.transform.position));
	    }
    
	    public static bool isSpawnableVoid(string biome) {
	    	bool ret = VoidSpikeLeviathanSystem.instance.isSpawnableVoid(biome);
	    	if (ret && Player.main.IsSwimming() && !Player.main.GetVehicle() && VoidGhostLeviathansSpawner.main.spawnedCreatures.Count < 3 && !VoidSpikesBiome.instance.isInBiome(Player.main.transform.position)) {
	    		VoidGhostLeviathansSpawner.main.timeNextSpawn = Time.time-1;
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
	    		if (v && v is SeaMoth && VoidSpikesBiome.instance.isInBiome(ep.transform.position))
	    			VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth((SeaMoth)v, 40);
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
	    	C2CMoth cm = hit ? hit.GetComponent<C2CMoth>() : null;
	    	if (cm) {
	    		cm.onHitByLavaBomb(bomb);
	    	}
	    }
	    
	    public static void onAnchorPodExplode(ExplodingAnchorPodDamage dmg) {
	    	if (VoidSpikesBiome.instance.isInBiome(dmg.toDamage.transform.position) && dmg.toDamage.gameObject.FindAncestor<Player>()) {
	    		dmg.damageAmount *= 0.67F;
	    	}
	    }
	    
	    public static void onPlanktonActivated(PlanktonCloudTag cloud, Collider hit) {
	    	SeaMoth sm = hit.gameObject.FindAncestor<SeaMoth>();
	    	if (sm) {
	    		bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
	    		float amt = UnityEngine.Random.Range(hard ? 15 : 8, hard ? 25 : 15);
	    		if (VanillaBiomes.VOID.isInBiome(sm.transform.position))
	    			VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, amt);
	    	}
	    }
	    
	    public static void tickVoidBubbles(VoidBubbleSpawnerTick t) {
	    	double dist = VoidSpikesBiome.instance.getDistanceToBiome(t.player.transform.position, true)-VoidSpikesBiome.biomeVolumeRadius;
	    	float f = (float)MathUtil.linterpolate(dist, 50, 300, 0, 1, true);
	    	//SNUtil.writeToChat(dist.ToString("0.0")+" > "+f.ToString("0.0000"));
	    	t.spawnChance *= f;
	    }
	    
	    public static void tickVoidBubble(VoidBubbleTag t) {
	    	double dist = VoidSpikesBiome.instance.getDistanceToBiome(Player.main.transform.position, true)-VoidSpikesBiome.biomeVolumeRadius;
	    	if (dist <= 120) {
	    		t.fade(dist <= 80 ? 2 : (dist <= 80 ? 5 : 10));
	    	}
	    }
	    
	    public static ClipMapManager.Settings modifyWorldMeshSettings(ClipMapManager.Settings values) {
	    	ClipMapManager.LevelSettings baseline = values.levels[0];
	    	
	    	for (int i = 1; i < values.levels.Length-2; i++) {
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
	   		if (ox.GetComponent<Pickupable>().GetTechType() == C2CItems.liquidTank.TechType) {
	   			return ox.GetSecondsLeft()+"s fluid stored in supply tank";
	   		}
	   		return LanguageCache.GetOxygenText(ox.GetSecondsLeft());
		}
	   
		public static string getBatteryTooltip(Battery ox) {
	   		if (ox.GetComponent<Pickupable>().GetTechType() == C2CItems.liquidTank.TechType)
	   			return Mathf.RoundToInt(ox.charge)+"s fluid stored in primary tank";
	   		return Language.main.GetFormat<float, int, float>("BatteryCharge", ox.charge/ox.capacity, Mathf.RoundToInt(ox.charge), ox.capacity);
		}
	   
	   public static void onClickedVehicleUpgrades(VehicleUpgradeConsoleInput v) {
			if (v.docked || SeaToSeaMod.anywhereSeamothModuleCheatActive || GameModeUtils.currentEffectiveMode == GameModeOption.Creative)
				v.OpenPDA();
	   }
	   
		public static void onHoverVehicleUpgrades(VehicleUpgradeConsoleInput v) {
			HandReticle main = HandReticle.main;
		   	if (!v.docked && !SeaToSeaMod.anywhereSeamothModuleCheatActive && GameModeUtils.currentEffectiveMode != GameModeOption.Creative) {
				main.SetInteractText(dockUpgradesLocaleKey); //locale key
				main.SetIcon(HandReticle.IconType.HandDeny, 1f);
		   	}
			else if (v.equipment != null) {
				main.SetInteractText(v.interactText);
				main.SetIcon(HandReticle.IconType.Hand, 1f);
			}
		}
	    
	    public static void tryKnife(DIHooks.KnifeAttempt k) {
	    	LifeformScanningSystem.instance.onObjectSeen(k.target.gameObject, false);
	    	if (CraftData.GetTechType(k.target.gameObject) == TechType.BlueAmoeba) {
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
	    		if (hasPlat == lookingAtPlat)
	    			return def;
	    		else if (hasPlat)
	    			return cc.shinyTarget;
	    		else
	    			return def;
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
	    	if (s.GetComponent<DeepStalkerTag>() && UnityEngine.Random.Range(0F, 1F) <= 0.8)
	    		return false;
	    	if (s.GetComponent<WaterParkCreature>() && !PDAScanner.complete.Contains(TechType.StalkerTooth))
	    		return false;
	    	return s.LoseTooth();
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
				if (!SeaToSeaMod.checkConditionAndShowPDAAndVoicelogIfNot(ExplorationTrackerPages.instance.isFullyComplete(false), ExplorationTrackerPages.INCOMPLETE_PDA, PDAMessages.Messages.NeedFinishExploreTrackerMessage)) {
					ExplorationTrackerPages.instance.showAllPages();
					return;
				}
			}
			if (!FinalLaunchAdditionalRequirementSystem.instance.checkIfScannedAllLifeforms()) {
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
	    	if (VoidSpikesBiome.instance.getDistanceToBiome(ch.position) <= VoidSpikesBiome.biomeVolumeRadius+225)
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
	    	float f = Mathf.Clamp01((ch.panel.maxDepth-effectiveDepth)/ch.panel.maxDepth);
	    	//SNUtil.writeToChat(depth+" > "+effectiveDepth+" > "+f+" > "+ch.panel.depthCurve.Evaluate(f));
	    	return ch.panel.depthCurve.Evaluate(f)*ch.panel.GetSunScalar();
	    }
	    
	    public static float getModuleFireCost(DIHooks.ModuleFireCostCheck ch) {
	    	bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
	    	if (hard)
	    		ch.value *= 1.5F;
	    	if (ch.module == TechType.SeamothSonarModule)
	    		ch.value *= hard ? 8/3F : 4/3F;
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
		}
	    
	    public static void interceptBulkheadLaserCutter(DIHooks.BulkheadLaserCutterHoverCheck ch) {
	    	if (!hasLaserCutterUpgrade())
	    		ch.refusalLocaleKey = "Need_laserCutterBulkhead_Chit";
	    }
	    
	    public static bool hasLaserCutterUpgrade() {
	    	return Story.StoryGoalManager.main.completedGoals.Contains(SeaToSeaMod.laserCutterBulkhead.goal.key);
	    }
	    
		public static void onKnifed(GameObject go) {
	    	if (CraftData.GetTechType(go) == TechType.BlueAmoeba)
	    		DIHooks.fireKnifeHarvest(go, new Dictionary<TechType, int>{{CraftingItems.getItem(CraftingItems.Items.AmoeboidSample).TechType, 1}});
		}
	    
	    public static void interceptItemHarvest(DIHooks.KnifeHarvest h) {
	    	if (h.drops.Count > 0) {
	    		if (h.objectType == C2CItems.kelp.TechType) {
	    			GlowKelpTag tag = h.hit.FindAncestor<GlowKelpTag>();
	    			h.drops[h.defaultDrop] = 2;
	    			float f = tag.isFarmed() ? 0 : 0.05F;
	    			WaterPark wp = tag.getACU();
	    			if (wp && wp.GetComponentInChildren<PurpleHolefishTag>())
	    				f = 0.015F;
	    			if (f > 0 && UnityEngine.Random.Range(0F, 1F) <= f)
	    				h.drops[CustomEgg.getEgg(SeaToSeaMod.purpleHolefish.TechType).TechType] = 1;
	    		}
		    	if (ObjectUtil.isFarmedPlant(h.hit) && WorldUtil.isPlantInNativeBiome(h.hit)) {
		        	h.drops[h.defaultDrop] = h.drops[h.defaultDrop]*2;
		        }
	    	}
	    }
	    
	    public static void onReaperGrab(ReaperLeviathan r, Vehicle v) {
	    	if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) && KnownTech.Contains(TechType.BaseUpgradeConsole) && !KnownTech.Contains(TechType.SeamothElectricalDefense)) {
		       	KnownTech.Add(TechType.SeamothElectricalDefense);
	    		SNUtil.triggerTechPopup(TechType.SeamothElectricalDefense);
		    }
	    }
	    
	    public static bool chargerConsumeEnergy(IPowerInterface pi, float amt, out float consumed, Charger c) {
	    	if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) && (c is PowerCellCharger || c.GetType().Name.Contains("FCS")))
	    		amt *= 1.5F;
	    	return pi.ConsumeEnergy(amt, out consumed);
	    }
	    
	    public static void tickScannerCamera(MapRoomCamera cam) {
	    	Vector3 campos = cam.transform.position;
	    	if (VoidSpikesBiome.instance.getDistanceToBiome(campos, true) < 200) {
	    		float time = DayNightCycle.main.timePassedAsFloat;
	    		if (time > nextCameraEMPTime) {
		    		float d = UnityEngine.Random.Range(96F, 150F);
		    		Vector3 pos = campos+cam.transform.forward*d;
		    		pos = MathUtil.getRandomVectorAround(pos, 45);
		    		pos = campos+((pos-campos).setLength(d));
		    		VoidSpikeLeviathanSystem.instance.spawnEMPBlast(pos);
		    		nextCameraEMPTime = time+UnityEngine.Random.Range(1.2F, 2.5F);
	    		}
	    	}
	    	float temp = EnvironmentalDamageSystem.instance.getWaterTemperature(campos);
	    	if (temp >= 100) {
	    		float amt = 5*(1+(temp-100)/100F);
	    		cam.liveMixin.TakeDamage(amt*Time.deltaTime, campos, DamageType.Heat);
	    	}
	    	if (!cam.dockingPoint) {
		    	float leak = EnvironmentalDamageSystem.instance.getLRPowerLeakage(cam.gameObject);
		    	if (leak >= 0) {
		    		cam.energyMixin.ConsumeEnergy(leak*Time.deltaTime*0.5F);
		    	}
	    	}
	    }
	    
	    public static float getCrushDamage(CrushDamage dmg) {
	    	float f = 1;
	    	if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
	    		float ratio = dmg.GetDepth()/dmg.crushDepth;
	    		if (ratio > 1) {
	    			f += Mathf.Pow(ratio, 4)-1; //so at 1700 with a limit of 1300 it is ~3x as much damage; at 1200 with a 900 limit it is 3.2x, at 900 with 500 it is 10.5x
	    			ratio = (dmg.GetDepth()-900)/300F; //add another +33% per 100m over 900m
	    			if (ratio > 0)
	    				f += ratio;
	    		} //net result: 1700 @ 1300 = 5.6x, 1200 @ 900 = 2.8x, 900 @ 500 = 7x, 300 @ 200 = 3.3x
	    	}
	    	return dmg.damagePerCrush*f;
	    }
		
		internal static void isItemMapRoomDetectable(ESHooks.ResourceScanCheck rt) {
	    	if (rt.resource.techType == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType) {
	    		rt.isDetectable = PDAScanner.complete.Contains(rt.resource.techType) || Story.StoryGoalManager.main.completedGoals.Contains("Precursor_LavaCastle_Log2"); //mentions lava castle
	    	}
	    	else if (rt.resource.techType == CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType) {
	    		rt.isDetectable = PDAScanner.complete.Contains(rt.resource.techType) || PDAManager.getPage("sunbeamdebrishint").isUnlocked();
	    	}
	    	else if (scanToScannerRoom.Contains(rt.resource.techType)) {
	    		rt.isDetectable = PDAScanner.complete.Contains(rt.resource.techType);
	    	}
	    	else if (rt.resource.techType == SeaToSeaMod.mushroomBioFragment.TechType) {
	    		rt.isDetectable = SNUtil.getFragmentScanCount(rt.resource.techType) > SeaToSeaMod.mushroomBioFragment.getFragmentCount()-2;
	    	}
	    	else if (rt.resource.techType == SeaToSeaMod.geyserCoral.TechType) {
	    		rt.isDetectable = SNUtil.getFragmentScanCount(rt.resource.techType) > SeaToSeaMod.geyserCoral.getFragmentCount()-4;
	    	}
	    	if (rt.resource.GetComponent<Drillable>()) {
	    		rt.isDetectable = Story.StoryGoalManager.main.completedGoals.Contains("OnConstructExosuit") || KnownTech.knownTech.Contains(AqueousEngineeringMod.grinderBlock.TechType);
	    	}
		}
	    
	    static void onVehicleEnter(Vehicle v, Player ep) {
	    	if (v is SeaMoth) {
	    		VoidSpikesBiome.instance.onSeamothEntered((SeaMoth)v, ep);
	    	}
	    }
	    
	    public static void getCompassDepthLevel(DIHooks.DepthCompassCheck ch) {
	    	if (skipCompassCalc)
	    		return;
	    	if (VoidSpikeLeviathanSystem.instance.isVoidFlashActive(true)) {
	    		ch.value = VoidSpikeLeviathanSystem.instance.getRandomDepthForDisplay();
	    		ch.crushValue = 1000-ch.value;
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
	    	float netForward = Mathf.Max(0, input.z) * v.forwardForce + Mathf.Max(0, -input.z) * v.backwardForce;
	    	float inputFracX = Mathf.Pow(Mathf.Abs(input.x/input.magnitude), 0.75F);
	    	float inputFracY = Mathf.Pow(Mathf.Abs(input.y/input.magnitude), 0.75F);
	    	float inputFracZ = Mathf.Pow(Mathf.Abs(input.z/input.magnitude), 0.75F);
	    	float origX = Mathf.Abs(input.x) * v.sidewardForce;
	    	float origY = Mathf.Abs(input.y * v.verticalForce);
	    	float ret = netForward*inputFracZ+origX*inputFracX+origY*inputFracY; //multiply each component by its component of the input vector rather than a blind sum
	    	//SNUtil.writeToChat("Input vector "+input+" > speeds "+orig.ToString("00.0000")+" & "+ret.ToString("00.0000"));
	    	return ret;
	    }
	    
	    public static void onPlayerRespawned(Survival s, Player ep, bool post) {
	    	if (post) {
	    		bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
	    		s.water = Mathf.Max(hard ? 5 : 15, waterToRestore);
	    		s.food = Mathf.Max(hard ? 5 : 15, foodToRestore);
	    	}
	    	else {
	    		waterToRestore = s.water;
	    		foodToRestore = s.food;
	    		EnvironmentalDamageSystem.instance.resetCooldowns();
	    	}
	    }
	    
	    public static void onItemsLost() {
	    	foreach (InventoryItem ii in ((IEnumerable<InventoryItem>)Inventory.main.container)) {
	    		if (ii != null && ii.item && ii.item.GetTechType() == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType) {
	    			ii.item.destroyOnDeath = true;
	    		}
	    	}
	    }
	    
	    public static void onSelfScan() {
	    	PDAMessages.Messages msg = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? PDAMessages.Messages.LiquidBreathingSelfScanHard : PDAMessages.Messages.LiquidBreathingSelfScanEasy;
	    	if (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(msg).key)) {
	    		PDAManager.getPage("liqbrefficiency").unlock();
	    	}
	    }
	    
	    public static void onScanComplete(PDAScanner.EntryData data) {
	    	LifeformScanningSystem.instance.onScanComplete(data);
	    }
	    
	    public static void onDataboxTooltipCalculate(BlueprintHandTarget tgt) {
	    	LiveMixin lv = tgt.GetComponent<LiveMixin>();
	    	if (lv && lv.health < lv.maxHealth) {
		    	HandReticle.main.SetInteractText(needRepairDataboxLocaleKey);
				HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
	    	}
	    }
	    
	    public static bool onDataboxClick(BlueprintHandTarget tgt) { //return true to prevent use
	    	if (tgt.used)
	    		return true;
	    	LiveMixin lv = tgt.GetComponent<LiveMixin>();
	    	return lv && lv.health < lv.maxHealth;
	    }
	    
	    public static void applyGeyserFilterBuildability(DIHooks.BuildabilityCheck check) {
	    	if (VoidSpikesBiome.instance.isInBiome(Player.main.transform.position)) {
	    		check.placeable = false;
	    		return;
	    	}
			if (Builder.constructableTechType == SeaToSeaMod.geyserFilter.TechType) {
	    		check.placeable = !check.placeOn && GeyserFilterLogic.findGeyser(Builder.GetGhostModel().transform.position);
	    		check.ignoreSpaceRequirements = check.placeable;
				//check.ignoreSpaceRequirements = true;
		   	}
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
	    	if (!orig)
	    		return false;
	    	return canFCSDrillOperate(drill);
	    }
	    
	    private static float lastDrillDepletionTime = -1;
	    
	    public static bool canFCSDrillOperate(MonoBehaviour drill) {
	    	//SNUtil.writeToChat("Drill "+drill+" @ "+drill.transform.position+" is trying to mine: "+orig);
	    	bool ret = DrillDepletionSystem.instance.hasRemainingLife(drill);
	    	if (!ret) {
	    		float time = DayNightCycle.main.timePassedAsFloat;
	    		if (time-lastDrillDepletionTime >= 1) {
	    			lastDrillDepletionTime = time;
	    			SNUtil.writeToChat("Drill in "+WorldUtil.getRegionalDescription(drill.transform.position, true)+" has depleted the local resources.");
		    		Component com = drill.GetComponent(FCSIntegrationSystem.instance.getFCSDrillOreManager());
		    		if (com) {
			    		PropertyInfo p = FCSIntegrationSystem.instance.getFCSDrillOreManager().GetProperty("AllowedOres", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			    		p.SetValue(com, new List<TechType>{});
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
	    	if (tt == TechType.Nickel)
	    		return Story.StoryGoalManager.main.completedGoals.Contains("Nickel") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.4F);
	    	if (tt == TechType.MercuryOre)
	    		return Story.StoryGoalManager.main.completedGoals.Contains("Mercury") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.2F);
	    	if (tt == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType)
	    		return Story.StoryGoalManager.main.completedGoals.Contains("Iridium") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.2F);
	    	if (tt == TechType.Kyanite)
	    		return Story.StoryGoalManager.main.completedGoals.Contains("Kyanite") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.25F);
	    	if (tt == TechType.Sulphur)
	    		return Story.StoryGoalManager.main.completedGoals.Contains("Sulfur") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.5F);
	    	if (tt == TechType.UraniniteCrystal)
	    		return Story.StoryGoalManager.main.completedGoals.Contains("Uranium") && (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.5F);
	    	return true;
	    }
	    
	    public static Action<int, int> cleanupFCSContainer(Action<int, int> notify, MonoBehaviour drill, Dictionary<TechType, int> dict) {
	    	if (dict.ContainsKey(TechType.None)) {
	    		SNUtil.writeToChat("Removed TechType.None from drill inventory");
	    		dict.Remove(TechType.None);
	    	}
	    	int removed = 0;
	    	if (dict.ContainsKey(TechType.MercuryOre) && !Story.StoryGoalManager.main.completedGoals.Contains("Mercury")) {
	    		removed += dict[TechType.MercuryOre];
	    		dict.Remove(TechType.MercuryOre);
	    	}
	    	if (dict.ContainsKey(TechType.Nickel) && !Story.StoryGoalManager.main.completedGoals.Contains("Nickel")) {
	    		removed += dict[TechType.Nickel];
	    		dict.Remove(TechType.Nickel);
	    	}
	    	if (dict.ContainsKey(TechType.Kyanite) && !Story.StoryGoalManager.main.completedGoals.Contains("Kyanite")) {
	    		removed += dict[TechType.Kyanite];
	    		dict.Remove(TechType.Kyanite);
	    	}
	    	if (dict.ContainsKey(TechType.Sulphur) && !Story.StoryGoalManager.main.completedGoals.Contains("Sulfur")) {
	    		removed += dict[TechType.Sulphur];
	    		dict.Remove(TechType.Sulphur);
	    	}
	    	if (dict.ContainsKey(TechType.UraniniteCrystal) && !Story.StoryGoalManager.main.completedGoals.Contains("Uranium")) {
	    		removed += dict[TechType.UraniniteCrystal];
	    		dict.Remove(TechType.UraniniteCrystal);
	    	}
	    	if (dict.ContainsKey(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType) && !Story.StoryGoalManager.main.completedGoals.Contains("Iridium")) {
	    		removed += dict[CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType];
	    		dict.Remove(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType);
	    	}
	    	if (removed > 0) {
	    		DrillableResourceArea d = DrillDepletionSystem.instance.getMotherlode(drill);
	    		SNUtil.writeToChat("Removing "+removed+" progression-gated resources from drill yield @ "+d);
		    	for (int i = 0; i < removed; i++) {
	    			TechType tt = d == null ? (UnityEngine.Random.Range(0F, 1F) <= 0.3 ? TechType.Copper : TechType.Titanium) : getRandomValidMotherlodeDrillYield(d);
		    		if (dict.ContainsKey(tt))
		    			dict[tt] = dict[tt]+1;
		    		else
		    			dict[tt] = 1;
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
	    	FCSIntegrationSystem.instance.manageDrunkenness(pi);
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
	    	foreach (var obj in cart) {
	    		TechType tt = (TechType)pi.GetValue(obj);
	    		if (tt == adding)
	    			return -1;
	    	}
	    	return origLimit;
	    }
	    
	    public static bool isTeleporterFunctional(bool orig, MonoBehaviour teleporter) {
	    	//SNUtil.writeToChat("Testing teleporter "+teleporter+" @ "+teleporter.transform.position);
	    	if (!orig)
	    		return false;
	    	return FCSIntegrationSystem.instance.checkTeleporterFunction(teleporter);
	    }
	    
	    public static float getCurrentGeneratorPower(float orig, MonoBehaviour generator) {
	    	float sp = DayNightCycle.main.dayNightSpeed*2;
	    	return sp*1.2F*FCSIntegrationSystem.instance.getCurrentGeneratorPowerFactor(generator);
	    }
	    
	    public static void onMeteorImpact(GameObject meteor, Pickupable drop) {
	    	if (!PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.MeteorPrompt).key)) {
	    		Story.StoryGoal.Execute(METEOR_GOAL, Story.GoalType.Story);
	    	}
	    }
	    
	    public static void buildDisplayMonitorButton(MonoBehaviour screen, uGUI_ItemIcon icon) {
	    	icon.transform.localScale = new Vector3(0.5F, 0.45F, 1);
	    	GameObject grid = ObjectUtil.getChildObject(screen.gameObject, "Canvas/Screens/MainScreen/ActualScreen/MainGrid");
	    	UnityEngine.UI.GridLayoutGroup grp = grid.GetComponent<UnityEngine.UI.GridLayoutGroup>();
	    	grp.cellSize = new Vector2(100, 90);
	    }
	    
	    public static bool isStorageVisibleToDisplayMonitor(bool skip, StorageContainer sc) {
	    	//SNUtil.writeToChat("checking SC="+sc+": "+skip);
	    	skip |= (sc && sc.gameObject.FindAncestor<MapRoomFunctionality>());
	    	skip |= (sc && sc.GetComponent<BioprocessorLogic>());
	    	skip |= (sc && sc.GetComponent<Planter>());
	    	return skip;
	    }
	    
	    public static void mergeDeathrunRecipeChange(TechType tt, TechData td) {
	    	TechData real = RecipeUtil.getRecipe(tt);
	    	if (real == null) {
	    		SNUtil.log("Discarding deathrun "+tt+" recipe, as there is no vanilla recipe");
	    		return;
	    	}
	    	SNUtil.log("Integrating deathrun recipe change: "+tt+" = "+RecipeUtil.toString(td)+" into "+RecipeUtil.toString(real));
	    	Dictionary<TechType, int> cost = RecipeUtil.getIngredientsDict(real);
	    	foreach (Ingredient i in td.Ingredients) {
	    		if (cost.ContainsKey(i.techType)) {
	    			cost[i.techType] = Math.Max(cost[i.techType], i.amount);
	    		}
	    	}
	    	RecipeUtil.modifyIngredients(tt, i => { i.amount = cost[i.techType]; return false;});
	    }
	    
	    public static void mergeDeathrunFragmentScanCount(TechType tt, int amt) {
	    	PDAHandler.EditFragmentsToScan(tt, Math.Max(amt, ReikaKalseki.Reefbalance.ReefbalanceMod.getScanCountOverride(tt)));
	    }
	}
}
