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
			is both not dense enough and is eating all the slots throughout the biome	
			GenUtil.registerSlotWorldgen(SeaToSeaMod.crashSanctuarySpawner.ClassID, SeaToSeaMod.crashSanctuarySpawner.PrefabFileName, SeaToSeaMod.crashSanctuarySpawner.TechType, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, BiomeType.CrashZone_Rock, 1, 6F);
			GenUtil.registerSlotWorldgen(SeaToSeaMod.crashSanctuarySpawner.ClassID, SeaToSeaMod.crashSanctuarySpawner.PrefabFileName, SeaToSeaMod.crashSanctuarySpawner.TechType, EntitySlot.Type.Small, LargeWorldEntity.CellLevel.Medium, BiomeType.CrashZone_Sand, 1, 6F);
			*/
			GenUtil.registerWorldgen(new PositionedPrefab(SeaToSeaMod.crashSanctuarySpawner.ClassID, biomeCenter));
		}
		
		public override VanillaMusic[] getMusicOptions() {
			return new VanillaMusic[]{VanillaMusic.COVE};
		}
		
		public override Vector3 getFogColor(Vector3 orig) {
			return waterColor.toVector();
		}
		
		public override float getSunIntensity(float orig) {
			return orig;
		}
		
		public override float getFogDensity(float orig) {
			return orig*1.5F;
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
	}
}
