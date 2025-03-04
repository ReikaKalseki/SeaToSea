using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using UnityEngine.UI;

using FMOD;
using FMODUnity;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class Drunk : PlayerMovementSpeedModifier {
		
		private static readonly DrunkVisual drunkVisual;
		
		static Drunk() {
	    	drunkVisual = new DrunkVisual();
	    	ScreenFXManager.instance.addOverride(drunkVisual);
		}
			
		private float nextSpeedRecalculation = -1;
		private float nextPushRecalculation = -1;
		private float nextShaderRecalculation = -1;
		private float lastVomitTime = -1;
			
		private float age;
			
		internal Vector3 currentPush;
		private float shaderIntensity;
		private float shaderIntensityTarget;
		private float shaderIntensityMoveSpeed;
		
		public float intensity = 1;
			
		internal Survival survivalObject;
			
		//private Rigidbody player;
			
		protected override void Update() {		
			//if (!player)
			//	player = GetComponent<Rigidbody>();
			float dT = Time.deltaTime;
			age += dT;
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time >= nextSpeedRecalculation) {
				nextSpeedRecalculation = time + UnityEngine.Random.Range(0.5F, 2.5F);
				speedModifier = 1-(UnityEngine.Random.Range(0.2F, 0.75F)*intensity);
			}
			if (time >= nextPushRecalculation) {
				nextPushRecalculation = time + UnityEngine.Random.Range(0.5F, 1.5F);
				currentPush = MathUtil.getRandomVectorAround(Vector3.zero, 1F).setLength(UnityEngine.Random.Range(0.25F, 1.0F))*intensity;
				if (!Player.main.IsSwimming())
					currentPush = currentPush.setY(0);
			}
			if (time >= nextShaderRecalculation) {
				float dur = UnityEngine.Random.Range(0.25F, 2.0F);
				nextShaderRecalculation = time + dur;
				shaderIntensityTarget = UnityEngine.Random.Range(0.33F, 1.5F)*intensity;
				shaderIntensityMoveSpeed = Mathf.Abs(shaderIntensity - shaderIntensityTarget) / dur;
			}			
			if (shaderIntensityTarget > shaderIntensity)
				shaderIntensity = Mathf.Min(shaderIntensityTarget, shaderIntensity + dT * shaderIntensityMoveSpeed);
			else if (shaderIntensityTarget < shaderIntensity)
				shaderIntensity = Mathf.Max(shaderIntensityTarget, shaderIntensity - dT * shaderIntensityMoveSpeed);
			drunkVisual.effect = 4 * shaderIntensity;
			//player.AddForce(currentPush, ForceMode.VelocityChange);
			if (UnityEngine.Random.Range(0F, 1F) < 0.04F)
				SNUtil.shakeCamera(UnityEngine.Random.Range(0.4F, 1.5F), UnityEngine.Random.Range(0.25F, 0.75F), UnityEngine.Random.Range(0.125F, 0.67F));
			if (age > 5F && time - lastVomitTime >= 5F/intensity && UnityEngine.Random.Range(0F, 1F) < 0.001F) {
				lastVomitTime = time;
				SNUtil.vomit(survivalObject, 0, UnityEngine.Random.Range(0F, 2F));
			}
			base.Update();
		}
		    
		void OnDisable() {
			drunkVisual.effect = 0;
		}
		    
		void OnDestroy() {
			OnDisable();
		}
			
		public static Drunk add(float duration) {
			Drunk m = Player.main.gameObject.EnsureComponent<Drunk>();
			m.speedModifier = 1;
			m.elapseWhen = DayNightCycle.main.timePassedAsFloat + duration;
			return m;
		}
		
		internal static void manageDrunkenness(DIHooks.PlayerInput pi) {
			Drunk d = Player.main.GetComponent<Drunk>();
			if (d)
				pi.selectedInput += d.currentPush;
		}
			
	}
		
	class DrunkVisual : ScreenFXManager.ScreenFXOverride {
			
		internal float effect = 0;
			
		internal DrunkVisual() : base(100) {
				
		}
	    	
		public override void onTick() {
			if (effect > 0) {
				ScreenFXManager.instance.registerOverrideThisTick(ScreenFXManager.instance.radialShader);
				ScreenFXManager.instance.radialShader.amount = effect;
			}
		}
			
	}
	
}
