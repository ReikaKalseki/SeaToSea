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
	
	public class UnmovingHeatBlade : Spawnable {
	        
	    internal UnmovingHeatBlade() : base("UnmovingHeatBlade", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = UnityEngine.Object.Instantiate(CraftData.GetPrefabForTechType(TechType.HeatBlade));
			world.SetActive(true);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			Pickupable pp = world.EnsureComponent<Pickupable>();
			pp.SetTechTypeOverride(TechType.HeatBlade);
			world.GetComponent<Rigidbody>().isKinematic = true;
			world.EnsureComponent<UnmovingHeatBladeTag>();
			return world;
	    }
			
	}
		
	class UnmovingHeatBladeTag : MonoBehaviour {
		
		private Rigidbody body;
		
		void Update() {
			if (!body) {
				body = GetComponent<Rigidbody>();
			}
			body.isKinematic = true;
		}
		
	}
}
