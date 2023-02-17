using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class AzuriteBattery : CustomBattery {
		
		public AzuriteBattery() : base(SeaToSeaMod.itemLocale.getEntry("t2battery"), 750) {
			unlockRequirement = TechType.Unobtanium;//CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType;
		}

		public override Vector2int SizeInInventory {
			get {return new Vector2int(1, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			go.transform.localScale = new Vector3(1.2F, 1.2F, 1.5F);
			AzuriteSparker az = go.EnsureComponent<AzuriteSparker>();
			az.size = 0.67F;
			az.activityLevel = 0.5F;
			az.particleOrigin = new Vector3(0, 0, -0.05F);
			//go.EnsureComponent<AzuriteBatteryTag>();
		}
	}
		/*
	class AzuriteBatteryTag : MonoBehaviour {
		
		private GameObject sparker;
		
		void Update() {
			if (!sparker) {
				sparker = ObjectUtil.createWorldObject("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc");
				sparker.transform.localScale = new Vector3(0.4F, 0.4F, 0.4F);
				sparker.transform.parent = transform;
				sparker.transform.localPosition = new Vector3(0, 0, -0.05F);
				//sparker.transform.eulerAngles = new Vector3(325, 180, 0);
				ObjectUtil.removeComponent<DamagePlayerInRadius>(sparker);
				ObjectUtil.removeComponent<PlayerDistanceTracker>(sparker);
			}
			if (gameObject.FindAncestor<Player>()) {
				sparker.SetActive(false);
			}
			else if (UnityEngine.Random.Range(0, 20) == 0) {
				if (!sparker.activeSelf) {
					sparker.SetActive(true);
				}
				else if (UnityEngine.Random.Range(0, 2) == 0) {
					sparker.SetActive(false);
				}
			}
		}
		
	}*/
}
