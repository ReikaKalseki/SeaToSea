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
	
	public class VoidSpikesBiome {
		
		public static readonly Vector3 end500m = new Vector3(925, -550, -2050);//new Vector3(895, -500, -1995);
		public static readonly Vector3 end900m = new Vector3(400, -950, -2275);//new Vector3(457, -900, -2261);
		public static readonly double length = Vector3.Distance(end500m, end900m);
		
		public static readonly Vector3 signalLocation = new Vector3(1725, 0, -1250);//new Vector3(1725, 0, -997-100);
		public static readonly double gap = Vector3.Distance(end500m, signalLocation);
		
		public static readonly int CLUSTER_COUNT = 80;
		
		public static readonly VoidSpikesBiome instance = new VoidSpikesBiome();
		
		private readonly VoidSpikes generator;
		private readonly VoidDebris debris;
		
		private VoidSpikeWreck wreck;
		
		private VoidSpikes.SpikeCluster entryPoint;
		
		private SignalManager.ModSignal signal;
		
		private VoidSpikesBiome() {
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
		
		public void register() {
			//SpikeCache.load();
			
			//GenUtil.registerWorldgen(generator);
			int seed = SBUtil.getInstallSeed();
			IEnumerable<WorldGenerator> gens = generator.split(seed);
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
			signal.addRadioTrigger(e.getField<string>("sound"), 1200);
			signal.register();
			
			debris.init();
			
			//IngameMenuHandler.Main.RegisterOnSaveEvent(SpikeCache.save);
		}
		
		public void onWorldStart() {
			signal.build(debris.spawnPDA(), signalLocation+Vector3.down*0.5F);
		}
		
		public void activateSignal() {
			SBUtil.log("Activating void signal");
			signal.activate(20);
		}
		
		public void fireRadio() {
			signal.fireRadio();
		}
		
		public string getSignalKey() {
			return signal.getRadioStoryKey();
		}
		
		private bool isValidSpikeLocation(Vector3 vec) {
			return isInBiome(vec) && GenUtil.allowableGenBounds.Contains(vec+new Vector3(0, 0, -40)); //safety buffer
		}
		
		public bool isInBiome(Vector3 vec) {
			double dist = MathUtil.getDistanceToLine(vec, end500m, end900m);
			//SBUtil.log("Checking spike validity @ "+vec+" (dist = "+dist+")/200; D500="+Vector3.Distance(end500m, vec)+"; D900="+Vector3.Distance(end900m, vec));
			return dist <= 240;
		}
		
		private double getSpikeDepth(Vector3 vec) {
			double d1 = Vector3.Distance(end500m, vec)/length;
			//double d2 = Vector3.Distance(end900m, vec)/length;
			double interp = MathUtil.linterpolate(d1, 0, length, end500m.y, end900m.y);
			return MathUtil.getRandomPlusMinus((float)interp, 40F);
		}
		
		private Vector3 getSpikeLocation() {
			Vector3 init = MathUtil.interpolate(end500m, end900m, UnityEngine.Random.Range(0F, 1F));
			if (UnityEngine.Random.Range(0, 7) == 0)
				init = end900m;
			return MathUtil.getRandomVectorAround(init, new Vector3(160, 40, 160));
		}
		
		public static void checkAndAddWaveBob(SkyApplier c) {
			checkAndAddWaveBob(c.gameObject, false);
		}
		
		internal static WaveBob checkAndAddWaveBob(GameObject go, bool force) {
			if (!force) {
				double dist = Vector3.Distance(go.transform.position, signalLocation);
				if (dist > 18)
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
			return b;
		}
		
		public static GameObject spawnEntity(string pfb) {
			GameObject go = SBUtil.createWorldObject(pfb);
			if (go == null)
				return go;
			LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
			//lw.cellLevel = LargeWorldEntity.CellLevel.Global;
			return go;
		}
	}
		/*
	static class SpikeCache {
		
		internal static SpikeStatus[] data = new SpikeStatus[VoidSpikesBiome.CLUSTER_COUNT];
			
		static SpikeCache() {
			for (int i = 0; i < data.Length; i++) {
				data[i] = new SpikeStatus();
			}
		}
		
		internal static void save() {
			string path = getSaveFile();
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			for (int i = 0; i < data.Length; i++) {
				XmlElement e = doc.CreateElement("spike");
				data[i].saveToXML(e);
				e.addProperty("index", i);
				doc.DocumentElement.AppendChild(e);
			}
			doc.Save(path);
		}
		
		internal static void load() {
			string path = getSaveFile();
			if (!File.Exists(path))
				return;
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			XmlElement rootnode = doc.DocumentElement;
			foreach (XmlElement e in rootnode.ChildNodes) {
				data[e.getInt("index", -1, false)].loadFromXML(e);
			}
		}
		
		private static string getSaveFile() {
			string folder = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "SeaToSea_Data");
			Directory.CreateDirectory(folder);
			return Path.Combine(folder, "voidspikes.xml");
		}
		
	}
	
	class SpikeStatus {
		
		internal bool generated = false;
		internal Vector3? rootPosition;
		
		internal void saveToXML(XmlElement e) {
			if (rootPosition != null && rootPosition.HasValue)
				e.addProperty("position", rootPosition.Value);
			e.addProperty("generated", generated);
		}
		
		internal void loadFromXML(XmlElement e) {
			generated = e.getBoolean("generated");
			rootPosition = e.getVector("position", true);
		}
		
	}*/
}
