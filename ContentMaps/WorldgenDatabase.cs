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
			string root = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string folder = Path.Combine(root, "XML/WorldgenSets");
			string xml = Path.Combine(root, "XML/worldgen.xml");
			if (Directory.Exists(folder)) {
				string[] files = Directory.GetFiles(folder);
				SBUtil.log("Loading worldgen maps from folder '"+folder+"': "+string.Join(", ", files));
				foreach (string file in files) {
					loadXML(file);
				}
			}
			else if (File.Exists(xml)) {
				loadXML(xml);
			}
			else {
				SBUtil.log("Worldgen XML not found!");
			}
		}
		
		private void loadXML(string xml) {
			SBUtil.log("Loading worldgen map from XML @ "+xml);
			XmlDocument doc = new XmlDocument();
			doc.Load(xml);
			foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
				try {
					string count = e.GetAttribute("count");
					string ch = e.GetAttribute("chance");
					int amt = string.IsNullOrEmpty(count) ? 1 : int.Parse(count);
					double chance = string.IsNullOrEmpty(ch) ? 1 : double.Parse(ch);
					for (int i = 0; i < amt; i++) {
						if (UnityEngine.Random.Range(0F, 1F) <= chance) {
							CustomPrefab gen = CustomPrefab.fromXML(e);
							if (gen != null) {
								if (gen.isCrate) {
									GenUtil.spawnItemCrate(gen.position, gen.tech, gen.rotation);
							    	//CrateFillMap.instance.addValue(gen.position, gen.tech);
								}
								else if (gen.isDatabox) {
							        GenUtil.spawnDatabox(gen.position, gen.tech, gen.rotation);
							    	//DataboxTypingMap.instance.addValue(gen.position, gen.tech);
								}
								//else if (gen.isFragment) {
							    //    GenUtil.spawnFragment(gen.position, gen.rotation);
							    //	FragmentTypingMap.instance.addValue(gen.position, gen.tech);
								//}
								else {
									GenUtil.registerWorldgen(gen, gen.getManipulationsCallable());
								}
								//TODO callbacks for manipulations!!!
								SBUtil.log("Loaded worldgen "+gen+" for "+e.InnerText);
							}
							else {
								SBUtil.log("No worldgen loadable for "+e.InnerText);
							}
						}
					}
				}
				catch (Exception ex) {
					SBUtil.log("Could not load element "+e.InnerText);
					SBUtil.log(ex.ToString());
				}
			}
		}
	}
}
