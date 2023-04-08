using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using FMOD;
using FMODUnity;

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
	    
	    private static readonly float DAZZLE_FADE_LENGTH = 3;
	    private static readonly float DAZZLE_TOTAL_LENGTH = 4.5F;
	    private static readonly double MAXDEPTH = 2000;//800;
	    
	    private static readonly float SONAR_INTERFERENCE_LENGTH = 7.5F;
	    private static readonly float SONAR_INTERFERENCE_DELAY = 1F;
	    private static readonly float SONAR_INTERFERENCE_FADE_LENGTH_IN = 0.5F;
	    private static readonly float SONAR_INTERFERENCE_FADE_LENGTH_OUT = 2.5F;
	    
	    private readonly List<SoundManager.SoundData> distantRoars = new List<SoundManager.SoundData>();
	    
	    private readonly SoundManager.SoundData empSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidlevi-emp", "Sounds/voidlevi/emp6.ogg", SoundManager.soundMode3D);
	    private readonly SoundManager.SoundData empHitSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidlevi-emp-hit", "Sounds/voidlevi/emp-hit.ogg", SoundManager.soundMode3D);
	    private readonly SoundManager.SoundData sonarBlockSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidlevi-sonarblock", "Sounds/voidlevi/sonarblock3.ogg", SoundManager.soundMode3D);
	    
	    private float nextDistantRoarTime = -1;
	    
	    private float lastFlashTime = -1;
	    private float currentFlashDuration = 0; //full blindness length; fade is (DAZZLE_FADE_LENGTH)x longer after that
	    
	    private float lastEMPHitSoundTime;
	    
	    private GameObject mainCamera;
	    private MesmerizedScreenFXController mesmerController;
	    private MesmerizedScreenFX mesmerShader;
	    private CyclopsSmokeScreenFXController smokeController;
	    private CyclopsSmokeScreenFX smokeShader;
	    private RadiationsScreenFXController radiationController;
	    private RadiationsScreenFX radiationShader;
	    private SonarScreenFX sonarShader;
	    private Vector4 defaultMesmerShaderColors;
	    private Color defaultSmokeShaderColors;
	    private Color defaultRadiationShaderColors;
	    
	    private readonly Color smokeDarkColors = new Color(-5, -5, -5, 1F);
	    private readonly Color smokeDazzleColors = new Color(50, 50, 50, 1F);
	    private readonly Vector4 mesmerDazzleColorsStart = new Vector4(600, 600, 1000, 0.5F);
	    private readonly Vector4 mesmerDazzleColorsEnd = new Vector4(600, 600, 1000, 0.2F);
	    
	    private readonly Vector4 sonarInterferenceColor = new Vector4(-1, -1, -0.25F, 1F);
	    
	    private float flashWashoutNetVisiblityFactor = 1;
	    
	    private float lastSonarInterferenceTime = -1;
	    
	    private Channel? sonarSoundEvent;
	    
	    private static readonly List<DistantFX> distantFXList = new List<DistantFX>(){
	    	new DistantFX("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc", 2),
	    	new DistantFX("6e4b4259-becc-4d2c-b56a-03ccedbc4672", 2),
	    	new DistantFX("3274b205-b153-41b6-9736-f3972e38f0ad", 2),/*
	    	new DistantFX("04781674-e27a-43ce-891f-a82781314c15", 2.5F, 5, 0.5F, go => {
	    		go.transform.rotation = UnityEngine.Random.rotationUniform;
	    		go.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.75F, 1.5F);
	    	}), //lavafall base*/
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
	    		UnityEngine.Object.Destroy(voidLeviathan);
	    	voidLeviathan = null;
	    }
	    
	    internal void deleteGhostTarget() {
	    	if (redirectedTarget)
	    		UnityEngine.Object.Destroy(redirectedTarget);
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
	    
	    internal void tick(Player ep) {
	    	if (!mainCamera && Camera.main) {
	    		mainCamera = Camera.main.gameObject;
	    		if (mainCamera) {
	    			//SNUtil.log("Load cam");
		    		mesmerController = mainCamera.GetComponent<MesmerizedScreenFXController>();
		    		mesmerShader = mainCamera.GetComponent<MesmerizedScreenFX>();
		    		if (mesmerShader && mesmerShader.mat)
		    			defaultMesmerShaderColors = mesmerShader.mat.GetVector("_ColorStrength");
		    		else
		    			mainCamera = null;
		    		if (!mainCamera) {
	    				//SNUtil.log("Fail A");
		    			return;
		    		}
		    		
		    		smokeController = mainCamera.GetComponent<CyclopsSmokeScreenFXController>();
		    		smokeShader = mainCamera.GetComponent<CyclopsSmokeScreenFX>();
		    		if (smokeShader) {
		    			smokeShader.enabled = true;
			    		if (smokeShader.mat)
			    			defaultSmokeShaderColors = smokeShader.mat.color;
			    		else
			    			mainCamera = null;
		    			smokeShader.enabled = false;
		    		}
		    		else
		    			mainCamera = null;		    		
		    		if (!mainCamera) {
	    				//SNUtil.log("Fail B");
		    			return;
		    		}
		    		
		    		radiationController = mainCamera.GetComponent<RadiationsScreenFXController>();
		    		radiationShader = mainCamera.GetComponent<RadiationsScreenFX>();
		    		if (radiationShader)
		    			defaultRadiationShaderColors = radiationShader.color;
		    		else
		    			mainCamera = null;
		    		
		    		if (!mainCamera) {
	    				//SNUtil.log("Fail C");
		    			return;
		    		}
		    		
		    		sonarShader = mainCamera.GetComponent<SonarScreenFX>();
	    		}
	    		else {
	    			return;
	    		}
	    	}
	    	//SNUtil.log("========================");
	    	//SNUtil.log("All non-null: "+mesmerShader+" x "+mesmerController+" x "+smokeShader+" x "+smokeController);
	    	if (sonarSoundEvent != null && sonarSoundEvent.Value.hasHandle()) {
				ATTRIBUTES_3D attr = mainCamera.transform.position.To3DAttributes();
				sonarSoundEvent.Value.set3DAttributes(ref attr.position, ref attr.velocity, ref attr.forward);
	    	}
	    	DayNightCycle day = DayNightCycle.main;
	    	if (!day || !smokeController || !smokeShader || !mesmerController || !mesmerShader)
	    		return;
	    	float time = day.timePassedAsFloat;
	    	playDistantRoar(ep, time);
	    	float dtf = time-lastFlashTime;
	    	float maxL = currentFlashDuration*(1+DAZZLE_FADE_LENGTH);
	    	float maxL2 = currentFlashDuration*(1+DAZZLE_TOTAL_LENGTH);
	    	//SNUtil.log("L calc done");
	    	if (dtf <= maxL) {
	    		float alpha = 0;
	    		float colorMix = 0;
	    		if (dtf <= 0.33) {
	    			alpha = dtf*3;
	    			colorMix = 0;
	    		}
	    		else if (dtf <= 0.67) {
	    			alpha = 1;
	    			colorMix = 0;
	    		}
	    		else if (dtf <= 1) { //4-78s
	    			alpha = 1;
	    			colorMix = (float)MathUtil.linterpolate(dtf, 0.67F, 1, 0, 1);
	    		}
	    		else if (dtf <= currentFlashDuration) { //4-8s
	    			alpha = 1;
	    			colorMix = 1;
	    		}
	    		else {
	    			alpha = (float)MathUtil.linterpolate(dtf, currentFlashDuration, maxL, 1, 0);
	    			colorMix = 1;
	    		}
	    		flashWashoutNetVisiblityFactor = 1-alpha;
	    	//SNUtil.log("pre smoke mat");
	    		//SNUtil.log(time+" > "+dtf+" / "+currentFlashDuration+" > A="+alpha.ToString("0.00000")+" & C="+colorMix.ToString("0.00000"));
	    		smokeController.enabled = false;
	    		smokeShader.enabled = true;
	    		smokeShader.intensity = 1;
	    	//SNUtil.log("pre smoke mat");
	    		if (smokeShader.mat)
	    			smokeShader.mat.color = Color.Lerp(smokeDazzleColors, smokeDarkColors, colorMix).WithAlpha(alpha);
	    	}
	    	else {
	    	//SNUtil.log("pre smoke disable");
	    		smokeController.enabled = true;
	    	//SNUtil.log("pre smoke mat disable");
	    		if (smokeShader.mat)
	    			smokeShader.mat.color = defaultSmokeShaderColors;
	    	}
	    	//SNUtil.log("done smoke, now mesmer");
	    	if (dtf <= maxL2) {
	    		float f = 0;
	    		if (dtf <= 0.5F) {
	    			f = dtf*2;
	    		}
	    		else if (dtf <= maxL) {
	    			f = 1;
	    		}
	    		else {
	    			f = (float)MathUtil.linterpolate(dtf, maxL, maxL2, 1, 0);
	    		}
	    	//SNUtil.log("pre mesmer fx");
	    		mesmerController.enabled = false;
	    		mesmerShader.enabled = true;
	    		mesmerShader.amount = f*f*0.003F;
	    	//SNUtil.log("pre mesmer mat");
	    		if (mesmerShader.mat)
	    			mesmerShader.mat.SetVector("_ColorStrength", Vector4.Lerp(mesmerDazzleColorsStart, mesmerDazzleColorsEnd, f));
	    	}
	    	else {
	    	//SNUtil.log("pre mesmer disable");
	    		mesmerController.enabled = true;
	    	//SNUtil.log("pre mesmer mat disable");
	    		if (mesmerShader.mat)
	    			mesmerShader.mat.SetVector("_ColorStrength", defaultMesmerShaderColors);
	    	}
	    	//SNUtil.log("end");
	    	//SNUtil.log("==============================");
	    	//SNUtil.log(radiationController+"");
	    	//SNUtil.log(radiationShader+"");
	    	//SNUtil.log(sonarShader+"");
	    	
	    	if (radiationController && radiationShader) {
	    		float dt = time-lastSonarInterferenceTime;
	    		float f = 0;
	    		if (dt <= SONAR_INTERFERENCE_LENGTH) {
	    			if (dt <= SONAR_INTERFERENCE_DELAY)
	    				f = 0.02F;
	    			else if (dt <= SONAR_INTERFERENCE_FADE_LENGTH_IN+SONAR_INTERFERENCE_DELAY)
	    				f = 0.02F+0.98F*(dt-SONAR_INTERFERENCE_DELAY)/SONAR_INTERFERENCE_FADE_LENGTH_IN;
	    			else if (dt > SONAR_INTERFERENCE_LENGTH-SONAR_INTERFERENCE_FADE_LENGTH_OUT)
	    				f = 1-((dt-(SONAR_INTERFERENCE_LENGTH-SONAR_INTERFERENCE_FADE_LENGTH_OUT))/SONAR_INTERFERENCE_FADE_LENGTH_OUT);
	    			else
	    				f = 1;
	    		}
	    		//SNUtil.log("f="+f);
	    		if (f > 0) {
		    		radiationController.enabled = false;
		    		radiationShader.enabled = true;
		    		radiationShader.noiseFactor = f*f*0.6F;
		    		radiationShader.color = sonarInterferenceColor.asColor();
	    		//SNUtil.log("postcolorset");
		    		if (sonarShader)
		    			sonarShader.enabled = false;
	    		}
	    		else {
	    			radiationController.enabled = true;
		    		radiationShader.color = defaultRadiationShaderColors;
		    	}
	    	}
	    }
	    
	    public void triggerEMInterference() { //TODO excessive sonar pings could increase aggro once it's actually implemented
	    	lastSonarInterferenceTime = DayNightCycle.main.timePassedAsFloat;
	    	Vector3 pos = mainCamera.transform.position;
	    	sonarSoundEvent = SoundManager.playSoundAt(sonarBlockSound, pos, false, -1, 1);
	    	spawnJustVisibleDistanceFX(mainCamera.transform);
	    	//if (UnityEngine.Random.Range(0F, 1F) < 0.4F)
	    	//	SoundManager.playSoundAt(distantRoars[UnityEngine.Random.Range(1, distantRoars.Count-1)], MathUtil.getRandomVectorAround(pos, 100), false, -1, 1);
	    }
	    
	    public bool isSonarInterferenceActive() {
	    	return DayNightCycle.main.timePassedAsFloat-lastSonarInterferenceTime <= SONAR_INTERFERENCE_LENGTH;
	    }
	    
	    public bool isVoidFlashActive(bool any) { //false if only the blind part
	    	if (currentFlashDuration <= 0)
	    		return false;
	    	float n = any ? DAZZLE_TOTAL_LENGTH : DAZZLE_FADE_LENGTH;
	    	return DayNightCycle.main.timePassedAsFloat <= lastFlashTime+currentFlashDuration*(1+n);
	    }
	    
	    public float getNetScreenVisibilityAfterFlash() {
	    	if (currentFlashDuration <= 0)
	    		return 1;
	    	return flashWashoutNetVisiblityFactor;
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
	    		Vector3 pos = spawnJustVisibleDistanceFX(ep.transform);
		    	if (forceEMP || (VoidSpikesBiome.instance.isPlayerInLeviathanZone(ep.transform.position))) {
		    		float chance = forceEMP ? 0.5F : Mathf.Min(0.33F, (ep.GetDepth()-600)/900);
		    		if (UnityEngine.Random.Range(0F, 1F) <= chance) {/*
		    			Vector3 rel = getRandomVisibleDistantPosition(ep, 3, 3);
		    			Vector3 pos = ep.transform.position+rel;*/
		    			Vector3 rel = pos-ep.transform.position;
		    			pos = ep.transform.position+rel.normalized*150;
		    			pos = MathUtil.getRandomVectorAround(pos, 15);
		    			
		    			if (forceEMP)
		    				SNUtil.writeToChat("Spawning EMP blast @ "+pos);
		    			spawnEMPBlast(pos);
		    			//shutdownSeamoth(ep.GetVehicle(), false);
		    		}
		    	}
	    	}
	    }
	    
	    public void onObjectEMPHit(EMPBlast e, GameObject go) { //this might be called many times!
	    	//SNUtil.writeToChat(">>"+e.gameObject.name+" > "+e.gameObject.name.StartsWith("VoidSpikeLevi_LightPulse", StringComparison.InvariantCultureIgnoreCase)+" @ "+go.FindAncestor<Player>());
	    	if (e.gameObject.name.StartsWith("VoidSpikeLevi_EMPulse", StringComparison.InvariantCultureIgnoreCase)) {
		    	//SNUtil.writeToChat("Match");
		    	MapRoomCamera cam = go.FindAncestor<MapRoomCamera>();
	    		if (cam) {
		    		go.FindAncestor<LiveMixin>().TakeDamage(999999, go.transform.position, DamageType.Electrical, e.gameObject);
		    		//cam.energyMixin.ConsumeEnergy(99999); //completely kill it
	    		}
	    		else {
		    		shutdownSeamoth(go.FindAncestor<Vehicle>(), true);
	    		}
	    	}
	    }
	    
	    //Returns the relative position, not absolute
	    private Vector3 getRandomVisibleDistantPosition(Transform cam, float distSc, float fovSc = 1) {
	    	float range = UnityEngine.Random.Range(30F, 40F);
	    	range *= distSc;
	    	Vector3 pos = cam.position+/*ep.transform.forward*/MainCamera.camera.transform.forward.normalized*range;
	    	pos = MathUtil.getRandomVectorAround(pos, 20*distSc*fovSc);
	    	Vector3 dist = pos-cam.position;
	    	dist = dist.setLength(range);
	    	return dist;
	    }
	    
	    private Vector3 spawnJustVisibleDistanceFX(Transform cam) {
	    	DistantFX type = distantFXList[UnityEngine.Random.Range(0, distantFXList.Count)];
	    	Vector3 dist = getRandomVisibleDistantPosition(cam, type.distanceScalar);
	    	Vector3 pos = cam.position+dist;
	    	GameObject go = getOrCreateSparkFX(type);
	    	go.transform.position = pos;
	    	go.transform.localScale = Vector3.one*UnityEngine.Random.Range(1.5F, 2.5F);
	    	if (type.modification != null)
	    		type.modification(distantSparkFX);
	    	VoidSparkFX fx = go.GetComponent<VoidSparkFX>();
	    	fx.relativePosition = dist;
	    	float speed = UnityEngine.Random.Range(2.5F, 10F)+(dist.magnitude-30)*0.5F;
	    	if (UnityEngine.Random.Range(0F, 1F) <= 0.2F)
	    		speed *= 3;
	    	speed *= type.speedScalar;
	    	fx.velocity = MathUtil.getRandomVectorAround(Vector3.zero, 1).normalized*speed;
	    	go.SetActive(true);
	    	UnityEngine.Object.Destroy(go, UnityEngine.Random.Range(0.33F, 0.75F)*type.lifeScalar);
	    	return pos;
	    }
	    
	    internal void shutdownSeamoth(Vehicle v, bool disable, float factor = 1) {
	    	if (v) {
	    		if (v is SeaMoth) {
	    			ObjectUtil.createSeamothSparkSphere((SeaMoth)v);
	    			float time = DayNightCycle.main.timePassedAsFloat;
	    			if (time-lastEMPHitSoundTime > 1F) {
	    				lastEMPHitSoundTime = time;
	    				SoundManager.playSoundAt(empHitSound, v.transform.position, false, -1, 1);
	    			}
	    		}
	    		v.ConsumeEnergy(UnityEngine.Random.Range(4F, 10F)*factor); //2-5% base
	    		if (disable)
	    			v.energyInterface.DisableElectronicsForTime(UnityEngine.Random.Range(1F, 5F)*factor);
	    	}
	    }
	    
	    internal void spawnAoEBlast(Vector3 pos, string idName, Action<GameObject> modify = null) {
	    	GameObject pfb = ObjectUtil.lookupPrefab(VanillaCreatures.CRABSQUID.prefab).GetComponent<EMPAttack>().ammoPrefab;
	    	for (int i = 0; i < 180; i += 30) {
				GameObject emp = UnityEngine.Object.Instantiate(pfb);
		    	emp.transform.position = pos;
		    	emp.transform.localRotation = Quaternion.Euler(i, 0, 0);
		    	//emp.EnsureComponent<VoidLeviElecSphereComponent>().spawn();
		    	Renderer r = emp.GetComponentInChildren<Renderer>();
		    	r.materials[0].color = new Color(0.8F, 0.33F, 1F, 1F);
		    	r.materials[1].color = new Color(0.67F, 0.9F, 1F, 1F);
		    	r.materials[0].SetColor("_ColorStrength", new Color(1, 1, 1000, 1));
		    	r.materials[1].SetColor("_ColorStrength", new Color(1, 1, 100, 1));
		    	EMPBlast e = emp.GetComponent<EMPBlast>();
		    	ObjectUtil.removeComponent<VFXLerpColor>(emp);
		    	e.blastRadius = AnimationCurve.Linear(0f, 0f, 1f, 400f);
		    	e.blastHeight = AnimationCurve.Linear(0f, 0f, 1f, 300f);
		    	e.lifeTime = 3.5F;
		    	e.disableElectronicsTime = UnityEngine.Random.Range(1F, 5F);
		    	if (modify != null)
		    		modify(emp);
		    	emp.name = idName+"_"+i;
		    	emp.SetActive(true);
		    	//ObjectUtil.dumpObjectData(emp);
	    	}
	    }
	    
	    internal void spawnEMPBlast(Vector3 pos) {
	    	SoundManager.playSoundAt(empSound, pos, false, -1, 1);
	    	spawnAoEBlast(pos, "VoidSpikeLevi_EMPulse");
	    }
	    
	    internal void doDebugFlash(bool usePulse) {
	    	if (usePulse)
	    		doFlash(Player.main.transform.position+MainCamera.camera.transform.forward.normalized*150);
	    	else
	    		onFlashHit();
	    }
	    
	    internal void doFlash(Vector3 pos) {
	    	Action<GameObject> a = e => {
	    		FlashBurst f = e.EnsureComponent<FlashBurst>();
	    		ObjectUtil.removeComponent<EMPBlast>(e);
		    	Renderer r = e.GetComponentInChildren<Renderer>();
		    	r.materials[0].color = Color.white;
		    	r.materials[1].color = Color.white;
		    	r.materials[0].SetColor("_ColorStrength", new Color(1000, 1000, 1000, 1));
		    	r.materials[1].SetColor("_ColorStrength", new Color(100, 100, 100, 1));
		    	foreach (OnTouch tt in e.GetComponentsInChildren<OnTouch>()) {
					tt.onTouch = new OnTouch.OnTouchEvent();
					tt.onTouch.RemoveAllListeners();
					tt.onTouch.AddListener(f.OnTouch);
		    	}
	    	};
	    	Action<GameObject> a2 = e => {
	    		a(e);
	    		e.EnsureComponent<FlashBurst>().lifeTime += 0.075F;
	    	};
	    	spawnAoEBlast(pos, "VoidSpikeLevi_LightPulse", a);
	    	spawnAoEBlast(pos, "VoidSpikeLevi_LightPulse", a2);
	    }
	    
	    internal void onFlashHit() {
	    	lastFlashTime = DayNightCycle.main.timePassedAsFloat;
	    	currentFlashDuration = UnityEngine.Random.Range(4F, 8F);
	    }
	    
	    internal void resetFlash() {
	    	lastFlashTime = -1;
	    	currentFlashDuration = 0;
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
	    	float time = DayNightCycle.main.timePassedAsFloat;
	    	SeamothStealthManager ping = sm.gameObject.EnsureComponent<SeamothStealthManager>();
	    	if (ping.nextStealthValidityTime >= 0 && time < ping.nextStealthValidityTime)
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
	    
	    public void temporarilyDisableSeamothStealth(SeaMoth sm, float duration) {
	    	if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
	    		duration *= 1.5F;
	    	SeamothStealthManager ping = sm.gameObject.EnsureComponent<SeamothStealthManager>();
	    	ping.nextStealthValidityTime = Mathf.Max(ping.nextStealthValidityTime, DayNightCycle.main.timePassedAsFloat+duration);
	    }
    
	    private class SeamothStealthManager : MonoBehaviour {
	    	
	    	internal float nextStealthValidityTime = -1;
	    	
	    }
	    
	    private class VoidSparkFX : MonoBehaviour {
	    	
	    	internal Vector3 velocity;
	    	internal Vector3 relativePosition;
			
			void Update() {
	    		gameObject.transform.position =  gameObject.transform.position+velocity*Time.deltaTime;
			}
	    	
	    }
	    
	    private class FlashBurst : MonoBehaviour {
			private void Start() {
				startTime = Time.time;
				SetProgress(0f);
			}
		
			private void Update() {
				float num = Mathf.InverseLerp(startTime, startTime + lifeTime, Time.time);
				num = Mathf.Clamp01(num);
				SetProgress(num);
				if (Mathf.Approximately(num, 1)) {
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		
			private void SetProgress(float progress) {
				currentBlastRadius = blastRadius.Evaluate(progress);
				currentBlastHeight = blastHeight.Evaluate(progress);
				transform.localScale = new Vector3(currentBlastRadius, currentBlastHeight, currentBlastRadius);
			}
		
			public void OnTouch(Collider collider) {
	    		Player ep = collider.gameObject.FindAncestor<Player>();
	    		//SNUtil.writeToChat(collider+">"+ep);
	    		if (ep) {
	    			VoidSpikeLeviathanSystem.instance.onFlashHit();
	    			return;
	    		}
	    		Vehicle v = collider.gameObject.FindAncestor<Vehicle>();
	    		if (v && v == Player.main.GetVehicle()) {
	    			VoidSpikeLeviathanSystem.instance.onFlashHit();
	    			return;
	    		}
	    		SubRoot s = collider.gameObject.FindAncestor<SubRoot>();
	    		if (s && s == Player.main.currentSub) {
	    			VoidSpikeLeviathanSystem.instance.onFlashHit();
	    			return;
	    		}
			}
		
			public float lifeTime = 0.5F;
		
			public AnimationCurve blastRadius = AnimationCurve.Linear(0f, 0f, 1f, 200F);
		
			public AnimationCurve blastHeight = AnimationCurve.Linear(0f, 0f, 1f, 150F);
		
			private float startTime;
		
			private float currentBlastRadius;
		
			private float currentBlastHeight;
		}
		
	}
	
}
