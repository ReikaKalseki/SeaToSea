using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml;

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
				item.isAdvanced = attr.isAdvanced;
				item.unlockRequirement = attr.dependency;
				if (m == Items.Sealant || m == Items.SealFabric) {
					item.unlockRequirement = SeaToSeaMod.alkali.TechType;
				}
				if (m == Items.CrystalLens) {
					item.unlockRequirement = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType;
				}
				if (m == Items.Luminol) {
					item.glowIntensity = 2;
				}
				item.sprite = TextureManager.getSprite("Textures/Items/"+id);
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
			[Item(typeof(BasicCraftingItem),	true, TechType.AramidFibers,	"WorldEntities/Natural/aerogel")]HoneycombComposite,
			[Item(typeof(BasicCraftingItem),	true, TechType.Kyanite,			"WorldEntities/Natural/Glass")]DenseAzurite,
			[Item(typeof(BasicCraftingItem),	true, TechType.Diamond,			"WorldEntities/Natural/EnameledGlass")]CrystalLens,
			[Item(typeof(BasicCraftingItem),	true, TechType.Kyanite,			"WorldEntities/Natural/WiringKit")]HullPlating, //was Magnesium
			[Item(typeof(Bioprocessed), 		false, TechType.None,			"WorldEntities/Natural/Lubricant")]Sealant,
			[Item(typeof(BasicCraftingItem),	true, TechType.None,			"WorldEntities/Natural/aramidfibers")]SealFabric,
			[Item(typeof(Bioprocessed),			false, TechType.GasPod,			"WorldEntities/Natural/polyaniline")]Chlorine,
			[Item(typeof(Bioprocessed),			false, TechType.SnakeMushroomSpore,	"WorldEntities/Natural/polyaniline")]Luminol,
			[Item(typeof(Bioprocessed),			true, TechType.HatchingEnzymes,	"WorldEntities/Natural/aramidfibers")]SmartPolymer,
			[Item(typeof(BasicCraftingItem),	false, TechType.AcidMushroom,	"WorldEntities/Natural/hydrochloricacid")]WeakAcid,
			[Item(typeof(BasicCraftingItem),	false, TechType.Lubricant,		"WorldEntities/Natural/Lubricant")]Motor,
			[Item(typeof(BasicCraftingItem),	false, TechType.Quartz,			"WorldEntities/Natural/Glass")]BaseGlass,
			[Item(typeof(BasicCraftingItem),	true, TechType.SeaCrownSeed,	"WorldEntities/Natural/polyaniline")]BioEnzymes,/*
			[Item(typeof(BasicCraftingItem),	false, TechType.ScrapMetal,		"WorldEntities/Natural/TitaniumIngot")]TitaniumIngotFromScrap,
			[Item(typeof(BasicCraftingItem),	false, TechType.Titanium,		"WorldEntities/Natural/Titanium")]TitaniumFromIngot,*/
			//[Item(typeof(BasicCraftingItem),	true, 	TechType.Kyanite,		"WorldEntities/Natural/polyaniline")]RebreatherFluid,
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
			
			public readonly bool isAdvanced;
			public readonly TechType dependency;			
			internal readonly Type itemClass;
			public readonly string template;
			
			public Item(Type item, bool adv, TechType dep, string temp) {
				itemClass = item;
				dependency = dep;
				isAdvanced = adv;
				template = temp;
			}
		}
	}
}
