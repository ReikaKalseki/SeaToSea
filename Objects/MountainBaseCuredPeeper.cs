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

	public class MountainBaseCuredPeeper : PickedUpAsOtherItem {

		internal MountainBaseCuredPeeper() : base("MountainBaseCuredPeeper", TechType.CuredPeeper) {

		}

		protected override void prepareGameObject(GameObject go) {
			go.GetComponent<Rigidbody>().isKinematic = true;
			go.EnsureComponent<MountainBaseCuredPeeperTag>();
			go.removeComponent<EcoTarget>();
		}

	}

	class MountainBaseCuredPeeperTag : MonoBehaviour {

		private Rigidbody body;

		void Update() {
			if (!body) {
				body = this.GetComponent<Rigidbody>();
			}
			body.isKinematic = true;
		}

	}
}
