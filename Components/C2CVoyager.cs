using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	internal class C2CVoyager : MonoBehaviour {

		private static readonly float MIN_VOID_TIME = 10;
		private static readonly float STRONG_VOID_TIME = 20;

		private Rigidbody body;
		private LiveMixin live;
		private PowerRelay power;

		private float voidTime = 0;

		void Update() {
			if (!body)
				body = this.GetComponent<Rigidbody>();
			if (!live)
				live = this.GetComponent<LiveMixin>();
			if (!power)
				power = this.GetComponent<PowerRelay>();

			if (this.shouldSink())
				voidTime += Time.deltaTime;
			else
				voidTime = 0;

			if (voidTime >= MIN_VOID_TIME) {
				float f2 = (float)MathUtil.linterpolate(voidTime-STRONG_VOID_TIME, 0, 10, 0, 0.5F, true);
				float f = (float)MathUtil.linterpolate(voidTime-MIN_VOID_TIME, 0, STRONG_VOID_TIME-MIN_VOID_TIME, 0, 50, true);
				if (voidTime < STRONG_VOID_TIME)
					f *= (float)MathUtil.linterpolate(-transform.position.y, 0, 25, 1, f2, true);
				body.AddForce(Vector3.down * Time.deltaTime * f, ForceMode.VelocityChange);
			}
		}

		void Start() {
			base.InvokeRepeating("tick", 0f, 0.5F);
			base.InvokeRepeating("slowTick", 0f, 5F);

			foreach (Constructable c in this.GetComponentsInChildren<Constructable>()) {
				c.gameObject.destroy(false);
			}

			Renderer decals = gameObject.getChildObject("Model/Exterior/Decals").GetComponent<MeshRenderer>();
			Material decal = decals.materials[4];
			decal.SetTexture("_MainTex", TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/SeaVoyagerDecal_MainTex"));
			decal.SetTexture("_SpecTex", TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/SeaVoyagerDecal_MainTex"));
			decal.SetTexture("_EmissionMap", TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/SeaVoyagerDecal_Illum"));
			decal.SetTexture("_Illum", TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/SeaVoyagerDecal_Illum"));
		}

		private void OnKill() {
			this.destroy(false);
		}

		void OnDisable() {
			base.CancelInvoke("tick");
			base.CancelInvoke("slowTick");
		}

		internal void tick() {
			if (voidTime >= MIN_VOID_TIME) {
				bool sunk = transform.position.y < -20;
				if (live)
					live.TakeDamage(sunk ? 50 : 10);
				if (power)
					power.ConsumeEnergy(sunk ? 5 : 2, out float trash);
			}
		}

		internal void slowTick() {
			ReikaKalseki.Ecocean.ECHooks.attractToSoundPing(this, true, 1);
		}

		private bool shouldSink() {
			return VanillaBiomes.VOID.isInBiome(transform.position.setY(-5)) || transform.position.y < -16; //once sunk stay sunk
		}

	}
}
