using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using FMOD;

using FMODUnity;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {
	public class PCFSecurityNode : Spawnable {

		public PCFSecurityNode(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {

		}

		public override GameObject GetGameObject() {
			GameObject go1 = ObjectUtil.createWorldObject("78009225-a9fa-4d21-9580-8719a3368373"); //block
			GameObject go2 = ObjectUtil.createWorldObject("473a8c4d-162f-4575-bbef-16c1c97d1e9d"); //light on top/projector base
			GameObject go = new GameObject("PCFSecurityNode(Clone)");
			go1.transform.SetParent(go.transform);
			Utils.ZeroTransform(go1.transform);
			go2.transform.SetParent(go.transform);
			Utils.ZeroTransform(go2.transform);
			return go;
		}

	}

	class PCFSecurityNodeTag : MonoBehaviour {

		void BashHit() { //prawn hit
			gameObject.destroy();
			C2CProgression.instance.stepPCFSecurity();
		}

	}
}
