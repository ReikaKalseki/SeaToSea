using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class DataCollectionTracker {

		private static readonly LocationDescriptor unknownLocation = new LocationDescriptor(() => true, "Unknown Location");
		private static readonly LocationDescriptor auroraGoal = new LocationDescriptor(() => true, "Aboard The Aurora"); //always known location

		public static readonly DataCollectionTracker instance = new DataCollectionTracker();

		internal static readonly string NEED_DATA_PDA = "needencydata";

		private readonly string saveFileName = "data_collection.dat";

		private readonly Dictionary<string, DataDownloadEntry> requiredAuroraData = new Dictionary<string, DataDownloadEntry>();
		private readonly Dictionary<string, DataDownloadEntry> requiredDegasiData = new Dictionary<string, DataDownloadEntry>();
		private readonly Dictionary<string, DataDownloadEntry> requiredAlienData = new Dictionary<string, DataDownloadEntry>();
		//private readonly HashSet<Area> discoveredAlienFacilities = new HashSet<Area>();

		private readonly List<AlienScanEntry> alienBaseScans = new List<AlienScanEntry>();

		private float needsPDAUpdate = -1;

		public static bool showAll = false;

		private DataCollectionTracker() {

		}

		private void addAlienScanEntry(LocationDescriptor f, TechType tt) {
			alienBaseScans.Add(new AlienScanEntry(f, tt));
		}

		private DataDownloadEntry addLifepodLog(string key, int pod) {
			return this.addRequiredData(key, "Lifepod " + pod + " Log", unknownLocation, requiredAuroraData);
		}

		private DataDownloadEntry addRequiredData(string key, string hint, LocationDescriptor loc, Dictionary<string, DataDownloadEntry> map) {
			DataDownloadEntry e = new DataDownloadEntry(loc, key, hint);
			map[key] = e;
			return e;
		}

		public void register() {
			//IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
			//IngameMenuHandler.Main.RegisterOnSaveEvent(save);

			StoryHandler.instance.addListener(s => { needsPDAUpdate = DayNightCycle.main.timePassedAsFloat + 1; });
		}

		public void buildSet() {
			if (requiredAuroraData.Count > 0)
				return;
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
			string genericPDA = "PDA Log";
			string genericHint = "Data Download";
			this.addRequiredData("Aurora_DriveRoom_Terminal1", "Black Box Data", auroraGoal, requiredAuroraData);
			this.addRequiredData("Aurora_RingRoom_Terminal3", "Escape Rocket Data", auroraGoal, requiredAuroraData).setVisible("RadioCaptainsQuartersCode");
			this.addLifepodLog("bkelpbase", 1);
			this.addLifepodLog("bkelpbase2", 1);
			this.addLifepodLog("lrpowerseal", 1);
			this.addLifepodLog("Lifepod1", 2);
			this.addLifepodLog("Lifepod3", 3);
			this.addLifepodLog("LifepodDecoy", 4).setVisible("RadioRadiationSuitNoSignalAltDatabank");
			this.addLifepodLog("LifepodCrashZone1", 6).setVisible("RadioShallows22NoSignalAltDatabank");
			this.addLifepodLog("LifepodCrashZone2", 6).setVisible("RadioShallows22NoSignalAltDatabank");
			this.addLifepodLog("LifepodRandom", 7).setVisible("RadioKelp28NoSignalAltDatabank");
			this.addLifepodLog("treaderpod", 9);
			this.addLifepodLog("treadercave", 9);
			this.addLifepodLog("crashmesa", 10).setVisible("crashmesahint");
			this.addLifepodLog("Lifepod2", 12);
			this.addLifepodLog("Lifepod4", 13);
			this.addLifepodLog("rescuepdalog", 13);
			this.addLifepodLog("treepda", 13);
			this.addLifepodLog("mountainpodearly", 14);
			this.addLifepodLog("mountainpodlate", 14);
			this.addLifepodLog("mountaincave", 14);
			this.addLifepodLog("islandpda", 14);
			this.addLifepodLog("islandcave", 14);
			this.addLifepodLog("voidpod", 15);
			this.addLifepodLog("voidspike", 15);
			this.addLifepodLog("LifepodSeaglide", 17);
			this.addLifepodLog("LifepodKeenDialog", 19);
			this.addLifepodLog("LifepodKeenLog", 19);
			this.addRequiredData("dunearch", "Unknown Survivor Log", unknownLocation, requiredAuroraData).setVisible("dunearchhint");
			this.addRequiredData("RendezvousFloatingIsland", "Rendezvous Log", unknownLocation, requiredAuroraData).setVisible("LifepodKeenLog");
			this.addRequiredData("CaptainPDA", "Aurora Captain Log", unknownLocation, requiredAuroraData).setVisible("RadioCaptainsQuartersCode");
			if (hard) {
				this.addRequiredData("Aurora_Locker_PDA1", "Aurora Data Log", auroraGoal, requiredAuroraData); //Degasi secondary mission
				this.addRequiredData("Aurora_Cargo_PDA1", "Aurora Conversation Log", auroraGoal, requiredAuroraData); //Yu and Berkeley
				this.addRequiredData("Aurora_Living_Area_PDA2b", "Aurora Conversation Log", auroraGoal, requiredAuroraData); //"You're dumping me"
				this.addRequiredData("InnerBiomeWreckLore7", "Aurora Conversation Log", unknownLocation, requiredAuroraData); //"you've both been equally incompetent"
				this.addRequiredData("OuterBiomeWreckLore9", "Aurora Conversation Log", unknownLocation, requiredAuroraData); //"suspicious keyword 'religious'"
			}

			LocationDescriptor floatislandBaseGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("FLOATISLAND_DEGASI"), "Detected at the Floating Island Degasi Base");
			LocationDescriptor jellyBaseGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("JELLY_DEGASI"), "Detected in the Jellyshroom Caves Degasi Base");
			LocationDescriptor dgrBaseGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("DGR_DEGASI"), "Detected in the Deep Grand Reef Degasi Base");
			this.addRequiredData("IslandsPDABase1bDesk", genericPDA, floatislandBaseGoal, requiredDegasiData); //1
			this.addRequiredData("IslandsPDABase1Desk", genericPDA, floatislandBaseGoal, requiredDegasiData); //2
			this.addRequiredData("IslandsPDAExterior", genericPDA, floatislandBaseGoal, requiredDegasiData); //3
			this.addRequiredData("IslandsPDABase1Interior", genericPDA, floatislandBaseGoal, requiredDegasiData); //paul1
			if (hard)
				this.addRequiredData("JellyPDARoom2Locker", genericPDA, floatislandBaseGoal, requiredDegasiData); //4, tablet
			this.addRequiredData("IslandsPDABase1a", genericPDA, floatislandBaseGoal, requiredDegasiData); //bart3
			if (hard)
				this.addRequiredData("JellyPDABreadcrumb", genericPDA, jellyBaseGoal, requiredDegasiData);
			this.addRequiredData("JellyPDABrokenCorridor", genericPDA, jellyBaseGoal, requiredDegasiData); //5
			this.addRequiredData("JellyPDARoom2Desk", genericPDA, jellyBaseGoal, requiredDegasiData); //6
			this.addRequiredData("JellyPDARoom1Desk", genericPDA, jellyBaseGoal, requiredDegasiData); //bart1
			this.addRequiredData("JellyPDAObservatory", genericPDA, jellyBaseGoal, requiredDegasiData); //bart2
			this.addRequiredData("JellyPDARoom1Locker", genericPDA, jellyBaseGoal, requiredDegasiData); //paul2
			if (hard)
				this.addRequiredData("JellyPDAExterior", genericPDA, dgrBaseGoal, requiredDegasiData); //rant
			this.addRequiredData("DeepPDA1", genericPDA, dgrBaseGoal, requiredDegasiData); //7
			this.addRequiredData("DeepPDA2", genericPDA, dgrBaseGoal, requiredDegasiData); //8
			this.addRequiredData("DeepPDA3", genericPDA, dgrBaseGoal, requiredDegasiData); //9
			this.addRequiredData("DeepPDA4", genericPDA, dgrBaseGoal, requiredDegasiData); //paul3

			LocationDescriptor anywhere = new LocationDescriptor(() => true, "No Specific Location");
			LocationDescriptor gunGoal = new LocationDescriptor("Precursor_Gun_DataDownload2", "Detected in the Quarantine Enforcement Platform");
			LocationDescriptor drfGoal = new LocationDescriptor("Precursor_LostRiverBase_Log2", "Detected in the Disease Research Facility");
			LocationDescriptor atpGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("SEE_ATP"), "Detected in the Alien Thermal Plant");
			LocationDescriptor pcfGoal = new LocationDescriptor("Precursor_Prison_MoonPool_Log1", "Detected in the Primary Containment Facility");
			LocationDescriptor lrlabGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("LR_LAB"), "Detected in the Lost River Lab Cache");
			this.addRequiredData("Precursor_Gun_DataDownload1", genericHint, gunGoal, requiredAlienData);
			this.addRequiredData("Precursor_Gun_DataDownload2", genericHint, gunGoal, requiredAlienData);
			this.addRequiredData("Precursor_SparseReefCache_DataDownload1", genericHint, new LocationDescriptor(C2CProgression.instance.getLocationGoal("SPARSE_CACHE"), "Detected in the Sparse Reef Sanctuary"), requiredAlienData);
			this.addRequiredData("Precursor_Cache_DataDownload2", genericHint, new LocationDescriptor(C2CProgression.instance.getLocationGoal("NBKELP_CACHE"), "Detected in the Blood Kelp Sanctuary"), requiredAlienData);
			this.addRequiredData("Precursor_Cache_DataDownload3", genericHint, new LocationDescriptor(C2CProgression.instance.getLocationGoal("DUNES_CACHE"), "Detected in the Dunes Sanctuary"), requiredAlienData);
			this.addRequiredData("Precursor_Cache_DataDownloadLostRiver", genericHint, lrlabGoal, requiredAlienData);
			this.addRequiredData("Precursor_LostRiverBase_DataDownload1", genericHint, drfGoal, requiredAlienData);
			this.addRequiredData("Precursor_LostRiverBase_DataDownload3", genericHint, drfGoal, requiredAlienData);
			this.addRequiredData("Precursor_LostRiverBase_Log3", genericHint, drfGoal, requiredAlienData); //drf cinematic
			this.addRequiredData("Precursor_LavaCastleBase_ThermalPlant2", genericHint, atpGoal, requiredAlienData);
			this.addRequiredData("Precursor_LavaCastleBase_ThermalPlant3", genericHint, atpGoal, requiredAlienData);
			this.addRequiredData("Precursor_LavaCastleBase_DataDownload1", genericHint, atpGoal, requiredAlienData); //ion power
			this.addRequiredData("Precursor_Prison_DataDownload1", genericHint, pcfGoal, requiredAlienData);
			this.addRequiredData("Precursor_Prison_DataDownload2", genericHint, pcfGoal, requiredAlienData);
			this.addRequiredData("Precursor_Prison_DataDownload3", genericHint, pcfGoal, requiredAlienData);

			this.addAlienScanEntry(gunGoal, TechType.PrecursorEnergyCore);
			this.addAlienScanEntry(gunGoal, TechType.PrecursorPrisonArtifact6); //bomb
			this.addAlienScanEntry(gunGoal, TechType.PrecursorPrisonArtifact7); //rifle
			this.addAlienScanEntry(lrlabGoal, TechType.PrecursorSensor);
			this.addAlienScanEntry(lrlabGoal, TechType.PrecursorLostRiverLabBones);
			this.addAlienScanEntry(lrlabGoal, TechType.PrecursorLabCacheContainer1);
			this.addAlienScanEntry(lrlabGoal, TechType.PrecursorLabCacheContainer2);
			this.addAlienScanEntry(lrlabGoal, TechType.PrecursorLabTable);
			this.addAlienScanEntry(drfGoal, TechType.PrecursorWarper);
			this.addAlienScanEntry(drfGoal, TechType.PrecursorFishSkeleton);
			this.addAlienScanEntry(drfGoal, TechType.PrecursorLostRiverLabRays);
			this.addAlienScanEntry(drfGoal, TechType.PrecursorLostRiverLabEgg);
			this.addAlienScanEntry(atpGoal, TechType.PrecursorThermalPlant);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact1);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact2);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact3);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact4);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact5);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact8);
			//does not exist addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact9);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact10);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact11);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact12);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact13);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonLabEmperorEgg);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonLabEmperorFetus);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonAquariumIncubator);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonAquariumIncubatorEggs);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPipeRoomIncomingPipe);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPipeRoomOutgoingPipe);
			this.addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonIonGenerator);
			this.addAlienScanEntry(anywhere, TechType.PrecursorTeleporter);
			this.addAlienScanEntry(anywhere, TechType.PrecursorPrisonAquariumFinalTeleporter); //this is a unique scan
		}

		public void tick(float time) {
			if (!Story.StoryGoalManager.main.IsGoalComplete("Goal_Scanner")) {
				return;
			}
			if (needsPDAUpdate >= 0 && time >= needsPDAUpdate) {
				PDAManager.getPage(NEED_DATA_PDA).update(this.generatePDAContent(), true);
				PDAManager.getPage(NEED_DATA_PDA).unlock();
				needsPDAUpdate = -1;
			}
		}
		/*
		private void loadSave() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			if (File.Exists(path)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				loadList(doc.DocumentElement, "Aurora", requiredAuroraData);
				loadList(doc.DocumentElement, "Degasi", requiredDegasiData);
				loadList(doc.DocumentElement, "Alien", requiredAlienData);
				XmlElement e = doc.DocumentElement.getDirectElementsByTagName("Scans")[0];
				foreach (XmlElement e2 in e.ChildNodes) {
					
				}
			}
			SNUtil.log("Loaded data collection cache: ");
			SNUtil.log(requiredAuroraData.toDebugString());
			SNUtil.log(requiredDegasiData.toDebugString());
			SNUtil.log(requiredAlienData.toDebugString());
		}
		
		private void save() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			saveList(rootnode, "Aurora", requiredAuroraData);
			saveList(rootnode, "Degasi", requiredDegasiData);
			saveList(rootnode, "Alien", requiredAlienData);
			XmlElement e = doc.CreateElement("Scans");
			foreach (AlienScanEntry a in alienBaseScans) {
				
			}
			rootnode.AppendChild(e);
			doc.Save(path);
		}
		
		private void saveList(XmlElement node, string key, Dictionary<string, DataDownloadEntry> li) {
			XmlElement wrap = node.OwnerDocument.CreateElement(key);
			foreach (DataDownloadEntry le in li.Values) {
				XmlElement e = node.OwnerDocument.CreateElement("entry");
				le.saveToXML(e);
				wrap.AppendChild(e);
			}
			node.AppendChild(wrap);
		}
		
		private void loadList(XmlElement node, string key, Dictionary<string, DataDownloadEntry> li) {
			XmlElement wrap = node.getDirectElementsByTagName(key)[0];
			foreach (XmlElement e in wrap.ChildNodes) {
				string ency = e.getProperty("encyKey");
				if (li.ContainsKey(ency))
					li[ency].loadFromXML(e);
			}
		}
		*/
		public void onScanComplete(PDAScanner.EntryData data) {
			needsPDAUpdate = DayNightCycle.main.timePassedAsFloat + 1;
		}

		private string generatePDAContent() {
			if (!Language.main) {
				SNUtil.log("Initialized DataCollect PDA before language!");
				return "ERROR";
			}
			this.buildSet();
			XMLLocale.LocaleEntry ll = SeaToSeaMod.pdaLocale.getEntry(NEED_DATA_PDA);
			string desc = ll.pda;
			bool alien = requiredAlienData.Any(e => e.Value.isCollected());
			if (alien)
				desc += "\n" + ll.getField<string>("alien");
			desc += "\n\n" + ll.getField<string>("prefix") + "\n";
			desc = this.appendDataList(desc, "Aurora Data", requiredAuroraData);
			desc = this.appendDataList(desc, "Degasi Data", requiredDegasiData);
			if (alien)
				desc = this.appendDataList(desc, "Alien Data", requiredAlienData);
			desc += "\n\nAlien Artifacts:\n";
			foreach (AlienScanEntry le in alienBaseScans) {
				bool has = le.isScanned();
				bool seen = le.location != null && (le.location.checkSeen == null || le.location.checkSeen.Invoke());
				string name = has ? Language.main.Get(le.tech) : "Unknown Object";
				if (showAll)
					name += " [" + Language.main.Get(le.tech) + "]";
				string color = has ? "20FF40" : (seen ? "FFE020" : "FF2040");
				desc += string.Format("\t<color=#{0}>{1}</color> ({2})\n", color, name, has ? "Collected" : (seen ? le.location.getDescription() : unknownLocation.getDescription()));
			}
			return desc;
		}

		private string appendDataList(string desc, string title, Dictionary<string, DataDownloadEntry> li) {
			if (li == null) {
				SNUtil.writeToChat("Null data collect map under title=" + title);
				return "ERROR";
			}
			desc += title + ":\n";
			foreach (KeyValuePair<string, DataDownloadEntry> kvp in li) {
				DataDownloadEntry le = kvp.Value;
				if (le == null) {
					SNUtil.writeToChat("Null entry in data collect PDA, key=" + kvp.Key);
					continue;
				}
				if (!le.isVisible())
					continue;
				bool has = le.isCollected();
				if (le.location == null)
					SNUtil.writeToChat("No location for " + le);
				else if (le.location.checkSeen == null)
					SNUtil.writeToChat("No location check for " + le);

				bool seen = le.location != null && (le.location.checkSeen == null || le.location.checkSeen.Invoke());
				string name = has ? Language.main.Get("Ency_"+le.encyKey) : le.hint;
				if (showAll)
					name += " [" + Language.main.Get("Ency_" + le.encyKey) + "]";
				string color = has ? "20FF40" : (seen ? "FFE020" : "FF2040");
				desc += string.Format("\t<color=#{0}>{1}</color> ({2})\n", color, name, has ? "Collected" : (seen ? le.location.getDescription() : unknownLocation.getDescription()));
			}
			desc += "\n\n";
			return desc;
		}

		public bool isFullyComplete() {
			return this.getMissingAuroraData().Count == 0 && this.getMissingDegasiData().Count == 0 && this.getMissingAlienData().Count == 0 && alienBaseScans.All(e => e.isScanned());
		}

		public List<DataDownloadEntry> getMissingAlienData() {
			return this.getMissingData(requiredAlienData);
		}

		public List<DataDownloadEntry> getMissingAuroraData() {
			return this.getMissingData(requiredAuroraData);
		}

		public List<DataDownloadEntry> getMissingDegasiData() {
			return this.getMissingData(requiredDegasiData);
		}

		private List<DataDownloadEntry> getMissingData(Dictionary<string, DataDownloadEntry> dict) {
			List<DataDownloadEntry> li = new List<DataDownloadEntry>();
			foreach (DataDownloadEntry e in dict.Values) {
				if (!e.isCollected())
					li.Add(e);
			}
			return li;
		}

		public class DataDownloadEntry : IComparable<DataDownloadEntry> {

			public readonly string encyKey;
			public readonly LocationDescriptor location;
			public readonly string category;
			internal readonly PDAEncyclopedia.EntryData pdaPage;

			internal readonly string hint;

			private Func<bool> visiblityTrigger;

			internal DataDownloadEntry(LocationDescriptor f, string ency, string h) {
				encyKey = ency;
				location = f;
				hint = h;

				pdaPage = this.getEncyData();
				category = pdaPage == null ? "General" : SNUtil.getDescriptiveEncyPageCategoryName(pdaPage);
			}

			public void setVisible(string goal) {
				this.setVisible(() => Story.StoryGoalManager.main.IsGoalComplete(goal));
			}

			public void setVisible(Func<bool> f) {
				visiblityTrigger = f;
			}

			public bool isVisible() {
				return visiblityTrigger == null || visiblityTrigger.Invoke();
			}

			public bool isCollected() {
				return Story.StoryGoalManager.main.IsGoalComplete(encyKey);//pdaPage != null && pdaPage.unlocked;
			}

			public PDAEncyclopedia.EntryData getEncyData() {
				return PDAEncyclopedia.mapping.ContainsKey(encyKey) ? PDAEncyclopedia.mapping[encyKey] : null;
			}

			public int CompareTo(DataDownloadEntry ro) {
				PDAEncyclopedia.EntryData us = this.getEncyData();
				PDAEncyclopedia.EntryData them = ro.getEncyData();
				return us == null && them == null
					? String.Compare(encyKey, ro.encyKey, StringComparison.InvariantCultureIgnoreCase)
					: us == null ? -1 : them == null ? 1 : String.Compare(us.path, them.path, StringComparison.InvariantCultureIgnoreCase);
			}

			internal void saveToXML(XmlElement n) {
				n.addProperty("encyKey", encyKey);
			}

			internal void loadFromXML(XmlElement e) {

			}

			public override string ToString() {
				return string.Format("[DataDownloadEntry EncyKey={0}, Location={1}, Category={2}, PdaPage={3}, Hint={4}, VisiblityTrigger={5}]", encyKey, location, category, pdaPage, hint, visiblityTrigger);
			}



		}

		public class AlienScanEntry : IComparable<AlienScanEntry> {

			public readonly LocationDescriptor location;
			public readonly TechType tech;

			internal AlienScanEntry(LocationDescriptor f, TechType tt) {
				location = f;
				tech = tt;
			}

			public bool isScanned() {
				return PDAScanner.complete.Contains(tech);
			}

			public int CompareTo(AlienScanEntry ro) {
				return tech.CompareTo(ro.tech);
			}

			internal void saveToXML(XmlElement n) {
				//if (location != null)
				//	n.addProperty("location", location.ToString());
				n.addProperty("tech", tech.ToString());
			}

			internal void loadFromXML(XmlElement e) {

			}


		}

		public class LocationDescriptor {

			public readonly Func<bool> checkSeen;
			public readonly Func<string> getDescription;

			internal LocationDescriptor(Story.StoryGoal goal, string desc) : this(goal.key, desc) {

			}

			internal LocationDescriptor(string goal, string desc) : this(() => Story.StoryGoalManager.main.IsGoalComplete(goal), desc) {

			}

			internal LocationDescriptor(Func<bool> see, string desc) : this(see, () => desc) {

			}

			internal LocationDescriptor(Func<bool> see, Func<string> desc) {
				checkSeen = see;
				getDescription = desc;
			}


		}
		/*
		enum Area {
			//Aurora
			AURORA,
			POD1,
			POD2,
			POD3,
			POD4,
			POD6,
			POD7,
			POD9,
			POD10,
			POD12,
			POD13, //khasar
			POD14,
			POD15,
			POD17, //ozzy
			POD19, //keen
			DUNEARCH,
			MOUNTAINISLAND,
			GARGSKULL,
			
			//Degasi
			FLOATISLAND,
			JELLYSHROOM,
			DGR,
			
			//Precursor
			GUN,
			SPARSECACHE,
			DUNESCACHE,
			NBKELPCACHE,
			LRLAB,
			DRF,
			ATP,
			PCF
		}
		*/
	}

}
