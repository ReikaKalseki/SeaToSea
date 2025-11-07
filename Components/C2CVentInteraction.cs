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

using Story;

namespace ReikaKalseki.SeaToSea {
	public class C2CVentInteraction : MonoBehaviour, IHandTarget {

		public void OnHandClick(GUIHand hand) {
			if (C2CProgression.instance.isPipeTravelEnabled(out bool invis)) {
				PipeTravelSystem.requestTravel(GetComponent<PrefabIdentifier>());
			}
			else if (!invis) {
				SoundManager.playSound("event:/env/keypad_wrong");
			}
		}

		public void OnHandHover(GUIHand hand) {
			if (C2CProgression.instance.isPipeTravelEnabled(out bool invis)) {
				HandReticle.main.SetInteractText("VentClick"); //is a locale key
				HandReticle.main.SetIcon(HandReticle.IconType.Interact);
			}
			else if (!invis) {
				HandReticle.main.SetInteractText("VentClickDeny"); //is a locale key
				HandReticle.main.SetIcon(HandReticle.IconType.HandDeny);
			}
		}
	}
}
