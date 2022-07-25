using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class Bioprocessed : BasicCraftingItem {
		
		public Bioprocessed(string id, string name, string desc, string template) : base(id, name, desc, template) {
			
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.None;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Uncategorized;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.Misc;
			}
		}
		
	}
}
