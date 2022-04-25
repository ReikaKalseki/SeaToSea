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
		
		private static readonly Dictionary<string, Pod> prefabs = new Dictionary<string, Pod>();
		
		private static readonly double measuredYDatum = -25.26;
		
		static GrandReefVoidChunk() { //median 4.81
			addPodType("228e5af5-a579-4c99-9fb0-04b653f73cd3", -19.98-2.5, -26.62); //"WorldEntities/Environment/Coral_reef_floating_stones_small_01"
			addPodType("1645f35d-af23-4b98-b1e4-44d430421721", -20.18, -31.74); //"WorldEntities/Environment/Coral_reef_floating_stones_small_02"
			addPodType("1cafd118-47e6-48c4-bfd7-718df9984685", -20.45, -32.52); //"WorldEntities/Environment/Coral_reef_floating_stones_mid_01"
			addPodType("7444baa0-1416-4cb6-aa9a-162ccd4b98c7", -20.78, -43.45); //"WorldEntities/Environment/Coral_reef_floating_stones_mid_02"
			addPodType("c72724f3-125d-4e87-b82f-a91b5892c936", -20.7, -52.36); //"WorldEntities/Environment/Coral_reef_floating_stones_big_02"
		}
		
		private static void addPodType(string prefab, double ymax, double ymin) {
			prefabs[prefab] = new Pod(prefab, ymax-measuredYDatum, ymin-measuredYDatum);
		}
		
		private float rotation;
		private Vector3 scale = Vector3.one;
		private int podCount;
		
		private GameObject rock;
		
		private List<GameObject> pods = new List<GameObject>();
		
		public GrandReefVoidChunk(Vector3 pos) : base(pos) {
			rotation = UnityEngine.Random.Range(0F, 360F);
			podCount = UnityEngine.Random.Range(2, 7); //2-6
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
			scale = new Vector3(UnityEngine.Random.Range(0.5F, 2.5F), UnityEngine.Random.Range(0.9F, 1.2F), UnityEngine.Random.Range(0.5F, 2.5F));
			
			rock = PlacedObject.createWorldObject(TERRAIN_CHUNK);
			rock.transform.position = position;
			rock.transform.localScale = scale;
			rock.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
			
			for (int i = 0; i < podCount; i++) {
				spawnAnchorPod(true, true);
			}
			
			//TODO ensure no floating pods
			//TODO add underside rocks, mineral
			//TODO add flora like gabe feather and ghostweed
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
			List<Pod> li = new List<Pod>(prefabs.Values);
			int max = allowLargeSize ? prefabs.Count : (allowMediumSize ? prefabs.Count-1 : 2);
			Pod p = li[UnityEngine.Random.Range(0, max)];
			GameObject go = PlacedObject.createWorldObject(p.prefab);
			go.transform.position = pos.Value;
			setPodHeight(p, go);
			pods.Add(go);
		}
		
		private Vector3? getRandomPodPosition() {
			Vector3? rand = null;
			int tries = 0;
			while ((rand == null || !rand.HasValue) && tries < 25) {
				rand = new Vector3(position.x, position.y, position.z); //TODO pick random locations, use polar coords
				foreach (GameObject go in pods) {
					if (rand.Value.DistanceSqrXZ(go.transform.position) < 9) {
						rand = null;
						break;
					}
				}
				tries++;
			}
			return rand;
		}
		
		private void setPodHeight(Pod p, GameObject go) {
			double maxSink = Math.Min(p.maximumSink*0.8, scale.y*TERRAIN_THICK);
			float sink = UnityEngine.Random.Range(0F, (float)maxSink);
			double newY = position.y+p.vineBaseOffset-sink;
			Vector3 pos = go.transform.position;
			pos.y = (float)newY;
			go.transform.position = pos;
		}
		
		private class Pod {
			
			internal readonly string prefab;
			internal readonly double vineBaseOffset; //amount needed to rise to only just embed, always > 0
			internal readonly double maximumSink; //further sinkability from @ vineBaseOffset, always > 0
			
			internal Pod(string pfb, double y, double ym) {
				prefab = pfb;
				vineBaseOffset = y;
				maximumSink = -ym-y;
			}
			
		}
	}
}
