using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using UnityEngine.UI;

using FMOD;
using FMODUnity;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ECCLibrary;

using DeExtinctionMod;
using DeExtinctionMod.Prefabs.Creatures;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.SeaToSea;

namespace ReikaKalseki.SeaToSea {
	
	public class DEIntegrationSystem {
		
		public static readonly DEIntegrationSystem instance = new DEIntegrationSystem();
		
		private readonly bool isDeELoaded;
		
		private readonly HashSet<TechType> creatures = new HashSet<TechType>();
		private readonly HashSet<TechType> eggs = new HashSet<TechType>();
		private TechType thelassaceanType;
		private TechType lrThelassaceanType;
		private TechType jellySpinnerType;
		private TechType rubyPincherType;
		private TechType gulperType;
		
		private VoidThalassacean voidThelassacean;
		
		public bool spawnVoidThalaAnywhere;
		public int maxVoidThala = 12;
    
	    internal WorldCollectedItem thalassaceanCud;
		
		private DEIntegrationSystem() {
	    	isDeELoaded = QModManager.API.QModServices.Main.ModPresent("DeExtinction");
	    	if (isDeELoaded) {
	    		
	    	}
		}
		
		public bool isLoaded() {
			return isDeELoaded;
		}
		
		public TechType getThalassacean() {
			return thelassaceanType;
		}
		
		public TechType getLRThalassacean() {
			return lrThelassaceanType;
		}
		
		public TechType getRubyPincher() {
			return rubyPincherType;
		}
		
		public TechType getGulper() {
			return gulperType;
		}
	    
	    public VoidThalassacean getVoidThalassacean() {
	    	return voidThelassacean;
	    }
		
		internal void applyPatches() {
			if (isDeELoaded)
				doApplyPatches();
		}
		
		private void doApplyPatches() {
			bool hard = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
						
			thalassaceanCud = new WorldCollectedItem(SeaToSeaMod.itemLocale.getEntry("ThalassaceanCud"), "bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782");
			thalassaceanCud.renderModify = (r) => {
				C2CThalassaceanCudTag.setupRenderer(r);
			};
			thalassaceanCud.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/ThalassaceanCud");
			thalassaceanCud.Patch();
			
			BioReactorHandler.SetBioReactorCharge(thalassaceanCud.TechType, BaseBioReactor.GetCharge(TechType.Hoopfish));
			
			thelassaceanType = findCreature("StellarThalassacean");
			lrThelassaceanType = findCreature("JasperThalassacean");
			
			jellySpinnerType = findCreature("JellySpinner");
			
			creatures.Add(thelassaceanType);
			creatures.Add(lrThelassaceanType);
			creatures.Add(jellySpinnerType);
			creatures.Add(findCreature("Twisteel"));
			creatures.Add(findCreature("Filtorb"));
			gulperType = findCreature("GulperLeviathan");
			creatures.Add(gulperType);
			creatures.Add(findCreature("GulperLeviathanBaby"));
			creatures.Add(findCreature("GrandGlider"));
			creatures.Add(findCreature("Axetail"));
			creatures.Add(findCreature("RibbonRay"));
			creatures.Add(findCreature("Filtorb"));
			creatures.Add(findCreature("TriangleFish"));
			rubyPincherType = findCreature("RubyClownPincher");
			creatures.Add(rubyPincherType);
			creatures.Add(findCreature("SapphireClownPincher"));
			creatures.Add(findCreature("EmeraldClownPincher"));
			creatures.Add(findCreature("AmberClownPincher"));
			creatures.Add(findCreature("CitrineClownPincher"));
			
			eggs.Add(findCreature("GrandGliderEgg"));
			eggs.Add(findCreature("StellarThalassaceanEgg"));
			eggs.Add(findCreature("JasperThalassaceanEgg"));
			eggs.Add(findCreature("TwisteelEgg"));
			eggs.Add(findCreature("GulperEgg"));
			
			int amt = RecipeUtil.removeIngredient(C2CItems.powerSeal.TechType, EcoceanMod.glowOil.TechType).amount;
			RecipeUtil.addIngredient(C2CItems.powerSeal.TechType, thalassaceanCud.TechType, amt);
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.HeatSealant).TechType, thalassaceanCud.TechType, 2);
			RecipeUtil.addIngredient(CraftingItems.getItem(CraftingItems.Items.SealFabric).TechType, thalassaceanCud.TechType, 1);
			RecipeUtil.addIngredient(C2CItems.depth1300.TechType, thalassaceanCud.TechType, 4);
			RecipeUtil.addIngredient(C2CItems.bandage.TechType, thalassaceanCud.TechType, 1);
			
			amt = RecipeUtil.removeIngredient(C2CItems.breathingFluid.TechType, TechType.Eyeye).amount;
			RecipeUtil.addIngredient(C2CItems.breathingFluid.TechType, jellySpinnerType, amt*3/2); //from 2 to 3
			
			foreach (TechType tt in eggs) {
				CreatureEggAsset egg = (CreatureEggAsset)SNUtil.getModPrefabByTechType(tt);
				foreach (LootDistributionData.BiomeData bd in egg.BiomesToSpawnIn) {
					float f = bd.probability;
					f = Mathf.Min(f, 0.75F)*0.67F;
					f = Mathf.Round(f*20F)/20F; //round to nearest 0.05
					f = Mathf.Max(f, 0.05F);
					SNUtil.log("Reducing spawn chance of "+egg.ClassID+" in "+Enum.GetName(typeof(BiomeType), bd.biome)+" from "+bd.probability+" to "+f);
					LootDistributionHandler.EditLootDistributionData(egg.ClassID, bd.biome, f, 1);
				}
			}
			
			ThalassaceanPrefab pfb = (ThalassaceanPrefab)SNUtil.getModPrefabByTechType(thelassaceanType);
			voidThelassacean = new VoidThalassacean(SeaToSeaMod.itemLocale.getEntry("VoidThalassacean"), pfb);
			voidThelassacean.Patch();
		}
		
		private TechType findCreature(string id) {
			TechType tt = TechType.None;
			if (!TechTypeHandler.TryGetModdedTechType(id, out tt))
				if (!TechTypeHandler.TryGetModdedTechType(id.ToLowerInvariant(), out tt))
					TechTypeHandler.TryGetModdedTechType(id.setLeadingCase(false), out tt);
			if (tt == TechType.None)
				throw new Exception("Could not find DeE TechType for '"+id+"'");
			return tt;
		}
	    
	    [Obsolete("Unimplemented")]
	    public void convertEgg(string type, float r) {
	    	foreach (PrefabIdentifier pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(Player.main.transform.position, r)) {
	    		if (pi && pi.ClassId == type) {
	    			//TODO
	    		}
	    	}
	    }
	    
	    public void tickVoidThalassaceanSpawner(Player ep) {
	    	if (spawnVoidThalaAnywhere || (ep.transform.position.y >= -800 && VanillaBiomes.VOID.isInBiome(ep.transform.position))) {
				HashSet<VoidThalassaceanTag> has = WorldUtil.getObjectsNearWithComponent<VoidThalassaceanTag>(ep.transform.position, 200);
				if (has.Count < maxVoidThala) {
					for (int i = has.Count; i < maxVoidThala; i++) {
						Vector3 pos = MathUtil.getRandomPointAtSetDistance(ep.transform.position, 200);
						if (pos.y > -25)
							continue;
						if (spawnVoidThalaAnywhere || VanillaBiomes.VOID.isInBiome(pos)) {
							GameObject go = ObjectUtil.createWorldObject(voidThelassacean.ClassID);
							go.transform.position = pos;
							ObjectUtil.fullyEnable(go);
							//SNUtil.writeToChat("spawned void thalassacean at "+go.transform.position+" dist="+Vector3.Distance(pos, ep.transform.position));
						}
					}
				}
	    	}
		}
	    
	    internal class C2CThalassacean : MonoBehaviour {
	    	
	    	public static readonly string MOUTH_NAME = "Mouth"; //already has one
			
			public static readonly float REGROW_TIME = 3600; //60 min, but do not serialize, so will reset if leave and come back
			
			internal float lastCollect = -9999;
	    	
	    	private GameObject mouthInteract;
	    	
	    	private GameObject mouthItem;
	    	
	    	void Start() {
	    		mouthInteract = ObjectUtil.getChildObject(gameObject, MOUTH_NAME);
	    	}
			
			void Update() {
	    		if (!DayNightCycle.main)
	    			return;
	    		if (!mouthInteract)
	    			mouthInteract = ObjectUtil.getChildObject(gameObject, MOUTH_NAME);
	    		
				bool act = DayNightCycle.main.timePassedAsFloat-lastCollect >= REGROW_TIME;
				//mouthInteract.SetActive(act);
				if (act && mouthInteract && (!mouthItem || !mouthItem.activeInHierarchy || mouthItem.transform.parent != mouthInteract.transform)) {
					mouthItem = ObjectUtil.createWorldObject(instance.thalassaceanCud.ClassID);
					mouthItem.SetActive(true);
	    			mouthItem.transform.SetParent(mouthInteract.transform);
				}
				if (mouthItem)
	    			mouthItem.transform.localPosition = new Vector3(0, 0, -0.5F);
			}
			/*
			public bool collect() {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time-lastCollect < REGROW_TIME)
					return false;
				InventoryUtil.addItem(instance.thalassaceanCud.TechType);
				lastCollect = time;
				return true;
			}*/
	    	
	    }
	    /*
	    internal class C2CThalassaceanMouthTag : MonoBehaviour, IHandTarget {
	    	
	    	private SphereCollider interact;			
			private C2CThalassacean owner;
	    	
	    	void Start() {
	    		interact = gameObject.EnsureComponent<SphereCollider>();
	    		interact.radius = 0.5F;
				owner = gameObject.FindAncestor<C2CThalassacean>();
	    	}
			
			public void OnHandHover(GUIHand hand) {
				HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
				HandReticle.main.SetInteractText("ThalassaceanMouthClick");
				HandReticle.main.SetTargetDistance(8);
			}
		
			public void OnHandClick(GUIHand hand) {
				owner.collect();
			}
	    	
	    }*/
	    internal class C2CThalassaceanCudTag : MonoBehaviour {
	    	
	    	private float lastParentageCheck;
	    	
	    	void Start() {
	    		Invoke("setupRenderer", 0.5F);
	    	}
	    	
	    	public void setupRenderer() {
	    		setupRenderer(this);
	    	}
	    	
	    	public static void setupRenderer(Component c) {
				GameObject root = c.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
				GasPod gp = root.GetComponent<GasPod>();
				ObjectUtil.removeComponent<UWE.TriggerStayTracker>(root);
				ObjectUtil.removeComponent<FMOD_StudioEventEmitter>(root);
				ObjectUtil.removeComponent<ResourceTracker>(root);
				GameObject pfb = ObjectUtil.lookupPrefab("505e7eff-46b3-4ad2-84e1-0fadb7be306c");
				GameObject mdl = UnityEngine.Object.Instantiate(pfb.GetComponentInChildren<Animator>().gameObject);
				ObjectUtil.removeChildObject(mdl, "root", false);
				mdl.transform.SetParent(gp.model.transform.parent);
				mdl.transform.localPosition = gp.model.transform.localPosition;
				UnityEngine.Object.DestroyImmediate(gp.model);
				UnityEngine.Object.DestroyImmediate(gp);
				Renderer r = root.GetComponentInChildren<Renderer>();
				//SNUtil.log("Adjusting Thalassacean cud renderer "+r.gameObject.GetFullHierarchyPath());
				Color clr = new Color(0.67F, 0.95F, 0.2F, 0.5F);//new Color(0.4F, 0.3F, 0.1F);
				Animator a = root.GetComponentInChildren<Animator>();
				a.transform.localScale = Vector3.one*2;
				a.speed = 0.5F;
				r.materials[0].SetColor("_Color", clr);
				r.materials[0].SetColor("_SpecColor", clr);
				r.materials[0].SetFloat("_Fresnel", 0.5F);
				r.materials[0].SetFloat("_Shininess", 0F);
				r.materials[0].SetFloat("_SpecInt", 0.75F);
				r.materials[0].SetFloat("_EmissionLM", 15F);
				r.materials[0].SetFloat("_EmissionLMNight", 15F);
				r.materials[0].SetFloat("_MyCullVariable", 1.6F);
				root.GetComponent<SphereCollider>().radius = 0.7F;
	    	}
	    	
	    	void Update() {
	    		float time = DayNightCycle.main.timePassedAsFloat;
	    		if (time-lastParentageCheck >= 1) {
		    		lastParentageCheck = time;
		    		if (!gameObject.FindAncestor<Creature>())
		    			UnityEngine.Object.Destroy(gameObject);
	    		}
	    	}
	    	
	    }
	    
		public class VoidThalassacean : ThalassaceanPrefab {
	    	
	    	private readonly XMLLocale.LocaleEntry locale;
	    	private readonly ThalassaceanPrefab template;
	    	
	    	internal VoidThalassacean(XMLLocale.LocaleEntry e, ThalassaceanPrefab p) : base(e.key, e.name, e.desc, (GameObject)getECCField(p, "model"), null) {
	    		locale = e;
	    		template = p;
	    		typeof(CreatureAsset).GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, (Atlas.Sprite)getECCField(template, "sprite"));
	    	}
	    	
	    	private static object getECCField(CreatureAsset c, string name) {
	    		return typeof(CreatureAsset).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(c);
	    	}
			
			public override GameObject GetGameObject() {
	    		GameObject go = base.GetGameObject();
	    		Renderer r = go.GetComponentInChildren<Renderer>();
	    		RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/Creature/VoidThalassacean");
	    		go.EnsureComponent<VoidThalassaceanTag>();
	    		go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
				return go;
			}
	    	
			public override string GetEncyDesc {
				get {
					return locale.pda;
				}
			}
	
			public override string GetEncyTitle {
				get {
					return locale.name;
				}
			}

			public override ScannableItemData ScannableSettings {
				get {
	    			ScannableItemData dat = template.ScannableSettings;
	    			dat.scanTime *= 1.5F;
	    			return dat;
				}
			}
		
			public override List<LootDistributionData.BiomeData> BiomesToSpawnIn {
				get {
	    			return new List<LootDistributionData.BiomeData>();
	    		}
			}
		}
	    
	    class VoidThalassaceanTag : MonoBehaviour, IOnTakeDamage {
	    	
	    	private static readonly float AGGRESSION_TIME = 2.5F;
	    	
	    	private static readonly Color calmColor = new Color(0.2F, 0.5F, 1F);
	    	private static readonly Color warnColor = new Color(1F, 0.75F, 0.15F);
	    	private static readonly Color attackingColor = new Color(1F, 0.1F, 0.05F);
	    	
	    	private static readonly SoundManager.SoundData aggroStartSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidthalaroar2", "Sounds/voidthalaroar2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 128);}, SoundSystem.masterBus);
			private static readonly SoundManager.SoundData attackStartSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidthalachirp", "Sounds/voidthalachirp.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 128);}, SoundSystem.masterBus);
			private static readonly SoundManager.SoundData attackHitSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidthalahit", "Sounds/voidthalahit.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 128);}, SoundSystem.masterBus);
		
			private Renderer renderer;
	    	//private SwimRandom swimTarget;
	    	//private AggressiveWhenSeeTarget aggression;
	    	//private AggressiveToPilotingVehicle aggression2;
	    	private AttackLastTarget attack;
	    	internal Rigidbody body;
	    	private SwimBehaviour behavior;
	    	
	    	private readonly List<VoidThalaHitDetection> triggers = new List<VoidThalaHitDetection>();
	    	private static readonly List<int> aggroTokens = new List<int>(){1, 2, 3}; //prevent more than three tag teaming
	    	
	    	private float aggressionLevel;
	    	private float aggressionColorFade;
	    	
	    	private float timeAggressive;
	    	private float timeFleeing;
	    	
	    	private float flashCycleVar;
	    	
	    	private Vector3 runAwayTarget;
	    	
	    	private int currentAggroToken;
	    	
	    	private float returnAttackLifetime;
	    	
	    	void Start() {
	    		float r = 100;
	    		renderer = GetComponentInChildren<Renderer>();
	    		renderer.materials[0].SetFloat("_Shininess", 2.5F);
	    		renderer.materials[0].SetFloat("_SpecInt", 5.0F);
	    		renderer.materials[0].SetFloat("_Fresnel", 0.75F);
	    		body = GetComponent<Rigidbody>();
	    		behavior = GetComponent<SwimBehaviour>();
	    		behavior.turnSpeed *= 1.5F;
	    		//swimTarget = GetComponent<SwimRandom>();
	    		/*
	    		aggression = gameObject.EnsureComponent<AggressiveWhenSeeTarget>();
	    		aggression.aggressionPerSecond = 1;
	    		aggression.creature = GetComponent<Creature>();
	    		aggression.ignoreSameKind = true;
	    		aggression.myTechType = instance.voidThelassacean.TechType;
	    		aggression.targetType = EcoTargetType.Shark;
	    		aggression.maxRangeScalar = r;
	    		aggression.isTargetValidFilter = (eco => ObjectUtil.isPlayer(eco.GetGameObject()));
	    		aggression.lastTarget = gameObject.EnsureComponent<LastTarget>();
	    		aggression2 = gameObject.EnsureComponent<AggressiveToPilotingVehicle>();
	    		aggression2.aggressionPerSecond = aggression.aggressionPerSecond;
	    		aggression2.creature = aggression.creature;
	    		aggression2.lastTarget = aggression.lastTarget;
	    		aggression2.range = r;
	    		aggression2.updateAggressionInterval = 0.5F;
	    		*/
	    		attack = gameObject.EnsureComponent<AttackLastTarget>();
	    		attack.aggressionThreshold = 0.5F;
	    		attack.creature = GetComponent<Creature>();//aggression.creature;
	    		attack.lastTarget = gameObject.EnsureComponent<LastTarget>();//aggression.lastTarget;
	    		attack.maxAttackDuration = 60;
	    		attack.minAttackDuration = 30;
	    		attack.swimVelocity = 45;
	    		attack.creature.ScanCreatureActions();
	    		
	    		GetComponent<LiveMixin>().damageReceivers = GetComponents<IOnTakeDamage>();
	    		
	    		Invoke("delayedStart", 0.5F);
	    	}
	    	
	    	void delayedStart() {
	    		foreach (Collider c in GetComponentsInChildren<Collider>(true)) {
	    			if (!c.isTrigger) {
	    				c.gameObject.EnsureComponent<VoidThalaHitDetection>().owner = this;
	    			}
	    		}
	    	}
	    	
	    	private GameObject getTarget(out bool vehicle) {
	    		if (GameModeUtils.IsInvisible()) {
	    			vehicle = false;
	    			return null;
	    		}
	    		Vehicle v = Player.main.GetVehicle();
	    		vehicle = (bool)v;
	    		return v ? v.gameObject : Player.main.gameObject;
	    	}
	    	
	    	void Update() {
	    		transform.localScale = Vector3.one*1.5F;
	    		bool vehicle;
	    		GameObject go = getTarget(out vehicle);
	    		bool flag = false;
	    		Color c = calmColor;
	    		
	    		float dT = Time.deltaTime;
	    		
	    		if (go) {
	    			float distSq = (go.transform.position-transform.position).sqrMagnitude;
	    			//SNUtil.writeToChat("D="+((int)(Mathf.Sqrt(distSq)))/10*10);
	    			if (distSq > 90000) { //more than 300m
	    				UnityEngine.Object.Destroy(gameObject);
	    				return;
	    			}
	    			else if (distSq < (vehicle ? 2500 : 400) || aggressionLevel > 0.9F || (returnAttackLifetime > 0 && distSq < 25600)) { //within 50m in vehicle or 20 on foot, or a queued attack
	    				flag = true;
	    			}
	    			else if (aggressionLevel < 0 && (distSq > 2500 || (transform.position-runAwayTarget).sqrMagnitude < 900)) { //more than 50m away while running, or at position
	    				aggressionLevel = 0;
	    				//SNUtil.writeToChat("Zeroing flee");
	    			}
	    		}
	    		else {
	    			aggressionLevel = 0;
	    		}
	    		
	    		if (Math.Abs(aggressionLevel) < 0.01F && body.velocity.magnitude >= 5) {
	    			body.velocity = body.velocity*0.995F;
	    			//SNUtil.writeToChat("Braking");
	    		}
	    		
	    		if (timeFleeing > 15)
	    			aggressionLevel = 0;
	    		
	    		if (returnAttackLifetime > 0)
	    			returnAttackLifetime -= dT;
	    		
	    		if (flag) {
	    			if (aggressionLevel >= 0) {
		    			bool wasAny = aggressionLevel > 0;
		    			bool was = aggressionLevel >= 1;
		    			aggressionLevel = Mathf.Clamp01(aggressionLevel+Time.deltaTime/AGGRESSION_TIME);
		    			if (aggressionLevel >= 1 && !was) {
		    				SoundManager.playSoundAt(attackStartSound, transform.position, false, 128, 2);
		    			}
		    			else if (aggressionLevel > 0 && !wasAny) {
		    				SoundManager.playSoundAt(aggroStartSound, transform.position, false, 128, 2);
		    			}
	    			}
	    		}
	    		else {
	    			aggressionLevel = 0;
		    		//SNUtil.writeToChat("No target, calming");
	    		}
	    		
	    		bool flag2 = false;
	    		if (aggressionLevel < 0) {
	    			flag2 = true;
	    			behavior.SwimTo(runAwayTarget, (runAwayTarget - transform.position).normalized, attack.swimVelocity*0.67F);
	    			timeFleeing += dT;
	    		}
	    		else if (aggressionLevel >= 1 && tryAllocateAggroToken()) {
	    			attack.lastTarget.target = go;
	    			attack.currentTarget = go;
	    			behavior.Attack(go.transform.position, (go.transform.position - transform.position).normalized, attack.swimVelocity);
	    			aggressionColorFade = Mathf.Clamp01(aggressionColorFade+dT*2);
	    			timeAggressive += dT;
		    		//SNUtil.writeToChat("Attacking!");
		    		timeFleeing = 0;
	    		}
	    		else {
	    			flag2 = true;
		    		timeFleeing = 0;
	    		}
	    		
	    		if (flag2) {
	    			aggressionColorFade = Mathf.Clamp01(aggressionColorFade-dT*0.5F);
	    			clearAggro();
	    		}
	    		
	    		if (timeAggressive > 30)
	    			resetAggro(true);
	    		
	    		if (aggressionColorFade > 0) {
	    			c = Color.Lerp(warnColor, attackingColor, aggressionColorFade);
	    		}
	    		else {
	    			c = Color.Lerp(calmColor, warnColor, aggressionLevel);
	    		}
	    		
	    		float f = body.velocity.magnitude/attack.swimVelocity;
	    		if (!flag2) //fast while fading from yellow to red
	    			f = 1.2F;
	    		flashCycleVar += dT*Mathf.Deg2Rad*6000*f; //faster flashing the faster it goes
	    		float glow = 3.5F+2.5F*Mathf.Sin(flashCycleVar); 
	    		if (aggressionLevel < 0)
	    			glow *= 1+aggressionLevel;
	    		
	    		renderer.materials[0].SetColor("_GlowColor", c);
	    		RenderUtil.setEmissivity(renderer, glow);
	    	}
	    	
			private bool tryAllocateAggroToken() {
	    		if (currentAggroToken > 0)
	    			return true;
				if (aggroTokens.Count == 0)
					return false;
				currentAggroToken = aggroTokens[0];
				aggroTokens.RemoveAt(0);
				return true;
			}
	    	
			private void clearAggro() {
				timeAggressive = 0;
				attack.lastTarget.target = null;
				attack.currentTarget = null;
				if (currentAggroToken != 0) {
					if (aggroTokens.Contains(currentAggroToken))
						SNUtil.writeToChat("Two voidthala with same aggro token: "+currentAggroToken);
					aggroTokens.Add(currentAggroToken);
					currentAggroToken = 0;
				}
				//SNUtil.writeToChat("Clearing aggression values");
			}
	    	
			public void resetAggro(bool deflect) {
				aggressionLevel = -1;
				Vector3 offset = body.velocity.setLength(120);
				offset = MathUtil.getRandomVectorAround(offset, deflect ? 90 : 50).setLength(120);
	    		runAwayTarget = Player.main.transform.position+offset;//MathUtil.getRandomPointAtSetDistance(transform.position, 100);
	    		if (runAwayTarget.y > -20)
	    			runAwayTarget = runAwayTarget.setY(-20);
		    	//SNUtil.writeToChat("Resetting aggro");
		    	Invoke("playFleeSound", 0.8F);
	    	}
	    	
	    	private void playFleeSound() {
	    		CancelInvoke("playFleeSound");
	    		SoundManager.playSoundAt(attackHitSound, transform.position, false, 128, 1);
	    	}
	    	
	    	public void OnTakeDamage(DamageInfo info) {
	    		if (info.type == DamageType.Electrical || info.type == DamageType.Normal)
	    			resetAggro(true);
	    	}
	    	
	    	public void returnAttack() {
	    		returnAttackLifetime = 30;
	    	}
	    	
		}
	    
		class VoidThalaHitDetection : MonoBehaviour {
	    	
			internal VoidThalassaceanTag owner;
	    	
			void Start() {
				if (!owner)
					owner = gameObject.FindAncestor<VoidThalassaceanTag>();
			}
	    	
			void OnCollisionEnter(Collision c) {
				if (!owner)
					return;
				if (c.gameObject.FindAncestor<VoidThalaImpactImmunity>())
					return;
				if (ObjectUtil.isPlayer(c.collider) || c.collider.gameObject.FindAncestor<Vehicle>()) {
					owner.resetAggro(false);
					if (UnityEngine.Random.Range(0F, 1F) < 0.67F) {
						owner.Invoke("returnAttack", UnityEngine.Random.Range(10F, 20F));
					}
					Vehicle v = c.collider.gameObject.FindAncestor<Vehicle>();
					if (v && v.liveMixin) {
						v.liveMixin.TakeDamage(2, c.contacts[0].point, DamageType.Normal, owner.gameObject);
					}
					c.rigidbody.AddForce(owner.body.velocity.setLength(15), ForceMode.VelocityChange);
					c.gameObject.EnsureComponent<VoidThalaImpactImmunity>().elapseWhen = DayNightCycle.main.timePassedAsFloat+0.5F;
				}
			}
	    	
		}
	    
	    class VoidThalaImpactImmunity : SelfRemovingComponent {
	    	
	    }
		
	}
	
}
