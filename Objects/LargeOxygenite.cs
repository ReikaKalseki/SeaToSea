using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	public class LargeOxygenite : Spawnable {

		public readonly XMLLocale.LocaleEntry locale;

		public LargeOxygenite(XMLLocale.LocaleEntry e) : base("Large_"+e.key, e.name, e.desc) {
			locale = e;
		}

		public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject(VanillaResources.LARGE_QUARTZ.prefab);
			Oxygenite.setupOxygeniteRender(go, 2.5F);
			Drillable dr = go.GetComponent<Drillable>();
			dr.Start();
			dr.minResourcesToSpawn = 1;
			dr.maxResourcesToSpawn = 1;
			dr.primaryTooltip = locale.name;
			dr.kChanceToSpawnResources = 1;
			TechType ox = CustomMaterials.getItem(CustomMaterials.Materials.OXYGENITE).TechType;
			dr.resources = new Drillable.ResourceType[1] { new Drillable.ResourceType { techType = ox, chance=1 } };
			ResourceTracker rt = go.EnsureComponent<ResourceTracker>();
			rt.techType = ox;
			rt.overrideTechType = ox;
			return go;
		}

		public void postRegister() {
			/*
			PDAManager.PDAPage page = PDAManager.createPage("ency_"+ClassID, FriendlyName, locale.pda, locale.getString("category"));
			page.setHeaderImage(TextureManager.getTexture(SeaToSeaMod.modDLL, locale.getString("header")));
			page.register();
			PDAScanner.EntryData e = new PDAScanner.EntryData();
			e.key = TechType;
			e.destroyAfterScan = false;
			e.locked = true;
			e.scanTime = 4;
			e.encyclopedia = page.id;
			PDAHandler.AddCustomScannerEntry(e);
			*/
		}

	}
}
