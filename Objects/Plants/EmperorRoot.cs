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

	public class EmperorRoot : Spawnable {

		public readonly string basePrefab;

		public EmperorRoot(XMLLocale.LocaleEntry e, string pfb) : base(e.key + "_" + pfb, e.name, e.desc) {
			basePrefab = pfb;
		}

		public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject(basePrefab);
			foreach (Light l in go.GetComponentsInChildren<Light>())
				l.gameObject.destroy(false);
			GameObject mdl = go.getChildObject("models");
			go.transform.localScale = Vector3.one * 0.3F;
			foreach (Renderer r in mdl.GetComponentsInChildren<Renderer>()) {
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Plants/EmperorRoot/" + r.transform.parent.parent.gameObject.name);
				r.materials[0].SetFloat("_Shininess", 2.5F);
				r.materials[0].SetFloat("_Fresnel", 0.75F);
				r.materials[0].SetFloat("_SpecInt", /*5F*/0.33F);
				r.materials[0].SetFloat("_EmissionLM", 0F);
				r.materials[0].SetFloat("_EmissionLMNight", 0F);
				RenderUtil.setEmissivity(r, 50);
			}
			foreach (PickPrefab pick in go.GetComponentsInChildren<PickPrefab>()) {
				pick.pickTech = C2CItems.emperorRootOil.TechType;
				foreach (Renderer r in pick.GetComponentsInChildren<Renderer>()) {
					EmperorRootOil.setupRendering(r, false);
				}
			}
			go.EnsureComponent<EmperorRootTag>();
			return go;
		}

		protected override void ProcessPrefab(GameObject world) {
			base.ProcessPrefab(world);
			world.EnsureComponent<TechTag>().type = C2CItems.emperorRootCommon;
		}

	}

	class EmperorRootTag : MonoBehaviour {

		private HarvestPoint[] oils; //they are made inactive, not deleted, when picked

		public static readonly float REGROW_TIME = 1800F; //30 min

		void Update() {
			transform.localScale = Vector3.one * 0.3F;
			if (oils == null) {
				PickPrefab[] pp = this.GetComponentsInChildren<PickPrefab>(true);
				oils = new HarvestPoint[pp.Length];
				for (int i = 0; i < pp.Length; i++) {
					oils[i] = new HarvestPoint(pp[i]);
				}
			}
			float time = DayNightCycle.main.timePassedAsFloat;
			foreach (HarvestPoint hp in oils) {
				float dT = time-hp.lastHarvestTime;
				/*
				if (dT > 1) {
					hp.pickable.gameObject.SetActive(true);
				}
				foreach (Collider c in hp.colliders)
					c.enabled = dT >= REGROW_TIME;
				float f = (float)MathUtil.linterpolate(dT, 0, REGROW_TIME, 0, 1, true);
				hp.pickable.transform.localScale = Vector3.one*f;
				foreach (Renderer r in hp.renders) {
					r.transform.localPosition = new Vector3((1-f)*2.5F, 0, 0);
				}*/
				hp.pickable.gameObject.SetActive(dT >= REGROW_TIME || hp.lastHarvestTime <= 0);
			}

		}

	}

	class HarvestPoint {

		internal readonly PickPrefab pickable;
		internal readonly ChildObjectIdentifier id;
		internal readonly Collider[] colliders;
		internal readonly Renderer[] renders;

		internal float lastHarvestTime = -1;

		internal HarvestPoint(PickPrefab pp) {
			pickable = pp;
			id = pp.GetComponent<ChildObjectIdentifier>();
			colliders = pp.GetComponentsInChildren<Collider>();
			renders = pp.GetComponentsInChildren<Renderer>();
			pp.pickedEvent.AddHandler(pickable, new UWE.Event<PickPrefab>.HandleFunction(p => {
				lastHarvestTime = DayNightCycle.main.timePassedAsFloat;
				//does not work, probably wrong GO p.gameObject.EnsureComponent<EmperorRootOil.EmperorRootOilTag>().pickupTime = lastHarvestTime;
			}));
		}

	}
}
