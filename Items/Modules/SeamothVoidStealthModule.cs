using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class SeamothVoidStealthModule : SeamothModule {
		
		private static readonly string DESC = "Greatly reduces the detectability of basic seamoth systems to passive listeners. Will not affect active echolocators, or creatures at close range.";
				
		public SeamothVoidStealthModule() : base("SeamothVoidStealth", "Acoustic Suppression Module", DESC) {
			
		}

		public override TechType RequiredForUnlock {
			get {
				return TechType.BaseUpgradeConsole;
			}
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Workbench;
			}
		}

		public override QuickSlotType QuickSlotType {
			get {
				return QuickSlotType.Passive;
			}
		}
		
		protected override Atlas.Sprite GetItemSprite()
		{
			return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
		}
	}
}
