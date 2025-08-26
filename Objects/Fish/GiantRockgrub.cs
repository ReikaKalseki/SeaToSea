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

	public class GiantRockGrub : RetexturedFish {

		private readonly XMLLocale.LocaleEntry locale;

		internal GiantRockGrub(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.ROCKGRUB.prefab) {
			locale = e;

			scanTime = 15;

			eggSpawnRate = 0F;
		}

		public override void prepareGameObject(GameObject world, Renderer[] r0) {
			GiantRockGrubTag kc = world.EnsureComponent<GiantRockGrubTag>();

			world.transform.localScale = new Vector3(30, 20, 20);

			world.GetComponent<WorldForces>().underwaterGravity = 3;

			world.GetComponent<LiveMixin>().data.maxHealth *= 20;

			world.removeComponent<Scareable>();

			world.removeComponent<Eatable>();

			world.removeComponent<MoveTowardsTarget>();

			Creature c = world.GetComponent<Creature>();

			AggressiveWhenSeeTarget agg = world.EnsureComponent<AggressiveWhenSeeTarget>();
			agg.targetType = EcoTargetType.Shark;
			agg.aggressionPerSecond = 0.5F;
			agg.creature = c;
			agg.ignoreSameKind = true;

			world.EnsureComponent<AttackLastTarget>().creature = c;
			MeleeAttack me = world.EnsureComponent<MeleeAttack>();
			me.ignoreSameKind = true;
			me.mouth = world;
			me.liveMixin = world.GetComponent<LiveMixin>();
			me.creature = c;

			world.GetComponent<Locomotion>().canWalkOnSurface = true;

			world.removeComponent<SphereCollider>();
			CapsuleCollider cc = world.EnsureComponent<CapsuleCollider>();
			cc.direction = 2;
			cc.height = 0.3F;
			cc.radius = 0.04F;
		}

		public override BehaviourType getBehavior() {
			return BehaviourType.Crab;
		}

	}

	class GiantRockGrubTag : MonoBehaviour {

		private Renderer[] renders;

		private float lastTickTime = -1;

		void Awake() {

		}

		void Update() {
			if (renders == null)
				renders = this.GetComponentsInChildren<Renderer>();
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastTickTime >= 0.5F) {
				lastTickTime = time;
				transform.localScale = new Vector3(30, 20, 20);
			}
		}

	}
}
