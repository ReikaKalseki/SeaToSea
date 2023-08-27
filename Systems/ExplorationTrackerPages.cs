using System;
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
		
		private ExplorationTrackerPages() {
			pages = new Dictionary<TrackerPages, TrackerPage>();
			
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
			
	    	StoryHandler.instance.addListener(this);
	    	LanguageHandler.SetLanguageLine("EncyPath_Findings", SeaToSeaMod.miscLocale.getEntry("TrackerPage").getField<string>("category"));
	    	
	    	TrackerPage p = addPage(TrackerPages.DEGASI1, new PositionTrigger(POITeleportSystem.instance.getPosition("degasi1"), 70));
	    	p.addFinding("pda", Finding.fromEncy("JellyPDARoom2Desk")).addFinding("databox", Finding.fromUnlock(TechType.HighCapacityTank)).addFinding("water", Finding.fromUnlock(TechType.BaseFiltrationMachine)).addFinding("breathcharge", Finding.fromEncy(SeaToSeaMod.rebreatherCharger.getPDAPage().id)).addFinding("azurite", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType));
	    	
	    	p = addPage(TrackerPages.JELLYSHROOM, new BiomeTrigger(VanillaBiomes.JELLYSHROOM));
	    	p.addFinding("mushroom", Finding.fromScan(TechType.SnakeMushroom)).addFinding("magnetite", Finding.fromScan(TechType.Magnetite)).addFinding("degasi", Finding.fromTracker(TrackerPages.DEGASI1));
	    	
	    	p = addPage(TrackerPages.POD3, new StoryTrigger("OnPlayRadioGrassy25"));
	    	p.addFinding("databox", Finding.fromUnlock(TechType.Compass)).addFinding("geyser", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType));
	    	
	    	p = addPage(TrackerPages.POD19, new PositionTrigger(new Vector3(-808.72F, -299.31F, -872.53F), 50));
	    	p.addFinding("databox", Finding.fromUnlock(TechType.HighCapacityTank)).addFinding("pda", Finding.fromEncy("LifepodKeenLog")).addFinding("cache", Finding.fromStory("Precursor_SparseReefCache_DataDownload1"));
	    	
	    	p = addPage(TrackerPages.POD17, new PositionTrigger(new Vector3(-515.56F, -98.79F, -56.18F), 80));
	    	p.addFinding("pda", Finding.fromEncy("LifepodSeaglide")).addFinding("seamoth", Finding.fromEncy("Seamoth")).addFinding("terminal", Finding.fromStory("Story_AuroraConsole1")).addFinding("jelly", Finding.fromTracker(TrackerPages.JELLYSHROOM)).addFinding("room", Finding.fromStory(C2CHooks.OZZY_FORK_DEEP_ROOM_GOAL));
	    	
	    	p = addPage(TrackerPages.POD6, new PositionTrigger(pod6Base, 80));
	    	p.addFinding("pda", Finding.fromEncy("LifepodCrashZone2")).addFinding("seacrown", Finding.fromScan(TechType.SeaCrown)).addFinding("salve", Finding.fromScan(C2CItems.healFlower.TechType));
	    	
	    	p = addPage(TrackerPages.KOOSH, new BiomeTrigger(VanillaBiomes.KOOSH));
	    	p.addFinding("pda", Finding.fromEncy("Lifepod2")).addFinding("mushkoosh", Finding.fromEncy("ency_"+SeaToSeaMod.geyserCoral.ClassID)).addFinding("mercury", Finding.fromScan(TechType.MercuryOre)).addFinding("reinf", Finding.fromUnlock(TechType.ReinforcedDiveSuit));
	    	
	    	p = addPage(TrackerPages.DUNECENTRAL, new BiomeTrigger(VanillaBiomes.DUNES));
	    	p.addFinding("shroom", Finding.fromScan(Ecocean.EcoceanMod.glowShroom.TechType)).addFinding("azurite", Finding.fromStory("Azurite")).addFinding("suit", Finding.fromUnlock(C2CItems.sealSuit.TechType));
	    	
	    	p = addPage(TrackerPages.METEOR, new PositionTrigger(POITeleportSystem.instance.getPosition("meteor").setY(-375), 180));
	    	p.addFinding("meteor", Finding.fromScan(Auroresource.AuroresourceMod.dunesMeteor.TechType)).addFinding("cache", Finding.fromStory("Precursor_Cache_DataDownload3"));
	    	
	    	p = addPage(TrackerPages.UNDERISLANDS, new BiomeTrigger(VanillaBiomes.UNDERISLANDS));
	    	p.addFinding("piezo", Finding.fromScan(Ecocean.EcoceanMod.piezo.TechType)).addFinding("wreck", Finding.fromStory(FallingGlassForestWreck.STORY_TAG)).addFinding("farmer", Finding.fromUnlock(AqueousEngineering.AqueousEngineeringMod.farmerBlock.TechType)).addFinding("databox", Finding.fromUnlock(TechType.CyclopsFireSuppressionModule)).addFinding("room", Finding.fromStory(C2CHooks.UNDERISLANDS_BLOCKED_ROOM_GOAL));
	    	
	    	p = addPage(TrackerPages.BKELPTRENCH, new ProgressionTrigger(ep => BiomeBase.getBiome(ep.transform.position) == VanillaBiomes.BLOODKELP && ep.transform.position.y < -350));
	    	p.addFinding("pod1", Finding.fromEncy("bkelpbase")).addFinding("pda", Finding.fromEncy("bkelpbase2")).addFinding("chit", Finding.fromStory(SeaToSeaMod.bioProcessorBoost.goal)).addFinding("databox", Finding.fromUnlock(C2CItems.depth1300.TechType));//TODO .addFinding("caves", Finding.(??));
	    	
	    	p = addPage(TrackerPages.NBKELP, new ProgressionTrigger(ep => BiomeBase.getBiome(ep.transform.position) == VanillaBiomes.BLOODKELPNORTH && ep.transform.position.y < -400));
	    	p.addFinding("pod2", Finding.fromEncy("Lifepod1")).addFinding("cache", Finding.fromStory("Precursor_Cache_DataDownload2"));//TODO .addFinding("nest", Finding.(??));
	    	if (hard)
	    		p.addFinding("levi", Finding.fromScan(TechType.GhostLeviathan));
	    	
	    	p = addPage(TrackerPages.BONEFIELD, new ProgressionTrigger(isBonesField));
	    	p.addFinding("pda", Finding.fromEncy("lrpowerseal")).addFinding("skull", Finding.fromScan(TechType.HugeSkeleton)).addFinding("sulfur", Finding.fromStory("GrabSulfur")).addFinding("seal", Finding.fromUnlock(C2CItems.powerSeal.TechType));
	    	if (hard)
	    		p.addFinding("levi", Finding.fromScan(TechType.GhostLeviathanJuvenile));
	    	
	    	p = addPage(TrackerPages.DRF, new StoryTrigger("Precursor_LostRiverBase_Log1"));
	    	p.addFinding("terminal1", Finding.fromStory("Precursor_LostRiverBase_DataDownload1")).addFinding("damage", Finding.fromStory("Precursor_LostRiverBase_DataDownload3")).addFinding("tablet", Finding.fromUnlock(TechType.PrecursorKey_White)).addFinding("cycheat", Finding.fromUnlock(C2CItems.cyclopsHeat.TechType)).addFinding("disease", Finding.fromStory("Precursor_LostRiverBase_Log3")).addFinding("egg", Finding.fromScan(TechType.PrecursorLostRiverLabEgg)).addFinding("dragon", Finding.fromScan(TechType.PrecursorSeaDragonSkeleton)).addFinding("warper", Finding.fromScan(TechType.PrecursorWarper));
	    	
	    	p = addPage(TrackerPages.LAVACASTLE, new StoryTrigger("Precursor_LavaCastle_Log1")); //1 is the entry into the castle, Precursor_LavaCastle_Log2 is the lava castle hint
	    	p.addFinding("kyanite", Finding.fromStory("Kyanite")).addFinding("tablet", Finding.fromUnlock(TechType.PrecursorKey_Blue)).addFinding("ion", Finding.fromUnlock(TechType.PrecursorIonBattery)).addFinding("tap", Finding.fromUnlock(AqueousEngineering.AqueousEngineeringMod.atpTapBlock.TechType));
	    	
	    	p = addPage(TrackerPages.CRAG, new BiomeTrigger(VanillaBiomes.CRAG));
	    	p.addFinding("pod7", Finding.fromEncy("LifepodRandom")).addFinding("scoop", Finding.fromUnlock(Ecocean.EcoceanMod.planktonScoop.TechType));
	    	
	    	p = addPage(TrackerPages.FLOATISLAND, new ProgressionTrigger(ep => ep.transform.position.y >= 15 && (ep.transform.position-POITeleportSystem.instance.getPosition("islandwreck")).sqrMagnitude <= 150*150));
	    	p.addFinding("degasi", Finding.fromTracker(TrackerPages.FLOATDEGASI)).addFinding("keen", Finding.fromEncy("RendezvousFloatingIsland")).addFinding("arch", Finding.fromStory(C2CHooks.FLOATING_ARCH_GOAL));
	    	
	    	p = addPage(TrackerPages.MOUNTAINISLAND, new ProgressionTrigger(isMountainIsland));
	    	p.addFinding("gun", Finding.fromTracker(TrackerPages.GUN)).addFinding("beachpda", Finding.fromEncy("islandpda")).addFinding("cavepda", Finding.fromEncy("islandcave")).addFinding("arch", Finding.fromStory("PrecursorMountainTeleporterActive")).addFinding("battery", Finding.fromUnlock(C2CItems.t2Battery.TechType)).addFinding("tablet", Finding.fromUnlock(TechType.PrecursorKey_Red));
	    	
	    	p = addPage(TrackerPages.GUN, new StoryTrigger("Goal_BiomePrecursorGunUpper"));
	    	p.addFinding("cube", Finding.fromScan(TechType.PrecursorIonCrystal)).addFinding("denied", Finding.fromStory("Precursor_Gun_DisableDenied")).addFinding("jailbreak", Finding.fromStory(Auroresource.AuroresourceMod.laserCutterJailbroken));
	    	
	    	p = addPage(TrackerPages.FLOATDEGASI, new TrackerPageAnyFindingsTrigger(TrackerPages.FLOATDEGASI));
	    	p.addFinding("main", Finding.fromEncy("IslandsPDABase1Interior")).addFinding("outside", Finding.fromEncy("IslandsPDAExterior")).addFinding("return", Finding.fromEncy("IslandsPDABase1a")).addFinding("init", Finding.fromEncy("IslandsPDABase1b"));
	    	
	    	p = addPage(TrackerPages.DUNEARCH, new PositionTrigger(POITeleportSystem.instance.getPosition("dunearch"), 120));
	    	p.addFinding("pda", Finding.fromEncy("dunearch")).addFinding("bioproc", Finding.fromEncy(SeaToSeaMod.processor.getPDAPage().id)).addFinding("liqbr", Finding.fromUnlock(C2CItems.liquidTank.TechType));
	    	
	    	p = addPage(TrackerPages.GRANDREEF, new BiomeTrigger(VanillaBiomes.GRANDREEF));
	    	p.addFinding("pda", Finding.fromEncy("dunearch")).addFinding("platinum", Finding.fromStory("Platinum")).addFinding("bioproc", Finding.fromEncy(SeaToSeaMod.processor.getPDAPage().id)).addFinding("poo", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes).TechType)).addFinding("sealed", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.SealFabric).TechType));
	    	if (hard)
	    		p.addFinding("levi", Finding.fromScan(TechType.GhostLeviathan));
	    	
	    	p = addPage(TrackerPages.MUSHTREE, new StoryTrigger("rescuepdalog")); //khasar pda is Lifepod4
	    	p.addFinding("initpda", Finding.fromEncy("rescuepdalog")).addFinding("treepda", Finding.fromEncy("treepda")).addFinding("chit", Finding.fromStory(SeaToSeaMod.laserCutterBulkhead.goal)).addFinding("bacteria", Finding.fromEncy("ency_"+SeaToSeaMod.mushroomBioFragment.ClassID));
	    	
	    	p = addPage(TrackerPages.MOUNTAINPOD, new TrackerPageAnyFindingsTrigger(TrackerPages.MOUNTAINPOD));
	    	p.addFinding("podpda", Finding.fromEncy("mountainpodearly")).addFinding("podpda2", Finding.fromEncy("mountainpodlate")).addFinding("basepda", Finding.fromEncy("mountaincave")).addFinding("mask", Finding.fromUnlock(C2CItems.rebreatherV2.TechType)).addFinding("stealth", Finding.fromUnlock(C2CItems.voidStealth.TechType)).addFinding("knife", Finding.fromUnlock(TechType.HeatBlade)).addFinding("battery", Finding.fromUnlock(AqueousEngineering.AqueousEngineeringMod.batteryBlock.TechType));
	    	
	    	p = addPage(TrackerPages.TREADERPOD, new StoryTrigger(SeaToSeaMod.treaderSignal.storyGate));
	    	p.addFinding("treader", Finding.fromScan(TechType.SeaTreader)).addFinding("platinum", Finding.fromStory("Platinum")).addFinding("pda", Finding.fromEncy("treaderpod")).addFinding("enzy", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.BioEnzymes).TechType)).addFinding("basepda", Finding.fromEncy("treadercave")).addFinding("databox", Finding.fromUnlock(TechType.VehicleHullModule2));
	    	
	    	p = addPage(TrackerPages.GLASSFOREST, new ProgressionTrigger(ep => UnderwaterIslandsFloorBiome.instance.isInBiome(ep.transform.position) && ep.transform.position.y < -400));
	    	p.addFinding("databox", Finding.fromUnlock(C2CItems.breathingFluid.TechType)).addFinding("bioproc", Finding.fromUnlock(SeaToSeaMod.processor.TechType)).addFinding("deepvine", Finding.fromStory("DeepvineSamples"));
	    	if (hard)
	    		p.addFinding("levi", Finding.fromScan(TechType.GhostLeviathan));
	    	
	    	p = addPage(TrackerPages.CRASH, new BiomeTrigger(VanillaBiomes.CRASH));
	    	p.addFinding("pod4", Finding.fromEncy("LifepodDecoy")).addFinding("crashmesa", Finding.fromUnlock(TechType.VehicleHullModule3)).addFinding("sanctuary", Finding.fromScan(C2CItems.sanctuaryPlant.TechType)).addFinding("trailerbase", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.HeatSealant).TechType)).addFinding("rebreather", Finding.fromUnlock(SeaToSeaMod.rebreatherCharger.TechType));
	    	
	    	
	    	p = addPage(TrackerPages.DEGASIEND, new BiomeTrigger(VanillaBiomes.DEEPGRAND));
	    	p.addFinding("end", Finding.fromEncy("DeepPDA3")).addFinding("tablet", Finding.fromUnlock(TechType.PrecursorKey_Orange)).addFinding("rebreather", Finding.fromUnlock(SeaToSeaMod.rebreatherCharger.TechType));
	    	
	    	p = addPage(TrackerPages.VOID, new PositionTrigger(VoidSpikesBiome.signalLocation, 30));
	    	p.addFinding("destroy", Finding.fromEncy(VoidSpikesBiome.PDA_KEY)).addFinding("databox", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType)).addFinding("end", Finding.fromEncy(VoidSpikeWreck.PDA_KEY)).addFinding("items", Finding.fromStory("PressureCrystals"));//TODO .addFinding("levi", Finding.fromScan(SeaToSeaMod.voidSpikeLevi.TechType));
		}
		
		private bool isBonesField(Player ep) {
			string biome = WaterBiomeManager.main.GetBiome(ep.transform.position);
			if (string.IsNullOrEmpty(biome))
				return false;
			return biome.Contains("BonesField") && !biome.Contains("Corridor");
		}
		
		private bool isMountainIsland(Player ep) {
			return ep.transform.position.y > 0 && ((ep.transform.position-POITeleportSystem.instance.getPosition("sunbeamsite")).sqrMagnitude <= 2500 || VanillaBiomes.MOUNTAINS.isInBiome(ep.transform.position));
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
		
		internal string getEncyKey(TrackerPages pg) {
			return pages[pg].encyPage.id;
		}
		
		internal int countFindings(TrackerPages pg) {
			return pages[pg].countCompleteFindings();
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
	
	internal enum TrackerPages {
		POD3, //compass databox, geyser cave [nanolathing drones]; pod3 radio
		POD19, //highcap br, pda to floatisland, precursor cache; pod 19 radio
        POD17, //pda, jellyshroom page, seamoth fragment, large nearby wreck (gibberish console [Story_AuroraConsole1], that deep room (-645.6, -102.7, -16.2) prox); pod 17 prox
		POD6, //pda, seacrown, salvebush; pod6 prox
		KOOSH, //pod 12 [?], koosh caves (mercury, scan or collect), reinf suit, mushkoosh cave [scan 1 geyser coral]; koosh biome OR kooshcaveprompt
		DUNECENTRAL, //sealed suit databox, azurite, lumenshrooms; dunes biome
		METEOR, //meteor [scan], precursor cache [Precursor_Cache_DataDownload3]; meteor prox R=100
		UNDERISLANDS, //falling GF wreck [fire storygoal], autofarmer, prop gun blocked room [prox to -124.38, -200.69, 855 R=5], cylcops fire suppression; underislands biome
		BKELPTRENCH, //caves [?], pod 1 [], bkelp base (bioproc upgrade AND smdepth4 AND the pda); biome in bkelp trench and depth > 350m
		NBKELP, //pod 2 [Lifepod1], precursor cache [Precursor_Cache_DataDownload2], nest in the cave, ghost leviathan [hardmode?]; biome in N bkelp and depth > 400
		BONEFIELD, //garg skull, powerseal, large sulfur; bonefield biome
		DRF, //story terminal [Precursor_LostRiverBase_DataDownload1], damage report [Precursor_LostRiverBase_DataDownload3], scan PrecursorLostRiverLabEgg, white tablet, kharaa progress [Precursor_LostRiverBase_Log3], cyclopsheat, scan PrecursorWarper, scan seadragon skeleton [PrecursorSeaDragonSkeleton]; first DRF pda message []
		LAVACASTLE, //kyanite, blue tablet, ion power, ATP tap; pda message
		CRAG, //pod 7 [LifepodRandom], planktonscoop; crag biome
		FLOATISLAND, //degasi [FLOATDEGASI page], keen meeting site [RendezvousFloatingIsland], teleporter (-662.55, 5.50, -1064.35) R = 25, but Y > 0 && Y < 22.5; prox to floating island and above y=20
		MOUNTAINISLAND, //gun [GUN page], teleporter activation [PrecursorMountainTeleporterActive], beach PDA, cave cache [t2 battery recipe], red tablet; MENTION MOUNTAINBASE ["came from elsewhere"] in the beach PDA entry; prox to sunbeam site or has beach pda
		GUN, //gun [Precursor_Gun_DisableDenied (attempt disable)], ion cube, lasercutter jailbreak; first gun goal []
		JELLYSHROOM, //degasi base 1 [DEGASI1 page], magnetite (scan or collect), jellyshroom; jellyshroom biome
				
		FLOATDEGASI, //main base [IslandsPDABase1Interior ("this planet won't cause us any new problems")], outside pda [IslandsPDAExterior], bart return [IslandsPDABase1a ("Shouldn't have gone so deep")], [IslandsPDABase1b (init)]; prox to base or any of the above conditions
		DEGASI1, //degasi base 1 [JellyPDARoom2Desk], water filter, breathcharge start, high cap DB, shaped azurite; proximity to degasi1 POI pos
		DEGASIEND, //DGR degasi base, their final PDA, rebreathercharger finish, orange tablet; DGR biome
		
		DUNEARCH, //dunearch wreck PDA, liqbr databox, bioproc pieces; pda prompt
		GRANDREEF, //sealed fabric, bioproc pieces, platinum, treaderpoo (digestive enzymes), ghost leviathan [hardmode?]; biome is grand reef
		MUSHTREE, //rescue/salvage [pda] bacterial samples, laser cutter upgrade; first rescue/salvage
		MOUNTAINPOD, //mountain pod, recirc mask, mountain base [voidstealth AND thermoblade AND seabase battery]; prox to mountainpod or mountainbase
		TREADERPOD, //pod 9 pda [mention the path?] and databox, treaderbase; pod 9 radio
		GLASSFOREST, //wreck, deepvine samples, bioproc finish, ghost leviathan [hardmode?]; glass forest and y < -400
		CRASH, //pod4 [], trailerbase [databpx], sanctuary [eye flame core], crashmesa [databox]; crash zone biome
		VOID, //pod 15 pda and databox, voidspikes PDA, abyssoclase; void pod radio
	}
	
	class TrackerPageAnyFindingsTrigger : ProgressionTrigger {
		
		public readonly TrackerPages page;
		
		public TrackerPageAnyFindingsTrigger(TrackerPages b) : base(ep => ExplorationTrackerPages.instance.countFindings(b) > 0) {
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
		
		internal TrackerPage(XMLLocale.LocaleEntry e) {
			locale = e;
			encyPage = PDAManager.createPage("ency_findings_"+e.key, e.name, "", "Findings");
			encyPage.register();
		}
		
		internal TrackerPage addFinding(string key, Func<bool> trigger) {
			findings[key] = new Finding(key, locale.getField<string>(key), trigger);
			return this;
		}
		
		internal void updateText() {
			encyPage.update(generatePDAContent(), true);
		}
		
		internal int countCompleteFindings() {
			int ret = 0;
			foreach (Finding f in findings.Values) {
				if (f.isTriggered.Invoke())
					ret++;
			}
			return ret;
		}
		
		internal bool isComplete() {
			return countCompleteFindings() == findings.Count;
		}
		
		private string generatePDAContent() {
			string desc = SeaToSeaMod.miscLocale.getEntry("TrackerPage").pda+"\n";
			bool incomplete = false;
			bool any = false;
			foreach (Finding f in findings.Values) {
				if (f.isTriggered.Invoke()) {
					desc += "\t - "+f.desc+"\n\n";
					any = true;
				}
				else {
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
		
		internal readonly Func<bool> isTriggered;
		
		internal Finding(string k, string s, Func<bool> f) {
			key = k;
			desc = s;
			isTriggered = f;
		}
		
		internal static Func<bool> combine(Func<bool> f1, Func<bool> f2) {
			return () => f1.Invoke() && f2.Invoke();
		}
		
		internal static Func<bool> fromScan(TechType tt) {
			return () => PDAScanner.complete.Contains(tt);
		}
		
		internal static Func<bool> fromUnlock(TechType tt) {
			return () => KnownTech.knownTech.Contains(tt);
		}
		
		internal static Func<bool> fromEncy(string ency) {
			return () => PDAEncyclopedia.entries.ContainsKey(ency);
		}
		
		internal static Func<bool> fromTracker(TrackerPages pg) {
			return () => PDAEncyclopedia.entries.ContainsKey(ExplorationTrackerPages.instance.getEncyKey(pg));
		}
		
		internal static Func<bool> fromStory(string key) {
			return () => StoryGoalManager.main.IsGoalComplete(key);
		}
		
		internal static Func<bool> fromStory(StoryGoal g) {
			return () => StoryGoalManager.main.IsGoalComplete(g.key);
		}
		
	}
		
}
	