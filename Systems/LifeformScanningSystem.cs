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
		
		internal static readonly string NEED_SCANS_PDA = "needencyscans";
		
		internal static readonly string UNEXPLORED_LOCATION_TEXT = "Unexplored Area";
		
		private readonly string oldSaveDir;
		private readonly string saveFileName = "lifeform_scans.dat";
	    
		private readonly Dictionary<TechType, LifeformEntry> requiredLifeforms = new Dictionary<TechType, LifeformEntry>();
		private readonly SortedDictionary<string, List<LifeformEntry>> byCategory = new SortedDictionary<string, List<LifeformEntry>>();
		
		private readonly HashSet<TechType> additionalScans = new HashSet<TechType>() {
			TechType.HugeSkeleton,
			TechType.CaveSkeleton,
			TechType.PrecursorSeaDragonSkeleton,
		};
		
		private float needsPDAUpdate = -1;
		
		private float lastAoECheckTime = -1;
		
		public static bool showAll = false;
		
		private LifeformScanningSystem() {
			oldSaveDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "lifeform_scans");
			if (Directory.Exists(oldSaveDir) && Directory.Exists(SNUtil.savesDir)) {
				migrateSaveData();
			}
		}
		
		public void register() {
			//loaded on first use, not load event IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
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
		
		private void migrateSaveData() {
			SNUtil.log("Migrating lifeform scan data from "+oldSaveDir+" to "+SNUtil.savesDir);
			bool all = true;
			foreach (string xml in Directory.GetFiles(oldSaveDir)) {
				if (xml.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase)) {
					string save = Path.Combine(SNUtil.savesDir, Path.GetFileNameWithoutExtension(xml));
					if (Directory.Exists(save)) {
						SNUtil.log("Moving lifeform scan data "+xml+" to "+save);
						File.Move(xml, Path.Combine(save, saveFileName));
					}
					else {
						SNUtil.log("No save found for '"+xml+", skipping");
						all = false;
					}
				}
			}
			SNUtil.log("Migration complete.");
			if (all) {
				SNUtil.log("All files moved, deleting old folder.");
				Directory.Delete(oldSaveDir);
			}
			else {
				SNUtil.log("Some files could not be moved so the old folder will not be deleted.");
			}
		}
		
		private void loadSave() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
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
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (LifeformEntry le in requiredLifeforms.Values) {
				XmlElement e = doc.CreateElement("entry");
				le.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
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
						name += " ["+le.objectType.AsString()+"="+Language.main.Get(le.objectType)+"]";
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
			if (tt == TechType.BasaltChunk || tt == TechType.SeaEmperor || tt == TechType.SeaEmperorJuvenile || tt == TechType.BloodGrass || tt == TechType.SmallFan)
				return true;
			return false;
		}
		
		internal bool mustScanToLeave(TechType tt) {
			if (CustomMaterials.getItemByTech(tt) != null)
				return true;
			else if (BasicCustomPlant.getPlant(tt) != null)
				return true;
			else if (tt == Ecocean.EcoceanMod.glowOil.TechType || tt == Ecocean.EcoceanMod.celeryTree || tt == Ecocean.EcoceanMod.piezo.TechType || tt == Ecocean.EcoceanMod.plankton.TechType ||tt == Ecocean.EcoceanMod.voidBubble.TechType)
				return true;
			else if (DEIntegrationSystem.instance.isLoaded() && tt == DEIntegrationSystem.instance.getVoidThalassacean().TechType)
				return true;
			string pfb = CraftData.GetClassIdForTechType(tt);
			if (pfb != null && VanillaFlora.getFromID(pfb) != null)
				return true;
			if (pfb != null && VanillaResources.getFromID(pfb) != null)
				return true;
			if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) && additionalScans.Contains(tt))
				return true;
			GameObject prefab = ObjectUtil.lookupPrefab(tt);
			if (prefab) {
				if (prefab.GetComponent<Creature>())
					return true;
				if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) && CraftData.GetTechType(prefab) == tt && prefab.GetComponentInChildren<Collider>()) { //interactable
					string key = PDAScanner.GetEntryData(tt).encyclopedia;
					if (!string.IsNullOrEmpty(key) && PDAEncyclopedia.mapping.ContainsKey(key)) { //scannable
						PDAEncyclopedia.EntryData ed = PDAEncyclopedia.mapping[key];
						if (ed != null && ed.key != "") { //has page, skip one dummied page
							if (ed.path.StartsWith("planetarygeology", StringComparison.InvariantCultureIgnoreCase) || ed.path.StartsWith("lifeforms", StringComparison.InvariantCultureIgnoreCase))
								return true;
						}
					}
				}
			}
			return false;
		}
		
		internal void onObjectSeen(GameObject go, bool identity, bool allowACU = false) { //call from getting attacked by, from mousing over, from touching, entering their ACU
			getRequiredLifeforms(); //populate list
			TechType tt = CraftData.GetTechType(go);
			if (requiredLifeforms.ContainsKey(tt)) {
				if (allowACU || !go.GetComponent<WaterParkItem>()) {
					requiredLifeforms[tt].seeAt(go, identity);
					needsPDAUpdate = DayNightCycle.main.timePassedAsFloat+0.5F;
				}
			}
		}
		
		internal void onBiomeDiscovered() {
			needsPDAUpdate = DayNightCycle.main.timePassedAsFloat+0.5F;
		}
			
		public string getLocalDescription(Vector3 pos) {
			BiomeBase bb = BiomeBase.getBiome(pos);
			string ret;
			if (bb != null && BiomeDiscoverySystem.instance.isDiscovered(bb)) {
				ret = WorldUtil.getRegionalDescription(pos, false);
			}
			else {
				ret = UNEXPLORED_LOCATION_TEXT;
				foreach (KeyValuePair<WorldUtil.CompassDirection, Vector3> kvp in WorldUtil.compassAxes) {
					BiomeBase near = BiomeBase.getBiome(pos+kvp.Value*250);
					if (near != null && BiomeDiscoverySystem.instance.isDiscovered(near)) {
						string opp = WorldUtil.getOpposite(kvp.Key).ToString();
						ret += " ("+opp[0]+opp.Substring(1).ToLowerInvariant()+" of "+near+")";
						break;
					}
				}
			}
			ret += ", "+(int)(-pos.y)+"m depth";
			if (bb.isCaveBiome())
				ret += " (Cave)";
			return ret;
		}
	    
	    class LifeformEntry : IComparable<LifeformEntry> {
	    	
			internal readonly TechType objectType;
			internal readonly string category;
			internal readonly PDAEncyclopedia.EntryData pdaPage;
			
			private readonly string hint;
			
			private Vector3 seenAt = Vector3.zero;
			private bool identityKnown;
			private bool seenACU;
			
			internal LifeformEntry(TechType tt) {
				objectType = tt;
				
				pdaPage = getEncyData();
				category = pdaPage == null ? "General" : SNUtil.getDescriptiveEncyPageCategoryName(pdaPage);
				if (tt == TechType.PrecursorDroid)
					category = Language.main.Get("EncyPath_Lifeforms/Fauna");
				else if (tt == TechType.PrecursorIonCrystal)
					category = Language.main.Get("EncyPath_PlanetaryGeology");
				
				hint = getHint(false);
				GameObject pfb = ObjectUtil.lookupPrefab(tt);
				if (pfb) {
					Creature c = pfb.GetComponent<Creature>();
					bool leviA = c is ReaperLeviathan || c is GhostLeviatanVoid || c is GhostLeviathan || c is SeaDragon;
					bool leviP = c is Reefback || c is SeaEmperorJuvenile || c is SeaEmperorBaby;
					if (DEIntegrationSystem.instance.isLoaded())
						leviA |= tt == DEIntegrationSystem.instance.getVoidThalassacean().TechType || tt == DEIntegrationSystem.instance.getGulper();
					if (leviA || leviP) {
						hint = leviA ? "Unknown Aggressive Leviathan" : "Unknown Leviathan";
					}
					else if (c is Warper || pfb.GetComponent<MeleeAttack>() || pfb.GetComponent<RangeAttacker>()) {
						hint = "Unknown Aggressive Fauna";
					}
					else if (c) {
						hint = "Unknown Fauna";
					}
					bool fauna = category.Contains("Fauna");
					bool flora = category.Contains("Flora");
					if (!leviA && !leviP && (fauna || flora)) {
						float size = 0;
						foreach (Renderer cc in pfb.GetComponentsInChildren<Renderer>(true))
							size += cc.bounds.size.magnitude;
						if (size >= 96 && fauna)
							hint += " - Leviathan";
						else if (size >= 32)
							hint += " - Large";
						else if (size <= 6F)
							hint += " - Small";
						else if (size <= 1.5F)
							hint += " - Tiny";
					}
					if (pdaPage != null && !leviA && !leviP && (flora || fauna)) {
						hint += ", "+Language.main.Get(pdaPage.nodes[pdaPage.nodes.Length-1]);
					}
				}
			}
			
			public bool isScanned() {
				return PDAScanner.complete.Contains(objectType);
			}
			
			public string getLastSeen() {
				return seenAt.magnitude > 0.5F ? (seenACU ? "In an ACU" : instance.getLocalDescription(seenAt)) : null;
			}
			
			public bool isIdentityKnown() {
				return identityKnown;
			}
			
			internal bool seeAt(GameObject go, bool identity) {
				Vector3 vec = go.transform.position;
				if (!(identity && !identityKnown) && seenAt.magnitude > 0.5F && (vec-seenAt).sqrMagnitude < 100)
					return false;
				seenAt = vec;
				identityKnown |= identity;
				if (go.GetComponent<WaterParkItem>()) {
					seenACU = true;
				}
				else {
					seenACU = false;
				}
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
