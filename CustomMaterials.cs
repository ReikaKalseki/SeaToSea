using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public static class CustomMaterials {
		
		private static readonly string[] texTypes = new string[]{"_MainTex", "_SpecTex", "_BumpMap", "_Illum"};
		
		private static readonly HashSet<Materials> alreadyRegisteredGen = new HashSet<Materials>();
		
		private static readonly Dictionary<Materials, Material> mappings = new Dictionary<Materials, Material>();
		
		static CustomMaterials() {
			foreach (Materials m in Enum.GetValues(typeof(Materials))) {
				getMaterial(m); //trigger registration
			}
		}
		
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
				SBUtil.log("Registering material "+key+": "+mappings[key]);
				mappings[key].entity.Patch();
				SBUtil.log(" > "+mappings[key].entity.TechType);
				mappings[key].enumIndex = key;
				PDAScanner.EntryData e = new PDAScanner.EntryData();
				e.key = mappings[key].getTechType();
				e.scanTime = 2;
				e.encyclopedia = "blaa";
				e.locked = true;
				PDAHandler.AddCustomScannerEntry(e);
			}
			return mappings[key];
		}
		
		private static Material lookupEntry(Materials key) {
			FieldInfo info = typeof(Materials).GetField(Enum.GetName(typeof(Materials), key));
			SBUtil.log("Fetching material for "+key+" > "+info+" > "+(info != null ? info.Name : "null"+" > "+(info != null ? info.GetValue(null) : "null")));
			return (Material)Attribute.GetCustomAttribute(info, typeof(Material));
		}
		
		public enum Materials {
			[Material("Azurite", "A gemstone with exquisite optical properties and electromagnetic conductivity.", "URANIUM")]MOUNTAIN_CRYSTAL,
			[Material("Platinum", "An ultra-rare metal with extreme resistance to corrosion. Also useful as a chemical catalyst.", "SALT")]PLATINUM,
			[Material("Hadeoclase", "Small, dense crystals formed under extreme pressure, with trace amounts of exotic elements such as Tellurium.\nPotentially able to be alloyed into other materials for increased compressive strength.", "LARGE_RUBY")]PRESSURE_CRYSTALS,
			[Material("Avolite", "An integral part of a phase gate, allowing for the high-level manipulation of space and time. These are not fully understood.", "KYANITE")]PHASE_CRYSTAL,
			
		}
		
		public sealed class MaterialEntity : Spawnable {
			
			public readonly Material material;
			
			internal MaterialEntity(Material m) : base(Enum.GetName(typeof(Materials), m.enumIndex), m.displayName, m.desc) {
				material = m;
				
				//OnFinishedPatching += () => {CraftData.pickupSoundList.Add(TechType, "event:/loot/pickup_glass");};
			}
			
			public override GameObject GetGameObject() {
				return material.getObject();
			}
			
		}
		
		public class Material : Attribute {
			
			//public readonly string prefab;
			//public readonly string prefabLarge;
			public readonly string displayName;
			public readonly string desc;
			public readonly VanillaResources baseTemplate;
			public readonly MaterialEntity entity;
			
			internal Materials enumIndex;
			
			public Material(/*string p, string l, */string n, string d, string template) {
				//prefab = p;
				//prefabLarge = l;
				displayName = n;
				desc = d;
				baseTemplate = (VanillaResources)typeof(VanillaResources).GetField(template).GetValue(null);
				entity = new MaterialEntity(this);
			}
			
			public TechType getTechType() {
				return entity.TechType;
			}
			
			public GameObject getObject() {
				GameObject prefab;
				if (UWE.PrefabDatabase.TryGetPrefab(baseTemplate.prefab, out prefab)) {
					GameObject world = UnityEngine.Object.Instantiate(prefab);
					world.SetActive(false);
					TechType tt = getTechType();
					world.EnsureComponent<TechTag>().type = tt;
					world.EnsureComponent<PrefabIdentifier>().ClassId = entity.ClassID;
					Renderer r = world.GetComponentInChildren<Renderer>();
					applyMaterialChanges(r);
					//SBUtil.writeToChat("Applying custom texes to "+world+" @ "+world.transform.position);
					return world;
				}
				else {
					SBUtil.writeToChat("Could not fetch template GO for "+this.enumIndex);
					return null;
				}
			}
			
			private void applyMaterialChanges(Renderer r) {
				if (true) {
					//SBUtil.log("render for "+this.enumIndex);
					//SBUtil.dumpObjectData(r);
					
					string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					foreach (String type in texTypes) {
						string path = Path.Combine(folder, "Textures/Resources/"+formatFileName()+type+".png");
						Texture2D newTex = ImageUtils.LoadTextureFromFile(path);
						if (newTex != null) {
							r.materials[0].SetTexture(type, newTex);
							//SBUtil.writeToChat("Found "+type+" texture @ "+path);
						}
						else {
							//SBUtil.writeToChat("No texture found at "+path);
						}
					}
				}
				switch(enumIndex) {
					case Materials.MOUNTAIN_CRYSTAL:
						break;
					case Materials.PLATINUM:
						break;
				}
			}
			
			private string formatFileName() {
				string n = this.enumIndex+"";
				System.Text.StringBuilder ret = new System.Text.StringBuilder();
				for (int i = 0; i < n.Length; i++) {
					char c = n[i];
					if (c == '_')
						continue;
					bool caps = i == 0 || n[i-1] == '_';
					if (caps) {
						c = Char.ToUpperInvariant(c);
					}
					else {
						c = Char.ToLowerInvariant(c);
					}
					ret.Append(c);
				}
				return ret.ToString();
			}
		}
	}
}
