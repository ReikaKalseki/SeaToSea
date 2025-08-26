using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public class Arrow : BasicCraftingItem {

		public Arrow(string id, string name, string desc, string template) : base(id, name, desc, template) {
			sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/" + id);
		}

		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.None;
			}
		}

		public sealed override TechGroup GroupForPDA {
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
