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
		
		private readonly TechType tablet;
	        
		internal BrokenTablet(TechType tt) : base(generateName(tt), "Broken "+tt.AsString(), "Pieces of "+tt.AsString()) { //TODO name needs fixing
			tablet = tt;
	    }
		
		private static string generateName(TechType tech) {
			string en = Enum.GetName(typeof(TechType), tech);
			return "brokentablet_"+en.Substring(en.LastIndexOf('_')+1);
		}
		
		public void register() {
			Patch();
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
			GameObject tabRef = null;
			if (UWE.PrefabDatabase.TryGetPrefab(CraftData.GetClassIdForTechType(tablet), out tabRef)) {
				GameObject world = SBUtil.createWorldObject("83b61f89-1456-4ff5-815a-ecdc9b6cc9e4", true, false);
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
					SBUtil.writeToChat("Could not fetch template GO for "+this);
					return null;
				}
			}
			else {
				SBUtil.writeToChat("Could not fetch tablet GO for "+this);
				return null;
			}
	    }
			
	}
}
