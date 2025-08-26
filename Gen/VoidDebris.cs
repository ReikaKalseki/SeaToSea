using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public sealed class VoidDebris : WorldGenerator {

		private GenUtil.ContainerPrefab databoxPrefab;

		static VoidDebris() {

		}

		public VoidDebris(Vector3 pos) : base(pos) {
			spawner = VoidSpikesBiome.spawnEntity;
		}

		public void init() {
			databoxPrefab = GenUtil.getOrCreateDatabox(CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, go => go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global);
		}

		public override void loadFromXML(XmlElement e) {

		}

		public override void saveToXML(XmlElement e) {

		}

		public override bool generate(List<GameObject> li) {
			for (int i = 0; i < 6; i++) {
				li.Add(this.generateObjectInRange(6, 0.1F, 6));
			}
			foreach (Prop s in PodDebris.alwaysPieces) {
				li.Add(this.generateObjectInRange(9, 0.25F, 9, 0, s));
			}
			for (int i = 0; i < 10; i++) {
				li.Add(this.generateObjectInRange(12, 0.5F, 12));
			}
			for (int i = 0; i < 8; i++) {
				li.Add(this.generateObjectInRange(4, 3, 4, -2, PodDebris.papers[UnityEngine.Random.Range(0, PodDebris.papers.Count)]));
			}
			GameObject go = spawner(databoxPrefab.ClassID);
			go.transform.position = MathUtil.getRandomVectorAround(position + (Vector3.down * 3.5F), 1);
			go.transform.rotation = UnityEngine.Random.rotationUniform;
			DamagedDataboxSystem.instance.onDataboxSpawn(go);
			go.fullyEnable();
			VoidSpikesBiome.checkAndAddWaveBob(go, true);
			return true;
		}

		public override LargeWorldEntity.CellLevel getCellLevel() {
			return LargeWorldEntity.CellLevel.Global;
		}
		/*
		[Obsolete]
		public GameObject spawnPDA() {
			GameObject pda = spawner(SignalManager.getSignal("voidpod").signalHolder.ClassID);
			pda.transform.rotation = UnityEngine.Random.rotationUniform;
			VoidSpikesBiome.checkAndAddWaveBob(pda, true);
			return pda;
		}*/

		private GameObject generateObjectInRange(float dx, float dy, float dz, double offsetY = 0, Prop type = null) {
			Prop p = type == null ? PodDebris.debrisProps.getRandomEntry() : type;
			float tilt = 0;
			if (p.freeAngle) {
				tilt = UnityEngine.Random.Range(0, 360F);
			}
			else {
				tilt = p.baseAngles[UnityEngine.Random.Range(0, p.baseAngles.Length)];
				tilt = UnityEngine.Random.Range(tilt - 15F, tilt + 15F);
			}
			GameObject go = spawner(p.prefab.ClassID);
			if (go == null)
				return go;
			Vector3 pos = MathUtil.getRandomVectorAround(position, new Vector3(dx, dy, dz));
			pos.y = Math.Min(pos.y, position.y) + (float)offsetY;
			go.transform.position = pos;
			go.transform.rotation = Quaternion.Euler(tilt, UnityEngine.Random.Range(0, 360F), 0);
			VoidSpikesBiome.checkAndAddWaveBob(go, true);
			return go;
		}
	}
}
