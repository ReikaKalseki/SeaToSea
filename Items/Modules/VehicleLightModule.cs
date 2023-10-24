using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class VehicleLightModule : CustomEquipable {
				
		public VehicleLightModule() : base(SeaToSeaMod.itemLocale.getEntry("VehicleLightBonus"), "d290b5da-7370-4fb8-81bc-656c6bde78f8") {
			preventNaturalUnlock();
		}
		
		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.VehicleModule;
			}
		}

		public override QuickSlotType QuickSlotType {
			get {
				return QuickSlotType.Passive;
			}
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.SeamothUpgrades;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"VehicleModules"};
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.VehicleUpgrades;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.VehicleUpgrades;
			}
		}
			
		public static GameObject createBonusLight(GameObject orig, bool isPrawn) {
			GameObject go = UnityEngine.Object.Instantiate(orig);
			go.transform.SetParent(orig.transform.parent);
			go.transform.position = orig.transform.position;
			go.transform.rotation = orig.transform.rotation;
			go.transform.localScale = orig.transform.localScale;
			Light l = go.GetComponent<Light>();
			l.color = Color.Lerp(l.color, Color.white, 0.5F);//new Color(0.75F, 0.95F, 1);
			l.range *= isPrawn ? 3F : 1.5F;
			l.intensity *= isPrawn ? 2.5F : 1.5F;
			l.spotAngle *= 1.5F;
			l.innerSpotAngle *= 1.5F;
			ObjectUtil.removeChildObject(go, "x_FakeVolumletricLight");
			return go;
		}
	}
}
