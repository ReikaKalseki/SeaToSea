using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class DamagedDataboxSystem {

		public static readonly DamagedDataboxSystem instance = new DamagedDataboxSystem();

		private readonly Dictionary<Vector3, DamagedDatabox> data = new Dictionary<Vector3, DamagedDatabox>();

		private DamagedDataboxSystem() {
			data[C2CHooks.crashMesa] = new DamagedDatabox(20, 0, 10, d: 4);
			data[C2CHooks.trailerBaseBioreactor] = new DamagedDatabox(30, 0, 30, d: 2);
			data[VoidSpikesBiome.signalLocation] = new DamagedDatabox(20, 0.33F, 60, d: 0.5F);
			data[SeaToSeaMod.treaderSignal.initialPosition] = new DamagedDatabox(30, 0.15F, 10, d: 2.5F);
			data[new Vector3(-114.6F, -234.5F, 854)] = new DamagedDatabox(20, 0.4F); //autofarmer
			data[new Vector3(986.3F, -271.8F, 1378.6F)] = new DamagedDatabox(45, 0.85F, d: 1.5F); //rebreather v2
			data[new Vector3(110, -264, -369)] = new DamagedDatabox(20, 0.25F, 2, d: 1F); //high cap tank in jellyshroom
			data[new Vector3(-809.76F, -302.24F, -876.86F)] = new DamagedDatabox(20, 0.25F, 2); //high cap tank by keen
		}

		internal void onDataboxSpawn(GameObject go) {
			DamagedDatabox db = this.getEntry(go.transform.position);
			if (db != null) {
				LiveMixin lv = go.EnsureComponent<LiveMixin>();
				lv.data = new LiveMixinData();
				lv.data.maxHealth = db.secondsToRepair * 35;
				lv.data.weldable = true;
				lv.health = Mathf.Max(0.01F, db.initialRepairFraction * lv.data.maxHealth);
				go.EnsureComponent<BrokenDatabox>();
				go.EnsureComponent<ImmuneToPropulsioncannon>();
				SNUtil.log("Damaging databox " + go + " @ " + go.transform.position + ": " + db);
			}
		}

		internal DamagedDatabox getEntry(Vector3 pos) {
			foreach (KeyValuePair<Vector3, DamagedDatabox> kvp in data) {
				if (Vector3.Distance(kvp.Key, pos) <= kvp.Value.searchRadius) {
					return kvp.Value;
				}
			}
			return null;
		}

		class BrokenDatabox : MonoBehaviour {

			private VFXController sparker;
			private LiveMixin live;

			private bool isSparking;

			private DamagedDatabox entry;

			void Update() {
				if (entry == null) {
					entry = instance.getEntry(transform.position);
				}

				if (!live) {
					live = this.GetComponent<LiveMixin>();
				}
				if (!sparker) {
					GameObject welder = ObjectUtil.createWorldObject("9ef36033-b60c-4f8b-8c3a-b15035de3116", false, false);
					sparker = welder.GetComponent<Welder>().fxControl.clone();
					sparker.transform.parent = transform;
					sparker.transform.localPosition = new Vector3(0, 0.2F, 0);
					sparker.transform.eulerAngles = Vector3.zero;
					sparker.transform.localScale = Vector3.one * 3;
					sparker.gameObject.SetActive(true);
				}
				if (live && live.health >= live.maxHealth) {
					sparker.StopAndDestroy(0);
					isSparking = false;
				}
				else {
					if (entry != null && live.health > live.maxHealth * 0.02F)
						live.TakeDamage(Time.deltaTime * live.maxHealth * 0.01F * entry.degradeRate);
					if (UnityEngine.Random.Range(0, isSparking ? 40 : 15) == 0) {
						if (isSparking)
							sparker.StopAndDestroy(0);
						else
							sparker.Play(0);
						isSparking = !isSparking;
					}
				}
			}

		}

		internal class DamagedDatabox {

			public readonly float secondsToRepair;
			public readonly float initialRepairFraction;
			public readonly float searchRadius;
			public readonly float degradeRate; //1% per second when =1

			internal DamagedDatabox(float s, float f, float r = 2, float d = 0) {
				secondsToRepair = s;
				initialRepairFraction = f;
				searchRadius = r;
				degradeRate = d;
			}

			public override string ToString() {
				return string.Format("[DamagedDatabox SecondsToRepair={0}, InitialRepairFraction={1}, SearchRadius={2}, degradeRate={3}]", secondsToRepair, initialRepairFraction, searchRadius, degradeRate);
			}


		}
	}

}
