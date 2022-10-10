using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class Platinum : BasicCustomOre {
		
		public Platinum(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
			
		}
		
		public override void prepareGameObject(GameObject go, Renderer[] r) {
			base.prepareGameObject(go, r);
			
			
			go.EnsureComponent<PlatinumTag>();
		}
		
	}
	
	class PlatinumTag : MonoBehaviour {
		
		private float lastTime;
		
		private float lastPickupTime;
		private float timeOnGround;
		private DeepStalkerTag currentStalker;
		
		private ResourceTracker resource;
		
		void Start() {
    		
		}
		
		void Update() {
			if (!resource)
				resource = gameObject.GetComponent<ResourceTracker>();
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = time-lastTime;
			if (dT >= 1) {
				gameObject.EnsureComponent<ResourceTrackerUpdater>().tracker = resource;
			}
			lastTime = time;
			if (currentStalker)
				timeOnGround = 0;
			else
				timeOnGround += dT;
		}
		
		public void pickup(DeepStalkerTag s) {
			currentStalker = s;
			lastPickupTime = DayNightCycle.main.timePassedAsFloat;
		} 
		
		public void drop() {
			currentStalker = null;
		}
		
		public float getTimeOnGround() {
			return timeOnGround;
		}
		
	}
}
