using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{
		internal class C2Crawler : MonoBehaviour {
		
			private BiomeBase biome;
        	
			void Start() {
				base.InvokeRepeating("tick", 0f, 0.5F);
			}

			private void OnKill() {
				UnityEngine.Object.Destroy(this);
			}
			
			void OnDisable() {
				base.CancelInvoke("tick");
			}

			internal void tick() {
				BiomeBase at = BiomeBase.getBiome(transform.position);
				if (at != biome && (at == VanillaBiomes.BLOODKELP || at == CrashZoneSanctuaryBiome.instance)) {
					foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
	    				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/CaveCrawlerBlue");
					}
				}
				biome = at;
			}
			
		}
}
