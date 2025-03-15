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

namespace ReikaKalseki.SeaToSea {
	
	[Obsolete]
	public class VoidWreck : MonoBehaviour, Ecocean.VoidBubbleReaction {
		
		//private bool acted;
		//private float timeToAct;
		
		//private float lastCheckTime = -1;
			
		void Start() {
			ObjectUtil.fullyEnable(gameObject);
			foreach (PrefabPlaceholdersGroup pg in GetComponentsInChildren<PrefabPlaceholdersGroup>(true)) {
				UnityEngine.Object.DestroyImmediate(pg);
			}
			
			foreach (PrefabPlaceholder pp in GetComponentsInChildren<PrefabPlaceholder>()) {
				UnityEngine.Object.DestroyImmediate(pp.gameObject);
			}
			Invoke("Apply", 2.0F);
		}
		
		void Apply() {
			//SNUtil.writeToChat("Initializing void wreck");
			ObjectUtil.removeChildObject(gameObject, "Slots");
			ObjectUtil.removeChildObject(gameObject, "*DataBox*");
			ObjectUtil.removeChildObject(gameObject, "*Spawner*");
			ObjectUtil.removeChildObject(gameObject, "*PDA*");
			ObjectUtil.removeChildObject(gameObject, "Decoration");
			ObjectUtil.removeChildObject(gameObject, "Interactable");
			
			//ObjectUtil.removeChildObject(gameObject, "ExplorableWreck2_clean/explorable_wreckage_03/wire_huge");
			
			GameObject wire = ObjectUtil.getChildObject(gameObject, "ExplorableWreck2_clean/explorable_wreckage_03/wire_huge");
			ObjectUtil.removeChildObject(wire, "wire_huge_01"); //the top cyl
			ObjectUtil.removeChildObject(wire, "wire_huge_collision/Capsule");
			ObjectUtil.removeChildObject(wire, "wire_huge_collision/Capsule (1)");
			
			GameObject panel0 = ObjectUtil.getChildObject(gameObject, "ExplorableWreck2_clean/explorable_wreckage_03/hull_01");
			GameObject panel = UnityEngine.Object.Instantiate(panel0);
			Renderer r = panel0.GetComponentInChildren<Renderer>();
			r.materials[0].SetFloat("_SpecInt", 0.2F);
			r.materials[0].SetColor("_Color", new Color(0.16F, 0.15F, 0.19F, 1));
			r.materials[2].SetFloat("_SpecInt", 0.1F);
			r.materials[2].SetColor("_Color", new Color(0.14F, 0.2F, 1.7F, 1));
			panel.transform.SetParent(panel0.transform.parent);
			panel.transform.localRotation = Quaternion.Euler(0, 50, 0);
			panel.transform.localPosition = new Vector3(-29.00F, -4.00F, -19.00F);
			//ObjectUtil.removeChildObject(gameObject, "ExplorableWreck2_clean/explorable_wreckage_03/hull_01");
			ObjectUtil.removeChildObject(gameObject, "ExplorableWreck2_clean/explorable_wreckage_03/hull_02");
			ObjectUtil.removeChildObject(gameObject, "ExplorableWreck2_clean/explorable_wreckage_03/hull_03");
			//GameObject panel = ObjectUtil.getChildObject(gameObject, "ExplorableWreck2_clean/explorable_wreckage_03/hull_03");
			//panel.transform.localRotation = Quaternion.Euler(0, 21, 0);
				/*
			GameObject g1 = ObjectUtil.getChildObject(gameObject, "Decoration");
			GameObject g2 = ObjectUtil.getChildObject(gameObject, "Interactable");
			
			Vector3 barPos = new Vector3(-69.61F, -466.25F, -1847.02F);
			foreach (GameObject bar in ObjectUtil.getChildObjects(g2, "Starship_exploded_debris_19")) {
				if (Vector3.Distance(bar.transform.position, barPos) < 0.5F)
					UnityEngine.Object.DestroyImmediate(bar);
			}
				
			foreach (BlueprintHandTarget pp in g1.GetComponentsInChildren<BlueprintHandTarget>()) {
				UnityEngine.Object.DestroyImmediate(pp.gameObject);
			}
			foreach (BlueprintHandTarget pp in g2.GetComponentsInChildren<BlueprintHandTarget>()) {
				UnityEngine.Object.DestroyImmediate(pp.gameObject);
			}
			foreach (StoryHandTarget pp in g1.GetComponentsInChildren<StoryHandTarget>()) {
				UnityEngine.Object.DestroyImmediate(pp.gameObject);
			}
			foreach (StoryHandTarget pp in g2.GetComponentsInChildren<StoryHandTarget>()) {
				UnityEngine.Object.DestroyImmediate(pp.gameObject);
			}
				
			foreach (PrefabIdentifier rb in g1.GetComponentsInChildren<PrefabIdentifier>()) {
				applyToRB(rb);
			}
			foreach (PrefabIdentifier rb in g2.GetComponentsInChildren<PrefabIdentifier>()) {
				applyToRB(rb);
			}*/
		}
			
		private void applyToRB(PrefabIdentifier pi) {
			string n = pi.name.ToLowerInvariant();
			if (!n.Contains("modular_wall") && !n.Contains("details") && !n.Contains("virtualentity") && !n.Contains("door") && !n.Contains("vent") && n[0] != 'x' && !n.Contains("crack") && !n.Contains("engine_console") && !n.Contains("wires") && !n.Contains("wall_planter") && !n.Contains("monitor") && !n.Contains("tech_box")) {
				//acted = true;
				Collider cc = pi.GetComponentInChildren<Collider>(true);
				if (cc) {
					ObjectUtil.applyGravity(pi.gameObject);
					PhysicsSettlingProp prop = pi.gameObject.EnsureComponent<PhysicsSettlingProp>();
					prop.init("voidwreck", 40);
					prop.destroyCondition = (p) => p.transform.position.y < -600;
					prop.bump(UnityEngine.Random.Range(5F, 10F));
				}
				else {
					UnityEngine.Object.DestroyImmediate(pi.gameObject);
				}
			}
		}
			/*
		void Update() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (!acted) {
				if (time >= timeToAct) {
					Apply();
					acted = true;
				}
				else if (time - lastCheckTime >= 1) {
					lastCheckTime = time;
					Apply();
				}
			}
		}
		*/
		public void onVoidBubbleTouch(Ecocean.VoidBubbleTag tag) {
			tag.fade(1);
		}
			
	}
}
