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
			
			inner.transform.localScale = new Vector3(1.8F, 1.9F, 1.8F);
			shell.transform.localScale = new Vector3(1.0F, 1.0F, 1.1F);
			
			Renderer r1 = inner.GetComponentInChildren<Renderer>();
			Renderer r2 = shell.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r1, "Textures/Plants/TreeColony");
			RenderUtil.setEmissivity(r1, 1, "GlowStrength");
			RenderUtil.setEmissivity(r2, 1, "GlowStrength");
			
			//RenderUtil.enableAlpha(r1.material);
			r1.material.SetColor("_GlowColor", Color.white);
			
			r1.material.EnableKeyword("UWE_WAVING");
			r1.material.SetFloat("_Shininess", 0F);
			r1.material.SetFloat("_SpecInt", 0F);
			r1.material.SetColor("_Color", Color.white);
			r1.material.SetVector("_Scale", new Vector4(0.4F, 0.3F, 0.4F, 0.3F));
			r1.material.SetVector("_Frequency", new Vector4(1.0F, 1.2F, 1.2F, 0.5F));
			r1.material.SetVector("_Speed", new Vector4(0.05F, 0.05F, 0.0F, 0.0F));
			r1.material.SetVector("_ObjectUp", new Vector4(0.25F, 0F, 1F, 0F));
			r1.material.SetFloat("_WaveUpMin", 0F);
			
			r2.material.EnableKeyword("UWE_WAVING");
			r2.material.SetFloat("_Shininess", 15F);
			r2.material.SetFloat("_SpecInt", 2F);
			r2.material.SetFloat("_Fresnel", 0.75F);
			r2.material.SetColor("_Color", Color.white);
			r2.material.SetVector("_Scale", new Vector4(0.3F, 0.1F, 0.3F, 0.15F));
			r2.material.SetVector("_Frequency", new Vector4(1.0F, 1.2F, 1.2F, 0.5F));
			r2.material.SetVector("_Speed", new Vector4(0.15F, 0.1F, 0.0F, 0.0F));
			r2.material.SetVector("_ObjectUp", new Vector4(0.25F, 0F, 1F, 0F));
			r2.material.SetFloat("_WaveUpMin", 0F);
			world.EnsureComponent<TreeColonyTag>();
			world.layer = LayerID.Useable;
			
			Color c = new Color(0.25F, 0.67F, 1F, 1F);
			Light l = ObjectUtil.addLight(world);
			l.range = 4;
			l.intensity = 0.85F;
			l.color = c;
			l = ObjectUtil.addLight(world);
			l.range = 1.5F;
			l.intensity = 2.5F;
			l.color = c;
			
			return world;
	    }
		
		public void register() {
			Patch();
		}
		
		public void postRegister() {
			TechType unlock = CraftingItems.getItem(CraftingItems.Items.BacterialSample).TechType;
        	KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>(){unlock});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = unlock;
			e.destroyAfterScan = false;
			e.locked = true;
			e.totalFragments = SeaToSeaMod.worldgen.getCount(ClassID);
			SNUtil.log("Found "+e.totalFragments+" "+ClassID+" to use as fragments", SeaToSeaMod.modDLL);
			e.isFragment = true;
			e.scanTime = 5;
			PDAHandler.AddCustomScannerEntry(e);
		}
			
	}
		
	class TreeColonyTag : MonoBehaviour {
		
		internal static readonly Simplex3DGenerator sizeXNoise = (Simplex3DGenerator)new Simplex3DGenerator(53476347).setFrequency(0.8);
		internal static readonly Simplex3DGenerator sizeZNoise = (Simplex3DGenerator)new Simplex3DGenerator(-1376491).setFrequency(0.8);
		
		private float lastResize = -1;
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time >= lastResize+1) {
				lastResize = time;
				transform.localScale = getRegionalScale();//new Vector3(0.5F, 0.25F, 0.5F);
			}
		}
		
		Vector3 getRegionalScale() {
			Vector3 pos = transform.position;
			return new Vector3(0.5F+0.2F*(float)sizeXNoise.getValue(pos), 0.25F, 0.5F+0.2F*(float)sizeZNoise.getValue(pos));
		}
		
	}
}
