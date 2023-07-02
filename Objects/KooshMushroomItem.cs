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
	
	public class KooshMushroomItem : Spawnable {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
	    internal KooshMushroomItem(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("2d72ad6c-d30d-41be-baa7-0c1dba757b7c");
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			world.EnsureComponent<KooshMushroomItemTag>();
			return world;
	    }
		
		public void register() {
			Patch();
		}
			
	}
		
	class KooshMushroomItemTag : MonoBehaviour {
		
	
		
	}
}
