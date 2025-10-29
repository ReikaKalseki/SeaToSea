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

	public class Obsidian : BasicCustomOre {

		internal static readonly Vector3 BASE_SCALE = new Vector3(0.1F, 0.4F, 0.25F);

		public Obsidian(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			collectSound = "event:/loot/pickup_quartz";
		}

		public override void prepareGameObject(GameObject go, Renderer[] r0) {
			base.prepareGameObject(go, r0);
			Collider c = go.GetComponentInChildren<Collider>();
			GameObject hold = c.gameObject;
			if (!(c is BoxCollider))
				c.destroy();
			BoxCollider bc = hold.EnsureComponent<BoxCollider>();
			bc.size = BASE_SCALE;
			bc.center = new Vector3(0, 0.15F, 0);
			foreach (Renderer r in r0) {
				//GameObject go = ;
				RenderUtil.setEmissivity(r, 0);
				r.transform.localScale = BASE_SCALE;
				r.materials[0].SetFloat("_Fresnel", 0.25F);
				r.materials[0].SetFloat("_SpecInt", 1F);
				r.materials[0].SetFloat("_Shininess", 45F);
			}
			go.EnsureComponent<ObsidianTag>();
		}

	}

	class ObsidianTag : MonoBehaviour {

		public static readonly float MELT_TIME = 15F;

		public float meltLevel;

		private Renderer[] renders;

		void Start() {
			renders = this.GetComponentsInChildren<Renderer>();
		}

		void Update() {
			float dT = Time.deltaTime;

			float temp = WaterTemperatureSimulation.main.GetTemperature(transform.position);
			if (temp >= 100) {
				meltLevel += dT / MELT_TIME;

				foreach (Renderer r in renders) {
					r.materials[0].SetColor("_SpecColor", new Color(1 + (9 * meltLevel), 1 + (3 * meltLevel), 1, 1));
					if (meltLevel > 0.67) {
						r.transform.localScale = Obsidian.BASE_SCALE * (float)MathUtil.linterpolate(meltLevel, 0.67, 1, 1, 0, true);
					}
				}

				if (meltLevel >= 1)
					gameObject.destroy(false);
			}
		}

	}
}
