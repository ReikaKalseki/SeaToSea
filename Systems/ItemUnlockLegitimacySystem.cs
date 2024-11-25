﻿using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using UnityEngine.UI;

using FMOD;
using FMODUnity;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class ItemUnlockLegitimacySystem {
		
		public static readonly ItemUnlockLegitimacySystem instance = new ItemUnlockLegitimacySystem();
		
		private readonly List<ItemGate> data = new List<ItemGate>();
		private readonly Dictionary<TechType, ItemGate> keyedData = new Dictionary<TechType, ItemGate>();
		private bool anyLoaded = false;
		
		private ItemUnlockLegitimacySystem() {
	    	
		}
		
		internal void add(string mod, string item, Func<bool> valid) {
			add(mod, item, valid, (pp, ep) => {Inventory.main.DestroyItem(pp.GetTechType()); SoundManager.playSoundAt(SoundManager.buildSound("event:/tools/gravsphere/explode"), ep.transform.position);});
		}
		
		internal void add(string mod, string item, Func<bool> valid, Action<Pickupable, Player> take) {
			data.Add(new ItemGate(mod, item, valid, take));
		}
		
		internal void applyPatches() {
			foreach (ItemGate g in data) {
				anyLoaded |= g.load();
				keyedData[g.itemType] = g;
			}
		}
		
		internal void tick(Player ep) {
			if (anyLoaded) {
			    Pickupable pp = Inventory.main.GetHeld();
			    if (pp) {
			    	TechType tt = pp.GetTechType();
			    	if (keyedData.ContainsKey(tt)) {
			    		ItemGate ig = keyedData[tt];
			    		if (!ig.validityCheck.Invoke()) {
					    	ig.failureEffect.Invoke(pp, ep);
			    		}
			    	}
			    }
			}
		}

		class ItemGate {
			
			internal readonly string sourceMod;
			internal readonly string techTypeName;
			
			internal readonly Func<bool> validityCheck;
			internal readonly Action<Pickupable, Player> failureEffect;
			
			internal bool isModLoaded;
			internal TechType itemType;
			
			internal ItemGate(string s, string tt, Func<bool> condition, Action<Pickupable, Player> take) {
				sourceMod = s;
				techTypeName = tt;
				validityCheck = condition;
				failureEffect = take;
			}
			
			internal bool load() {
				isModLoaded = QModManager.API.QModServices.Main.ModPresent(sourceMod);
				itemType = tryFindItem();
				return itemType != TechType.None;
			}
		
			private TechType tryFindItem() {
				TechType tt = TechType.None;
				if (!TechTypeHandler.TryGetModdedTechType(techTypeName, out tt))
					if (!TechTypeHandler.TryGetModdedTechType(techTypeName.ToLowerInvariant(), out tt))
						TechTypeHandler.TryGetModdedTechType(techTypeName.setLeadingCase(false), out tt);
				if (tt == TechType.None && isModLoaded)
					SNUtil.log("Could not find TechType for '"+techTypeName+"' in mod '"+sourceMod+"'");
				return tt;
			}
			
		}

	}
	
}