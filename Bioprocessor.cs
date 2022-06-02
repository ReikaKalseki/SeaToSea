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
	
	public class Bioprocessor : CustomMachine<BioprocessorLogic> {
		
		internal static readonly Dictionary<TechType, BioRecipe> recipes = new Dictionary<TechType, BioRecipe>();
		
		public static void addRecipes() {
			addRecipe(TechType.AcidMushroom, TechType.HydrochloricAcid, 6, 20).inputCount = 2;
			addRecipe(TechType.BloodOil, TechType.Benzene).inputCount = 4;
			addRecipe(SeaToSeaMod.alkali.TechType, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 5, 30).outputCount = 2;
			BioRecipe r = addRecipe(TechType.GasPod, CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType, 1, 15);
			r.inputCount = 3;
			r.outputCount = 2;
			r = addRecipe(TechType.JellyPlantSeed, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2, 90);
			r.inputCount = 4;
		}
		
		private static BioRecipe addRecipe(TechType inp, TechType o, int salt = 5, float secs = 45) {
			BioRecipe r = new BioRecipe(salt, secs, inp, o);
			recipes[r.inputItem] = r;
			return r;
		}
		
		public Bioprocessor() : base("bioprocessor", "Bioprocessor", "Decomposes and recombines organic matter into useful raw chemicals.", "6d71afaa-09b6-44d3-ba2d-66644ffe6a99") {
			addIngredient(TechType.TitaniumIngot, 1);
			addIngredient(TechType.Magnetite, 12);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 2);
			addIngredient(TechType.CopperWire, 1);
			addIngredient(TechType.Glass, 3);
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			UnityEngine.Object.Destroy(go.GetComponent<Aquarium>());
		}
		
	}
		
	public class BioprocessorLogic : CustomMachineLogic {
		
		private BioRecipe currentOperation;
		private int saltRequired;
		private float nextSaltTime;
		
		protected override void updateEntity(GameObject go) {
			SBUtil.writeToChat("I am ticking @ "+go.transform.position);
			
			if (currentOperation != null) {
				float time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000F;
				if (time >= nextSaltTime) {
					StorageContainer con = go.GetComponentInChildren<StorageContainer>();
					IList<InventoryItem> salt = con.container.GetItems(TechType.Salt);
					if (salt != null && salt.Count >= 1) {
						con.container.RemoveItem(salt[0].item);
						saltRequired--;
					}
					else {
						setRecipe(null);
					}
					nextSaltTime = time+currentOperation.secondsPerSalt;
					if (saltRequired <= 0) {
						IList<InventoryItem> ing = con.container.GetItems(currentOperation.inputItem);
						if (ing != null && ing.Count >= currentOperation.inputCount) {
							for (int i = 0; i < currentOperation.inputCount; i++)
								con.container.RemoveItem(ing[i].item);
							for (int i = 0; i < currentOperation.outputCount; i++) {
								GameObject item = SBUtil.createWorldObject(CraftData.GetClassIdForTechType(currentOperation.outputItem));
								item.SetActive(false);
								con.container.AddItem(item.GetComponent<Pickupable>());
							}
						}
						else {
							setRecipe(null);
						}
					}
				}
			}
			else {
				foreach (BioRecipe r in Bioprocessor.recipes.Values) {
					if (canRunRecipe(r)) {
						setRecipe(r);
						break;
					}
				}
			}
		}
		
		private bool canRunRecipe(BioRecipe r) {
			StorageContainer con = gameObject.GetComponentInChildren<StorageContainer>();
			IList<InventoryItem> ing = con.container.GetItems(currentOperation.inputItem);
			IList<InventoryItem> salt = con.container.GetItems(TechType.Salt);
			return ing != null && salt != null && salt.Count >= r.saltCount && ing.Count >= r.inputCount;
		}
		
		private void setRecipe(BioRecipe r) {
			currentOperation = r;
			saltRequired = r != null ? r.saltCount : -1;
			nextSaltTime = r != null ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000F+r.secondsPerSalt : -1;
		}
		
	}
	
	class BioRecipe {
			
		internal readonly TechType inputItem;
		internal readonly TechType outputItem;
		internal readonly int saltCount;
		internal readonly float processTime;
		
		internal readonly float secondsPerSalt;
		
		internal int inputCount = 1;
		internal int outputCount = 1;
		
		internal BioRecipe(int s, float t, TechType inp, TechType o) {
			inputItem = inp;
			outputItem = o;
			saltCount = s;
			processTime = t;
			secondsPerSalt = processTime/(float)saltCount;
		}
		
	}
}
