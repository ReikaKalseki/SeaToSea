using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	[Obsolete]
	public class VoidLeviElecSphere : Spawnable {

		internal VoidLeviElecSphere() : base("levipulse", "", "") {
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<HeatSinkTag>().addField("spawnTime"));
			};
		}

		public override GameObject GetGameObject() {/*
			GameObject world = ObjectUtil.createWorldObject("1ff4c159-f8fe-443d-b3d3-f04a278459d9");
			*/
			GameObject sm = ObjectUtil.lookupPrefab("1c34945a-656d-4f70-bf86-8bc101a27eee");
			ElectricalDefense def = sm.GetComponent<SeaMoth>().seamothElectricalDefensePrefab.GetComponent<ElectricalDefense>();
			GameObject sphere = def.fxElecSpheres[def.fxElecSpheres.Length-1];
			GameObject world = UnityEngine.Object.Instantiate(sphere);
			world.SetActive(false);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
			world.removeComponent<VFXDestroyAfterSeconds>();
			VoidLeviElecSphereComponent kc = world.EnsureComponent<VoidLeviElecSphereComponent>();
			Renderer r = world.GetComponentInChildren<Renderer>();
			return world;
		}

	}

	class VoidLeviElecSphereComponent : MonoBehaviour {

		private float spawnTime;

		void Awake() {
			LargeWorldStreamer.main.MakeEntityTransient(gameObject);
		}

		private void Update() {
			this.GetComponent<ParticleSystem>().Play(true);
			if (spawnTime <= 0)
				return;
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = time-spawnTime;
			transform.localScale = Vector3.one * (0.0001f + dT);
			if (dT >= 10)
				gameObject.destroy();
		}

		internal void spawn() {
			spawnTime = DayNightCycle.main.timePassedAsFloat;
		}

	}
}
