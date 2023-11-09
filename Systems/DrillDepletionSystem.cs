using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class DrillDepletionSystem {
		
		public static readonly DrillDepletionSystem instance = new DrillDepletionSystem();
		
		private static readonly float DRILL_LIFE = 3600; //seconds
    
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
	    	DrillDepletionAOETag tag = getAoEForDrill(drill);
	    	return tag && tag.totalDrillTime <= DRILL_LIFE;
	    }
	    
	    internal void deplete(MonoBehaviour drill) {
	    	DrillDepletionAOETag tag = getAoEForDrill(drill);
	    	if (tag)
	    		tag.totalDrillTime += DayNightCycle.main.deltaTime; //this is the time step they use too
	    }
	    
	    internal float getRemainingDrillTime(MonoBehaviour drill) {
	    	DrillDepletionAOETag tag = getAoEForDrill(drill);
	    	return tag ? DRILL_LIFE-tag.totalDrillTime : 0;
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
			sc.radius = 300;
			sc.center = Vector3.zero;
			go.layer = LayerID.NotUseable;
			return go;
		}
		
	}
	
	class DrillDepletionAOETag : MonoBehaviour {
		
		internal float totalDrillTime = 0;
		
	}
	
}
