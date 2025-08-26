using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	internal class C2CStasisRifle : CustomGrindable, ReactsOnDrilled {

		private float nextSphereTime = -1;

		public override GameObject chooseRandomResource() {
			return null; //unused as not recyclable
		}

		public void onDrilled(Vector3 pos) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time >= nextSphereTime) {
				StasisSphere ss = WorldUtil.createStasisSphere(pos, 2, 0.5F);
				SoundManager.playSoundAt(this.GetComponent<StasisRifle>().fireSound, pos);
				Utils.PlayOneShotPS(ObjectUtil.lookupPrefab(VanillaCreatures.CRASHFISH.prefab).GetComponent<Crash>().detonateParticlePrefab, transform.position, transform.rotation);
				nextSphereTime = time + ss.getLifespan();
			}
		}

		public override bool isRecyclable() {
			return false;
		}

	}
}
