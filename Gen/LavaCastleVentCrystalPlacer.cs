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
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	internal class LavaCastleVentCrystalPlacer : Spawnable {

		internal LavaCastleVentCrystalPlacer() : base("LavaCastleVentCrystalPlacer", "", "") {

		}

		public override GameObject GetGameObject() {
			GameObject go = new GameObject("LavaCastleVentCrystalPlacer");
			go.EnsureComponent<LavaCastleVentCrystalConverter>();
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
			return go;
		}

	}

	internal class LavaCastleVentCrystalConverter : MonoBehaviour {

		void Update() {
			if ((transform.position - Player.main.transform.position).sqrMagnitude <= 90000) {
				float ch = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.15F : 0.25F;
				GameObject azur = null;
				if (UnityEngine.Random.Range(0F, 1F) < ch) {
					azur = ObjectUtil.createWorldObject(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).ClassID);
					azur.transform.rotation = transform.rotation;
					azur.transform.position = transform.position;
					azur.SetActive(true);
				}
				SNUtil.log("Converted lava castle vent placeholder @ " + transform.position + ": " + (azur ? azur.name : "NULL"));
				gameObject.destroy(false);
			}
		}

	}
}
