using System;
using System.Reflection;
using System.Collections.Generic;

using SMLHelper.V2.Handlers;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public static class CustomMaterials {
		
		private static readonly HashSet<Materials> alreadyRegisteredGen = new HashSet<Materials>();
		
		private static readonly Dictionary<Materials, Material> mappings = new Dictionary<Materials, Material>();
		
		public static void registerWorldgen(Materials m, BiomeType biome, int amt, float chance) {
			SBUtil.log("Adding worldgen "+biome+" x"+amt+" @ "+chance+"% to "+m);
			string id = CraftData.GetClassIdForTechType(getMaterial(m).getTechType());
			if (alreadyRegisteredGen.Contains(m)) {
		        LootDistributionHandler.EditLootDistributionData(id, biome, chance, amt); //will add if not present
			}
			else {				
		        string path;
		        UWE.PrefabDatabase.TryGetPrefabFilename(id, out path);
				LootDistributionData.BiomeData b = new LootDistributionData.BiomeData{biome = biome, count = amt, probability = chance};
		        List<LootDistributionData.BiomeData> li = new List<LootDistributionData.BiomeData>{b};
		        UWE.WorldEntityInfo info = null;//new UWE.WorldEntityInfo();
		        UWE.WorldEntityDatabase.TryGetInfo(id, out info);
		       	WorldEntityDatabaseHandler.AddCustomInfo(id, info);
		        LootDistributionHandler.AddLootDistributionData(id, path, li, info);
		        
				alreadyRegisteredGen.Add(m);
			}
		}
		
		public static Material getMaterial(Materials key) {
			if (!mappings.ContainsKey(key)) {
				mappings[key] = lookupEntry(key);
				mappings[key].enumIndex = key;
			}
			return mappings[key];
		}
		
		private static Material lookupEntry(Materials key) {
			MemberInfo info = typeof(Materials).GetField(Enum.GetName(typeof(Materials), key));
			return (Material)Attribute.GetCustomAttribute(info, typeof(Material));
		}
		
		public enum Materials {
			[Material("", "")]MOUNTAIN_CRYSTAL,
			[Material("", "")]TREADER_CAVE_ORE,
			
		}
		
		public class Material : Attribute {
			
			//public readonly string prefab;
			//public readonly string prefabLarge;
			public readonly string displayName;
			public readonly string desc;
			
			internal Materials enumIndex;
			
			public Material(/*string p, string l, */string n, string d) {
				//prefab = p;
				//prefabLarge = l;
				displayName = n;
				desc = d;
			}
			
			public TechType getTechType() {
				TechType ret;
				if (TechTypeHandler.TryGetModdedTechType(Enum.GetName(typeof(Materials), enumIndex), out ret)) {
					return ret;
				}
				else {
					throw new Exception("Material "+enumIndex+" has no tech type!");
				}
			}
		}
	}
}
