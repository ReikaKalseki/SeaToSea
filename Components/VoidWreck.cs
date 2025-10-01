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

	public class VoidWreck : MonoBehaviour, Ecocean.VoidBubbleReaction {

		private float lastTickTime = -1;

		void Start() {
			CustomPrefab def = (CustomPrefab)SeaToSeaMod.worldgen.getByID("d28a3bf7-880f-4a4f-a8a5-c706da344a7b");
			//SNUtil.log("Restoring void wreck from def:\n"+def.xmlString);
			WreckDoorSwaps doors = def.getManipulations<WreckDoorSwaps>()[0];
			doors.applyToObject(gameObject);

		}

		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastTickTime < 0.2F)
				return;
			if (WreckDoorSwaps.areWreckDoorSwapsPending(gameObject))
				return;
			lastTickTime = time;
			GameObject go = gameObject.getChildObject("ExplorableWreck2_clean(Clone)/explorable_wreckage_03");
			go.removeChildObject("exterior_01");
			go.removeChildObject("exterior_02");
			//go.removeChildObject("exterior_03");
			go.removeChildObject("hull_01");
			go.removeChildObject("hull_03");
			go.removeChildObject("room_04/room_04/exterior_04");
			foreach (Transform t in gameObject.getChildObject("Decoration").transform) {
				string n = t.name.ToLowerInvariant();
				if (n.Contains("(placeholder)")) {
					t.gameObject.destroy();
				}
				else if (n.Contains("starship_cargo")) {
					setupGravityAndDelete(t.gameObject);
				}
				else if ((n.Contains("starship_girder") || n.Contains("starship_wires")) && go.transform.position.y < -425) {
					setupGravityAndDelete(t.gameObject);
				}
				else if (n.Contains("starship_doors_door") && Vector3.Distance(t.position, new Vector3(-319.29F, -433.01F, -1739.14F)) <= 0.5F) {
					t.GetComponent<Rigidbody>().mass = 1000000;
					t.gameObject.EnsureComponent<ImmuneToPropulsioncannon>();
				}
			}
			foreach (Transform t in gameObject.getChildObject("Interactable").transform) {
				string n = t.name.ToLowerInvariant();
				if (n.Contains("(placeholder)")) {
					t.gameObject.destroy();
				}
				else if (n.Contains("starship_exploded_debris_29") && Vector3.Distance(t.position, new Vector3(-280.15F, -429.08F, -1764.08F)) <= 0.5F) {
					t.gameObject.destroy();
				}
				else if (n.Contains("starship_exploded_debris") || n.Contains("starship_girder")) {
					setupGravityAndDelete(t.gameObject);
				}
				else if (n.Contains("ventcover") && Vector3.Distance(t.position, new Vector3(-324.86F, -435.89F, -1752.78F)) <= 0.5F) {
					t.gameObject.destroy();
				}
			}
			foreach (SupplyCrate sc in WorldUtil.getObjectsNearWithComponent<SupplyCrate>(transform.position, 100)) {
				sc.gameObject.destroy();
			}
		}

		private void setupGravityAndDelete(GameObject go) {
			go.EnsureComponent<VoidWreckFallingPiece>();
		}

		public void onVoidBubbleTouch(Ecocean.VoidBubbleTag tag) {
			tag.fade(1);
		}

	}

	class VoidWreckFallingPiece : MonoBehaviour {

		private float age;

		void Start() {
			gameObject.applyGravity();
		}

		void Update() {
			age += Time.deltaTime;
			if (transform.position.y < -500)
				gameObject.destroy();
			if (age >= 30) {
				GetComponent<Rigidbody>().isKinematic = true;
				//this.destroy();
			}
		}

	}
}
