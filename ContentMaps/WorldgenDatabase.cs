using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class WorldgenDatabase
	{
		public static readonly WorldgenDatabase instance = new WorldgenDatabase();
		
		private WorldgenDatabase() {
			
		}
		
		public void load() {
			string xml = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "worldgen.xml");
			if (File.Exists(xml)) {
				SBUtil.log("Loading worldgen map from XML @ "+xml);
				XmlDocument doc = new XmlDocument();
				doc.Load(xml);
				foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
					try {
						PositionedPrefab gen = PositionedPrefab.fromXML(e);
						//PlacedObject obj = PlacedObject.fromXML(e, gen, false);
						if (gen != null) {
							if (gen.prefabName.StartsWith("res_", StringComparison.InvariantCultureIgnoreCase)) {
								gen.prefabName = ((VanillaResources)typeof(VanillaResources).GetField(gen.prefabName.Substring(4).ToUpper()).GetValue(null)).prefab;
							}
							else if (gen.prefabName.StartsWith("fauna_", StringComparison.InvariantCultureIgnoreCase)) {
								gen.prefabName = ((VanillaCreatures)typeof(VanillaCreatures).GetField(gen.prefabName.Substring(6).ToUpper()).GetValue(null)).prefab;
							}
							else if (gen.prefabName == "crate") {
								string tech = e.getProperty("item");
								TechType techt = (TechType)Enum.Parse(typeof(TechType), tech);
								GenUtil.spawnItemCrate(gen.position, gen.rotation);
						    	CrateFillMap.instance.addValue(gen.position, techt);
							}
							else if (gen.prefabName == "databox") {
								string tech = e.getProperty("tech");
								TechType techt = (TechType)Enum.Parse(typeof(TechType), tech);
						        GenUtil.spawnDatabox(gen.position, gen.rotation);
						    	DataboxTypingMap.instance.addValue(gen.position, techt);
							}
							else {
								GenUtil.registerWorldgen(gen);
							}
							SBUtil.log("Loaded worldgen "+gen+" for "+e.InnerText);
						}
						else {
							SBUtil.log("No worldgen loadable for "+e.InnerText);
						}
					}
					catch (Exception ex) {
						SBUtil.log("Could not load element "+e.InnerText);
						SBUtil.log(ex.ToString());
					}
				}
			}
			else {
				SBUtil.log("Databox XML not found!");
			}
		}
	}
}
