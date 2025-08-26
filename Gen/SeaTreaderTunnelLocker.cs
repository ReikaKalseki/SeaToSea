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

	public class SeaTreaderTunnelLocker : WorldGenerator {

		internal static readonly Dictionary<TechType, int> itemList = new Dictionary<TechType, int>();

		static SeaTreaderTunnelLocker() {
			addItem(TechType.Titanium, 3);
			addItem(TechType.AluminumOxide, 5);
			addItem(TechType.Diamond, 6);
			addItem(TechType.Gold, 2);

			addItem(TechType.CuredSpadefish, 4);
			addItem(TechType.CookedBladderfish, 3);

			addItem(TechType.FirstAidKit, 1);

			addItem(TechType.SmallMelon, 2);
		}

		public SeaTreaderTunnelLocker(Vector3 pos) : base(pos) {

		}

		public override void saveToXML(XmlElement e) {

		}

		public override void loadFromXML(XmlElement e) {

		}

		public override bool generate(List<GameObject> li) {
			foreach (KeyValuePair<TechType, int> kvp in SeaTreaderTunnelLocker.itemList) {
				for (int i = 0; i < kvp.Value; i++) {
					GameObject go = ObjectUtil.createWorldObject(kvp.Key);
					go.transform.position = MathUtil.getRandomVectorAround(position, 0.4F);
					go.GetComponent<Rigidbody>().isKinematic = false;
					go.transform.localRotation = UnityEngine.Random.rotationUniform;
					if (kvp.Key != TechType.FirstAidKit)
						go.transform.localScale = Vector3.one * (kvp.Key == TechType.CuredSpadefish || kvp.Key == TechType.CookedBladderfish ? 0.67F : 0.25F);
					SeaTreaderTunnelBaseItem prop = go.EnsureComponent<SeaTreaderTunnelBaseItem>();
					prop.Invoke("fixInPlace", 45);
				}
			}
			return true;
		}

		public override LargeWorldEntity.CellLevel getCellLevel() {
			return LargeWorldEntity.CellLevel.Medium;
		}

		public static void addItem(TechType item, int amt) {
			itemList[item] = itemList.ContainsKey(item) ? itemList[item] + amt : amt;
		}

	}

	class SeaTreaderTunnelBaseItem : MonoBehaviour {

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
