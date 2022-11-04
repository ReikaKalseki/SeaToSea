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
	
	public class SeaTreaderTunnelLocker : Spawnable {
		
		internal static readonly Dictionary<TechType, int> itemList = new Dictionary<TechType, int>();
		
		static SeaTreaderTunnelLocker() {
			addItem(TechType.Titanium, 3);
			addItem(TechType.AluminumOxide, 5);
			addItem(TechType.Diamond, 6);
			addItem(TechType.Gold, 2);
			addItem(TechType.Titanium, 4);
			
			addItem(TechType.CuredSpadefish, 4);
			addItem(TechType.CookedBladderfish, 3);
			
			addItem(TechType.FirstAidKit, 1);
			
			addItem(TechType.SmallMelon, 2);
		}
	        
	    internal SeaTreaderTunnelLocker() : base("SeaTreaderTunnelLocker", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("775feb4c-dab9-4322-b4a5-a4289ca1cf6a");
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			world.EnsureComponent<SeaTreaderTunnelLockerTag>();
			return world;
	    }
		
		public static void addItem(TechType item, int amt) {
			if (itemList.ContainsKey(item))
				itemList[item] = itemList[item]+amt;
			else
				itemList[item] = amt;
		}
			
	}
		
	class SeaTreaderTunnelLockerTag : MonoBehaviour {
		
		private StorageContainer container;
		
		void Update() {
			if (!container) {
				container = GetComponent<StorageContainer>();
			}
			if (container) {
				foreach (KeyValuePair<TechType, int> kvp in SeaTreaderTunnelLocker.itemList) {
					int has = container.container.GetCount(kvp.Key);
					if (has < kvp.Value) {
						for (int i = 0; i < kvp.Value-has; i++) {
							GameObject go = CraftData.GetPrefabForTechType(kvp.Key);
							go = UnityEngine.Object.Instantiate(go);
							go.SetActive(false);
							container.container.AddItem(go.GetComponent<Pickupable>());
						}
					}
				}
			}
		}
		
	}
}
