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
	public sealed class GrandReefVoidChunk : WorldGenerator {
		
		private static readonly string TERRAIN_CHUNK = "a474e5fa-1552-4cea-abdb-945f85ed4b1a";
		private static readonly string ROCK_CHUNK = "91af2ecb-d63c-44f4-b6ad-395cf2c9ef04";
		private static readonly double TERRAIN_THICK = 9.1; //crash zone rock is 9.1m thick
		
		private static readonly VanillaFlora[] podPrefabs = new VanillaFlora[]{
			VanillaFlora.ANCHOR_POD_SMALL1,
			VanillaFlora.ANCHOR_POD_SMALL2,
			VanillaFlora.ANCHOR_POD_MED1,
			VanillaFlora.ANCHOR_POD_MED2,
			VanillaFlora.ANCHOR_POD_LARGE,
		};
		
		private static readonly VanillaFlora[] plantPrefabs = new VanillaFlora[]{
			VanillaFlora.GABE_FEATHER,
			VanillaFlora.GHOSTWEED,
			VanillaFlora.MEMBRAIN,
			VanillaFlora.REGRESS,	
			VanillaFlora.BRINE_LILY,		
		};
		
		private float rotation;
		private Vector3 scale = Vector3.one;
		private int podCount;
		
		private GameObject rock;
		
		private List<GameObject> pods = new List<GameObject>();
		private List<GameObject> plants = new List<GameObject>();
		
		public GrandReefVoidChunk(Vector3 pos) : base(pos) {
			rotation = UnityEngine.Random.Range(0F, 360F);
			podCount = UnityEngine.Random.Range(2, 7); //2-6
			scale = new Vector3(UnityEngine.Random.Range(0.5F, 2.5F), UnityEngine.Random.Range(0.9F, 1.2F), UnityEngine.Random.Range(0.5F, 2.5F));
		}
		
		public override void loadFromXML(XmlElement e) {
			e.getFloat("rotation", rotation);
			e.getInt("podCount", podCount);
			Vector3? sc = e.getVector("scale", true);
			if (sc != null && sc.HasValue) {
				scale = sc.Value;
			}
		}
		
		public override void saveToXML(XmlElement e) {
			e.addProperty("rotation", rotation);
			e.addProperty("podCount", podCount);
			e.addProperty("scale", scale);
		}
		
		public override void generate() {			
			rock = PlacedObject.createWorldObject(TERRAIN_CHUNK);
			rock.transform.position = position;
			rock.transform.localScale = scale;
			rock.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
			
			for (int i = 0; i < podCount; i++) {
				spawnAnchorPod(true, true);
			}
			
			double plantYAvg = position.y+4.5;
			for (int i = 0; i < 8; i++) {
				VanillaFlora p = plantPrefabs[UnityEngine.Random.Range(0, plantPrefabs.Length)];
				Vector3? pos = getRandomPlantPosition();
				if (pos != null && pos.HasValue) {
					GameObject go = PlacedObject.createWorldObject(p.prefab);
					go.transform.position = pos.Value;
					go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
					setPlantHeight(p, go);
					plants.Add(go);
				}
			}
			
			//TODO ensure no floating pods
			//TODO add underside rocks, mineral
		}
		
		private void spawnAnchorPod(bool allowLargeSize, bool allowMediumSize) {
			Vector3? pos = getRandomPodPosition();
			if (pos == null || !pos.HasValue)
				return;
			double dist = MathUtil.py3d((pos.Value.x-position.x)/scale.x, 0, (pos.Value.z-position.z)/scale.z);
			if (dist > 4) {
				allowLargeSize = false;
			}
			if (dist > 7) {
				allowMediumSize = false;
			}
			int max = allowLargeSize ? podPrefabs.Length : (allowMediumSize ? podPrefabs.Length-1 : 2);
			VanillaFlora p = podPrefabs[UnityEngine.Random.Range(0, max)];
			GameObject go = PlacedObject.createWorldObject(p.prefab);
			go.transform.position = pos.Value;
			go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
			setPlantHeight(p, go);
			pods.Add(go);
		}
		
		private Vector3? getRandomPodPosition() {
			return getRandomFreePosition(pods, 3);
		}
		
		private Vector3? getRandomPlantPosition() {
			return getRandomFreePosition(plants, 1.5);
		}
		
		private Vector3? getRandomFreePosition(IEnumerable<GameObject> occupied, double minGap) {
			Vector3? rand = null;
			int tries = 0;
			while ((rand == null || !rand.HasValue) && tries < 25) {
				rand = new Vector3(position.x, position.y, position.z); //TODO pick random locations, use polar coords, ellipse maybe
				foreach (GameObject go in occupied) {
					if (rand.Value.DistanceSqrXZ(go.transform.position) < minGap*minGap) {
						rand = null;
						break;
					}
				}
				tries++;
			}
			return rand;
		}
		
		private void setPlantHeight(VanillaFlora p, GameObject go) {
			double maxSink = Math.Min(p.maximumSink*0.8, scale.y*TERRAIN_THICK);
			float sink = UnityEngine.Random.Range(0F, (float)maxSink);
			double newY = position.y+p.baseOffset-sink;
			Vector3 pos = go.transform.position;
			pos.y = (float)newY;
			go.transform.position = pos;
		}
		
		private class PlacementBox {
			
		}
	}
}
