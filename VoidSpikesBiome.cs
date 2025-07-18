﻿using System;
using System.Collections.Generic;
using System.Linq;
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
	
	public class VoidSpikesBiome : CustomBiome { //FIXME: 3. disappearing spikes 4. custom levi
		
		public static readonly Vector3 end500m = new Vector3(360, -550, 320);//new Vector3(925, -550, -2050);//new Vector3(895, -500, -1995);
		public static readonly Vector3 end900m = new Vector3(800, -950, -120);//new Vector3(400, -950, -2275);//new Vector3(457, -900, -2261);
		public static readonly double length = Vector3.Distance(end500m, end900m);
		
		public static readonly Vector3 signalLocation = new Vector3(1725, 0, -1250);//new Vector3(1725, 0, -997-100);
		//public static readonly double gap = Vector3.Distance(end500m, signalLocation);
		
		private static readonly Vector3 travel = new Vector3(1, 0, -1).setLength(1500);
		public static readonly Vector3 voidEndpoint500m = (signalLocation+travel).setY(end500m.y); //2785, -550, -2310
		public static readonly Vector3 voidEndpoint900m = voidEndpoint500m+(end900m-end500m); //3225, -950, -2750
		
		public static readonly float biomeVolumeRadius = 200;
		
		public static readonly float waterTemperature = 18;
	    
	    //public static Color? waterColor;
	    public static readonly Vector3 waterColorFalloff = new Vector3(8, 4, 1);
	    public static readonly float murkiness = 4.0F;
	    public static readonly float fogStart = 20;
		
		public static readonly int CLUSTER_COUNT = 120;//104;//88;
		
		public static readonly string PDA_KEY = "voidpod";
		
		public static readonly VoidSpikesBiome instance = new VoidSpikesBiome();
		
		private readonly AtmoFX atmoFX = new AtmoFX();
		
		private readonly VoidSpikes generator;
		private readonly VoidDebris debris;
		
		private VoidSpikeWreck wreck;
		
		private VoidSpikes.SpikeCluster entryPoint;
		
		private SignalManager.ModSignal signal;
			
		private bool delayTeleportOutUntilEnterSeamoth = false;
		private bool delayTeleportInUntilEnterSeamoth = false;
		
		private VoidSpikesBiome() : base("Void Spikes", 0.4F) {
			generator = new VoidSpikes((end500m+end900m)/2);
	      	generator.count = CLUSTER_COUNT;
	      	//generator.scaleXZ = 16;
	      	//generator.scaleY = 6;
	      	generator.generateLeviathan = false;
	      	generator.generateAux = true;
	      	generator.fishCount = generator.count*20;
	      	generator.positionValidity = isValidSpikeLocation;
	      	//generator.depthCallback = getSpikeDepth;
	      	generator.spikeLocationProvider = getSpikeLocation;
	      	generator.shouldRerollCounts = false;
	      		
			debris = new VoidDebris(signalLocation+Vector3.down*0.2F);
		}
		
		public override VanillaMusic[] getMusicOptions() {
			return new VanillaMusic[]{VanillaMusic.DUNES, VanillaMusic.BKELP, VanillaMusic.KOOSH, VanillaMusic.DEEPGRAND};
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
			return orig;
		}
		
		public override void register() {
			//SpikeCache.load();
			
			//GenUtil.registerWorldgen(generator);
			int seed = SNUtil.getInstallSeed();
			IEnumerable<VoidSpikes.SpikeCluster> gens = generator.split(seed);
			foreach (VoidSpikes.SpikeCluster gen in gens) {
				GenUtil.registerWorldgen(gen);
				if (entryPoint == null || Vector3.Distance(gen.position, end500m) < Vector3.Distance(entryPoint.position, end500m)) {
					entryPoint = gen;
				}
			}
	      		
			wreck = new VoidSpikeWreck(entryPoint.getRootLocation()+Vector3.up*0.1F);
			entryPoint.needsCenterSpace = true;
			entryPoint.additionalGen = wreck.generate;
			
			GenUtil.registerWorldgen(debris);
			
			XMLLocale.LocaleEntry e = SeaToSeaMod.signalLocale.getEntry("voidpod");
			signal = SignalManager.createSignal(e);
			//signal.pdaEntry.addSubcategory("AuroraSurvivors");
			signal.addRadioTrigger(e.getField<string>("sound"));
			signal.register("32e48451-8e81-428e-9011-baca82e9cd32", signalLocation);
			signal.addWorldgen(UnityEngine.Random.rotationUniform);
			
			createDiscoveryStoryGoal(10, SeaToSeaMod.miscLocale.getEntry("voidspikeenter"));
			
			atmoFX.Patch();
			
			GenUtil.registerWorldgen(PDAManager.getPage(PDA_KEY).getPDAClassID(), signalLocation+Vector3.down*1.25F, UnityEngine.Random.rotationUniform);
			//GenUtil.registerWorldgen(leviathan.ClassID, end900m, Vector3.zero);
			
			for (float i = -100; i <= length+100; i += biomeVolumeRadius*0.5F) {
				addAtmoFX(end500m+(end900m-end500m).normalized*i);
			}
			addAtmoFX(end900m);
			
			debris.init();
			
			//IngameMenuHandler.Main.RegisterOnSaveEvent(SpikeCache.save);
		}
		
		public override mset.Sky getSky() {
			return WorldUtil.getSkybox(VanillaBiomes.VOID.getIDs().First());
		}
		
		public override bool isCaveBiome() {
			return false;
		}
		
		public override bool existsInSeveralPlaces() {
			return false;
		}
		
		private void addAtmoFX(Vector3 pos) {
			GenUtil.registerWorldgen(atmoFX.ClassID, pos, Quaternion.identity, go => go.transform.localScale = Vector3.one*(100+biomeVolumeRadius));
		}
		
		public void tickPlayer(Player ep) {
			Vector3 pos = ep.transform.position;
			//SNUtil.writeToChat("Dist @ "+pos+" = "+dist);
			/*
			if (dist < biomeVolumeRadius+75) {
				while (AtmosphereDirector.main.priorityQueue.Count > 0)
					AtmosphereDirector.main.PopSettings(AtmosphereDirector.main.priorityQueue[AtmosphereDirector.main.priorityQueue.Count-1]);
				AtmosphereDirector.main.PushSettings(AtmosphereDirector.main.defaultSettings);
			}*/
			VoidSpikeLeviathanSystem.instance.tick(ep);
	    	if (ep.currentSub == null && UnityEngine.Random.Range(0, (int)(10/Time.timeScale)) == 0) {
			   	if (Vector3.Distance(pos, end500m) <= biomeVolumeRadius/2) {
					//moved to biome storygoals if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.PROMPTS))
					//	PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.VoidSpike).key);
			   	}
				else {
					tickTeleportCheck(ep);
				}
			}
		}
		
		internal void tickTeleportCheck(MonoBehaviour go) {
			Vector3 pos = go.transform.position;
			double dist = getDistanceToBiome(pos, false);
			float f1 = biomeVolumeRadius+25;
			if (dist >= f1 && dist <= f1+20) {
				Vector3 tgt = (voidEndpoint500m+(pos-end500m).addLength(30)).setY(pos.y);
				if (go is Player && !delayTeleportOutUntilEnterSeamoth) {
					SNUtil.teleportPlayer((Player)go, tgt);
				  	SNUtil.log("Teleported player back from biome: "+tgt);
				}
				else if (go is C2CMoth) {
					delayTeleportOutUntilEnterSeamoth = true;
				}
			}
			else {
				dist = MathUtil.getDistanceToLineSegment(pos, voidEndpoint500m, voidEndpoint900m);
				if (dist <= f1) {
					Vector3 tgt = (end500m+(pos-voidEndpoint500m).addLength(-30)).setY(pos.y);
					if (go is Player && !delayTeleportInUntilEnterSeamoth) {
						SNUtil.teleportPlayer((Player)go, tgt);
				   		SNUtil.log("Teleported player to biome: "+tgt);
						foreach (GameObject levi in VoidGhostLeviathansSpawner.main.spawnedCreatures) {
							Vector3 delta = levi.transform.position-tgt;
							levi.transform.position = tgt+delta;
						}
					}
					else if (go is C2CMoth) {
						delayTeleportInUntilEnterSeamoth = true;
					}
				}
			}
		}
		
		public void onSeamothEntered(SeaMoth sm, Player ep) {
			Vector3 pos = sm.transform.position;
			if (delayTeleportInUntilEnterSeamoth) {
				Vector3 tgt = (voidEndpoint500m+(pos-end500m).addLength(30)).setY(pos.y);
				//SNUtil.writeToChat("Delayed tele in");
				ep.transform.position = tgt;
				sm.transform.position = tgt;
				SNUtil.log("Teleported player to biome: "+tgt);
				delayTeleportInUntilEnterSeamoth = false;
			}
			else if (delayTeleportOutUntilEnterSeamoth) {
				//SNUtil.writeToChat("Delayed tele out");
				Vector3 tgt = (end500m+(pos-voidEndpoint500m).addLength(-30)).setY(pos.y);
				ep.transform.position = tgt;
				sm.transform.position = tgt;
				SNUtil.log("Teleported player back from biome: "+tgt);
				delayTeleportOutUntilEnterSeamoth = false;
			}
		}
		
		public void onWorldStart() {
			//AtmosphereDirector.main.debug = true;
			if (isInBiome(Player.main.transform.position) && !anySeamothsInBiome()) {
				delayTeleportOutUntilEnterSeamoth = true;
			}
		}
		
		internal bool anySeamothsInBiome() {
			foreach (SeaMoth sm in UnityEngine.Object.FindObjectsOfType<SeaMoth>()) {
				if (Vector3.Distance(sm.transform.position, end500m) <= biomeVolumeRadius+40)
					return true;
			}
			return false;
		}
		
		public void activateSignal() {
			SNUtil.log("Activating void signal");
			signal.activate(18);
		}
		
		public void fireRadio() {
			signal.fireRadio();
		}
		
		public bool isRadioFired() {
			return signal.isRadioFired();
		}
		
		public string getSignalKey() {
			return signal.storyGate;
		}
		
		private bool isValidSpikeLocation(Vector3 vec) {
			return isInBiome(vec) && GenUtil.allowableGenBounds.Contains(vec+new Vector3(0, 0, -40)); //safety buffer
		}
		
		public override bool isInBiome(Vector3 vec) {
			//SNUtil.log("Checking spike validity @ "+vec+" (dist = "+dist+")/200; D500="+Vector3.Distance(end500m, vec)+"; D900="+Vector3.Distance(end900m, vec));
			return vec.y < -320 && getDistanceToBiome(vec, false) <= biomeVolumeRadius+150;
		}
		
		public override double getDistanceToBiome(Vector3 vec) {
			return getDistanceToBiome(vec, true);
		}
		
		public double getDistanceToBiome(Vector3 vec, bool includeVoidSide) {
			double ret = MathUtil.getDistanceToLineSegment(vec, end500m, end900m);// endcap dist either < L
			if (includeVoidSide)
				ret = Math.Min(ret, MathUtil.getDistanceToLineSegment(vec, voidEndpoint500m, voidEndpoint900m));
			return ret;
		}
		
		private double getSpikeDepth(Vector3 vec) {
			double d1 = Vector3.Distance(end500m, vec)/length;
			//double d2 = Vector3.Distance(end900m, vec)/length;
			double interp = MathUtil.linterpolate(d1, 0, length, end500m.y, end900m.y);
			return MathUtil.getRandomPlusMinus((float)interp, 40F);
		}
		
		private Vector3 getSpikeLocation() {
			Vector3 init = MathUtil.interpolate(end500m, end900m, UnityEngine.Random.Range(0F, 1F));
			if (UnityEngine.Random.Range(0, 9) == 0)
				init = end900m;
			return MathUtil.getRandomVectorAround(init, new Vector3(160, 40, 160));
		}
		
		public bool isPlayerInLeviathanZone(Vector3 pos) {
			return isInBiome(pos) && Vector3.Distance(pos, end900m) <= biomeVolumeRadius*1.5F;
		}
		
		public Vector3 getPDALocation() {
			return wreck.getPDALocation();
		}
		
		public static void checkAndAddWaveBob(SkyApplier c) {
			checkAndAddWaveBob(c.gameObject, false);
		}
		
		internal static WaveBob checkAndAddWaveBob(GameObject go, bool force) {
	    	if (C2CHooks.skipWaveBob)
	    		return null;
			if (!force) {
				double distsq = (go.transform.position-signalLocation).sqrMagnitude;
				if (distsq > 18*18)
					return null;
				if (go.GetComponentInParent<Creature>() != null || go.GetComponentInParent<Player>() != null)
					return null;
				if (go.GetComponentInParent<Vehicle>() != null)
					return null;
			}
			WaveBob b = go.EnsureComponent<WaveBob>();
			b.rootPosition = go.transform.position;
			b.speed = UnityEngine.Random.Range(1.75F, 2.5F);
			b.amplitude = UnityEngine.Random.Range(0.15F, 0.3F);
			b.speed2Ratio = UnityEngine.Random.Range(2.5F, 3F);
			b.amplitude2Ratio = UnityEngine.Random.Range(0.05F, 0.1F);
			if (b.rootPosition.y <= -1.25) {
				b.amplitude *= 0.25F;
				b.speed *= 0.5F;
			}
			if (go.GetComponentInChildren<BlueprintHandTarget>() != null) {
				b.amplitude *= 0.5F;
				b.speed *= 0.4F;
			}
			
			LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
			if (lw.cellLevel != LargeWorldEntity.CellLevel.Global)
				lw.cellLevel = LargeWorldEntity.CellLevel.Batch;
			return b;
		}
		
		public static GameObject spawnEntity(string pfb) {
			GameObject go = ObjectUtil.createWorldObject(pfb);
			if (go == null)
				return go;
			//DestroyDetector dd = go.EnsureComponent<DestroyDetector>();
			LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
			//lw.cellLevel = LargeWorldEntity.CellLevel.Global;
			return go;
		}
	}
	
	class AtmoFX : GenUtil.CustomPrefabImpl {
	       
		internal AtmoFX() : base("voidspikeFX", "58b3c65d-1915-497d-b652-f6beba004def") { //blood kelp
			
		}
	
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
			lw.cellLevel = LargeWorldEntity.CellLevel.Batch;
			AtmosphereVolume vol = go.EnsureComponent<AtmosphereVolume>();
			vol.affectsVisuals = true;
			vol.enabled = true;
			vol.fogMaxDistance = 100;
			vol.fogStartDistance = 20;
			vol.fogColor = new Color(vol.fogColor.r*0.75F, vol.fogColor.g, Mathf.Min(1, vol.fogColor.b*1.25F), vol.fogColor.a);
			vol.overrideBiome = VoidSpikesBiome.instance.biomeName;
		}
	}
}
