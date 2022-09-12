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
	
	public class BrokenTablet : Spawnable {
		
		public readonly TechType tablet;
		
		private static readonly List<BrokenTablet> tablets = new List<BrokenTablet>();
	        
		internal BrokenTablet(TechType tt) : base(generateName(tt), "Broken "+tt.AsString(), "Pieces of "+tt.AsString()) {
			tablet = tt;
			tablets.Add(this);
	    }
		
		private static string generateName(TechType tech) {
			string en = Enum.GetName(typeof(TechType), tech);
			return "brokentablet_"+en.Substring(en.LastIndexOf('_')+1);
		}
		
		public static void updateLocale() {
			foreach (BrokenTablet d in tablets) {
				Language.main.strings[d.TechType.AsString()] = "Broken "+Language.main.strings[d.tablet.AsString()];
				Language.main.strings["Tooltip_"+d.TechType.AsString()] = "A shattered "+Language.main.strings[d.tablet.AsString()]+". Not very useful directly.";
			}
		}
		
		public void register() {
			Patch();
			GameObject tabPfb = ObjectUtil.lookupPrefab(CraftData.GetClassIdForTechType(tablet));
			//tabPfb.SetActive(false);
	    	VFXFabricating fab = ObjectUtil.getChildObject(tabPfb, "Model").EnsureComponent<VFXFabricating>();
	    	fab.localMaxY = 0.1F;
	    	fab.localMinY = -0.1F;
        	KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>(){tablet});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = tablet;
			e.destroyAfterScan = false;
			e.locked = true;
			e.totalFragments = 1;
			e.isFragment = true;
			e.scanTime = tablet == TechType.PrecursorKey_Orange ? 10 : 15;
			//e.encyclopedia = Enum.GetName(typeof(TechType), tablet);//PDAScanner.mapping.ContainsKey(tablet) ? PDAScanner.mapping[tablet].encyclopedia : null;
			PDAHandler.AddCustomScannerEntry(e);
		}
			
	    public override GameObject GetGameObject() {
			GameObject tabRef = ObjectUtil.lookupPrefab(CraftData.GetClassIdForTechType(tablet));
			GameObject world = ObjectUtil.createWorldObject("83b61f89-1456-4ff5-815a-ecdc9b6cc9e4", true, false);
			tabRef.SetActive(false);
			if (world != null) {
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				Material m1a = tabRef.GetComponentInChildren<Renderer>().materials[1];
				foreach (Renderer r in world.GetComponentsInChildren<Renderer>()) {
					int idx = 0;
					Material m1b = r.materials[1];
					foreach (Material m in r.materials) {
						foreach (string tex in m.GetTexturePropertyNames()) {
							m1b.SetTexture(tex, m1a.GetTexture(tex));
							m1b.SetTextureOffset(tex, m1a.GetTextureOffset(tex));
							m1b.SetTextureScale(tex, m1a.GetTextureScale(tex));
						}
						idx++;
					}
				}
				return world;
			}
			else {
				SNUtil.writeToChat("Could not fetch template GO for "+this);
				return null;
			}
	    }
			
	}
}
