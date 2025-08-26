using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class POITeleportSystem {

		public static readonly POITeleportSystem instance = new POITeleportSystem();

		private readonly Dictionary<string, POI> data = new Dictionary<string, POI>();

		private POITeleportSystem() {

		}

		internal void populate() { //lazyload because some of these come later
			this.addPOI("origin", Vector3.zero);
			this.addPOI("aurora", new Vector3(1010, 38, 119)).setActions(this.prepareAurora);
			this.addPOI("prawnbay", new Vector3(986, 6, -1.6F)).setActions(this.prepareAurora);
			this.addPOI("cove", new Vector3(-855, -881, 403));
			this.addPOI("lavacastle", new Vector3(-32, -1204, 142));
			this.addPOI("degasi1", WorldUtil.DEGASI_JELLY_BASE);
			this.addPOI("degasi2", WorldUtil.DEGASI_DGR_BASE);
			this.addPOI("treaderpod", SeaToSeaMod.treaderSignal.initialPosition + (Vector3.up * 10));
			this.addPOI("crashmesa", C2CHooks.crashMesa);
			this.addPOI("voidpod", VoidSpikesBiome.signalLocation);
			this.addPOI("pod6base", new Vector3(338.5F, -110, 286.5F));
			this.addPOI("keen", new Vector3(-822, -290, -873));
			this.addPOI("bkelpbase", C2CHooks.bkelpBaseGeoCenter + (Vector3.up * 30));
			this.addPOI("trailerbase", C2CHooks.trailerBaseBioreactor + (Vector3.up * 20));
			this.addPOI("dunearch", new Vector3(-1610, -334, 92));
			this.addPOI("mountainpod", new Vector3(993, -260, 1379));
			this.addPOI("mountainbase", C2CHooks.mountainBaseGeoCenter);
			this.addPOI("sunbeamsite", WorldUtil.SUNBEAM_SITE);
			this.addPOI("islandwreck", WorldUtil.DEGASI_FLOATING_BASE);
			this.addPOI("cragwreck", new Vector3(330, -266, -1451));
			this.addPOI("mtnislandcave", new Vector3(372, -90, 1039));
			this.addPOI("treadertunnel", new Vector3(-1250, -277, -725));
			this.addPOI("redkey", new Vector3(156.5F, -200, 951));
			this.addPOI("lrcache", new Vector3(-1120, -682, -694));
			this.addPOI("drf", new Vector3(-248, -800, 281));
			this.addPOI("khasar", new Vector3(-925, -178, 500));
			this.addPOI("mushtree", new Vector3(-870, -93, 591));
			this.addPOI("mushkoosh", new Vector3(712.84F, -222.55F, 532.76F));
			this.addPOI("stepcave", new Vector3(64, -103, -611));
			this.addPOI("kooshcaves", new Vector3(1223, -258, 527.5F));
			this.addPOI("prison", Creature.prisonAquriumBounds.center);
			this.addPOI("meteor", new Vector3(-1125, -360, 1130));
			this.addPOI("lavadome", new Vector3(-273, -1355, -152));
			this.addPOI("fcswreck", new Vector3(99, -410, 1445));
			this.addPOI("bkelpnest", new Vector3(-846, -522, 1294));
			this.addPOI("dunesgeode", new Vector3(-1419, -585, 376));
			this.addPOI("geysercave", C2CProgression.instance.dronePDACaveEntrance + new Vector3(5, 0, 5));
			this.addPOI("glassforest", UnderwaterIslandsFloorBiome.wreckCtrPos1.setY(-480));
			this.addPOI("voidwreck", new Vector3(-66, -445, -1863));
			this.addPOI("voidspikes", VoidSpikesBiome.end500m);
			this.addPOI("postcove", new Vector3(-1114, -1000, 525));
			this.addPOI("pod12", C2CProgression.instance.pod12Location + (Vector3.up * 20));
			this.addPOI("sanctuary", CrashZoneSanctuaryBiome.biomeCenter + (Vector3.up * 30));
			this.addPOI("deepvoid", ((VoidSpikesBiome.signalLocation + VoidSpikesBiome.voidEndpoint500m) / 2F).setY(-950)).setActions(() => {
				SubConsoleCommand.main.SpawnSub("cyclops", Player.main.transform.position + new Vector3(10, 0, 0), Quaternion.identity);
				InventoryUtil.addItem(TechType.CyclopsHullModule3);
				InventoryUtil.addItem(TechType.CyclopsShieldModule);
			});
		}

		public Vector3 getPosition(string key) {
			return !data.ContainsKey(key) ? throw new Exception("No such POI '" + key + "'") : data[key].position;
		}

		private void prepareAurora() {
			CrashedShipExploder.main.SwapModels(true);
			InventoryUtil.addItem(TechType.RadiationSuit);
			InventoryUtil.addItem(TechType.RadiationHelmet);
			InventoryUtil.addItem(TechType.RadiationGloves);
		}

		private POI addPOI(string name, Vector3 pos) {
			POI p = new POI(name, pos);
			data[name] = p;
			return p;
		}

		internal void jumpToPOI(string name) {
			POI p = data.ContainsKey(name) ? data[name] : null;
			if (p != null) {
				SNUtil.teleportPlayer(Player.main, p.position);
				if (p.additionalActions != null)
					p.additionalActions.Invoke();
				SNUtil.writeToChat("Jumped to POI '" + p.name + "' @ " + p.position);
			}
			else {
				SNUtil.writeToChat("No POI exists for name '" + name + "'.");
			}
		}

		class POI {

			public readonly string name;
			public readonly Vector3 position;
			internal Action additionalActions = null;

			internal POI(string n, Vector3 pos) {
				name = n;
				position = pos;
			}

			internal POI setActions(Action a) {
				additionalActions = a;
				return this;
			}

		}
	}

}
