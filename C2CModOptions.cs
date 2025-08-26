using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Options;

using UnityEngine;

namespace ReikaKalseki.SeaToSea {

	internal class C2CModOptions : ModOptions {

		private readonly Dictionary<string, Keybind> bindings = new Dictionary<string, Keybind>();

		public static readonly string PROPGUNSWAP = "PropGunSwap";

		public C2CModOptions() : base(SeaToSeaMod.MOD_KEY.from('.')) {
			KeybindChanged += (s, e) => { bindings[e.Id].selectedKey = e.Key; };
		}

		public override void BuildModOptions() {
			this.addBinding(PROPGUNSWAP, "(Pro/Re)pulsion Gun Swap", KeyCode.PageUp);
		}

		private void addBinding(string id, string name, KeyCode def) {
			this.AddKeybindOption(id, name, GameInput.Device.Keyboard, def);
			bindings[id] = new Keybind(id, def);
		}

		public KeyCode getBinding(string id) {
			return bindings.ContainsKey(id) ? bindings[id].selectedKey : KeyCode.None;
		}

		private class Keybind {

			public readonly string optionID;

			public KeyCode selectedKey;

			internal Keybind(string s, KeyCode def) {
				optionID = s;
				selectedKey = def;
			}

		}
	}
}
