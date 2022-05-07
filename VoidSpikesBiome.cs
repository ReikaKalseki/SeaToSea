using System;
using System.Collections.Generic;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class VoidSpikesBiome {
		
		public static readonly Vector3 end500m = new Vector3(925, -550, -2050);//new Vector3(895, -500, -1995);
		public static readonly Vector3 end900m = new Vector3(400, -950, -2275);//new Vector3(457, -900, -2261);
		public static readonly double length = Vector3.Distance(end500m, end900m);
		
		public static readonly Vector3 signalLocation = new Vector3(1725, 0, -1250);//new Vector3(1725, 0, -997-100);
		public static readonly double gap = Vector3.Distance(end500m, signalLocation);
		
		public static readonly VoidSpikesBiome instance = new VoidSpikesBiome();
		
		private readonly VoidSpikes generator;
		private readonly VoidDebris debris;
		
		private SignalManager.ModSignal signal;
		private PDAManager.PDAPage debrisPDA;
		
		private VoidSpikesBiome() {
			generator = new VoidSpikes((end500m+end900m)/2);
	      	generator.count = 80;
	      	//generator.scaleXZ = 16;
	      	//generator.scaleY = 6;
	      	generator.generateLeviathan = false;
	      	generator.generateAux = true;
	      	generator.fishCount = generator.count*30;
	      	generator.positionValidity = isValidSpikeLocation;
	      	//generator.depthCallback = getSpikeDepth;
	      	generator.spikeLocationProvider = getSpikeLocation;
	      	generator.shouldRerollCounts = false;
	      		
			debris = new VoidDebris(signalLocation+Vector3.down*0.2F);
		}
		
		public void register() {
			//GenUtil.registerWorldgen(generator);
			int seed = SBUtil.getInstallSeed();
			IEnumerable<WorldGenerator> gens = generator.split(seed);
			foreach (WorldGenerator gen in gens) {
				GenUtil.registerWorldgen(gen);
			}
			
			GenUtil.registerWorldgen(debris);
			
			signal = SignalManager.createSignal(SeaToSeaMod.signals.getEntry("voidpod"));
			signal.pdaEntry.addSubcategory("AuroraSurvivors");
			signal.register(TextureManager.getSprite("Textures/Signal"));
			
			XMLLocale.LocaleEntry e = SeaToSeaMod.pdas.getEntry("voidpod");
			debrisPDA = PDAManager.createPage(e);
			debrisPDA.addSubcategory("AuroraSurvivors");
			debrisPDA.setVoiceover(SBUtil.getSound(e.getField<string>("audio")));
			debrisPDA.register();
		}
		
		public PDAManager.PDAPage getPDA() {
			return debrisPDA;
		}
		
		public void onWorldStart() {
			signal.build(debris.spawnPDA(), signalLocation+Vector3.down*0.3F);
		}
		
		public void activateSignal() {
			SBUtil.log("Activating void signal");
			signal.activate();
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
		
		public static void checkAndAddWaveBob(GameObject go, bool force) {
			if (!force) {
				double dist = Vector3.Distance(go.transform.position, signalLocation);
				if (dist > 18)
					return;
				if (go.GetComponentInParent<Creature>() != null || go.GetComponentInParent<Player>() != null)
					return;
				if (go.GetComponentInParent<Vehicle>() != null)
					return;
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
				
		}
		
		public static GameObject spawnEntity(string pfb) {
			GameObject go = SBUtil.createWorldObject(pfb);
			if (go == null)
				return go;
			LargeWorldEntity lw = go.EnsureComponent<LargeWorldEntity>();
			lw.cellLevel = LargeWorldEntity.CellLevel.Global;
			return go;
		}
	}
}
