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
	
	public class LifeformScanningSystem {
		
		public static readonly LifeformScanningSystem instance = new LifeformScanningSystem();
	    
		private readonly Dictionary<TechType, LifeformEntry> requiredLifeforms = new Dictionary<TechType, LifeformEntry>();
		private readonly SortedDictionary<string, List<LifeformEntry>> byCategory = new SortedDictionary<string, List<LifeformEntry>>();
		
		internal static readonly string NEED_SCANS_PDA = "needencyscans";
		
		private readonly string xmlPathRoot;
		
		private float needsPDAUpdate = -1;
		
		private float lastAoECheckTime = -1;
		
		public static bool showAll = false;
		
		private LifeformScanningSystem() {
			xmlPathRoot = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "lifeform_scans");
		}
		
		public void register() {
			//IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
			IngameMenuHandler.Main.RegisterOnSaveEvent(save);
		}
		
		public void tick(float time) {
			if (!Story.StoryGoalManager.main.IsGoalComplete("Goal_Scanner")) {
				return;
			}
			if (needsPDAUpdate >= 0 && time >= needsPDAUpdate) {
				PDAManager.getPage(NEED_SCANS_PDA).update(generatePDAContent(), true);
				PDAManager.getPage(NEED_SCANS_PDA).unlock();
				needsPDAUpdate = -1;
			}
			if (time-lastAoECheckTime >= 1.0F) {
				lastAoECheckTime = time;
				WorldUtil.getGameObjectsNear(Player.main.transform.position, 60, go => {
					if (ObjectUtil.isVisible(go)) {
						onObjectSeen(go, false);
					}
				});
			}
		}
		
		private void loadSave() {
			string path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".xml");
			if (File.Exists(path)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
					TechType tt = SNUtil.getTechType(e.getProperty("techtype"));
					if (tt != TechType.None && requiredLifeforms.ContainsKey(tt))
						requiredLifeforms[tt].loadFromXML(e);
				}
			}
			SNUtil.log("Loaded lifeform scan cache: ");
			SNUtil.log(requiredLifeforms.toDebugString());
		}
		
		private void save() {
			string path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".xml");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (LifeformEntry le in requiredLifeforms.Values) {
				XmlElement e = doc.CreateElement("entry");
				le.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
			Directory.CreateDirectory(xmlPathRoot);
			doc.Save(path);
		}
		
		public void onScanComplete(PDAScanner.EntryData data) {
			needsPDAUpdate = DayNightCycle.main.timePassedAsFloat+1;
		}
		
		private string generatePDAContent() {
			getRequiredLifeforms();
			string desc = SeaToSeaMod.pdaLocale.getEntry(NEED_SCANS_PDA).pda+"\n";
			foreach (KeyValuePair<string, List<LifeformEntry>> kvp in byCategory) {
				desc += kvp.Key+":\n";
				foreach (LifeformEntry le in kvp.Value) {
					bool has = le.isScanned();
					string seen = le.getLastSeen();
					string name = has || le.isIdentityKnown() ? Language.main.Get(le.objectType) : le.getHint(seen != null);
					if (showAll)
						name += " ["+Language.main.Get(le.objectType)+"]";
					string color = has ? "20FF40" : (seen != null ? "FFE020" : "FF2040");
					desc += string.Format("\t<color=#{0}>{1}</color> ({2})\n", color, name, has ? "Analyzed" : (seen != null ? "Last Seen Near "+seen : "Not Yet Encountered"));
				}
				desc += "\n\n";
			}
			return desc;
		}
		
		public bool hasScannedEverything() {
			foreach (TechType tt in getRequiredLifeforms()) {
				if (!requiredLifeforms[tt].isScanned())
					return false;
			}
			return true;
		}
		
		private IEnumerable<TechType> getRequiredLifeforms() {
			if (requiredLifeforms.Count == 0) {
				foreach (TechType tt in PDAScanner.mapping.Keys) {
					if (!isDummiedOut(tt) && mustScanToLeave(tt)) {
						LifeformEntry le = new LifeformEntry(tt);
						requiredLifeforms[tt] = le;
						addOrCreateEntry(le);
						//SNUtil.log("Adding "+le.objectType.AsString()+" to lifeform scanning system: "+le.category);
					}
				}
				loadSave();
			}
			return requiredLifeforms.Keys;
		}
		
		private void addOrCreateEntry(LifeformEntry le) {
			if (byCategory.ContainsKey(le.category)) {
				byCategory[le.category].Add(le);
				byCategory[le.category].Sort();
			}
			else {
				byCategory[le.category] = new List<LifeformEntry>(){le};
			}
		}
		
		internal bool isDummiedOut(TechType tt) {
			if (tt == C2CItems.voidSpikeLevi.TechType && !VoidSpikeLeviathanSystem.instance.isLeviathanEnabled())
				return true;
			if (tt == TechType.BasaltChunk || tt == TechType.SeaEmperor || tt == TechType.BloodGrass || tt == TechType.SmallFan)
				return true;
			return false;
		}
		
		internal bool mustScanToLeave(TechType tt) {
			if (CustomMaterials.getItemByTech(tt) != null)
				return true;
			else if (BasicCustomPlant.getPlant(tt) != null)
				return true;
			string pfb = CraftData.GetClassIdForTechType(tt);
			if (pfb != null && VanillaFlora.getFromID(pfb) != null)
				return true;
			if (pfb != null && VanillaResources.getFromID(pfb) != null)
				return true;
			GameObject prefab = ObjectUtil.lookupPrefab(tt);
			if (prefab) {
				if (prefab.GetComponent<Creature>())
					return true;
				if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) && CraftData.GetTechType(prefab) == tt && prefab.GetComponentInChildren<Collider>()) { //scannable
					string key = PDAScanner.GetEntryData(tt).encyclopedia;
					if (!string.IsNullOrEmpty(key) && PDAEncyclopedia.mapping.ContainsKey(key)) {
						PDAEncyclopedia.EntryData ed = PDAEncyclopedia.mapping[key];
						if (ed != null) {
							if (ed.path.StartsWith("planetarygeology", StringComparison.InvariantCultureIgnoreCase) || ed.path.StartsWith("lifeforms", StringComparison.InvariantCultureIgnoreCase))
								return true;
						}
					}
				}
			}
			return false;
		}
		
		internal void onObjectSeen(GameObject go, bool identity) { //call from getting attacked by, from mousing over, from touching
			getRequiredLifeforms(); //populate list
			TechType tt = CraftData.GetTechType(go);
			if (requiredLifeforms.ContainsKey(tt)) {
				if (!go.GetComponent<WaterParkItem>()) {
					requiredLifeforms[tt].seeAt(go.transform.position, identity);
					needsPDAUpdate = DayNightCycle.main.timePassedAsFloat+0.5F;
				}
			}
		}
		
		internal void onBiomeDiscovered() {
			needsPDAUpdate = DayNightCycle.main.timePassedAsFloat+0.5F;
		}
			
		public string getLocalDescription(Vector3 pos) {
			BiomeBase bb = BiomeBase.getBiome(pos);
			if (bb != null && BiomeDiscoverySystem.instance.isDiscovered(bb))
				return WorldUtil.getRegionalDescription(pos, false);
			else
				return "Unexplored Area";
		}
	    
	    class LifeformEntry : IComparable<LifeformEntry> {
	    	
			internal readonly TechType objectType;
			internal readonly string category;
			
			private readonly string hint;
			
			private Vector3 seenAt = Vector3.zero;
			private bool identityKnown;
			
			internal LifeformEntry(TechType tt) {
				objectType = tt;
				
				PDAEncyclopedia.EntryData data = getEncyData();
				category = data == null ? "General" : SNUtil.getDescriptiveEncyPageCategoryName(data);
				if (tt == TechType.PrecursorDroid)
					category = Language.main.Get("EncyPath_Lifeforms/Fauna");
				else if (tt == TechType.PrecursorIonCrystal)
					category = Language.main.Get("EncyPath_PlanetaryGeology");
				
				hint = getHint(false);
				GameObject pfb = ObjectUtil.lookupPrefab(tt);
				if (pfb) {
					Creature c = pfb.GetComponent<Creature>();
					bool levi = c is ReaperLeviathan || c is GhostLeviatanVoid || c is GhostLeviathan || c is Reefback || c is SeaDragon || c is SeaEmperorJuvenile;
					if (levi) {
						hint = "Unknown Leviathan";
					}
					else if (c is Warper || pfb.GetComponent<MeleeAttack>() || pfb.GetComponent<RangeAttacker>()) {
						hint = "Unknown Aggressive Fauna";
					}
					else if (c) {
						hint = "Unknown Fauna";
					}
					if (!levi && category.Contains("Fauna")) {
						float size = 0;
						foreach (Renderer cc in pfb.GetComponentsInChildren<Renderer>(true))
							size += cc.bounds.size.magnitude;
						if (size >= 32)
							hint += " (Large)";
						else if (size <= 8F)
							hint += " (Small)";
					}
				}
			}
			
			public bool isScanned() {
				return PDAScanner.complete.Contains(objectType);
			}
			
			public string getLastSeen() {
				return seenAt.magnitude > 0.5F ? instance.getLocalDescription(seenAt) : null;
			}
			
			public bool isIdentityKnown() {
				return identityKnown;
			}
			
			internal bool seeAt(Vector3 vec, bool identity) {
				if (!(identity && !identityKnown) && seenAt.magnitude > 0.5F && (vec-seenAt).sqrMagnitude < 100)
					return false;
				seenAt = vec;
				identityKnown |= identity;
				return true;
			}
			
			public string getHint(bool seen) {
				return seen ? hint : "Unknown "+category.Replace(" Data", "")+" Entity";
			}
			
			public PDAScanner.EntryData getScannerData() {
				return PDAScanner.mapping[objectType];
			}
			
			public PDAEncyclopedia.EntryData getEncyData() {
				string key = getScannerData().encyclopedia;
				if (string.IsNullOrEmpty(key))
					return null;
				return PDAEncyclopedia.mapping.ContainsKey(key) ? PDAEncyclopedia.mapping[key] : null;
			}
		
			public int CompareTo(LifeformEntry ro) {
				PDAEncyclopedia.EntryData us = getEncyData();
				PDAEncyclopedia.EntryData them = ro.getEncyData();
				if (us == null && them == null)
					return objectType.CompareTo(ro.objectType);
				else if (us == null)
					return -1;
				else if (them == null)
					return 1;
				else
					return String.Compare(us.path, them.path, StringComparison.InvariantCultureIgnoreCase);
			}
			
			internal void saveToXML(XmlElement n) {
				n.addProperty("techtype", objectType.AsString());
				n.addProperty("seen", seenAt);
				n.addProperty("known", identityKnown);
			}
			
			internal void loadFromXML(XmlElement e) {
				seenAt = e.getVector("seen").Value;
				identityKnown = e.getBoolean("known");
			}
			
			public override string ToString() {
				return string.Format("[ObjectType={0}, Category={1}, Hint={2}, SeenAt={3}, IdentityKnown={4}]", objectType.AsString(), category, hint, seenAt, identityKnown);
			}

	    	
	    }
		
	}
	
}
