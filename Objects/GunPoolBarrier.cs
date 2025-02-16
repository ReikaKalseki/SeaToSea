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
	
	public class GunPoolBarrier : LockedPrecursorDoor {
	        
		internal GunPoolBarrier() : base("GunPoolDoor", PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Orange, new PositionedPrefab("", new Vector3(481.808F, -125.032F, 1257.852F), Quaternion.Euler(0, 20, 0), Vector3.one*4), new PositionedPrefab("", new Vector3(460.4F, -93.85F, 1236.9F), Quaternion.Euler(0, 200, 0))) {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = base.GetGameObject();
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
			return go;
	    }
			
	}
}
