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
	
	public class SanctuaryGrassSpawner : WorldGenerator {
	        
	    public SanctuaryGrassSpawner(Vector3 pos) : base(pos) {
			
	    }
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
			
	    public override void generate(List<GameObject> li) {	
			
	    }
			
	}
}
