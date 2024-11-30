using GHPC.Utility;
using GHPC;
using GHPC.Vehicle;
using UnityEngine;
using HarmonyLib;
using GHPC.Weapons;
using MelonLoader;
using MelonLoader.TinyJSON;
using static MelonLoader.MelonLogger;
using GHPC.Player;
using System;
using GHPC.Camera;

namespace PactIncreasedLethality
{
    public class LockOnLead : MonoBehaviour
    {
        public Vehicle target;
        public Renderer tracking_center;
        public FireControlSystem fcs;
        public T72.CustomGuidanceComputer guidance_computer;
        private float cd = 0f;
        public bool engaged = false;
        public Vector2 offset;
        static Vector2 monitor_dims = new Vector2(1024f, 576f);

        void LateUpdate() {
            if (!target) return;

            Transform tracking_object = target.gameObject.transform.Find("TRACKING OBJECT");

            if (tracking_object == null) return;

            Camera camera = CameraManager.MainCam;
            Bounds bounds = tracking_object.GetComponent<MeshRenderer>().bounds;
            Vector3[] ss_corners = new Vector3[8];
            ss_corners[0] = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));
            ss_corners[1] = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));
            ss_corners[2] = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));
            ss_corners[3] = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
            ss_corners[4] = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));
            ss_corners[5] = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));
            ss_corners[6] = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
            ss_corners[7] = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));

            float min_x = ss_corners[0].x;
            float min_y = ss_corners[0].y;
            float max_x = ss_corners[0].x;
            float max_y = ss_corners[0].y;

            for (int i = 1; i < 8; i++)
            {
                min_x = Mathf.Min(min_x, ss_corners[i].x);
                min_y = Mathf.Min(min_y, ss_corners[i].y);
                max_x = Mathf.Max(max_x, ss_corners[i].x);
                max_y = Mathf.Max(max_y, ss_corners[i].y);
            }

            RectTransform rt = Sosna.ThermalMonitor.tracking_gates.GetComponent<RectTransform>();
            rt.position = new Vector2(min_x, min_y) / monitor_dims * new Vector2(Screen.width, Screen.height);
            rt.sizeDelta = new Vector2(max_x - min_x, max_y - min_y) / monitor_dims * new Vector2(Screen.width, Screen.height) * new Vector2(768f / Screen.width, 465f / Screen.height);
        }

        void ResetTracking() {
            target = null;
            engaged = false;
            guidance_computer.autotrackingEnabled = false;
            offset = Vector2.zero;
        }

        void Update()
        {
            if (fcs == null) return;
            if (PlayerInput.Instance.CurrentPlayerWeapon.FCS != fcs) return;

            if (cd > 0f)
                cd -= Time.deltaTime;

            if (Input.GetKey(KeyCode.Mouse2) && cd <= 0f)
            {
                cd = 0.2f;

                if (!engaged && target != null)
                {
                    engaged = true;
                }
                else
                {
                    ResetTracking();
                }
            }

            Ray ray = new Ray();
            if (!target || (target && !engaged))
            {
                ray = new Ray(fcs.ReferenceTransform.position, fcs.AimWorldVector);
            }
            if (engaged) 
            {
                Vector3 forward = tracking_center.bounds.center - fcs.ReferenceTransform.position;
                ray = new Ray(fcs.ReferenceTransform.position, Matrix4x4.TRS(forward, Quaternion.LookRotation(forward), Vector3.one).MultiplyPoint3x4(offset));
            }

            RaycastHit raycastHit; 
            Physics.Raycast(ray, out raycastHit, 5000f, 1 << 14);

            if (raycastHit.transform != null)
            {
                GameObject raycast_hit = raycastHit.transform.gameObject;

                if (raycast_hit.gameObject != null)
                {
                    target = raycast_hit.GetComponentInParent<Vehicle>();
                    Transform tracking_object = target.gameObject.transform.Find("TRACKING OBJECT");
                    tracking_center = tracking_object.GetComponent<Renderer>();
                }
                else if (!engaged)
                {
                    ResetTracking();
                }
            }
            else if (!engaged)
            {
                ResetTracking();
            }

            if (engaged && target)
            {

                guidance_computer.autotrackingEnabled = true;

                // adapted from GHPC.AI.BehaviorTrees.ActionBaseLookAt.GetAimPositionAtTarget
                MissileGuidanceUnit mgu = fcs.CurrentWeaponSystem.GuidanceUnit;
                AmmoType current_ammo = fcs.CurrentAmmoType;
                BallisticComputerRepository computer = BallisticComputerRepository.Instance;
                bool missile_active = current_ammo.Guidance > AmmoType.GuidanceType.Unguided && mgu.CurrentMissiles.Count > 0;

                Vector3 position = missile_active ? mgu.CurrentMissiles[0].transform.position : fcs.ReferenceTransform.transform.position;
                Vector3 forward = tracking_center.bounds.center - position;
                float range = forward.magnitude;
                float flight_time = computer.GetFlightTime(current_ammo, range);

                Vector3 target_velocity = target.Aircraft != null ? target.Aircraft.VelocityNormalized : target.Chassis.Velocity;

                Vector3 a = flight_time * (target_velocity - fcs.Mounts[0]._unit.Chassis.Velocity);
                Vector3 compensated = tracking_center.bounds.center + a / (current_ammo.Guidance > AmmoType.GuidanceType.Unguided ? current_ammo.TurnSpeed : 1f);
                offset += PlayerInput.Instance.VirtualJoystick * (fcs.MainOptic.slot.CurrentFov / 60f * 2f);

                fcs.SetAimWorldPosition(
                    Matrix4x4.TRS(compensated, Quaternion.LookRotation(forward), Vector3.one).MultiplyPoint3x4(offset)
                );

                guidance_computer.transform.LookAt(Matrix4x4.TRS(compensated, Quaternion.LookRotation(forward), Vector3.one).MultiplyPoint3x4(offset));

                fcs.SetRange((tracking_center.bounds.center - fcs.ReferenceTransform.transform.position).magnitude);
            }
        }
    }

    [HarmonyPatch(typeof(GHPC.Equipment.Optics.UsableOptic), "LateUpdate")]
    public static class LockReticle
    {
        private static bool Prefix(GHPC.Equipment.Optics.UsableOptic __instance)
        {
            LockOnLead lead = __instance.FCS.gameObject.GetComponent<LockOnLead>() ?? null;

            if (lead != null && lead.target && lead.engaged)
            {
                Vector3 forward = lead.tracking_center.bounds.center - lead.fcs.ReferenceTransform.position;

                __instance.LookAt(
                    Matrix4x4.TRS(lead.tracking_center.bounds.center, Quaternion.LookRotation(forward), Vector3.one).MultiplyPoint3x4(lead.offset)
                );
                return false;
            }
            return true;
        }
    }
}