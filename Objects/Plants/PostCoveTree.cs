using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	[Obsolete]
	public class PostCoveTree : Spawnable {

		internal static readonly Dictionary<DecoPlants, float> templates = new Dictionary<DecoPlants, float>(){
			{DecoPlants.LOST_BRANCHES_4, 0.33F},
			{DecoPlants.LOST_BRANCHES_5, 0.25F},
			{DecoPlants.LOST_BRANCHES_6, 0.08F},
		};

		internal static readonly Dictionary<string, PostCoveTree> types = new Dictionary<string, PostCoveTree>();

		public readonly XMLLocale.LocaleEntry locale;
		public readonly DecoPlants template;

		private static int FRAGMENT_COUNT;

		public PostCoveTree(XMLLocale.LocaleEntry e, DecoPlants pfb) : base(e.key + "_" + pfb, e.name, e.desc) {
			locale = e;
			template = pfb;

			OnFinishedPatching += () => { types[ClassID] = this; };
		}

		public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject(template.prefab);
			foreach (Light l in go.GetComponentsInChildren<Light>())
				l.gameObject.destroy(false);
			go.EnsureComponent<PostCoveTreeTag>();
			return go;
		}

		protected override void ProcessPrefab(GameObject world) {
			base.ProcessPrefab(world);
			//world.EnsureComponent<TechTag>().type = C2CItems.postCoveTreeCommon;
		}

		public static PDAScanner.EntryData postRegister() {
			TechType unlock = CraftingItems.getItem(CraftingItems.Items.ObsidianGlass).TechType;
			//KnownTechHandler.Main.SetAnalysisTechEntry(C2CItems.postCoveTreeCommon, new List<TechType>(){unlock});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			//e.key = C2CItems.postCoveTreeCommon;
			e.blueprint = unlock;
			e.destroyAfterScan = false;
			e.locked = true;
			FRAGMENT_COUNT = 0;/*
			foreach (PostCoveTree pc in C2CItems.postCoveTrees.Values) {
				FRAGMENT_COUNT += SeaToSeaMod.worldgen.getCount(pc.ClassID);
			}*/
			e.totalFragments = FRAGMENT_COUNT;
			SNUtil.log("Found " + e.totalFragments + " of post-cove-tree to use as fragments", SeaToSeaMod.modDLL);
			e.isFragment = true;
			e.scanTime = 5;
			PDAHandler.AddCustomScannerEntry(e);
			return e;
		}

		public static int getFragmentCount() {
			return FRAGMENT_COUNT;
		}
		/*
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
			FRAGMENT_COUNT = SeaToSeaMod.worldgen.getCount(ClassID);
			e.totalFragments = FRAGMENT_COUNT;
			SNUtil.log("Found "+e.totalFragments+" "+ClassID+" to use as fragments", SeaToSeaMod.modDLL);
			e.isFragment = true;
			e.scanTime = 10;
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
		}*/

	}

#pragma warning disable CS0612 // Type or member is obsolete
	class PostCoveTreeTag : MonoBehaviour {

		private Renderer renderer;

		private bool isHot;

		void Start() {
			gameObject.removeComponent<DIHooks.FruitPlantTag>();
			gameObject.removeComponent<FruitPlant>();
			foreach (PickPrefab l in this.GetComponentsInChildren<PickPrefab>())
				l.gameObject.destroy(false);
			renderer = this.GetComponentInChildren<Renderer>();
			renderer.materials[1].color = Color.clear;
			PostCoveTree type = PostCoveTree.types[this.GetComponent<PrefabIdentifier>().ClassId];
			transform.localScale = Vector3.one * PostCoveTree.templates[type.template];
		}

		void Update() {
			bool hot = transform.position.y < -1040;//VanillaBiomes.ILZ.isInBiome(transform.position) || WaterTemperatureSimulation.main.GetTemperature(transform.position) >= 90;
			bool retexture = isHot != hot;
			isHot = hot;
			if (retexture) {
				PostCoveTree type = PostCoveTree.types[this.GetComponent<PrefabIdentifier>().ClassId];
				string n = type.template.getName();
				string tex = "Textures/Plants/PostCoveTree/"+(isHot ? "Hot" : "Cool")+n[n.Length-1];
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, renderer, tex);
				RenderUtil.setGlossiness(renderer.materials[0], 60, 15, 0.6F);
				RenderUtil.setEmissivity(renderer.materials[0], 10);
				renderer.materials[0].SetColor("_Color", Color.white);
				renderer.materials[0].SetColor("_SpecColor", Color.white);
				renderer.materials[0].SetColor("_GlowColor", Color.white);
			}
		}

	}
#pragma warning restore CS0612 // Type or member is obsolete
}
