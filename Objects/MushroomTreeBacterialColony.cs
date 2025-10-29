using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class MushroomTreeBacterialColony : InteractableSpawnable {

		public static readonly Color BLUE_COLOR = new Color(0, 112/255F, 1, 1);
		public static readonly Color PURPLE_COLOR = new Color(160/255F, 12/255F, 1);

		internal MushroomTreeBacterialColony(XMLLocale.LocaleEntry e) : base(e) {
			scanTime = 5;
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<TreeColonyTag>().addField("scanned"));
			};
		}

		public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaFlora.AMOEBOID.getRandomPrefab(true));
			world.EnsureComponent<TechTag>().type = TechType;
			PrefabIdentifier pi = world.EnsureComponent<PrefabIdentifier>();
			pi.ClassId = ClassID;
			GameObject child = world.getChildObject("lost_river_plant_04");
			GameObject inner = child.getChildObject("lost_river_plant_04");
			GameObject shell = child.getChildObject("lost_river_plant_04_membrane");

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

			Light l = world.addLight(1, 4, BLUE_COLOR).setName("WideLight");
			l.gameObject.transform.localPosition = Vector3.up * 1.5F;
			l = world.addLight(3, 1.5F, PURPLE_COLOR).setName("InnerLight");
			l.gameObject.transform.localPosition = Vector3.up * 1.5F;

			world.EnsureComponent<ImmuneToPropulsioncannon>().immuneToRepulsionCannon = true;

			if (!SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
				ResourceTracker rt = world.EnsureComponent<ResourceTracker>();
				rt.techType = TechType;
				rt.overrideTechType = TechType;
				rt.prefabIdentifier = pi;
			}

			return world;
		}

		public void register() {
			this.Patch();
		}

		public void postRegister() {
			countGen(SeaToSeaMod.worldgen);
			setFragment(CraftingItems.getItem(CraftingItems.Items.BacterialSample).TechType, fragmentCount);
			registerEncyPage();
		}

		public static void setupWave(Renderer r2, float str = 1) {
			r2.material.SetColor("_GlowColor", Color.white);
			r2.material.EnableKeyword("UWE_WAVING");
			r2.material.SetFloat("_Shininess", 0F);
			r2.material.SetFloat("_SpecInt", 0F);
			r2.material.SetColor("_Color", Color.white);
			r2.material.SetVector("_Scale", new Vector4(0.1F, 0.05F, 0.1F, 0.005F) * Mathf.Pow(str, 2.2F));
			r2.material.SetVector("_Frequency", new Vector4(3.0F, 4.0F, 4.0F, 25.0F) * str);
			r2.material.SetVector("_Speed", new Vector4(0.02F, 0.02F, 0.0F, 0.0F));
			r2.material.SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
			r2.material.SetFloat("_WaveUpMin", 0F);
		}

		public static float updateColors(MonoBehaviour c, Renderer r, float time) {
			float f = 0.5F+(0.5F*Mathf.Sin((time*0.193F)+(c.transform.position.magnitude%1781)));
			setColors(r, f);
			return f;
		}

		internal static void setColors(Renderer r, float f) {
			r.material.SetColor("_GlowColor", new Color(f * 1.5F, 1, 1, 1));
			r.material.SetColor("_Color", new Color(0.75F + (1.25F * f), 1, 1, 1));
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
			if (time >= lastResize + 1) {
				lastResize = time;
				transform.localScale = this.getRegionalScale();//new Vector3(0.5F, 0.25F, 0.5F);
			}
			if (!render)
				render = gameObject.getChildObject("lost_river_plant_04/lost_river_plant_04_membrane").GetComponentInChildren<Renderer>();
			if (!innerLight)
				innerLight = gameObject.getChildObject("InnerLight").GetComponentInChildren<Light>();

			float f = MushroomTreeBacterialColony.updateColors(this, render, time);
			float f2 = 3F+(0.5F*(1-f));
			if (scanned)
				scannedFade = Mathf.Min(scannedFade + (0.5F * Time.deltaTime), 1);

			Color c = Color.Lerp(MushroomTreeBacterialColony.BLUE_COLOR, MushroomTreeBacterialColony.PURPLE_COLOR, 0.33F+(0.67F*f));
			if (scannedFade > 0) {
				f += 2.5F * scannedFade;
				f2 += 0.75F * scannedFade;
				c = Color.Lerp(c, new Color(1, 0, 0), scannedFade);
				MushroomTreeBacterialColony.setupWave(render, 1 + (scannedFade * 0.004F));
				MushroomTreeBacterialColony.setColors(render, f);
			}
			innerLight.color = c;
			innerLight.intensity = f2;
		}

		Vector3 getRegionalScale() {
			Vector3 pos = transform.position;
			return new Vector3(0.5F + (0.2F * (float)sizeXNoise.getValue(pos)), 0.25F, 0.5F + (0.2F * (float)sizeZNoise.getValue(pos)));
		}

		void OnScanned() {
			scanned = true;
			SNUtil.addBlueprintNotification(MushroomTreeBacterialColony.fragmentUnlock);
		}

	}
}
