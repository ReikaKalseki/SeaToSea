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
	
	public class UnderwaterIslandsFloorBiome {
		
		public static readonly float minimumDepth = 375;
		public static readonly float biomeRadius = 240;
		public static readonly Vector3 biomeCenter = new Vector3(-107, -481, 953);
		
		public static readonly string biomeName = "Glass Forest";
		public static readonly float waterTemperature = 35;
    
	    public static readonly Vector3 wreckCtrPos1 = new Vector3(-122, -506, 913);
	    public static readonly Vector3 wreckCtrPos2 = new Vector3(-112, -506, 896);
		
		public static readonly UnderwaterIslandsFloorBiome instance = new UnderwaterIslandsFloorBiome();
		
		//private readonly AtmoFX atmoFX = new AtmoFX();
		
		private UnderwaterIslandsFloorBiome() {
			
		}
		
		public void register() {
        	GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.GHOST_LEVIATHAN.prefab, new Vector3(-125, -450, 980)));
			
			//atmoFX.Patch();
			/*
			for (float i = -100; i <= length+100; i += biomeVolumeRadius*0.5F) {
				addAtmoFX(end500m+(end900m-end500m).normalized*i);
			}
			addAtmoFX(end900m);
			*/
			
			GenUtil.registerSlotWorldgen(SeaToSeaMod.kelp.ClassID, SeaToSeaMod.kelp.PrefabFileName, SeaToSeaMod.kelp.TechType, EntitySlot.Type.Tall, LargeWorldEntity.CellLevel.VeryFar, BiomeType.UnderwaterIslands_ValleyFloor, 1, 3.2F);
			//GenUtil.registerSlotWorldgen(kelp.ClassID, kelp.PrefabFileName, kelp.TechType, false, BiomeType.UnderwaterIslands_Geyser, 1, 2F);
		}
		
		private void addAtmoFX(Vector3 pos) {
			//GenUtil.registerWorldgen(atmoFX.ClassID, pos, Quaternion.identity, go => go.transform.localScale = Vector3.one*(100+biomeVolumeRadius));
		}
		
		public void tickPlayer(Player ep) {
			
		}
		
		public void onWorldStart() {
			
		}
		
		public bool isInBiome(string orig, Vector3 pos) {
			if (orig == null || pos.y > -minimumDepth)
				return false;
			if (orig == biomeName)
				return true;
			//bool match = string.Equals(orig, "underwaterislands", StringComparison.InvariantCultureIgnoreCase) || string.Equals(orig, "UnderwaterIslands_ValleyFloor", StringComparison.InvariantCultureIgnoreCase);
			return orig.ToLowerInvariant().Contains("underwaterislands") && getDistanceToBiome(pos) < 5;
		}
		
		public double getDistanceToBiome(Vector3 vec) {
			return Math.Max(0, Vector3.Distance(vec, biomeCenter)-biomeRadius);
		}
		
		public float getTemperatureBoost(float baseline, Vector3 pos) {
			float boost = ((-pos.y)-minimumDepth-75)*0.5F; // so add about 50C
			if (boost <= 0)
				return 0;
			boost /= 1+(float)getDistanceToBiome(pos)*0.01F;
			return Mathf.Min(boost, 200-baseline);
		}
	}
	/*
	class AtmoFX : GenUtil.CustomPrefabImpl {
	       
		internal AtmoFX() : base("glassforestFX", "58b3c65d-1915-497d-b652-f6beba004def") { //blood kelp
			
		}
	
		public override void prepareGameObject(GameObject go, Renderer r) {
			LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
			lw.cellLevel = LargeWorldEntity.CellLevel.Batch;
			AtmosphereVolume vol = go.EnsureComponent<AtmosphereVolume>();
			vol.affectsVisuals = true;
			vol.enabled = true;
			vol.fogMaxDistance = 100;
			vol.fogStartDistance = 20;
			vol.fogColor = new Color(vol.fogColor.r*0.75F, vol.fogColor.g, Mathf.Min(1, vol.fogColor.b*1.25F), vol.fogColor.a);
			vol.overrideBiome = VoidSpikesBiome.biomeName;
		}
	}*/
}
