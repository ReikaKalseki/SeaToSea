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
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class CrashZoneSanctuaryGrass : Spawnable {
	        
	    internal CrashZoneSanctuaryGrass() : base("CrashZoneSanctuaryGrass", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject(VanillaFlora.VEINED_NETTLE.getRandomPrefab(false));
			ObjectUtil.removeComponent<LiveMixin>(go);
			ObjectUtil.removeComponent<Collider>(go);
			ObjectUtil.removeComponent<Rigidbody>(go);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			Renderer main = null;
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
				if (!r.name.Contains("LOD"))
					main = r;
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Plants/SanctuaryGrass", new Dictionary<int, string>{{0, ""}, {1, ""}, {2, ""}, {3, ""}});
				r.materials[0].DisableKeyword("MARMO_EMISSION");
				r.materials[1].DisableKeyword("MARMO_EMISSION");
				r.receiveShadows = false;
				r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
			GameObject r2 = UnityEngine.Object.Instantiate(main.gameObject);
			r2.transform.SetParent(main.transform.parent);
			r2.transform.localPosition = Vector3.zero;
			r2.transform.localScale = Vector3.one;
			r2.transform.localRotation = Quaternion.Euler(270, UnityEngine.Random.Range(50, 130), 0);
			go.transform.localScale = new Vector3(2.5F, 3F, 2.5F);
			return go;
	    }
			
	}
}
