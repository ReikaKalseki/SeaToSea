using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class AvoliteSpawner {
		
		public static readonly AvoliteSpawner instance = new AvoliteSpawner();
		
		public static readonly int AVOLITE_COUNT = 9;//6;
		
		private readonly Dictionary<TechType, int> itemChoices = new Dictionary<TechType, int>();
		
		private readonly TechType avolite = CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType;
		
		private readonly Vector3 eventCenter = new Vector3(215, 425.6F, 2623.6F);
		private readonly Vector3 eventUITargetLocation = new Vector3(297.2F, 3.5F, 1101);
		private readonly Vector3 mountainCenter = new Vector3(356.3F, 29F, 1039.4F);
		private readonly Vector3 biomeCenter = new Vector3(800, 0, 1300);//new Vector3(966, 0, 1336);
		
		private readonly Dictionary<Vector3, TechType> boxes = new Dictionary<Vector3, TechType>();
		private readonly Dictionary<Vector3, string> looseItems = new Dictionary<Vector3, string>();
		
		private readonly int scrapCount = UnityEngine.Random.Range(45, 71); //45-70
		
		private AvoliteSpawner() {
			addItem(TechType.NutrientBlock, 10);
			addItem(TechType.DisinfectedWater, 8);
			addItem(TechType.Battery, 4);
			//addItem(TechType.Beacon, 40, true, false);
			
			addItem(TechType.PowerCell, 1);
			addItem(TechType.FireExtinguisher, 2);
			//addItem(avolite, 20, true, true);
			
			addItem(TechType.EnameledGlass, 5);
			addItem(TechType.Titanium, 25);
			addItem(TechType.ComputerChip, 3);
			addItem(TechType.WiringKit, 6);
			addItem(TechType.CopperWire, 20);
		}
		
		private void addItem(TechType item, int amt) {
			itemChoices[item] = amt;
		}
		
		public void doSpawn() {	
			foreach (KeyValuePair<TechType, int> kvp in itemChoices) {
				for (int i = 0; i < kvp.Value; i++) {
					Vector3 pos = getRandomPosition();
					GameObject go = SBUtil.dropItem(pos, kvp.Key);
					applyPhysics(go);
				}
			}
			for (int i = 0; i < AVOLITE_COUNT; i++) {
				Vector3 pos = getRandomPosition();
				GameObject go = SBUtil.dropItem(pos, avolite);
				applyPhysics(go);
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
				applyPhysics(go);
				go.transform.position = pos;
				go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0F, 360F), Vector3.up);
			}
			
			PDAManager.getPage("sunbeamdebrishint").unlock();
		}
		
		private void applyPhysics(GameObject go) {
			//go.transform.position = MathUtil.getRandomVectorAround(eventUITargetLocation+Vector3.up*200, 9);//go.transform.position+dist*0.1F;
			//Vector3 target = MathUtil.getRandomVectorAround(biomeCenter, fuzz);//MathUtil.getRandomVectorAround(mountainCenter+new Vector3(150, 0, 150), new Vector3(150, 30, 150));
			//Vector3 dist = (target-go.transform.position)._X0Z();
			SBUtil.applyGravity(go);
			go.transform.rotation = UnityEngine.Random.rotationUniform;
			//go.GetComponent<Rigidbody>().velocity = dist*0.04F;
			go.GetComponent<WorldForces>().aboveWaterGravity *= 0.5F;
			go.GetComponent<WorldForces>().underwaterGravity *= 2.5F;
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
			//go.GetComponent<WorldForces>().underwaterDrag = 1.5F;
		}
		
		private GameObject spawnBox() {
			Vector3 pos = getRandomPosition();
			GameObject box = SBUtil.createWorldObject("8c21d402-1767-4266-ada6-b3e40c798e9f"); //powercell
			if (box != null)
				box.transform.position = pos;
			return box;
		}
		
		private Vector3 getRandomPosition() {
			/*
			Vector3 vec = eventCenter*0.5F+eventUITargetLocation*0.5F;
			vec.y = eventCenter.y;
			return MathUtil.getRandomVectorAround(vec, fuzz);
			*/
			Vector3 vec = MathUtil.getRandomVectorAround(biomeCenter, new Vector3(300, 0, 200));
			vec.y = MathUtil.getRandomPlusMinus(200, 50);
			return vec;
		}
		
		public class TriggerCallback : MonoBehaviour {
			
			void trigger() {
				instance.doSpawn();
			}
			
		}
	}
	
}
