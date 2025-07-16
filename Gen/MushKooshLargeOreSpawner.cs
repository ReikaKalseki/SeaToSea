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
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class MushKooshLargeOreSpawner : WorldGenerator {
		
		private Quaternion rotation;
		private bool isKoosh;
		
		private static readonly WeightedRandom<VanillaResources> kooshTable = new WeightedRandom<VanillaResources>();
		private static readonly WeightedRandom<VanillaResources> mushroomTable = new WeightedRandom<VanillaResources>();
	        
		static MushKooshLargeOreSpawner() {
			kooshTable.addEntry(VanillaResources.LARGE_MERCURY, 20);
			kooshTable.addEntry(VanillaResources.LARGE_LITHIUM, 25);
			kooshTable.addEntry(VanillaResources.LARGE_RUBY, 20);
			
			mushroomTable.addEntry(VanillaResources.LARGE_DIAMOND, 30);
			mushroomTable.addEntry(VanillaResources.LARGE_LITHIUM, 30);
			mushroomTable.addEntry(VanillaResources.LARGE_SALT, 20);
	    }
	        
	    public MushKooshLargeOreSpawner(Vector3 pos) : base(pos) {
			
	    }
		
		public override void saveToXML(XmlElement e) {
			PositionedPrefab.saveRotation(e, rotation);
			e.addProperty("isKoosh", isKoosh);
		}
		
		public override void loadFromXML(XmlElement e) {
			rotation = PositionedPrefab.readRotation(e);
			isKoosh = e.getBoolean("isKoosh");
		}
			
	    public override bool generate(List<GameObject> li) {
			if (WorldUtil.getObjectsNearWithComponent<Drillable>(position, 6).Count == 0 && UnityEngine.Random.Range(0F, 1F) < (isKoosh ? 0.8F : 0.6F)) {
				VanillaResources res = (isKoosh ? kooshTable : mushroomTable).getRandomEntry();
				GameObject obj = spawner(res.prefab);
				obj.transform.position = position;
				obj.transform.rotation = rotation;
				li.Add(obj);
			}
			return true;
		}
		
		public override LargeWorldEntity.CellLevel getCellLevel() {
			return LargeWorldEntity.CellLevel.Medium;
		}
			
	}
}
