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
		
		private int FRAGMENT_COUNT;
	        
	    internal GeyserCoral(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
			
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<GeyserCoralTag>().addField("scanned"));
			};
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = new GameObject(ClassID);
			world.EnsureComponent<TechTag>().type = TechType;
			PrefabIdentifier pi = world.EnsureComponent<PrefabIdentifier>();
			pi.ClassId = ClassID;
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			world.EnsureComponent<GeyserCoralTag>();
			if (!SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
				ResourceTracker rt = world.EnsureComponent<ResourceTracker>();
				rt.techType = TechType;
				rt.overrideTechType = TechType;
				rt.prefabIdentifier = pi;
			}
			world.layer = LayerID.Useable;
			
			//rotate to 270, 0, 0
			
			world.EnsureComponent<ImmuneToPropulsioncannon>().immuneToRepulsionCannon = true;
			
			Light l = ObjectUtil.addLight(world);
			l.intensity = 0.75F;
			l.range = 6F;
			FlickeringLight f = l.gameObject.EnsureComponent<FlickeringLight>();
			f.dutyCycle = 0.8F;
			f.updateRate = 0.25F;
			f.fadeRate = 2F;
			l.transform.localPosition = Vector3.up*1.2F;
						
			return world;
	    }
		
		public void register() {
			Patch();
		}
		
		public void postRegister() {
			PDAManager.PDAPage page = PDAManager.createPage("ency_"+ClassID, FriendlyName, locale.pda, "Lifeforms/Coral");
			page.setHeaderImage(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/PDA/GeyserCoral"));
			page.register();
			TechType unlock = C2CItems.geyserFilter.TechType;
        	KnownTechHandler.Main.SetAnalysisTechEntry(TechType, new List<TechType>(){unlock});
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.blueprint = unlock;
			e.destroyAfterScan = false;
			e.locked = true;
			FRAGMENT_COUNT = SeaToSeaMod.worldgen.getCount(ClassID)-1;
			e.totalFragments = FRAGMENT_COUNT;
			SNUtil.log("Found "+e.totalFragments+" "+ClassID+" to use as fragments", SeaToSeaMod.modDLL);
			e.isFragment = true;
			e.scanTime = 3;
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
		}
		
		public int getFragmentCount() {
			return FRAGMENT_COUNT;
		}
			
	}
		
	class GeyserCoralTag : MonoBehaviour {
		
		private bool scanned = false;
		private float scannedFade = 0;
		
		private GameObject plateHolder;
		private List<GameObject> plates = new List<GameObject>();
		
		private Light light;
		private FlickeringLight flicker;
			
		private Renderer[] render = null;
		
		private bool didTerrainCheck;
		private float age;
		
		private bool isHot;
		
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (!plateHolder)
				clear();
			if (!light)
				light = GetComponentInChildren<Light>();
			if (!flicker)
				flicker = GetComponentInChildren<FlickeringLight>();
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
			if (isHot) {
				flicker.maxIntensity = 1.25F; //from 0.75
				flicker.fadeRate = 4F;
				flicker.updateRate = 0.2F;
				light.range = 3F;
			}
			light.color = hot ? new Color(1F, 0.4F, 0.0F, 1) : new Color(0.2F, 0.7F, 1F, 1);
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
				float f = 100+(20+scannedFade*80)*Mathf.Sin(time*(0.617F+scannedFade*1.4F)+(r.transform.position.magnitude*10)%1781);
				RenderUtil.setEmissivity(r, f*(0.25F+Mathf.Max(0.25F, 0.75F*flicker.currentIntensity/flicker.maxIntensity)));
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
    		SNUtil.addBlueprintNotification(C2CItems.geyserFilter.TechType);
		}
		
	}
}
