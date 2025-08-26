using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public class KharaaTreatment : BasicCraftingItem {

		public KharaaTreatment() : base(SeaToSeaMod.itemLocale.getEntry("KharaaTreatment"), "WorldEntities/Natural/FirstAidKit") {
			sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/KharaaTreatment");
			unlockRequirement = TechType.Unobtanium;//TechType.Workbench;//SeaToSeaMod.healFlower.TechType;
			craftingTime = 18;
			inventorySize = new Vector2int(2, 2);
			renderModify = r => {
				r.transform.localScale = new Vector3(1, 3, 1);
				r.materials[0].SetFloat("_Shininess", 7.5F);
				r.materials[0].SetFloat("_SpecInt", 10F);
				r.materials[0].SetFloat("_Fresnel", 0.5F);
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
				return new string[] { "C2CMedical" };
			}
		}
	}
}
