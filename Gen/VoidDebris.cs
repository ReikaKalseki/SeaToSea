using System;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public sealed class VoidDebris : WorldGenerator { //TODO add pieces like medkit box; scatter paper submerged below
		
		private static readonly WeightedRandom<string> debrisProps = new WeightedRandom<string>();
		
		static VoidDebris() {
			debrisProps.addEntry("08a95141-7c00-4d55-b582-306fa2e217ed", 100);
			debrisProps.addEntry("0c65ee6e-a84a-4989-a846-19eb53c13071", 100);
			debrisProps.addEntry("0d798a35-29e8-4ddb-b1be-9d760d3a9eb6", 30);
			debrisProps.addEntry("1235093d-3e84-4e98-9823-602db2e8fa5f", 100);
			debrisProps.addEntry("1c147fcd-f727-4404-b10e-a1f03363e5bf", 100);
			debrisProps.addEntry("2f56b14c-d84c-407e-ad84-eab2df2fc09b", 50);
			debrisProps.addEntry("314e696f-67bc-4d6c-8ce5-cf9ed7f34746", 100);
			debrisProps.addEntry("3981a55f-0754-466a-8932-6e245b4ef846", 20);
			debrisProps.addEntry("4322ded1-04ba-44eb-afe5-44b9c4112c64", 80);
			debrisProps.addEntry("4e8f6009-fc9c-4774-9ddc-27a6b0081dde", 200); //hull panel
			/*
			debrisProps.addEntry("", 100);
			debrisProps.addEntry("", 100);
			debrisProps.addEntry("", 100);
			debrisProps.addEntry("", 100);
			debrisProps.addEntry("", 100);
			debrisProps.addEntry("", 100);
			debrisProps.addEntry("", 100);
			debrisProps.addEntry("", 100);*/
			
			//paper "32e48451-8e81-428e-9011-baca82e9cd32"
			
			//bag 3616e7f3-5079-443d-85b4-9ad68fcbd924
			
			//big platform 5a6279e2-fab9-48c9-bcb3-fdeb02fd4ce2
		}
		
		public VoidDebris(Vector3 pos) : base(pos) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void generate(List<GameObject> li) {		
			for (int i = 0; i < 8; i++) {
				GameObject go = SBUtil.createWorldObject(debrisProps.getRandomEntry());
				go.transform.position = MathUtil.getRandomVectorAround(position, new Vector3(6, 0, 6));
				VoidSpikesBiome.checkAndAddWaveBob(go.GetComponent<LargeWorldEntity>(), true);
				li.Add(go);
			}
		}
	}
}
