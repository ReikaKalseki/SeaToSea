using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	internal class C2CTreader : MonoBehaviour {

		private readonly List<DeepStalkerTag> stalkers = new List<DeepStalkerTag>();

		void Start() {
			base.InvokeRepeating("tick", 0f, 1);
		}

		void Update() {

		}

		private void OnKill() {
			this.destroy(false);
		}

		void OnDisable() {
			base.CancelInvoke("tick");
		}

		internal void attachStalker(DeepStalkerTag s) {
			if (!stalkers.Contains(s))
				stalkers.Add(s);
		}

		internal void removeStalker(DeepStalkerTag s) {
			stalkers.Remove(s);
		}

		internal void tick() {
			if (C2CHooks.skipTreaderTick)
				return;
			Player ep = Player.main;
			if (ep) {
				float dist = Vector3.Distance(ep.transform.position, transform.position+(transform.forward*10)+(transform.up*0));
				if (dist <= 12) {
					int amt = Inventory.main.GetPickupCount(TechType.SeaTreaderPoop);
					if (amt > 0) {
						float df = Mathf.Clamp01(1.5F/dist);
						float chance = Mathf.Clamp(0.25F*amt, 0, 0.8F)*df;
						//SNUtil.writeToChat(dist+" x "+amt+" > "+df+" > "+chance);
						if (chance > 0 && UnityEngine.Random.Range(0F, 1F) <= chance) {
							gameObject.GetComponent<SeaTreaderMeleeAttack>().OnAttackTriggerEnter(Player.main.GetComponentInChildren<Collider>());
						}
					}
				}
				if (dist <= 120) {
					int amt = DeepStalkerTag.countDeepStalkersNear(transform);
					//int amt = stalkers.Count;
					for (int i = amt; i < 3; i++) {
						GameObject go = ObjectUtil.createWorldObject(C2CItems.deepStalker.ClassID, true, true);
						go.transform.position = MathUtil.getRandomVectorAround(transform.position, 12).setY(transform.position.y + 2);
						go.GetComponent<DeepStalkerTag>().bindToTreader(this.GetComponent<SeaTreader>());
					}
				}
			}
		}

	}
}
