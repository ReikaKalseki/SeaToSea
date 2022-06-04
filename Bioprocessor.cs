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
		
		internal static readonly float POWER_COST_IDLE = 6.0F; //per second; was 1.5 then 2.5
		internal static readonly float POWER_COST_ACTIVE = 32.0F; //per second
		
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
			foreach (Aquarium a in go.GetComponentsInParent<Aquarium>())
				UnityEngine.Object.Destroy(a);
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			con.enabled = true;
			con.height = 6;
			con.width = 6;
			con.hoverText = "Use Bioprocessor";
			con.storageLabel = "BIOPROCESSOR";
			Transform t = go.transform.Find("model/Coral");
		 	if (t != null)
				UnityEngine.Object.Destroy(t.gameObject);/*
			t = go.transform.Find("bioprocessor(Clone)/model/Coral");
		 	if (t != null)
				UnityEngine.Object.Destroy(t.gameObject);
			t = go.transform.Find("bioprocessor/model/Coral");
		 	if (t != null)
				UnityEngine.Object.Destroy(t.gameObject);*/
		}
		
	}
		
	public class BioprocessorLogic : CustomMachineLogic {
		
		private BioRecipe currentOperation;
		private int saltRequired;
		private float nextSaltTimeRemaining;
		
		void Start() {
			SeaToSeaMod.processor.prepareGameObject(gameObject, gameObject.GetComponentInChildren<Renderer>());
		}
		
		protected override void updateEntity(GameObject go, float seconds) {
			//SBUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (seconds <= 0)
				return;
			
			if (consumePower(seconds)) {
				if (currentOperation != null) {
					nextSaltTimeRemaining -= seconds;
					//SBUtil.writeToChat("remaining: "+nextSaltTimeRemaining);
					if (nextSaltTimeRemaining <= 0 && consumePower(seconds*((Bioprocessor.POWER_COST_ACTIVE/Bioprocessor.POWER_COST_IDLE)-1))) {
						StorageContainer con = go.GetComponentInChildren<StorageContainer>();
						IList<InventoryItem> salt = con.container.GetItems(TechType.Salt);
						if (salt != null && salt.Count >= 1) {
							con.container.RemoveItem(salt[0].item);
							saltRequired--;
						}
						else {
							setRecipe(null);
						}
						nextSaltTimeRemaining = currentOperation.secondsPerSalt;
						if (saltRequired <= 0) {
							//SBUtil.writeToChat("try craft");
							IList<InventoryItem> ing = con.container.GetItems(currentOperation.inputItem);
							if (ing != null && ing.Count >= currentOperation.inputCount) {
								//SBUtil.writeToChat("success");
								for (int i = 0; i < currentOperation.inputCount; i++)
									con.container.RemoveItem(ing[0].item); //list is updated in realtime
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
					//SBUtil.writeToChat("Looking for recipe");
					foreach (BioRecipe r in Bioprocessor.recipes.Values) {
						if (canRunRecipe(r)) {
							SBUtil.writeToChat("Found "+r);
							setRecipe(r);
							break;
						}
					}
				}
			}
			else {
				setRecipe(null);
				SBUtil.writeToChat("Insufficient power");
			}
		}
		
		private bool consumePower(float sc = 1) {
			SubRoot sub = gameObject.GetComponentInParent<SubRoot>();
			if (sub == null)
				return false;
			float receive;
			sub.powerRelay.ConsumeEnergy(Bioprocessor.POWER_COST_IDLE*sc, out receive);
			receive += 0.0001F;
			if (receive < Bioprocessor.POWER_COST_IDLE*sc)
				SBUtil.writeToChat("Wanted "+(Bioprocessor.POWER_COST_IDLE*sc)+", got "+receive);
			return receive >= Bioprocessor.POWER_COST_IDLE*sc;//Mathf.Approximately(Bioprocessor.POWER_COST*sc, receive);
		}
		
		private bool canRunRecipe(BioRecipe r) {
			StorageContainer con = gameObject.GetComponentInChildren<StorageContainer>();
			IList<InventoryItem> ing = con.container.GetItems(r.inputItem);
			IList<InventoryItem> salt = con.container.GetItems(TechType.Salt);
			return ing != null && salt != null && salt.Count >= r.saltCount && ing.Count >= r.inputCount;
		}
		
		private void setRecipe(BioRecipe r) {
			currentOperation = r;
			saltRequired = r != null ? r.saltCount : -1;
			nextSaltTimeRemaining = r != null ? r.secondsPerSalt : -1;
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
		
		public override string ToString()
		{
			return string.Format("[BioRecipe InputItem={0}, OutputItem={1}, SaltCount={2}, ProcessTime={3}, SecondsPerSalt={4}, InputCount={5}, OutputCount={6}]", inputItem, outputItem, saltCount, processTime, secondsPerSalt, inputCount, outputCount);
		}

		
	}
}
