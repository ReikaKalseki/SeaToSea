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
			new Vector3(6, 4, 6),
			new Vector3(10, 6, 10),
			new Vector3(15, 8, 15),
			new Vector3(20, 10, 20),
			new Vector3(25, 15, 25),
		};
		
		private readonly List<VoidSpike> spikes = new List<VoidSpike>();
		
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
					VoidSpike s = new VoidSpike(pos.Value);
					spikes.Add(s);
					s.generate(generated);
				}
			}
		}
		
		private Vector3? getSafePosition() {
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
			foreach (VoidSpike s in spikes) {
				if (MathUtil.py3d(s.position, pos) <= 4) {
					return true;
				}
			}
			return false;
		}
		
		private static class SpikeCluster {
			
		}
	}
}
