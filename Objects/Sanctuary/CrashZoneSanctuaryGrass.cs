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

	public class CrashZoneSanctuaryFern : Spawnable {

		internal CrashZoneSanctuaryFern() : base("CrashZoneSanctuaryFern", "", "") {

		}

		public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject(VanillaFlora.VEINED_NETTLE.getRandomPrefab(false));
			go.removeComponent<LiveMixin>();
			go.removeComponent<Collider>();
			go.removeComponent<Rigidbody>();
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			Renderer main = null;
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
				if (!r.name.Contains("LOD"))
					main = r;
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Plants/SanctuaryGrass", new Dictionary<int, string> { { 0, "" }, { 1, "" }, { 2, "" }, { 3, "" } });
				r.materials[0].DisableKeyword("MARMO_EMISSION");
				r.materials[1].DisableKeyword("MARMO_EMISSION");
				r.receiveShadows = false;
				r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
			GameObject r2 = main.gameObject.clone();
			r2.transform.SetParent(main.transform.parent);
			r2.transform.localPosition = Vector3.zero;
			r2.transform.localScale = Vector3.one;
			r2.transform.localRotation = Quaternion.Euler(270, UnityEngine.Random.Range(50, 130), 0);
			go.transform.localScale = new Vector3(2.5F, 3F, 2.5F);
			return go;
		}

	}
}
