using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml;

using UnityEngine;

using SMLHelper.V2.Handlers;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public static class CraftingItems {
		
		private static readonly Dictionary<Items, BasicCraftingItem> mappings = new Dictionary<Items, BasicCraftingItem>();
		private static readonly Dictionary<TechType, BasicCraftingItem> techs = new Dictionary<TechType, BasicCraftingItem>();
		
		static CraftingItems() {
			foreach (Items m in Enum.GetValues(typeof(Items))) {
				string id = Enum.GetName(typeof(Items), m);
				SNUtil.log("Constructing crafting item "+id);
				Item attr = getAttr(m);
				XMLLocale.LocaleEntry e = SeaToSeaMod.itemLocale.getEntry(id);
				BasicCraftingItem item = (BasicCraftingItem)Activator.CreateInstance(attr.itemClass, new object[]{id, e.name, e.desc, attr.template});
				mappings[m] = item;
				item.craftingSubCategory = attr.category == TechCategory.VehicleUpgrades ? "C2Chemistry" : ""+attr.category;
				item.unlockRequirement = attr.dependency;/*
				if (m == Items.Sealant || m == Items.SealFabric) {
					item.unlockRequirement = SeaToSeaMod.alkali.TechType;
				}
				if (m == Items.CrystalLens || m == Items.DenseAzurite) {
					item.unlockRequirement = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType;
				}*/
				item.glowIntensity = 0;
				switch(m) {
					case Items.Luminol:
						//item.glowIntensity = 2;
						item.renderModify = r => {RenderUtil.setPolyanilineColor(r, new Color(0.2F, 0.6F, 1F, 1)); RenderUtil.setEmissivity(r.materials[1], 2, "GlowStrength");};
					break;
					case Items.TreaderEnzymes:
						item.renderModify = r => RenderUtil.setPolyanilineColor(r, new Color(107/255F, 80/255F, 62/255F, 1));
					break;
					case Items.BioEnzymes:
						item.renderModify = r => RenderUtil.setPolyanilineColor(r, new Color(1F, 170/255F, 0F, 1));
					break;
					case Items.KelpEnzymes:
						item.renderModify = r => RenderUtil.setPolyanilineColor(r, new Color(0.5F, 0.0F, 1F, 1));
					break;
					case Items.Chlorine:
						item.renderModify = r => RenderUtil.setPolyanilineColor(r, new Color(179/255F, 211/255F, 97/255F, 1));
					break;
					case Items.Sealant:
						item.renderModify = r => RenderUtil.setPolyanilineColor(r, new Color(0.5F, 0.6F, 1, 1));
					break;
					case Items.HoneycombComposite:
					item.renderModify = r => r.materials[0].EnableKeyword("MARMO_SPECMAP");
					break;
					case Items.DenseAzurite:
						item.glowIntensity = 2;
					break;
					case Items.CrystalLens:
						item.inventorySize = new Vector2int(2, 2);
						item.renderModify = r => r.gameObject.EnsureComponent<CrystalLensAnimator>();
					break;
					case Items.LathingDrone:
						item.inventorySize = new Vector2int(2, 2);
						item.renderModify = r => {
							//GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("6ca93e93-5209-4c27-ba60-5f68f36a95fb"), "Starship_control_terminal_01"));
						};
					break;
					case Items.HullPlating:
					case Items.SmartPolymer:
						item.inventorySize = new Vector2int(2, 1);
					break;
					case Items.FuelTankWall:
						item.renderModify = r => {r.materials[0].SetFloat("_SpecInt", 0.5F); r.materials[0].SetFloat("_Shininess", 0F);};
					break;
					case Items.RocketFuel:
						item.inventorySize = new Vector2int(1, 2);
						item.renderModify = r => {r.gameObject.EnsureComponent<RocketFuelAnimator>(); RenderUtil.setEmissivity(r.materials[3], 2, "GlowStrength");};
						//item.glowIntensity = 2;
					break;
					case Items.BacterialSample:
						item.renderModify = r => {
							r.gameObject.EnsureComponent<BacteriaAnimator>(); MushroomTreeBacterialColony.setupWave(r, 2);
							r.materials[0].SetFloat("_Fresnel", 0.6F);
							r.materials[0].SetFloat("_Shininess", 15F);
							r.materials[0].SetFloat("_SpecInt", 18F);
							r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
						};
						item.glowIntensity = 1;
					break;
				}
				if (item.sprite == null)
					item.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/"+id);
			}
		}
		
		public static void addAll() {
			foreach (BasicCraftingItem item in mappings.Values) {
				item.Patch();
				techs[item.TechType] = item;
				SNUtil.log("Registered > "+item+" > {"+RecipeUtil.toString(CraftDataHandler.GetTechData(item.TechType))+"} @ "+string.Join("-", item.StepsToFabricatorTab)+" in "+item.GroupForPDA+"/"+item.CategoryForPDA);
			}
		}	
		
		public enum Items {
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.AramidFibers,		"WorldEntities/Natural/aerogel")]HoneycombComposite,
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.Unobtanium,		"WorldEntities/Natural/Glass")]DenseAzurite,
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.Unobtanium,		"WorldEntities/Natural/EnameledGlass")]CrystalLens,
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.Unobtanium,		"WorldEntities/Natural/Magnesium")]HullPlating, //was wiring kit
			[Item(typeof(Bioprocessed), 		TechCategory.VehicleUpgrades, 	TechType.Unobtanium,		"WorldEntities/Natural/polyaniline")]Sealant,
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.Unobtanium,		"WorldEntities/Natural/aramidfibers")]SealFabric,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.GasPod,			"WorldEntities/Natural/polyaniline")]Chlorine,
			[Item(typeof(Bioprocessed),			TechCategory.VehicleUpgrades, 	TechType.SnakeMushroomSpore,"WorldEntities/Natural/polyaniline")]Luminol,
			[Item(typeof(Bioprocessed),			TechCategory.AdvancedMaterials, TechType.HatchingEnzymes,	"WorldEntities/Natural/FiberMesh")]SmartPolymer,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.AcidMushroom,		"WorldEntities/Natural/hydrochloricacid")]WeakAcid,
			[Item(typeof(BasicCraftingItem),	TechCategory.Electronics, 		TechType.Lubricant,			"WorldEntities/Natural/Lubricant")]Motor,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.SeaTreaderPoop,	"WorldEntities/Natural/polyaniline")]TreaderEnzymes,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.Kyanite,			"WorldEntities/Natural/CrashPowder")]BacterialSample,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.Unobtanium,		"WorldEntities/Natural/polyaniline")]BioEnzymes,
			[Item(typeof(BasicCraftingItem),	TechCategory.Electronics, 		TechType.Unobtanium,		"WorldEntities/Natural/WiringKit")]LathingDrone,
			[Item(typeof(Bioprocessed),			TechCategory.VehicleUpgrades, 	TechType.Unobtanium,		"WorldEntities/Natural/polyaniline")]KelpEnzymes,
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.HatchingEnzymes,	"WorldEntities/Natural/Silicone")]FuelTankWall,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.Kyanite,			"WorldEntities/Natural/benzene")]RocketFuel,
		}
		
		private static Item getAttr(Items key) {
			FieldInfo info = typeof(Items).GetField(Enum.GetName(typeof(Items), key));
			return (Item)Attribute.GetCustomAttribute(info, typeof(Item));
		}
		
		public static BasicCraftingItem getItem(Items key) {
			return mappings[key];
		}
		
		public static BasicCraftingItem getItemByTech(TechType tt) {
			return techs.ContainsKey(tt) ? techs[tt] : null;
		}
		
		public class Item : Attribute {
			
			public readonly TechCategory category;
			public readonly TechType dependency;			
			internal readonly Type itemClass;
			public readonly string template;
			
			public Item(Type item, TechCategory cat, TechType dep, string temp) {
				itemClass = item;
				dependency = dep;
				category = cat;
				template = temp;
			}
		}
	}
	
	class BacteriaAnimator : MonoBehaviour {
		
		private Renderer render;
		
		private void Update() {
			if (!render) {
				render = gameObject.GetComponent<Renderer>();
			}
			MushroomTreeBacterialColony.updateColors(this, render, DayNightCycle.main.timePassedAsFloat);
		}
		
	}
	
	class CrystalLensAnimator : MonoBehaviour {
		
		private Renderer render;
		
		private void Update() {
			if (!render) {
				render = gameObject.GetComponent<Renderer>();
			}
			
		}
		
	}
	
	class RocketFuelAnimator : MonoBehaviour {
		
		private Renderer render;
		
		private void Update() {
			if (!render) {
				render = gameObject.GetComponent<Renderer>();
			}
			float f = Mathf.Sin(DayNightCycle.main.timePassedAsFloat*0.17F+gameObject.GetInstanceID()*0.6943F);
			RenderUtil.setEmissivity(render.materials[3], 2.5F-f, "GlowStrength");
			render.materials[0].SetColor("_GlowColor", new Color(0, f, 1, 1));
		}
		
	}
}
