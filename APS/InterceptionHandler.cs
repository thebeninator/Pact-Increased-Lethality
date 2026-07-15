using UnityEngine;
using HarmonyLib;

namespace ActiveProtectionSystem
{
    [HarmonyPatch(typeof(GHPC.Weapons.LiveRound), "penCheck")]
    internal static class InterceptionHandler
    {
        private static bool Prefix(GHPC.Weapons.LiveRound __instance, object[] __args)
        {
            if (__instance.IsSpall) return true;

            Collider collider = (Collider)__args[0];
            Vector3 impact_point = (Vector3)__args[3];
            APSCollider aps = collider.GetComponent<APSCollider>();

            if (aps == null) return true;
            if (__instance.ShotInfo.TypeInfo.Caliber < aps.schema.min_engage_caliber && __instance.ShotInfo.TypeInfo.Caliber != 0f) return true;
            if (__instance.CurrentSpeed < aps.schema.min_engage_velocity || __instance.CurrentSpeed > aps.schema.max_engage_velocity) return true;

            __instance.Story.builder.AppendLine("Detected by APS radar");

            if (!aps.TryFireProjectile(__instance._lastFramePosition))
            {
                __instance.Story.builder.AppendLine("APS launcher expended");
                return true;
            }

            __instance.Story.builder.AppendLine("Intercepted by APS");
            __instance._parentUnit = aps.parent_unit;
            __instance._frameData.VehicleStruck = aps.parent_unit;
            __instance.ShotInfo.Distance = Vector3.Distance(impact_point, __instance.Shooter.transform.position);
            __instance.reportShotTraceFrame();
            //__instance._fuzeCompleted = true;
            __instance.Detonate();
            __instance.createExplosion(false, 0f, Vector3.zero, 0.03f, 55);

            return false;
        }
    }
}
