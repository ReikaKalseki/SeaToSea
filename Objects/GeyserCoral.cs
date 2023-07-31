using System;
using System.IO;
using System.Linq;
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
	
	public class GeyserCoral : Spawnable {
		
		private readonly XMLLocale.LocaleEntry locale;
		
		//public static readonly Color GLOW_COLOR = new Color(1, 112/255F, 0, 1);
	        
	    internal GeyserCoral(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
			
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<GeyserCoralTag>().addField("scanned"));
			};
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = new GameObject(ClassID);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			world.EnsureComponent<GeyserCoralTag>();
			world.layer = LayerID.Useable;
			
			//rotate to 270, 0, 0
			
			world.EnsureComponent<ImmuneToPropulsioncannon>().immuneToRepulsionCannon = true;
			
			return world;
	    }
		
		public void register() {
			Patch();
		}
		
		public void postRegister() {
			TechType unlock = SeaToSeaMod.geyserFilter.TechType;
        	KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>(){unlock});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = unlock;
			e.destroyAfterScan = false;
			e.locked = true;
			e.totalFragments = SeaToSeaMod.worldgen.getCount(ClassID);
			SNUtil.log("Found "+e.totalFragments+" "+ClassID+" to use as fragments", SeaToSeaMod.modDLL);
			e.isFragment = true;
			e.scanTime = 3;
			PDAHandler.AddCustomScannerEntry(e);
		}
			
	}
		
	class GeyserCoralTag : MonoBehaviour {
		
		private bool scanned = false;
		private float scannedFade = 0;
		
		private GameObject plateHolder;
		private List<GameObject> plates = new List<GameObject>();
			
		private Renderer[] render = null;
		
		private bool didTerrainCheck;
		private float age;
		
		private bool isHot;
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (!plateHolder)
				clear();
			if (!plateHolder)
				plateHolder = ObjectUtil.getChildObject(gameObject, "plateHolder");
			if (!plateHolder)
				plateHolder = new GameObject("plateHolder");
			plateHolder.transform.SetParent(transform);
			plateHolder.transform.localRotation = Quaternion.Euler(270, 0, 0);
			plateHolder.transform.localPosition = Vector3.zero;
			int targetCount = (int)(transform.position.magnitude)%3+3; //3-5
			if (plates.Count == 0) {
				foreach (Transform t in plateHolder.transform) {
					plates.Add(t.gameObject);
				}
			}
			age += Time.deltaTime;
			if (plates.Count != targetCount) {		
				clear();
				UnityEngine.Random.InitState((int)(transform.position.magnitude));
				while (plates.Count < targetCount) {
					GameObject pfb = ObjectUtil.lookupPrefab(VanillaFlora.TABLECORAL_ORANGE.getRandomPrefab(false));
					int i = plates.Count;
					GameObject plate = new GameObject("plate"+i);
					GameObject model = UnityEngine.Object.Instantiate(pfb.GetComponentInChildren<Renderer>().gameObject);
					GameObject collider = UnityEngine.Object.Instantiate(pfb.GetComponentInChildren<BoxCollider>().gameObject);
					model.transform.SetParent(plate.transform);
					collider.transform.SetParent(plate.transform);
					
					plate.transform.SetParent(plateHolder.transform);
					float d = -0.5F+0.25F*i;
					plate.transform.localPosition = new Vector3(0, d+UnityEngine.Random.Range(-0.05F, 0.05F), 0);
					plate.transform.localRotation = Quaternion.identity;//Quaternion.Euler(270, 0, 0);
					plate.transform.localScale = Vector3.one;
					plates.Add(plate);
				}
			
				render = null;
			}
			bool hot = VanillaBiomes.KOOSH.isInBiome(transform.position) || WaterTemperatureSimulation.main.GetTemperature(transform.position) > 40;
			bool retexture = isHot != hot;
			isHot = hot;
			if (render == null) {
				render = GetComponentsInChildren<Renderer>();
				retexture = true;
			}
			if (retexture) {
				foreach (Renderer r2 in render) {
					RenderUtil.swapTextures(SeaToSeaMod.modDLL, r2, isHot ? "Textures/GeyserCoral" : "Textures/GeyserCoral2");
					RenderUtil.setGlossiness(r2, 6, -200, -10);
					RenderUtil.setEmissivity(r2, 120);
					r2.material.SetColor("_Color", Color.white);
					r2.material.SetColor("_SpecColor", Color.white);
					r2.material.SetColor("_GlowColor", Color.white);
				}
			}
			
			if (scanned)
				scannedFade = Mathf.Min(scannedFade+0.5F*Time.deltaTime, 1);
			
			if (!didTerrainCheck && age >= 2 && Vector3.Distance(Player.main.transform.position, transform.position) < 120) {
				didTerrainCheck = true;
				bool any = false;
				UnityEngine.Random.InitState((int)(transform.position.magnitude));
				foreach (GameObject go in plates) {
					RaycastHit? hit = WorldUtil.getTerrainVectorAt(go.transform.position+transform.up*1.5F, 4, -transform.up);
					if (hit.HasValue) {
						go.SetActive(true);
						go.transform.forward = hit.Value.normal;
						go.transform.position = hit.Value.point+go.transform.forward*-0.05F;
						//go.transform.up = hit.Value.normal;
						go.transform.Rotate(new Vector3(0, 0, UnityEngine.Random.Range(-15F, 15F)), Space.Self);
						any = true;
					}
					else {
						go.SetActive(false);
					}
				}
				if (!any) {
					transform.position = transform.position+transform.up*0.1F;
					didTerrainCheck = false;
				}
			}
			
			foreach (Renderer r in render) {
				float f = 100+(20+scannedFade*40)*Mathf.Sin(time*0.617F+(r.transform.position.magnitude*10)%1781);
				RenderUtil.setEmissivity(r, f);
			}
		}
		
		void clear() {
			foreach (GameObject go in plates) {
				UnityEngine.Object.DestroyImmediate(go);
			}
			plates.Clear();
		}
		
		void OnScanned() {
			scanned = true;
		}
		
	}
}
