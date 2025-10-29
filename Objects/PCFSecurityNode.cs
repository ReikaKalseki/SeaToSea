using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	public class PCFSecurityNode : InteractableSpawnable {

		//private static readonly WeightedRandom<TechType> dropTable = new WeightedRandom<TechType>();

		public PCFSecurityNode(XMLLocale.LocaleEntry e) : base(e) {
			scanTime = 2;
		}

		public override GameObject GetGameObject() {
			//GameObject go1 = ObjectUtil.createWorldObject("78009225-a9fa-4d21-9580-8719a3368373").setName("Base"); //block
			GameObject baseObj = ObjectUtil.createWorldObject("473a8c4d-162f-4575-bbef-16c1c97d1e9d"); //light on top/projector base
			GameObject fx = ObjectUtil.lookupPrefab("2834aa49-4721-4d4c-9ccf-13fbbd324745").getChildObject("FX").clone();
			fx.removeChildObject("x_Precursor_ComputerTerminal_Symbol");
			fx.removeChildObject("x_Precursor_ComputerTerminal_SmallSymbol");
			GameObject cone = fx.getChildObject("x_Precursor_ComputerTerminal_Halo");
			cone.transform.localPosition = new Vector3(0, 0, -0.375F);
			Color c = new Color(0.2F, 1.0F, 1.5F, 1F);
			foreach (Renderer r in fx.GetComponentsInChildren<Renderer>()) {
				foreach (Material m in r.materials) {
					m.SetColor("_Color", c);
				}
			}
			GameObject dots = fx.getChildObject("x_Precursor_ComputerTerminal_ScreenBG");
			c = new Color(0.2F, 2.5F, 4F, 1);
			for (float ang = 30; ang < 360; ang += 30) {
				string n = "BCGAng_"+ang.ToString("0");
				GameObject go3b = fx.getChildObject(n);
				if (!go3b)
					go3b = dots.clone().setName("BCGAng_" + ang.ToString("0"));
				go3b.transform.SetParent(fx.transform);
				Utils.ZeroTransform(go3b.transform);
				go3b.transform.localEulerAngles = new Vector3(270, ang, 0);
				go3b.transform.localPosition = new Vector3(0, ang / 360F * 0.8F - 0.4F, 0);
				Material m = go3b.GetComponent<Renderer>().material;
				m.SetColor("_Color", c);
			}
			dots.GetComponent<Renderer>().material.SetColor("_Color", c);
			//go1.transform.SetParent(go.transform);
			//Utils.ZeroTransform(go1.transform);
			baseObj.transform.SetParent(baseObj.transform);
			baseObj.transform.localScale = new Vector3(2, 6, 1.8F);
			fx.transform.SetParent(baseObj.transform);
			Utils.ZeroTransform(fx.transform);
			fx.transform.localScale = new Vector3(0.33F, 1.5F, 0.33F);
			fx.transform.localPosition = new Vector3(0, -2.1F, 0);
			//baseObj.GetComponent<SphereCollider>().radius = 200;
			baseObj.removeComponent<SphereCollider>();
			//BoxCollider bc = baseObj.EnsureComponent<BoxCollider>();
			baseObj.EnsureComponent<PCFSecurityNodeTag>();
			baseObj.GetComponentInChildren<BoxCollider>().gameObject.EnsureComponent<PCFSecurityNodeRelay>();
			baseObj.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
			return baseObj;
		}

		public void postRegister() {
			countGen(SeaToSeaMod.worldgen);
			registerEncyPage();
		}
	}

	class PCFSecurityNodeTag : MonoBehaviour {

		private static readonly SoundManager.SoundData breakSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "pcfnodebreak", "Sounds/pcfnodebreak.ogg", SoundManager.soundMode3D);
		
		//private GameObject baseObject;
		private GameObject fxObject;

		PrecursorActivatedPillar pillarTrigger;

		private Renderer baseRender;

		private float rotation;

		void Start() {
			//baseObject = gameObject.getChildObject("Base");
			fxObject = gameObject.getChildObject("FX");

			pillarTrigger = GetComponent<PrecursorActivatedPillar>();
			pillarTrigger.isFullyExtended = false;

			pillarTrigger.flare.color = new Color(0.25F, 0.8F, 1F);
			pillarTrigger.flare.intensity = 2.5F;

			GameObject go = pillarTrigger.gameObject.getChildObject("Precursor_prison_exterior_box_01");
			baseRender = go.GetComponent<Renderer>();
			baseRender.material.SetColor("_GlowColor", new Color(0, 1, 2.5F, 1));
		}

		void Update() {
			Vector3 dist = (Camera.main.transform.position-fxObject.transform.position);
			float ang = Mathf.Atan2(dist.y, dist.x);
			fxObject.transform.localEulerAngles = new Vector3(0, ang + rotation, 0);

			rotation += Time.deltaTime * 30;
			pillarTrigger.extended = true;
			pillarTrigger.SetGlowColor(new Color(0, 1, 2.5F, 1), false);
		}

		public void BashHit() { //prawn hit
			if (!pillarTrigger || !fxObject.activeInHierarchy)
				return;
			pillarTrigger.flare.gameObject.SetActive(false);
			pillarTrigger.destroy();
			C2CProgression.instance.stepPCFSecurity();
			for (int i = 0; i < 20; i++)
				WorldUtil.spawnParticlesAt(transform.position+transform.up*1.5F, "361b23ed-58dd-4f45-9c5f-072fa66db88a", 0.5F, true);
			RenderUtil.setEmissivity(baseRender, 0);
			fxObject.SetActive(false);
			SoundManager.playSoundAt(breakSound, transform.position, false, -1);
		}

	}

	class PCFSecurityNodeRelay : MonoBehaviour {
		void BashHit() { //prawn hit
			GetComponentInParent<PCFSecurityNodeTag>().BashHit();
		}
	}
}
