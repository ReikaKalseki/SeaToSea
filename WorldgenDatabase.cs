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
						if (gen != null) {
							if (gen.prefabName == "databox") {
								string tech = e.getProperty("tech");
								TechType techt = (TechType)Enum.Parse(typeof(TechType), tech);
								spawnDatabox(techt, gen.position, gen.rotation);
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
    
	    private void spawnDatabox(TechType tech, Vector3 pos) {
	    	spawnDatabox(tech, pos, Quaternion.identity);
	    }
	    
	    private void spawnDatabox(TechType tech, Vector3 pos, double rotY) {
			spawnDatabox(tech, pos, Quaternion.Euler(0, (float)rotY, 0));
	    }
	    
	    private void spawnDatabox(TechType tech, Vector3 pos, Quaternion rot) {
	        GenUtil.spawnDatabox(pos, rot);
	    	DataboxTypingMap.instance.addValue(pos, tech);
	    }
	}
}
