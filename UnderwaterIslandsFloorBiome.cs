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
	
	public class UnderwaterIslandsFloorBiome : CustomBiome {
		
		public static readonly float minimumDepth = 375;
		public static readonly float biomeRadius = 240;
		public static readonly Vector3 biomeCenter = new Vector3(-107, -481, 953);
		
		public static readonly string biomeName = "Glass Forest";
		public static readonly float waterTemperature = 35;
    
	    public static readonly Vector3 wreckCtrPos1 = new Vector3(-110.76F, -499F, 940.19F);
	    public static readonly Vector3 wreckCtrPos2 = new Vector3(-138.38F, -497F, 932.69F);
	    
	   // public static Color? waterColor;
	    public static readonly Vector3 waterColorFalloff = new Vector3(4, 10F, 2.0F);//new Vector3(5, 12.3F, 2.5F);
	    public static readonly float murkiness = 1.75F;//1.4F;
	    public static readonly float fogStart = 0;
		
		public static readonly UnderwaterIslandsFloorBiome instance = new UnderwaterIslandsFloorBiome();
		
		private readonly GlassForestAtmoFX atmoFX = new GlassForestAtmoFX();
		
		private UnderwaterIslandsFloorBiome() : base(biomeName, 0.8F) {
			
		}
		
		public override void register() {
        	GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.GHOST_LEVIATHAN.prefab, new Vector3(-125, -450, 980)));
			
			atmoFX.Patch();
			/*
			for (float i = -100; i <= length+100; i += biomeVolumeRadius*0.5F) {
				addAtmoFX(end500m+(end900m-end500m).normalized*i);
			}
			addAtmoFX(end900m);
			*/
			
			//GenUtil.registerWorldgen(atmoFX.ClassID, biomeCenter, Quaternion.identity, go => go.transform.localScale = Vector3.one*(biomeRadius+50));
			
			GenUtil.registerSlotWorldgen(C2CItems.kelp.ClassID, C2CItems.kelp.PrefabFileName, C2CItems.kelp.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Batch, BiomeType.UnderwaterIslands_ValleyFloor, 1, 3.2F);
			//GenUtil.registerSlotWorldgen(kelp.ClassID, kelp.PrefabFileName, kelp.TechType, false, BiomeType.UnderwaterIslands_Geyser, 1, 2F);
		}
		/*
		private void addAtmoFX(Vector3 pos) {
			GenUtil.registerWorldgen(atmoFX.ClassID, pos, Quaternion.identity, go => go.transform.localScale = Vector3.one*(100+biomeVolumeRadius));
		}*/
		
		public override VanillaMusic[] getMusicOptions() {
			return new VanillaMusic[]{VanillaMusic.ILZ, VanillaMusic.JELLYSHROOM, VanillaMusic.AURORA};
		}
		
		public override float getSunScale(float orig) {
			return 0;
		}
		
		public override float getFogStart(float orig) {
			return fogStart;
		}
		
		public override float getMurkiness(float orig) {
			return murkiness;
		}
		
		public override Vector3 getColorFalloff(Vector3 orig) {
			return waterColorFalloff;
		}
		
		public override Color getWaterColor(Color orig) {
			return Color.white;//waterColor.Value;
		}
		
		public override bool isCaveBiome() {
			return false;
		}
		
		public void tickPlayer(Player ep) {
			
		}
		
		public void onWorldStart() {
			
		}
		
		public override bool isInBiome(Vector3 pos) {
			string orig = WaterBiomeManager.main.GetBiome(pos, false);
			return isInBiome(orig, pos);
		}
		
		public bool isInBiome(string orig, Vector3 pos) {
			if (orig == biomeName)
				return true;
			if (orig == null || pos.y > -minimumDepth)
				return false;
			return VanillaBiomes.UNDERISLANDS.containsID(orig) && MathUtil.isPointInCylinder(biomeCenter.setY(-400), pos, biomeRadius, 150);//getDistanceToBiome(pos) < 5;
		}
		
		public override double getDistanceToBiome(Vector3 vec) {
			float ret = Math.Max(0, Vector3.Distance(vec, biomeCenter)-biomeRadius);
			if (vec.y >= -minimumDepth)
				ret = Math.Max(ret, vec.y+minimumDepth);
			return ret;
		}
		
		public float getTemperatureBoost(float baseline, Vector3 pos) {
			float boost = ((-pos.y)-minimumDepth-75)*0.5F; // so add about 50C
			if (boost <= 0)
				return 0;
			boost /= 1+(float)getDistanceToBiome(pos)*0.01F;
			float ret = Mathf.Min(boost, 200-baseline);
			float dist = (float)MathUtil.getDistanceToLineSegment(pos, wreckCtrPos1, wreckCtrPos2);
			if (dist <= 40) {
				ret *= Mathf.Clamp01(dist/30F);
			}
			return ret;
		}
		
		public bool isAtmoFX(PrefabIdentifier pi) {
			return pi && pi.ClassId == atmoFX.ClassID;
		}
	}
	
	class GlassForestAtmoFX : GenUtil.CustomPrefabImpl {
	       
		internal GlassForestAtmoFX() : base("glassforestFX", "62a47c16-bf83-46ee-b16b-8cc35e7df97d") { //valley floor
			
		}
	
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
			lw.cellLevel = LargeWorldEntity.CellLevel.VeryFar;
			AtmosphereVolume vol = go.EnsureComponent<AtmosphereVolume>();/*
			vol.affectsVisuals = true;
			vol.priority = 1;
			vol.enabled = true;
			vol.fogMaxDistance = 150;
			vol.fogStartDistance = 90;
			vol.fogColor = new Color(0.44F, 0.188F, 1, vol.fogColor.a);
			/*
			vol.fog.color = vol.fogColor;*//*
			vol.fog.maxDistance = 600F;
			vol.fog.startDistance = 150F;
			GradientColorKey[] keys = vol.fog.dayNightColor.colorKeys;
			for (int i = 0; i < keys.Length; i++)
				keys[i].color = vol.fogColor;
			vol.fog.dayNightColor.colorKeys = keys;
			//vol.fog.dayNightColor.
			*/
			vol.overrideBiome = UnderwaterIslandsFloorBiome.biomeName;
		}
	}
}
