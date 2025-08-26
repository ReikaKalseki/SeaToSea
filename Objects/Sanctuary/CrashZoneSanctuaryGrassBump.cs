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

	[Obsolete]
	public class CrashZoneSanctuaryGrassBump : Spawnable {

		internal CrashZoneSanctuaryGrassBump() : base("CrashZoneSanctuaryGrassBump", "", "") {

		}

		public override GameObject GetGameObject() {
			string pfb = VanillaFlora.MUSHROOM_BUMP.getRandomPrefab(false);
			GameObject go = ObjectUtil.createWorldObject(pfb);
			go.removeComponent<LiveMixin>();
			go.removeComponent<Collider>();
			go.removeComponent<PlantBehaviour>();
			go.removeComponent<FMOD_StudioEventEmitter>();
			go.removeComponent<CoralBlendWhite>();
			go.removeComponent<Light>();
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/SanctuaryGrassBump");
				r.material.DisableKeyword("MARMO_EMISSION");
				r.receiveShadows = false;
				r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				if (pfb == "5086a02a-ea6d-41ba-90c3-ea74d97cf6b5") {
					r.transform.localRotation = Quaternion.Euler(-90, 0, 0);
					r.transform.localScale = new Vector3(1, 0.25F, 1);
				}
				else if (pfb != "f3de21af-550b-4901-a6e8-e45e31c1509d") {
					r.transform.localScale = new Vector3(1, 0.5F, 1);
				}
			}
			return go;
		}

	}
}
