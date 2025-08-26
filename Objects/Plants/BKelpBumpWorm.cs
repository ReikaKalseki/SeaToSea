using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class BKelpBumpWorm : Spawnable {

		internal BKelpBumpWorm(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			OnFinishedPatching += () => {
				SNUtil.addPDAEntry(this, 3, e.getField<string>("category"), e.pda, e.getField<string>("header"), null);
			};
		}

		public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject(VanillaFlora.TIGER.getPrefabID());
			go.removeComponent<SpikePlant>();
			go.removeComponent<LiveMixin>();
			go.removeComponent<RangeAttacker>();
			go.removeComponent<RangeTargeter>();
			go.removeComponent<RangedAttackLastTarget>();
			go.removeComponent<AttackLastTarget>();
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
			go.transform.localScale = new Vector3(2, 2, 2);
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Creature/BKelpBumpWorm");
				RenderUtil.enableAlpha(r.materials[0], 0.2F);
				//looks bad RenderUtil.makeTransparent(r.materials[0]);
				r.materials[0].SetFloat("_Shininess", 0);
				r.materials[0].SetFloat("_SpecInt", 0.2F);
				r.materials[0].SetFloat("_Fresnel", 0F);
				RenderUtil.setEmissivity(r, 0.75F);
			}
			Animator a = go.GetComponentInChildren<Animator>();
			a.speed = 4;
			SphereCollider sc = go.GetComponentInChildren<SphereCollider>();
			sc.radius *= 1.25F;
			sc.transform.localPosition += Vector3.up * 0.4F;
			sc.gameObject.EnsureComponent<BKelpBumpWormInteractTag>();
			go.EnsureComponent<BKelpBumpWormTag>();
			ObjectUtil.makeMapRoomScannable(go, C2CItems.bkelpBumpWormItem.TechType);
			return go;
		}

		public class BKelpBumpWormTag : MonoBehaviour {

			public static readonly float REGROW_TIME = 5400; //90 min, but do not serialize, so will reset if leave and come back

			private Animator animator;
			private SphereCollider collider;

			private float lastCollect = -9999;

			void Start() {
				animator = this.GetComponentInChildren<Animator>();
				this.Invoke("cleanup", 1);
				collider = this.GetComponentInChildren<SphereCollider>();
			}

			void Update() {
				animator.speed = 4;
				bool visible = DayNightCycle.main.timePassedAsFloat-lastCollect >= REGROW_TIME;
				animator.gameObject.SetActive(visible);
				collider.gameObject.SetActive(visible);
			}

			void cleanup() {
				this.cleanup(4.5F);
			}

			void cleanup(float r) {
				bool trig = Physics.queriesHitTriggers;
				Physics.queriesHitTriggers = true;
				foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(transform.position, r)) {
					if (VanillaFlora.DEEP_MUSHROOM.includes(pi.ClassId)) {
						pi.gameObject.destroy(false);
					}
				}
				Physics.queriesHitTriggers = trig;
			}

			public bool collect() {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time - lastCollect < REGROW_TIME)
					return false;
				InventoryUtil.addItem(C2CItems.bkelpBumpWormItem.TechType);
				lastCollect = time;
				return true;
			}

		}

		class BKelpBumpWormInteractTag : MonoBehaviour, IHandTarget {

			private BKelpBumpWormTag owner;

			void Start() {
				owner = gameObject.FindAncestor<BKelpBumpWormTag>();
			}

			public void OnHandHover(GUIHand hand) {
				HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
				HandReticle.main.SetInteractText("BKelpBumpWormClick");
				HandReticle.main.SetTargetDistance(8);
			}

			public void OnHandClick(GUIHand hand) {
				owner.collect();
			}

		}

	}
}
