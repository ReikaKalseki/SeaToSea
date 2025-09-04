using System;
using System.Collections.Generic;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {
	public sealed class SeamothVoidStealthModule : SeamothModule {

		internal static readonly IrreplaceableItemRegistry.IrreplaceableItemData lossData = new IrreplaceableItemRegistry.IrreplaceableItemData(
			IrreplaceableItemRegistry.DEFAULT_EFFECTS.onAttemptToDrop,
			IrreplaceableItemRegistry.DEFAULT_EFFECTS.onDiedWhileHolding,
			(v, m, ii, li) => {
				if (m) {
					GameObject go = Utils.PlayOneShotPS(ObjectUtil.lookupPrefab(VanillaCreatures.CRASHFISH.prefab).GetComponent<Crash>().detonateParticlePrefab, v.transform.position, v.transform.rotation);
					go.transform.localScale = Vector3.one*(v.transform.position-Player.main.transform.position).magnitude;
					Player.main.gameObject.EnsureComponent<DelayedKill>().initialize(v == Player.main.GetVehicle() ? 0.05F : 0.5F, DamageType.Explosive);
				}
				else {
					IrreplaceableItemRegistry.DEFAULT_EFFECTS.onLostWithVehicle.Invoke(v, m, ii, li);
				}
			}
		); //to destroy the seamoth and cause the shockwave from it overcharging just destroy the seamoth, which will trigger this because of the IrreplaceableItemRegistry

		public SeamothVoidStealthModule() : base(SeaToSeaMod.itemLocale.getEntry("SeamothVoidStealth")) {
			this.preventNaturalUnlock();
		}

		public override QuickSlotType QuickSlotType {
			get {
				return CraftData.GetQuickSlotType(TechType.SeamothSonarModule);
			}
		}
		public override float getUsageCooldown() {
			return 5;
		}

		public override void onFired(SeaMoth sm, int slotID, float charge) {
			sm.GetComponent<C2CMoth>().dumpSoundEnergy();
		}

		public override Vector2int SizeInInventory {
			get {
				return new Vector2int(2, 2);
			}
		}
		/*
		protected override Atlas.Sprite GetItemSprite()
		{
			return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
		}*/
	}
}
