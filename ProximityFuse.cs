using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace PactIncreasedLethality
{
    public class ProximityFuse : MonoBehaviour
    {
        private GHPC.Weapons.LiveRound live_round;
        private static GameObject prox_fuse;
        private static HashSet<string> prox_ammos = new HashSet<string>();
        private float radius, forward_distance; 
        private bool detonated = false;

        // must be called at least once 
        public static void Init() {
            if (!prox_fuse)
            {
                prox_fuse = new GameObject("prox fuse");
                prox_fuse.layer = 8;
                prox_fuse.SetActive(false);
                prox_fuse.AddComponent<ProximityFuse>();
                prox_fuse.AddComponent<MeshFilter>();
                prox_fuse.AddComponent<MeshRenderer>();
            }
        }

        public static void AddProximityFuse(AmmoType ammo_type)
        {
            if (!prox_ammos.Contains(ammo_type.Name))
            {
                prox_ammos.Add(ammo_type.Name);
            }
        }

        void Detonate()
        {
            if (!detonated) {
                live_round._rangedFuseActive = true;
                live_round._rangedFuseCountdown = 0f;
                detonated = true;
            }
        }

        void Update()
        {
            if (!live_round) return;

            RaycastHit hit;
            Vector3 pos = live_round.transform.position;
            
            if (Physics.SphereCast(pos, 3.5f, live_round.transform.forward, out hit, 0.1f, 1 << 8))
            {
                if (hit.collider.CompareTag("Penetrable"))
                    Detonate();
            }
        }
        
        [HarmonyPatch(typeof(GHPC.Weapons.LiveRound), "Start")]
        public static class SpawnProximityFuse
        {
            private static void Prefix(GHPC.Weapons.LiveRound __instance)
            {
                if (prox_ammos.Contains(__instance.Info.Name) && __instance.gameObject.transform.Find("prox fuse(Clone)") == null)
                {
                    GameObject p = GameObject.Instantiate(prox_fuse, __instance.transform);
                    p.GetComponent<ProximityFuse>().live_round = __instance;
                    p.SetActive(true);
                }
                else if (__instance.gameObject.transform.Find("prox fuse(Clone)")) {
                    GameObject.DestroyImmediate(__instance.gameObject.transform.Find("prox fuse(Clone)").gameObject);
                }
            }
        }
        
    }
}
