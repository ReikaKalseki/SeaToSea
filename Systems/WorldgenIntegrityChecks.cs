﻿using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using UnityEngine.UI;

using FMOD;
using FMODUnity;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public static class WorldgenIntegrityChecks {
		
		private static bool xmlLoadFailure = false;
		
		private static readonly List<string> currentErrorText = new List<string>();
		
		public static bool checkWorldgenIntegrity(bool flag) {
			xmlLoadFailure = SeaToSeaMod.worldgen.getCount() <= 0;
			if (flag || xmlLoadFailure || SeaToSeaMod.mushroomBioFragment.getFragmentCount() <= 0 || SeaToSeaMod.geyserCoral.getFragmentCount() <= 0 || DataboxTypingMap.instance.isEmpty()) {
				currentErrorText.Clear();
				currentErrorText.Add("C2C worldgen failed to initialize, and all progression is invalid! Do not continue playing!");
				if (xmlLoadFailure)
					currentErrorText.Add("Main worldgen DB failed to load");
				DIHooks.setWarningText(currentErrorText);
				return true;
			}
			return false;
		}
		
		public static void throwError() {
			throw new Exception(currentErrorText.toDebugString());
		}

	}
	
}
