/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

using ReikaKalseki.SeaToSea;
using ReikaKalseki.DIAlterra;


namespace ReikaKalseki.SeaToSea
{		
		[Serializable]
		internal class CustomPrefab : PositionedPrefab {
		
			public static readonly string TAGNAME = "customprefab";
			
			private static readonly Dictionary<string, ModifiedObjectPrefab> prefabCache = new Dictionary<string, ModifiedObjectPrefab>();
			
			[SerializeField]
			internal TechType tech = TechType.None;
			[SerializeField]
			internal readonly List<ManipulationBase> manipulations = new List<ManipulationBase>();
			
			public bool isSeabase {get; protected set;}
			public bool isBasePiece {get; internal set;}
			public bool isCrate {get; private set;}
			public bool isFragment {get; private set;}
			public bool isDatabox {get; private set;}
			public bool isPDA {get; private set;}
			
			public ModifiedObjectPrefab customPrefab {get; private set;}
			
			internal CustomPrefab(string pfb) : base(pfb) {				
				
			}
			
			internal CustomPrefab(PositionedPrefab pfb) : base(pfb) {				
				
			}
			
			static CustomPrefab() {
				registerType(TAGNAME, e => new CustomPrefab(e.getProperty("prefab")));
			}
			
			public void setSeabase() {
				isSeabase = true;
				prefabName = "seabase";
			}
		
			public override string getTagName() {
				return TAGNAME;
			}
			
			public override void saveToXML(XmlElement e) {
				string n = prefabName;
				if (isBasePiece) {
					e.addProperty("piece", prefabName);
					prefabName = "basePart";
				}
				base.saveToXML(e);
				prefabName = n;
				if (tech != TechType.None)
					e.addProperty("tech", Enum.GetName(typeof(TechType), tech));
				if (manipulations.Count > 0) {
					XmlElement e1 = e.OwnerDocument.CreateElement("objectManipulation");
					foreach (ManipulationBase mb in manipulations) {
						XmlElement e2 = e.OwnerDocument.CreateElement(mb.GetType().Name);
						mb.saveToXML(e2);
						e1.AppendChild(e2);
					}
					e.AppendChild(e1);
				}
			}
			
			public Action<GameObject> getManipulationsCallable() {
				return go => {
					foreach (ManipulationBase mb in manipulations) {
						mb.applyToObject(go);
					}
				};
			}
			
			public override GameObject createWorldObject() {
				if (isBasePiece) {
					GameObject go = ObjectUtil.getBasePiece(prefabName);
					if (go != null) {
						go.transform.position = position;
						go.transform.rotation = rotation;
						go.transform.localScale = scale;
					}
					return go;
				}
				else {
					return base.createWorldObject();
				}
			}
			
			public override void loadFromXML(XmlElement e) {
				base.loadFromXML(e);
				if (prefabName.StartsWith("res_", StringComparison.InvariantCultureIgnoreCase)) {
					prefabName = ((VanillaResources)typeof(VanillaResources).GetField(prefabName.Substring(4).ToUpper()).GetValue(null)).prefab;
				}
				else if (prefabName.StartsWith("fauna_", StringComparison.InvariantCultureIgnoreCase)) {
					prefabName = ((VanillaCreatures)typeof(VanillaCreatures).GetField(prefabName.Substring(6).ToUpper()).GetValue(null)).prefab;
				}
				else if (prefabName.StartsWith("flora_", StringComparison.InvariantCultureIgnoreCase)) {
					prefabName = VanillaFlora.getByName(prefabName.Substring(6)).getRandomPrefab(false);
				}
				else if (prefabName.StartsWith("base_", StringComparison.InvariantCultureIgnoreCase)) {
					isBasePiece = true;
				}
				else if (prefabName == "crate") {
					isCrate = true;
					string techn = e.getProperty("item");
					tech = SNUtil.getTechType(techn);
					prefabName = GenUtil.getOrCreateCrate(tech, e.getBoolean("sealed")).ClassID;
					SNUtil.log("Redirected customprefab to crate "+prefabName);
				}
				else if (prefabName == "databox") {
					isDatabox = true;
					string techn = e.getProperty("tech");
					tech = SNUtil.getTechType(techn);
					prefabName = GenUtil.getOrCreateDatabox(tech).ClassID;
					SNUtil.log("Redirected customprefab to databox "+prefabName);
				}
				else if (prefabName == "fragment") {
					isFragment = true;
					string techn = e.getProperty("tech");
					tech = SNUtil.getTechType(techn);
					GenUtil.ContainerPrefab g = GenUtil.getFragment(tech, e.getInt("index", 0));
					if (g == null)
						throw new Exception("No such fragment!");
					prefabName = g.ClassID;
					SNUtil.log("Redirected customprefab to fragment "+prefabName);
				}
				else if (prefabName == "pda") {
					isPDA = true;
					string pagen = e.getProperty("page");
					PDAManager.PDAPage page = PDAManager.getPage(pagen);
					prefabName = page.getPDAClassID();
					SNUtil.log("Redirected customprefab to pda "+prefabName);
				}
				else if (prefabName == "basePart") {
					isBasePiece = true;
					prefabName = e.getProperty("piece");
					List<XmlElement> li0 = e.getDirectElementsByTagName("supportData");
					if (li0.Count == 1)
						manipulations.Add(new SeabaseLegLengthPreservation(li0[0]));
					SNUtil.log("Redirected customprefab to base piece "+prefabName+" >> "+li0.Count+"::"+string.Join(", ", li0.Select<XmlElement, string>(el => el.OuterXml)));
				}
				else if (prefabName == "seabase") {
					prefabName = "e9b75112-f920-45a9-97cc-838ee9b389bb"; //base GO
					isSeabase = true;
					manipulations.Add(new SeabaseReconstruction(e));
					SNUtil.log("Redirected customprefab to seabase");
				}
				//else if (prefabName == "fragment") {
				//	prefabName = ?;
				//	isFragment = true;
				//	string techn = e.getProperty("type");
				//	tech = SNUtil.getTechType(techn);
				//}
				string tech2 = e.getProperty("tech", true);
				if (tech == TechType.None && tech2 != null && tech2 != "None") {
					tech = SNUtil.getTechType(tech2);
				}
				XmlNodeList xli = e.OwnerDocument.DocumentElement != null ? e.OwnerDocument.DocumentElement.getAllChildrenIn("transforms") : null;
				if (xli != null)
					loadManipulations(xli, manipulations); 
				List<XmlElement> li = e.getDirectElementsByTagName("objectManipulation");
				if (li.Count == 1) {
					ModifiedObjectPrefab mod = getManipulatedObject(li[0], this);
					if (mod != null) {
						prefabName = mod.ClassID;
						tech = mod.TechType;
					}
				}
			}
			
			public static ModifiedObjectPrefab getManipulatedObject(XmlElement e, CustomPrefab pfb) {
				loadManipulations(e, pfb.manipulations);
				if (pfb.manipulations.Count > 0) {
					bool needReapply = false;
					foreach (ManipulationBase mb in pfb.manipulations) {
						if (mb.needsReapplication()) {
							needReapply = true;
							break;
						}
					}
					if (needReapply) {
						string xmlKey = pfb.prefabName+"##"+System.Security.SecurityElement.Escape(e.InnerXml);
						return getOrCreateModPrefab(pfb, xmlKey);
					}
				}
				return null;
			}
			
			private static ModifiedObjectPrefab getOrCreateModPrefab(CustomPrefab orig, string key) {
				ModifiedObjectPrefab pfb = prefabCache.ContainsKey(key) ? prefabCache[key] : null;
				if (pfb == null) {
					pfb = new ModifiedObjectPrefab(key, orig.prefabName, orig.manipulations);
					prefabCache[key] = pfb;
					pfb.Patch();
					TechType from = orig.tech != TechType.None ? orig.tech : CraftData.entClassTechTable.GetOrDefault(key, TechType.None);
					if (from != TechType.None) {
						KnownTechHandler.Main.SetAnalysisTechEntry(pfb.TechType, new List<TechType>(){from});
						PDAScanner.EntryData e = new PDAScanner.EntryData();
						e.key = pfb.TechType;
						e.blueprint = from;
						e.destroyAfterScan = false;
						e.locked = true;
						e.scanTime = 5;
						PDAHandler.AddCustomScannerEntry(e);
					}
					SNUtil.log("Created customprefab GO template: "+key+" ["+from+"] > "+pfb);
				}
				else {
					SNUtil.log("Using already-generated prefab for GO template: "+key+" > "+pfb);
				}
				return pfb;
			}
			
			internal static void loadManipulations(XmlNodeList es, List<ManipulationBase> li) {
				if (es == null)
					return;
				foreach (XmlElement e2 in es) {
					ManipulationBase mb = loadManipulation(e2);
					if (mb != null)
						li.Add(mb);
				}
			}
			
			internal static void loadManipulations(XmlElement e, List<ManipulationBase> li) {
				loadManipulations(e.ChildNodes, li);
			}
			
			public static ManipulationBase loadManipulation(XmlElement e2) {
				try {
					if (e2 == null)
						throw new Exception("Null XML elem");
					Type t = InstructionHandlers.getTypeBySimpleName("ReikaKalseki.SeaToSea."+e2.Name);
					if (t == null)
						throw new Exception("Type '"+e2.Name+"' not found");
					System.Reflection.ConstructorInfo ct = t.GetConstructor(new Type[0]);
					if (ct == null)
						throw new Exception("Constructor not found");
					try {
						ManipulationBase mb = (ManipulationBase)ct.Invoke(new object[0]);
						mb.loadFromXML(e2);
						return mb;
					}
					catch (Exception ex) {
						throw new Exception("Construction error "+ex);
					}
				}
				catch (Exception ex) {
					string err = "Could not rebuild manipulation from XML "+e2.Name+"/"+e2.InnerText+": "+ex;
					SNUtil.log(err);
					SNUtil.writeToChat(err);
					return null;
				}
			}
			
		}
		
		class ModifiedObjectPrefab : GenUtil.CustomPrefabImpl {
		
			private readonly List<ManipulationBase> mods = new List<ManipulationBase>();
	        
			internal ModifiedObjectPrefab(string key, string template, List<ManipulationBase> li) : base(key, template) {
				mods = li;
	        }
			
			public override sealed void prepareGameObject(GameObject go, Renderer r) {
				foreach (ManipulationBase mb in mods) {
					mb.applyToObject(go);
				}
			}
			
			public override string ToString()
			{
				return "Modified "+baseTemplate.prefab+getString(mods);
			}
			
			private static string getString(List<ManipulationBase> li) {
				return " x"+li.Count+"="+string.Join("/", li.Select(mb => mb.GetType().Name));
			}
 
			
		}
}
