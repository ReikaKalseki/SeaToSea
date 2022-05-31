using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class SealedSuit : CustomEquipable {
		
		public SealedSuit() : base(SeaToSeaMod.locale.getEntry("SealedSuit"), "WorldEntities/Tools/Stillsuit") {
			
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			UnityEngine.Object.Destroy(go.GetComponent<Stillsuit>());
			foreach (PDANotification pda in go.GetComponents<PDANotification>()) {
				SBUtil.writeToChat(pda.text);
				UnityEngine.Object.Destroy(pda);
			}
			SBUtil.writeToChat(string.Join(", ", (object[])go.GetComponents<PDANotification>()));
		}

		public override TechType RequiredForUnlock {
			get {
				return TechType.Kyanite;
			}
		}
		
		public override sealed EquipmentType EquipmentType {
			get {
				return EquipmentType.Body;
			}
		}
	}
}
