using System;
using System.Reflection;
using System.Collections.Generic;

using SMLHelper.V2.Handlers;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public static class CraftingItems {
		
		private static readonly Dictionary<Items, Item> mappings = new Dictionary<Items, Item>();
		
		public enum Items {
			[Item("Honeycomb Composite Plating", "A lightweight and low-conductivity panel.", true, TechType.AramidFibers)]HoneycombComposite,
			[Item("Refractive Lens", "A lens with the ability to refract several kinds of matter.", true, TechType.Diamond)]CrystalLens,
			[Item("Platinum", "An ultra-rare metal with extreme resistance to corrosion. Also useful as a chemical catalyst.", false, TechType.Diamond)]Platinum,
			[Item("Pressure Crystals", "??", false, TechType.Nickel)]PressureCrystals,
			[Item("Phase Crystal", "An integral part of a phase gate, allowing for the high-level manipulation of space and time.", false, TechType.Nickel)]PhaseCrystal,
			[Item("Hull Plating", "Heavy armor for vehicles.", true, TechType.PlasteelIngot)]HullPlating,
		}
		
		public static Item getItemEntry(Items key) {
			if (!mappings.ContainsKey(key)) {
				mappings[key] = lookupEntry(key);
				mappings[key].map(key);
			}
			return mappings[key];
		}
		
		private static Item lookupEntry(Items key) {
			MemberInfo info = typeof(Items).GetField(Enum.GetName(typeof(Items), key));
			return (Item)Attribute.GetCustomAttribute(info, typeof(Item));
		}
		
		public class Item : Attribute, ItemDef<BasicCraftingItem> {
			
			public readonly string displayName;
			public readonly string desc;
			
			public readonly bool isAdvanced;
			public readonly TechType dependency;
			
			private BasicCraftingItem item;
			private Items enumIndex;
			
			public Item(string n, string d) : this(n, d, false) {
				
			}
			
			public Item(string n, string d, bool adv) : this(n, d, adv, TechType.None) {
				
			}
			
			public Item(string n, string d, bool adv, TechType dep) {
				displayName = n;
				desc = d;
				dependency = dep;
				isAdvanced = adv;
			}
			
			internal void map(Items i) {
				enumIndex = i;
				item = new BasicCraftingItem(Enum.GetName(typeof(Items), i), displayName, desc);
				item.isAdvanced = isAdvanced;
				item.unlockRequirement = dependency;
			}
		
			public string getDisplayName() {
				return displayName;
			}
			
			public string getDesc() {
				return desc;
			}
			
			public TechType getTechType() {/*
				TechType ret;
				if (TechTypeHandler.TryGetModdedTechType(enumIndex, out ret)) {
					return ret;
				}
				else {
					throw new Exception("Item "+enumIndex+" has no tech type!");
				}*/
				return item.TechType;
			}
			
			public BasicCraftingItem getItem() {
				return item;
			}
			
			public void register() {
				item.Patch();
			}
			
			public ItemDef<BasicCraftingItem> addIngredient(TechType item, int amt) {
				this.item.addIngredient(item, amt);
				return this;
			}
			
			public Item addIngredient(Items item, int amt) {
				return (Item)addIngredient(getItemEntry(item).getTechType(), amt);	
			}
		}
	}
}
