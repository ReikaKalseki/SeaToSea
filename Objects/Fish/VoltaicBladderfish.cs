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

	public class VoltaicBladderfish : RetexturedFish, MultiTexturePrefab {

		private readonly XMLLocale.LocaleEntry locale;

		internal VoltaicBladderfish(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.BLADDERFISH.prefab) {
			locale = e;
			glowIntensity = 1.0F;
		}

		public override void prepareGameObject(GameObject world, Renderer[] r0) {
			VoltaicBladderfishTag kc = world.EnsureComponent<VoltaicBladderfishTag>();
			foreach (Renderer r in r0) {
				foreach (Material m in r.materials) {
					m.SetColor("_GlowColor", new Color(1, 1, 1, 1));
					RenderUtil.disableTransparency(m);
				}
			}

			GameObject inner = ObjectUtil.lookupPrefab(VanillaCreatures.BOOMERANG.prefab).GetComponentInChildren<Animator>().gameObject.clone();
			inner.transform.SetParent(world.GetComponentInChildren<Animator>().transform);
			inner.gameObject.name = "AuxMdl";
			foreach (Renderer r in inner.GetComponentsInChildren<Renderer>()) {
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Creature/VoltaicBladderfishAux");
				RenderUtil.setEmissivity(r, 1);
			}
			Utils.ZeroTransform(inner.transform);
			inner.transform.localScale = new Vector3(1, 0.5F, 1.2F);
			inner.transform.localPosition = new Vector3(-0.0075F, 0, 0.155F);

			world.EnsureComponent<VoltaicFishSparker>();
		}

		public override BehaviourType getBehavior() {
			return BehaviourType.SmallFish;
		}

		public Dictionary<int, string> getTextureLayers(Renderer r) {
			return new Dictionary<int, string> { { 0, "" }, { 1, "" } };
		}

	}
	
	class VoltaicFishSparker : AzuriteSparker {

		public VoltaicFishSparker() : base(2.5F, 0.5F, false, new Vector3(0, 0, 0)) {

		}

		public override bool disableSparking() {
			return GetComponent<WaterParkCreature>();
		}
	}

	class VoltaicBladderfishTag : MonoBehaviour {

		private Renderer[] renders;

		private float currentEmissivity = 1;
		private float targetEmissivity;

		void Update() {
			if (renders == null)
				renders = this.GetComponentsInChildren<Renderer>();

			if (Mathf.Abs(currentEmissivity-targetEmissivity) < 0.1F) {
				targetEmissivity = UnityEngine.Random.Range(0.9F, 2.5F);
			}
			else {
				currentEmissivity += Mathf.Sign(targetEmissivity - currentEmissivity) * Time.deltaTime;
			}

			foreach (Renderer r in renders) {
				RenderUtil.setEmissivity(r, currentEmissivity);
			}

			transform.localScale = Vector3.one*1.5F;
		}

	}
}
