using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	internal class C2CRocket : MonoBehaviour {

		private Rocket rocket;
		private RocketLocker[] lockers;

		private float lastPDAUpdate = -1;

		void Awake() {
			this.getLockers();
		}

		void getLockers() {
			lockers = this.GetComponentsInChildren<RocketLocker>();
			foreach (RocketLocker cl in lockers) {
				StorageContainer sc = cl.GetComponent<StorageContainer>();
				sc.Resize(6, 8);
			}
		}

		void Update() {
			if (C2CHooks.skipRocketTick)
				return;
			if (!rocket)
				rocket = this.GetComponent<Rocket>();
			if (lockers == null)
				this.getLockers();

			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastPDAUpdate >= 0.5F) {
				lastPDAUpdate = time;
				List<ItemsContainer> li = lockers.Where(l => (bool)l).Select(sc => sc.GetComponent<StorageContainer>().container).ToList();
				if (Player.main.precursorOutOfWater && Player.main.transform.position.y > 30) //in rocket
					li.Add(Inventory.main.container);
				FinalLaunchAdditionalRequirementSystem.instance.updateContentsAndPDAPageChecklist(rocket, li);
			}
		}

	}
}
