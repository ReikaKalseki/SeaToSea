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
	
	public class GelFountain : Spawnable {
		
		private readonly XMLLocale.LocaleEntry locale;
		
		//public static TechType unlock { get; private set; }
		
		//private int FRAGMENT_COUNT;
	        
	    internal GelFountain(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
			OnFinishedPatching += () => {
				SaveSystem.addSaveHandler(ClassID, new SaveSystem.ComponentFieldSaveHandler<GelFountainTag>().addField("nextHarvestTime"));
			};
	    }
			
	    public override GameObject GetGameObject() {
			//prefab ideas
			//b71823a1-4fbc-42dd-aa3a-caa5809f1f6c
			//b5a62048-0577-4a85-a7bd-a1896fbc1357
			//db86ef34-e1fa-4eb2-aa18-dda5af30cb45
			//9966bd1d-8db4-492a-b8c6-1f5e075c1d5b
			//eca96e8f-0097-4627-b906-f454c329d9e5
			//VanillaFlora.BRAIN_CORAL.getRandomPrefab(true)
			//VanillaFlora.MUSHROOM_BUMP.getRandomPrefab(false)
			GameObject world = ObjectUtil.createWorldObject("1ce074ee-1a58-439b-bb5b-e5e3d9f0886f");
			world.EnsureComponent<TechTag>().type = TechType;
			PrefabIdentifier pi = world.EnsureComponent<PrefabIdentifier>();
			pi.ClassId = ClassID;
			world.EnsureComponent<GelFountainTag>();
			ObjectUtil.removeComponent<CoralBlendWhite>(world);
			ObjectUtil.removeComponent<Light>(world);
			//ObjectUtil.removeComponent<IntermittentInstantiate>(world);
			//ObjectUtil.removeComponent<BrainCoral>(world);
			ObjectUtil.removeComponent<LiveMixin>(world);
			ObjectUtil.removeComponent<Pickupable>(world);
			ObjectUtil.removeComponent<ResourceTracker>(world);
			ObjectUtil.makeMapRoomScannable(world, TechType);
			ObjectUtil.removeComponent<Rigidbody>(world);
			ObjectUtil.removeComponent<WorldForces>(world);
			BoxCollider bc = world.GetComponent<BoxCollider>();
			bc.size = Vector3.Scale(bc.size, new Vector3(1.5F, 1.5F, 4.0F));
			//ObjectUtil.removeComponent<GoalObject>(world);
			//ObjectUtil.removeChildObject(world, "EmitPoint");
			Renderer r = world.GetComponentInChildren<Renderer>();
			r.transform.localScale = new Vector3(2, 2, 5);
			r.transform.localPosition = Vector3.up*-0.1F;
			r.materials[0].SetFloat("_Shininess", 0F);
			r.materials[0].SetFloat("_SpecInt", 1F);
			r.materials[0].SetColor("_GlowColor", Color.white);
			//r.transform.localEulerAngles = new Vector3(-90, 0, 0);
			//world.GetComponentInChildren<Animator>().speed *= 0.25F;
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			
			RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/GelFountain");
			RenderUtil.setEmissivity(r, 2);
			
			world.EnsureComponent<ImmuneToPropulsioncannon>().immuneToRepulsionCannon = true;
			
			if (false && !SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
				ResourceTracker rt = world.EnsureComponent<ResourceTracker>();
				rt.techType = TechType;
				rt.overrideTechType = TechType;
				rt.prefabIdentifier = pi;
			}
			
			return world;
	    }
		
		public void register() {
			Patch();
		}
		
		public void postRegister() {			
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.scanTime = 10;
			e.locked = true;
			PDAManager.PDAPage page = PDAManager.createPage("ency_"+ClassID, FriendlyName, locale.pda, "PlanetaryGeology");
			page.setHeaderImage(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/PDA/GelFountain"));
			page.register();
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
			
			GenUtil.registerPrefabWorldgen(this, false, BiomeType.UnderwaterIslands_IslandCaveWall, 1, 3.0F);
		}
			
	}
		
	class GelFountainTag : MonoBehaviour, IHandTarget {
		
		private Renderer render;
		
		private float nextHarvestTime;
		private float nextDripTime;
		
		private bool hasDoneCaveCheck;
		private float nextCaveCheck = 10;
		
		void Update() {
			if (!render)
				render = GetComponentInChildren<Renderer>();
			float time = DayNightCycle.main.timePassedAsFloat;
			float h = getHarvestReadiness();
			if (time >= nextDripTime && hasDoneCaveCheck) {
				spawn(true);
				float f = UnityEngine.Random.Range(0.5F, 2)*(h > 0 ? Mathf.Min(5, 1F/h) : 5);
				nextDripTime = time+f;
			}
			
			if (time > nextCaveCheck) {
				Vector3 vec = transform.up;
				Ray ray = new Ray(transform.position, vec);
				if (UWE.Utils.RaycastIntoSharedBuffer(ray, 45, Voxeland.GetTerrainLayerMask()) > 0) {
					RaycastHit hit = UWE.Utils.sharedHitBuffer[0];
					if (hit.transform == null || hit.distance < 5) {
						UnityEngine.Object.Destroy(this.gameObject);
						return;
					}
				}
				hasDoneCaveCheck = true;
				nextCaveCheck = time+10;
			}
			
			RenderUtil.setEmissivity(render, h*h);
			render.transform.localScale = new Vector3(2, 2, 3+2*h);
		}
		
		private float getHarvestReadiness() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time >= nextHarvestTime)
				return 1;
			float diff = nextHarvestTime-time;
			if (diff >= 1200)
				return 0;
			return (float)MathUtil.linterpolate(diff, 0, 1200, 1, 0, true);
		}
		
		public void onKnifed() {
			if (tryHarvest()) {
				
			}
			
		}
		
		private bool tryHarvest() {
			if (getHarvestReadiness() >= 1) {
				spawn(false);
				nextHarvestTime = DayNightCycle.main.timePassedAsFloat+UnityEngine.Random.Range(3600, 7200); //1-2h
				return true;
			}
			return false;
		}
		
		private void spawn(bool drip) {
			GameObject go = ObjectUtil.createWorldObject(drip ? SeaToSeaMod.geogelDrip.ClassID : SeaToSeaMod.geogel.ClassID);
			ObjectUtil.fullyEnable(go);
			ObjectUtil.ignoreCollisions(go, gameObject);
			go.transform.position = transform.position+transform.up*0.5F;
			Rigidbody rb = go.GetComponent<Rigidbody>();
			rb.isKinematic = false;
			Vector3 vec = MathUtil.getRandomVectorAround(transform.up.normalized, 0.5F)*3;
			rb.AddForce(vec, ForceMode.VelocityChange);
			LargeWorldStreamer.main.MakeEntityTransient(go);
		}
		
		void OnScanned() {
			
		}
		
		public void OnHandHover(GUIHand hand) {
			float h = getHarvestReadiness();
			HandReticle.main.SetProgress(h);
			HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
			HandReticle.main.SetTargetDistance(8);
			if (h < 1) {
				HandReticle.main.SetInteractText("GelFountainRecharging");
			}
			else {
			   	HandReticle.main.SetInteractText("GelFountainClick");
			}
		}
	
		public void OnHandClick(GUIHand hand) {
			
		}
		
	}
}
