using System;
using GHPC.Utility;
using GHPC.Weapons;
using TMPro;
using UnityEngine;
using HarmonyLib;

namespace PactIncreasedLethality
{
    public class UVBU : MonoBehaviour
    {
        public FireControlSystem fcs;
        public GameObject readout_go;
        public TextMeshProUGUI readout;
        public float cd = 0f;

        void Update()
        {
            if (cd > 0f && readout.IsActive())
            {
                cd -= Time.deltaTime;
            }

            if (cd <= 0f && readout.IsActive())
            {
                cd = 0f;
                readout_go.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(GHPC.Weapons.FireControlSystem), "DoLase")]
    public static class UVBULead
    {
        private static void Postfix(GHPC.Weapons.FireControlSystem __instance)
        {
            UVBU uvbu = __instance.GetComponentInChildren<UVBU>();

            if (uvbu == null) return;

            uvbu.cd = 2f;

            uvbu.readout_go.SetActive(true);

            float flight_time = __instance._bc.GetFlightTime(__instance._bcAmmo, __instance.TargetRange);
            float x = __instance._averageTraverseRate.x * 0.017453292f * __instance.TargetRange * flight_time * -10f;
            x /= (1f - __instance.transform.localPosition.x) * Mathf.Clamp(__instance.TargetRange / 1500f, 0f, 1f);
            string sign = Math.Sign(x) > 0 ? "+" : "-";

            int lead = (int)Math.Abs(MathUtil.RoundIntToMultipleOf((int)x, 5));

            if (lead > 999) lead = 999;
            if ((int)lead == 0) sign = "";

            uvbu.readout.text = sign + lead.ToString("000");
        }
    }
}
