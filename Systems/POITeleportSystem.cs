using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class POITeleportSystem {
		
		public static readonly POITeleportSystem instance = new POITeleportSystem();
		
		private readonly Dictionary<string, POI> data = new Dictionary<string, POI>();
		
		private POITeleportSystem() {

		}
		
		internal void populate() { //lazyload because some of these come later
			addPOI("origin", Vector3.zero);
			addPOI("aurora", new Vector3(1010, 38, 119)).setActions(prepareAurora);
			addPOI("prawnbay", new Vector3(986, 4, -1.6F)).setActions(prepareAurora);
			addPOI("cove", new Vector3(-855, -881, 403));
			addPOI("lavacastle", new Vector3(-32, -1204, 142));
			addPOI("degasi1", new Vector3(85, -260, -356));
			addPOI("degasi2", new Vector3(-643, -505, -944.5F));
			addPOI("treaderpod", SeaToSeaMod.treaderSignal.initialPosition+Vector3.up*10);
			addPOI("crashmesa", C2CHooks.crashMesa);
			addPOI("voidpod", VoidSpikesBiome.signalLocation);
			addPOI("pod6base", new Vector3(338.5F, -110, 286.5F));
			addPOI("bkelpbase", C2CHooks.bkelpBaseGeoCenter+Vector3.up*30);
			addPOI("trailerbase", C2CHooks.trailerBaseBioreactor+Vector3.up*20);
			addPOI("dunearch", new Vector3(-1610, -334, 92));
			addPOI("mountainpod", new Vector3(993, -260, 1379));
			addPOI("mountainbase", C2CHooks.mountainBaseGeoCenter);
			addPOI("sunbeamsite", new Vector3(301, 15, 1086));
			addPOI("islandwreck", new Vector3(-763, 20, -1104));
			addPOI("cragwreck", new Vector3(330, -266, -1451));
			addPOI("mtnislandcave", new Vector3(372, -90, 1039));
			addPOI("treadertunnel", new Vector3(-1250, -277, -725));
			addPOI("redkey", new Vector3(156.5F, -200, 951));
			addPOI("lrcache", new Vector3(-1120, -682, -694));
			addPOI("drf", new Vector3(-248, -800, 281));
			addPOI("khasar", new Vector3(-925, -178, 500));
			addPOI("mushtree", new Vector3(-870, -93, 591));
			addPOI("mushkoosh", new Vector3(712.84F, -222.55F, 532.76F));
			addPOI("stepcave", new Vector3(64, -103, -611));
			addPOI("kooshcaves", new Vector3(1223, -258, 527.5F));
			addPOI("prison", Creature.prisonAquriumBounds.center);
			addPOI("meteor", new Vector3(-1125, -360, 1130));
			addPOI("lavadome", new Vector3(-273, -1355, -152));
			addPOI("fcswreck", new Vector3(99, -410, 1445));
			addPOI("geysercave", C2CProgression.instance.dronePDACaveEntrance+new Vector3(5, 0, 5));
			addPOI("glassforest", UnderwaterIslandsFloorBiome.wreckCtrPos1.setY(-480));
			addPOI("voidspikes", VoidSpikesBiome.end500m);
			addPOI("sanctuary", CrashZoneSanctuaryBiome.biomeCenter+Vector3.up*30);
			addPOI("deepvoid", ((VoidSpikesBiome.signalLocation+VoidSpikesBiome.voidEndpoint500m)/2F).setY(-950)).setActions(() => {
    			SubConsoleCommand.main.SpawnSub("cyclops", Player.main.transform.position+new Vector3(10, 0, 0), Quaternion.identity);
    			InventoryUtil.addItem(TechType.CyclopsHullModule3);
    			InventoryUtil.addItem(TechType.CyclopsShieldModule);
			});
		}
		
		public Vector3 getPosition(string key) {
			return data[key].position;
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
				SNUtil.writeToChat("Jumped to POI '"+p.name+"' @ "+p.position);
			}
			else {
				SNUtil.writeToChat("No POI exists for name '"+name+"'.");
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
