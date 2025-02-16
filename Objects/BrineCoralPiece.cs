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
	
	public class BrineCoralPiece : Spawnable {
	        
	    internal BrineCoralPiece(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			
	    }
		
		protected override Atlas.Sprite GetItemSprite() {
			return TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/BrineCoralPiece");
		}
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaResources.TITANIUM.prefab);
			Renderer r = world.GetComponentInChildren<Renderer>();
			GameObject mdl = RenderUtil.setModel(r, ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("908d3f0e-04b9-42b4-80c8-a70624eb5455"), "lost_river_skull_coral_01"));
			r = mdl.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/BrineCoralPiece");
			world.SetActive(false);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			return world;
	    }
			
	}
}
