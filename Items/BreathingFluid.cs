using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public class BreathingFluid : BasicCraftingItem {

		public BreathingFluid() : base(SeaToSeaMod.itemLocale.getEntry("breathfluid"), "WorldEntities/Natural/polyaniline") {
			sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/BreathFluid");
			unlockRequirement = TechType.Unobtanium;//SeaToSeaMod.rebreatherV2.TechType;
			craftingSubCategory = "C2Chemistry";
			craftingTime = 15;
			inventorySize = new Vector2int(3, 3);
			renderModify = r => {
				r.transform.localScale = new Vector3(2.4F, 2.4F, 1);
				r.setPolyanilineColor(new Color(1, 158 / 255F, 201 / 255F, 1.5F));
				r.materials[1].SetFloat("_Shininess", 5F);
				r.materials[1].SetFloat("_SpecInt", 12F);
				r.materials[1].SetFloat("_Fresnel", 0F);
			};
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
				return new string[] { "Resources", "C2Chemistry" };
			}
		}

	}
}
