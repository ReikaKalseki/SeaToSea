using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class EmperorRootOil : WorldCollectedItem {
		
		public static readonly float LIFESPAN = 120; //was 90
		private static float lastInventoryTickTime;
	        
	    internal EmperorRootOil(XMLLocale.LocaleEntry e) : base(e, "18229b4b-3ed3-4b35-ae30-43b1c31a6d8d") {
			sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/EmperorRootOil");
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<EmperorRootOilTag>().addField("pickupTime"));
			};
	    }
			
	    public override void prepareGameObject(GameObject go, Renderer[] rr) {
			foreach (Renderer r in rr)
				setupRendering(r, true);
			go.EnsureComponent<EmperorRootOilTag>();
	    }
			
	    public static void setupRendering(Renderer r, bool light) {
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Items/World/EmperorRootOil", new Dictionary<int, string>{{0, "Shell"}, {1, "Inner"}});
			if (light) {
				Light l = ObjectUtil.addLight(r.gameObject);
				l.color = new Color(0.2F, 0.5F, 1F);
				l.intensity = 2;
				l.range = 2F;
			}
	    }
		
		public static void tickInventory(Player ep, float time) {
			if (time-lastInventoryTickTime >= 1) {
				InventoryUtil.forEachOfType(Inventory.main.container, C2CItems.emperorRootOil.TechType, ii => {
					EmperorRootOilTag tag = ii.item.GetComponent<EmperorRootOilTag>();
					if (tag && tag.pickupTime > -1 && time-tag.pickupTime >= LIFESPAN) {
						InventoryUtil.forceRemoveItem(Inventory.main.container, ii);
					}
				});
				lastInventoryTickTime = time;
			}
		}
		
		internal class EmperorRootOilTag : MonoBehaviour {
			
			public float pickupTime = -1;
			
		}
			
	}
}
