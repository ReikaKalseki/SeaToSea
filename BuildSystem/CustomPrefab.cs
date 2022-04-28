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
			
			[SerializeField]
			internal TechType tech = TechType.None;
			[SerializeField]
			internal readonly List<ManipulationBase> manipulations = new List<ManipulationBase>();
			
			public bool isCrate {get; private set;}
			public bool isFragment {get; private set;}
			public bool isDatabox {get; private set;}
			
			internal CustomPrefab(string pfb) : base(pfb) {				
				
			}
			
			internal CustomPrefab(PositionedPrefab pfb) : base(pfb) {				
				
			}
			
			internal override XmlElement asXML(XmlDocument doc) {
				XmlElement n = base.asXML(doc);
				if (tech != TechType.None)
					n.addProperty("tech", Enum.GetName(typeof(TechType), tech));
				if (manipulations.Count > 0) {
					XmlElement e = doc.CreateElement("objectManipulation");
					foreach (ManipulationBase mb in manipulations) {
						XmlElement e2 = doc.CreateElement(mb.GetType().Name);
						mb.saveToXML(e2);
						e.AppendChild(e2);
					}
					n.AppendChild(e);
				}
				return n;
			}
			
			public Action<GameObject> getManipulationsCallable() {
				return go => {
					foreach (ManipulationBase mb in manipulations) {
						mb.applyToObject(go);
					}
				};
			}
			
			public static new CustomPrefab fromXML(XmlElement e) {
				PositionedPrefab pfb = PositionedPrefab.fromXML(e);
				try {
					CustomPrefab b = new CustomPrefab(pfb);
					if (b.prefabName.StartsWith("res_", StringComparison.InvariantCultureIgnoreCase)) {
						b.prefabName = ((VanillaResources)typeof(VanillaResources).GetField(b.prefabName.Substring(4).ToUpper()).GetValue(null)).prefab;
					}
					else if (b.prefabName.StartsWith("fauna_", StringComparison.InvariantCultureIgnoreCase)) {
						b.prefabName = ((VanillaCreatures)typeof(VanillaCreatures).GetField(b.prefabName.Substring(6).ToUpper()).GetValue(null)).prefab;
					}
					else if (b.prefabName == "crate") {
						b.prefabName = "15a3e67b-0c76-4e8d-889e-66bc54213dac";
						b.isCrate = true;
						string tech = e.getProperty("item");
						TechType techt = SBUtil.getTechType(tech);
						b.tech = techt;
					}
					else if (b.prefabName == "databox") {
						b.prefabName = "1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6";
						b.isDatabox = true;
						string tech = e.getProperty("tech");
						TechType techt = SBUtil.getTechType(tech);
						b.tech = techt;
					}
					//else if (b.prefabName == "fragment") {
					//	b.prefabName = ?;
					//	b.isFragment = true;
					//	string tech = e.getProperty("type");
					//	TechType techt = (TechType)Enum.Parse(typeof(TechType), tech);
					//}
					string tech2 = e.getProperty("tech", true);
					if (b.tech == TechType.None && tech2 != null && tech2 != "None") {
						b.tech = SBUtil.getTechType(tech2);
					}
					if (glob != null)
						b.manipulations.AddRange(glob);
					List<XmlElement> li = e.getDirectElementsByTagName("objectManipulation");
					if (li.Count == 1) {
						loadManipulations(li[0], b.manipulations);
					}
					return b;
				}
				catch (Exception ex) {
					SBUtil.log("Could not construct customprefab from XML: "+ex);
					return null;
				}
			}
			
			public static void loadManipulations(XmlElement e, List<ManipulationBase> li) {
				foreach (XmlElement e2 in e.ChildNodes) {
					ManipulationBase mb = loadManipulation(e2);
					if (mb != null)
						li.Add(mb);
				}
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
