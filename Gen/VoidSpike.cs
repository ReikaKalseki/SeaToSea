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
	public sealed class VoidSpike : WorldGenerator {
		
		private static readonly string FLOATER = "37ea521a-6be4-437c-8ed7-6b453d9218a8";
		private static readonly string FLOATER_LIGHT = "923a14c0-a7a2-49bd-a6fd-915d661582ee";
		private static readonly float FLOATER_BASE_SCALE = 0.12F;
		
		private static readonly Spike[] spikes = new Spike[]{
			new Spike("282cdcbc-8670-4f9a-ae1d-9d8a09f9e880"),
			new Spike("f0438971-2761-412c-bc42-df80577de473"),
		};
		
		private static readonly VanillaFlora[] podPrefabs = new VanillaFlora[]{
			VanillaFlora.ANCHOR_POD_SMALL1,
			VanillaFlora.ANCHOR_POD_SMALL2,
			VanillaFlora.ANCHOR_POD_MED1,
			VanillaFlora.ANCHOR_POD_MED2,
			VanillaFlora.ANCHOR_POD_LARGE,
		};
		
		private static readonly WeightedRandom<VanillaFlora> plantPrefabs = new WeightedRandom<VanillaFlora>();
		
		static VoidSpike() {
			plantPrefabs.addEntry(VanillaFlora.GABE_FEATHER, 100);
			plantPrefabs.addEntry(VanillaFlora.GHOSTWEED, 85);
			plantPrefabs.addEntry(VanillaFlora.MEMBRAIN, 25);
			plantPrefabs.addEntry(VanillaFlora.REGRESS, 10);
			plantPrefabs.addEntry(VanillaFlora.BRINE_LILY, 50);
		}
		
		private float scale = 1;
		private Vector3 scaleVec;
		
		private bool hasFloater = false;
		private bool hasPod = false;
		
		private GameObject spike;
		private GameObject pod;
		private GameObject floater;
		private GameObject floaterLight;
		
		private List<GameObject> plants = new List<GameObject>();
		private List<GameObject> resources = new List<GameObject>();
		
		public VoidSpike(Vector3 pos) : base(pos) {
			scale = UnityEngine.Random.Range(0.75F, 2.5F);
			scaleVec = new Vector3(scale, scale, scale);
			if (UnityEngine.Random.Range(0, 4) == 0) {
				hasFloater = true;
			}
			else {
				hasPod = true;
			}
		}
		
		public override void loadFromXML(XmlElement e) {
			scale = (float)e.getFloat("scale", scale);
			scaleVec = new Vector3(scale, scale, scale);
			
			if (e.hasProperty("hasFloater"))
				hasFloater = e.getBoolean("hasFloater");
			if (e.hasProperty("hasPod"))
				hasPod = e.getBoolean("hasPod");
		}
		
		public override void saveToXML(XmlElement e) {
			e.addProperty("scale", scale);
			e.addProperty("hasFloater", hasFloater);
			e.addProperty("hasPod", hasPod);
		}
		
		public override void generate(List<GameObject> generated) {
			Spike s = spikes[UnityEngine.Random.Range(0, spikes.Length)];
			spike = SBUtil.createWorldObject(s.prefab);
			spike.transform.position = position;
			spike.transform.localScale = scaleVec;
			spike.transform.rotation = Quaternion.Euler(180, UnityEngine.Random.Range(0F, 360F), 0);
			generated.Add(spike);
			
			if (hasPod) {
				pod = spawnAnchorPod(scale >= 1.5, scale >= 1, scale <= 1);
				generated.Add(pod);
			}
			if (hasFloater) {
				floater = SBUtil.createWorldObject(FLOATER);
				floater.transform.position = position+Vector3.up*0.7F*scale;
				floater.transform.localScale = scaleVec*FLOATER_BASE_SCALE;
				floater.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0F, 360F), 0);
				floaterLight = SBUtil.createWorldObject(FLOATER_LIGHT);
				floaterLight.transform.position = floater.transform.position+Vector3.up*1*scale;
				generated.Add(floater);
				generated.Add(floaterLight);
			}
			else {
				VoidChunkPlants vc = new VoidChunkPlants(position+Vector3.up*0.5F*scale);
				vc.fuzz *= 2*scale;
				vc.fuzz.y *= 0.25F;
				vc.generate(plants);
			}
			generated.AddRange(plants);
			generated.AddRange(resources);
		}
		
		private GameObject spawnAnchorPod(bool allowLargeSize, bool allowMediumSize, bool allowSmallSize) {
			int min = allowSmallSize ? 0 : (allowMediumSize ? 2 : podPrefabs.Length-1);
			int max = allowLargeSize ? podPrefabs.Length : (allowMediumSize ? podPrefabs.Length-1 : 2);
			VanillaFlora p = podPrefabs[UnityEngine.Random.Range(min, max)];
			GameObject go = SBUtil.createWorldObject(p.getRandomPrefab(true));
			go.transform.position = position+Vector3.up*-0.4F*scale;
			go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
			setPlantHeight(position.y, p, go);
			return go;
		}
		
		private void setPlantHeight(double yRef, VanillaFlora p, GameObject go) {
			double maxSink = Math.Min(p.maximumSink*0.8, scale*3);
			float sink = UnityEngine.Random.Range(0F, (float)maxSink);
			if (p == VanillaFlora.BRINE_LILY)
				sink = (float)(p.maximumSink*0.95);
			double newY = yRef+p.baseOffset-sink;
			Vector3 pos = go.transform.position;
			go.transform.position = pos;
		}
		
		private class Spike {
			
			internal readonly string prefab;
			
			internal Spike(string s) {
				prefab = s;
			}
			
		}
	}
}
