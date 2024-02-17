using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using GHPC.Weapons;
using HarmonyLib;
using MelonLoader;
using static MelonLoader.MelonLogger;

namespace PactIncreasedLethality
{
    public class EFP : MonoBehaviour
    {
        private GHPC.Weapons.LiveRound live_round;
        private AmmoType penetrator; 
        private static GameObject efp;
        private static HashSet<string> efp_ammos = new HashSet<string>();
        private static Dictionary<string, AmmoType> penetrators = new Dictionary<string, AmmoType>();
        private static Dictionary<string, bool> has_second_warhead = new Dictionary<string, bool>();
        private bool detonated = false;

        public static void Init()
        {
            if (!efp)
            {
                efp = new GameObject("efp sensor");
                efp.layer = 8;
                efp.SetActive(false);
                efp.AddComponent<EFP>();
                efp.AddComponent<MeshFilter>();
                efp.AddComponent<MeshRenderer>();
            }
        }

        /// <summary>
        /// penetrator name NEEDS to be the parent name + " EFP" 
        /// ex: "TOW-2B EFP"
        /// </summary>
        public static void AddEFP(AmmoType ammo_type, AmmoType penetrator, bool second_warhead=false)
        {
            if (!efp_ammos.Contains(ammo_type.Name))
                efp_ammos.Add(ammo_type.Name);
            
            if (!penetrators.ContainsKey(penetrator.Name)) {
                penetrators.Add(penetrator.Name, penetrator); 
                has_second_warhead.Add(penetrator.Name, second_warhead);
            }
        }

        public void Update()
        {
            if (!live_round) return;

            RaycastHit hit;
            Vector3 pos = live_round.transform.position;

            if (Physics.Raycast(pos, Vector3.down, out hit, 1.8f, 1 << 8))
            {
                if (hit.collider.CompareTag("Penetrable"))
                    Invoke("Detonate", 0.85f / (live_round.CurrentSpeed + hit.distance));
                
            }
        }

        private void Detonate()
        {
            if (!detonated && Vector3.Distance(live_round.transform.position, live_round._trueInitialPosition) >= live_round.Info.ArmingDistance)
            {
                live_round._rangedFuseActive = true;
                live_round._rangedFuseCountdown = 0f;
                detonated = true;
            }
        }

        [HarmonyPatch(typeof(GHPC.Weapons.LiveRound), "Start")]
        public static class SpawnEFPSensor
        {
            private static void Prefix(GHPC.Weapons.LiveRound __instance)
            {
                if (efp == null)  
                    MelonLogger.Msg("Could not find EFP sensor game object, make sure EFP.Init() has been called");
                

                if (efp_ammos.Contains(__instance.Info.Name) && __instance.gameObject.transform.Find("efp sensor(Clone)") == null)
                {
                    GameObject p = GameObject.Instantiate(efp, __instance.transform);
                    p.GetComponent<EFP>().live_round = __instance;
                    p.GetComponent<EFP>().penetrator = penetrators[__instance.Info.Name + " EFP"];
                    p.SetActive(true);
                }
                else if (__instance.gameObject.transform.Find("efp sensor(Clone)"))
                {
                    GameObject.DestroyImmediate(__instance.gameObject.transform.Find("efp sensor(Clone)").gameObject);
                }
            }
        }

        [HarmonyPatch(typeof(GHPC.Weapons.LiveRound), "createExplosion")]
        public static class DetonateEFP
        {
            private static void Prefix(GHPC.Weapons.LiveRound __instance)
            {
                if (efp_ammos.Contains(__instance.Info.Name))
                {
                    AmmoType penetrator = penetrators[__instance.Info.Name + " EFP"];

                    GHPC.Weapons.LiveRound component;
                    component = LiveRoundMarshaller.Instance.GetRoundOfVisualType(LiveRoundMarshaller.LiveRoundVisualType.Invisible)
                        .GetComponent<GHPC.Weapons.LiveRound>();

                    component.Info = penetrator;
                    component.CurrentSpeed = 1500f;
                    component.MaxSpeed = 1500f;
                    component.IsSpall = false;
                    component.Shooter = __instance.Shooter;
                    component.transform.position = __instance.transform.position;
                    component.transform.forward = Vector3.down;

                    component.Init(null, null);
                    component.name = "efp penetrator";

                    if (has_second_warhead[penetrator.Name])
                    {
                        GHPC.Weapons.LiveRound component2;
                        component2 = LiveRoundMarshaller.Instance.GetRoundOfVisualType(LiveRoundMarshaller.LiveRoundVisualType.Shell)
                            .GetComponent<GHPC.Weapons.LiveRound>();

                        component2.Info = penetrator;
                        component2.CurrentSpeed = 1500f;
                        component2.MaxSpeed = 1500f;
                        component2.IsSpall = false;
                        component2.Shooter = __instance.Shooter;
                        component2.transform.position = __instance.transform.position - __instance.transform.forward * 0.15f;
                        component2.transform.forward = Vector3.down;

                        component2.Init(null, null);
                        component2.name = "efp2 penetrator";
                    }
                }
            }
        }

    }
}
