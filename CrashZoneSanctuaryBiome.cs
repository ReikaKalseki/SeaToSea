using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Json;
using SMLHelper.V2.Utility;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class CrashZoneSanctuaryBiome : CustomBiome {
		
		public static readonly float biomeRadius = 120;
		public static readonly Vector3 biomeCenter = new Vector3(1111.16F, -360.5F, -985F);
		
		public static Color waterColor = new Color(0.25F, 0.75F, 1F);
		public static readonly string biomeName = "Sanctuary";
		
		public static readonly CrashZoneSanctuaryBiome instance = new CrashZoneSanctuaryBiome();
		
		private CrashZoneSanctuaryBiome() : base(biomeName) {
			
		}
		
		public override void register() {/*
			GenUtil.registerWorldgen(new PositionedPrefab(SeaToSeaMod.crashSanctuarySpawner.ClassID, biomeCenter));
			
			UnityEngine.Random.InitState(873451871);
			for (int i = 0; i < 180; i++) {
				Vector3 pos = MathUtil.getRandomVectorAround(biomeCenter, new Vector3(biomeRadius, 0, biomeRadius)).setY(-300);
				if (isInBiome(pos))
					GenUtil.registerWorldgen(new PositionedPrefab(SeaToSeaMod.sanctuaryGrassSpawner.ClassID, pos));
			}*/
		}
		
		public override VanillaMusic[] getMusicOptions() {
			return new VanillaMusic[]{VanillaMusic.COVE};
		}
		
		public override bool isCaveBiome() {
			return false;
		}
		
		public override bool isInBiome(Vector3 pos) {
			return Vector3.Distance(pos, biomeCenter) <= biomeRadius;
		}
		
		public override double getDistanceToBiome(Vector3 vec) {
			return Math.Max(0, Vector3.Distance(vec, biomeCenter)-biomeRadius);
		}
		
		public override float getMurkiness(float orig) {
			return 0.99F;
		}
		
		public override float getScatteringFactor(float orig) {
			return orig;
		}
		
		public override Vector3 getColorFalloff(Vector3 orig) {
			return new Vector3(40, 3.2F, 2.5F)*0.8F;
		}
		
		public override float getFogStart(float orig) {
			return 18;
		}
		
		public override float getScatterFactor(float orig) {
			return orig;
		}
		
		public override Color getWaterColor(Color orig) {
			return orig;
		}
		
		public override float getSunScale(float orig) {
			return 0.5F;
		}
		
		public static void cleanPlantOverlap() { //called manually to compute prebaked positions
			HashSet<Vector3> positions = new HashSet<Vector3>();
			foreach (SanctuaryPlantTag sp in UnityEngine.Object.FindObjectsOfType<SanctuaryPlantTag>()) {
				Vector3 pos = sp.transform.position;
				if (!instance.isInBiome(pos))
					continue;
				positions.Add(pos);
				foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(pos, 2.5F)) { //does not find the grass because no collider
					if (CrashZoneSanctuarySpawner.spawnsPlant(pi.ClassId))
						UnityEngine.Object.DestroyImmediate(pi.gameObject);
				}
			}
			HashSet<Vector3> satellitePositions = new HashSet<Vector3>();
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
				if (pi && CrashZoneSanctuarySpawner.spawnsPlant(pi.ClassId))
					satellitePositions.Add(pi.transform.position);
			}
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
				if (pi && (pi.ClassId == SeaToSeaMod.crashSanctuaryGrass.ClassID || pi.ClassId == SeaToSeaMod.sanctuaryGrassBump.ClassID)) {
					foreach (Vector3 pos in positions) {
						if (Vector3.Distance(pos, pi.transform.position) <= 3.75F) {
							UnityEngine.Object.DestroyImmediate(pi.gameObject);
							break;
						}
					}
					if (!pi || !pi.transform)
						continue;
					foreach (Vector3 pos in satellitePositions) {
						if (Vector3.Distance(pos, pi.transform.position) <= 2F) {
							UnityEngine.Object.DestroyImmediate(pi.gameObject);
							break;
						}
					}
				}
			}
		}
		
		public static void dumpPlantData() {
			string path = BuildingHandler.instance.getDumpFile("sanctuary_plants");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			
			foreach (SanctuaryPlantTag sp in UnityEngine.Object.FindObjectsOfType<SanctuaryPlantTag>()) {
				Vector3 pos = sp.transform.position;
				if (!instance.isInBiome(pos))
					continue;
				PositionedPrefab pfb = new PositionedPrefab(sp.GetComponent<PrefabIdentifier>());				
				XmlElement e = doc.CreateElement("object");
				pfb.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
			
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
				if (pi && (pi.ClassId == SeaToSeaMod.crashSanctuaryGrass.ClassID || pi.ClassId == SeaToSeaMod.sanctuaryGrassBump.ClassID)) {
					PositionedPrefab pfb = new PositionedPrefab(pi);				
					XmlElement e = doc.CreateElement("grass");
					pfb.saveToXML(e);
					doc.DocumentElement.AppendChild(e);
				}
			}
			
			doc.Save(path);
		}
	}
}
