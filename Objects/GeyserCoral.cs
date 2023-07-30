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
	
	public class GeyserCoral : Spawnable {
		
		private readonly XMLLocale.LocaleEntry locale;
		
		//public static readonly Color GLOW_COLOR = new Color(1, 112/255F, 0, 1);
	        
	    internal GeyserCoral(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
			
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<GeyserCoralTag>().addField("scanned"));
			};
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaFlora.TABLECORAL_ORANGE.getRandomPrefab(true));
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			
			foreach (Renderer r2 in world.GetComponentsInChildren<Renderer>()) {
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r2, "Textures/Plants/GeyserCoral");
				RenderUtil.setGlossiness(r2, 6, -200, -10);
				RenderUtil.setEmissivity(r2, 20);
				r2.material.SetColor("_Color", Color.white);
				r2.material.SetColor("_SpecColor", Color.white);
				r2.material.SetColor("_GlowColor", Color.white);
			}
			world.EnsureComponent<GeyserCoralTag>();
			world.layer = LayerID.Useable;
			
			//rotate to 270, 0, 0
			
			ObjectUtil.removeComponent<LiveMixin>(world);
			
			world.EnsureComponent<ImmuneToPropulsioncannon>().immuneToRepulsionCannon = true;
			
			return world;
	    }
		
		public void register() {
			Patch();
		}
		
		public void postRegister() {
			TechType unlock = SeaToSeaMod.geyserFilter.TechType;
        	KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>(){unlock});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = unlock;
			e.destroyAfterScan = false;
			e.locked = true;
			e.totalFragments = SeaToSeaMod.worldgen.getCount(ClassID);
			SNUtil.log("Found "+e.totalFragments+" "+ClassID+" to use as fragments", SeaToSeaMod.modDLL);
			e.isFragment = true;
			e.scanTime = 6;
			PDAHandler.AddCustomScannerEntry(e);
		}
			
	}
		
	class GeyserCoralTag : MonoBehaviour {
		
		private bool scanned = false;
		private float scannedFade = 0;
		
		private Renderer render;
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (!render)
				render = GetComponentInChildren<Renderer>();
			
			if (scanned)
				scannedFade = Mathf.Min(scannedFade+0.5F*Time.deltaTime, 1);
			float f = 100+(20+scannedFade*40)*Mathf.Sin(time*0.617F+transform.position.magnitude%1781);
			RenderUtil.setEmissivity(render, f);
			//SNUtil.writeToChat(render.materials[0].GetFloat("_GlowStrength").ToString("0.000"));
			
			//render.material.SetColor("_GlowColor", GeyserCoral.GLOW_COLOR);
		}
		
		void OnScanned() {
			scanned = true;
		}
		
	}
}
