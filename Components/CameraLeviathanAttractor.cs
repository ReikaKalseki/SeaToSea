using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class CameraLeviathanAttractor : MonoBehaviour, AggroAttractor {

		private float lastLeviCheckTime;

		private MapRoomCamera cam;

		public bool isAggroable {
			get {
				return !cam.dockingPoint;
			}
		}

		public void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (!cam)
				cam = GetComponent<MapRoomCamera>();
			if (time-lastLeviCheckTime >= 2 && cam && !cam.dockingPoint) {
				doLeviCheck();
				lastLeviCheckTime = time;
			}
		}

		private void doLeviCheck() {
			ECHooks.attractToSoundPing(this, false, 0.375F); //150m
		}

	}
}
