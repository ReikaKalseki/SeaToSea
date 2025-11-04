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

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

using ReikaKalseki.Ecocean;

namespace ReikaKalseki.SeaToSea {
	public class LavaCastleSmokeVolumeTrigger : MonoBehaviour {

		private static float lastCollectTime = 0;

		void OnTriggerStay(Collider other) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastCollectTime < 2.5F)
				return;
			SeaMoth sm = other.gameObject.FindAncestor<SeaMoth>();
			if (sm) {
				SeamothPlanktonScoop.checkAndTryScoop(sm, Time.deltaTime, CraftingItems.getItem(CraftingItems.Items.LavaPlankton).TechType, out GameObject drop);
				if (drop) {
					lastCollectTime = time;
				}
			}
			
		}


	}
}
