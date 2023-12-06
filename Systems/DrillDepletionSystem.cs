using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Auroresource;
using ReikaKalseki.SeaToSea;

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
	    			UnityEngine.Object.Destroy(tag.gameObject);
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
	    	DrillableResourceArea aoe = getMotherlode(drill);
	    	if (aoe != null) {
	    		drill.gameObject.EnsureComponent<MotherlodeDrillTag>().deposit = aoe;
	    		return true;
	    	}
	    	DrillDepletionAOETag tag = getAoEForDrill(drill);
	    	return tag && tag.totalDrillTime <= DRILL_LIFE;
	    }
	    
	    internal void deplete(MonoBehaviour drill) {
	    	DrillableResourceArea aoe = getMotherlode(drill);
	    	//SNUtil.writeToChat("motherlode = "+aoe);
	    	if (aoe != null) {
	    		drill.gameObject.EnsureComponent<MotherlodeDrillTag>().deposit = aoe;
	    		return;
	    	}
	    	DrillDepletionAOETag tag = getAoEForDrill(drill);
	    	if (tag)
	    		tag.totalDrillTime += DayNightCycle.main.deltaTime; //this is the time step they use too
	    }
	    
	    internal DrillableResourceArea getMotherlode(MonoBehaviour drill) {
	    	foreach (DrillableResourceArea.DrillableResourceAreaTag d in WorldUtil.getObjectsNearWithComponent<DrillableResourceArea.DrillableResourceAreaTag>(drill.transform.position, DrillableResourceArea.getMaxRadius()+10)) {
	    		SphereCollider aoe = d.GetComponentInChildren<SphereCollider>();
	    		Vector3 ctr = aoe.transform.position+aoe.center;
	    		if (ctr.y < drill.transform.position.y && MathUtil.isPointInCylinder(ctr, drill.transform.position, aoe.radius-10, aoe.radius*1.5F+10)) {
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
		
		private static PropertyInfo allowedOreField;
		private static FieldInfo oresPerDayField;
		private static MethodInfo oresPerDaySet;
		
		private static Type drillerDisplay;
		private static MethodInfo updateDisplay;
		private static FieldInfo filterGridField;
		private static FieldInfo filterListField;
		
		private static Type gridHelper;
		private static MethodInfo showGridPage;
		//private static FieldInfo currentGridPage;
		private static FieldInfo gridGO;
		
		private static Type iconButton;
		private static FieldInfo buttonItem;
		private static FieldInfo buttonIcon;
		
		internal DrillableResourceArea deposit;
		
		private Component drillerDisplayComponent;
		
	    private float lastOreTableAssignTime = -1;
		
		void Start() {
			SNUtil.writeToChat("Drill at "+WorldUtil.getRegionalDescription(transform.position)+" is mining deposit: "+Language.main.Get(deposit.TechType.AsString()));
		}
		
		void Update() {
	    	if (allowedOreField == null) {
	    		Type t = FCSIntegrationSystem.instance.getFCSDrillOreManager();
	    		allowedOreField = t.GetProperty("AllowedOres", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    		oresPerDayField = t.GetField("_oresPerDay", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    		oresPerDaySet = t.GetMethod("SetOresPerDay", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    		
	    		drillerDisplay = t.Assembly.GetType("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerDisplay");
	    		updateDisplay = drillerDisplay.GetMethod("UpdateDisplayValues", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    		filterGridField = drillerDisplay.GetField("_filterGrid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    		filterListField = drillerDisplay.GetField("_trackedFilterState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    		
	    		gridHelper = InstructionHandlers.getTypeBySimpleName("FCS_AlterraHub.Mono.GridHelper");
	    		//currentGridPage = gridHelper.GetField("_currentPage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    		showGridPage = gridHelper.GetMethod("DrawPage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.HasThis, new Type[0], null);
	    		gridGO = gridHelper.GetField("_itemsGrid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    		
	    		iconButton = gridHelper.Assembly.GetType("FCS_AlterraHub.Model.GUI.uGUI_FCSDisplayItem");
	    		buttonItem = iconButton.GetField("_techType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    		buttonIcon = iconButton.GetField("_icon", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	    	}
	    	
	    	float time = DayNightCycle.main.timePassedAsFloat;
	    	if (time-lastOreTableAssignTime >= 1) {
	    		lastOreTableAssignTime = time;
		    	Component com = GetComponent(FCSIntegrationSystem.instance.getFCSDrillOreManager());
		    	if (com) {
			   		allowedOreField.SetValue(com, deposit.getAllAvailableResources());
			   		
			   		//set ores per day count too; default is 25 but increase to 60 on a motherlode
			   		int get = (int)oresPerDayField.GetValue(com);
			   		if (get != MOTHERLODE_ORES_PER_DAY)
			   			oresPerDaySet.Invoke(com, new object[]{MOTHERLODE_ORES_PER_DAY});
			   		
					drillerDisplayComponent = GetComponent(drillerDisplay);
					if (drillerDisplayComponent) {
						IDictionary dict = (IDictionary)filterListField.GetValue(drillerDisplayComponent);
						dict.Clear();
					  		
						MonoBehaviour grid = (MonoBehaviour)filterGridField.GetValue(drillerDisplayComponent);
						if (grid != null) {
					  		GameObject go = (GameObject)gridGO.GetValue(grid);
					  		ObjectUtil.removeChildObject(go, "OreBTN");
					  		showGridPage.Invoke(grid, new object[0]);
						}
						
						updateDisplay.Invoke(drillerDisplayComponent, new object[0]);
					}
		    	}
	    	}
						
			MonoBehaviour grid2 = (MonoBehaviour)filterGridField.GetValue(drillerDisplayComponent);
			if (grid2 != null) {
				GameObject go = (GameObject)gridGO.GetValue(grid2);
				foreach (Component c in go.GetComponentsInChildren(iconButton)) {
					TechType tt = (TechType)buttonItem.GetValue(c);
					uGUI_Icon ico = (uGUI_Icon)buttonIcon.GetValue(c);
	    			ico.sprite = SpriteManager.Get(C2CHooks.isFCSDrillMaterialAllowed(tt) ? tt : TechType.None);
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
