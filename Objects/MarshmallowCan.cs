﻿using System;
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
			/*
			Vector3[] pos = new Vector3[]{lid.transform.localPosition};
			Vector3[] rot = new Vector3[]{Vector3.zero};
			for (int i = 0; i < pos.Length; i++) {
				GameObject par = new GameObject("Marshmallow");
				GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
				par.transform.localScale = new Vector3(0.2F, 0.4F, 0.2F);
				cyl.name = "Marshmallow";
				par.transform.SetParent(go.transform);
				cyl.transform.SetParent(par.transform);
				par.transform.localPosition = pos[i];
				par.transform.localRotation = Quaternion.Euler(rot[i]);
				cyl.transform.localScale = Vector3.one;
				cyl.transform.localPosition = Vector3.zero;
				cyl.transform.localRotation = Quaternion.identity;
				ObjectUtil.removeComponent<Collider>(cyl);
				ECCLibrary.ECCHelpers.ApplySNShaders(cyl, new ECCLibrary.UBERMaterialProperties(0, 2, 0));
				Renderer r = cyl.GetComponentInChildren<Renderer>();
				RenderUtil.setGlossiness(r, 2, 0, 1);
			}*/
			return go;
	    }
			
	}
}
