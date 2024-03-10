using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Utility;
using GHPC;
using GHPC.Vehicle;
using UnityEngine;
using HarmonyLib;
using MelonLoader;
using GHPC.Weapons;

namespace PactIncreasedLethality
{
    public class LockOnLead : MonoBehaviour
    {
        public Vehicle target;
        public FireControlSystem fcs;
        private float cd = 0f;
        public bool engaged = false;

        void Awake() { 
            fcs = gameObject.GetComponent<FireControlSystem>();
        }

        public void Update()
        {
            if (fcs == null) return;

            cd -= Time.deltaTime;

            if (Input.GetKey(KeyCode.Mouse2) && cd <= 0f)
            {
                cd = 0.2f;

                if (!engaged)
                {
                    engaged = true;
                    //fcs._autoDumpViaPalmSwitches = false;
                    Util.GetDayOptic(fcs).RotateAzimuth = true;
                    fcs.DoLase();
                }
                else
                {
                    target = null;
                    engaged = false;
                    //fcs._autoDumpViaPalmSwitches = true;
                    Util.GetDayOptic(fcs).RotateAzimuth = false;
                    fcs.DumpLead();
                }
            }

            if (target == null)
            {
                Util.GetDayOptic(fcs).RotateAzimuth = false;
                //fcs._autoDumpViaPalmSwitches = true;
                return;
            }

            fcs.FinalSetAimWorldPosition(target.Center.position);
        }
    }

    [HarmonyPatch(typeof(GHPC.Weapons.FireControlSystem), "DoLase")]
    public static class LockTarget
    {
        private static void Postfix(GHPC.Weapons.FireControlSystem __instance)
        {
            LockOnLead lead = __instance.GetComponent<LockOnLead>();

            if (lead == null) return;
            if (!lead.engaged) return;

            float num = -1f;
            int layerMask = 1 << CodeUtils.LAYER_MASK_VISIBILITYONLY;
            RaycastHit raycastHit;
            if (Physics.Raycast(__instance.LaserOrigin.position, __instance.LaserOrigin.forward, out raycastHit, __instance.MaxLaserRange, layerMask) && raycastHit.collider.tag == "Smoke")
            {
                return;
            }
            if (Physics.Raycast(__instance.LaserOrigin.position, __instance.LaserOrigin.forward, out raycastHit, __instance.MaxLaserRange, ConstantsAndInfoManager.Instance.LaserRangefinderLayerMask.value) && (raycastHit.distance < num || num == -1f))
            {
                num = raycastHit.distance;
            }

            GameObject raycast_hit = raycastHit.transform.gameObject;

            lead.target = (Vehicle)raycast_hit.GetComponent<IArmor>().Unit;

            return;
        }
    }
}