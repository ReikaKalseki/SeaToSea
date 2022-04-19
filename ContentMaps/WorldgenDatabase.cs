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
						string count = e.GetAttribute("count");
						int amt = string.IsNullOrEmpty(count) ? 1 : int.Parse(count);
						for (int i = 0; i < amt; i++) {
							CustomPrefab gen = CustomPrefab.fromXML(e);
							if (gen != null) {
								if (gen.isCrate) {
									GenUtil.spawnItemCrate(gen.position, gen.rotation);
							    	CrateFillMap.instance.addValue(gen.position, gen.tech);
								}
								else if (gen.isDatabox) {
							        GenUtil.spawnDatabox(gen.position, gen.rotation);
							    	DataboxTypingMap.instance.addValue(gen.position, gen.tech);
								}
								//else if (gen.isFragment) {
							    //    GenUtil.spawnFragment(gen.position, gen.rotation);
							    //	FragmentTypingMap.instance.addValue(gen.position, gen.tech);
								//}
								else {
									GenUtil.registerWorldgen(gen);
								}
								//TODO callbacks for manipulations!!!
								SBUtil.log("Loaded worldgen "+gen+" for "+e.InnerText);
							}
							else {
								SBUtil.log("No worldgen loadable for "+e.InnerText);
							}
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
