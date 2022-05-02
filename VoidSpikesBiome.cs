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
		
		public static readonly Vector3 end500m = new Vector3(895, -500, -1995);
		public static readonly Vector3 end900m = new Vector3(457, -900, -2261);
		public static readonly double length = Vector3.Distance(end500m, end900m);
		
		public static readonly Vector3 signalLocation = new Vector3(1725, 0, -997-100);
		public static readonly double gap = Vector3.Distance(end500m, signalLocation);
		
		public static readonly VoidSpikesBiome instance = new VoidSpikesBiome();
		
		private readonly VoidSpikes generator;
		private readonly VoidDebris debris;
		
		private VoidSpikesBiome() {		
			generator = new VoidSpikes((end500m+end900m)/2);
	      	generator.count = 72;
	      	generator.scaleXZ = 6;
	      	generator.scaleY = 4;
	      	generator.generateLeviathan = false;
	      	generator.generateAux = true;
	      	generator.fishCount = 1200;
	      	generator.positionValidity = isValidSpikeLocation;
	      	generator.depthCallback = getSpikeDepth;
	      		
			debris = new VoidDebris(signalLocation);
		}
		
		public void register() {
			GenUtil.registerWorldgen(generator);
			GenUtil.registerWorldgen(debris);
		}
		
		private bool isValidSpikeLocation(Vector3 vec) {
			double dist = MathUtil.getDistanceToLine(vec, end500m, end900m); //NOT WORKING
			SBUtil.log("Checking spike validity @ "+vec+" (dist = "+dist+")/200; D500="+Vector3.Distance(end500m, vec)+"; D900="+Vector3.Distance(end900m, vec));
			return dist <= 350;
		}
		
		private double getSpikeDepth(Vector3 vec) {
			double d1 = Vector3.Distance(end500m, vec)/length;
			//double d2 = Vector3.Distance(end900m, vec)/length;
			double interp = MathUtil.linterpolate(d1, 0, length, end500m.y, end900m.y);
			return MathUtil.getRandomPlusMinus((float)interp, 60F);
		}
		
		public static void checkAndAddWaveBob(LargeWorldEntity c) {
			checkAndAddWaveBob(c, false);
		}
		
		public static void checkAndAddWaveBob(LargeWorldEntity c, bool force) {
			if (!force) {
				double dist = Vector3.Distance(c.gameObject.transform.position, signalLocation);
				if (dist > 18)
					return;
				if (c.gameObject.GetComponentInParent<Creature>() != null || c.gameObject.GetComponentInParent<Player>() != null)
					return;
				if (c.gameObject.GetComponentInParent<Vehicle>() != null)
					return;
			}
			WaveBob b = c.gameObject.EnsureComponent<WaveBob>();
			b.rootPosition = c.gameObject.transform.position;
			b.speed = 0.05;
			b.amplitude = 0.2;
		}
	}
}
