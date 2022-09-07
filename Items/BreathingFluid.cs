using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class BreathingFluid : BasicCraftingItem {
		
		public BreathingFluid() : base(SeaToSeaMod.itemLocale.getEntry("breathfluid"), "WorldEntities/Natural/polyaniline") {
			sprite = TextureManager.getSprite("Textures/Items/BreathFluid");
			unlockRequirement = TechType.Unobtanium;//SeaToSeaMod.rebreatherV2.TechType;
			craftingSubCategory = "C2Chemistry";
			craftingTime = 15;
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
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
				return SeaToSeaMod.chemistryCategory;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"Resources", "C2Chemistry"};
			}
		}

		public override Vector2int SizeInInventory {
			get {
				return new Vector2int(3, 3);
			}
		}
		
	}
}
