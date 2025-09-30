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

	public class PowerSealModuleFragment : Spawnable {

		internal PowerSealModuleFragment() : base("powersealmodulefragment", C2CItems.powerSeal.FriendlyName, C2CItems.powerSeal.Description) {

		}

		public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(C2CItems.powerSeal.ClassID);
			world.EnsureComponent<TechTag>().type = TechType;
			PrefabIdentifier pi = world.EnsureComponent<PrefabIdentifier>();
			pi.ClassId = ClassID;
			world.removeComponent<WorldForces>();
			world.removeComponent<Pickupable>();
			//world.removeComponent<ResourceTracker>();
			ResourceTracker rt = world.EnsureComponent<ResourceTracker>();
			rt.techType = TechType.Fragment;
			rt.overrideTechType = TechType.Fragment;
			rt.prefabIdentifier = pi;
			world.GetComponent<Rigidbody>().isKinematic = true;
			world.EnsureComponent<BrokenModule>();
			Renderer r = world.GetComponentInChildren<Renderer>();
			return world;
		}

		public void register() {
			this.Patch();
			KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>() { C2CItems.powerSeal.TechType });
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = C2CItems.powerSeal.TechType;
			e.destroyAfterScan = true;
			e.locked = true;
			e.totalFragments = 1;
			e.isFragment = true;
			e.scanTime = 8;
			PDAHandler.AddCustomScannerEntry(e);
		}

	}

	class BrokenModule : MonoBehaviour {

		private VFXController sparker;

		private bool isSparking;

		void Update() {
			if (!sparker) {
				GameObject welder = ObjectUtil.createWorldObject("9ef36033-b60c-4f8b-8c3a-b15035de3116", false, false);
				sparker = welder.GetComponent<Welder>().fxControl.clone();
				sparker.transform.parent = transform;
				sparker.transform.localPosition = new Vector3(0, -0.05F, 0);
				sparker.transform.eulerAngles = new Vector3(325, 180, 0);
				sparker.gameObject.SetActive(true);
			}
			transform.localScale = new Vector3(1, 1.3F, 1);
			if (UnityEngine.Random.Range(0, 30) == 0) {
				if (isSparking)
					sparker.StopAndDestroy(0);
				else
					sparker.Play(0);
				isSparking = !isSparking;
			}/*
			if (UnityEngine.Random.Range(0, 5) == 0) { //prevent burying under a resource
				RaycastHit[] hit = Physics.SphereCastAll(gameObject.transform.position, 0.25F, new Vector3(1, 1, 1), 0.25F);
				foreach (RaycastHit rh in hit) {
					if (rh.transform != null && rh.transform.gameObject) {
						Pickupable p = rh.transform.gameObject.GetComponent<Pickupable>();
						if (p) {
							p.gameObject.destroy();
						}
					}
				}
			}*/
		}

	}
}
