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
using ReikaKalseki.Exscansion;
using ECCLibrary;

namespace ReikaKalseki.SeaToSea {
	
	public class VoidSpikeLeviathan : CreatureAsset {
		
		private readonly XMLLocale.LocaleEntry locale;
	        
		internal VoidSpikeLeviathan(XMLLocale.LocaleEntry e) : base("voidspikelevi", e.name, e.desc, loadAsset(), null) {
			locale = e;
	    }

		public override EcoTargetType EcoTargetType {
			get {
				return EcoTargetType.Leviathan;
			}
		}

		public override BehaviourType BehaviourType {
			get {
				return BehaviourType.Leviathan;
			}
		}

		public override LargeWorldEntity.CellLevel CellLevel {
			get {
				return LargeWorldEntity.CellLevel.VeryFar;
			}
		}

		public override SwimRandomData SwimRandomSettings {
			get {
				return new SwimRandomData(true, Vector3.one*150, 10F, 5F, 0.1F);
			}
		}

		public override CreatureAsset.CreatureTraitsData TraitsSettings {
			get {
				return new CreatureAsset.CreatureTraitsData(0.1F, 0.02F, 0.25F);
			}
		}

		public override ItemSoundsType ItemSounds {
			get {
				return ItemSoundsType.Fish;
			}
		}

		public override UBERMaterialProperties MaterialSettings {
			get {
				return new UBERMaterialProperties(1F, 3F, 2F);
			}
		}

		public override CreatureAsset.StayAtLeashData StayAtLeashSettings {
			get {
				return new CreatureAsset.StayAtLeashData(0.5F, 150);
			}
		}

		public override CreatureAsset.RespawnData RespawnSettings {
			get {
				return new CreatureAsset.RespawnData(false);
			}
		}

		public override float TurnSpeedHorizontal {
			get {
				return 0.5F;
			}
		}

		public override float TurnSpeedVertical {
			get {
				return 0.8F;
			}
		}

		public override bool ScannerRoomScannable {
			get {
				return true;
			}
		}

		public override bool CanBeInfected {
			get {
				return false;
			}
		}
/*
		public override CreatureAsset.RoarAbilityData RoarAbilitySettings {
			get {
				return new RoarAbilitySettings();
			}
		}
*/
		public override CreatureAsset.SmallVehicleAggressivenessSettings AggressivenessToSmallVehicles {
			get {
				return new CreatureAsset.SmallVehicleAggressivenessSettings(1F, 440F);
			}
		}

		public override float Mass {
			get {
				return 10000F; //4x reaper
			}
		}

		public override CreatureAsset.BehaviourLODLevelsStruct BehaviourLODSettings {
			get {
				return new CreatureAsset.BehaviourLODLevelsStruct(600, 999, 999);
			}
		}

		public override float EyeFov {
			get {
				return -1;
			}
		}

		public override CreatureAsset.AvoidObstaclesData AvoidObstaclesSettings {
			get {
				return new CreatureAsset.AvoidObstaclesData(0.2F, false, 25);
			}
		}

		public override string GetEncyDesc {
			get {
				return locale.pda;
			}
		}

		public override ScannableItemData ScannableSettings {
			get {
				const string path = "Lifeforms/Fauna/Leviathans";
				return new ScannableItemData(true, 20, path, path.Split('/'), null, TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/PDA/"+locale.getField<string>("header")));
			}
		}

        public override void AddCustomBehaviour(CreatureComponents cc) {
            //if you have special components you want to add here (like your own custom CreatureActions) then add them here
            if (cc.infectedMixin)
            	cc.infectedMixin.RemoveInfection();
            if (cc.locomotion)
            	cc.locomotion.maxVelocity = 30F;
            cc.pickupable = null;
            if (cc.renderer)
				cc.renderer.materials[0].SetFloat("_Fresnel", 0.8F);
            prefab.EnsureComponent<VoidSpikeLeviathanAI>();
            CreateTrail(prefab, cc, 1.5F, 15F, 1.5F);
			MakeAggressiveTo(512, 8, EcoTargetType.Leviathan, 0, 1F);
			MakeAggressiveTo(512, 8, EcoTargetType.Whale, 0.0F, 0.5F);
			MakeAggressiveTo(400, 6, EcoTargetType.Shark, 0.2F, 0.25F);
        }

        public override void SetLiveMixinData(ref LiveMixinData data) {
            data.maxHealth = 75000F; //15x reaper
        }
		
		private static GameObject loadAsset() {
			 AssetBundle ab = ReikaKalseki.DIAlterra.AssetBundleManager.getBundle(SeaToSeaMod.modDLL, "voidlevi");
			 return ab.LoadAsset<GameObject>("VoidSpikeLevi_FixedRig");
		}
		
		public void register() {
			Patch();
			ESHooks.addLeviathan(TechType);
			//SNUtil.addPDAEntry(this, 20, "Lifeforms/Fauna/Leviathans", locale.pda, locale.getField<string>("header"));
		}
	
		internal class VoidSpikeLeviathanAI : MonoBehaviour {
	    	
	    	private Creature creatureClass;
			
	    	void Start() {
	    		
	    	}
			
			void Update() {
	    		if (!creatureClass)
	    			creatureClass = GetComponent<Creature>();
	    		
	    		if (creatureClass)
	    			creatureClass.leashPosition = Player.main.transform.position;
			}
			
			void OnDisable() {
	    		UnityEngine.Object.DestroyImmediate(gameObject);
			}
			
			void OnDestroy() {
				VoidSpikeLeviathanSystem.instance.deleteVoidLeviathan();
			}
		
			public void OnMeleeAttack(GameObject target) {
				//SNUtil.writeToChat(this+" attacked "+target);
				Vehicle v = target.GetComponent<Vehicle>();
				if (v) {
					VoidSpikeLeviathanSystem.instance.shutdownSeamoth(v, 6); //shut down for up to 30s, drain up to 25% power
				}
			}
			
		}
	}
}
