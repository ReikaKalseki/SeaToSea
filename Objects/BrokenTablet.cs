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
				CustomLocaleKeyDatabase.registerKey(d.TechType.AsString(), "Broken "+Language.main.Get(d.tablet));
				CustomLocaleKeyDatabase.registerKey("Tooltip_"+d.TechType.AsString(), "A shattered "+Language.main.Get(d.tablet)+". Not very useful directly.");
				SNUtil.log("Relocalized broken tablet "+d+" > "+d.tablet.AsString()+" > "+Language.main.Get(d.TechType), SNUtil.diDLL);
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
			//GameObject sparker = ObjectUtil.createWorldObject("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc");
			tabRef.SetActive(false);
			if (world != null) {
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				Material m1a = tabRef.GetComponentInChildren<MeshRenderer>().materials[1];
				foreach (Renderer r in world.GetComponentsInChildren<Renderer>()) {
					if (r.materials.Length != 2) //any other renderers, like the VFXVoluLight added on purple and thus inherited here
						continue;
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
				//fetch existing light, added by C2CHooks skyapplier for purple
				Light l = world.GetComponentInChildren<Light>();
				FlickeringLight f = l.GetComponent<FlickeringLight>();
				switch(tablet) {
					case TechType.PrecursorKey_Orange: {
						l.intensity = 0.4F;
						l.range = 18F;
						l.color = new Color(1F, 0.63F, 0F, 1);
						l.transform.localPosition = new Vector3(0, 0.03F, 0);
						l.shadows = LightShadows.Soft;
						f.dutyCycle = 0.8F;
						f.updateRate = 0.67F;
						f.fadeRate = 8F;
						break;
					}
					case TechType.PrecursorKey_Red: {
						l.intensity = 1F;
						l.range = 10F;
						l.color = new Color(1F, 0.33F, 0.33F, 1);
						l.transform.localPosition = new Vector3(-0.25F, 0.3F, 0);
						f.dutyCycle = 0.67F;
						f.updateRate = 0.25F;
						f.fadeRate = 5F;
						break;
					}
					case TechType.PrecursorKey_White: {
						l.intensity = 0.9F;
						l.range = 45F;
						l.color = new Color(216F/255F, 247F/255F, 1F, 1);
						l.shadows = LightShadows.Soft;
						l.transform.localPosition = new Vector3(0, 1.25F, 0);
						f.dutyCycle = 0.3F;
						f.updateRate = 0.08F;
						f.fadeRate = 500F;
						break;
					}
				}/*
				sparker.transform.SetParent(world.transform);
				ObjectUtil.removeChildObject(sparker, "ElecLight");
				ObjectUtil.removeChildObject(sparker, "xElec");
				foreach (ParticleSystemRenderer r in sparker.GetComponentsInChildren<ParticleSystemRenderer>()) {
					foreach (Material m in r.materials)
						m.SetColor("_Color", l.color);
				}
				ObjectUtil.removeComponent<DamagePlayerInRadius>(sparker);
				*/
				return world;
			}
			else {
				SNUtil.writeToChat("Could not fetch template GO for "+this);
				return null;
			}
	    }
			
	}
}
