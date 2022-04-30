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
		
		public VoidSpikes(Vector3 pos) : base(pos) {
			count = UnityEngine.Random.Range(5, 11);
			scaleXZ = UnityEngine.Random.Range(1F, 4F);
			scaleY = UnityEngine.Random.Range(0.75F, 2F);
		}
		
		public override void loadFromXML(XmlElement e) {
			count = e.getInt("count", count);
			scaleXZ = (float)e.getFloat("scale", scaleXZ);
			scaleY = (float)e.getFloat("scale", scaleY);
		}
		
		public override void saveToXML(XmlElement e) {
			e.addProperty("count", count);
			e.addProperty("scaleXZ", scaleXZ);
			e.addProperty("scaleY", scaleY);
		}
		
		public override void generate(List<GameObject> generated) {
			for (int i = 0; i < count; i++) {
				Vector3? pos = getSafePosition();
				if (pos != null && pos.HasValue) {
					SpikeCluster s = new SpikeCluster(pos.Value);
					spikes.Add(s);
					s.generate(generated);
				}
			}
		}
		
		private Vector3? getSafePosition() {
			if (count == 1)
				return position;
			Vector3 sc = new Vector3(scaleXZ, scaleY, scaleXZ);
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
				if (dist.x*dist.x+dist.z*dist.z <= 256) {
					return true;
				}
			}
			return false;
		}
		
		private class SpikeCluster {
		
			internal readonly int spikeCount;
			internal readonly Vector3 position;
			private readonly List<VoidSpike> spikes = new List<VoidSpike>();
			
			private float topY = -10000;
			
			internal SpikeCluster(Vector3 vec) {
				spikeCount = UnityEngine.Random.Range(2, 11);
				position = vec;
			}
			
			internal void generate(List<GameObject> li) {
				for (int i = 0; i < spikeCount; i++) {
					Vector3 pos = MathUtil.getRandomVectorAround(position, new Vector3(4, 9, 4));
					VoidSpike s = new VoidSpike(pos);
					spikes.Add(s);
					s.generate(li);
					topY = Math.Max(topY, pos.y);
				}
				foreach (VoidSpike s in spikes) {
					if (s.position.y < topY-4) {
						s.hasFlora = false;
						s.hasFloater = false;
						s.hasPod = false;
					}
					s.generate(li);
				}
			}
			
		}
	}
}
