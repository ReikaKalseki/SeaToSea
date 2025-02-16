using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class DamagedDataboxSystem {
		
		public static readonly DamagedDataboxSystem instance = new DamagedDataboxSystem();
		
		private readonly Dictionary<Vector3, DamagedDatabox> data = new Dictionary<Vector3, DamagedDatabox>();
		
		private DamagedDataboxSystem() {
			data[C2CHooks.crashMesa] = new DamagedDatabox(15, 0, 10);
			data[C2CHooks.trailerBaseBioreactor] = new DamagedDatabox(30, 0, 30);
			data[VoidSpikesBiome.signalLocation] = new DamagedDatabox(20, 0.33F, 60);
			data[SeaToSeaMod.treaderSignal.initialPosition] = new DamagedDatabox(30, 0.15F, 10);
			data[new Vector3(-114.6F, -234.5F, 854)] = new DamagedDatabox(20, 0.4F); //autofarmer
			data[new Vector3(986.3F, -271.8F, 1378.6F)] = new DamagedDatabox(45, 0.85F); //rebreather v2
			data[new Vector3(110, -264, -369)] = new DamagedDatabox(20, 0.25F, 2); //high cap tank in jellyshroom
			data[new Vector3(-809.76F, -302.24F, -876.86F)] = new DamagedDatabox(20, 0.25F, 2); //high cap tank by keen
		}
		
		internal void onDataboxSpawn(GameObject go) {
			Vector3 pos = go.transform.position;
			foreach (KeyValuePair<Vector3, DamagedDatabox> kvp in data) {
				if (Vector3.Distance(kvp.Key, pos) <= kvp.Value.searchRadius) {
					LiveMixin lv = go.EnsureComponent<LiveMixin>();
		    		lv.data = new LiveMixinData();
		    		lv.data.maxHealth = kvp.Value.secondsToRepair*35;
		    		lv.data.weldable = true;
		    		lv.health = Mathf.Max(0.01F, kvp.Value.initialRepairFraction*lv.data.maxHealth);
		    		go.EnsureComponent<BrokenDatabox>();
		    		go.EnsureComponent<ImmuneToPropulsioncannon>();
		    		SNUtil.log("Damaging databox "+go+" @ "+go.transform.position+": "+kvp.Value);
				}
			}
		}
		
		class BrokenDatabox : MonoBehaviour {
			
			private VFXController sparker;
			private LiveMixin live;
			
			private bool isSparking;
			
			void Update() {
				if (!live) {
					live = GetComponent<LiveMixin>();
				}
				if (!sparker) {
					GameObject welder = ObjectUtil.createWorldObject("9ef36033-b60c-4f8b-8c3a-b15035de3116", false, false);
					sparker = UnityEngine.Object.Instantiate(welder.GetComponent<Welder>().fxControl);
					sparker.transform.parent = transform;
					sparker.transform.localPosition = new Vector3(0, 0.2F, 0);
					sparker.transform.eulerAngles = Vector3.zero;
					sparker.transform.localScale = Vector3.one*3;
					sparker.gameObject.SetActive(true);
				}
				if (live && live.health >= live.maxHealth) {
					sparker.StopAndDestroy(0);
					isSparking = false;
				}
				else if (UnityEngine.Random.Range(0, isSparking ? 40 : 15) == 0) {
					if (isSparking)
						sparker.StopAndDestroy(0);
					else
						sparker.Play(0);
					isSparking = !isSparking;
				}
			}
			
		}
		
		class DamagedDatabox {
			
			public readonly float secondsToRepair;
			public readonly float initialRepairFraction;
			public readonly float searchRadius;
			
			internal DamagedDatabox(float s, float f, float r = 2) {
				secondsToRepair = s;
				initialRepairFraction = f;
				searchRadius = r;
			}
			
			public override string ToString()
			{
				return string.Format("[DamagedDatabox SecondsToRepair={0}, InitialRepairFraction={1}, SearchRadius={2}]", secondsToRepair, initialRepairFraction, searchRadius);
			}

			
		}
	}
	
}
