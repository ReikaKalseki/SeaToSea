using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class PostCoveDome : Spawnable {
		
		public readonly XMLLocale.LocaleEntry locale;
		
		public static readonly float HOT_THRESHOLD = -1070F;
		
		private int FRAGMENT_COUNT;
		
		public PostCoveDome(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
		}
		
		public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject(VanillaCreatures.GIANT_FLOATER.prefab);
			foreach (Light l in go.GetComponentsInChildren<Light>())
				UnityEngine.Object.Destroy(l.gameObject);
			go.EnsureComponent<PostCoveDomeTag>();
			return go;
		}
		
		public int getFragmentCount() {
			return FRAGMENT_COUNT;
		}
		
		public void postRegister() {
			PDAManager.PDAPage page = PDAManager.createPage("ency_"+ClassID, FriendlyName, locale.pda, locale.getField<string>("category"));
			page.setHeaderImage(TextureManager.getTexture(SeaToSeaMod.modDLL, locale.getField<string>("header")));
			page.register();
			TechType unlock = CraftingItems.getItem(CraftingItems.Items.ObsidianGlass).TechType;
        	KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>(){unlock});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = unlock;
			e.destroyAfterScan = false;
			e.locked = true;
			FRAGMENT_COUNT = SeaToSeaMod.worldgen.getCount<PostCoveDomeGenerator>();
			e.totalFragments = FRAGMENT_COUNT;
			SNUtil.log("Found "+e.totalFragments+" "+ClassID+" to use as fragments", SeaToSeaMod.modDLL);
			e.isFragment = true;
			e.scanTime = 10;
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
		}
		
		public static void setupRenderGloss(Renderer r) {
			RenderUtil.setGlossiness(r.materials[0], 750, 15, 0.8F);
			RenderUtil.setEmissivity(r.materials[0], 0);
			if (r.materials.Length > 1) {
				RenderUtil.setGlossiness(r.materials[1], 0, 0, 0F);
				RenderUtil.setEmissivity(r.materials[1], 2);
			}
			r.materials[0].SetColor("_Color", Color.white);
			r.materials[0].SetColor("_SpecColor", Color.white);
			r.materials[0].SetColor("_GlowColor", Color.white);
			r.materials[0].SetFloat("_IBLReductionAtNight", 0);
			r.materials[0].SetFloat("_EmissionLM", 0);
			r.materials[0].SetFloat("_EmissionLMNight", 0);
		}
		
	}
	
	public class PostCoveDomeTag : MonoBehaviour {
		
		private Renderer[] renderers;
		
		private bool computedTexture;
		private bool isHot;
		
		private Light light;
		
		void Start() {
			ObjectUtil.removeComponent<Floater>(gameObject);
			GetComponentInChildren<Animator>().speed = 0.04F;
			renderers = GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
				r.materials[1].color = Color.clear;
			transform.localScale = Vector3.one * 0.1F;
			
			light = GetComponentInChildren<Light>();
			if (!light)
				light = ObjectUtil.addLight(gameObject);
			light.range = 32;
			light.intensity = 1.5F;
			light.transform.localPosition = Vector3.up*3;
			light.shadows = LightShadows.Soft;
			Invoke("spawnOffspring", 30);
		}
		
		void Update() {
			bool hot = transform.position.y < PostCoveDome.HOT_THRESHOLD;//VanillaBiomes.ILZ.isInBiome(transform.position) || WaterTemperatureSimulation.main.GetTemperature(transform.position) >= 90;
			bool retexture = isHot != hot;
			isHot = hot;
			if (retexture || !computedTexture) {
				string tex = "Textures/Plants/PostCoveTree/"+(isHot ? "Hot" : "Cold");
				foreach (Renderer r in renderers) {
					RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, tex, new Dictionary<int, string>{{1, "Inner"}, {0, "Shell"}});
					PostCoveDome.setupRenderGloss(r);
				}
				computedTexture = true;
				light.color = isHot ? new Color(1, 0.8F, 0.2F) : new Color(0.2F, 0.6F, 1F);
			}
		}
		
		public static int maximumDomeChildren = 16;
		public static bool fastReproduction = false;
		
		void spawnOffspring() {
			IEnumerable<Vector3> li = WorldUtil.getObjectsNearWithComponent<PostCoveDomeGenerator.ResourceDomeTag>(transform.position, 24).Select(tag => tag.transform.position);
			if (li.Count() < maximumDomeChildren) {
				GameObject go = PostCoveDomeGenerator.placeRandomResourceDome(gameObject, li);
				if (go) {
					go.GetComponent<PostCoveDomeGenerator.ResourceDomeTag>().growFade = 1;
				}
			}
			Invoke("spawnOffspring", fastReproduction ? 1 : UnityEngine.Random.Range(30F, 120F));
		}
		
	}
}
