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
						TechType techt = (TechType)Enum.Parse(typeof(TechType), tech);
						b.tech = techt;
					}
					else if (b.prefabName == "databox") {
						b.prefabName = "1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6";
						b.isDatabox = true;
						string tech = e.getProperty("tech");
						TechType techt = (TechType)Enum.Parse(typeof(TechType), tech);
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
						b.tech = (TechType)Enum.Parse(typeof(TechType), tech2);
					}
					List<XmlElement> li = e.getDirectElementsByTagName("objectManipulation");
					if (li.Count == 1) {
						foreach (XmlElement e2 in li[0].ChildNodes) {
							Type t = Type.GetType("ReikaKalseki.SeaToSea."+e2.Name);
							ManipulationBase mb = (ManipulationBase)t.GetConstructor(new Type[0]).Invoke(new object[0]);
							mb.loadFromXML(e2);
							b.manipulations.Add(mb);
						}
					}
					return b;
				}
				catch (Exception ex) {
					SBUtil.log("Could not construct customprefab from XML: "+ex);
					return null;
				}
			}
			
		}
}
