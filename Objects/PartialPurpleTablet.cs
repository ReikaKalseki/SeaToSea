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

	public class PartialPurpleTablet : Spawnable {

		private readonly bool includePartA;
		private readonly bool includePartB;

		internal PartialPurpleTablet(bool includeA, bool includeB) : base("PartialPurpleTablet_" + (includeA ? "A" : "") + (includeB ? "B" : ""), "", "") {
			includePartA = includeA;
			includePartB = includeB;
		}

		public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject("83b61f89-1456-4ff5-815a-ecdc9b6cc9e4");
			GameObject mdl = go.getChildObject("precursor_key_cracked_01");
			if (!includePartA)
				mdl.removeChildObject("PrecursorKeyCracked_01");
			if (!includePartB)
				mdl.removeChildObject("PrecursorKeyCracked_02");
			return go;
		}

		protected override void ProcessPrefab(GameObject go) {
			base.ProcessPrefab(go);
			go.EnsureComponent<TechTag>().type = TechType.PrecursorKey_PurpleFragment;
		}

	}
}
