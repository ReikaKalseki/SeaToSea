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

namespace ReikaKalseki.SeaToSea
{
		internal class C2CLifepod : MonoBehaviour {
		
			private Simplex1DGenerator pathVariation;
		
			private Rigidbody body;
			private Stabilizer stabilizer;
			private WorldForces forces;
			private LiveMixin live;
		
			private Vector3 rotationSpeed;
			private float sinkRate = 0;
		
			private static readonly float MAX_ROTATE_SPEED = 2F;
			
			void FixedUpdate() {
				if (!body)
					body = GetComponent<Rigidbody>();
				if (!stabilizer)
					stabilizer = GetComponent<Stabilizer>();
				if (!forces)
					forces = GetComponent<WorldForces>();
				if (!live)
					live = GetComponent<LiveMixin>();
				if (body && SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
					body.constraints = RigidbodyConstraints.None;
					body.drag = 0;
					body.angularDrag = 0;
					float sp = getMovementSpeed();
					float dT = Time.fixedDeltaTime;
					if (pathVariation == null)
						getOrCreateNoise();
					if (sp > 0) {
						stabilizer.enabled = false;
						forces.enabled = false;
						float sp2 = sp < 1 ? sp*sp : sp;
						Vector3 force = 0.2F*new Vector3(-1F, 0, 1.2F*(float)pathVariation.getValue(new Vector3(DayNightCycle.main.timePassedAsFloat, 0, 0)))*sp2;
						//SNUtil.writeToChat(sp.ToString("0.000")+">"+force.ToString("F4"));
						body.velocity = force;
						
						live.TakeDamage(2*sp*dT, transform.position, DamageType.Normal);
						
						if (transform.position.y < -5) {
							body.transform.Rotate(rotationSpeed*dT, Space.Self);
							rotationSpeed += new Vector3(UnityEngine.Random.Range(-0.15F, 0.15F), UnityEngine.Random.Range(-0.15F, 0.15F), UnityEngine.Random.Range(-0.15F, 0.15F));
							rotationSpeed.x = Mathf.Clamp(rotationSpeed.x, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
							rotationSpeed.y = Mathf.Clamp(rotationSpeed.y, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
							rotationSpeed.z = Mathf.Clamp(rotationSpeed.z, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
						}
					}
					float depth = -transform.position.y;
					float tgt = getTargetDepth();
					SNUtil.writeToChat(depth.ToString("000.0")+"/"+tgt.ToString("000.0"));
					if (tgt > depth) {
						body.velocity = body.velocity.setY(-0.25F);
						SNUtil.writeToChat(body.velocity.ToString("F4"));
					}
				}
			}
		
			private void getOrCreateNoise() {
				long use = SaveLoadManager.main.firstStart;
				if (pathVariation == null || pathVariation.seed != use) {
					pathVariation = (Simplex1DGenerator)new Simplex1DGenerator(use).setFrequency(0.02F);
				}
			}
			
			private float getPassedDays() {
				float time = DayNightCycle.main.timePassedAsFloat-0.4F;
				//SNUtil.writeToChat((time*10/DayNightCycle.kDayLengthSeconds).ToString("00.00"));
				return time*10/DayNightCycle.kDayLengthSeconds;
			}
			
			private float getMovementSpeed() {
				float days = getPassedDays();
				if (days < 3)
					return 0;
				else if (days < 8)
					return 1;
				else if (days < 20)
					return 1+1.5F*(days-8)/12F;
				else
					return Mathf.Min(5F, 2.5F+(days-20)*0.5F);
			}
			
			private float getTargetDepth() {
				BiomeBase biome = BiomeBase.getBiome(WaterBiomeManager.main.GetBiome(transform.position.setY(Mathf.Min(-3, transform.position.y-10)), false));
				float days = getPassedDays();
				if (biome == VanillaBiomes.SHALLOWS)
					return (float)MathUtil.linterpolate(days, 3, 20, -2, 8, true);
				if (biome == VanillaBiomes.KELP)
					return 25;
				if (biome == VanillaBiomes.REDGRASS || biome == VanillaBiomes.BLOODKELP)
					return 50;
				if (biome == VanillaBiomes.MUSHROOM)
					return 100;
				if (biome == VanillaBiomes.DUNES || biome == VanillaBiomes.SPARSE || biome == VanillaBiomes.GRANDREEF)
					return 200;
				if (biome == VanillaBiomes.VOID)
					return 9999;
				return -transform.position.y;
			}
			
		}
			
		enum PodPhases {
			STABLE,
			DRIFTING,
			SINKING,
			UNRECOVERABLE,
		}
}
