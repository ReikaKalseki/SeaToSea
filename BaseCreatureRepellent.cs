using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class BaseCreatureRepellent : CustomMachine<BaseCreatureRepellentLogic> {
		
		internal static readonly float POWER_COST = 0.25F; //per second
		internal static readonly float POWER_COST_ACTIVE = 2.0F; //per second
		internal static readonly float RANGE = 50F; //m
		internal static readonly float RANGE_INNER = 20F; //m
		
		static BaseCreatureRepellent() {
			
		}
		
		public BaseCreatureRepellent() : base("basecreaturerepel", "Seabase Creature Repellant Antenna", "Discourages close approach of creatures to the seabase.", "8949b0da-5173-431f-a989-e621af02f942") {
			addIngredient(TechType.Polyaniline, 1);
			addIngredient(TechType.AdvancedWiringKit, 1);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 8);
			addIngredient(TechType.PowerCell, 2);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		public override bool isOutdoors() {
			return true;
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<PowerRelay>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
						
			BaseCreatureRepellentLogic lgc = go.GetComponent<BaseCreatureRepellentLogic>();
			
			Renderer r = go.GetComponentInChildren<Renderer>();/*
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/
			
			//go.GetComponent<Constructable>().model = go;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}
		
	}
		
	public class BaseCreatureRepellentLogic : CustomMachineLogic {
		
		private float cooldown;
		
		void Start() {
			SNUtil.log("Reinitializing base sonar");
			SeaToSeaMod.repellentBlock.initializeMachine(gameObject);
		}
		
		protected override void updateEntity(float seconds) {
			//if (mainRenderer == null)
			//	mainRenderer = ObjectUtil.getChildObject(gameObject, "model").GetComponent<Renderer>();
			
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (Vector3.Distance(Player.main.transform.position, transform.position) >= BaseCreatureRepellent.RANGE*4)
				return;
			if (cooldown > 0) {
				cooldown -= seconds;
				return;
			}
			if (seconds > 0 && consumePower(BaseCreatureRepellent.POWER_COST, seconds)) {
				float r0 = BaseCreatureRepellent.RANGE*2;
				float r = BaseCreatureRepellent.RANGE;
				bool flag = false;
				RaycastHit[] hit = Physics.SphereCastAll(gameObject.transform.position, r, new Vector3(1, 1, 1), r);
				foreach (RaycastHit rh in hit) {
					if (rh.transform != null && rh.transform.gameObject) {
						Creature c = rh.transform.gameObject.GetComponent<Creature>();
						if (c && c.friend != Player.main.gameObject && (c.Aggression.Value > 0 || c.GetComponent<AggressiveWhenSeeTarget>())) {
							if (c is GhostLeviatanVoid)
								continue;
							float dd = Vector3.Distance(c.transform.position, transform.position);
							float f = dd <= BaseCreatureRepellent.RANGE_INNER ? 0.3F : 0.15F;
							if (c is GhostLeviathan || c is ReaperLeviathan || c is SeaDragon) {
								r = r0;
								f *= 0.5F;
							}
							if (dd >= r)
								continue;
							//SNUtil.writeToChat(c+" @ "+c.transform.position+" D="+dd+" > "+c.Scared.Value);
							c.flinch = 1;
							c.Scared.Add(f*seconds);
							c.Aggression.Add(-f*seconds*0.2F);
							flag = true;
							if (c.Scared.Value > 0.5F && dd < r*0.5F) {
								Vector3 vec = transform.position+((c.transform.position-transform.position)*3);
								c.GetComponent<SwimBehaviour>().SwimTo(vec, 20*f);
							}
						}
					}
				}
				if (flag) {
					if (!consumePower(BaseCreatureRepellent.POWER_COST_ACTIVE-BaseCreatureRepellent.POWER_COST, seconds))
						cooldown = 5;
				}
			}
		}	
	}
}
