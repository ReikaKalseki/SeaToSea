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
	
	public class UnmovingHeatBlade : PickedUpAsOtherItem {
	        
	    internal UnmovingHeatBlade() : base("UnmovingHeatBlade", TechType.HeatBlade) {
			
	    }
			
	    protected override void prepareGameObject(GameObject go) {
			go.GetComponent<Rigidbody>().isKinematic = true;
			go.EnsureComponent<UnmovingHeatBladeTag>();
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
