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

	public class TrailerBaseLabEquipmentSpawner : WorldGenerator {

		internal static readonly Dictionary<string, int> itemList = new Dictionary<string, int>();

		static TrailerBaseLabEquipmentSpawner() {
			addItem("1faf2b57-ff4f-4ea5-a715-7cc5ff6aae60", 7);
			addItem("1b0b7f6d-9793-469c-9872-dfe690834fee", 4);
			addItem("7f601dd4-0645-414d-bb62-5b0b62985836", 6);
			addItem("a227d6b6-d64c-4bf0-b919-2db02d67d037", 5);
			addItem("d6389e01-f2cd-4f9d-a495-0867753e44f0", 4);
			addItem("e7f9c5e7-3906-4efd-b239-28783bce17a5", 2);
		}

		public TrailerBaseLabEquipmentSpawner(Vector3 pos) : base(pos) {

		}

		public override void saveToXML(XmlElement e) {

		}

		public override void loadFromXML(XmlElement e) {

		}

		public override bool generate(List<GameObject> li) {
			foreach (KeyValuePair<string, int> kvp in TrailerBaseLabEquipmentSpawner.itemList) {
				for (int i = 0; i < kvp.Value; i++) {
					GameObject go = spawner(kvp.Key);
					go.transform.position = MathUtil.getRandomVectorAround(position, 0.4F);
					go.GetComponent<Rigidbody>().isKinematic = false;
					go.EnsureComponent<WorldForces>().underwaterGravity = 3;
					go.transform.localRotation = UnityEngine.Random.rotationUniform;
					TrailerBaseLabEquipmentItem prop = go.EnsureComponent<TrailerBaseLabEquipmentItem>();
					prop.Invoke("fixInPlace", 45);
				}
			}
			return true;
		}

		public override LargeWorldEntity.CellLevel getCellLevel() {
			return LargeWorldEntity.CellLevel.Medium;
		}

		public static void addItem(string item, int amt) {
			itemList[item] = itemList.ContainsKey(item) ? itemList[item] + amt : amt;
		}

	}

	class TrailerBaseLabEquipmentItem : MonoBehaviour {

		private static readonly Vector3 vent1 = new Vector3(-134.15F, -501, 940.29F);
		private static readonly Vector3 vent2 = new Vector3(-125.20F, -503, 936.16F);

		private Rigidbody body;

		private float time;

		void Update() {
			if (!body)
				body = this.GetComponentInChildren<Rigidbody>();
			time += Time.deltaTime;
			Vector3 pos = transform.position;

			if (time > 4F && body.velocity.magnitude < 0.03)
				this.fixInPlace();
		}

		void fixInPlace() {
			body.isKinematic = true;
		}

	}
}
