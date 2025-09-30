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

	[Obsolete]
	public class KharaaFog : Spawnable {

		internal KharaaFog() : base("kharaafog", "", "") {

		}

		public override GameObject GetGameObject() {
			GameObject podRef = ObjectUtil.lookupPrefab("bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782");
			GasPod pod = podRef.GetComponent<GasPod>();
			GameObject fog = pod.gasEffectPrefab;
			GameObject world = fog.clone();
			world.SetActive(false);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			SphereCollider sc = world.AddComponent<SphereCollider>();
			sc.radius = 7.5F;
			sc.isTrigger = true;
			KharaaFogComponent kc = world.EnsureComponent<KharaaFogComponent>();
			kc.sphere = sc;
			kc.tracker = world.GetComponent<UWE.TriggerStayTracker>();
			Renderer r = world.GetComponentInChildren<Renderer>();
			return world;
		}

	}

	class KharaaFogComponent : MonoBehaviour {

		private static readonly float damagePerSecond = 2.5F;
		private static readonly float damageInterval = 0.25F; //TODO make healthkits not work here, also do for lost river

		internal SphereCollider sphere;
		internal UWE.TriggerStayTracker tracker;

		private float timeLastDamageTick;

		private void Update() {
			if (timeLastDamageTick + damageInterval <= Time.time) {
				foreach (GameObject go in tracker.Get()) {
					if (go) {
						SNUtil.writeToChat("" + go);
						LiveMixin live = gameObject.GetComponent<LiveMixin>();
						if (live != null && live.IsAlive()) {
							Player component2 = gameObject.GetComponent<Player>();
							if (gameObject.GetComponent<Player>() != null || gameObject.GetComponent<Living>() != null) {
								live.TakeDamage(damagePerSecond * damageInterval, gameObject.transform.position, DamageType.Starve, null);
								SNUtil.writeToChat("" + (damagePerSecond * damageInterval));
							}
						}
					}
				}
				timeLastDamageTick = Time.time;
			}
		}

	}
}
