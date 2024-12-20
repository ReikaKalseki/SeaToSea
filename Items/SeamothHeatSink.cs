﻿using System;
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
	
	public class SeamothHeatSink : BasicCraftingItem {
		
		public SeamothHeatSink() : base(SeaToSeaMod.itemLocale.getEntry("SeamothHeatSink"), "WorldEntities/Natural/CopperWire") {
			sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/Seamothheatsink");
			craftingSubCategory = "Tools";
			craftingTime = 4;
			unlockRequirement = TechType.Unobtanium;
			inventorySize = new Vector2int(1, 2);
			renderModify = r => {
				EjectedHeatSink.setTexture(r);
			};
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.SeamothUpgrades;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.VehicleUpgrades;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.VehicleUpgrades;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"Torpedoes"};
			}
		}
		
	}
}
