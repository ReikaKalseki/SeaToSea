using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class Bioprocessor : CustomMachine { //TODO make this consume salt to operate
		
		public Bioprocessor() : base("bioprocessor", "Bioprocessor", "Decomposes and recombines organic matter into useful raw chemicals.", "6d71afaa-09b6-44d3-ba2d-66644ffe6a99") {
			addIngredient(TechType.TitaniumIngot, 1);
			addIngredient(TechType.Magnetite, 12);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 2);
			addIngredient(TechType.CopperWire, 1);
			addIngredient(TechType.Glass, 3);
		}
		
		protected override void onTick(GameObject go) {
			SBUtil.writeToChat("I am ticking @ "+go.transform.position);
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			IList<InventoryItem> salt = con.container.GetItems(TechType.Salt);
			if (salt != null && salt.Count >= 4) {
				for (int i = 0; i < 4; i++) {
					con.container.RemoveItem(salt[i], true, true);
				}
			}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			UnityEngine.Object.Destroy(go.GetComponent<Aquarium>());
		}
		
	}
}
