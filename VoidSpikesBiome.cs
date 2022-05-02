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
		
		public readonly Vector3 end500m = new Vector3(895, -500, -1995);
		public readonly Vector3 end900m = new Vector3(457, -900, -2261);
		public readonly double length;
		
		public readonly Vector3 signalLocation = new Vector3(1725, 0, -997-100);
		public readonly double gap;
		
		public static readonly VoidSpikesBiome instance = new VoidSpikesBiome();
		
		private readonly VoidSpikes generator;
		private readonly VoidDebris debris;
		
		private VoidSpikesBiome() {
			length = Vector3.Distance(end500m, end900m);
			gap = Vector3.Distance(end500m, signalLocation);
		
			generator = new VoidSpikes((end500m+end900m)/2);
	      	generator.count = 72;
	      	generator.scaleXZ = 15;
	      	generator.scaleY = 8;
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
			double dist = MathUtil.getDistanceToLine(vec, end500m, end900m); NOT WORKING
			return dist <= 200;
		}
		
		private double getSpikeDepth(Vector3 vec) {
			double d1 = Vector3.Distance(end500m, vec)/length;
			//double d2 = Vector3.Distance(end900m, vec)/length;
			double interp = MathUtil.linterpolate(d1, 0, length, end500m.y, end900m.y);
			return MathUtil.getRandomPlusMinus((float)interp, 60F);
		}
	}
}
