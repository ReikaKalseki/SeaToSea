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
	
	public class PurpleHolefish : RetexturedFish {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
	    internal PurpleHolefish(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.HOLEFISH.prefab) {
			locale = e;
			glowIntensity = 0.5F;
			
			scanTime = 4;
			
			eggBase = TechType.GasopodEgg;
			bigEgg = false;
			eggMaturationTime = 1200;
			eggSpawnRate = 1.5F;
			eggSpawns.Add(BiomeType.UnderwaterIslands_IslandCaveFloor);
	    }
			
		public override void prepareGameObject(GameObject world, Renderer[] r0) {
			PurpleHolefishTag kc = world.EnsureComponent<PurpleHolefishTag>();
			foreach (Renderer r in r0) {
				r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
				RenderUtil.setGlossiness(r, 3F, 0, 1F);
			}
			world.GetComponent<SwimBehaviour>().turnSpeed *= 0.125F;
			world.GetComponent<SwimRandom>().swimVelocity *= 0.25F;
			world.GetComponent<SwimRandom>().swimForward *= 0.5F;
			world.GetComponent<Locomotion>().maxVelocity *= 0.25F;
			world.GetComponentInChildren<AnimateByVelocity>().animSpeedValue *= 0.25F;
			world.GetComponentInChildren<AnimateByVelocity>().animationMoveMaxSpeed *= 0.25F;
			world.GetComponent<SplineFollowing>().inertia *= 2;
			ObjectUtil.removeComponent<FleeWhenScared>(world);
			ObjectUtil.removeComponent<Scareable>(world);
			ObjectUtil.removeComponent<Pickupable>(world);
	    }
		
		public override BehaviourType getBehavior() {
			return BehaviourType.MediumFish;
		}
			
	}
	
	class PurpleHolefishTag : MonoBehaviour {
		
		private Renderer[] renders;
		private Animator[] animators;
		
		void Update() {
			transform.localScale = Vector3.one*5;
			if (renders == null)
				renders = GetComponentsInChildren<Renderer>();
			if (animators == null)
				animators = GetComponentsInChildren<Animator>();
			foreach (Animator a in animators)
				a.speed = 0.25F;
		}
		
	}
}
