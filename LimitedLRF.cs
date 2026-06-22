using GHPC.Utility;
using GHPC;
using TMPro;
using UnityEngine;
using HarmonyLib;
using GHPC.PhysicsHelpers;

namespace PactIncreasedLethality
{
    public class LimitedLRF : MonoBehaviour {
        public Transform canvas; 
    }

    // hijack lase method to prevent it from ranging the gun itself 
    [HarmonyPatch(typeof(GHPC.Weapons.FireControlSystem), "DoLase")]
    public static class LimitedLase
    {
        private static bool Prefix(GHPC.Weapons.FireControlSystem __instance)
        {
            if (!__instance.GetComponent<LimitedLRF>())
            {
                return true;
            }

            if (__instance._laserDestroyed)
            {
                return true;
            }

            __instance._laseQueued = false;

            float num = -1f;
            int layer_mask = 1 << CodeUtils.LAYER_INDEX_VISIBILITYONLY;
            Ray ray = new Ray(__instance.LaserOrigin.position, __instance.LaserOrigin.forward);

            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, __instance.MaxLaserRange, layer_mask) && raycastHit.collider.tag == "Smoke")
            {
                num = raycastHit.distance;
            }
            if (RaycastColliderUtils.Raycast(ray, out raycastHit, __instance.MaxLaserRange, ConstantsAndInfoManager.Instance.LaserRangefinderLayerMask) && (raycastHit.distance < num || num == -1f))
            {
                num = raycastHit.distance;
            }

            if (num != -1f)
            {
                Transform range_readout = __instance.GetComponent<LimitedLRF>().canvas;
                var text = range_readout.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                text.text = ((int)MathUtil.RoundFloatToMultipleOf(num, 50)).ToString("0000");
            }

            return false;
        }
    }
}
