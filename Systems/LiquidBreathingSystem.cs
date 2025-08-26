using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea {

	public class LiquidBreathingSystem {

		public static readonly LiquidBreathingSystem instance = new LiquidBreathingSystem();

		internal static readonly float ITEM_VALUE = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 1800 : 2700;
		//seconds
		internal static readonly float TANK_CHARGE = 10 * 60;
		//how much time you can spend (total) of liquid before returning to a base with a charger
		internal static readonly float TANK_CAPACITY = 2.5F * 60;
		//per "air tank" before you need to go back to a powered air-filled space

		private static readonly string customHUDText = "CF<size=30>X</size>•O<size=30>Y</size>";

		private LiquidBreathingHUDMeters meters;

		private Texture2D baseO2BarTexture;
		private Color baseO2BarColor;
		private Texture2D baseO2BubbleTexture;
		private float baseOverlayAlpha2;
		private float baseOverlayAlpha1;
		private string baseLabel;

		private float lastRechargeRebreatherTime = -1;
		private float rechargingTintStrength = 0;

		private float forceAllowO2 = 0;

		private float lastUnequippedTime = -1;

		private bool hasTemporaryKharaaTreatment;
		private bool startedUsingTemporaryKharaaTreatment;
		public float kharaaTreatmentRemainingTime { get; private set; }

		private LiquidBreathingSystem() {

		}

		public void onEquip() {
			InfectedMixin inf = Player.main.GetComponent<InfectedMixin>();
			inf.SetInfectedAmount(Mathf.Max(inf.infectedAmount, 0.25F));
		}

		public void onUnequip() {
			float amt = Player.main.oxygenMgr.GetOxygenAvailable();
			lastUnequippedTime = DayNightCycle.main.timePassedAsFloat;
			Player.main.oxygenMgr.RemoveOxygen(amt/*-1*/);
			//SNUtil.writeToChat("Removed "+amt+" oxygen, player now has "+Player.main.oxygenMgr.GetOxygenAvailable());
			SoundManager.playSoundAt(SoundManager.buildSound(Player.main.IsUnderwater() ? "event:/player/Puke_underwater" : "event:/player/Puke"), Player.main.lastPosition, false, 12);
		}

		public float getLastUnequippedTime() {
			return lastUnequippedTime;
		}

		public void refreshGui() {
			lastRechargeRebreatherTime = DayNightCycle.main.timePassedAsFloat;
		}
		/*
	    public void refillPlayerO2Bar(Player p, float amt) {
			forceAllowO2 += amt;
			if (amt > 0)
				p.oxygenMgr.AddOxygen(amt);
			onAddO2ToBar();
			//SNUtil.writeToChat("Added "+add);
	    }*/
		/*
	    internal void onAddO2ToBar(float amt) {
	    	if (!hasLimited)
	    		return;
	    	Oxygen o = getTankTank();
	    	float rem = o.oxygenAvailable-75;
	    	if (rem > 0)
	    		o.RemoveOxygen(rem);
	    }*/

		public float getFuelLevel() {
			Battery b = this.getTankBattery();
			return b ? b.charge : 0;
		}

		public float getAvailableFuelSpace() {
			Battery b = this.getTankBattery();
			return b ? b.capacity - b.charge : 0;
		}

		private Battery getTankBattery() {
			InventoryItem tank = Inventory.main.equipment.GetItemInSlot("Tank");
			if (tank.item.GetTechType() != C2CItems.liquidTank.TechType)
				return null;
			Battery b = tank.item.gameObject.GetComponent<Battery>();
			return b;
		}

		private Oxygen getTankTank() {
			InventoryItem tank = Inventory.main.equipment.GetItemInSlot("Tank");
			if (tank.item.GetTechType() != C2CItems.liquidTank.TechType)
				return null;
			Oxygen b = tank.item.gameObject.GetComponent<Oxygen>();
			return b;
		}

		public float rechargePlayerLiquidBreathingFuel(float amt) {
			Battery b = this.getTankBattery();
			if (!b)
				return 0;
			float add = Mathf.Min(amt, b.capacity - b.charge);
			if (add > 0) {
				b.charge += add;
				this.refreshGui();
			}
			return add;
		}

		public bool isLiquidBreathingActive(Player ep) {
			if (this.isInPoweredArea(ep))
				return false;
			Vehicle v = ep.GetVehicle();
			if (v && !v.IsPowered())
				return true;
			SubRoot sub = ep.currentSub;
			return (sub && sub.powerRelay && !sub.powerRelay.IsPowered()) || ep.IsUnderwater() || ep.IsSwimming();
		}

		public bool isO2BarAbleToFill(Player ep) {
			return !this.hasTankButNoMask() && !EnvironmentalDamageSystem.instance.isPlayerRecoveringFromPressure() && (!this.hasLiquidBreathing() || this.isInPoweredArea(ep) || !this.isLiquidBreathingActive(ep));
		}

		public bool isInPoweredArea(Player p) {
			if (!p)
				return false;
			if (p.IsUnderwater() || p.IsSwimming())
				return false;
			if (p.currentEscapePod && p.currentEscapePod == EscapePod.main && Story.StoryGoalManager.main && EscapePod.main.fixPanelGoal != null && Story.StoryGoalManager.main.IsGoalComplete(EscapePod.main.fixPanelGoal.key))
				return true;
			Vehicle v = p.GetVehicle();
			if (v && v.IsPowered())
				return true;
			SubRoot sub = p.currentSub;
			return (sub && sub.powerRelay && sub.powerRelay.IsPowered()) || p.precursorOutOfWater;
		}

		public bool tryFillPlayerO2Bar(Player p, ref float amt, bool force = false) {
			if (this.hasTankButNoMask()) {
				amt = 0;
				return false;
			}
			if (!this.hasLiquidBreathing())
				return true;
			if (!force) {
				if (!this.isInPoweredArea(p)) {
					amt = 0;
					return false;
				}
			}
			Battery b = this.getTankBattery();
			if (!b) {
				amt = 0;
				return false;
			}
			amt = Mathf.Min(amt, b.charge);
			//if (hasReducedCapacity()) does not work reliably
			//	amt = Mathf.Min(amt, 75-getTankTank().oxygenAvailable);
			if (amt > 0)
				b.charge -= amt;
			//SNUtil.writeToChat(amt+" > "+b.charge);
			return amt > 0;
		}

		public bool hasTankButNoMask() {
			return Inventory.main.equipment.GetTechTypeInSlot("Head") != C2CItems.rebreatherV2.TechType && Inventory.main.equipment.GetTechTypeInSlot("Tank") == C2CItems.liquidTank.TechType;
		}

		public bool hasLiquidBreathing() {
			return Inventory.main.equipment.GetTechTypeInSlot("Head") == C2CItems.rebreatherV2.TechType && Inventory.main.equipment.GetTechTypeInSlot("Tank") == C2CItems.liquidTank.TechType;
		}

		public void checkLiquidBreathingSupport(OxygenArea a) {
			OxygenAreaWithLiquidSupport oxy = a.gameObject.GetComponent<OxygenAreaWithLiquidSupport>();
			//SNUtil.writeToChat("Check pipe: "+oxy+" > "+(oxy != null ? oxy.supplier+"" : "null"));
			if (oxy != null && oxy.supplier != null && DayNightCycle.main.timePassedAsFloat - oxy.lastVerify < 5) {
				this.refillFrom(oxy.supplier, Time.deltaTime);
			}
		}

		public void refillFrom(RebreatherRechargerLogic lgc, float seconds) {
			if (this.hasLiquidBreathing()) {
				float add = lgc.consume(this.getAvailableFuelSpace(), seconds);
				float added = this.rechargePlayerLiquidBreathingFuel(add);
				lgc.refund(add - added); //if somehow added less than space, refund it
			}
		}

		public static GameObject getO2Label(uGUI_OxygenBar gui) {
			return gui.gameObject.getChildObject("OxygenTextLabel");
		}

		public void updateOxygenGUI(uGUI_OxygenBar gui) {
			uGUI_CircularBar bar = gui.bar;
			Text t = getO2Label(gui).GetComponent<Text>();
			Text tn = gui.gameObject.getChildObject("OxygenTextValue").GetComponent<Text>();
			if (baseO2BarTexture == null) {
				baseO2BarTexture = bar.texture;
				baseO2BarColor = bar.borderColor;
				baseO2BubbleTexture = bar.overlay;
				baseOverlayAlpha1 = gui.overlay1Alpha;
				baseOverlayAlpha2 = gui.overlay2Alpha;
				baseLabel = t.text; //O<size=30>2</size>
									//RenderUtil.dumpTexture("o2bar_core", baseO2BarTexture);
									//RenderUtil.dumpTexture("o2bar_bubble", baseO2BubbleTexture);
			}

			bool pink = this.hasLiquidBreathing();

			bar.edgeWidth = pink ? 0.25F : 0.2F;
			bar.borderWidth = pink ? 0.1F : 0.2F;
			bar.borderColor = pink ? new Color(1, 0.6F, 0.82F) : baseO2BarColor;
			bar.texture = pink ? TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/HUD/o2bar_liquid") : baseO2BarTexture;
			bar.overlay = pink ? TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/HUD/o2bar_liquid_bubble") : baseO2BubbleTexture;
			bar.overlay1Alpha = pink ? Math.Min(1, baseOverlayAlpha1 * 2) : baseOverlayAlpha1;
			bar.overlay2Alpha = pink ? Math.Min(1, baseOverlayAlpha2 * 2) : baseOverlayAlpha2;
			t.text = pink ? customHUDText /*"O<size=30>2</size><size=20>(aq)</size>"*/ : baseLabel;
			bool inactive = !this.isLiquidBreathingActive(Player.main);
			Color tc = Color.white;
			if (pink) {
				if (inactive) {
					tn.text = "-";
					tc = Color.gray;
				}
				else {
					tc = SNUtil.isPlayerCured()
						? Color.green
						: this.isKharaaTreatmentActive()
							? kharaaTreatmentRemainingTime > 0 ? Color.white : Color.yellow
							: Color.Lerp(Color.red, Color.yellow, 0.5F);
				}
			}
			tn.color = tc;
			bar.color = Color.white;

			float time = DayNightCycle.main.timePassedAsFloat;
			rechargingTintStrength = time - lastRechargeRebreatherTime <= 0.5
				? Math.Min(1, (rechargingTintStrength * 1.01F) + 0.025F)
				: Math.Max(0, (rechargingTintStrength * 0.992F) - 0.0125F);
			if (pink && rechargingTintStrength > 0) {
				float f = 1 - (0.33F * (0.5F + (rechargingTintStrength * 0.5F)));
				bar.color = new Color(f, f, 1);
			}
		}

		public bool isO2BarFlashingRed() {
			return Player.main.GetDepth() >= 400 && EnvironmentalDamageSystem.instance.isPlayerInOcean() && Inventory.main.equipment.GetTechTypeInSlot("Head") != C2CItems.rebreatherV2.TechType;
		}

		public void applyToBasePipes(RebreatherRechargerLogic machine, Transform seabase) {
			foreach (Transform child in seabase) {
				IPipeConnection root = child.gameObject.GetComponent<IPipeConnection>();
				if (root != null) {
					for (int i = 0; i < OxygenPipe.pipes.Count; i++) {
						OxygenPipe p = OxygenPipe.pipes[i];
						if (p && p.oxygenProvider != null && p.GetRoot() == root && p.oxygenProvider.activeInHierarchy) {
							OxygenAreaWithLiquidSupport oxy = p.oxygenProvider.EnsureComponent<OxygenAreaWithLiquidSupport>();
							oxy.supplier = machine;
							oxy.lastVerify = DayNightCycle.main.timePassedAsFloat;
							//SNUtil.writeToChat("Enable oxy area @ "+oxy.lastVerify);
						}
					}
				}
			}
		}

		internal bool useKharaaTreatment() {
			float dur = this.getTreatmentDuration();
			if (kharaaTreatmentRemainingTime > Mathf.Max(dur * 0.01F, 60))
				return false;
			kharaaTreatmentRemainingTime = dur;
			return true;
		}

		internal bool applyTemporaryKharaaTreatment() {
			if (hasTemporaryKharaaTreatment || !this.hasLiquidBreathing() || !this.isInPoweredArea(Player.main))
				return false;
			hasTemporaryKharaaTreatment = true;
			return true;
		}

		public float getTreatmentDuration() {
			return SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 2400 : 7200;
		}

		private bool isKharaaTreatmentActive() {
			return kharaaTreatmentRemainingTime > 0 || hasTemporaryKharaaTreatment;
		}

		internal void tickLiquidBreathing(bool has, bool active) {
			Player ep = Player.main;
			if (!ep || !DIHooks.isWorldLoaded())
				return;
			if (!meters) {
				uGUI_OxygenBar o2bar = UnityEngine.Object.FindObjectOfType<uGUI_OxygenBar>();
				if (o2bar) {
					GameObject go = new GameObject("LiquidBreathingMeters");
					meters = go.EnsureComponent<LiquidBreathingHUDMeters>();
					go.transform.SetParent(o2bar.transform, false);
					go.transform.localPosition = Vector3.zero;
				}
			}
			if (meters) {
				meters.gameObject.SetActive(has);
				if (has) {
					Battery b = Inventory.main.equipment.GetItemInSlot("Tank").item.GetComponent<Battery>();
					meters.primaryTankBar.currentTime = b.charge;
					meters.primaryTankBar.currentFillLevel = b.charge / b.capacity;
					meters.treatmentBar.currentTime = hasTemporaryKharaaTreatment ? -1 : kharaaTreatmentRemainingTime;
					meters.treatmentBar.currentFillLevel = hasTemporaryKharaaTreatment ? 1 : kharaaTreatmentRemainingTime / this.getTreatmentDuration();
				}
			}
			if (has && active) {
				if (startedUsingTemporaryKharaaTreatment && (this.isInPoweredArea(ep) || !this.hasLiquidBreathing())) {
					this.clearTempTreatment();
				}
				if (hasTemporaryKharaaTreatment && !startedUsingTemporaryKharaaTreatment && !this.isInPoweredArea(ep) && this.hasLiquidBreathing()) {
					startedUsingTemporaryKharaaTreatment = true;
					SNUtil.writeToChat("Kharaa treatment engaged");
				}
				if (ep.infectedMixin.IsInfected() && kharaaTreatmentRemainingTime > 0) {
					kharaaTreatmentRemainingTime = Mathf.Max(0, kharaaTreatmentRemainingTime - Time.deltaTime);
				}
			}
			else if (!has || startedUsingTemporaryKharaaTreatment) {
				this.clearTempTreatment();
			}
		}

		public void clearTempTreatment() {
			if (hasTemporaryKharaaTreatment) {
				hasTemporaryKharaaTreatment = false;
				startedUsingTemporaryKharaaTreatment = false;
				SNUtil.writeToChat("Weak kharaa treatment cleared");
			}
		}

		public bool hasReducedCapacity() {
			return !this.isKharaaTreatmentActive() && !SNUtil.isPlayerCured() && this.hasLiquidBreathing();
		}

		class OxygenAreaWithLiquidSupport : MonoBehaviour {

			internal RebreatherRechargerLogic supplier;
			internal float lastVerify;

		}

		public class LiquidBreathingHUDMeters : MonoBehaviour {

			internal LiquidBreathingHUDMeterUnit primaryTankBar;
			internal LiquidBreathingHUDMeterUnit treatmentBar;

			void Update() {
				if (!primaryTankBar) {
					primaryTankBar = this.createBar("Primary");
					primaryTankBar.leftSide = true;
				}
				if (!treatmentBar) {
					treatmentBar = this.createBar("Treatment");
				}
				treatmentBar.gameObject.SetActive(!SNUtil.isPlayerCured());
			}

			private LiquidBreathingHUDMeterUnit createBar(string name) {
				GameObject go = new GameObject(name);
				go.transform.SetParent(transform, false);
				go.layer = gameObject.layer;
				return go.EnsureComponent<LiquidBreathingHUDMeterUnit>();
			}

		}

		public class LiquidBreathingHUDMeterUnit : MonoBehaviour {

			internal GameObject fillBar;
			internal Image background;
			internal Image foreground;

			internal Text timer;

			internal bool leftSide;

			internal float currentFillLevel = 0;
			internal float currentTime = 0;

			public float overrideValue = -1;
			public float overrideTime = -1;

			public Color currentColor;

			void rebuild() {
				TextureManager.refresh();
				foreground.sprite = Sprite.Create(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/LiquidBreathingFuelBar"), new Rect(0, 0, 128, 128), Vector2.zero);
			}

			void Update() {
				if (!fillBar) {
					fillBar = new GameObject("Bar");
					fillBar.transform.SetParent(transform, false);
					fillBar.layer = gameObject.layer;
					fillBar.transform.localPosition = Vector3.zero;//new Vector3(0.55F, -0.55F, 0);
					fillBar.transform.localScale = Vector3.one * 2.5F;
					GameObject go = new GameObject("Background");
					go.transform.SetParent(fillBar.transform, false);
					go.layer = gameObject.layer;
					background = go.AddComponent<Image>();
					background.sprite = Sprite.Create(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/LiquidBreathingFuelBarBack"), new Rect(0, 0, 128, 128), Vector2.zero);
					background.rectTransform.offsetMin = new Vector2(-32f, -32f);
					background.rectTransform.offsetMax = new Vector2(32f, 32f);
					GameObject go2 = new GameObject("Foreground");
					go2.transform.SetParent(fillBar.transform, false);
					go2.layer = gameObject.layer;
					foreground = go2.AddComponent<Image>();
					foreground.sprite = Sprite.Create(TextureManager.getTexture(SeaToSeaMod.modDLL, "Textures/LiquidBreathingFuelBar"), new Rect(0, 0, 128, 128), Vector2.zero);
					foreground.rectTransform.offsetMin = new Vector2(-32f, -32f);
					foreground.rectTransform.offsetMax = new Vector2(32f, 32f);
					foreground.type = Image.Type.Filled;
					foreground.fillMethod = Image.FillMethod.Radial360;
					foreground.fillClockwise = true;
					foreground.fillOrigin = (int)Image.Origin360.Bottom;
					foreground.transform.rotation = Quaternion.identity;
					fillBar.SetActive(true);
				}
				if (!timer) {
					GameObject lbl = UnityEngine.Object.Instantiate(LiquidBreathingSystem.getO2Label(gameObject.FindAncestor<uGUI_OxygenBar>())).setName("SideBarText");
					timer = lbl.GetComponent<Text>();
					timer.transform.SetParent(transform, false);
					timer.transform.localPosition = Vector3.zero;
					timer.transform.rotation = Quaternion.identity;
					timer.alignment = leftSide ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
					timer.resizeTextForBestFit = false;
					timer.fontSize = 30;
				}

				if (leftSide) {
					fillBar.transform.localRotation = Quaternion.Euler(0, 0, 202.5F);
					foreground.transform.localRotation = Quaternion.Euler(0, 0, 157.5F);
					if (timer)
						timer.transform.localPosition = new Vector3(-60, 64, 0);
				}
				else {
					fillBar.transform.localRotation = Quaternion.Euler(0, 0, 22.5F);
					foreground.transform.localRotation = Quaternion.Euler(0, 0, -22.5F);
					foreground.fillClockwise = false;
					if (timer)
						timer.transform.localPosition = new Vector3(60, 64, 0);
				}

				float f = (currentFillLevel * 0.395F) + 0.0625F; //this fraction is because it does not fill the bar, slightly more than 3/8 to use up texture
				if (overrideValue >= 0)
					f = overrideValue;
				foreground.fillAmount = f;
				currentColor = currentFillLevel < 0.5F ? new Color(1, currentFillLevel * 2, 0, 1) : new Color(1 - ((currentFillLevel - 0.5F) * 2), 1, 0, 1);
				if (currentFillLevel < 0.1) { //make flash if very low
					float ff = (1+Mathf.Sin(DayNightCycle.main.timePassedAsFloat*Mathf.Deg2Rad*(float)MathUtil.linterpolate(currentFillLevel, 0.1, 0, 240, 4800, true)))*0.5F;
					currentColor = Color.Lerp(currentColor, Color.white, ff);
				}
				foreground.color = currentColor;

				if (timer) {
					f = currentTime;
					if (overrideTime >= 0)
						f = overrideTime;
					if (f >= 0) {
						TimeSpan ts = TimeSpan.FromSeconds(f);
						timer.text = f >= 3600 ? ts.ToString(@"hh\:mm\:ss") : ts.ToString(@"mm\:ss");
					}
					else {
						timer.text = "N/A";
					}
				}
			}

		}

	}

}
