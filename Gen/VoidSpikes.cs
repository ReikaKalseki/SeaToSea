using System;
using System.Collections.Generic;
using System.Xml;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class VoidSpikes : WorldGenerator {
			
		private static readonly Vector3[] spacing = new Vector3[]{
			new Vector3(16, 8, 16),
			new Vector3(24, 10, 24),
			new Vector3(32, 16, 32),
			new Vector3(40, 16, 40),
			new Vector3(50, 24, 50),
		};
		
		private readonly List<SpikeCluster> spikes = new List<SpikeCluster>();
		
		private int count;
		private float scaleXZ = 1;
		private float scaleY = 1;
		private bool generateAux = true;
		
		public VoidSpikes(Vector3 pos) : base(pos) {
			count = UnityEngine.Random.Range(5, 11);
			scaleXZ = UnityEngine.Random.Range(1F, 4F);
			scaleY = UnityEngine.Random.Range(0.75F, 2F);
		}
		
		public override void loadFromXML(XmlElement e) {
			count = e.getInt("count", count);
			scaleXZ = (float)e.getFloat("scale", scaleXZ);
			scaleY = (float)e.getFloat("scale", scaleY);
			if (e.hasProperty("generateAux"))
				generateAux = e.getBoolean("generateAux");
		}
		
		public override void saveToXML(XmlElement e) {
			e.addProperty("count", count);
			e.addProperty("scaleXZ", scaleXZ);
			e.addProperty("scaleY", scaleY);
			e.addProperty("generateAux", generateAux);
		}
		
		public override void generate(List<GameObject> generated) {
			for (int i = 0; i < count; i++) {
				Vector3? pos = getSafePosition();
				if (pos != null && pos.HasValue) {
					SpikeCluster s = new SpikeCluster(pos.Value, generateAux);
					spikes.Add(s);
					s.generate(generated);
				}
			}
		}
		
		private Vector3? getSafePosition() {
			if (count == 1)
				return position;
			Vector3 sc = new Vector3(scaleXZ, scaleY, scaleXZ)*2;
			Vector3 ret = MathUtil.getRandomVectorAround(position, Vector3.Scale(spacing[0], sc));
			int tries = 0;
			while (tries <= 50 && isTooClose(ret)) {
				ret = MathUtil.getRandomVectorAround(position, Vector3.Scale(spacing[tries/10], sc));
				tries++;
			}
			return tries >= 50 ? (Vector3?)null : ret;
		}
		
		private bool isTooClose(Vector3 pos) {
			foreach (SpikeCluster s in spikes) {
				Vector3 dist = s.position-pos;
				if (dist.x*dist.x+dist.z*dist.z <= 625) {
					return true;
				}
			}
			return false;
		}
		
		private class SpikeCluster {
		
			internal readonly int terraceSpikeCount;
			internal readonly int auxSpikeCount;
			internal readonly Vector3 position;
			private readonly bool generateAux;
			
			private VoidSpike centralSpike;
			private readonly List<VoidSpike> firstRow = new List<VoidSpike>();
			private readonly List<VoidSpike> auxSpikes = new List<VoidSpike>();
						
			internal SpikeCluster(Vector3 vec, bool aux) {
				terraceSpikeCount = UnityEngine.Random.Range(4, 8);
				auxSpikeCount = UnityEngine.Random.Range(3, 9);
				position = vec;
				generateAux = aux;
			}
			
			internal void generate(List<GameObject> li) {
				centralSpike = new VoidSpike(position);
				centralSpike.setScale(Math.Max(centralSpike.getScale(), 1.8F));
				centralSpike.oreRichness = 0.2;
				centralSpike.plantRate = 2;
				if (UnityEngine.Random.Range(0, 4) > 0) {
					centralSpike.hasFlora = false;
					centralSpike.hasPod = false;
					centralSpike.hasFloater = true;
				}
				else {
					centralSpike.hasFlora = true;
					centralSpike.hasPod = true;
					centralSpike.hasFloater = false;
				}
				centralSpike.generateSpike();
				for (int i = 0; i < terraceSpikeCount; i++) {
					float down = UnityEngine.Random.Range(1F, 3F);
					float radius = UnityEngine.Random.Range(4F, 12F);
					float angle = UnityEngine.Random.Range(0, 2F*(float)Math.PI);
					float cos = (float)Math.Cos(angle);
					float sin = (float)Math.Sin(angle);
					Vector3 pos = new Vector3(position.x+radius*cos, position.y-down, position.z+radius*sin);
					VoidSpike s = new VoidSpike(pos);
					s.hasFloater = false;
					s.hasFlora = true;
					s.plantRate = 1.0;
					if (radius <= 9)
						s.hasPod = false;
					if (s.hasPod) {
						s.podSizeDecr = 1;
						s.podOffset = new Vector3(0.125F*cos, 0, 0.125F*sin);
					}
					s.oreRichness = 0.5;
					s.validPlantPosCheck = (vec, n) => !posIntersectsAnySpikes(vec, n, s);
					s.setScale(Math.Min(s.getScale(), 1.2F));
					firstRow.Add(s);
					s.generateSpike();
				}
				if (generateAux) {
					generateAuxSpikes(centralSpike, 2);
					foreach (VoidSpike s0 in firstRow) {
						generateAuxSpikes(s0, 6);
					}
				}
				
				generateDeco(li);
			}
			
			private void generateDeco(List<GameObject> li) {
				SBUtil.log("Decorating central "+centralSpike);
				generateDeco(li, centralSpike);
				foreach (VoidSpike s in firstRow) {
					SBUtil.log("Decorating terrace "+s);
					generateDeco(li, s);
				}
				foreach (VoidSpike s in auxSpikes) {
					SBUtil.log("Decorating aux "+s);
					generateDeco(li, s);
				}
			}
			
			private void generateDeco(List<GameObject> li, VoidSpike s) {
				s.generateFlora();
				s.generateResources();
				s.collateGenerated(li);
			}
			
			private bool posIntersectsAnySpikes(Vector3 vec, string n, VoidSpike except) {
				SBUtil.log("Checking "+vec+" "+n+" against central "+centralSpike);
				if (centralSpike.intersects(vec))
					return true;
				foreach (VoidSpike s in firstRow) {
					if (s == except)
						continue;
					SBUtil.log("Checking "+vec+" "+n+" against terrace "+s);
					if (s.intersects(vec))
						return true;
				}
				foreach (VoidSpike s in auxSpikes) {
					if (s == except)
						continue;
					SBUtil.log("Checking "+vec+" "+n+" against aux "+s);
					if (s.intersects(vec))
						return true;
				}
				return false;
			}
			
			private void generateAuxSpikes(VoidSpike s0, float down) {
				for (int i = 0; i < auxSpikeCount; i++) {
					Vector3 pos = MathUtil.getRandomVectorAround(s0.position-Vector3.up*(down+1), new Vector3(4, down, 4));
					pos.y = Math.Min(pos.y, s0.position.y-1);
					VoidSpike s = new VoidSpike(pos);
					s.setScale(Math.Min(s.getScale(), 0.875F));
					s.hasFlora = true;
					s.hasFloater = false;
					s.hasPod = false;
					s.validPlantPosCheck = (vec, n) => !posIntersectsAnySpikes(vec, n, s);
					s.oreRichness = s0.oreRichness;
					s.plantRate = 0.67;
					auxSpikes.Add(s);
					s.generateSpike();
				}
			}
			
		}
	}
}
