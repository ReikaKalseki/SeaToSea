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
		
		private static readonly WeightedRandom<VanillaFlora> plantPrefabs = new WeightedRandom<VanillaFlora>();
		
		static GrandReefVoidChunk() {
			plantPrefabs.addEntry(VanillaFlora.GABE_FEATHER, 100);
			plantPrefabs.addEntry(VanillaFlora.GHOSTWEED, 85);
			plantPrefabs.addEntry(VanillaFlora.MEMBRAIN, 25);
			plantPrefabs.addEntry(VanillaFlora.REGRESS, 10);
			plantPrefabs.addEntry(VanillaFlora.BRINE_LILY, 50);
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
		private List<GameObject> rocks = new List<GameObject>();
		private List<GameObject> resources = new List<GameObject>();
		
		public GrandReefVoidChunk(Vector3 pos) : base(pos) {
			rotation = UnityEngine.Random.Range(0F, 360F)*0;
			scale = new Vector3(UnityEngine.Random.Range(0.75F, 2.5F), /*UnityEngine.Random.Range(0.9F, 1.2F)*/1, UnityEngine.Random.Range(0.75F, 2.5F));
			double sf = scale.x*scale.z;
			podCount = Math.Max(1, (int)(sf*UnityEngine.Random.Range(2, 5))); //2-4 base
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
		
		public override void generate(List<GameObject> generated) {			
			rock = PlacedObject.createWorldObject(TERRAIN_CHUNK);
			rock.transform.position = position;
			rock.transform.localScale = scale;
			rock.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
			
			for (int i = 0; i < podCount; i++) {
				spawnAnchorPod(true, true);
			}
			
			double plantYAvg = position.y+0.2;
			int nplants = (int)(9*scale.x*scale.x*scale.z*scale.z);
			for (int i = 0; i < nplants; i++) {
				VanillaFlora p = plantPrefabs.getRandomEntry();
				Vector3? pos = getRandomPlantPosition();
				if (pos != null && pos.HasValue) {
					Vector3 use = pos.Value;
					while (!SBUtil.objectCollidesPosition(rock, use) && position.y-use.y < 2) {
						use.y -= 0.05F;
					}
					if (position.y-use.y < 1.9) {
						GameObject go = PlacedObject.createWorldObject(p.getRandomPrefab(true));
						go.transform.position = pos.Value;
						go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
						setPlantHeight(plantYAvg, p, go);
						plants.Add(go);
					}
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
				rocks.Add(rock);
			}
			
			int genned = 0;
			for (int i = 0; i < 400 && genned < 5; i++) {
				Vector3 pos = getRandomMountPosition();
				pos.y = position.y-5;
				while (position.y-pos.y < 9 && isColliding(pos, rocks)) {
					pos.y -= 0.1F;
				}
				if (!isColliding(pos, rocks)) {
					pos.y += 0.05F;
					GameObject go = PlacedObject.createWorldObject(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType.ToString());
					go.transform.position = pos;
					go.transform.rotation = UnityEngine.Random.rotationUniform;
					genned++;
				}
			}
			
			//rotateProps(); //about central axis
		}
		
		private void rotateProps() {
			foreach (GameObject go in pods) {
				MathUtil.rotateObjectAround(go, position, rotation);
			}
			foreach (GameObject go in plants) {
				MathUtil.rotateObjectAround(go, position, rotation);
			}
			foreach (GameObject go in rocks) {
				MathUtil.rotateObjectAround(go, position, rotation);
			}
			foreach (GameObject go in resources) {
				MathUtil.rotateObjectAround(go, position, rotation);
			}
		}
		
		private Vector3 getRandomRockPosition(double y) {
			Vector3 vec = getRandomMountPosition();
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
				rand = getRandomMountPosition();
				foreach (GameObject go in occupied) {
					if (rand.Value.DistanceSqrXZ(go.transform.position) < minGap*minGap) {
						rand = null;
						break;
					}
				}
				tries++;
			}
			//SBUtil.log("Found @ "+rand);
			return rand;
		}
		
		private Vector3 getRandomMountPosition() {
			//PlacementBox box = boxes.getRandomEntry();
			//rand = box.pickRandomPosition();
			return MathUtil.findRandomPointInsideEllipse(this.position, 20.3F*scale.z*1.5F, 26.6F*scale.x*1.5F); //Z is the long axis of the terrain prop
		}
		
		private void setPlantHeight(double yRef, VanillaFlora p, GameObject go) {
			double maxSink = Math.Min(p.maximumSink*0.8, scale.y*TERRAIN_THICK);
			float sink = UnityEngine.Random.Range(0F, (float)maxSink);
			if (p == VanillaFlora.BRINE_LILY)
				sink = (float)(p.maximumSink*0.95);
			double newY = yRef+p.baseOffset-sink;
			Vector3 pos = go.transform.position;
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
