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

	public class PurpleHoopfish : RetexturedFish {

		private readonly XMLLocale.LocaleEntry locale;

		internal PurpleHoopfish(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.HOOPFISH.prefab) {
			locale = e;
			glowIntensity = 1.0F;
		}

		public override void prepareGameObject(GameObject world, Renderer[] r0) {
			PurpleHoopfishTag kc = world.EnsureComponent<PurpleHoopfishTag>();
			foreach (Renderer r in r0) {
				r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
			}
		}

		public override BehaviourType getBehavior() {
			return BehaviourType.SmallFish;
		}

	}

	class PurpleHoopfishTag : MonoBehaviour {

		private Renderer[] renders;

		void Update() {
			if (renders == null)
				renders = this.GetComponentsInChildren<Renderer>();
		}

	}
}
