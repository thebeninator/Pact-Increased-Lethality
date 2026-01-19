using GHPC.Vehicle;
using UnityEngine;
using HarmonyLib;
using GHPC.Weapons;
using GHPC.Player;
using GHPC.Thermals;
using System;

namespace PactIncreasedLethality
{
    public class LockOnLead : MonoBehaviour
    {
        private Vehicle self;
        public Vehicle target;
        public Renderer tracking_center;
        public RectTransform tracking_gates;
        public FireControlSystem fcs;
        public CustomGuidanceComputer guidance_computer;
        private float cd = 0f;
        public bool engaged = false;
        public Vector2 offset;

        public event Action<bool> TargetLockChanged;

        void Awake()
        {
            self = GetComponentInParent<Vehicle>();
        }

        void ResetTracking() {
            if (target != null)
            {
                TargetLockChanged(false);
            }

            target = null;
            engaged = false;
            guidance_computer.autotrackingEnabled = false;
            offset = Vector2.zero;
        }

        void Update()
        {
            if (fcs == null) return;
            if (PlayerInput.Instance?.CurrentPlayerWeapon?.FCS != fcs) {
                ResetTracking();
                return;
            }

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
            if (target == null || (target && !engaged))
            {
                ray = new Ray(fcs.ReferenceTransform.position, fcs.AimWorldVector);
            }

            if (engaged) 
            {
                Vector3 forward = tracking_center.bounds.center - fcs.ReferenceTransform.position;
                ray = new Ray(fcs.ReferenceTransform.position, forward);
            }

            RaycastHit raycast_hit;
            int main_body_layer = 1 << 14;
            int terrain_layer = 1 << 18;
            Physics.Raycast(ray, out raycast_hit, 5000f, main_body_layer | terrain_layer);
            GameObject raycast_hit_obj = raycast_hit.transform?.gameObject;
            Vehicle possible_target = raycast_hit_obj?.GetComponentInParent<Vehicle>();

            if (possible_target != null)
            {
                if (possible_target.GetInstanceID() != self.GetInstanceID())
                {
                    if (target == null || target.GetInstanceID() != possible_target.GetInstanceID())
                    {
                        TargetLockChanged(true);
                    }

                    target = possible_target;
                    Transform tracking_object = target.gameObject.transform.Find("TRACKING OBJECT");
                    tracking_center = tracking_object.GetComponent<Renderer>();
                }
            }
            else
            {
                ResetTracking();
            }

            if (engaged && target)
            {
                offset += PlayerInput.Instance.VirtualJoystick * (fcs.MainOptic.slot.CurrentFov / 60f * 2f);

                if (!fcs.CurrentWeaponSystem.AbleToFire) return;

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
                Vector3 travel_dist = flight_time * (target_velocity - fcs.Mounts[0]._unit.Chassis.Velocity);

                if (current_ammo.Guidance > AmmoType.GuidanceType.Unguided)
                {
                    travel_dist = Vector3.zero;
                }

                Vector3 compensated = tracking_center.bounds.center + travel_dist;

                fcs.SetAimWorldPosition(
                    Matrix4x4.TRS(compensated, Quaternion.LookRotation(forward), Vector3.one).MultiplyPoint3x4(offset)
                );

                guidance_computer.transform.LookAt(Matrix4x4.TRS(compensated, Quaternion.LookRotation(forward), Vector3.one).MultiplyPoint3x4(offset));

                float actual_range = (tracking_center.bounds.center - fcs.ReferenceTransform.transform.position).magnitude;
                fcs.SetRange(actual_range, forceUpdate: true);
            }
        }
    }

    [HarmonyPatch(typeof(GHPC.Equipment.Optics.UsableOptic), "LateUpdate")]
    public static class LockOnLeadPatch
    {
        private static bool Prefix(GHPC.Equipment.Optics.UsableOptic __instance)
        {
            if (__instance.FCS == null) return true;

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

        public static void Postfix(GHPC.Equipment.Optics.UsableOptic __instance)
        {
            if (__instance.FCS == null) return;

            LockOnLead lead = __instance.FCS.gameObject.GetComponent<LockOnLead>() ?? null;

            if (lead == null) return;
            if (lead.target == null) return;
            if (lead.tracking_gates == null) return;

            Transform tracking_object = lead.target.gameObject.transform.Find("TRACKING OBJECT");

            if (tracking_object == null) return;

            Camera camera = FLIRCamera.Instance._thermalCamera;
            Vector2 monitor_dims = new Vector2(camera.pixelWidth, camera.pixelHeight);
            Vector2 screen_dims = new Vector2(Screen.width, Screen.height);
            Bounds bounds = tracking_object.GetComponent<MeshRenderer>().bounds;

            Vector3[] ss_corners = new Vector3[] {
                camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.max.z)),
                camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z)),
                camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z)),
                camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z)),
                camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z)),
                camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z)),
                camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z)),
                camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z))
            };

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

            lead.tracking_gates.position = new Vector2(min_x, min_y) / monitor_dims * screen_dims;
            lead.tracking_gates.sizeDelta = new Vector2(max_x - min_x, max_y - min_y);
        }
    }
}