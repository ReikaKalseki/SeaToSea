using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Mono;

using ReikaKalseki.Auroresource;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class DrillDepletionSystem {

		public static readonly DrillDepletionSystem instance = new DrillDepletionSystem();

		private static readonly float DRILL_LIFE = 3600; //seconds
		internal static readonly float RADIUS = 200;//300;

		internal DrillDepletionAOE aoeEntity;

		private DrillDepletionSystem() {

		}

		internal void register() {
			aoeEntity = new DrillDepletionAOE();
			aoeEntity.Patch();
			SaveSystem.addSaveHandler(aoeEntity.ClassID, new SaveSystem.ComponentFieldSaveHandler<DrillDepletionAOETag>().addField("totalDrillTime"));
		}

		private DrillDepletionAOETag getAoEForDrill(MonoBehaviour drill) {
			HashSet<DrillDepletionAOETag> set = WorldUtil.getObjectsNearWithComponent<DrillDepletionAOETag>(drill.transform.position, 5);
			//SNUtil.writeToChat("Drill "+drill+" @ "+drill.transform.position+" fetching tag = "+tag.toDebugString());
			float initialValue = 0;
			if (set.Count > 1) {
				foreach (DrillDepletionAOETag tag in set) {
					initialValue += tag.totalDrillTime;
					tag.gameObject.destroy(false);
				}
				set.Clear();
			}
			if (set.Count == 0) {
				GameObject go = ObjectUtil.createWorldObject(aoeEntity.ClassID);
				go.transform.position = drill.transform.position;
				DrillDepletionAOETag tag = go.GetComponent<DrillDepletionAOETag>();
				tag.totalDrillTime = initialValue;
				set.Add(tag);
			}
			return set.First();
		}

		internal bool hasRemainingLife(MonoBehaviour drill) {
			DrillableResourceArea aoe = this.getMotherlode(drill);
			if (aoe != null) {
				drill.gameObject.EnsureComponent<MotherlodeDrillTag>().deposit = aoe;
				return true;
			}
			DrillDepletionAOETag tag = this.getAoEForDrill(drill);
			return tag && tag.totalDrillTime <= DRILL_LIFE;
		}

		internal void deplete(MonoBehaviour drill) {
			DrillableResourceArea aoe = this.getMotherlode(drill);
			//SNUtil.writeToChat("motherlode = "+aoe);
			if (aoe != null) {
				drill.gameObject.EnsureComponent<MotherlodeDrillTag>().deposit = aoe;
				return;
			}
			DrillDepletionAOETag tag = this.getAoEForDrill(drill);
			if (tag)
				tag.totalDrillTime += DayNightCycle.main.deltaTime; //this is the time step they use too
		}

		internal DrillableResourceArea getMotherlode(MonoBehaviour drill) {
			foreach (DrillableResourceArea.DrillableResourceAreaTag d in WorldUtil.getObjectsNearWithComponent<DrillableResourceArea.DrillableResourceAreaTag>(drill.transform.position, DrillableResourceArea.getMaxRadius() + 10)) {
				SphereCollider aoe = d.GetComponentInChildren<SphereCollider>();
				Vector3 ctr = aoe.transform.position+aoe.center;
				if (ctr.y < drill.transform.position.y && MathUtil.isPointInCylinder(ctr, drill.transform.position, aoe.radius - 10, (aoe.radius * 1.5F) + 10)) {
					return DrillableResourceArea.getResourceNode(d.GetComponent<PrefabIdentifier>().ClassId);
				}
				else {
					//SNUtil.writeToChat("motherlode too far away @ "+ctr+" for "+drill.transform.position+" R="+aoe.radius);
				}
			}
			return null;
		}
	}

	class MotherlodeDrillTag : MonoBehaviour {

		private static readonly int MOTHERLODE_ORES_PER_DAY = 60;
		private static readonly int MOTHERLODE_STORAGE_CAPACITY = 1200;

		private static PropertyInfo allowedOreField;
		private static FieldInfo oresPerDayField;
		private static MethodInfo oresPerDaySet;

		private static Type drillerDisplay;
		private static MethodInfo updateDisplay;
		//private static MethodInfo refreshDisplayStorage;
		private static FieldInfo filterGridField;
		private static FieldInfo filterListField;
		private static FieldInfo storageText;

		private static PropertyInfo controllerStorage;

		private static FieldInfo storageCapacity;

		private static MethodInfo showGridPage;
		//private static FieldInfo currentGridPage;
		private static FieldInfo gridGO;

		private static FieldInfo buttonItem;
		private static FieldInfo buttonIcon;

		internal DrillableResourceArea deposit;

		private Component drillerDisplayComponent;

		private float lastOreTableAssignTime = -1;

		void Start() {
			SNUtil.writeToChat("Drill at " + WorldUtil.getRegionalDescription(transform.position, true) + " is mining deposit: " + Language.main.Get(deposit.TechType.AsString()));
		}

		void Update() {
			if (allowedOreField == null) {
				Type t = FCSIntegrationSystem.instance.getFCSDrillOreManager();
				allowedOreField = t.GetProperty("AllowedOres", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				oresPerDayField = t.GetField("_oresPerDay", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				oresPerDaySet = t.GetMethod("SetOresPerDay", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				drillerDisplay = t.Assembly.GetType("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerDisplay");
				updateDisplay = drillerDisplay.GetMethod("UpdateDisplayValues", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				//refreshDisplayStorage = drillerDisplay.GetMethod("RefreshStorageAmount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				filterGridField = drillerDisplay.GetField("_filterGrid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				filterListField = drillerDisplay.GetField("_trackedFilterState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				storageText = drillerDisplay.GetField("_itemCounter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				controllerStorage = FCSIntegrationSystem.instance.getFCSDrillController().GetProperty("DeepDrillerContainer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				storageCapacity = FCSIntegrationSystem.instance.getFCSDrillStorage().GetField("_storageSize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				//currentGridPage = gridHelper.GetField("_currentPage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				showGridPage = typeof(GridHelper).GetMethod("DrawPage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.HasThis, new Type[0], null);
				gridGO = typeof(GridHelper).GetField("_itemsGrid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				buttonItem = typeof(uGUI_FCSDisplayItem).GetField("_techType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				buttonIcon = typeof(uGUI_FCSDisplayItem).GetField("_icon", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				GridHelper grid = (GridHelper)filterGridField.GetValue(drillerDisplayComponent);
				grid.OnLoadDisplay += (data) => this.rebuildDisplay();
			}

			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastOreTableAssignTime >= 1) {
				lastOreTableAssignTime = time;
				Component com = this.GetComponent(FCSIntegrationSystem.instance.getFCSDrillOreManager());
				if (com) {
					allowedOreField.SetValue(com, deposit.getAllAvailableResources());

					//set ores per day count too; default is 25 but increase to 60 on a motherlode
					int get = (int)oresPerDayField.GetValue(com);
					if (get != MOTHERLODE_ORES_PER_DAY)
						oresPerDaySet.Invoke(com, new object[] { MOTHERLODE_ORES_PER_DAY });

					this.rebuildDisplay();
				}

				Component com2 = this.GetComponent(FCSIntegrationSystem.instance.getFCSDrillController());
				if (com2) {
					object storage = controllerStorage.GetValue(com2);
					storageCapacity.SetValue(storage, MOTHERLODE_STORAGE_CAPACITY); //defaults to 300
				}
			}

			GridHelper grid2 = (GridHelper)filterGridField.GetValue(drillerDisplayComponent);
			if (grid2 != null) {
				GameObject go = (GameObject)gridGO.GetValue(grid2);
				foreach (uGUI_FCSDisplayItem c in go.GetComponentsInChildren<uGUI_FCSDisplayItem>()) {
					TechType tt = (TechType)buttonItem.GetValue(c);
					uGUI_Icon ico = (uGUI_Icon)buttonIcon.GetValue(c);
					ico.sprite = SpriteManager.Get(C2CHooks.isFCSDrillMaterialAllowed(tt, true) ? tt : TechType.None);
				}

				Text t = (Text)storageText.GetValue(drillerDisplayComponent);
				t.text = t.text.Substring(0, t.text.LastIndexOf('/') + 1) + MOTHERLODE_STORAGE_CAPACITY;
				updateDisplay.Invoke(drillerDisplayComponent, new object[0]);
			}
		}

		private void rebuildDisplay() {
			drillerDisplayComponent = this.GetComponent(drillerDisplay);
			if (drillerDisplayComponent) {
				IDictionary dict = (IDictionary)filterListField.GetValue(drillerDisplayComponent);
				dict.Clear();

				GridHelper grid = (GridHelper)filterGridField.GetValue(drillerDisplayComponent);
				if (grid != null) {
					GameObject go = (GameObject)gridGO.GetValue(grid);
					go.removeChildObject("OreBTN");
					showGridPage.Invoke(grid, new object[0]);
				}
			}
		}

	}

	internal class DrillDepletionAOE : Spawnable {

		internal DrillDepletionAOE() : base("DrillDepletionAOE", "", "") {

		}

		public override GameObject GetGameObject() {
			GameObject go = new GameObject("DrillDepletionAOE(Clone)");
			go.EnsureComponent<PrefabIdentifier>().classId = ClassID;
			go.EnsureComponent<TechTag>().type = TechType;
			go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
			go.EnsureComponent<DrillDepletionAOETag>();
			SphereCollider sc = go.EnsureComponent<SphereCollider>();
			sc.isTrigger = true;
			sc.radius = DrillDepletionSystem.RADIUS;
			sc.center = Vector3.zero;
			go.layer = LayerID.NotUseable;
			return go;
		}

	}

	class DrillDepletionAOETag : MonoBehaviour {

		internal float totalDrillTime = 0;

	}

}
