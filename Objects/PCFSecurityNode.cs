using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	public class PCFSecurityNode : InteractableSpawnable {

		//private static readonly WeightedRandom<TechType> dropTable = new WeightedRandom<TechType>();

		internal static readonly float AMBIENT_SOUND_RANGE = 80;

		internal static Dictionary<Vector3, float> nodePositionChecks = new Dictionary<Vector3, float>();
		internal static Dictionary<Vector3, PositionedPrefab> expectedNodeRotations = new Dictionary<Vector3, PositionedPrefab>();

		internal static Sound loopSoundReference;
		internal static readonly SoundManager.SoundData loopSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "pcfnodeambient", "Sounds/pcfnodeambient.ogg", SoundManager.soundMode3D, s => {loopSoundReference = s; SoundManager.setup3D(s, AMBIENT_SOUND_RANGE); SoundManager.setLooping(s);});

		private readonly bool isLiveVariant;

		public PCFSecurityNode(XMLLocale.LocaleEntry e, bool isLive) : base(e) {
			scanTime = 2;
			isLiveVariant = isLive;
		}

		public override GameObject GetGameObject() {
			//GameObject go1 = ObjectUtil.createWorldObject("78009225-a9fa-4d21-9580-8719a3368373").setName("Base"); //block
			GameObject baseObj = ObjectUtil.createWorldObject("473a8c4d-162f-4575-bbef-16c1c97d1e9d"); //light on top/projector base
			GameObject fx = ObjectUtil.lookupPrefab("2834aa49-4721-4d4c-9ccf-13fbbd324745").getChildObject("FX").clone();
			fx.removeChildObject("x_Precursor_ComputerTerminal_Symbol");
			fx.removeChildObject("x_Precursor_ComputerTerminal_SmallSymbol");
			GameObject cone = fx.getChildObject("x_Precursor_ComputerTerminal_Halo");
			cone.transform.localPosition = new Vector3(0, 0, -0.375F);
			Color c = new Color(0.2F, 1.0F, 1.5F, 1F);
			foreach (Renderer r in fx.GetComponentsInChildren<Renderer>()) {
				foreach (Material m in r.materials) {
					m.SetColor("_Color", c);
				}
			}
			GameObject dots = fx.getChildObject("x_Precursor_ComputerTerminal_ScreenBG");
			c = new Color(0.2F, 2.5F, 4F, 1);
			for (float ang = 30; ang < 360; ang += 30) {
				string n = "BCGAng_"+ang.ToString("0");
				GameObject go3b = fx.getChildObject(n);
				if (!go3b)
					go3b = dots.clone().setName("BCGAng_" + ang.ToString("0"));
				go3b.transform.SetParent(fx.transform);
				Utils.ZeroTransform(go3b.transform);
				go3b.transform.localEulerAngles = new Vector3(270, ang, 0);
				go3b.transform.localPosition = new Vector3(0, ang / 360F * 0.8F - 0.4F, 0);
				Material m = go3b.GetComponent<Renderer>().material;
				m.SetColor("_Color", c);
			}
			dots.GetComponent<Renderer>().material.SetColor("_Color", c);
			//go1.transform.SetParent(go.transform);
			//Utils.ZeroTransform(go1.transform);
			baseObj.transform.SetParent(baseObj.transform);
			baseObj.transform.localScale = new Vector3(2, 6, 1.8F);
			fx.transform.SetParent(baseObj.transform);
			Utils.ZeroTransform(fx.transform);
			fx.transform.localScale = new Vector3(0.33F, 1.5F, 0.33F);
			fx.transform.localPosition = new Vector3(0, -2.1F, 0);
			//baseObj.GetComponent<SphereCollider>().radius = 200;
			baseObj.removeComponent<SphereCollider>();
			//BoxCollider bc = baseObj.EnsureComponent<BoxCollider>();
			PCFSecurityNodeTag tag;
			if (isLiveVariant)
				tag = baseObj.EnsureComponent<LivePCFSecurityNodeTag>();
			else
				tag = baseObj.EnsureComponent<BrokenPCFSecurityNodeTag>();
			baseObj.GetComponentInChildren<BoxCollider>().gameObject.EnsureComponent<PCFSecurityNodeRelay>();
			baseObj.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
			ObjectUtil.makeMapRoomScannable(baseObj, Exscansion.ExscansionMod.alienBase.TechType);

			tag.soundLoop = baseObj.EnsureComponent<FMOD_CustomLoopingEmitter>();
			tag.soundLoop.asset = loopSound.asset;
			tag.soundLoop.playOnAwake = false;
			loopSoundReference.getLength(out uint millis, TIMEUNIT.MS);
			tag.soundLoop.length = millis / 1000F;

			return baseObj;
		}

		public void postRegister() {
			countGen(SeaToSeaMod.worldgen);
			registerEncyPage();
		}

		internal static void registerPresentNode(PCFSecurityNodeTag node) {
			Vector3 at = node.transform.position;
			foreach (Vector3 pos in nodePositionChecks.Keys.ToList()) { //toList to make wrapper to avoid concurrent modification
				if ((pos - at).sqrMagnitude < 1) {
					nodePositionChecks[pos] = DayNightCycle.main.timePassedAsFloat;
					return;
				}
			}
			SNUtil.writeToChat("PCF node "+node+" @ " + at + " was in an unexpected place!");
		}

		private static float spawnCheckTimer = 0;

		internal static void registerExpectedSpawns(WorldgenDatabase gen) {
			foreach (PositionedPrefab pos in gen.getPositions(SeaToSeaMod.securityNodeLive.ClassID)) {
				nodePositionChecks[pos.position] = -1;
				expectedNodeRotations[pos.position] = pos;
			}
		}

		internal static void tickSpawnValidation(Player ep) {
			if (nodePositionChecks.Keys.Count == 0)
				return;
			if (ep) {
				spawnCheckTimer += Time.deltaTime;
				if (spawnCheckTimer >= 20) {
					doSpawnCheck(ep);
					spawnCheckTimer = 0;
				}
			}
			else {
				spawnCheckTimer = 0;
			}
		}

		private static void doSpawnCheck(Player ep) {
			float time = DayNightCycle.main.timePassedAsFloat;
			foreach (KeyValuePair<Vector3, float> kvp in nodePositionChecks) {
				Vector3 pos = kvp.Key;
				if (time - kvp.Value >= 20 && (ep.transform.position - pos).sqrMagnitude <= 10000 && WorldUtil.getObjectsNearOfClassIDs(pos, 10, SeaToSeaMod.securityNodeLive.ClassID, SeaToSeaMod.securityNodeBroken.ClassID).Count == 0) {
					SNUtil.writeToChat("Regenerating PCF node @ " + pos);
					expectedNodeRotations[pos].createWorldObject();
				}
			}
		}
	}

	class BrokenPCFSecurityNodeTag : PCFSecurityNodeTag {
		void Update() {
			if (pillarTrigger) {
				pillarTrigger.flare.gameObject.SetActive(false);
				pillarTrigger.enabled = false;
				fxObject.SetActive(false);
				RenderUtil.setEmissivity(baseRender, 0);
			}

			PCFSecurityNode.registerPresentNode(this);
		}
	}

	class LivePCFSecurityNodeTag : PCFSecurityNodeTag {

		private float lastSound = -1;

		private float age;

		void Update() {
			Vector3 dist = (Camera.main.transform.position-fxObject.transform.position);
			float ang = Mathf.Atan2(dist.y, dist.x);
			fxObject.transform.localEulerAngles = new Vector3(0, ang + rotation, 0);

			float dT = Time.deltaTime;
			rotation += dT * 30;
			pillarTrigger.extended = true;
			pillarTrigger.SetGlowColor(new Color(0, 1, 2.5F, 1), false);

			float time = DayNightCycle.main.timePassedAsFloat;

			age += dT;

			if (time - lastSound >= soundLoop.length - 0.1F && age > 1 && pillarTrigger.enabled && fxObject.activeInHierarchy) {
				lastSound = time;
				//SNUtil.playSoundAt(SNUtil.getSound("event:/sub_module/workbench/working"), gameObject.transform.position);
				soundInstance = SoundManager.playSoundAt(PCFSecurityNode.loopSound, gameObject.transform.position);
				if (soundInstance != null && soundInstance.Value.hasHandle()) {
					float dd = dist.sqrMagnitude;
					float f = (float)MathUtil.linterpolate(dd, PCFSecurityNode.AMBIENT_SOUND_RANGE*0.33F, PCFSecurityNode.AMBIENT_SOUND_RANGE*0.8F, 1, 0.1F);
					soundInstance.Value.setVolume(f);
				}
			}

			PCFSecurityNode.registerPresentNode(this);
		}

	}

	abstract class PCFSecurityNodeTag : ImmuneToPropulsioncannon, DIHooks.ImmuneToStasis {

		//private GameObject baseObject;
		protected GameObject fxObject;

		protected PrecursorActivatedPillar pillarTrigger;

		protected Renderer baseRender;

		protected float rotation;

		internal FMOD_CustomLoopingEmitter soundLoop;

		protected Channel? soundInstance;

		private static readonly SoundManager.SoundData breakSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "pcfnodebreak", "Sounds/pcfnodebreak.ogg", SoundManager.soundMode3D);

		void Start() {
			immuneToRepulsionCannon = true;

			//baseObject = gameObject.getChildObject("Base");
			fxObject = gameObject.getChildObject("FX");

			pillarTrigger = GetComponent<PrecursorActivatedPillar>();
			pillarTrigger.isFullyExtended = false;

			pillarTrigger.flare.color = new Color(0.25F, 0.8F, 1F);
			pillarTrigger.flare.intensity = 2.5F;

			GameObject go = pillarTrigger.gameObject.getChildObject("Precursor_prison_exterior_box_01");
			baseRender = go.GetComponent<Renderer>();
			baseRender.material.SetColor("_GlowColor", new Color(0, 1, 2.5F, 1));
		}

		public void BashHit() { //prawn hit
			if (this is BrokenPCFSecurityNodeTag || !pillarTrigger.enabled || !fxObject.activeInHierarchy)
				return;
			C2CProgression.instance.stepPCFSecurity();
			for (int i = 0; i < 20; i++)
				WorldUtil.spawnParticlesAt(transform.position + transform.up * 1.5F, "361b23ed-58dd-4f45-9c5f-072fa66db88a", 0.5F, true);
			SoundManager.playSoundAt(breakSound, transform.position, false, -1);
			/*
			pillarTrigger.flare.gameObject.SetActive(false);
			pillarTrigger.enabled = false;
			RenderUtil.setEmissivity(baseRender, 0);
			fxObject.SetActive(false);
			*/
			GameObject repl = ObjectUtil.createWorldObject(SeaToSeaMod.securityNodeBroken.ClassID);
			repl.transform.SetParent(transform.parent);
			repl.transform.position = transform.position;
			repl.transform.rotation = transform.rotation;
			repl.transform.localScale = transform.localScale;
			gameObject.destroy(false, 0.05F);
			if (soundInstance != null && soundInstance.Value.hasHandle())
				soundInstance.Value.stop();
		}

	}

	class PCFSecurityNodeRelay : MonoBehaviour {
		void BashHit(IExosuitArm arm) { //prawn hit
			if (arm is ExosuitClawArm)
				GetComponentInParent<PCFSecurityNodeTag>().BashHit();
		}
	}
}
