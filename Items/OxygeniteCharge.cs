using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public class OxygeniteCharge : BasicCraftingItem {

		public OxygeniteCharge() : base(SeaToSeaMod.itemLocale.getEntry("OxygeniteCharge"), "WorldEntities/Natural/FirstAidKit") {
			sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/OxygeniteCharge");
			unlockRequirement = TechType.Unobtanium;
			craftingTime = 20;
			inventorySize = new Vector2int(3, 3);
			renderModify = r => {
				
			};
		}

		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Workbench;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Workbench;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.Workbench;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[] { "Tank" };
			}
		}
	}
}
