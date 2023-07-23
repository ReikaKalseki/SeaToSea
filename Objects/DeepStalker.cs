using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.SeaToSea {
	
	public class DeepStalker : Spawnable {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
	    internal DeepStalker(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			locale = e;
	    }
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(VanillaCreatures.STALKER.prefab, true, true);
			world.EnsureComponent<TechTag>().type = TechType;
			world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			DeepStalkerTag kc = world.EnsureComponent<DeepStalkerTag>();
			foreach (Renderer r in world.GetComponentsInChildren<Renderer>()) {
				RenderUtil.setEmissivity(r, 1.25F);
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Creature/DeepStalker");
				r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
			}
			world.EnsureComponent<AggressiveToPilotingVehicle>().aggressionPerSecond = 0.2F;
			return world;
	    }
		
		public void register() {
			Patch();
			SNUtil.addPDAEntry(this, 8, "Lifeforms/Fauna/Carnivores", locale.pda, locale.getField<string>("header"), null);
			
			CustomEgg.createAndRegisterEgg(this, TechType.StalkerEgg, 1, locale.desc, true, e => {e.eggProperties.growingPeriod = 2400;}, 0.25F, BiomeType.GrandReef_TreaderPath);
	    
	   		//GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Creature, LargeWorldEntity.CellLevel.Medium, BiomeType.SeaTreaderPath_OpenDeep_CreatureOnly, 1, 0.15F);
	   		//GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.GrandReef_TreaderPath, 1, 0.3F);
	   		
	   		BehaviourData.behaviourTypeList[TechType] = BehaviourType.Shark;
	   		
	   		BioReactorHandler.SetBioReactorCharge(TechType, BaseBioReactor.charge[TechType.Stalker]);
		}
			
	}
	
	class DeepStalkerTag : MonoBehaviour {
				
		private static readonly SoundManager.SoundData biteSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "deepstalkerbite", "Sounds/deepstalker-bite.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 24);}, SoundSystem.masterBus);
		
		private static float lastPlayerBiteTime;
		
		private Renderer render;
		private Stalker creatureComponent;
		private AggressiveWhenSeeTarget playerHuntComponent;
		private MeleeAttack attackComponent;
		private CollectShiny collectorComponent;
		private SwimBehaviour swimmer;
		private WaterParkCreature acuComponent;
		
		private readonly Color peacefulColor = new Color(0.2F, 0.67F, 1F, 1);
		private readonly Color aggressiveColor = new Color(1, 0, 0, 1);
		private readonly float colorChangeSpeed = 1;
		
		private float aggressionForColor = 0;
		private float aggressionForACUColor = 0;
		
		private float platinumGrabTime = -1;
		private float lastAreaCheck = -1;
		private float lastDespawnCheck = -1;
		
		private float lastTextureSwapTime;
		
		private float targetCooldown = 0;
		
		private GameObject currentForcedTarget;
		
		private SeaTreader treaderTarget;
		
		void Start() {
			acuComponent = GetComponent<WaterParkCreature>();
		}
		
		private void Update() {
			if (!render) {
				render = GetComponentInChildren<Renderer>();
			}
			if (!creatureComponent) {
				creatureComponent = GetComponent<Stalker>();
				creatureComponent.liveMixin.data.maxHealth = 800; //stalker base is 300
			}
			if (!attackComponent) {
				attackComponent = GetComponent<MeleeAttack>();
				attackComponent.biteDamage *= 0.67F;
				attackComponent.biteAggressionDecrement *= 2;
				attackComponent.biteAggressionThreshold *= 0.8F;
				attackComponent.canBeFed = true;
				attackComponent.canBiteCyclops = false;
				attackComponent.canBiteCreature = true;
				attackComponent.canBitePlayer = true;
				attackComponent.canBiteVehicle = true;
				attackComponent.ignoreSameKind = false;
				//attackComponent.attackSound.asset = biteSound.asset;
				//attackComponent.attackSound.path = biteSound.asset.path;
			}
			if (!collectorComponent) {
				collectorComponent = GetComponent<CollectShiny>();
				//collectorComponent.priorityMultiplier.
			}
			if (!swimmer) {
				swimmer = GetComponent<SwimBehaviour>();
			}
			if (!playerHuntComponent) {
				foreach (AggressiveWhenSeeTarget agg in GetComponents<AggressiveWhenSeeTarget>()) {
					if (agg.targetType == EcoTargetType.Shark) {
						agg.aggressionPerSecond *= 0.15F;
						agg.ignoreSameKind = false;
						agg.maxRangeScalar *= 1.5F;
						playerHuntComponent = agg;
						break;
					}
				}
			}
			
			float dT = Time.deltaTime;
			
			if (render && creatureComponent) {
				float target = creatureComponent.Aggression.Value;
				if (acuComponent) {
					if (UnityEngine.Random.Range(0F, 1F) <= 0.008F) {
						aggressionForACUColor = UnityEngine.Random.Range(0F, 1F);
					}
					target = aggressionForACUColor;
				}
				if (aggressionForColor < target) {
					aggressionForColor = Mathf.Min(target, aggressionForColor+dT*colorChangeSpeed);
				}
				else if (aggressionForColor > target) {
					aggressionForColor = Mathf.Max(target, aggressionForColor-dT*colorChangeSpeed);
				}
				render.materials[0].SetColor("_GlowColor", Color.Lerp(peacefulColor, aggressiveColor, aggressionForColor));
			}
			
			float time = DayNightCycle.main.timePassedAsFloat;
			
			if (render && time > lastTextureSwapTime+1) {
				lastTextureSwapTime = time;
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, render, "Textures/Creature/DeepStalker", null);
			}
			
			if (time < targetCooldown) {
				playerHuntComponent.lastTarget.SetTarget(null);
				currentForcedTarget = null;
				collectorComponent.DropShinyTarget();
			}
			else {
				bool has = currentlyHasPlatinum();
				if (has) {
					playerHuntComponent.lastTarget.SetTarget(null);
					currentForcedTarget = null;
					creatureComponent.Aggression.Add(-0.15F);
					collectorComponent.shinyTarget.EnsureComponent<ResourceTrackerUpdater>().tracker = collectorComponent.shinyTarget.GetComponent<ResourceTracker>();
				}
				
				if (currentForcedTarget && currentForcedTarget == Player.main.gameObject && UnityEngine.Random.Range(0F, 1F) <= 0.12F) {
					if (has || time-lastPlayerBiteTime < 5 || Inventory.main.GetPickupCount(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType) == 0) {
						//SNUtil.writeToChat("Dropped player target");
						playerHuntComponent.lastTarget.SetTarget(null);
						currentForcedTarget = null;
					}
				}
				
				if (currentForcedTarget && time-platinumGrabTime <= 12) {
					triggerPtAggro(currentForcedTarget, false);
				}
				else if (!has && !currentForcedTarget && time-lastAreaCheck >= 1 && !GetComponent<WaterParkCreature>()) {
					lastAreaCheck = time;
					if (!treaderTarget || !treaderTarget.gameObject.activeInHierarchy || Vector3.Distance(treaderTarget.transform.position, transform.position) >= 120)
						bindToTreader(WorldUtil.getClosest<SeaTreader>(gameObject));
					List<GameObject> loosePlatinum = new List<GameObject>();
					List<CollectShiny> stalkersWithPlatinum = new List<CollectShiny>();
					HashSet<GameObject> set = WorldUtil.getObjectsNear(transform.position, 40);
					foreach (GameObject go in set) {
						PlatinumTag pt = go.GetComponent<PlatinumTag>();
						if (pt && pt.getTimeOnGround() >= 2.5F) {
							//collectorComponent.shinyTarget = pt.gameObject;
							loosePlatinum.Add(pt.gameObject);
						}
						CollectShiny c = go.GetComponent<CollectShiny>();
						if (c && c.shinyTarget && c.targetPickedUp && c.shinyTarget.GetComponent<PlatinumTag>()) {
							//collectorComponent.shinyTarget = c.shinyTarget;
							//triggerPtAggro(c.gameObject);
							//break;
							stalkersWithPlatinum.Add(c);
						}
					}
					bool flag = false;
					if (loosePlatinum.Count > 0) {
						collectorComponent.shinyTarget = loosePlatinum[UnityEngine.Random.Range(0, loosePlatinum.Count)];
						flag = true;
					}
					else {
						Player ep = Player.main;
						float dist = Vector3.Distance(ep.transform.position, transform.position);
						if (dist <= 30)  {
							int amt = Inventory.main.GetPickupCount(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType);
							//SNUtil.writeToChat("Counting platinum = "+amt);
							if (amt > 0 && UnityEngine.Random.Range(0F, 1F) <= Mathf.Min(amt*0.06F, 0.67F)) {
								triggerPtAggro(ep.gameObject);
								flag = true;
							}
						}
					}
					if (!flag && stalkersWithPlatinum.Count > 0) {
						CollectShiny c = stalkersWithPlatinum[UnityEngine.Random.Range(0, stalkersWithPlatinum.Count)];
						collectorComponent.shinyTarget = c.shinyTarget;
						triggerPtAggro(c.gameObject);
					}
				}
			}
			if (!GetComponent<WaterParkCreature>()) {
				float distp = Vector3.Distance(transform.position, Player.main.transform.position);
				if (distp >= 200) {
					destroyCreature();
				}
				else if (treaderTarget) {
					float dist = Vector3.Distance(transform.position, treaderTarget.transform.position);
					if (dist >= 150 && distp >= 30) {
						destroyCreature();
					}
					else if (dist >= 80) {
						swimmer.SwimTo(treaderTarget.transform.position, 20);	
					}
				}
				if (time >= lastDespawnCheck+2.5F && distp > 40) {
					lastDespawnCheck = time;
					HashSet<DeepStalkerTag> set = WorldUtil.getObjectsNearWithComponent<DeepStalkerTag>(transform.position, 60);
					int amt = 0;
					foreach (DeepStalkerTag c in set) {
						if (c.isAlive() && !c.GetComponent<WaterParkCreature>()) {
							amt++;
						}
					}
					if (amt > 5)
						destroyCreature();
				}
			}
		}
		
		private void destroyCreature() {
			collectorComponent.DropShinyTarget();
			UnityEngine.Object.DestroyImmediate(gameObject);
		}
		
		public bool isAlive() {
			return creatureComponent && creatureComponent.liveMixin && creatureComponent.liveMixin.IsAlive();
		}
		
		public bool currentlyHasPlatinum() {
			return collectorComponent && collectorComponent.targetPickedUp && collectorComponent.shinyTarget && collectorComponent.shinyTarget.GetComponent<PlatinumTag>();
		}
		
		internal void onHitWithElectricDefense() {
			currentForcedTarget = null;
			targetCooldown = DayNightCycle.main.timePassedAsFloat+12;
			creatureComponent.Aggression.Add(-1F);
			collectorComponent.DropShinyTarget();
			collectorComponent.timeNextFindShiny = targetCooldown;
		}
		
		public void OnMeleeAttack(GameObject target) {
			//SNUtil.writeToChat(this+" attacked "+target);
			if (target == Player.main.gameObject) {
				lastPlayerBiteTime = DayNightCycle.main.timePassedAsFloat;
				Pickupable p = Inventory.main.container.RemoveItem(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType);
				if (p) {
					float ch = Mathf.Clamp(SeaToSeaMod.config.getFloat(C2CConfig.ConfigEntries.PLATTHEFT), 0.25F, 1F);
					if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
						ch *= 3;
					if (UnityEngine.Random.Range(0F, 1F) < ch) {
						Inventory.main.InternalDropItem(p, false);
						grab(p.gameObject);
					}
				}
			}
			else {
				Stalker s = target.GetComponentInParent<Stalker>();
				if (s) {
					s.liveMixin.AddHealth(attackComponent.biteDamage);
					CollectShiny c = s.GetComponent<CollectShiny>();
					GameObject go = c.shinyTarget;
					if (go) {
						c.DropShinyTarget();
						grab(go);
					}
				}
			}
		}
		
		public void OnShinyPickedUp(GameObject target) {
			PlatinumTag pt = target.GetComponent<PlatinumTag>();
			if (pt)
				pt.pickup(this);
		}
		
		public void OnShinyDropped(GameObject target) {
			PlatinumTag pt = target.GetComponent<PlatinumTag>();
			if (pt)
				pt.drop();
		}
		
		private void grab(GameObject go) {
			collectorComponent.DropShinyTarget();
			collectorComponent.shinyTarget = go;
			collectorComponent.TryPickupShinyTarget();
		}
		
		internal void tryStealFrom(Stalker s) {
			triggerPtAggro(s.gameObject, true);
		}
		
		internal void bindToTreader(SeaTreader s) {
			treaderTarget = s;
			if (s)
				s.gameObject.GetComponent<C2CTreader>().attachStalker(this);
		}
		
		void OnDestroy() {
			if (treaderTarget) {
				C2CTreader c2c = treaderTarget.gameObject.GetComponent<C2CTreader>();
				if (c2c)
					c2c.removeStalker(this);
			}
			if (collectorComponent)
				collectorComponent.DropShinyTarget();
		}

		void OnDisable() {
			OnDestroy();
		}

		void OnKill() {
			OnDestroy();
		}
		
		internal void triggerPtAggro(GameObject target, bool isNew = true) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time < targetCooldown)
				return;
			if (GetComponent<WaterParkCreature>())
				return;
			if (isNew) {
				platinumGrabTime = time;
				//SNUtil.writeToChat(this+" aimed at "+target);
			}
			else {
				//SNUtil.writeToChat(this+" is seeking "+target);
			}
			currentForcedTarget = target;
			if (creatureComponent && creatureComponent.liveMixin && creatureComponent.liveMixin.IsAlive()) {
				creatureComponent.Aggression.Add(isNew ? 0.3F : 0.1F);
				if (playerHuntComponent) {
					playerHuntComponent.lastTarget.SetTarget(currentForcedTarget);
				}
				swimmer.SwimTo(currentForcedTarget.transform.position, 20);
			}
		}
		
	}
}
