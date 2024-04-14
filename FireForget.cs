/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Utility;
using GHPC;
using GHPC.Vehicle;
using GHPC.Weapons;
using UnityEngine;
using HarmonyLib;
using MelonLoader;
using static UnityEngine.GraphicsBuffer;
using GHPC.Camera;
using UnityEngine.UI;

namespace PactIncreasedLethality
{
    public class FireForget
    {
        private static HashSet<string> ff_ammo = new HashSet<string>();

        public class FireForgetTracker : MonoBehaviour
        {
            public class MGUHelper : MonoBehaviour
            {
                public FireControlSystem fcs;
            }

            private CameraManager camera_manager;
            public FireControlSystem fcs;
            public MissileGuidanceUnit mgu;
            public Vehicle target = null;
            private Vehicle current_target = null;
            private float TIME_TO_LOCK = 3f;
            private float current_lock_time = 3f;
            private float cd = 0;
            public bool fired = false;

            void Awake()
            {
                camera_manager = GameObject.Find("_APP_GHPC_").GetComponent<CameraManager>();
                fcs = GetComponent<FireControlSystem>();
                mgu = fcs.CurrentWeaponSystem.MissileGuidance;
                MGUHelper mgu_helper = mgu.gameObject.AddComponent<MGUHelper>();
                mgu_helper.fcs = fcs;
            }

            public void ResetGuidance()
            {
                current_lock_time = TIME_TO_LOCK;
                mgu.transform.localPosition = new Vector3(-1.1509f, 0.5546f, 0.0471f);
                mgu.transform.localEulerAngles = new Vector3(0.1569f, 359.86f, 0f);
                mgu.AimElement = fcs.AimTransform;
                target = null;
                current_target = null;
                fcs.GetComponent<BLYAT>().square.SetActive(false);
            }

            public void SetTarget(Vehicle _target)
            {
                mgu.AimElement = mgu.gameObject.transform;
                fcs.GetComponent<BLYAT>().square.SetActive(true);
                fcs.GetComponent<BLYAT>().square.GetComponent<Image>().color = Color.red;
                target = _target;
            }

            void DoLockOn()
            {
                if (fired) return;

                RaycastHit raycast_hit;
                int mask = ConstantsAndInfoManager.Instance.LaserRangefinderLayerMask.value;
                if
                (
                    !Physics.Raycast(fcs.LaserOrigin.position, fcs.LaserOrigin.forward, out raycast_hit, fcs.MaxLaserRange, mask)
                    || raycast_hit.transform.gameObject.GetComponent<IArmor>() == null
                    || raycast_hit.transform.gameObject.GetComponent<IArmor>().Unit == null
                )
                {
                    ResetGuidance();
                    return;
                }

                Vehicle _target = (Vehicle)raycast_hit.transform.gameObject.GetComponent<IArmor>().Unit;

                if (current_lock_time > 0f)
                {
                    current_lock_time -= Time.deltaTime;
                    current_target = _target;
                }
                else
                {
                    SetTarget(_target);
                }

                if (_target != null && current_target != null && current_target.GetInstanceID() != _target.GetInstanceID())
                {
                    ResetGuidance();
                }
            }

            void Update()
            {
                DoLockOn();

                if (current_target == null) return;

                Vector3 s = camera_manager.CameraFollow.BufferedCamera.WorldToScreenPoint(current_target.Center.position);
                fcs.GetComponent<BLYAT>().square.transform.position = new Vector3((int)s.x, (int)s.y, 1f);
                fcs.GetComponent<BLYAT>().square.SetActive(true);

                if (target == null) return;

                Vector3 loc = target.transform.position;
                loc.y = target.transform.position.y + 120f;
                mgu.transform.position = loc;
                mgu.transform.LookAt(target.transform);
            }
        }

        public static void AddFireForgetAmmo(AmmoType ammo_type)
        {
            if (!ff_ammo.Contains(ammo_type.Name))
                ff_ammo.Add(ammo_type.Name);
        }

        [HarmonyPatch(typeof(GHPC.Weapons.FireControlSystem), "DoLase")]
        public static class LockTarget
        {
            private static void Postfix(GHPC.Weapons.FireControlSystem __instance)
            {
                return;

                if (!ff_ammo.Contains(__instance.CurrentAmmoType.Name)) return;

                FireForgetTracker ff_tracker = __instance.GetComponent<FireForgetTracker>();

                int layerMask = 1 << CodeUtils.LAYER_MASK_VISIBILITYONLY;
                Vector3 laser_origin = __instance.LaserOrigin.position;
                Vector3 forward = __instance.LaserOrigin.forward;
                float max_lase_range = __instance.MaxLaserRange;

                RaycastHit raycastHit;
                if (Physics.Raycast(laser_origin, forward, out raycastHit, max_lase_range, layerMask) && raycastHit.collider.tag == "Smoke")
                {
                    ff_tracker.ResetGuidance();
                    return;
                }

                if (
                    Physics.Raycast(laser_origin, forward, out raycastHit, max_lase_range, ConstantsAndInfoManager.Instance.LaserRangefinderLayerMask.value)
                    && raycastHit.transform.gameObject != null
                    && raycastHit.transform.gameObject.GetComponent<IArmor>() != null
                    && raycastHit.transform.gameObject.GetComponent<IArmor>().Unit != null
                )
                {
                    ff_tracker.SetTarget((Vehicle)raycastHit.transform.gameObject.GetComponent<IArmor>().Unit);
                    return;
                }

                ff_tracker.ResetGuidance();
            }
        }

        [HarmonyPatch(typeof(GHPC.Weapons.MissileGuidanceUnit), "OnGuidanceStarted")]
        public static class Fired
        {
            private static void Prefix(GHPC.Weapons.MissileGuidanceUnit __instance)
            {
                if (__instance.gameObject.GetComponent<FireForgetTracker.MGUHelper>() == null) return;

                __instance.gameObject.GetComponent<FireForgetTracker.MGUHelper>().fcs.GetComponent<FireForgetTracker>().fired = true;
            }
        }

        [HarmonyPatch(typeof(GHPC.Weapons.MissileGuidanceUnit), "OnGuidanceStopped")]
        public static class ResetTargetGuidanceStopped
        {
            private static void Postfix(GHPC.Weapons.MissileGuidanceUnit __instance)
            {
                if (__instance.gameObject.GetComponent<FireForgetTracker.MGUHelper>() == null) return;

                __instance.gameObject.GetComponent<FireForgetTracker.MGUHelper>().fcs.GetComponent<FireForgetTracker>().ResetGuidance();
                __instance.gameObject.GetComponent<FireForgetTracker.MGUHelper>().fcs.GetComponent<FireForgetTracker>().fired = false;
            }
        }

        [HarmonyPatch(typeof(GHPC.Weapons.MissileGuidanceUnit), "StopGuidance")]
        public static class KeepTracking
        {
            private static bool Prefix(GHPC.Weapons.MissileGuidanceUnit __instance)
            {
                if (__instance.CurrentMissiles.Count > 0 && ff_ammo.Contains(__instance.CurrentMissiles[0].ShotInfo.TypeInfo.Name))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
*/