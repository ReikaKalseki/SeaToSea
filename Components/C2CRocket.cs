﻿using System;
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

namespace ReikaKalseki.SeaToSea {
	
	internal class C2CRocket : MonoBehaviour {
		
		private Rocket rocket;
		private RocketLocker[] lockers;
		private readonly List<StorageContainer> containers = new List<StorageContainer>();
			
		private float lastPDAUpdate = -1;
		
		void Awake() {
			getLockers();
		}
		
		void getLockers() {
		   	lockers = GetComponentsInChildren<RocketLocker>();
		   	containers.Clear();
		    foreach (RocketLocker cl in lockers) {
		        StorageContainer sc = cl.GetComponent<StorageContainer>();
		        containers.Add(sc);
		        sc.Resize(6, 8);
		    }	
		}

		void Update() {
		   	if (C2CHooks.skipRocketTick)
		   		return;
		   	if (!rocket)
		   		rocket = GetComponent<Rocket>();
		   	if (lockers == null)
		   		getLockers();
		    	
		   	float time = DayNightCycle.main.timePassedAsFloat;
		   	if (time-lastPDAUpdate >= 0.5F) {
		   		lastPDAUpdate = time;		   		
		   		FinalLaunchAdditionalRequirementSystem.instance.updateContentsAndPDAPageChecklist(rocket, containers);
		   	}
		}
			
	}
}
