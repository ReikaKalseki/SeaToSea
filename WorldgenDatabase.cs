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
						WorldGen gen = WorldGen.getGen(e);
						if (gen != null) {
							
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
	
		private class WorldGen {
			
			internal static WorldGen getGen(XmlElement e) {
				return null;
			}
			
		}
	}
}
