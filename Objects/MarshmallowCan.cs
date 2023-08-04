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
	
	public class MarshmallowCan : Spawnable {
	        
	    internal MarshmallowCan() : base("MarshmallowCan", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject go = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(ObjectUtil.lookupPrefab(TechType.PlanterPot), "model/Base_interior_Planter_Pot_01"));
			ObjectUtil.removeChildObject(go, "pot_generic_plant_01");
			go.transform.localScale = new Vector3(0.2F, 0.2F, 0.5F);
			GameObject lid = ObjectUtil.getChildObject(go, "Base_exterior_Planter_Tray_ground");
			lid.transform.localPosition = new Vector3(0, 0, 0.06F);
			GameObject can = ObjectUtil.getChildObject(go, "Base_interior_Planter_Pot_01 1");
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, lid.GetComponentInChildren<Renderer>(), "Textures/marshmallows");
			Renderer cr = can.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, cr, "Textures/marshmallowcan");
			RenderUtil.setGlossiness(cr, 6F, 0, 1);
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			go.EnsureComponent<TechTag>().type = TechType;
			return go;
	    }
			
	}
}
