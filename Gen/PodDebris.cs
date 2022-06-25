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
	public sealed class PodDebris : WorldGenerator {
		
		internal static readonly WeightedRandom<Prop> debrisProps = new WeightedRandom<Prop>();
		internal static readonly List<Prop> alwaysPieces = new List<Prop>();
		internal static readonly List<Prop> papers = new List<Prop>();
		
		static PodDebris() {
			debrisProps.addEntry(new Prop("08a95141-7c00-4d55-b582-306fa2e217ed"), 100);
			debrisProps.addEntry(new Prop("0c65ee6e-a84a-4989-a846-19eb53c13071"), 100);
			debrisProps.addEntry(new Prop("0d798a35-29e8-4ddb-b1be-9d760d3a9eb6"), 30);
			debrisProps.addEntry(new Prop("1235093d-3e84-4e98-9823-602db2e8fa5f"), 100);
			debrisProps.addEntry(new Prop("1c147fcd-f727-4404-b10e-a1f03363e5bf"), 100);
			debrisProps.addEntry(new Prop("2f56b14c-d84c-407e-ad84-eab2df2fc09b"), 50);
			debrisProps.addEntry(new Prop("314e696f-67bc-4d6c-8ce5-cf9ed7f34746"), 100);
			debrisProps.addEntry(new Prop("3981a55f-0754-466a-8932-6e245b4ef846"), 20);
			debrisProps.addEntry(new Prop("4322ded1-04ba-44eb-afe5-44b9c4112c64"), 80);
			debrisProps.addEntry(new Prop("4e8f6009-fc9c-4774-9ddc-27a6b0081dde", -90, 90), 200); //hull panel
			debrisProps.addEntry(new Prop("f901b968-5b3c-4795-8ded-82db2fa23440", null), 30); //"power cyl"
			debrisProps.addEntry(new Prop("3616e7f3-5079-443d-85b4-9ad68fcbd924", null), 20); //bag
						
			alwaysPieces.Add(new Prop("c0175cf7-0b6a-4a1d-938f-dad0dbb6fa06", -90, 90)); //medkit fab
			alwaysPieces.Add(new Prop("4f045c69-1539-4c53-b157-767df47c1aa6", -90, 90)); //radio lookalike
			//alwaysPieces.Add(new Prop("cdade216-3d4d-4adf-901c-3a91fb3b88c4", -90, 90)); //centrifuge
			alwaysPieces.Add(new Prop("9f16d82b-11f4-4eeb-aedf-f2fa2bfca8e3", -90, 90)); //fab
			alwaysPieces.Add(new Prop("f901b968-5b3c-4795-8ded-82db2fa23440", null)); //"power cyl"
			
			papers.Add(new Prop("32e48451-8e81-428e-9011-baca82e9cd32", null));	
			papers.Add(new Prop("b4ec5044-5519-4743-b61b-92a8b6fe4a32", null));			
			
			//big platform 5a6279e2-fab9-48c9-bcb3-fdeb02fd4ce2
		}
		
		internal bool generateRecognizablePieces = false;
		internal int paperCount = 6;
		internal float debrisAmount = 1;
		internal float debrisScale = 0.5F;
		internal int scrapCount = 0;
		internal float areaSpread = 1;
		internal Vector3? bounds = null;
		
		internal float yBaseline;
		
		public PodDebris(Vector3 pos) : base(pos) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			paperCount = e.getInt("paperCount", paperCount);
			generateRecognizablePieces = e.getBoolean("generateRecognizablePieces");
			debrisAmount = (float)e.getFloat("debrisAmount", debrisAmount);
			debrisScale = (float)e.getFloat("debrisScale", debrisScale);
			scrapCount = e.getInt("scrapCount", scrapCount);
			yBaseline = (float)e.getFloat("yBaseline", double.NaN);
		}
		
		public override void saveToXML(XmlElement e) {
			e.addProperty("paperCount", paperCount);
			e.addProperty("generateRecognizablePieces", generateRecognizablePieces);
			e.addProperty("debrisAmount", debrisAmount);
			e.addProperty("debrisScale", debrisScale);
			e.addProperty("scrapCount", scrapCount);
			e.addProperty("yBaseline", yBaseline);
		}
		
		public override void generate(List<GameObject> li) {		
			for (int i = 0; i < 6*debrisAmount; i++) {
				li.Add(generateObjectInRange(9, 0.125F, 9));
			}
			if (generateRecognizablePieces) {
				foreach (Prop s in alwaysPieces) {
					li.Add(generateObjectInRange(15, 0, 15, 0, s.prefab.ClassID, false));
				}
			}
			for (int i = 0; i < 12*debrisAmount; i++) {
				li.Add(generateObjectInRange(24, 0.125F, 24));
			}
			for (int i = 0; i < paperCount; i++) {
				li.Add(generateObjectInRange(4, 2, 4, 2, papers[UnityEngine.Random.Range(0, papers.Count)].prefab.ClassID, false));
			}
			for (int i = 0; i < scrapCount; i++) {
				VanillaResources mtl = VanillaResources.SCRAP1;
				switch(UnityEngine.Random.Range(0, 4)) {
					case 0:
						mtl = VanillaResources.SCRAP1;
						break;
					case 1:
						mtl = VanillaResources.SCRAP2;
						break;
					case 2:
						mtl = VanillaResources.SCRAP3;
						break;
					case 3:
						mtl = VanillaResources.SCRAP4;
						break;
				}
				GameObject drop = generateObjectInRange(18, 0, 18, 0, mtl.prefab, false);
				li.Add(drop);
			}
		}
		
		private GameObject generateObjectInRange(float dx, float dy, float dz, float offsetY = 0, string pfb = null, bool scale = true) {
			if (pfb == null)
				pfb = debrisProps.getRandomEntry().prefab.ClassID;
			GameObject go = spawner(pfb);
			if (go == null)
				return go;
			Vector3 pos = MathUtil.getRandomVectorAround(position, new Vector3(dx, dy, dz)*areaSpread);
			if (bounds != null && bounds.HasValue) {
				pos.x = Math.Max(Math.Min(pos.x, position.x+bounds.Value.x), position.x-bounds.Value.x);
				pos.y = Math.Max(Math.Min(pos.y, position.y+bounds.Value.y), position.y-bounds.Value.y);
				pos.z = Math.Max(Math.Min(pos.z, position.z+bounds.Value.z), position.z-bounds.Value.z);
			}
			pos.y = yBaseline+offsetY+UnityEngine.Random.Range(-dy, dy);
			go.transform.position = pos;
			go.transform.rotation = UnityEngine.Random.rotationUniform;
			if (scale)
				go.transform.localScale = Vector3.one*debrisScale;
			return go;
		}
	}
		
	public class Prop {
		
		private static readonly Dictionary<string, Prop> propCache = new Dictionary<string, Prop>();
		
		public readonly PropPrefab prefab;
		internal readonly float[] baseAngles;
		internal readonly bool freeAngle;
		
		internal Prop(string pfb, float ang1, float ang2) : this(pfb, new float[]{ang1, ang2}) {
			
		}
		
		internal Prop(string pfb, float ang = 0) : this(pfb, new float[]{ang}) {
			
		}
		
		internal Prop(string pfb, float[] ang) {
			prefab = new PropPrefab(pfb, false).register();
			//prefabGravity = new PropPrefab(pfb, true).register();
			baseAngles = ang;
			freeAngle = baseAngles == null || baseAngles.Length == 0;
			propCache[pfb] = this;
		}
		
		public Prop getForPrefab(string pfb) {
			return propCache.ContainsKey(pfb) ? propCache[pfb] : null;
		}
		
	}
		
	public sealed class PropPrefab : GenUtil.CustomPrefabImpl {
		
		//private static readonly Dictionary<string, PropPrefab>[] prefabCache = new Dictionary<string, PropPrefab>[2];
		
		static PropPrefab() {
			//for (int i = 0; i < prefabCache.Length; i++)
			//	prefabCache[i] = new Dictionary<string, PropPrefab>();
		}
			
		private readonly bool useGravity;
	       
		internal PropPrefab(string template, bool g) : base("podprop_"+(g ? "g_" : "")+template, template) {
			useGravity = g;
	    }
	
		public override void prepareGameObject(GameObject go, Renderer r) {
			ObjectUtil.removeComponent<MedicalCabinet>(go);
			ObjectUtil.removeComponent<Fabricator>(go);
			ObjectUtil.removeComponent<Centrifuge>(go);
			ObjectUtil.removeComponent<Radio>(go);
			ObjectUtil.removeComponent<Constructable>(go);
			PreventDeconstruction prev = go.EnsureComponent<PreventDeconstruction>();
			prev.enabled = true;
			prev.inEscapePod = true;
			if (useGravity) {
				ObjectUtil.applyGravity(go);
			}
			else {
				Rigidbody b = go.GetComponentInChildren<Rigidbody>();
				//b.detectCollisions = false;
				if (b != null)
					b.constraints = RigidbodyConstraints.FreezeAll;
			}
			Pickupable p = go.GetComponentInChildren<Pickupable>();
			if (p != null) {
				TechType tt = CraftData.GetTechType(ObjectUtil.lookupPrefab(baseTemplate.prefab));
				SNUtil.log(ClassID+" had PP, TT = "+tt);
				if (tt != TechType.None)
					p.SetTechTypeOverride(tt);
			}
			//VoidSpikesBiome.checkAndAddWaveBob(go, true);
		}
		
		internal PropPrefab register() {
			Patch();
			//prefabCache[useGravity ? 1 : 0][baseTemplate.prefab] = this;
			return this;
		}
		/*
		public static PropPrefab getProp(bool grav, string id) {
			Dictionary<string, PropPrefab> dict = prefabCache[grav ? 1 : 0];
			return dict.ContainsKey(id) ? dict[id] : null;
		}*/
			
	}
}
