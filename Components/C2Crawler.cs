using System;
using System.Collections.Generic;
using System.IO;
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
	internal class C2Crawler : MonoBehaviour {

		private BiomeBase biome;

		void Start() {
			base.InvokeRepeating("tick", 0f, 0.5F);
		}

		private void OnKill() {
			this.destroy(false);
		}

		void OnDisable() {
			base.CancelInvoke("tick");
		}

		internal void tick() {
			if (C2CHooks.skipCrawlerTick)
				return;
			BiomeBase at = BiomeBase.getBiome(transform.position);
			if (at != biome && (at == VanillaBiomes.BLOODKELP || at == CrashZoneSanctuaryBiome.instance)) {
				foreach (Renderer r in this.GetComponentsInChildren<Renderer>()) {
					RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/CaveCrawlerBlue");
				}
			}
			biome = at;
		}

	}
}
