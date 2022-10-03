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
	
	public class VoidSpikeLeviathan : Spawnable {
	        
	    internal VoidSpikeLeviathan() : base("", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			return null;
	    }
			
	}
}
