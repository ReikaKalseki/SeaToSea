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
	
	public class VoidSpikeLeviathanSystem {
		
		public static readonly VoidSpikeLeviathanSystem instance = new VoidSpikeLeviathanSystem();
	    
	    private static GameObject voidLeviathan;
	    private static GameObject redirectedTarget;
	    
	    private static GameObject distantSparkFX;
	    
	    private static readonly float DAZZLE_FADE_LENGTH = 5;
	    private static readonly double MAXDEPTH = 2000;//800;
	    
	    private readonly List<SoundManager.SoundData> distantRoars = new List<SoundManager.SoundData>();
	    
	    private float nextDistantRoarTime = -1;
	    private float lastFlashTime = -1;
	    private float currentFlashDuration = 0; //full blindness length; fade is (DAZZLE_FADE_LENGTH)x longer after that
	    
	    private GameObject mainCamera;
	    private MesmerizedScreenFXController mesmerController;
	    private MesmerizedScreenFX mesmerShader;
	    private Vector4 defaultMesmerShaderColors;
	    private readonly Vector4 dazzleColors = new Vector4(800, 800, 1000, 1);
	    
	    private static readonly List<DistantFX> distantFXList = new List<DistantFX>(){
	    	new DistantFX("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc"),
	    	new DistantFX("6e4b4259-becc-4d2c-b56a-03ccedbc4672"),
	    	new DistantFX("3274b205-b153-41b6-9736-f3972e38f0ad"),
	    	new DistantFX("04781674-e27a-43ce-891f-a82781314c15", 2.5F, 5, 0.5F, go => {
	    		go.transform.rotation = UnityEngine.Random.rotationUniform;
	    		go.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.75F, 1.5F);
	    	}), //lavafall base
	    };
	    
	    class DistantFX {
	    	
	    	internal readonly string prefab;
	    	internal readonly float distanceScalar;
	    	internal readonly float lifeScalar;
	    	internal readonly float speedScalar;
	    	internal readonly Action<GameObject> modification;
	    	
	    	internal DistantFX(string pfb, float d = 1, float l = 1, float s = 1, Action<GameObject> a = null) {
	    		prefab = pfb;
	    		distanceScalar = d;
	    		lifeScalar = l;
	    		speedScalar = s;
	    		modification = a;
	    	}
	    	
	    }
		
		private VoidSpikeLeviathanSystem() {
	    	for (int i = 0; i <= 2; i++) {
	    		distantRoars.Add(SoundManager.registerSound(SeaToSeaMod.modDLL, "voidlevi-roar-far-"+i, "Sounds/voidlevi/roar-distant-"+i+".ogg", SoundManager.soundMode3D));
	    	}
		}
	    
	    internal void deleteVoidLeviathan() {
	    	if (voidLeviathan)
	    		UnityEngine.Object.DestroyImmediate(voidLeviathan);
	    	voidLeviathan = null;
	    }
	    
	    internal void deleteGhostTarget() {
	    	if (redirectedTarget)
	    		UnityEngine.Object.DestroyImmediate(redirectedTarget);
	    	redirectedTarget = null;
	    }
	    
	    private GameObject getOrCreateTarget() {
	    	if (!redirectedTarget) {
	    		redirectedTarget = new GameObject("Void Ghost Target");
	    	}
	    	return redirectedTarget;
	    }
	    
	    private GameObject getOrCreateSparkFX(DistantFX fx) {
	    	if (!distantSparkFX) {
	    		distantSparkFX = ObjectUtil.createWorldObject(fx.prefab); //or ff8e782e-e6f3-40a6-9837-d5b6dcce92bc
	    		distantSparkFX.EnsureComponent<VoidSparkFX>();
	    	}
	    	return distantSparkFX;
	    }
	    
	    private GameObject createSparkSphere(SeaMoth sm) {
			ElectricalDefense def = sm.seamothElectricalDefensePrefab.GetComponent<ElectricalDefense>();
			GameObject sphere = def.fxElecSpheres[0];
	    	return Utils.SpawnZeroedAt(sphere, sm.transform, false);
	    }
	    
	    internal void tick(Player ep) {
	    	if (!mainCamera) {
	    		mainCamera = Camera.main.gameObject;
	    		mesmerController = mainCamera.GetComponent<MesmerizedScreenFXController>();
	    		mesmerShader = mainCamera.GetComponent<MesmerizedScreenFX>();
	    		defaultMesmerShaderColors = mesmerShader.mat.GetVector("_ColorStrength");
	    	}
	    	float time = DayNightCycle.main.timePassedAsFloat;
	    	playDistantRoar(ep, time);
	    	float dtf = time-lastFlashTime;
	    	if (dtf <= currentFlashDuration*(1+DAZZLE_FADE_LENGTH)) {
	    		float f = 0;
	    		if (dtf <= 0.33) {
	    			f = dtf*3;
	    		}
	    		else if (dtf <= currentFlashDuration) {
	    			f = 1;
	    		}
	    		else {
	    			float f2 = (dtf-currentFlashDuration)/(currentFlashDuration*DAZZLE_FADE_LENGTH);
	    			f = 1-f2;
	    			f *= f;
	    			f = Mathf.Clamp01(f*1.2F-0.1F);
	    		}
	    		//SNUtil.writeToChat(dtf+" / "+currentFlashDuration+" > "+f.ToString("0.00000"));
	    		//SNUtil.log(time+" > "+f.ToString("0.00000"), SeaToSeaMod.modDLL);
	    		mesmerController.enabled = false;
	    		mesmerShader.enabled = true;
	    		mesmerShader.amount = f;
	    		mesmerShader.mat.SetVector("_ColorStrength", dazzleColors);
	    	}
	    	else {
	    		mesmerController.enabled = true;
	    		mesmerShader.mat.SetVector("_ColorStrength", defaultMesmerShaderColors);
	    	}
	    }
	    
	    internal void playDistantRoar(Player ep, float time) {
	    	if (ep.currentSub)
	    		return;
	    	if (time < nextDistantRoarTime)
	    		return;
	    	if (voidLeviathan && voidLeviathan.activeInHierarchy && Vector3.Distance(voidLeviathan.transform.position, ep.transform.position) <= 200)
	    		return;
	    	doDistantRoar(ep);
	    }
	    
	    internal void doDistantRoar(Player ep, bool forceSpark = false, bool forceEMP = false) {
	    	SoundManager.SoundData? roar = null;
	    	double dist = VoidSpikesBiome.instance.getDistanceToBiome(ep.transform.position, true);
	    	string biome = ep.GetBiomeString();
	    	//SNUtil.writeToChat(dist+" @ "+biome);
	    	float vol = 1;
	    	bool inBiome = false;
	    	if (dist <= VoidSpikesBiome.biomeVolumeRadius) {
	    		roar = distantRoars[UnityEngine.Random.Range(1, distantRoars.Count-1)];
	    		float dd = Vector3.Distance(ep.transform.position, VoidSpikesBiome.end900m);
	    		vol = Mathf.Clamp01(2-dd/400F);
	    		inBiome = true;
	    	}
	    	else if (dist <= 1000 && (biome == null || biome == VoidSpikesBiome.biomeName || string.Equals(biome, "void", StringComparison.InvariantCultureIgnoreCase))) {
	    		roar = distantRoars[0];
	    		vol = 1-(float)Math.Max(0, Math.Min(1, (dist-250)/1000D));
	    	}
	    	if (roar != null && roar.HasValue) {
	    		float delta = (float)Math.Max(30, UnityEngine.Random.Range(30F, 120F)*dist/1000);
	    		nextDistantRoarTime = DayNightCycle.main.timePassedAsFloat+delta;
	    		//SNUtil.writeToChat(dist+" @ "+biome+" > "+roar+"/"+vol+" >> "+delta);
	    		SoundManager.playSoundAt(roar.Value, MathUtil.getRandomVectorAround(ep.transform.position, 100), false, -1, vol);
	    	}
	    	if (forceSpark || inBiome) {
	    		spawnJustVisibleDistanceFX(ep);
	    	}
	    	if (forceEMP || (VoidSpikesBiome.instance.isPlayerInLeviathanZone(ep.transform.position))) {
	    		float chance = Mathf.Min(0.33F, (ep.GetDepth()-600)/900);
	    		if (UnityEngine.Random.Range(0F, 1F) <= chance)
	    			shutdownSeamoth(ep.GetVehicle());
	    	}
	    }
	    
	    private void spawnJustVisibleDistanceFX(Player ep) {
	    	float range = UnityEngine.Random.Range(30F, 40F);
	    	DistantFX type = distantFXList[UnityEngine.Random.Range(0, distantFXList.Count)];
	    	range *= type.distanceScalar;
	    	Vector3 pos = ep.transform.position+/*ep.transform.forward*/MainCamera.camera.transform.forward.normalized*range;
	    	pos = MathUtil.getRandomVectorAround(pos, 20);
	    	Vector3 dist = pos-ep.transform.position;
	    	dist = dist.setLength(range);
	    	pos = ep.transform.position+dist;
	    	GameObject go = getOrCreateSparkFX(type);
	    	go.transform.position = pos;
	    	go.transform.localScale = Vector3.one*UnityEngine.Random.Range(1.5F, 2.5F);
	    	if (type.modification != null)
	    		type.modification(distantSparkFX);
	    	VoidSparkFX fx = go.GetComponent<VoidSparkFX>();
	    	fx.relativePosition = dist;
	    	float speed = UnityEngine.Random.Range(2.5F, 10F)+(range-30)*0.5F;
	    	if (UnityEngine.Random.Range(0F, 1F) <= 0.2F)
	    		speed *= 3;
	    	speed *= type.speedScalar;
	    	fx.velocity = MathUtil.getRandomVectorAround(Vector3.zero, 1).normalized*speed;
	    	go.SetActive(true);
	    	UnityEngine.Object.Destroy(go, UnityEngine.Random.Range(0.33F, 0.75F)*type.lifeScalar);
	    }
	    
	    internal void shutdownSeamoth(Vehicle v, float factor = 1) {
	    	if (v) {
	    		if (v is SeaMoth)
	    			createSparkSphere((SeaMoth)v).SetActive(true);
	    		v.energyInterface.DisableElectronicsForTime(UnityEngine.Random.Range(1F, 5F)*factor);
	    		v.ConsumeEnergy(UnityEngine.Random.Range(4F, 10F)*factor); //2-5% base
	    	}
	    }
	    
	    internal void doFlash() {
	    	lastFlashTime = DayNightCycle.main.timePassedAsFloat;
	    	currentFlashDuration = UnityEngine.Random.Range(4F, 8F);
	    }
    
	    public bool isSpawnableVoid(string biome) {
	    	Player ep = Player.main;
	    	if (VoidSpikesBiome.instance.isPlayerInLeviathanZone(ep.transform.position) && isLeviathanEnabled() && voidLeviathan && voidLeviathan.activeInHierarchy) {
	    		return false;
	    	}
	    	bool edge = string.Equals(biome, "void", StringComparison.InvariantCultureIgnoreCase);
	    	bool far = string.IsNullOrEmpty(biome);
	    	if (VoidSpikesBiome.instance.getDistanceToBiome(ep.transform.position, true) <= VoidSpikesBiome.biomeVolumeRadius+25)
	    		far = true;
	    	if (!far && !edge)
	    		return false;
	    	if (ep.inSeamoth) {
	    		SeaMoth sm = (SeaMoth)ep.GetVehicle();
	    		double ch = getAvoidanceChance(ep, sm, edge, far);
	    		//SNUtil.writeToChat(ch+" @ "+sm.transform.position);
	    		if (ch > 0 && (ch >= 1 || UnityEngine.Random.Range(0F, 1F) <= ch)) {
	    			if (InventoryUtil.vehicleHasUpgrade(sm, C2CItems.voidStealth.TechType))
	    				return false;
	    			//SNUtil.writeToChat("Tried and failed");
	    		}
	    	}
	    	return true;
	    }
	    
	    public bool isLeviathanEnabled() {
	    	return false;
	    }
	    
	    public GameObject getVoidLeviathan(VoidGhostLeviathansSpawner spawner, Vector3 pos) {
	    	if (isLeviathanEnabled() && VoidSpikesBiome.instance.isPlayerInLeviathanZone(Player.main.transform.position)) {
	    		GameObject go = ObjectUtil.createWorldObject(SeaToSeaMod.voidSpikeLevi.ClassID);
	    		go.transform.position = pos;
	    		voidLeviathan = go;
	    		return go;
	    	}
	    	else {
	    		return UnityEngine.Object.Instantiate<GameObject>(spawner.ghostLeviathanPrefab, pos, Quaternion.identity);
	    	}
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
			VoidSpikeLeviathan.VoidSpikeLeviathanAI spikeType = gv.gameObject.GetComponentInChildren<VoidSpikeLeviathan.VoidSpikeLeviathanAI>();
			bool spike = spikeType;
			bool zone = VoidSpikesBiome.instance.isPlayerInLeviathanZone(main.transform.position);
			bool validVoid = spike ? zone : (!(zone && isLeviathanEnabled()) && main2.IsPlayerInVoid());
			bool flag = main2 && validVoid;
			gv.updateBehaviour = flag;
			gv.AllowCreatureUpdates(gv.updateBehaviour);
			if (spike && UnityEngine.Random.Range(0, 100) == 0) {
				//SoundManager.playSoundAt(SeaToSeaMod.voidspikeLeviFX, gv.gameObject.transform.position, false, 128);
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
	    	if (isLeviathanEnabled() && VoidSpikesBiome.instance.isPlayerInLeviathanZone(ep.transform.position))
	    		return 0;
	    	SonarPinged pinged = sm.gameObject.GetComponentInParent<SonarPinged>();
	    	if (pinged != null && pinged.getTimeSince() <= 30)
	    		return 0;
	    	double minDist = double.PositiveInfinity;
	    	foreach (GameObject go in VoidGhostLeviathansSpawner.main.spawnedCreatures) {
	    		float dist = Vector3.Distance(go.transform.position, sm.transform.position);
	    		minDist = Math.Min(dist, minDist);
	    	}
	    	double frac2 = double.IsPositiveInfinity(minDist) || double.IsNaN(minDist) ? 0 : Math.Max(0, 1-minDist/120D);
	    	if (frac2 >= 1)
	    		return 0;
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
	    	ping.lastPing = DayNightCycle.main.timePassedAsFloat;
	    }
    
	    private class SonarPinged : MonoBehaviour {
	    	
	    	internal float lastPing;
	    	
	    	internal float getTimeSince() {
	    		return DayNightCycle.main.timePassedAsFloat-lastPing;
	    	}
	    }
	    
	    private class VoidSparkFX : MonoBehaviour {
	    	
	    	internal Vector3 velocity;
	    	internal Vector3 relativePosition;
			
			void Update() {
	    		gameObject.transform.position =  gameObject.transform.position+velocity*Time.deltaTime;
			}
	    	
	    }
		
	}
	
}
