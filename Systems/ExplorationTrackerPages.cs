﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;

using Story;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using UnityEngine;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea
{
	public class ExplorationTrackerPages : IStoryGoalListener {
		
		public static readonly ExplorationTrackerPages instance = new ExplorationTrackerPages();
		
		private readonly Dictionary<TrackerPages, TrackerPage> pages;
		
		private float lastUpdate = -1;
		
		public readonly Vector3 pod6Base = new Vector3(338.5F, -110, 286.5F);
		
		internal static readonly string INCOMPLETE_PDA = null;//"unfinishedexplore";
		
		private ExplorationTrackerPages() {
			pages = new Dictionary<TrackerPages, TrackerPage>();
			
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
			
	    	StoryHandler.instance.addListener(this);
	    	CustomLocaleKeyDatabase.registerKey("EncyPath_Findings", SeaToSeaMod.miscLocale.getEntry("TrackerPage").getField<string>("category"));
	    	
	    	TrackerPage p = addPage(TrackerPages.DEGASI1, new PositionTrigger(POITeleportSystem.instance.getPosition("degasi1"), 70));
	    	p.addFinding("pda", Finding.fromEncy("JellyPDARoom2Desk")).addFinding("databox", Finding.fromUnlock(TechType.HighCapacityTank)).addFinding("water", Finding.fromUnlock(TechType.BaseFiltrationMachine)).addFinding("breathcharge", Finding.fromEncy(C2CItems.rebreatherCharger.getPDAPage().id)).addFinding("azurite", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite)));
	    	
	    	p = addPage(TrackerPages.JELLYSHROOM, new BiomeTrigger(VanillaBiomes.JELLYSHROOM));
	    	p.addFinding("mushroom", Finding.fromScan(TechType.SnakeMushroom)).addFinding("seamothdepth", Finding.fromStory("seamothdepthchit1")).addFinding("magnetite", Finding.fromScan(TechType.Magnetite)).addFinding("degasi", Finding.fromTracker(TrackerPages.DEGASI1));
	    	
	    	p = addPage(TrackerPages.POD3, new StoryTrigger("OnPlayRadioGrassy25"));
	    	p.addFinding("databox", Finding.fromUnlock(TechType.Compass)).addFinding("cavehint", Finding.fromStory("KelpCaveHint")).addFinding("geyser", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.LathingDrone)));
	    	
	    	p = addPage(TrackerPages.POD19, new PositionTrigger(new Vector3(-808.72F, -299.31F, -872.53F), 150));
	    	p.addFinding("databox", Finding.fromUnlock(TechType.HighCapacityTank)).addFinding("pda", Finding.fromEncy("LifepodKeenLog")).addFinding("cycfire", Finding.fromUnlock(TechType.CyclopsFireSuppressionModule)).addFinding("console", Finding.fromUnlock(TechType.BaseUpgradeConsole)).addFinding("nanowrap", Finding.fromUnlock(C2CItems.bandage)).addFinding("cache", Finding.fromStory("Precursor_SparseReefCache_DataDownload1")).addFinding("vent", Finding.fromScan(TechType.PrecursorSurfacePipe));
	    	
	    	p = addPage(TrackerPages.POD17, new PositionTrigger(new Vector3(-515.56F, -98.79F, -56.18F), 80));
	    	p.addFinding("pda", Finding.fromEncy("LifepodSeaglide")).addFinding("seamoth", Finding.fromEncy("Seamoth")).addFinding("terminal", Finding.fromStory("Story_AuroraConsole1")).addFinding("jelly", Finding.fromTracker(TrackerPages.JELLYSHROOM)).addFinding("room", Finding.fromStory(C2CProgression.instance.getLocationGoal("OZZY_FORK_DEEP_ROOM")));
	    	
	    	p = addPage(TrackerPages.POD6, new PositionTrigger(pod6Base, 80));
	    	p.addFinding("wreck", Finding.fromStory(C2CProgression.instance.getLocationGoal("EAST_GRASS_WRECK"))).addFinding("pda", Finding.fromEncy("LifepodCrashZone2")).addFinding("seacrown", Finding.fromScan(TechType.SeaCrown)).addFinding("salve", Finding.fromScan(C2CItems.healFlower));
	    	
	    	p = addPage(TrackerPages.SOUTHGRASS, new PositionTrigger(POITeleportSystem.instance.getPosition("stepcave"), 150));
	    	p.addFinding("wreck", Finding.fromStory(C2CProgression.instance.getLocationGoal("SOUTH_GRASS_WRECK"))).addFinding("stepcave", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.MicroFilter).TechType)).addFinding("salve", Finding.fromScan(C2CItems.healFlower));
	    	
	    	p = addPage(TrackerPages.KOOSH, new BiomeTrigger(VanillaBiomes.KOOSH));
	    	p.addFinding("pda", Finding.fromEncy("Lifepod2")).addFinding("mushkoosh", Finding.fromEncy("ency_"+SeaToSeaMod.geyserCoral.ClassID)).addFinding("mercury", Finding.fromScan(TechType.MercuryOre)).addFinding("reinf", Finding.fromUnlock(TechType.ReinforcedDiveSuit)).addFinding("arch", Finding.fromStory(C2CProgression.instance.getLocationGoal("KOOSH_ARCH")));
	    	
	    	p = addPage(TrackerPages.DUNECENTRAL, new BiomeTrigger(VanillaBiomes.DUNES));
	    	p.addFinding("shroom", Finding.fromScan(Ecocean.EcoceanMod.glowShroom)).addFinding("azurite", Finding.fromStory("Azurite")).addFinding("suit", Finding.fromUnlock(C2CItems.sealSuit)).addFinding("vent", Finding.fromScan(TechType.PrecursorSurfacePipe)).addFinding("arch", Finding.fromTracker(TrackerPages.DUNEARCH)).addFinding("emperor", Finding.fromEncy("SeaEmperorBaby").setVisible("SeaEmperorBabiesSpawnedOutsideOfPrisonAquarium"));
	    	if (hard)
	    		p.addFinding("reaper", Finding.fromScan(TechType.ReaperLeviathan));
	    	
	    	p = addPage(TrackerPages.MOUNTAINS, new ProgressionTrigger(ep => BiomeBase.getBiome(ep.transform.position) == VanillaBiomes.MOUNTAINS && ep.transform.position.x > C2CHooks.mountainCenter.x+100 && ep.transform.position.y < -50));
	    	p.addFinding("pod", Finding.fromTracker(TrackerPages.MOUNTAINPOD)).addFinding("azurite", Finding.fromStory("Azurite")).addFinding("pyropod", Finding.fromStory("Pyropod")).addFinding("debris", Finding.fromStory("SunbeamDebris")).addFinding("magnetite", Finding.fromScan(TechType.Magnetite)).addFinding("vent", Finding.fromScan(TechType.PrecursorSurfacePipe)).addFinding("emperor", Finding.fromEncy("SeaEmperorBaby").setVisible("SeaEmperorBabiesSpawnedOutsideOfPrisonAquarium"));
	    	if (hard)
	    		p.addFinding("reaper", Finding.fromScan(TechType.ReaperLeviathan));
	    	
	    	p = addPage(TrackerPages.METEOR, new PositionTrigger(POITeleportSystem.instance.getPosition("meteor").setY(-375), 160));
	    	p.addFinding("meteor", Finding.fromScan(Auroresource.AuroresourceMod.dunesMeteor)).addFinding("cache", Finding.fromStory("Precursor_Cache_DataDownload3"));
	    	
	    	p = addPage(TrackerPages.UNDERISLANDS, new BiomeTrigger(VanillaBiomes.UNDERISLANDS));
	    	p.addFinding("glass", Finding.fromTracker(TrackerPages.GLASSFOREST)).addFinding("piezo", Finding.fromScan(Ecocean.EcoceanMod.piezo)).addFinding("wreck", Finding.fromStory(FallingGlassForestWreck.STORY_TAG)).addFinding("farmer", Finding.fromUnlock(AqueousEngineering.AqueousEngineeringMod.farmerBlock)).addFinding("databox", Finding.fromUnlock(TechType.CyclopsFireSuppressionModule)).addFinding("room", Finding.fromStory(C2CProgression.instance.getLocationGoal("UNDERISLANDS_BLOCKED_ROOM"))).addFinding("geogel", Finding.fromScan(SeaToSeaMod.gelFountain));
	    	
	    	p = addPage(TrackerPages.BKELPTRENCH, new TrackerPageAnyFindingsTrigger(TrackerPages.BKELPTRENCH));
	    	p.addFinding("pod1", Finding.fromEncy("bkelpbase")).addFinding("pda", Finding.fromEncy("bkelpbase2")).addFinding("chit", Finding.fromStory(SeaToSeaMod.bioProcessorBoost.goal)).addFinding("databox", Finding.fromUnlock(C2CItems.depth1300));//TODO .addFinding("caves", Finding.(??));
	    	
	    	p = addPage(TrackerPages.NBKELP, new ProgressionTrigger(ep => BiomeBase.getBiome(ep.transform.position) == VanillaBiomes.BLOODKELPNORTH && ep.transform.position.y < -400));
	    	p.addFinding("pod2", Finding.fromEncy("Lifepod1")).addFinding("cache", Finding.fromStory("Precursor_Cache_DataDownload2")).addFinding("nest", Finding.fromScan(SeaToSeaMod.bkelpBumpWorm)).addFinding("emperor", Finding.fromEncy("SeaEmperorBaby").setVisible("SeaEmperorBabiesSpawnedOutsideOfPrisonAquarium"));
	    	if (hard)
	    		p.addFinding("levi", Finding.fromScan(TechType.GhostLeviathan));
	    	
	    	p = addPage(TrackerPages.LOSTRIVER, new BiomeTrigger(VanillaBiomes.LOSTRIVER));
	    	p.addFinding("drf", Finding.fromTracker(TrackerPages.DRF)).addFinding("cache", Finding.fromUnlock(C2CItems.treatment)).addFinding("dragon", Finding.fromScan(TechType.PrecursorSeaDragonSkeleton)).addFinding("skull", Finding.fromScan(TechType.HugeSkeleton)).addFinding("seal", Finding.fromUnlock(C2CItems.powerSeal)).addFinding("pda", Finding.fromEncy("lrpowerseal")).addFinding("sulfur", Finding.fromStory("GrabSulfur")).addFinding("arch", Finding.fromStory(C2CProgression.instance.getLocationGoal("LR_ARCH"))).addFinding("coral", Finding.fromScan(C2CItems.brineCoral)).addFinding("cove", Finding.fromBiome(VanillaBiomes.COVE)).addFinding("calcite", Finding.fromStory("Calcite"));
	    	if (hard)
	    		p.addFinding("levi", Finding.fromScan(TechType.GhostLeviathanJuvenile));
	    	
	    	p = addPage(TrackerPages.DRF, new StoryTrigger("Precursor_LostRiverBase_Log1"));
	    	p.addFinding("terminal1", Finding.fromStory("Precursor_LostRiverBase_DataDownload1")).addFinding("egg", Finding.fromScan(TechType.PrecursorLostRiverLabEgg)).addFinding("damage", Finding.fromStory("Precursor_LostRiverBase_DataDownload3")).addFinding("tablet", Finding.fromUnlock(TechType.PrecursorKey_White)).addFinding("cycheat", Finding.fromUnlock(C2CItems.cyclopsHeat)).addFinding("disease", Finding.fromStory("Precursor_LostRiverBase_Log3")).addFinding("warper", Finding.fromScan(TechType.PrecursorWarper));
	    	
	    	p = addPage(TrackerPages.LAVACASTLE, new StoryTrigger("Precursor_LavaCastle_Log1")); //1 is the entry into the castle, Precursor_LavaCastle_Log2 is the lava castle hint
	    	p.addFinding("kyanite", Finding.fromStory("Kyanite")).addFinding("tablet", Finding.fromUnlock(TechType.PrecursorKey_Blue)).addFinding("ion", Finding.fromUnlock(TechType.PrecursorIonBattery)).addFinding("tap", Finding.fromUnlock(AqueousEngineering.AqueousEngineeringMod.atpTapBlock));
	    	
	    	p = addPage(TrackerPages.ILZ, new BiomeTrigger(VanillaBiomes.ILZ));
	    	p.addFinding("castle", Finding.fromTracker(TrackerPages.LAVACASTLE)).addFinding("azurite", Finding.fromStory("ILZAzurite")).addFinding("kyanite", Finding.fromScan(TechType.Kyanite)).addFinding("mushroom", Finding.fromScan(Ecocean.EcoceanMod.lavaShroom)).addFinding("obsidian", Finding.fromStory("Obsidian")).addFinding("pit", Finding.fromScan(Auroresource.AuroresourceMod.lavaPitCenter));
	    	
	    	p = addPage(TrackerPages.CRAG, new BiomeTrigger(VanillaBiomes.CRAG));
	    	p.addFinding("pod7", Finding.fromEncy("LifepodRandom")).addFinding("scoop", Finding.fromUnlock(Ecocean.EcoceanMod.planktonScoop)).addFinding("arch", Finding.fromStory(C2CProgression.instance.getLocationGoal("CRAG_ARCH"))).addFinding("emperor", Finding.fromEncy("SeaEmperorBaby").setVisible("SeaEmperorBabiesSpawnedOutsideOfPrisonAquarium"));
	    	
	    	p = addPage(TrackerPages.FLOATISLAND, new ProgressionTrigger(ep => ep.transform.position.y >= 15 && (ep.transform.position-POITeleportSystem.instance.getPosition("islandwreck")).sqrMagnitude <= 150*150));
	    	p.addFinding("degasi", Finding.fromTracker(TrackerPages.FLOATDEGASI)).addFinding("keen", Finding.fromEncy("RendezvousFloatingIsland")).addFinding("arch", Finding.fromStory(C2CProgression.instance.getLocationGoal("FLOATING_ARCH"))).addFinding("floater", Finding.fromScan(TechType.LargeFloater)).addFinding("tablet", Finding.fromStory("ScanFloatingIslandTablet"));
	    	
	    	p = addPage(TrackerPages.MOUNTAINISLAND, new ProgressionTrigger(ep => WorldUtil.isMountainIsland(ep.transform.position)));
	    	p.addFinding("gun", Finding.fromTracker(TrackerPages.GUN)).addFinding("arch", Finding.fromStory("PrecursorMountainTeleporterActive")).addFinding("beachpda", Finding.fromEncy("islandpda")).addFinding("campfire", Finding.fromStory("campfire").setOptional(true)).addFinding("cavepda", Finding.fromEncy("islandcave")).addFinding("battery", Finding.fromUnlock(C2CItems.t2Battery)).addFinding("purpletablets", /*Finding.fromEncy("PrecursorKey_Purple")*/Finding.fromStory("ScanMountainIslandTablet")).addFinding("tablet", Finding.fromUnlock(TechType.PrecursorKey_Red));
	    	if (hard)
	    		p.addFinding("plantalcove", Finding.fromStory(C2CProgression.instance.getLocationGoal("PLANT_ALCOVE")));
	    	
	    	p = addPage(TrackerPages.GUN, new StoryTrigger("Goal_BiomePrecursorGunUpper"));
	    	p.addFinding("cube", Finding.fromScan(TechType.PrecursorIonCrystal)).addFinding("denied", Finding.fromStory("Precursor_Gun_DisableDenied")).addFinding("jailbreak", Finding.fromStory(Auroresource.AuroresourceMod.laserCutterJailbroken));
	    	
	    	p = addPage(TrackerPages.FLOATDEGASI, new TrackerPageAnyFindingsTrigger(TrackerPages.FLOATDEGASI));
	    	p.addFinding("main", Finding.fromEncy("IslandsPDABase1Interior")).addFinding("outside", Finding.fromEncy("IslandsPDAExterior")).addFinding("return", Finding.fromEncy("IslandsPDABase1a")).addFinding("init", Finding.fromEncy("IslandsPDABase1bDesk"));
	    	
	    	p = addPage(TrackerPages.DUNEARCH, new PositionTrigger(POITeleportSystem.instance.getPosition("dunearch"), 120));
	    	p.addFinding("pda", Finding.fromEncy("dunearch")).addFinding("bioproc", Finding.fromEncy(C2CItems.processor.getPDAPage().id)).addFinding("liqbr", Finding.fromUnlock(C2CItems.breathingFluid));
	    	if (hard)
	    		p.addFinding("reaper", Finding.fromScan(TechType.ReaperLeviathan));
	    	
	    	p = addPage(TrackerPages.GRANDREEF, new BiomeTrigger(VanillaBiomes.GRANDREEF));
	    	p.addFinding("base", Finding.fromTracker(TrackerPages.DEGASIEND)).addFinding("platinum", Finding.fromStory("Platinum")).addFinding("poo", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes))).addFinding("sealed", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.SealFabric))).addFinding("bioproc", Finding.fromEncy(C2CItems.processor.getPDAPage().id)).addFinding("vent", Finding.fromScan(TechType.PrecursorSurfacePipe)).addFinding("emperor", Finding.fromEncy("SeaEmperorBaby").setVisible("SeaEmperorBabiesSpawnedOutsideOfPrisonAquarium"));
	    	if (hard)
	    		p.addFinding("levi", Finding.fromScan(TechType.GhostLeviathan));
	    	
	    	p = addPage(TrackerPages.MUSHTREE, new StoryTrigger(C2CProgression.instance.getLocationGoal("MUSHTREE")));
	    	p.addFinding("treepda", Finding.fromEncy("treepda")).addFinding("chit", Finding.fromStory(SeaToSeaMod.laserCutterBulkhead.goal)).addFinding("bacteria", Finding.fromEncy("ency_"+SeaToSeaMod.mushroomBioFragment.ClassID));
	    	
	    	p = addPage(TrackerPages.MUSHROOMS, new BiomeTrigger(VanillaBiomes.MUSHROOM));
	    	p.addFinding("initpda", Finding.fromEncy("rescuepdalog")).addFinding("khasar", Finding.fromEncy("Lifepod4")).addFinding("cyclops", Finding.fromEncy("cyclops")).addFinding("highwreck", Finding.fromUnlock(Auroresource.AuroresourceMod.meteorDetector)).addFinding("tree", Finding.fromTracker(TrackerPages.MUSHTREE)).addFinding("mushkoosh", Finding.fromScan(SeaToSeaMod.geyserCoral.TechType)).addFinding("vent", Finding.fromScan(TechType.PrecursorSurfacePipe)).addFinding("arch", Finding.fromStory(C2CProgression.instance.getLocationGoal("MUSHTREE_ARCH"))).addFinding("vasestrand", Finding.fromScan(Ecocean.EcoceanMod.mushroomVaseStrand));
	    	
	    	p = addPage(TrackerPages.MOUNTAINPOD, new TrackerPageAnyFindingsTrigger(TrackerPages.MOUNTAINPOD));
	    	p.addFinding("podpda", Finding.fromEncy("mountainpodearly")).addFinding("podpda2", Finding.fromEncy("mountainpodlate")).addFinding("mask", Finding.fromUnlock(C2CItems.rebreatherV2)).addFinding("stealth", Finding.fromUnlock(C2CItems.voidStealth)).addFinding("knife", Finding.fromUnlock(TechType.HeatBlade)).addFinding("battery", Finding.fromUnlock(AqueousEngineering.AqueousEngineeringMod.batteryBlock)).addFinding("basepda", Finding.fromEncy("mountaincave"));
	    	
	    	p = addPage(TrackerPages.TREADERPOD, new StoryTrigger("OnPlay"+SeaToSeaMod.treaderSignal.storyGate));
	    	p.addFinding("treader", Finding.fromScan(TechType.SeaTreader)).addFinding("platinum", Finding.fromStory("Platinum")).addFinding("pda", Finding.fromEncy("treaderpod")).addFinding("enzy", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.BioEnzymes))).addFinding("basepda", Finding.fromEncy("treadercave")).addFinding("databox", Finding.fromUnlock(TechType.VehicleHullModule2));
	    	
	    	p = addPage(TrackerPages.GLASSFOREST, new StoryTrigger(UnderwaterIslandsFloorBiome.instance.discoveryGoal));
	    	p.addFinding("databox", Finding.fromUnlock(C2CItems.liquidTank)).addFinding("speed", Finding.fromUnlock(C2CItems.speedModule)).addFinding("bioproc", Finding.fromUnlock(C2CItems.processor)).addFinding("deepvine", Finding.fromStory("DeepvineSamples"));
	    	if (hard)
	    		p.addFinding("levi", Finding.fromScan(TechType.GhostLeviathan));
	    	
	    	p = addPage(TrackerPages.CRASH, new BiomeTrigger(VanillaBiomes.CRASH));
	    	p.addFinding("pod4", Finding.fromEncy("LifepodDecoy")).addFinding("crashmesa", Finding.fromUnlock(TechType.VehicleHullModule3)).addFinding("stasis", Finding.fromUnlock(AqueousEngineering.AqueousEngineeringMod.stasisBlock)).addFinding("sanctuary", Finding.fromStory(CrashZoneSanctuaryBiome.instance.discoveryGoal)).addFinding("trailerbase", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.HeatSealant)));
	    	if (hard)
	    		p.addFinding("reaper", Finding.fromScan(TechType.ReaperLeviathan));
	    	
	    	p = addPage(TrackerPages.DEGASIEND, new PositionTrigger(C2CHooks.deepDegasiTablet, 80));
	    	p.addFinding("end", Finding.fromEncy("DeepPDA3")).addFinding("tablet", Finding.fromUnlock(TechType.PrecursorKey_Orange)).addFinding("rebreather", Finding.fromUnlock(C2CItems.rebreatherCharger));
	    	
	    	p = addPage(TrackerPages.VOID, new TrackerPageAnyFindingsTrigger(TrackerPages.VOID));
	    	p.addFinding("spikes", Finding.fromTracker(TrackerPages.VOIDSPIKES)).addFinding("destroy", Finding.fromEncy(VoidSpikesBiome.PDA_KEY)).addFinding("bubble", Finding.fromScan(Ecocean.EcoceanMod.voidBubble)).addFinding("databox", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.HullPlating)));
	    	
	    	p = addPage(TrackerPages.VOIDSPIKES, new StoryTrigger(VoidSpikesBiome.instance.discoveryGoal));
	    	p.addFinding("end", Finding.fromEncy(VoidSpikeWreck.PDA_KEY)).addFinding("items", Finding.fromStory("PressureCrystals"));//TODO .addFinding("levi", Finding.fromScan(SeaToSeaMod.voidSpikeLevi));
		}
		
		private TrackerPage addPage(TrackerPages pgs, ProgressionTrigger firstAppear) {
			string id = Enum.GetName(typeof(TrackerPages), pgs);
			XMLLocale.LocaleEntry e = SeaToSeaMod.trackerLocale.getEntry(id);
			if (e == null)
				throw new Exception("No tracker XML locale entry '"+id+"'");
			TrackerPage page = new TrackerPage(e);
			pages[pgs] = page;
			if (StoryHandler.instance == null)
				throw new Exception("Story handler not initialized yet!");
			StoryHandler.instance.registerTrigger(firstAppear, new DelayedEncyclopediaEffect(page.encyPage, 0.002F, 4, true));
			SNUtil.log("Added findings ency page "+page.encyPage);
			return page;
		}
		
		public string getEncyKey(TrackerPages pg) {
			return pages[pg].encyPage.id;
		}
		
		public int countFindings(TrackerPages pg, bool requireOptional) {
			return pages[pg].countCompleteFindings(requireOptional);
		}
		
		public bool isComplete(TrackerPages pg, bool requireOptional) {
			return pages[pg].isComplete(requireOptional);
		}
		
		public bool isFullyComplete(bool requireOptional) {
			foreach (TrackerPages p in Enum.GetValues(typeof(TrackerPages))) {
				if (!isComplete(p, requireOptional))
					return false;
			}
			return true;
		}
		
		internal void markAllDiscovered() {
			foreach (TrackerPages p in Enum.GetValues(typeof(TrackerPages))) {
				foreach (Finding f in pages[p].findings.Values) {
					f.trigger.markComplete();
				}
			}
		}
		
		internal void showAllPages() {
			foreach (TrackerPages p in Enum.GetValues(typeof(TrackerPages))) {
				pages[p].encyPage.unlock(false);
				if (!pages[p].isComplete(false))
					pages[p].encyPage.markUpdated(5);
			}
		}
		
		public void NotifyGoalComplete(string key) {
			
		}
		
		public void tick() {
			if (DayNightCycle.main.timePassedAsFloat-lastUpdate <= 0.5F)
				return;
			lastUpdate = DayNightCycle.main.timePassedAsFloat;
			foreach (TrackerPage p in pages.Values) {
				p.updateText();
			}
		}
	}
	
	public enum TrackerPages {
		POD3, //compass databox, geyser cave [nanolathing drones]; pod3 radio
		POD19, //highcap br, pda to floatisland, precursor cache, vehicle upgrade console, cyclops fire suppress; pod 19 radio
        POD17, //pda, jellyshroom page, seamoth fragment, large nearby wreck (gibberish console [Story_AuroraConsole1], that deep room (-645.6, -102.7, -16.2) prox); pod 17 prox
		POD6, //pda, seacrown, salvebush; pod6 prox
		SOUTHGRASS, //the upright wreck, stepcave
		KOOSH, //pod 12 [?], koosh caves (mercury, scan or collect), reinf suit, mushkoosh cave [scan 1 geyser coral]; koosh biome OR kooshcaveprompt
		DUNECENTRAL, //sealed suit databox, azurite, lumenshrooms; dunes biome
		METEOR, //meteor [scan], precursor cache [Precursor_Cache_DataDownload3]; meteor prox R=100
		UNDERISLANDS, //falling GF wreck [fire storygoal], autofarmer, prop gun blocked room [prox to -124.38, -200.69, 855 R=5], cylcops fire suppression; underislands biome
		BKELPTRENCH, //caves [?], pod 1 [], bkelp base (bioproc upgrade AND smdepth4 AND the pda); biome in bkelp trench and depth > 350m
		NBKELP, //pod 2 [Lifepod1], precursor cache [Precursor_Cache_DataDownload2], nest in the cave, ghost leviathan [hardmode?]; biome in N bkelp and depth > 400
		LOSTRIVER, //garg skull, powerseal, large sulfur; lostriver
		DRF, //story terminal [Precursor_LostRiverBase_DataDownload1], damage report [Precursor_LostRiverBase_DataDownload3], scan PrecursorLostRiverLabEgg, white tablet, kharaa progress [Precursor_LostRiverBase_Log3], cyclopsheat, scan PrecursorWarper, scan seadragon skeleton [PrecursorSeaDragonSkeleton]; first DRF pda message []
		LAVACASTLE, //kyanite, blue tablet, ion power, ATP tap; pda message
		ILZ,
		CRAG, //pod 7 [LifepodRandom], planktonscoop; crag biome
		FLOATISLAND, //degasi [FLOATDEGASI page], keen meeting site [RendezvousFloatingIsland], teleporter (-662.55, 5.50, -1064.35) R = 25, but Y > 0 && Y < 22.5; prox to floating island and above y=20
		MOUNTAINISLAND, //gun [GUN page], teleporter activation [PrecursorMountainTeleporterActive], beach PDA, cave cache [t2 battery recipe], red tablet; MENTION MOUNTAINBASE ["came from elsewhere"] in the beach PDA entry; prox to sunbeam site or has beach pda
		GUN, //gun [Precursor_Gun_DisableDenied (attempt disable)], ion cube, lasercutter jailbreak; first gun goal []
		JELLYSHROOM, //degasi base 1 [DEGASI1 page], magnetite (scan or collect), jellyshroom; jellyshroom biome
		MOUNTAINS, //pod 14 [MOUNTAINPOD page], magnetite (scan or collect), azurite, pyropods, sunbeam debris [collect sunbeam debris], reaper (hardmode)
				
		FLOATDEGASI, //main base [IslandsPDABase1Interior ("this planet won't cause us any new problems")], outside pda [IslandsPDAExterior], bart return [IslandsPDABase1a ("Shouldn't have gone so deep")], [IslandsPDABase1b (init)]; prox to base or any of the above conditions
		DEGASI1, //degasi base 1 [JellyPDARoom2Desk], water filter, breathcharge start, high cap DB, shaped azurite; proximity to degasi1 POI pos
		DEGASIEND, //DGR degasi base, their final PDA, rebreathercharger finish, orange tablet; DGR biome
		
		DUNEARCH, //dunearch wreck PDA, liqbr databox, bioproc pieces; pda prompt
		GRANDREEF, //sealed fabric, bioproc pieces, platinum, treaderpoo (digestive enzymes), ghost leviathan [hardmode?]; biome is grand reef
		MUSHROOMS, //; biome is mushrooms
		MUSHTREE, //rescue/salvage [pda] bacterial samples, laser cutter upgrade; first rescue/salvage
		MOUNTAINPOD, //mountain pod, recirc mask, mountain base [voidstealth AND thermoblade AND seabase battery]; prox to mountainpod or mountainbase
		TREADERPOD, //pod 9 pda [mention the path?] and databox, treaderbase; pod 9 radio
		GLASSFOREST, //wreck, deepvine samples, bioproc finish, ghost leviathan [hardmode?]; glass forest and y < -400
		CRASH, //pod4 [], trailerbase [databpx], sanctuary [eye flame core], crashmesa [databox]; crash zone biome
		VOID, //pod 15 pda and databox; void pod radio
		VOIDSPIKES, //voidspikes PDA, abyssoclase; void spikes biome
	}
	
	class TrackerPageAnyFindingsTrigger : ProgressionTrigger {
		
		public readonly TrackerPages page;
		
		public TrackerPageAnyFindingsTrigger(TrackerPages b) : base(ep => ExplorationTrackerPages.instance.countFindings(b, true) > 0) {
			page = b;
		}
		
		public override string ToString() {
			return "Tracker Page Any Findings For "+page;
		}
	}
	
	class TrackerPage {
		
		internal readonly PDAManager.PDAPage encyPage;
		
		internal readonly Dictionary<string, Finding> findings = new Dictionary<string, Finding>();
		
		private readonly XMLLocale.LocaleEntry locale;
		
		private bool hasUpdated = false;
		
		internal TrackerPage(XMLLocale.LocaleEntry e) {
			locale = e;
			encyPage = PDAManager.createPage("ency_findings_"+e.key, e.name, "", "Findings");
			encyPage.register();
		}
		
		internal TrackerPage addFinding(string key, FindingTrigger trigger) {
			findings[key] = new Finding(key, locale.getField<string>(key), trigger);
			return this;
		}
		
		internal void updateText() {
			encyPage.update(generatePDAContent(), true, hasUpdated);
			hasUpdated = true;
		}
		
		internal int countCompleteFindings(bool requireOptional) {
			int ret = 0;
			foreach (Finding f in findings.Values) {
				if ((f.trigger.isOptional && !requireOptional) || f.trigger.isTriggered.Invoke())
					ret++;
			}
			return ret;
		}
		
		internal bool isComplete(bool requireOptional) {
			return countCompleteFindings(requireOptional) >= findings.Count;
		}
		
		private string generatePDAContent() {
			string desc = SeaToSeaMod.miscLocale.getEntry("TrackerPage").pda+"\n";
			bool incomplete = false;
			bool any = false;
			foreach (Finding f in findings.Values) {
				if (!f.trigger.isVisible)
					continue;
				if (SeaToSeaMod.trackerShowAllCheatActive || (GameModeUtils.currentGameMode != GameModeOption.Creative && f.trigger.isTriggered.Invoke())) {
					desc += "\t •  "+f.desc+"\n\n";
					any = true;
				}
				else if (!f.trigger.isOptional || f.trigger.triggerMoreToExplore) {
					incomplete = true;
				}
			}
			if (!any)
				desc = "";
			if (incomplete)
				desc += string.Format("{1}<color=#FF9D14FF>{0}</color>", SeaToSeaMod.miscLocale.getEntry("TrackerPage").getField<string>("incomplete"), any ? "\n" : "");
			return desc;
		}
		
	}
	
	class Finding {
		
		internal readonly string key;
		internal readonly string desc;
		internal readonly FindingTrigger trigger;
		
		internal Finding(string k, string s, FindingTrigger f) {
			key = k;
			desc = s;
			trigger = f;
		}
		
		internal static Func<bool> combine(Func<bool> f1, Func<bool> f2) {
			return () => f1.Invoke() && f2.Invoke();
		}
		
		internal static FindingTrigger fromScan(ModPrefab pfb) {
			return fromScan(pfb.TechType);
		}
		
		internal static FindingTrigger fromScan(TechType tt) {
			return new FindingTrigger(() => PDAScanner.complete.Contains(tt), () => PDAScanner.complete.Add(tt));
		}
		
		internal static FindingTrigger fromUnlock(ModPrefab pfb) {
			return fromUnlock(pfb.TechType);
		}
		
		internal static FindingTrigger fromUnlock(TechType tt) {
			return new FindingTrigger(() => KnownTech.knownTech.Contains(tt), () => KnownTech.Add(tt));
		}
		
		internal static FindingTrigger fromEncy(string ency) {
			return new FindingTrigger(() => PDAEncyclopedia.entries.ContainsKey(ency), () => PDAEncyclopedia.Add(ency, false));
		}
		
		internal static FindingTrigger fromTracker(TrackerPages pg) { //do not overload to fromEncy because the pg may not be loaded yet
			return new FindingTrigger(() => PDAEncyclopedia.entries.ContainsKey(ExplorationTrackerPages.instance.getEncyKey(pg)), () => PDAEncyclopedia.Add(ExplorationTrackerPages.instance.getEncyKey(pg), false));
		}
		
		internal static FindingTrigger fromStory(string key) {
			return new FindingTrigger(() => StoryGoalManager.main.IsGoalComplete(key), () => StoryGoalManager.main.completedGoals.Add(key));
		}
		
		internal static FindingTrigger fromStory(StoryGoal g) {
			return fromStory(g.key);
		}
		
		internal static FindingTrigger fromBiome(BiomeBase bb) {
			return new FindingTrigger(() => BiomeDiscoverySystem.instance.isDiscovered(bb), () => BiomeDiscoverySystem.instance.forceDiscovery(bb));
		}
		
	}
	
	class FindingTrigger {
		
		internal readonly Func<bool> isTriggered;
		internal readonly Action markComplete;
		
		private Func<bool> visibleFunc;
		internal bool isOptional;
		internal bool triggerMoreToExplore;
		internal bool isVisible { get { return visibleFunc == null || visibleFunc.Invoke(); } }
		
		internal FindingTrigger(Func<bool> f, Action a) {
			isTriggered = f;
			markComplete = a;
		}
		
		internal FindingTrigger setOptional(bool triggerMoreExplore) {
			triggerMoreToExplore = triggerMoreExplore;
			isOptional = true;
			return this;
		}
		
		internal FindingTrigger setVisible(StoryGoal sg) {
			return setVisible(sg.key);
		}
		
		internal FindingTrigger setVisible(string goal) {
			return setVisible(() => StoryGoalManager.main.IsGoalComplete(goal));
		}
		
		internal FindingTrigger setVisible(Func<bool> f) {
			visibleFunc = f;
			return this;
		}
		
	}
		
}
	