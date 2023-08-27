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
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class FallingGlassForestWreck : Spawnable {
		
		public static readonly string STORY_TAG = "GlassWreckFall";
	        
	    internal FallingGlassForestWreck() : base("fallingwreck", "", "") {
			
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("1618a787-67b7-4e35-9869-3ec558ed2835");
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			
			ObjectUtil.removeChildObject(world, "Slots");
			ObjectUtil.removeChildObject(world, "Starship_exploded_debris_*");
			ObjectUtil.removeChildObject(world, "Starship_cargo_*");
			ObjectUtil.removeChildObject(world, "*starship_work*");
			ObjectUtil.removeChildObject(world, "*DataBox*");
			ObjectUtil.removeChildObject(world, "*Spawner*");
			ObjectUtil.removeChildObject(world, "*PDA*");
			ObjectUtil.removeChildObject(world, "*Wrecks_LaserCutFX*");
			//ObjectUtil.removeChildObject(world, "InteriorEntities");
			//ObjectUtil.removeChildObject(world, "InteriorProps");
			GameObject exterior = ObjectUtil.getChildObject(world, "ExteriorEntities");
			ObjectUtil.removeChildObject(exterior, "ExplorableWreckHull01");
			ObjectUtil.removeChildObject(exterior, "ExplorableWreckHull02");
			
			ObjectUtil.removeComponent<StarshipDoor>(world);
			ObjectUtil.removeComponent<StarshipDoorLocked>(world);
			ObjectUtil.removeComponent<Sealed>(world);
			ObjectUtil.removeComponent<LaserCutObject>(world);
			
			world.EnsureComponent<FallingGFWreckTag>();
				
			Rigidbody rb = world.EnsureComponent<Rigidbody>();
			rb.mass = 2000;
			rb.drag = 0;
			rb.isKinematic = true;
			WorldForces wf = world.EnsureComponent<WorldForces>();
			wf.underwaterGravity = 0.25F;
			wf.underwaterDrag *= 0.33F;
			
			ObjectUtil.fullyEnable(world);
			return world;
	    }
			
	}
		
	internal class FallingGFWreckTag : MonoBehaviour {
		
		private static readonly SoundManager.SoundData[] groanSounds = new SoundManager.SoundData[]{
			SoundManager.registerSound(SeaToSeaMod.modDLL, "wreckgroan1", "Sounds/wreckgroan1.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 180);}, SoundSystem.masterBus),
			SoundManager.registerSound(SeaToSeaMod.modDLL, "wreckgroan2", "Sounds/wreckgroan2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 180);}, SoundSystem.masterBus),
			SoundManager.registerSound(SeaToSeaMod.modDLL, "wreckgroan3", "Sounds/wreckgroan3.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 180);}, SoundSystem.masterBus),
		};
		private static readonly SoundManager.SoundData[] hitSounds = new SoundManager.SoundData[]{
			SoundManager.registerSound(SeaToSeaMod.modDLL, "wreckhit1", "Sounds/wreckhit1.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 180);}, SoundSystem.masterBus),
			SoundManager.registerSound(SeaToSeaMod.modDLL, "wreckhit2", "Sounds/wreckhit2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 180);}, SoundSystem.masterBus),
		};
		
		private static readonly SoundManager.SoundData impactSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "wreckland", "Sounds/wreckland2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
		
		private static readonly Vector3 root = new Vector3(-135.45F, -194.82F, 849.19F);

		private PrefabIdentifier prefab;
		private Rigidbody mainBody;
		//private Collider[] colliders;
		private List<MeshRenderer> doors = new List<MeshRenderer>();
		
		private bool isFalling = false;
		private float fallTime = 0;
		
		private BoxCollider box;
		
		private Vector3 lastPos;
		
		private float nextShakeableTime = 0;
		
		void Start() {
			ObjectUtil.fullyEnable(gameObject);
		}
		
		void Update() {
			if (!mainBody)
				mainBody = GetComponentInChildren<Rigidbody>();
			if (!prefab)
				prefab = GetComponentInChildren<PrefabIdentifier>();
			
			//transform.localScale = Vector3.one*0.8F;
			
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = Time.deltaTime;
			if (dT <= 0)
				return;
			Player ep = Player.main;
			if (isFalling) {
				if ((ep && Vector3.Distance(ep.transform.position, transform.position) >= 300) || transform.position.y <= -475) {
					doImpactFX();
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
				else {
					fallTime += dT;
					foreach (MeshRenderer mr in doors) {
						if (mr && mr.gameObject)
							mr.gameObject.transform.localPosition = Vector3.zero;
					}
					Vector3 diff = (transform.position-root).normalized;
					if (fallTime < 1.5F) {
						transform.RotateAround(root, new Vector3(0, 0, 1), 25*dT*fallTime);
						mainBody.MovePosition(transform.position+diff*dT*10*fallTime);
						if (UnityEngine.Random.Range(0F, 1F) < 0.067F)
							doUnstableFX(true, true);
					}
					else {
						mainBody.isKinematic = false;
						box.enabled = (transform.position-lastPos).magnitude > 0.01F || mainBody.angularVelocity.magnitude > 5;
						mainBody.velocity = Vector3.down*15+diff*10F;
					}
				}
			}
			else {
				if (ep && Vector3.Distance(ep.transform.position, transform.position) <= 30) {
					//LOS
					if (WorldUtil.lineOfSight(gameObject, ep.gameObject, r => !r.collider.gameObject.FindAncestor<Vehicle>()) && ObjectUtil.isOnScreen(gameObject, Camera.main)) {
						startFall();
					}
				}
				if (UnityEngine.Random.Range(0F, 1F) < 0.0075F)
					doUnstableFX(UnityEngine.Random.Range(0F, 1F) < 0.5F && DayNightCycle.main.timePassedAsFloat >= nextShakeableTime, false);
			}
			
			lastPos = transform.position;
		}
		
		private void doUnstableFX(bool sound, bool groan) {
			Collider[] boxes = GetComponentsInChildren<Collider>(true);
			for (int i = 0; i < 14; i++) {
				Collider c = boxes[UnityEngine.Random.Range(0, boxes.Length)];
				Vector3 pos = MathUtil.getRandomVectorBetween(c.bounds.min, c.bounds.max);
				if (UnityEngine.Random.Range(0F, 1F) <= 0.75) {
					ParticleSystem fx = WorldUtil.spawnParticlesAt(pos, "ee56cc29-1da3-41d7-8cf3-d8f028cb9559", 5);
					ParticleSystem.MainModule mod = fx.main;
					mod.duration *= 1.5F;
					mod.startSizeMultiplier *= 4.5F;
					mod.startSize = 6;
					mod.startColor = new Color(0.3F, 0.4F, 0.7F);
					ParticleSystem.EmissionModule emit = fx.emission;
					emit.rateOverTimeMultiplier *= 16;
					//ParticleSystem.ShapeModule shape = fx.shape;
					//if (shape != null)
					//	shape.radius *= 9;
					//fx.main = mod;
				}
				else {
					ParticleSystem fx = getSmokeFX();
					fx.transform.position = pos;
					WorldUtil.setParticlesTemporary(fx, UnityEngine.Random.Range(0.33F, 0.75F));
					ParticleSystem.MainModule mod = fx.main;
					mod.duration *= 5F;
					mod.startLifetime = 2;
					mod.startSizeMultiplier = 15F;
					mod.startSize = 8;
					mod.gravityModifier = 0.3F;
					ParticleSystem.ColorOverLifetimeModule clr = fx.colorOverLifetime;
					ParticleSystem.MinMaxGradient color = clr.color;
					color.mode = ParticleSystemGradientMode.TwoColors;
					color.colorMin = new Color(0.2F, 0.2F, 0.15F, 0.7F);
					color.colorMax = new Color(0.2F, 0.2F, 0.15F, 0);
					ParticleSystem.VelocityOverLifetimeModule vel = fx.velocityOverLifetime;
					vel.zMultiplier = -3;
					//mod.startColor = new Color(0.2F, 0.2F, 0.15F);
					//ParticleSystem.ColorOverLifetimeModule clr = fx.colorOverLifetime;
					//clr.enabled = false;
				}
			}
			if (sound) {
				SoundManager.SoundData snd = groan ? groanSounds[UnityEngine.Random.Range(0, groanSounds.Length)] : hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)];
				SoundManager.playSoundAt(snd, transform.position, false, 120, groan ? 2 : UnityEngine.Random.Range(1F, 1.5F));
				float intensity = 1-Vector3.Distance(Player.main.transform.position, transform.position)/100F;
				if (intensity > 0) {
					intensity = Mathf.Pow(intensity, 1.6F);
					if (groan)
						intensity *= 0.67F;
					float dur = UnityEngine.Random.Range(2F, 4F);
					SNUtil.shakeCamera(dur, intensity*UnityEngine.Random.Range(1F, 2F), UnityEngine.Random.Range(2.5F, 4F));
					nextShakeableTime = DayNightCycle.main.timePassedAsFloat+UnityEngine.Random.Range(0.5F, 1.5F);
				}
			}
		}
		
		private void doImpactFX() {
			ParticleSystem fx = getSmokeFX();
			fx.transform.position = Player.main.transform.position+Vector3.down*125;//transform.position.setY(-320);
			WorldUtil.setParticlesTemporary(fx, 2.5F, 60);
			ParticleSystem.MainModule mod = fx.main;
			mod.duration *= 10F;
			mod.startLifetimeMultiplier *= 20;//10F;
			mod.startSizeMultiplier = 250;//120;//75F;
			mod.startSize = 30;//8;
			mod.startColor = new Color(1F, 0.6F, 0.4F);
			mod.gravityModifier = 0;//0.05F;
			//ParticleSystem.EmissionModule emit = fx.emission;
			//emit.rateOverTimeMultiplier *= 16;
			ParticleSystem.ShapeModule shape = fx.shape;
			shape.radius = 8;//4;//12;
			//fx.main = mod;
			ParticleSystem.ColorOverLifetimeModule clr = fx.colorOverLifetime;
			//clr.enabled = false;
			ParticleSystem.MinMaxGradient color = clr.color;
			color.mode = ParticleSystemGradientMode.TwoColors;
			color.colorMin = new Color(1F, 0.6F, 0.4F);
			color.colorMax = new Color(1F, 0.6F, 0.4F, 0);
			ParticleSystem.VelocityOverLifetimeModule vel = fx.velocityOverLifetime;
			vel.zMultiplier = 30;//15;
			SoundManager.playSoundAt(impactSound, Player.main.transform.position, false, -1, 2);
			SoundManager.playSoundAt(impactSound, Player.main.transform.position, false, -1, 2);
			//foreach (SoundManager.SoundData snd in hitSounds)
			//	SoundManager.playSoundAt(snd, Player.main.transform.position, false, 120, 2);
		}
		
		private ParticleSystem getSmokeFX() {
			GameObject go = ObjectUtil.createWorldObject("5ce9eb7b-064b-46e6-ae7b-43fc4bd016c3");
			UnityEngine.Object.DestroyImmediate(go.GetComponent<ParticleSystem>());
			GameObject child = ObjectUtil.getChildObject(go, "xSmk");
			foreach (Transform t in go.transform) {
				if (t != child.transform)
					UnityEngine.Object.DestroyImmediate(t.gameObject);
			}
			return child.GetComponent<ParticleSystem>();
		}
		
		private void startFall() {
			foreach (Collider c in GetComponentsInChildren<Collider>())
				UnityEngine.Object.Destroy(c.gameObject);
			box = gameObject.EnsureComponent<BoxCollider>();
			box.center = new Vector3(5, 7F, -7.5F);
			box.size = new Vector3(20, 15, 35);
			box.isTrigger = false;
			box.enabled = false;
			
			Story.StoryGoal.Execute(FallingGlassForestWreck.STORY_TAG, Story.GoalType.Story);
			
			isFalling = true;
			//mainBody.isKinematic = true;
			foreach (StarshipDoor d in GetComponentsInChildren<StarshipDoor>()) {
				Vector3 pos = d.transform.position;
				Quaternion rot = d.transform.rotation;
				d.transform.SetParent(transform);
				d.transform.position = pos;
				d.transform.rotation = rot;
				doors.AddRange(d.GetComponentsInChildren<MeshRenderer>());
			}
			ObjectUtil.removeComponent<Animator>(gameObject);
			ObjectUtil.removeComponent<StarshipDoor>(gameObject);
			ObjectUtil.removeComponent<StarshipDoorLocked>(gameObject);
			ObjectUtil.removeComponent<Sealed>(gameObject);
			ObjectUtil.removeComponent<LaserCutObject>(gameObject);
			ObjectUtil.removeComponent<TrailRenderer>(gameObject);
			ObjectUtil.removeChildObject(gameObject, "*Wrecks_LaserCutFX*");
			
			doUnstableFX(true, true);
		}
		
		void OnDestroy() {
			
		}
		
		void OnDisable() {
			//if (transform.position.y <= -400)
			//	UnityEngine.Object.Destroy(gameObject);
		}
		
	}
}
