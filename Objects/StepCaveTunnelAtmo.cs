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

	public class StepCaveTunnelAtmo : Spawnable {

		internal StepCaveTunnelAtmo() : base("StepCaveTunnelAtmo", "", "") {

		}

		public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject("b179b366-4342-4545-aa4d-a86ad88b780e");
			world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
			return world;
		}

	}
}
