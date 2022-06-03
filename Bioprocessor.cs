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
		
		internal static readonly Arrow leftArrow = new Arrow("arrowL", "", "", "");
		internal static readonly Arrow rightArrow = new Arrow("arrowR", "", "", "");
		
		internal static readonly float POWER_COST = 1.5F;
		
		static Bioprocessor() {
			leftArrow.Patch();
			rightArrow.Patch();
		}
		
		public static void addRecipes() {
			addRecipe(TechType.WhiteMushroom, TechType.HydrochloricAcid, 6, 20, 6);
			addRecipe(TechType.BloodOil, TechType.Benzene, 5, 45, 4);
			addRecipe(SeaToSeaMod.alkali.TechType, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 5, 30, 5);
			addRecipe(TechType.GasPod, CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType, 1, 15, 3, 2);
			addRecipe(TechType.SnakeMushroomSpore, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2, 90, 2);
		}
		
		private static void addRecipe(TechType inp, TechType o, int salt = 5, float secs = 45, int inamt = 1, int outamt = 1) {
			BioRecipe r = new BioRecipe(salt, secs, inp, o);
			recipes[r.inputItem] = r;
			RecipeUtil.addRecipe(o);
			RecipeUtil.addIngredient(o, SeaToSeaMod.processor.TechType, 1);
			RecipeUtil.addIngredient(o, leftArrow.TechType, 1);
			RecipeUtil.addIngredient(o, inp, inamt);
			RecipeUtil.addIngredient(o, TechType.Salt, salt);
			r.inputCount = inamt;
			r.outputCount = outamt;
		}
		
		public Bioprocessor() : base("bioprocessor", "Bioprocessor", "Decomposes and recombines organic matter into useful raw chemicals.", "6d71afaa-09b6-44d3-ba2d-66644ffe6a99") {
			addIngredient(TechType.TitaniumIngot, 1);
			addIngredient(TechType.Magnetite, 12);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 6);
			addIngredient(TechType.CopperWire, 1);
			addIngredient(TechType.Glass, 3);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		//protected OrientedBounds[] GetBounds { get; }
		
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
			
			if (consumePower()) {
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
			else {
				setRecipe(null);
			}
		}
		
		private bool consumePower() {
			SubRoot sub = gameObject.GetComponentInParent<SubRoot>();
			float receive;
			sub.powerRelay.ConsumeEnergy(Bioprocessor.POWER_COST, out receive);
			return Mathf.Approximately(Bioprocessor.POWER_COST, receive);
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
