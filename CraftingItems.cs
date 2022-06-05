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
		
		static CraftingItems() {
			foreach (Items m in Enum.GetValues(typeof(Items))) {
				string id = Enum.GetName(typeof(Items), m);
				SBUtil.log("Constructing crafting item "+id);
				Item attr = getAttr(m);
				XMLLocale.LocaleEntry e = SeaToSeaMod.locale.getEntry(id);
				BasicCraftingItem item = (BasicCraftingItem)Activator.CreateInstance(attr.itemClass, new object[]{id, e.name, e.desc, attr.template});
				mappings[m] = item;
				item.isAdvanced = attr.isAdvanced;
				item.unlockRequirement = attr.dependency;
				if (m == Items.Sealant || m == Items.SealFabric) {
					item.unlockRequirement = SeaToSeaMod.alkali.TechType;
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
				SBUtil.log("Registered > "+item);
			}
		}
		
		public enum Items {
			[Item(typeof(BasicCraftingItem),	true, TechType.AramidFibers,	"WorldEntities/Natural/aerogel")]HoneycombComposite,
			[Item(typeof(BasicCraftingItem),	true, TechType.Diamond,			"WorldEntities/Natural/Glass")]DenseAzurite,
			[Item(typeof(BasicCraftingItem),	true, TechType.Diamond,			"WorldEntities/Natural/EnameledGlass")]CrystalLens,
			[Item(typeof(BasicCraftingItem),	true, TechType.PlasteelIngot,	"WorldEntities/Natural/WiringKit")]HullPlating, //was Magnesium
			[Item(typeof(Bioprocessed), 		true, TechType.None,			"WorldEntities/Natural/Lubricant")]Sealant,
			[Item(typeof(BasicCraftingItem),	true, TechType.None,			"WorldEntities/Natural/aramidfibers")]SealFabric,
			[Item(typeof(Bioprocessed),			true, TechType.GasPod,			"WorldEntities/Natural/polyaniline")]Chlorine,
			[Item(typeof(Bioprocessed),			true, TechType.SnakeMushroom,	"WorldEntities/Natural/polyaniline")]Luminol,
			[Item(typeof(Bioprocessed),			true, TechType.HatchingEnzymes,	"WorldEntities/Natural/aramidfibers")]SmartPolymer,
		}
		
		private static Item getAttr(Items key) {
			FieldInfo info = typeof(Items).GetField(Enum.GetName(typeof(Items), key));
			return (Item)Attribute.GetCustomAttribute(info, typeof(Item));
		}
		
		public static BasicCraftingItem getItem(Items key) {
			return mappings[key];
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
