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
		
		internal DrillableResourceArea deposit;
		
	    private float lastOreTableAssignTime = -1;
		
		void Start() {
			SNUtil.writeToChat("Drill at "+WorldUtil.getRegionalDescription(transform.position)+" is mining deposit: "+Language.main.Get(deposit.TechType.AsString()));
		}
		
		void Update() {
	    	float time = DayNightCycle.main.timePassedAsFloat;
	    	if (time-lastOreTableAssignTime >= 1) {
	    		lastOreTableAssignTime = time;
		    	Component com = GetComponent(FCSIntegrationSystem.instance.getFCSDrillOreManager());
		    	if (com) {
			   		PropertyInfo p = FCSIntegrationSystem.instance.getFCSDrillOreManager().GetProperty("AllowedOres", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			   		p.SetValue(com, deposit.getAllAvailableResources());
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
