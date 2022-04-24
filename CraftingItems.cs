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
				BasicCraftingItem item = (BasicCraftingItem)Activator.CreateInstance(attr.itemClass, new object[]{id, e.name, e.desc});
				mappings[m] = item;
				item.isAdvanced = attr.isAdvanced;
				item.unlockRequirement = attr.dependency;
				item.sprite = TextureManager.getSprite("Textures/Items/"+m);
				item.Patch();	
			}
		}
		
		public static void addAll() {
			foreach (BasicCraftingItem item in mappings.Values) {
				item.Patch();
				SBUtil.log("Registered > "+item);
			}
		}
		
		public enum Items {
			[Item(typeof(BasicCraftingItem), true, TechType.AramidFibers)]HoneycombComposite,
			[Item(typeof(BasicCraftingItem), true, TechType.Diamond)]CrystalLens,
			[Item(typeof(BasicCraftingItem), true, TechType.PlasteelIngot)]HullPlating,
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
			
			public Item(Type item, bool adv, TechType dep) {
				itemClass = item;
				dependency = dep;
				isAdvanced = adv;
			}
		}
	}
}
