using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class VoidGhostLeviathanSystem {
		
		public static readonly VoidGhostLeviathanSystem instance = new VoidGhostLeviathanSystem();
	    
	    private static GameObject voidLeviathan;
	    private static GameObject redirectedTarget;
	    
	    private static readonly double MAXDEPTH = 2000;//800;
		
		private VoidGhostLeviathanSystem() {
			
		}
	    
	    public void deleteVoidLeviathan() {
	    	if (voidLeviathan != null)
	    		UnityEngine.Object.DestroyImmediate(voidLeviathan);
	    	voidLeviathan = null;
	    }
	    
	    public void deleteGhostTarget() {
	    	if (redirectedTarget != null)
	    		UnityEngine.Object.DestroyImmediate(redirectedTarget);
	    	redirectedTarget = null;
	    }
	    
	    private GameObject getOrCreateTarget() {
	    	if (redirectedTarget == null) {
	    		redirectedTarget = null;
	    	}
	    	return redirectedTarget;
	    }
    
	    public bool isSpawnableVoid(string biome) {
	    	if (VoidSpikesBiome.instance.isPlayerInLeviathanZone() && voidLeviathan != null && voidLeviathan.activeInHierarchy) {
	    		return false;
	    	}
	    	Player ep = Player.main;
	    	bool edge = string.Equals(biome, "void", StringComparison.OrdinalIgnoreCase);
	    	bool far = string.IsNullOrEmpty(biome);
	    	if (VoidSpikesBiome.instance.getDistanceToBiome(ep.transform.position) <= VoidSpikesBiome.biomeVolumeRadius+25)
	    		far = true;
	    	if (!far && !edge)
	    		return false;
	    	if (ep.inSeamoth) {
	    		SeaMoth sm = (SeaMoth)ep.GetVehicle();
	    		double ch = getAvoidanceChance(ep, sm, edge, far);
	    		//SNUtil.writeToChat(ch+" @ "+sm.transform.position);
	    		if (ch > 0 && (ch >= 1 || UnityEngine.Random.Range(0F, 1F) <= ch)) {
		    		foreach (int idx in sm.slotIndexes.Values) {
		    			InventoryItem ii = sm.GetSlotItem(idx);
		    			if (ii != null && ii.item.GetTechType() != TechType.None && ii.item.GetTechType() == SeaToSeaMod.voidStealth.TechType) {
	    					//SNUtil.writeToChat("Avoid");
		    				return false;
		    			}
		    		}
	    			//SNUtil.writeToChat("Tried and failed");
	    		}
	    	}
	    	return true;
	    }
	    
	    public GameObject getVoidLeviathan(VoidGhostLeviathansSpawner spawner, Vector3 pos) {
	    	GameObject go = UnityEngine.Object.Instantiate<GameObject>(spawner.ghostLeviathanPrefab, pos, Quaternion.identity);
	    	if (VoidSpikesBiome.instance.isPlayerInLeviathanZone()) {
	    		GameObject orig = go;
			 	//GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.lookupPrefab("e82d3c24-5a58-4307-a775-4741050c8a78").transform.Find("model").gameObject);
			 	//mdl.transform.localPosition = Vector3.zero;
			 	
			 	AssetBundle ab = ReikaKalseki.DIAlterra.AssetBundleManager.getBundle(SeaToSeaMod.modDLL, "voidspike");
			 	GameObject bdl = ab.LoadAsset<GameObject>("VoidSpikeLevi_FixedRig");
			 	//ObjectUtil.dumpObjectData(bdl);
			 	Mesh tryM = ab.LoadAsset<Mesh>("Ghost_Leviathan_geo.001");
			 	if (tryM == null) {
			 		SNUtil.log("Direct fetch not found");
			 		System.Object[] all = ab.LoadAllAssets();
			 		tryM = all.Length >= 4 ? all[3] as Mesh : null;
			 	}
			 	if (tryM == null) {
			 		SNUtil.log("Index fetch not found");
			 		SkinnedMeshRenderer[] smrs = bdl.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			 		if (smrs.Length != 1)
			 			SNUtil.log("Wrong number of SMRs for mesh: "+string.Join(", ", (object[])smrs));
			 		else
			 			tryM = smrs[0].sharedMesh;
			 	}
			 	if (true)
			 		tryM = bdl.GetComponentsInChildren<SkinnedMeshRenderer>(true)[0].sharedMesh;
			 	//ObjectUtil.dumpObjectData(tryM);
			 	
			 	RenderUtil.setMesh(go, tryM);
			 	/*
			 	go = ObjectUtil.createWorldObject(VanillaCreatures.AMPEEL.prefab);
			 	go.transform.position = pos;
			 	go.transform.rotation = Quaternion.identity;
			 	go.transform.localScale = new Vector3(3, 3, 4);
			 	
				foreach (Component c in go.GetComponentsInChildren<Component>()) {
					if (c is Transform || c is Renderer || c is MeshFilter || c is Collider || c is PrefabIdentifier || c is SkyApplier || c is Rigidbody) {
						continue;
					}
					if (c is SplineFollowing || c is WorldForces || c is ShockerMeleeAttack || c is FMOD_CustomEmitter || c is FMOD_StudioEventEmitter) {
						continue;
					}
					if (c is VFXShockerElec || c is LODGroup || c is Locomotion) {
						continue;
					}
					UnityEngine.Object.DestroyImmediate(c);
				}
				foreach (Component c in orig.GetComponentsInChildren<Component>()) {
					if (c is Transform || c is Renderer || c is MeshFilter || c is Collider || c is PrefabIdentifier || c is SkyApplier || c is Rigidbody) {
						continue;
					}
					if (c is SplineFollowing || c is WorldForces || c is FMOD_CustomEmitter || c is FMOD_StudioEventEmitter || c is VFXController) {
						continue;
					}
					if (c is Locomotion || c is AnimateByVelocity) {
						continue;
					}
			 		Component tgt = go.EnsureComponent(c.GetType());
					tgt.copyObject(c);
				}
			 	go.GetComponent<CreatureFollowPlayer>().creature = go.GetComponent<GhostLeviatanVoid>();
			 	go.GetComponent<Locomotion>().levelOfDetail = go.GetComponent<BehaviourLOD>();
			 	/*			 	
			 	ObjectUtil.copyComponent<GhostLeviatanVoid>(orig, go);
			 	ObjectUtil.copyComponent<LiveMixin>(orig, go);
			 	ObjectUtil.copyComponent<EntityTag>(orig, go);
			 	ObjectUtil.copyComponent<AggressiveOnDamage>(orig, go);
			 	ObjectUtil.copyComponent<SwimRandom>(orig, go);
			 	//ObjectUtil.copyComponent<GhostLeviathanMeleeAttack>(orig, go);
			 	ObjectUtil.copyComponent<AggressiveOnDamage>(orig, go);
			 	ObjectUtil.copyComponent<AttackCyclops>(orig, go);
			 	ObjectUtil.copyComponent<TechTag>(orig, go);
			 	ObjectUtil.copyComponent<AttackLastTarget>(orig, go);
			 	ObjectUtil.copyComponent<CreatureFollowPlayer>(orig, go).creature = go.GetComponent<GhostLeviatanVoid>();
			 	ObjectUtil.copyComponent<LastTarget>(orig, go);
			 	ObjectUtil.copyComponent<LastScarePosition>(orig, go);
			 	ObjectUtil.copyComponent<Locomotion>(orig, go);
			 	ObjectUtil.copyComponent<StayAtLeashPosition>(orig, go);
			 	ObjectUtil.copyComponent<SwimRandom>(orig, go);*/
			 	
	    		SkinnedMeshRenderer r = go.GetComponentInChildren<SkinnedMeshRenderer>();
				r.materials[0].SetFloat("_SpecInt", 0F);
	    		RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/VoidSpikeLevi/", new Dictionary<int, string>(){{0, "Outer"}, {1, "Inner"}});
	    		//r.materials[0].color = new Color(0, 0, 0, 0);
	    		go.EnsureComponent<VoidSpikeLeviathan>().init(go);
	    			    		
	    		//MeshFilter mesh = go.GetComponentInChildren<MeshFilter>();
	    		/*
	    		Vector3[] verts = mesh.mesh.vertices;
	    		for (int i = 0; i < verts.Length; i++) {
	    			Vector3 vv = verts[i];
	    			vv.x *= 0.5;
	    			verts[i] = vv;
	    		}
	    		mesh.mesh.vertices = verts;*/
	    		//if (tryM != null)
	    		//	mesh.mesh = tryM;
	    		/*
	    		FMODAsset bite = SeaToSeaMod.voidspikeLeviBite;
	    		FMOD_StudioEventEmitter std = go.GetComponent<FMOD_StudioEventEmitter>();
	    		std.asset = bite;
	    		std.path = bite.path;
	    		
	    		FMODAsset chg = SeaToSeaMod.voidspikeLeviRoar;
	    		FMOD_CustomEmitter cus = go.GetComponent<FMOD_CustomEmitter>();
	    		cus.asset = chg;
	    		
	    		FMODAsset idle = SeaToSeaMod.voidspikeLeviAmbient;
	    		FMOD_CustomLoopingEmitter loop = go.GetComponent<FMOD_CustomLoopingEmitter>();
	    		loop.asset = idle;
	    		FMOD_CustomLoopingEmitterWithCallback loop2 = go.GetComponent<FMOD_CustomLoopingEmitterWithCallback>();
	    		loop2.asset = idle;
	    		*/
	    		voidLeviathan = go;
	    	}
	    	return go;
	    }
	    
	    public void tickVoidLeviathan(GhostLeviatanVoid gv) {
			Player main = Player.main;
			VoidGhostLeviathansSpawner main2 = VoidGhostLeviathansSpawner.main;
			if (!main || Vector3.Distance(main.transform.position, gv.transform.position) > gv.maxDistanceToPlayer) {
				UnityEngine.Object.Destroy(gv.gameObject);
				if (gv.gameObject == voidLeviathan)
					voidLeviathan = null;
				return;
			}
			VoidSpikeLeviathan spikeType = gv.gameObject.GetComponentInChildren<VoidSpikeLeviathan>();
			bool spike = spikeType != null;
			bool zone = VoidSpikesBiome.instance.isPlayerInLeviathanZone();
			bool validVoid = spike ? zone : (!zone && main2.IsPlayerInVoid());
			bool flag = main2 && validVoid;
			gv.updateBehaviour = flag;
			gv.AllowCreatureUpdates(gv.updateBehaviour);
			if (spike && UnityEngine.Random.Range(0, 100) == 0) {
				SNUtil.playSoundAt(SeaToSeaMod.voidspikeLeviFX, gv.gameObject.transform.position);
			}
			if (flag || (spike && Vector3.Distance(main.transform.position, gv.transform.position) <= 50)) {
				if (false) {
					gv.Aggression.Value = 0;
					gv.lastTarget.target = getOrCreateTarget();
				}
				else {
					gv.Aggression.Add(spike ? 2.5F : 1);
					gv.lastTarget.target = main.gameObject;
				}
			}
			else {
				Vector3 a = gv.transform.position - main.transform.position;
				Vector3 vector = gv.transform.position + a * gv.maxDistanceToPlayer;
				vector.y = Mathf.Min(vector.y, -50f);
				gv.swimBehaviour.SwimTo(vector, 30f);
			}
	    }
	    
	    private double getAvoidanceChance(Player ep, SeaMoth sm, bool edge, bool far) {
	    	SonarPinged pinged = sm.gameObject.GetComponentInParent<SonarPinged>();
	    	if (pinged != null && pinged.getTimeSince() <= 10000)
	    		return 0;
	    	double minDist = double.PositiveInfinity;
	    	foreach (GameObject go in VoidGhostLeviathansSpawner.main.spawnedCreatures) {
	    		float dist = Vector3.Distance(go.transform.position, sm.transform.position);
	    		minDist = Math.Min(dist, minDist);
	    	}
	    	double frac2 = double.IsPositiveInfinity(minDist) ? 0 : Math.Max(0, (120-minDist)/120D);
	    	double depth = -sm.transform.position.y;
	    	if (depth < MAXDEPTH)
	    		return 1;
	    	double over = depth-MAXDEPTH;
	    	double fade = sm.lightsActive ? 100 : 200;
	    	double frac = Math.Min(1, over/fade);
	    	return 1D-Math.Max(frac, frac2);
	    }
	    
	    public void tagSeamothSonar(SeaMoth sm) {
	    	SonarPinged ping = sm.gameObject.EnsureComponent<SonarPinged>();
	    	ping.lastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	    }
    
	    private class SonarPinged : MonoBehaviour {
	    	
	    	internal long lastPing;
	    	
	    	internal long getTimeSince() {
	    		return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()-lastPing;
	    	}
	    }
	
		private class VoidSpikeLeviathan : MonoBehaviour {
			
			public void init(GameObject go) {
				
			}
			
			void Update() {
				 gameObject.transform.localScale = new Vector3(3, 3, 4);
			}
			
			void OnDestroy() {
				instance.deleteVoidLeviathan();
			}
			
		}
		
	}
	
}
