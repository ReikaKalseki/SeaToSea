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
		private static readonly float radiusFuzz = 24;
		public static readonly Vector3 biomeCenter = new Vector3(1111.16F, -360.5F, -985F);
		private static readonly Simplex3DGenerator edgeFuzz = (Simplex3DGenerator)new Simplex3DGenerator(2376547).setFrequency(0.1);
		
		public static readonly string biomeName = "Glowing Sanctuary";
		
		public static readonly CrashZoneSanctuaryBiome instance = new CrashZoneSanctuaryBiome();
		
		private readonly Dictionary<VanillaCreatures, int> creatureCounts = new Dictionary<VanillaCreatures, int>();
		
		private CrashZoneSanctuaryBiome() : base(biomeName, 1F) {
			creatureCounts[VanillaCreatures.BLADDERFISH] = 36;
			creatureCounts[VanillaCreatures.BOOMERANG] = 48;
			creatureCounts[VanillaCreatures.CAVECRAWLER] = 27;
			creatureCounts[VanillaCreatures.GASOPOD] = 4;
			creatureCounts[VanillaCreatures.HOOPFISH] = 90;
			creatureCounts[VanillaCreatures.MESMER] = 6;
			
			creatureCounts[VanillaCreatures.SCHOOL_HOOPFISH] = 6;
			creatureCounts[VanillaCreatures.SCHOOL_HOLEFISH] = 3;
			creatureCounts[VanillaCreatures.SCHOOL_BOOMERANG] = 6;
			creatureCounts[VanillaCreatures.SCHOOL_BLADDERFISH] = 3;
		}
		
		public override void register() {/* prebaked
			GenUtil.registerWorldgen(new PositionedPrefab(SeaToSeaMod.crashSanctuarySpawner.ClassID, biomeCenter));
			
			UnityEngine.Random.InitState(873451871);
			for (int i = 0; i < 160; i++) {
				Vector3 pos = MathUtil.getRandomVectorAround(biomeCenter, new Vector3(biomeRadius, 0, biomeRadius)*0.8F).setY(-300);
				if (isInBiome(pos))
					GenUtil.registerWorldgen(new PositionedPrefab(SeaToSeaMod.sanctuaryGrassSpawner.ClassID, pos));
			}
			*/
			foreach (KeyValuePair<VanillaCreatures, int> kvp in creatureCounts) {
				for (int i = 0; i < kvp.Value; i++) {
					Vector3 pos = MathUtil.getRandomVectorAround(biomeCenter, new Vector3(biomeRadius, 0, biomeRadius)*0.67F).setY(-340);
					if (isInBiome(pos))
						GenUtil.registerWorldgen(new PositionedPrefab(kvp.Key.getPrefabID(), pos));
				}
			}
		}
		
		public override VanillaMusic[] getMusicOptions() {
			return new VanillaMusic[]{VanillaMusic.COVE};
		}
		
		public override bool isCaveBiome() {
			return false;
		}
		
		public override bool isInBiome(Vector3 pos) {
			float dist = Vector3.Distance(pos, biomeCenter);
			if (dist > biomeRadius+radiusFuzz)
				return false;
			return dist <= biomeRadius+edgeFuzz.getValue(pos)*radiusFuzz;
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
			return new Vector3(40, 3.12F, 2.75F)*0.875F;
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
			HashSet<Vector3> fernPositions = new HashSet<Vector3>();
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
				if (pi && pi.ClassId == SeaToSeaMod.crashSanctuaryFern.ClassID) {
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
					if (!pi || !pi.transform)
						continue;
					foreach (Vector3 pos in fernPositions) {
						if (Vector3.Distance(pos, pi.transform.position) <= 0.3F) {
							UnityEngine.Object.DestroyImmediate(pi.gameObject);
							break;
						}
					}
					if (!pi || !pi.transform)
						continue;
					fernPositions.Add(pi.transform.position);
				}
			}
		}
		
		public static void dumpPlantData() {
			string path = BuildingHandler.instance.getDumpFile("sanctuary_plants");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			
			foreach (SanctuaryPlantTag sp in UnityEngine.Object.FindObjectsOfType<SanctuaryPlantTag>()) {
				if (!instance.isInBiome(sp.transform.position))
					continue;
				PositionedPrefab pfb = new PositionedPrefab(sp.GetComponent<PrefabIdentifier>());
				XmlElement e = doc.CreateElement("flame");
				pfb.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
			
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
				if (pi && CrashZoneSanctuarySpawner.spawnsPlant(pi.ClassId) && instance.isInBiome(pi.transform.position)) {
					PositionedPrefab pfb = new PositionedPrefab(pi);
					XmlElement e = doc.CreateElement("plant");
					pfb.saveToXML(e);
					doc.DocumentElement.AppendChild(e);
				}
			}
			
			foreach (PrefabIdentifier pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
				if (pi && pi.ClassId == SeaToSeaMod.crashSanctuaryFern.ClassID) {
					PositionedPrefab pfb = new PositionedPrefab(pi);				
					XmlElement e = doc.CreateElement("fern");
					pfb.saveToXML(e);
					doc.DocumentElement.AppendChild(e);
				}
			}
			
			doc.Save(path);
		}
	}
}
