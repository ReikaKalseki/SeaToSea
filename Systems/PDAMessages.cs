using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Handlers;

using Story;

namespace ReikaKalseki.SeaToSea {
	public static class PDAMessages {

		public static void addAll() {
			foreach (Messages m in Enum.GetValues(typeof(Messages))) {
				Message attr = getAttr(m);
				XMLLocale.LocaleEntry e = SeaToSeaMod.miscLocale.getEntry(attr.key);
				PDAMessagePrompts.instance.addPDAMessage(e);
			}
		}

		public enum Messages {
			//[Message("voidspikeenter")]VoidSpike,
			[Message("aurorafire")]AuroraFireWarn,
			[Message("aurorafire_norad")]AuroraFireWarn_NoRad,
			//[Message("auroracut")]AuroraSalvage,
			[Message("kelpcavedrone")]KelpCavePrompt,
			[Message("kelpcavedronelate")]KelpCavePromptLate,
			[Message("redgrasscave")]RedGrassCavePrompt,
			[Message("kooshcave")]KooshCavePrompt,
			[Message("treaderpoo")]TreaderPooPrompt,
			//[Message("obsidian")]ObsidianPrompt,
			[Message("dunearch")]DuneArchPrompt,
			[Message("followradio")]FollowRadioPrompt,
			[Message("underislandgeyserminerals")]UnderwaterIslandsPrompt,
			[Message("sanctuaryprompt")]SanctuaryPrompt,
			[Message("hiddenseamothprompt")]JellySeamothDepthPrompt,
			[Message("bkelpnestprompt")]BloodKelpNestPrompt,
			[Message("trailerbase")]TrailerBasePrompt,
			[Message("meteorprompt")]MeteorPrompt,
			[Message("needlaunchcargo")]NeedLaunchCargoMessage,
			[Message("needscaneverything")]NeedScansMessage,
			[Message("needalldata")]NeedDataMessage,
			[Message("notseenallbiomes")]NotSeenBiomesMessage,
			[Message("unfinishedexplore")]NeedFinishExploreTrackerMessage,
			[Message("liqbrselfscaneasy")]LiquidBreathingSelfScanEasy,
			[Message("liqbrselfscanhard")]LiquidBreathingSelfScanHard,
		}

		public static Message getAttr(Messages key) {
			FieldInfo info = typeof(Messages).GetField(Enum.GetName(typeof(Messages), key));
			return (Message)Attribute.GetCustomAttribute(info, typeof(Message));
		}

		public class Message : Attribute {

			public readonly string key;

			public Message(string s) {
				key = s;
			}
		}
	}
}
