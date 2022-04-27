using System;

using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class AvoliteSpawner {
		
		public static readonly AvoliteSpawner instance = new AvoliteSpawner();
		
		private readonly WeightedRandom<TechType> itemChoicesLoose = new WeightedRandom<TechType>();
		private readonly WeightedRandom<TechType> itemChoicesBox = new WeightedRandom<TechType>();
		
		private readonly TechType avolite = CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType;
		
		private readonly Vector3 eventCenter = new Vector3(?);
		private readonly Vector3 eventUITargetLocation = new Vector3(?);
		private readonly Vector3 mountainCenter = new Vector3(?);
		
		private readonly Dictionary<Vector3, TechType> boxes = new Dictionary<Vector3, TechType>();
		private readonly Dictionary<Vector3, string> looseItems = new Dictionary<Vector3, string>();
		
		private int boxCount = UnityEngine.Random.Range(8, 14); //8-13
		private int looseCount = UnityEngine.Random.Range(18, 31); //18-30
		private int scrapCount = UnityEngine.Random.Range(25, 41); //25-40
		
		private int gennedAvolite;
		
		private AvoliteSpawner() {
			addItem(TechType.NutrientBlock, 250, true, false);
			addItem(TechType.DisinfectedWater, 200, true, false);
			addItem(TechType.Battery, 40, true, false);
			addItem(TechType.Beacon, 40, true, false);
			
			addItem(TechType.PowerCell, 15, true, true);
			addItem(TechType.FireExtinguisher, 25, true, true);
			addItem(avolite, 20, true, true);
			
			addItem(TechType.EnameledGlass, 100, false, true);
			addItem(TechType.Titanium, 150, false, true);
			addItem(TechType.ComputerChip, 50, false, true);
			addItem(TechType.WiringKit, 75, false, true);
			addItem(TechType.CopperWire, 150, false, true);
		}
		
		private void addItem(TechType item, double wt, bool box, bool loose) {
			if (box)
				itemChoicesBox.addEntry(item, wt);
			if (loose)
				itemChoicesLoose.addEntry(item, wt);
		}
		
		public void doSpawn() {
			for (int i = 0; i < boxCount; i++) {
				GameObject box = spawnBox();
				if (box != null) {
					SBUtil.setCrateItem(box.EnsureComponent<SupplyCrate>(), generateRandomItem());
					ensureGravity(box);
				}
			}
			for (int i = 0; i < looseCount; i++) {
				Vector3 pos = getRandomPosition();
				GameObject go = SBUtil.dropItem(pos, generateRandomItem());
				ensureGravity(go);
			}
			while (gennedAvolite < 3) {
				Vector3 pos = getRandomPosition();
				GameObject go = SBUtil.dropItem(pos, avolite);
				ensureGravity(go);
				gennedAvolite++;
			}
			for (int i = 0; i < scrapCount; i++) {
				Vector3 pos = getRandomPosition();
				VanillaResources mtl = null;
				switch(UnityEngine.Random.Range(0, 4)) {
					case 0:
						mtl = VanillaResources.SCRAP1;
						break;
					case 1:
						mtl = VanillaResources.SCRAP2;
						break;
					case 2:
						mtl = VanillaResources.SCRAP3;
						break;
					case 3:
						mtl = VanillaResources.SCRAP4;
						break;
				}
				GameObject go = SBUtil.createWorldObject(mtl.prefab);
				ensureGravity(go);
				go.transform.position = pos;
				go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
			}
		}
		
		private GameObject spawnBox() {
			Vector3 pos = getRandomPosition();
			GameObject box = SBUtil.createWorldObject("8c21d402-1767-4266-ada6-b3e40c798e9f"); //powercell
			if (box != null)
				box.transform.position = pos;
			return box;
		}
		
		private TechType generateRandomItem(bool box) {
			TechType ret = (box ? itemChoicesBox : itemChoicesLoose).getRandomEntry();
			if (ret == avolite)
				gennedAvolite++;
			return ret;
		}
		
		private Vector3 getRandomPosition() {
			return eventCenter; //TODO
		}
		
		private void ensureGravity(GameObject go) {
			
		}
	}
	
}
