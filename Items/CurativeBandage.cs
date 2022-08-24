using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class CurativeBandage : BasicCraftingItem {
		
		public CurativeBandage() : base(SeaToSeaMod.itemLocale.getEntry("CurativeBandage"), "WorldEntities/Natural/FirstAidKit") {
			sprite = TextureManager.getSprite("Textures/Items/CurativeBandage");
			unlockRequirement = SeaToSeaMod.healFlower.TechType;
			craftingTime = 6;
		}

		public override void prepareGameObject(GameObject go, Renderer r) {
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
				return new string[]{};
			}
		}

		public override Vector2int SizeInInventory {
			get {
				return new Vector2int(1, 2);
			}
		}
	}
}
