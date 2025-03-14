using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class DataCollectionTracker {
		
		private static readonly LocationDescriptor unknownLocation = new LocationDescriptor(() => true, "Unknown Location");
		private static readonly LocationDescriptor auroraGoal = new LocationDescriptor(() => true, "The Aurora"); //always known location
		
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
			return addRequiredData(key, "Lifepod "+pod+" Log", unknownLocation, requiredAuroraData);
		}
		
		private DataDownloadEntry addRequiredData(string key, string hint, LocationDescriptor loc, Dictionary<string, DataDownloadEntry> map) {
			DataDownloadEntry e = new DataDownloadEntry(loc, key, hint);
			map[key] = e;
			return e;
		}
		
		public void register() {
			//IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
			//IngameMenuHandler.Main.RegisterOnSaveEvent(save);
			
	        StoryHandler.instance.addListener(s => {needsPDAUpdate = DayNightCycle.main.timePassedAsFloat+1;});
		}
		
		public void buildSet() {
			if (requiredAuroraData.Count > 0)
				return;
			string genericPDA = "PDA Log";
			string genericHint = "Data Download";
			addRequiredData("Aurora_DriveRoom_Terminal1", "Black Box Data", auroraGoal, requiredAuroraData);
			addRequiredData("Aurora_RingRoom_Terminal3", "Escape Rocket Data", auroraGoal, requiredAuroraData).setVisible("RadioCaptainsQuartersCode");
			addLifepodLog("bkelpbase", 1);
			addLifepodLog("bkelpbase2", 1);
			addLifepodLog("lrpowerseal", 1);
			addLifepodLog("Lifepod1", 2);
			addLifepodLog("Lifepod3", 3);
			addLifepodLog("LifepodDecoy", 4).setVisible("RadioRadiationSuitNoSignalAltDatabank");
			addLifepodLog("LifepodCrashZone1", 6).setVisible("RadioShallows22NoSignalAltDatabank");
			addLifepodLog("LifepodCrashZone2", 6).setVisible("RadioShallows22NoSignalAltDatabank");
			addLifepodLog("LifepodRandom", 7).setVisible("RadioKelp28NoSignalAltDatabank");
			addLifepodLog("treaderpod", 9);
			addLifepodLog("treadercave", 9);
			addLifepodLog("crashmesa", 10).setVisible("crashmesahint");
			addLifepodLog("Lifepod2", 12);
			addLifepodLog("Lifepod4", 13);
			addLifepodLog("rescuepdalog", 13);
			addLifepodLog("treepda", 13);
			addLifepodLog("mountainpodearly", 14);
			addLifepodLog("mountainpodlate", 14);
			addLifepodLog("mountaincave", 14);
			addLifepodLog("islandpda", 14);
			addLifepodLog("islandcave", 14);
			addLifepodLog("voidpod", 15);
			addLifepodLog("voidspike", 15);
			addLifepodLog("LifepodSeaglide", 17);
			addLifepodLog("LifepodKeenDialog", 19);
			addLifepodLog("LifepodKeenLog", 19);
			addRequiredData("dunearch", "Unknown Survivor Log", unknownLocation, requiredAuroraData).setVisible("dunearchhint");
			addRequiredData("RendezvousFloatingIsland", "Rendezvous Log", unknownLocation, requiredAuroraData).setVisible("LifepodKeenLog");
			addRequiredData("CaptainPDA", "Aurora Captain Log", unknownLocation, requiredAuroraData).setVisible("RadioCaptainsQuartersCode");
			//InnerBiomeWreckLore7 "you've both been equally incompetent"
			
			LocationDescriptor floatislandBaseGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("FLOATISLAND_DEGASI"), "Floating Island Degasi Base");
			LocationDescriptor jellyBaseGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("JELLY_DEGASI"), "Jellyshroom Caves Degasi Base");
			LocationDescriptor dgrBaseGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("DGR_DEGASI"), "Deep Grand Reef Degasi Base");
			addRequiredData("IslandsPDABase1bDesk", genericPDA, floatislandBaseGoal, requiredDegasiData); //1
			addRequiredData("IslandsPDABase1Desk", genericPDA, floatislandBaseGoal, requiredDegasiData); //2
			addRequiredData("IslandsPDAExterior", genericPDA, floatislandBaseGoal, requiredDegasiData); //3
			addRequiredData("IslandsPDABase1Interior", genericPDA, floatislandBaseGoal, requiredDegasiData); //paul1
			addRequiredData("JellyPDARoom2Locker", genericPDA, floatislandBaseGoal, requiredDegasiData); //4, tablet
			addRequiredData("IslandsPDABase1a", genericPDA, floatislandBaseGoal, requiredDegasiData); //bart3
			addRequiredData("JellyPDABreadcrumb", genericPDA, jellyBaseGoal, requiredDegasiData);
			addRequiredData("JellyPDABrokenCorridor", genericPDA, jellyBaseGoal, requiredDegasiData); //5
			addRequiredData("JellyPDARoom2Desk", genericPDA, jellyBaseGoal, requiredDegasiData); //6
			addRequiredData("JellyPDARoom1Desk", genericPDA, jellyBaseGoal, requiredDegasiData); //bart1
			addRequiredData("JellyPDAObservatory", genericPDA, jellyBaseGoal, requiredDegasiData); //bart2
			addRequiredData("JellyPDARoom1Locker", genericPDA, jellyBaseGoal, requiredDegasiData); //paul2
			addRequiredData("JellyPDAExterior", genericPDA, dgrBaseGoal, requiredDegasiData); //rant
			addRequiredData("DeepPDA1", genericPDA, dgrBaseGoal, requiredDegasiData); //7
			addRequiredData("DeepPDA2", genericPDA, dgrBaseGoal, requiredDegasiData); //8
			addRequiredData("DeepPDA3", genericPDA, dgrBaseGoal, requiredDegasiData); //9
			addRequiredData("DeepPDA4", genericPDA, dgrBaseGoal, requiredDegasiData); //paul3
			
			LocationDescriptor anywhere = new LocationDescriptor(() => true, "No Specific Location");
			LocationDescriptor gunGoal = new LocationDescriptor("Precursor_Gun_DataDownload2", "Quarantine Enforcement Platform");
			LocationDescriptor drfGoal = new LocationDescriptor("Precursor_LostRiverBase_Log2", "Disease Research Facility");
			LocationDescriptor atpGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("SEE_ATP"), "Alien Thermal Plant");
			LocationDescriptor pcfGoal = new LocationDescriptor("Precursor_Prison_MoonPool_Log1", "Primary Containment Facility");
			LocationDescriptor lrlabGoal = new LocationDescriptor(C2CProgression.instance.getLocationGoal("LR_LAB"), "Lost River Lab Cache");
			addRequiredData("Precursor_Gun_DataDownload1", genericHint, gunGoal, requiredAlienData);
			addRequiredData("Precursor_Gun_DataDownload2", genericHint, gunGoal, requiredAlienData);
			addRequiredData("Precursor_SparseReefCache_DataDownload1", genericHint, new LocationDescriptor(C2CProgression.instance.getLocationGoal("SPARSE_CACHE"), "Sparse Reef Sanctuary"), requiredAlienData);
			addRequiredData("Precursor_Cache_DataDownload2", genericHint, new LocationDescriptor(C2CProgression.instance.getLocationGoal("NBKELP_CACHE"), "Blood Kelp Sanctuary"), requiredAlienData);
			addRequiredData("Precursor_Cache_DataDownload3", genericHint, new LocationDescriptor(C2CProgression.instance.getLocationGoal("DUNES_CACHE"), "Dunes Sanctuary"), requiredAlienData);
			addRequiredData("Precursor_Cache_DataDownloadLostRiver", genericHint, lrlabGoal, requiredAlienData);
			addRequiredData("Precursor_LostRiverBase_DataDownload1", genericHint, drfGoal, requiredAlienData);
			addRequiredData("Precursor_LostRiverBase_DataDownload3", genericHint, drfGoal, requiredAlienData);
			addRequiredData("Precursor_LostRiverBase_Log3", genericHint, drfGoal, requiredAlienData); //drf cinematic
			addRequiredData("Precursor_LavaCastleBase_ThermalPlant2", genericHint, atpGoal, requiredAlienData);
			addRequiredData("Precursor_LavaCastleBase_ThermalPlant3", genericHint, atpGoal, requiredAlienData);
			addRequiredData("Precursor_LavaCastleBase_DataDownload1", genericHint, atpGoal, requiredAlienData); //ion power
			addRequiredData("Precursor_Prison_DataDownload1", genericHint, pcfGoal, requiredAlienData);
			addRequiredData("Precursor_Prison_DataDownload2", genericHint, pcfGoal, requiredAlienData);
			addRequiredData("Precursor_Prison_DataDownload3", genericHint, pcfGoal, requiredAlienData);
			
			addAlienScanEntry(gunGoal, TechType.PrecursorEnergyCore);
			addAlienScanEntry(gunGoal, TechType.PrecursorPrisonArtifact6); //bomb
			addAlienScanEntry(gunGoal, TechType.PrecursorPrisonArtifact7); //rifle
			addAlienScanEntry(lrlabGoal, TechType.PrecursorSensor);
			addAlienScanEntry(lrlabGoal, TechType.PrecursorLostRiverLabBones);
			addAlienScanEntry(lrlabGoal, TechType.PrecursorLabCacheContainer1);
			addAlienScanEntry(lrlabGoal, TechType.PrecursorLabCacheContainer2);
			addAlienScanEntry(lrlabGoal, TechType.PrecursorLabTable);
			addAlienScanEntry(drfGoal, TechType.PrecursorWarper);
			addAlienScanEntry(drfGoal, TechType.PrecursorFishSkeleton);
			addAlienScanEntry(drfGoal, TechType.PrecursorLostRiverLabRays);
			addAlienScanEntry(drfGoal, TechType.PrecursorLostRiverLabEgg);
			addAlienScanEntry(atpGoal, TechType.PrecursorThermalPlant);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact1);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact2);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact3);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact4);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact5);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact8);
			//does not exist addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact9);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact10);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact11);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact12);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact13);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonLabEmperorEgg);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonLabEmperorFetus);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonAquariumIncubator);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonAquariumIncubatorEggs);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPipeRoomIncomingPipe);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPipeRoomOutgoingPipe);
			addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonIonGenerator);
			addAlienScanEntry(anywhere, TechType.PrecursorTeleporter);
			addAlienScanEntry(anywhere, TechType.PrecursorPrisonAquariumFinalTeleporter); //this is a unique scan
		}
		
		public void tick(float time) {
			if (needsPDAUpdate >= 0 && time >= needsPDAUpdate) {
				PDAManager.getPage(NEED_DATA_PDA).update(generatePDAContent(), true);
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
			needsPDAUpdate = DayNightCycle.main.timePassedAsFloat+1;
		}
		
		private string generatePDAContent() {
			if (!Language.main) {
				SNUtil.log("Initialized DataCollect PDA before language!");
				return "ERROR";
			}
			buildSet();
			XMLLocale.LocaleEntry ll = SeaToSeaMod.pdaLocale.getEntry(NEED_DATA_PDA);
			string desc = ll.pda;
			bool alien = requiredAlienData.Any(e => e.Value.isCollected());
			if (alien)
				desc += "\n"+ll.getField<string>("alien");
			desc += "\n\n"+ll.getField<string>("prefix")+"\n";
			desc = appendDataList(desc, "Aurora Data", requiredAuroraData);
			desc = appendDataList(desc, "Degasi Data", requiredDegasiData);
			if (alien)
				desc = appendDataList(desc, "Alien Data", requiredAlienData);
			desc += "\n\nAlien Artifacts:\n";
			foreach (AlienScanEntry le in alienBaseScans) {
				bool has = le.isScanned();
				bool seen = le.location != null && (le.location.checkSeen == null || le.location.checkSeen.Invoke());
				string name = has ? Language.main.Get(le.tech) : "Unknown Object";
				if (showAll)
					name += " [" + Language.main.Get(le.tech) + "]";
				string color = has ? "20FF40" : (seen ? "FFE020" : "FF2040");
				desc += string.Format("\t<color=#{0}>{1}</color> ({2})\n", color, name, has ? "Analyzed" : (seen ? "Last Seen In Or Near " + le.location.getDescription() : "Not Yet Encountered"));
			}
			return desc;
		}
	
		private string appendDataList(string desc, string title, Dictionary<string, DataDownloadEntry> li) {
			if (li == null) {
				SNUtil.writeToChat("Null data collect map under title="+title);
				return "ERROR";
			}
				desc += title+":\n";
				foreach (KeyValuePair<string, DataDownloadEntry> kvp in li) {
					DataDownloadEntry le = kvp.Value;
					if (le == null) {
						SNUtil.writeToChat("Null entry in data collect PDA, key="+kvp.Key);
						continue;
					}
					if (!le.isVisible())
						continue;
					bool has = le.isCollected();
					if (le.location == null)
						SNUtil.writeToChat("No location for "+le);
					else if (le.location.checkSeen == null)
						SNUtil.writeToChat("No location check for "+le);
					
					bool seen = le.location != null && (le.location.checkSeen == null || le.location.checkSeen.Invoke());
					string name = has ? Language.main.Get("Ency_"+le.encyKey) : le.hint;
					if (showAll)
						name += " ["+Language.main.Get("Ency_"+le.encyKey)+"]";
					string color = has ? "20FF40" : (seen ? "FFE020" : "FF2040");
					desc += string.Format("\t<color=#{0}>{1}</color> ({2})\n", color, name, has ? "Collected" : (seen ? "Last Seen In Or Near "+le.location.getDescription() : "Unknown Location"));
				}
				desc += "\n\n";
				return desc;
		}
		
		internal List<DataDownloadEntry> getMissingAlienData() {
			return getMissingData(requiredAlienData);
		}
		
		internal List<DataDownloadEntry> getMissingAuroraData() {
			return getMissingData(requiredAuroraData);
		}
		
		internal List<DataDownloadEntry> getMissingDegasiData() {
			return getMissingData(requiredDegasiData);
		}
		
		private List<DataDownloadEntry> getMissingData(Dictionary<string, DataDownloadEntry> dict) {
			List<DataDownloadEntry> li = new List<DataDownloadEntry>();
			foreach (DataDownloadEntry e in dict.Values) {
				if (!e.isCollected())
					li.Add(e);
			}
			return li;
		}
	    
	    internal class DataDownloadEntry : IComparable<DataDownloadEntry> {
	    	
			internal readonly string encyKey;
			internal readonly LocationDescriptor location;
			internal readonly string category;
			internal readonly PDAEncyclopedia.EntryData pdaPage;
			
			internal readonly string hint;
			
			private Func<bool> visiblityTrigger;
			
			internal DataDownloadEntry(LocationDescriptor f, string ency, string h) {
				encyKey = ency;
				location = f;
				hint = h;
				
				pdaPage = getEncyData();
				category = pdaPage == null ? "General" : SNUtil.getDescriptiveEncyPageCategoryName(pdaPage);
			}
			
			public void setVisible(string goal) {
				setVisible(() => Story.StoryGoalManager.main.IsGoalComplete(goal));
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
				PDAEncyclopedia.EntryData us = getEncyData();
				PDAEncyclopedia.EntryData them = ro.getEncyData();
				if (us == null && them == null)
					return String.Compare(encyKey, ro.encyKey, StringComparison.InvariantCultureIgnoreCase);
				else if (us == null)
					return -1;
				else if (them == null)
					return 1;
				else
					return String.Compare(us.path, them.path, StringComparison.InvariantCultureIgnoreCase);
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
	    
	    class AlienScanEntry : IComparable<AlienScanEntry> {
	    	
			internal readonly LocationDescriptor location;
			internal readonly TechType tech;
			
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
	    
	    internal class LocationDescriptor {
			
			internal readonly Func<bool> checkSeen;
			internal readonly Func<string> getDescription;
			
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
