using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class SeamothDepthModule : SeamothModule {
		
		public readonly int maxDepth;
		public readonly int depthBonus;
		
		public SeamothDepthModule(string id, string name, string desc, int d) : base(id, name, desc) {
			maxDepth = d;
			depthBonus = maxDepth-200;
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
	}
}
