using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class MushroomTreeBacterialColony : Spawnable {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
	    internal MushroomTreeBacterialColony(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaFlora.AMOEBOID.getRandomPrefab(true));
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			GameObject child = ObjectUtil.getChildObject(world, "lost_river_plant_04");
			GameObject inner = ObjectUtil.getChildObject(child, "lost_river_plant_04");
			GameObject shell = ObjectUtil.getChildObject(child, "lost_river_plant_04_membrane");
			
			child.transform.localScale = new Vector3(1F, 0.5F, 1F);
			inner.transform.localScale = new Vector3(1.8F, 1.9F, 1.8F);
			
			Renderer r1 = inner.GetComponentInChildren<Renderer>();
			Renderer r2 = shell.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r1, "Textures/Plants/TreeColony");
			RenderUtil.setEmissivity(r1, 1, "GlowStrength");
			RenderUtil.setEmissivity(r2, 1, "GlowStrength");
			
			r1.material.EnableKeyword("UWE_WAVING");
			r1.material.SetFloat("_Shininess", 0F);
			r1.material.SetFloat("_SpecInt", 0F);
			r1.material.SetColor("_Color", Color.white);
			r1.material.SetVector("_Scale", new Vector4(0.4F, 0.3F, 0.4F, 0.3F));
			r1.material.SetVector("_Frequency", new Vector4(1.0F, 1.2F, 1.2F, 0.5F));
			r1.material.SetVector("_Speed", new Vector4(0.05F, 0.05F, 0.0F, 0.0F));
			r1.material.SetVector("_ObjectUp", new Vector4(0.25F, 0F, 1F, 0F));
			r1.material.SetFloat("_WaveUpMin", 10F);
			
			r2.material.EnableKeyword("UWE_WAVING");
			r2.material.SetFloat("_Shininess", 15F);
			r2.material.SetFloat("_SpecInt", 2F);
			r2.material.SetFloat("_Fresnel", 0.75F);
			r2.material.SetColor("_Color", Color.white);
			r2.material.SetVector("_Scale", new Vector4(0.3F, 0.2F, 0.3F, 0.2F));
			r2.material.SetVector("_Frequency", new Vector4(1.0F, 1.2F, 1.2F, 0.5F));
			r2.material.SetVector("_Speed", new Vector4(0.15F, 0.1F, 0.0F, 0.0F));
			r2.material.SetVector("_ObjectUp", new Vector4(0.25F, 0F, 1F, 0F));
			r2.material.SetFloat("_WaveUpMin", 10F);
			world.EnsureComponent<TreeColonyTag>();
			return world;
	    }
		
		public void register() {
			Patch();
			TechType unlock = CraftingItems.getItem(CraftingItems.Items.BacterialSample).TechType;
        	KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>(){unlock});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = unlock;
			e.destroyAfterScan = false;
			e.locked = true;
			e.totalFragments = 9;
			e.isFragment = true;
			e.scanTime = 5;
			PDAHandler.AddCustomScannerEntry(e);
		}
			
	}
		
	class TreeColonyTag : MonoBehaviour {
		
		void Update() {
			
		}
		
	}
}
