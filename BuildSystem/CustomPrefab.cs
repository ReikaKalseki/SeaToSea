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
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

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
			
			public bool isBasePiece {get; private set;}
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
		
			public override string getTagName() {
				return TAGNAME;
			}
			
			public override void saveToXML(XmlElement e) {
				base.saveToXML(e);
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
					tech = SBUtil.getTechType(techn);
					prefabName = GenUtil.getOrCreateCrate(tech, e.getBoolean("sealed"));
				}
				else if (prefabName == "databox") {
					isDatabox = true;
					string techn = e.getProperty("tech");
					tech = SBUtil.getTechType(techn);
					prefabName = GenUtil.getOrCreateDatabox(tech);
				}
				else if (prefabName == "pda") {
					isPDA = true;
					string pagen = e.getProperty("page");
					PDAManager.PDAPage page = PDAManager.getPage(pagen);
					prefabName = page.getPDAClassID();
				}
				//else if (prefabName == "fragment") {
				//	prefabName = ?;
				//	isFragment = true;
				//	string techn = e.getProperty("type");
				//	tech = SBUtil.getTechType(techn);
				//}
				string tech2 = e.getProperty("tech", true);
				if (tech == TechType.None && tech2 != null && tech2 != "None") {
					tech = SBUtil.getTechType(tech2);
				}
				loadManipulations(e.OwnerDocument.DocumentElement.getAllChildrenIn("transforms"), manipulations); 
				List<XmlElement> li = e.getDirectElementsByTagName("objectManipulation");
				if (li.Count == 1) {
					loadManipulations(li[0], manipulations);
				}
				if (manipulations.Count > 0) {
					bool needReapply = false;
					foreach (ManipulationBase mb in manipulations) {
						if (mb.needsReapplication()) {
							needReapply = true;
							break;
						}
					}
					if (needReapply) {
						string xmlKey = prefabName+"##"+System.Security.SecurityElement.Escape(li[0].InnerXml);
						customPrefab = getOrCreateModPrefab(xmlKey);
						prefabName = customPrefab.ClassID;
						tech = customPrefab.TechType;
					}
				}
			}
			
			private ModifiedObjectPrefab getOrCreateModPrefab(string key) {
				ModifiedObjectPrefab pfb = prefabCache.ContainsKey(key) ? prefabCache[key] : null;
				if (pfb == null) {
					pfb = new ModifiedObjectPrefab(key, prefabName, manipulations);
					prefabCache[key] = pfb;
					pfb.Patch();
					TechType from = tech != TechType.None ? tech : CraftData.entClassTechTable.GetOrDefault(key, TechType.None);
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
					SBUtil.log("Created customprefab GO template: "+key+" ["+from+"] > "+pfb);
				}
				else {
					SBUtil.log("Using already-generated prefab for GO template: "+key+" > "+pfb);
				}
				return pfb;
			}
			
			public static void loadManipulations(XmlNodeList es, List<ManipulationBase> li) {
				if (es == null)
					return;
				foreach (XmlElement e2 in es) {
					ManipulationBase mb = loadManipulation(e2);
					if (mb != null)
						li.Add(mb);
				}
			}
			
			private static void loadManipulations(XmlElement e, List<ManipulationBase> li) {
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
					SBUtil.writeToChat("Could not rebuild manipulation from XML "+e2.Name+"/"+e2.InnerText+": "+ex);
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
