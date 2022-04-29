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
							ObjectTemplate ot = ObjectTemplate.construct(e);
							if (ot == null) {
								throw new Exception("No worldgen loadable for '"+e.Name+"' "+e.InnerText+": NULL");
							}
							else if (ot is CustomPrefab) {
								CustomPrefab pfb = (CustomPrefab)ot;
								if (pfb.isCrate) {
									GenUtil.spawnItemCrate(pfb.position, pfb.tech, pfb.rotation);
							    	//CrateFillMap.instance.addValue(gen.position, gen.tech);
								}
								else if (pfb.isDatabox) {
							        GenUtil.spawnDatabox(pfb.position, pfb.tech, pfb.rotation);
							    	//DataboxTypingMap.instance.addValue(gen.position, gen.tech);
								}
								//else if (gen.isFragment) {
							    //    GenUtil.spawnFragment(gen.position, gen.rotation);
							    //	FragmentTypingMap.instance.addValue(gen.position, gen.tech);
								//}
								else {
									GenUtil.registerWorldgen(pfb, pfb.getManipulationsCallable());
								}
								SBUtil.log("Loaded worldgen prefab "+pfb+" for "+e.InnerText);
							}
							else if (ot is WorldGenerator) {
								WorldGenerator gen = (WorldGenerator)ot;
								GenUtil.registerWorldgen(gen);
								SBUtil.log("Loaded worldgenator "+gen+" for "+e.InnerText);
							}
							else {
								throw new Exception("No worldgen loadable for '"+e.Name+"' "+e.InnerText);
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
