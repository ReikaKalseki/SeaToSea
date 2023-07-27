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
			ObjectUtil.removeComponent<LiveMixin>();
			ObjectUtil.removeComponent<Collider>();
			Renderer r = go.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/SanctuaryGrass", new Dictionary<int, string>{{0, ""}, {1, ""}, {2, ""}, {3, ""}});
			r.materials[0].DisableKeyword("MARMO_EMISSION");
			r.materials[1].DisableKeyword("MARMO_EMISSION");
			GameObject r2 = UnityEngine.Object.Instantiate(r.gameObject);
			r2.transform.SetParent(r.transform.parent);
			r2.transform.localPosition = Vector3.zero;
			r2.transform.localScale = Vector3.one;
			r2.transform.localRotation = Quaternion.Euler(270, UnityEngine.Random.Range(30, 150), 0);
			return go;
	    }
			
	}
}
