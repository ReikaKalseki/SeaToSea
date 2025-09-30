using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public sealed class VoidSpikes : WorldGenerator {

		private static readonly Vector3[] spacing = new Vector3[]{
			new Vector3(16, 8, 16),
			new Vector3(24, 10, 24),
			new Vector3(32, 16, 32),
			new Vector3(40, 16, 40),
			new Vector3(50, 24, 50),
		};

		private static readonly WeightedRandom<string> fishTypes = new WeightedRandom<string>();

		private readonly List<SpikeCluster> spikes = new List<SpikeCluster>();

		public int count;
		public float scaleXZ = 1;
		public float scaleY = 1;
		public bool generateAux = true;
		public bool generateLeviathan = true;
		public int fishCount;
		public bool shouldRerollCounts = false;

		public Vector3 offset = Vector3.zero;

		public Func<Vector3, bool> positionValidity = null;
		public Func<Vector3, double> depthCallback = null;
		public Func<Vector3> spikeLocationProvider = null;

		static VoidSpikes() {
			fishTypes.addEntry(VanillaCreatures.REGINALD.prefab, 75);
			fishTypes.addEntry(VanillaCreatures.BLADDERFISH.prefab, 25);
			fishTypes.addEntry(VanillaCreatures.EYEYE.prefab, 40);
			fishTypes.addEntry(VanillaCreatures.SHUTTLEBUG.prefab, 50);
			fishTypes.addEntry(VanillaCreatures.SPINEFISH.prefab, 100);
			fishTypes.addEntry(VanillaCreatures.SPADEFISH.prefab, 50);

			fishTypes.addEntry(VanillaCreatures.BLIGHTER.prefab, 40);
			fishTypes.addEntry(VanillaCreatures.BLEEDER.prefab, 40);
			fishTypes.addEntry(VanillaCreatures.MESMER.prefab, 20);
		}

		internal static void addFish(string id, float weight) {
			fishTypes.addEntry(id, weight);
		}

		public VoidSpikes(Vector3 pos) : base(pos) {
			this.rerollCounts();

			spawner = VoidSpikesBiome.spawnEntity;
		}

		public override void loadFromXML(XmlElement e) {
			count = e.getInt("count", count);
			scaleXZ = (float)e.getFloat("scale", scaleXZ);
			scaleY = (float)e.getFloat("scale", scaleY);
			if (e.hasProperty("generateAux"))
				generateAux = e.getBoolean("generateAux");
			generateLeviathan = e.getBoolean("generateLeviathan");
			fishCount = e.getInt("fishCount", fishCount);
		}

		public override void saveToXML(XmlElement e) {
			e.addProperty("count", count);
			e.addProperty("scaleXZ", scaleXZ);
			e.addProperty("scaleY", scaleY);
			e.addProperty("generateAux", generateAux);
			e.addProperty("generateLeviathan", generateLeviathan);
		}

		private void rerollCounts() {
			count = UnityEngine.Random.Range(5, 11);
			scaleXZ = UnityEngine.Random.Range(1F, 4F);
			scaleY = UnityEngine.Random.Range(0.75F, 2F);
			fishCount = UnityEngine.Random.Range(40, 61);
		}

		public IEnumerable<SpikeCluster> split(int seed) {
			SNUtil.log("Initializing spike clusters [" + count + "] with seed " + seed);
			UnityEngine.Random.InitState(seed);
			this.calculateSpikeClusters();
			foreach (SpikeCluster s in spikes) {
				s.fishCount = fishCount / spikes.Count;
			}
			SNUtil.log("Initialized " + spikes.Count + " spikes");
			fishCount = 0;
			return new ReadOnlyCollection<SpikeCluster>(spikes);
		}

		private void calculateSpikeClusters() {
			for (int i = 0; i < count; i++) {
				Vector3? pos = this.getSafePosition();
				if (pos != null && pos.HasValue) {
					Vector3 vec = pos.Value;
					//SNUtil.log("Success, spike @ "+vec);
					if (depthCallback != null)
						vec.y = (float)depthCallback(vec);
					float f = ((float)MathUtil.getDistanceToLineSegment(vec, VoidSpikesBiome.end500m, VoidSpikesBiome.end900m))/VoidSpikesBiome.biomeVolumeRadius;
					//SNUtil.log("Re-y spike @ "+vec);
					SpikeCluster s = new SpikeCluster(vec, generateAux, 1-f);
					s.spawner = spawner;
					spikes.Add(s);
				}
			}
		}

		public override bool generate(List<GameObject> generated) {/*
			if (!Player.main)
				return false;
			Vector3 pos = Player.main.transform.position;
			if (pos.y > -300)
				return false;
			BiomeBase bb = BiomeBase.getBiome(pos);
			if (bb != VanillaBiomes.VOID && bb != VoidSpikesBiome.instance)
				return false;*/
			UnityEngine.Random.InitState(SNUtil.getWorldSeedInt());
			if (shouldRerollCounts)
				this.rerollCounts();
			if (spikes.Count == 0) {
				this.calculateSpikeClusters();
			}
			foreach (SpikeCluster s in spikes) {
				s.generate(generated);
			}
			for (int i = 0; i < fishCount; i++) {
				Vector3 vec = MathUtil.getRandomVectorAround(position+offset, Vector3.Scale(spacing[spacing.Length-1], new Vector3(scaleXZ, scaleY, scaleXZ)));
				if (this.posIntersectsAnySpikes(vec, "fish") || (positionValidity != null && !positionValidity(vec))) {
					i--;
					continue;
				}
				GameObject fish = spawner.Invoke(fishTypes.getRandomEntry());
				//SNUtil.log("Spawning fish "+fish+" @ "+vec);
				fish.transform.position = vec;
				generated.Add(fish);
			}
			if (generateLeviathan) {
				GameObject levi = spawner(VanillaCreatures.GHOST_LEVIATHAN_BABY.prefab);
				levi.transform.position = position + offset;
				generated.Add(levi);
			}
			for (int i = 0; i < 4; i++) {
				GameObject ent = spawner(VanillaCreatures.CRABSQUID.prefab);
				ent.transform.position = MathUtil.interpolate(VoidSpikesBiome.end500m, VoidSpikesBiome.end900m, UnityEngine.Random.Range(0.25F, 0.75F));
				generated.Add(ent);
			}
			return true;
		}

		public override LargeWorldEntity.CellLevel getCellLevel() {
			return LargeWorldEntity.CellLevel.VeryFar;
		}

		private bool posIntersectsAnySpikes(Vector3 vec, string why) {
			foreach (SpikeCluster s in spikes) {
				if (s.posIntersectsAnySpikes(vec, why, null))
					return true;
			}
			return false;
		}

		private Vector3? getSafePosition() {
			if (count == 1)
				return position + offset;
			Vector3 sc = new Vector3(scaleXZ, scaleY, scaleXZ)*2;
			Vector3 ret = spikeLocationProvider != null ? spikeLocationProvider() : MathUtil.getRandomVectorAround(position+offset, Vector3.Scale(spacing[0], sc));
			int tries = 0;
			while (tries < 50 && !this.isValidPosition(ret)) {
				ret = spikeLocationProvider != null ? spikeLocationProvider() : MathUtil.getRandomVectorAround(position + offset, Vector3.Scale(spacing[tries / 10], sc));
				tries++;
			}
			return tries >= 50 ? (Vector3?)null : ret;
		}

		private bool isValidPosition(Vector3 ret) {
			return (positionValidity == null || positionValidity(ret)) && !this.isTooClose(ret);
		}

		private bool isTooClose(Vector3 pos) {
			foreach (SpikeCluster s in spikes) {
				Vector3 dist = s.position-pos;
				if ((dist.x * dist.x) + (dist.z * dist.z) <= 900) {
					return true;
				}
			}
			return false;
		}

		public class SpikeCluster : WorldGenerator {

			internal int terraceSpikeCount;
			internal int auxSpikeCount;
			internal bool generateAux;
			internal bool needsCenterSpace;

			public int fishCount = 0;

			private VoidSpike centralSpike;
			private readonly List<VoidSpike> firstRow = new List<VoidSpike>();
			private readonly List<VoidSpike> auxSpikes = new List<VoidSpike>();

			private float centralScale;
			private float edgeFactor;

			public Func<List<GameObject>, bool> additionalGen = null;

			internal SpikeCluster(Vector3 vec, bool aux, float f) : base(vec) {
				edgeFactor = f;
				terraceSpikeCount = (int)Mathf.Round(UnityEngine.Random.Range(Mathf.Lerp(3F, 4F, edgeFactor), Mathf.Lerp(3F, 8F, edgeFactor)));
				auxSpikeCount = (int)Mathf.Round(UnityEngine.Random.Range(Mathf.Lerp(2F, 3F, edgeFactor), Mathf.Lerp(2F, 9F, edgeFactor)));
				generateAux = aux;

				centralScale = UnityEngine.Random.Range(1.8F, 2.5F);
			}

			public override void loadFromXML(XmlElement e) {
				if (e.hasProperty("generateAux"))
					generateAux = e.getBoolean("generateAux");
				terraceSpikeCount = e.getInt("terraceSpikeCount", terraceSpikeCount);
				auxSpikeCount = e.getInt("auxSpikeCount", auxSpikeCount);
				edgeFactor = (float)e.getFloat("edgeFactor", edgeFactor);
			}

			public override void saveToXML(XmlElement e) {
				e.addProperty("generateAux", generateAux);
				e.addProperty("terraceSpikeCount", terraceSpikeCount);
				e.addProperty("auxSpikeCount", auxSpikeCount);
				e.addProperty("edgeFactor", edgeFactor);
			}

			public Vector3 getRootLocation() {
				return position + (Vector3.up * 0.5F * centralScale);
			}

			public override bool generate(List<GameObject> li) {
				centralSpike = new VoidSpike(position);
				centralSpike.spawner = spawner;
				centralSpike.setScale(centralScale);
				centralSpike.oreRichness = needsCenterSpace ? 0.1 : 0.2;
				centralSpike.plantRate = needsCenterSpace ? 1.25 : 2.5;
				centralSpike.needsCenterSpace = needsCenterSpace;
				if (needsCenterSpace) {
					centralSpike.hasFlora = false;
					centralSpike.hasPod = true;
					centralSpike.podSizeDecr = -2;
					centralSpike.hasFloater = false;
					centralScale = Math.Max(centralScale, 2);
					centralSpike.setScale(centralScale);
				}
				else if (UnityEngine.Random.Range(0, 4) > 0) {
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
					Vector3 pos = new Vector3(position.x+(radius*cos), position.y-down, position.z+(radius*sin));
					VoidSpike s = new VoidSpike(pos);
					s.spawner = spawner;
					s.hasFloater = false;
					s.hasFlora = true;
					s.plantRate = 2;
					if (radius <= 9)
						s.hasPod = false;
					if (s.hasPod) {
						s.podSizeDecr = 1;
						s.podOffset = new Vector3(0.125F * cos, 0, 0.125F * sin);
					}
					s.oreRichness = 0.5;
					s.validPlantPosCheck = (vec, n) => !this.posIntersectsAnySpikes(vec, n, s);
					s.setScale(Math.Min(s.getScale(), 1.2F));
					firstRow.Add(s);
					s.generateSpike();
				}
				if (generateAux) {
					this.generateAuxSpikes(centralSpike, 2);
					foreach (VoidSpike s0 in firstRow) {
						this.generateAuxSpikes(s0, 6);
					}
				}

				this.generateDeco(li);
				if (additionalGen != null) {
					additionalGen.Invoke(li);
				}

				for (int i = 0; i < fishCount; i++) {
					Vector3 vec = MathUtil.getRandomVectorAround(position, 60);
					if (this.posIntersectsAnySpikes(vec, "fish", null)) {
						i--;
						continue;
					}
					GameObject fish = spawner.Invoke(fishTypes.getRandomEntry());
					//SNUtil.log("Spawning fish "+fish+" @ "+vec);
					fish.transform.position = vec;
					li.Add(fish);
				}
				/*
                                GameObject atmo = spawner("58b3c65d-1915-497d-b652-f6beba004def");
                                atmo.transform.position = position;
                                atmo.transform.localScale = Vector3.one*75;
                                li.Add(atmo);
                                SNUtil.log("Generated atmo "+atmo+" @ "+atmo.transform.position);*/
				return true;
			}

			public override LargeWorldEntity.CellLevel getCellLevel() {
				return LargeWorldEntity.CellLevel.VeryFar;
			}

			private void generateDeco(List<GameObject> li) {
				//SNUtil.log("Decorating central "+centralSpike);
				this.generateDeco(li, centralSpike);
				foreach (VoidSpike s in firstRow) {
					//SNUtil.log("Decorating terrace "+s);
					this.generateDeco(li, s);
				}
				foreach (VoidSpike s in auxSpikes) {
					//SNUtil.log("Decorating aux "+s);
					this.generateDeco(li, s);
				}
			}

			private void generateDeco(List<GameObject> li, VoidSpike s) {
				s.generateFlora();
				s.generateResources();
				s.collateGenerated(li);
			}

			internal bool posIntersectsAnySpikes(Vector3 vec, string n, VoidSpike except) {
				double r = (n == "ore") ? 0 : (n.Contains("membrain") ? 0.3 : 0.15);
				//SNUtil.log("Checking "+vec+" "+n+" against central "+centralSpike);
				if (centralSpike.intersects(vec, r))
					return true;
				foreach (VoidSpike s in firstRow) {
					if (s == except)
						continue;
					//SNUtil.log("Checking "+vec+" "+n+" against terrace "+s);
					if (s.intersects(vec, r))
						return true;
				}
				foreach (VoidSpike s in auxSpikes) {
					if (s == except)
						continue;
					//SNUtil.log("Checking "+vec+" "+n+" against aux "+s);
					if (s.intersects(vec, r))
						return true;
				}
				return false;
			}

			private void generateAuxSpikes(VoidSpike s0, float down) {
				for (int i = 0; i < auxSpikeCount; i++) {
					Vector3 pos = MathUtil.getRandomVectorAround(s0.position-(Vector3.up*(down+1)), new Vector3(4, down, 4));
					pos.y = Math.Min(pos.y, s0.position.y - 1);
					VoidSpike s = new VoidSpike(pos);
					s.spawner = spawner;
					s.setScale(Math.Min(s.getScale(), 0.875F));
					s.hasFlora = true;
					s.hasFloater = false;
					s.hasPod = false;
					s.validPlantPosCheck = (vec, n) => !this.posIntersectsAnySpikes(vec, n, s);
					s.oreRichness = s0.oreRichness;
					s.isAux = true;
					s.plantRate = 1.5;
					auxSpikes.Add(s);
					s.generateSpike();
				}
			}

		}
	}
}
