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
	public class CrashZoneSanctuaryCoralSheet : Spawnable {

		internal CrashZoneSanctuaryCoralSheet() : base("CrashZoneSanctuaryCoralSheet", "", "") {

		}

		public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject("");
			go.removeComponent<LiveMixin>();
			go.removeComponent<Collider>();
			go.removeComponent<PlantBehaviour>();
			go.removeComponent<FMOD_StudioEventEmitter>();
			go.removeComponent<CoralBlendWhite>();
			go.removeComponent<Light>();
			Renderer r = go.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/SanctuaryCoral");
			r.material.DisableKeyword("MARMO_EMISSION");
			r.receiveShadows = false;
			r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			go.transform.localScale = new Vector3(10, 1, 10);
			return go;
		}

	}
}
