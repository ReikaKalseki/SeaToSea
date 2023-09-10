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
		
		public static readonly string LATHING_DRONE_RENDER_OBJ_NAME = "DroneModel";
		
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
						item.renderModify = r => RenderUtil.setPolyanilineColor(r, new Color(0.2F, 0.6F, 1F, 1), 2);
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
						item.renderModify = r => r.transform.localScale = Vector3.one*2;
					break;
					case Items.CrystalLens:
						item.inventorySize = new Vector2int(2, 2);
						item.renderModify = r => {
							GameObject mdl = RenderUtil.setModel(r, ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("59381275-1f6e-4bb9-8b00-7bbe77f0df1c"), "Coral_reef_shell_01"));
							r = mdl.GetComponentInChildren<Renderer>();
							RenderUtil.swapToModdedTextures(r, item);
							RenderUtil.makeTransparent(r);
							r.gameObject.EnsureComponent<CrystalLensAnimator>();
							r.transform.localScale = Vector3.one*0.5F;
							
							GameObject root = mdl.FindAncestor<PrefabIdentifier>().gameObject;
							ObjectUtil.removeComponent<Collider>(root);
							SphereCollider sc = root.EnsureComponent<SphereCollider>();
							sc.radius = 0.25F;
							sc.center = Vector3.zero;
							
							VFXFabricating fab = root.GetComponentInChildren<VFXFabricating>();
							fab.posOffset = new Vector3(0, 0.3F, 0.1F);
							fab.localMinY = -0.4F;
							
							//r.materials[0].SetFloat("_SpecInt", 7.5F);
							//r.materials[0].SetFloat("_Shininess", 12F);
							//r.materials[0].SetFloat("_Fresnel", 0.5F);
						};
					break;
					case Items.LathingDrone:
						item.inventorySize = new Vector2int(2, 2);
						item.glowIntensity = 1.5F;
						item.renderModify = r => {
							GameObject vehicleBayPrefab = ObjectUtil.lookupPrefab("dd0298c1-49c2-44a0-8b32-da98e12228fb");
							GameObject droneObj = vehicleBayPrefab.GetComponent<Constructor>().buildBotPrefab;
							GameObject mdl = RenderUtil.setModel(r, ObjectUtil.getChildObject(droneObj, "model/constructor_drone"));
							r = mdl.GetComponentInChildren<Renderer>();
							mdl.name = LATHING_DRONE_RENDER_OBJ_NAME;
							r.transform.localRotation = Quaternion.identity;
							r.transform.parent.localRotation = Quaternion.identity;
							r.transform.parent.localPosition = Vector3.zero;
							//r.gameObject.transform.localScale
							//RenderUtil.swapToModdedTextures(r, item);
							
							GameObject root = mdl.FindAncestor<PrefabIdentifier>().gameObject;
							ObjectUtil.removeComponent<Collider>(root);
							SphereCollider sc = root.EnsureComponent<SphereCollider>();
							sc.radius = 0.25F;
							sc.center = Vector3.zero;
							
							VFXFabricating fab = root.GetComponentInChildren<VFXFabricating>();
							fab.eulerOffset = new Vector3(180, 0, 0);
							//fab.posOffset = new Vector3(0, 0.5F, 0);
							
							//r.materials[0].color = Color.clear;
							//r.materials[3].color = Color.clear;
							//RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/"+item.getTextureFolder()+"/"+ObjectUtil.formatFileName(item), new Dictionary<int, string>{{1, ""}});
						};
					break;
					case Items.SmartPolymer:
						item.inventorySize = new Vector2int(2, 1);
						item.renderModify = r => {r.transform.localScale = Vector3.one*2F;};
					break;
					case Items.HullPlating:
						item.inventorySize = new Vector2int(2, 1);
						item.renderModify = r => {r.materials[0].color = Color.white; r.materials[0].SetFloat("_SpecInt", 1.5F); r.materials[0].SetFloat("_Shininess", 0F);};
					break;
					case Items.Motor:
						item.renderModify = r => {
							r.transform.localScale = new Vector3(1.5F, 0.85F, 1.5F);
							r.materials[0].SetColor("_Color", Color.white);
							r.materials[0].SetFloat("_SpecInt", 7F);
							r.materials[0].SetFloat("_Shininess", 4F);
						};
					break;
					case Items.FuelTankWall:
						item.renderModify = r => {r.materials[0].SetFloat("_SpecInt", 0.5F); r.materials[0].SetFloat("_Shininess", 0F);};
					break;
					case Items.RocketFuel:
						item.inventorySize = new Vector2int(1, 2);
						item.renderModify = r => {
							RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/"+item.getTextureFolder()+"/"+ObjectUtil.formatFileName(item), new Dictionary<int, string>{{1, ""}, {3, ""}});
							r.gameObject.EnsureComponent<RocketFuelAnimator>();
							RenderUtil.setEmissivity(r.materials[3], 2);
							r.materials[3].EnableKeyword("FX_BUILDING");
							r.materials[3].SetFloat("_Built", 0.1F);
							r.materials[3].SetFloat("_BuildLinear", 0.55F);
							r.materials[3].SetFloat("_NoiseThickness", 0.45F);
							r.materials[3].SetFloat("_NoiseStr", 1F);
							//r.materials[0].SetColor("_BorderColor", new Color(20, 2F, 1, 1));
							r.materials[3].SetColor("_BorderColor", new Color(0.5F, 1, 1, 1));
							r.materials[3].SetVector("_BuildParams", new Vector4(0, 1, 15F, 0.3F));
							r.materials[3].SetInt("_Cutoff", 0);
						};
						//item.glowIntensity = 2;
					break;
					case Items.BacterialSample:
						item.renderModify = r => {
							r.gameObject.EnsureComponent<BacteriaAnimator>();
							MushroomTreeBacterialColony.setupWave(r, 2);
							r.materials[0].SetFloat("_Fresnel", 0.6F);
							r.materials[0].SetFloat("_Shininess", 20F);
							r.materials[0].SetFloat("_SpecInt", 18F);
							r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
						};
						item.glowIntensity = 1;
					break;
					case Items.TraceMetals:
						item.renderModify = r => {
							r.materials[0].SetFloat("_Fresnel", 2F);
							r.materials[0].SetFloat("_Shininess", 20F);
							r.materials[0].SetFloat("_SpecInt", 90F);
							r.transform.localScale = new Vector3(1, 1, 2F);
						};
						item.glowIntensity = 1;
					break;
					case Items.BrokenT2Battery:
						item.renderModify = r => {
							GameObject root = r.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
							r.transform.localScale = new Vector3(3F, 2.4F, 2.4F);
							ObjectUtil.removeComponent<Battery>(root);
							root.gameObject.EnsureComponent<BrokenAzuriteBatterySparker>();
							root.GetComponent<Pickupable>().destroyOnDeath = false;
							RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/"+item.getTextureFolder()+"/"+ObjectUtil.formatFileName(C2CItems.t2Battery));
						};
						item.glowIntensity = 1;
					break;
					case Items.AmoeboidSample:
						item.inventorySize = new Vector2int(1, 1);
						item.renderModify = r => {
							GameObject root = r.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
							ObjectUtil.removeComponent<Eatable>(root);
							ObjectUtil.removeComponent<Plantable>(root);
							
							RenderUtil.swapToModdedTextures(r, item);
							RenderUtil.makeTransparent(r);
							r.transform.localScale = Vector3.one*1.5F;
							r.materials[0].SetFloat("_Fresnel", 0.55F);
							r.materials[0].SetFloat("_Shininess", 20F);
							r.materials[0].SetFloat("_SpecInt", 12F);
							
							r.material.EnableKeyword("UWE_WAVING");
							r.material.SetColor("_Color", Color.white);
							r.material.SetColor("_SpecColor", Color.white);
							r.material.SetVector("_Scale", new Vector4(0.03F, 0.03F, 0.03F, 0.03F));
							r.material.SetVector("_Frequency", new Vector4(12.0F, 12.0F, 12.0F, 12.0F));
							r.material.SetVector("_Speed", new Vector4(0.08F, 0.08F, 0.0F, 0.0F));
							r.material.SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
							r.material.SetFloat("_WaveUpMin", 10F);
						};
						//item.glowIntensity = 2;
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
			[Item(typeof(NotFabricable),		TechCategory.VehicleUpgrades, 	TechType.Unobtanium,		"WorldEntities/Natural/hydrochloricacid")]SulfurAcid,
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.Unobtanium,		"WorldEntities/Natural/EnameledGlass")]CrystalLens,
			[Item(typeof(BasicCraftingItem),	TechCategory.BasicMaterials, 	TechType.JeweledDiskPiece,	"WorldEntities/Natural/CrashPowder")]TraceMetals,
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.Unobtanium,		"WorldEntities/Natural/Magnesium")]HullPlating, //was wiring kit
			[Item(typeof(NotFabricable), 		TechCategory.VehicleUpgrades, 	TechType.Unobtanium,		"WorldEntities/Natural/polyaniline")]Sealant,
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.Unobtanium,		"WorldEntities/Natural/aramidfibers")]SealFabric,
			[Item(typeof(BasicCraftingItem), 	TechCategory.AdvancedMaterials,	TechType.Unobtanium,		"WorldEntities/Natural/polyaniline")]HeatSealant,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.GasPod,			"WorldEntities/Natural/polyaniline")]Chlorine,
			[Item(typeof(NotFabricable),		TechCategory.VehicleUpgrades, 	TechType.SnakeMushroomSpore,"WorldEntities/Natural/polyaniline")]Luminol,
			[Item(typeof(NotFabricable),		TechCategory.AdvancedMaterials, TechType.HatchingEnzymes,	"WorldEntities/Natural/FiberMesh")]SmartPolymer,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.AcidMushroom,		"WorldEntities/Natural/hydrochloricacid")]WeakAcid,
			[Item(typeof(BasicCraftingItem),	TechCategory.Electronics, 		TechType.Lubricant,			"WorldEntities/Natural/Lubricant")]Motor,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.SeaTreaderPoop,	"WorldEntities/Natural/polyaniline")]TreaderEnzymes,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.Kyanite,			"WorldEntities/Natural/CrashPowder")]BacterialSample,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.Unobtanium,		"WorldEntities/Natural/polyaniline")]BioEnzymes,
			[Item(typeof(BasicCraftingItem),	TechCategory.Electronics, 		TechType.Unobtanium,		"WorldEntities/Natural/WiringKit")]LathingDrone,
			[Item(typeof(NotFabricable),		TechCategory.VehicleUpgrades, 	TechType.Unobtanium,		"WorldEntities/Natural/polyaniline")]KelpEnzymes,
			[Item(typeof(BasicCraftingItem),	TechCategory.AdvancedMaterials, TechType.HatchingEnzymes,	"WorldEntities/Natural/Silicone")]FuelTankWall,
			[Item(typeof(BasicCraftingItem),	TechCategory.VehicleUpgrades, 	TechType.Kyanite,			"WorldEntities/Natural/benzene")]RocketFuel,
			[Item(typeof(NotFabricable),		TechCategory.Misc, 				TechType.Unobtanium,		"WorldEntities/Tools/Battery")]BrokenT2Battery,
			[Item(typeof(NotFabricable),		TechCategory.Misc, 				TechType.Unobtanium,		"WorldEntities/Seeds/CreepvinePiece")]AmoeboidSample,
			[Item(typeof(NotFabricable),		TechCategory.AdvancedMaterials, TechType.Unobtanium,		"WorldEntities/Natural/CrashPowder")]GeyserMinerals,
			[Item(typeof(NotFabricable),		TechCategory.VehicleUpgrades, 	TechType.Unobtanium,		"WorldEntities/Natural/benzene")]WeakEnzyme42,
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
	
	class BrokenAzuriteBatterySparker : AzuriteSparker {
		BrokenAzuriteBatterySparker() : base(1.5F, 1.0F, false, new Vector3(0, 0, -0.05F)) {
			
		}		
	}
	
	class LathingDroneSparker : AzuriteSparker {
		LathingDroneSparker() : base(1.5F, 1.0F, true, new Vector3(0, 0, 0)) {
			
		}		
	}
	
	class BacteriaAnimator : AnimatorComponent {
		
		private Renderer render;
		
		private void Update() {
			if (!render) {
				render = gameObject.GetComponent<Renderer>();
			}
			MushroomTreeBacterialColony.updateColors(this, render, DayNightCycle.main.timePassedAsFloat);
		}
		
	}
	
	class CrystalLensAnimator : AnimatorComponent {
		
		private Renderer render;
		
		private void Update() {
			if (!render) {
				render = gameObject.GetComponent<Renderer>();
			}
			float f = Mathf.Sin(DayNightCycle.main.timePassedAsFloat*0.17F+gameObject.GetInstanceID()*0.6943F);
			float f2 = 0.5F+0.5F*Mathf.Sin(DayNightCycle.main.timePassedAsFloat*0.383F+gameObject.GetInstanceID()*0.357F);
			render.materials[0].SetFloat("_SpecInt", 15F+10*f);
			render.materials[0].SetFloat("_Shininess", 7.5F-2.5F*f);
			render.materials[0].SetFloat("_Fresnel", 0.5F+0.4F*f);
			Color c = new Color(f2, 0, 1, 1);
			render.materials[0].SetColor("_GlowColor", c);
			RenderUtil.setEmissivity(render, 1+f2);
		}
		
	}
	
	class RocketFuelAnimator : AnimatorComponent {
		
		private Renderer render;
		
		private void Update() {
			if (!render) {
				render = gameObject.GetComponent<Renderer>();
			}
			float f = Mathf.Sin(DayNightCycle.main.timePassedAsFloat*0.37F+gameObject.GetInstanceID()*0.6943F);
			RenderUtil.setEmissivity(render.materials[3], 2.5F-f);
			Color c = new Color(0, 0.33F+0.67F*f, 1, 1);
			render.materials[3].color = c;
			render.materials[3].SetColor("_SpecColor", c);
			render.materials[3].SetColor("_GlowColor", c);
		}
		
	}
}
