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

namespace PactIncreasedLethality
{
    public class LockOnLead : MonoBehaviour
    {
        public Vehicle target;
        public FireControlSystem fcs;
        public T72.CustomGuidanceComputer guidance_computer;
        private float cd = 0f;
        public bool engaged = false;
        public Vector2 offset; 

        public void Update()
        {
            if (fcs == null) return;

            if (cd > 0f)
                cd -= Time.deltaTime;

            if (Input.GetKey(KeyCode.Mouse2) && cd <= 0f)
            {
                cd = 0.2f;

                if (!engaged)
                {
                    engaged = true;
                    fcs.DoLase();
                }
                else
                {
                    if (target)
                        fcs.SetAimWorldPosition(target.Center.position);

                    target = null;
                    engaged = false;
                    guidance_computer.autotrackingEnabled = false;
                    offset = Vector2.zero;
                }
            }

            if (!target) {
                engaged = false;
                guidance_computer.autotrackingEnabled = false;
                return;
            }

            guidance_computer.autotrackingEnabled = true;

            // adapted from GHPC.AI.BehaviorTrees.ActionBaseLookAt.GetAimPositionAtTarget
            MissileGuidanceUnit mgu = fcs.CurrentWeaponSystem.GuidanceUnit;
            AmmoType current_ammo = fcs.CurrentAmmoType;
            BallisticComputerRepository computer = BallisticComputerRepository.Instance;
            bool missile_active = current_ammo.Guidance > AmmoType.GuidanceType.Unguided && mgu.CurrentMissiles.Count > 0;

            Vector3 position = missile_active ? mgu.CurrentMissiles[0].transform.position : fcs.ReferenceTransform.transform.position;
            Vector3 forward = target.Center.position - position;
            float range = forward.magnitude;
            float flight_time = computer.GetFlightTime(current_ammo, range);

            Vector3 a = flight_time * (target.Chassis.Velocity - fcs.Mounts[0]._unit.Chassis.Velocity);
            Vector3 compensated = target.Center.position + a / (current_ammo.Guidance > AmmoType.GuidanceType.Unguided ? current_ammo.TurnSpeed : 1f); 
            offset += PlayerInput.Instance.VirtualJoystick * (fcs.MainOptic.slot.CurrentFov / 60f * 2f);

            fcs.SetAimWorldPosition(
                Matrix4x4.TRS(compensated, Quaternion.LookRotation(forward), Vector3.one).MultiplyPoint3x4(offset)
            );

            guidance_computer.transform.LookAt(Matrix4x4.TRS(compensated, Quaternion.LookRotation(forward), Vector3.one).MultiplyPoint3x4(offset));

            fcs.SetRange((target.Center.position - fcs.ReferenceTransform.transform.position).magnitude);
        }
    }

    [HarmonyPatch(typeof(GHPC.Equipment.Optics.UsableOptic), "LateUpdate")]
    public static class LockReticle
    {
        private static bool Prefix(GHPC.Equipment.Optics.UsableOptic __instance)
        {
            LockOnLead lead = __instance.FCS.gameObject.GetComponent<LockOnLead>() ?? null;

            if (lead != null && lead.target)
            {
                Vector3 forward = lead.target.Center.position - lead.fcs.ReferenceTransform.position;

                __instance.LookAt(
                    Matrix4x4.TRS(lead.target.Center.position, Quaternion.LookRotation(forward), Vector3.one).MultiplyPoint3x4(lead.offset)
                );
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(GHPC.Weapons.FireControlSystem), "DoLase")]
    public static class LockTarget
    {
        private static void Postfix(GHPC.Weapons.FireControlSystem __instance)
        {
            LockOnLead lead = __instance.gameObject.GetComponent<LockOnLead>();

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

            if (raycastHit.transform != null)
            {
                GameObject raycast_hit = raycastHit.transform.gameObject;

                if (raycast_hit.gameObject != null && raycast_hit.gameObject.GetComponent<IArmor>() != null)
                    lead.target = (Vehicle)raycast_hit.GetComponent<IArmor>().Unit;
            }

            return;
        }
    }
}