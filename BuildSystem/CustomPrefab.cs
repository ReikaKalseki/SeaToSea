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
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{		
		[Serializable]
		internal class CustomPrefab : PositionedPrefab {
		
			public static readonly string TAGNAME = "customprefab";
			
			[SerializeField]
			internal TechType tech = TechType.None;
			[SerializeField]
			internal readonly List<ManipulationBase> manipulations = new List<ManipulationBase>();
			
			public bool isBasePiece {get; private set;}
			public bool isCrate {get; private set;}
			public bool isFragment {get; private set;}
			public bool isDatabox {get; private set;}
			public bool isPDA {get; private set;}
			
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
				else if (prefabName.StartsWith("base_", StringComparison.InvariantCultureIgnoreCase)) {
					isBasePiece = true;
				}
				else if (prefabName == "crate") {
					prefabName = "15a3e67b-0c76-4e8d-889e-66bc54213dac";
					isCrate = true;
					string techn = e.getProperty("item");
					tech = SBUtil.getTechType(techn);
				}
				else if (prefabName == "databox") {
					prefabName = "1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6";
					isDatabox = true;
					string techn = e.getProperty("tech");
					tech = SBUtil.getTechType(techn);
				}
				else if (prefabName == "pda") {
					prefabName = "02dbd99a-a279-4678-9be7-a21202862cb7";
					isPDA = true;
					string pagen = e.getProperty("page");
					manipulations.Add(new SetPDAPage(pagen));
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
}
