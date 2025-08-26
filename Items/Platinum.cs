using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public class Platinum : BasicCustomOre {

		public Platinum(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			collectSound = "event:/loot/pickup_diamond";
		}

		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);


			go.EnsureComponent<PlatinumTag>();
		}

	}

	class PlatinumTag : MonoBehaviour {

		private float lastTime;

		private float lastPickupTime;
		private float timeOnGround;
		private DeepStalkerTag currentStalker;

		private ResourceTracker resource;

		private float spawnTime;
		private float lastPLayerDistanceCheckTime;

		void Start() {

		}

		void Update() {
			if (!resource)
				resource = this.GetComponent<ResourceTracker>();
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = time-lastTime;
			if (spawnTime <= 0)
				spawnTime = time;
			if (spawnTime > 0 && time - lastPLayerDistanceCheckTime >= 0.5) {
				lastPLayerDistanceCheckTime = time;
				if (Player.main && Vector3.Distance(transform.position, Player.main.transform.position) > 250 && !gameObject.FindAncestor<StorageContainer>()) {
					gameObject.destroy(false);
				}
			}
			if (spawnTime > 0 && time - spawnTime >= 600 && !currentStalker && !gameObject.FindAncestor<StorageContainer>()) {
				gameObject.destroy(false);
			}
			if (dT >= 1) {
				gameObject.EnsureComponent<ResourceTrackerUpdater>().tracker = resource;
			}
			lastTime = time;
			if (currentStalker)
				timeOnGround = 0;
			else
				timeOnGround += dT;
		}

		public void pickup(DeepStalkerTag s) {
			currentStalker = s;
			lastPickupTime = DayNightCycle.main.timePassedAsFloat;
		}

		public void drop() {
			currentStalker = null;
		}

		public float getTimeOnGround() {
			return timeOnGround;
		}

	}
}
