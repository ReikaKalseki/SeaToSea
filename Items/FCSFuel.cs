using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class FCSFuel : BasicCraftingItem {
		
		public FCSFuel() : base(SeaToSeaMod.itemLocale.getEntry("FCSFuel"), "WorldEntities/Natural/Lubricant") {
			sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/FCSFuel");
			unlockRequirement = TechType.Unobtanium;
			craftingTime = 6;
			numberCrafted = 4;
			inventorySize = new Vector2int(2, 2);
		}

		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Fabricator;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Resources;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return C2CItems.chemistryCategory;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"Resources", "C2Chemistry"};
			}
		}
	}
}
