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
		
		private static readonly WeightedRandom<PlacementBox> boxes = new WeightedRandom<PlacementBox>();
		
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
		
		static GrandReefVoidChunk() {
			//addBox();
		}
		
		private static void addBox(double x1, double x2, double z1, double z2) {
			PlacementBox box = new PlacementBox(x1, x2, z1, z2);
			boxes.addEntry(box, box.getArea());
		}
		
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
					GameObject go = PlacedObject.createWorldObject(p.getRandomPrefab(true));
					go.transform.position = pos.Value;
					go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
					setPlantHeight(plantYAvg, p, go);
					plants.Add(go);
				}
			}
			
			//TODO ensure no floating pods
			//TODO add underside rocks, mineral
			
			double rockYAvg = position.y-4.5;
			for (int i = 0; i < 30; i++) {
				Vector3 pos = getRandomRockPosition(rockYAvg);
				GameObject go = PlacedObject.createWorldObject(ROCK_CHUNK);
				go.transform.position = pos;
				go.transform.rotation = UnityEngine.Random.rotationUniform;
			}
		}
		
		private Vector3 getRandomRockPosition(double y) {
			Vector3 vec = boxes.getRandomEntry().pickRandomPosition();
			vec.y = UnityEngine.Random.Range((float)y-1, (float)y+1);
			return vec;
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
			GameObject go = PlacedObject.createWorldObject(p.getRandomPrefab(true));
			go.transform.position = pos.Value;
			go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
			setPlantHeight(position.y, p, go);
			pods.Add(go);
		}
		
		private Vector3? getRandomPodPosition() {
			return getRandomFreePosition(pods, 3);
		}
		
		private Vector3? getRandomPlantPosition() {
			return getRandomFreePosition(plants, 1);
		}
		
		private Vector3? getRandomFreePosition(IEnumerable<GameObject> occupied, double minGap) {
			Vector3? rand = null;
			int tries = 0;
			while ((rand == null || !rand.HasValue) && tries < 25) {
				PlacementBox box = boxes.getRandomEntry();
				rand = box.pickRandomPosition();
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
		
		private void setPlantHeight(double yRef, VanillaFlora p, GameObject go) {
			double maxSink = Math.Min(p.maximumSink*0.8, scale.y*TERRAIN_THICK);
			float sink = UnityEngine.Random.Range(0F, (float)maxSink);
			if (p == VanillaFlora.BRINE_LILY)
				sink = (float)(p.maximumSink*0.95);
			double newY = yRef+p.baseOffset-sink;
			Vector3 pos = go.transform.position;
			pos.y = (float)newY;
			go.transform.position = pos;
		}
		
		private class PlacementBox {
			
			internal readonly Rect bounds;
			
			internal double areaFraction;
			
			internal PlacementBox(double x1, double x2, double z1, double z2) : this(new Rect((float)x1, (float)z1, (float)(x2-x1), (float)(z2-z1))) {
				
			}
			
			internal PlacementBox(Rect r) {
				bounds = r;
			}
			
			internal Vector3 pickRandomPosition() {
				return new Vector3(UnityEngine.Random.Range(bounds.xMin, bounds.xMax), 0, UnityEngine.Random.Range(bounds.yMin, bounds.yMax));
			}
			
			internal double getArea() {
				return bounds.width*(double)bounds.height;
			}
			
		}
	}
}
