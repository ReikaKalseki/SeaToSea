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
	
	public class DeadMelon : Spawnable {
	        
	    internal DeadMelon() : base("DeadMelon", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject("e9445fdf-fbae-49dc-a005-48c05bf9f401");
			ObjectUtil.removeComponent<Pickupable>(go);
			ObjectUtil.removeComponent<PickPrefab>(go);
			ObjectUtil.removeComponent<LiveMixin>(go);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			return go;
	    }

		protected override void ProcessPrefab(GameObject go) {
			base.ProcessPrefab(go);
			go.EnsureComponent<TechTag>().type = TechType.MelonPlant;
		}
			
	}
}
