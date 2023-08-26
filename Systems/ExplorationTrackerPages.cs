using System;
using System.Collections.Generic;
using System.Linq;

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
			
	    	StoryHandler.instance.addListener(this);
	    	LanguageHandler.SetLanguageLine("EncyPath_Findings", SeaToSeaMod.miscLocale.getEntry("TrackerPage").getField<string>("category"));
	    	
	    	addPage(TrackerPages.POD3, new StoryTrigger("OnPlayRadioGrassy25")).addFinding("databox", Finding.fromUnlock(TechType.Compass)).addFinding("geyser", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType));
	    	addPage(TrackerPages.POD19, new PositionTrigger(new Vector3(-808.72F, -299.31F, -872.53F), 50)).addFinding("databox", Finding.fromUnlock(TechType.HighCapacityTank)).addFinding("pda", Finding.fromEncy("LifepodKeenLog")).addFinding("cache", Finding.fromStory("Precursor_SparseReefCache_DataDownload1"));
	    	addPage(TrackerPages.POD17, new PositionTrigger(new Vector3(-515.56F, -98.79F, -56.18F), 30)).addFinding("pda", Finding.fromEncy("LifepodSeaglide")).addFinding("seamoth", Finding.fromEncy("Seamoth")).addFinding("modstation", Finding.fromEncy("ModificationStation")).addFinding("lasercutter", Finding.fromEncy("LaserCutter")).addFinding("battcharge", Finding.fromUnlock(TechType.BatteryCharger));
	    	addPage(TrackerPages.POD6, new PositionTrigger(pod6Base, 80)).addFinding("pda", Finding.fromEncy("LifepodCrashZone2")).addFinding("seamoth", Finding.fromEncy("Seamoth")).addFinding("modstation", Finding.fromEncy("ModificationStation")).addFinding("lasercutter", Finding.fromEncy("LaserCutter")).addFinding("battcharge", Finding.fromUnlock(TechType.BatteryCharger));
	    	
	    	addPage(TrackerPages.DEGASIEND, new BiomeTrigger(VanillaBiomes.DEEPGRAND)).addFinding("end", Finding.fromEncy("DeepPDA3")).addFinding("tablet", Finding.fromUnlock(TechType.PrecursorKey_Orange)).addFinding("rebreather", Finding.fromUnlock(SeaToSeaMod.rebreatherCharger.TechType));
	    	addPage(TrackerPages.VOID, new PositionTrigger(VoidSpikesBiome.signalLocation, 30)).addFinding("destroy", Finding.fromEncy(VoidSpikesBiome.PDA_KEY)).addFinding("databox", Finding.fromUnlock(CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType)).addFinding("end", Finding.fromEncy(VoidSpikeWreck.PDA_KEY)).addFinding("items", Finding.fromStory("PressureCrystals"));//TODO .addFinding("levi", Finding.fromScan(SeaToSeaMod.voidSpikeLevi.TechType));
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
			//LanguageHandler.Main.SetLanguageLine();
			return page;
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
	
	enum TrackerPages {
		POD3, //compass databox, geyser cave [nanolathing drones]; pod3 radio
		POD19, //highcap br, pda to floatisland, precursor cache; pod 19 radio
        POD17, //pda, jellyshroom, seamoth fragment, large nearby wreck (gibberish console [Story_AuroraConsole1], that deep room (-645.6, -102.7, -16.2) prox); pod 17 prox
		POD6, //pda, seacrown, salvebush; pod6 prox
		KOOSH, //pod 12, koosh caves (mercury, scan or collect), reinf suit, mushkoosh cave [scan 1 geyser coral]; koosh biome OR kooshcaveprompt
		DUNECENTRAL, //sealed suit databox, azurite, lumenshrooms; dunes biome
		METEOR, //meteor [scan], precursor cache [Precursor_Cache_DataDownload3]; meteor prox R=100
		UNDERISLANDS, //falling GF wreck [fire storygoal], autofarmer, prop gun blocked room [prox to -124.38, -200.69, 855 R=5], cylcops fire suppression; underislands biome
		BKELPTRENCH, //caves, pod 1, bkelp base (bioproc upgrade AND smdepth4 AND the pda); biome in bkelp trench and depth > 300m
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
		CRASH, //pod4, trailerbase [databpx], sanctuary [eye flame core], crashmesa [databox]; crash zone biome
		VOID, //pod 15 pda and databox, voidspikes PDA, abyssoclase; void pod radio
	}
	
	class TrackerPage {
		
		internal readonly PDAManager.PDAPage encyPage;
		
		private readonly List<Finding> findings = new List<Finding>();
		
		private readonly XMLLocale.LocaleEntry locale;
		
		internal TrackerPage(XMLLocale.LocaleEntry e) {
			locale = e;
			encyPage = PDAManager.createPage("ency_findings_"+e.key, e.name, "", "Findings");
			encyPage.register();
		}
		
		internal TrackerPage addFinding(string key, Func<bool> trigger) {
			findings.Add(new Finding(locale.getField<string>(key), trigger));
			return this;
		}
		
		internal void updateText() {
			encyPage.update(generatePDAContent(), true);
		}
		
		private string generatePDAContent() {
			string desc = SeaToSeaMod.miscLocale.getEntry("TrackerPage").pda+"\n";
			bool incomplete = false;
			bool any = false;
			foreach (Finding f in findings) {
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
		
		internal readonly string desc;
		
		internal readonly Func<bool> isTriggered;
		
		internal Finding(string s, Func<bool> f) {
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
		
		internal static Func<bool> fromStory(string key) {
			return () => StoryGoalManager.main.IsGoalComplete(key);
		}
		
		internal static Func<bool> fromStory(StoryGoal g) {
			return () => StoryGoalManager.main.IsGoalComplete(g.key);
		}
		
	}
		
}
	