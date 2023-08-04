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
	
	public class Mattress : Spawnable {
	        
	    internal Mattress() : base("Mattress", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("c3994649-d0da-4f8c-bb77-1590f50838b9"), "bed_narrow"));
			ObjectUtil.removeChildObject(go, "bed_narrow");
			ObjectUtil.removeChildObject(go, "blanket_narrow");
			ObjectUtil.removeChildObject(go, "end_position");
			ObjectUtil.removeChildObject(go, "obstacle_check");
			ObjectUtil.getChildObject(go, "matress_narrow").transform.localPosition = Vector3.zero;
			ObjectUtil.getChildObject(go, "pillow_01").transform.localPosition = new Vector3(0, 0.11F, -0.67F);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			go.EnsureComponent<TechTag>().type = TechType;
			return go;
	    }

		protected override void ProcessPrefab(GameObject go) {
			base.ProcessPrefab(go);
		}
			
	}
}
