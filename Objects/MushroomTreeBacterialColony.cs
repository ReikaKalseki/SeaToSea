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
		
		public static readonly Color BLUE_COLOR = new Color(0, 112/255F, 1, 1);
		public static readonly Color PURPLE_COLOR = new Color(160/255F, 12/255F, 1);
		
		private int FRAGMENT_COUNT;
	        
	    internal MushroomTreeBacterialColony(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
			
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<TreeColonyTag>().addField("scanned"));
			};
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaFlora.AMOEBOID.getRandomPrefab(true));
			world.EnsureComponent<TechTag>().type = TechType;
			PrefabIdentifier pi = world.EnsureComponent<PrefabIdentifier>();
			pi.ClassId = ClassID;
			GameObject child = ObjectUtil.getChildObject(world, "lost_river_plant_04");
			GameObject inner = ObjectUtil.getChildObject(child, "lost_river_plant_04");
			GameObject shell = ObjectUtil.getChildObject(child, "lost_river_plant_04_membrane");
			
			//inner.transform.localScale = new Vector3(1.8F, 1.9F, 1.8F);
			inner.SetActive(false);
			shell.transform.localScale = new Vector3(0.9F, 0.9F, 1F);
			shell.transform.localPosition = new Vector3(0, -0.25F, 0);
			
			Renderer r2 = shell.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r2, "Textures/Plants/TreeColony");
			RenderUtil.setEmissivity(r2, 2);
			RenderUtil.disableTransparency(r2.material);
			setupWave(r2);
			world.EnsureComponent<TreeColonyTag>();
			world.layer = LayerID.Useable;
			
			Light l = ObjectUtil.addLight(world);
			l.range = 4;
			l.intensity = 1F;
			l.color = BLUE_COLOR;
			l.gameObject.name = "WideLight";
			l.gameObject.transform.localPosition = Vector3.up*1.5F;
			l = ObjectUtil.addLight(world);
			l.range = 1.5F;
			l.intensity = 3F;
			l.color = PURPLE_COLOR;
			l.gameObject.name = "InnerLight";
			l.gameObject.transform.localPosition = Vector3.up*1.5F;
			
			world.EnsureComponent<ImmuneToPropulsioncannon>().immuneToRepulsionCannon = true;
			
			if (!SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
				ResourceTracker rt = world.EnsureComponent<ResourceTracker>();
				rt.techType = TechType;
				rt.overrideTechType = TechType;
				rt.prefabIdentifier = pi;
			}
			
			return world;
	    }
		
		public int getFragmentCount() {
			return FRAGMENT_COUNT;
		}
		
		public void register() {
			Patch();
		}
		
		public void postRegister() {
			PDAManager.PDAPage page = PDAManager.createPage("ency_"+ClassID, FriendlyName, locale.pda, "Lifeforms/Coral");
			page.setHeaderImage(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/PDA/Bacteria"));
			page.register();
			TechType unlock = CraftingItems.getItem(CraftingItems.Items.BacterialSample).TechType;
        	KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>(){unlock});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = unlock;
			e.destroyAfterScan = false;
			e.locked = true;
			FRAGMENT_COUNT = SeaToSeaMod.worldgen.getCount(ClassID);
			e.totalFragments = FRAGMENT_COUNT;
			SNUtil.log("Found "+e.totalFragments+" "+ClassID+" to use as fragments", SeaToSeaMod.modDLL);
			e.isFragment = true;
			e.scanTime = 5;
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
		}
		
		public static void setupWave(Renderer r2, float str = 1) {
			r2.material.SetColor("_GlowColor", Color.white);			
			r2.material.EnableKeyword("UWE_WAVING");
			r2.material.SetFloat("_Shininess", 0F);
			r2.material.SetFloat("_SpecInt", 0F);
			r2.material.SetColor("_Color", Color.white);
			r2.material.SetVector("_Scale", new Vector4(0.1F, 0.05F, 0.1F, 0.005F)*Mathf.Pow(str, 2.2F));
			r2.material.SetVector("_Frequency", new Vector4(3.0F, 4.0F, 4.0F, 25.0F)*str);
			r2.material.SetVector("_Speed", new Vector4(0.02F, 0.02F, 0.0F, 0.0F));
			r2.material.SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
			r2.material.SetFloat("_WaveUpMin", 0F);
		}
		
		public static float updateColors(MonoBehaviour c, Renderer r, float time) {
			float f = 0.5F+0.5F*Mathf.Sin(time*0.193F+c.transform.position.magnitude%1781);
			setColors(r, f);
			return f;
		}
		
		internal static void setColors(Renderer r, float f) {
			r.material.SetColor("_GlowColor", new Color(f*1.5F, 1, 1, 1));
			r.material.SetColor("_Color", new Color(0.75F+1.25F*f, 1, 1, 1));
		}
			
	}
		
	class TreeColonyTag : MonoBehaviour {
		
		internal static readonly Simplex3DGenerator sizeXNoise = (Simplex3DGenerator)new Simplex3DGenerator(53476347).setFrequency(0.8);
		internal static readonly Simplex3DGenerator sizeZNoise = (Simplex3DGenerator)new Simplex3DGenerator(-1376491).setFrequency(0.8);
		
		private float lastResize = -1;
		private bool scanned = false;
		private float scannedFade = 0;
		
		private Renderer render;
		private Light innerLight;
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time >= lastResize+1) {
				lastResize = time;
				transform.localScale = getRegionalScale();//new Vector3(0.5F, 0.25F, 0.5F);
			}
			if (!render)
				render = ObjectUtil.getChildObject(gameObject, "lost_river_plant_04/lost_river_plant_04_membrane").GetComponentInChildren<Renderer>();
			if (!innerLight)
				innerLight = ObjectUtil.getChildObject(gameObject, "InnerLight").GetComponentInChildren<Light>();
			
			float f = MushroomTreeBacterialColony.updateColors(this, render, time);
			float f2 = 3F+0.5F*(1-f);
			if (scanned)
				scannedFade = Mathf.Min(scannedFade+0.5F*Time.deltaTime, 1);
			
			Color c = Color.Lerp(MushroomTreeBacterialColony.BLUE_COLOR, MushroomTreeBacterialColony.PURPLE_COLOR, 0.33F+0.67F*f);
			if (scannedFade > 0) {
				f += 2.5F*scannedFade;
				f2 += 1.5F*scannedFade;
				c = Color.Lerp(c, new Color(1, 0, 0), scannedFade);
				MushroomTreeBacterialColony.setupWave(render, 1+scannedFade*0.004F);
				MushroomTreeBacterialColony.setColors(render, f);
			}
			innerLight.color = c;
			innerLight.intensity = f2;
		}
		
		Vector3 getRegionalScale() {
			Vector3 pos = transform.position;
			return new Vector3(0.5F+0.2F*(float)sizeXNoise.getValue(pos), 0.25F, 0.5F+0.2F*(float)sizeZNoise.getValue(pos));
		}
		
		void OnScanned() {
			scanned = true;
		}
		
	}
}
