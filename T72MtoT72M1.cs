using GHPC.AI;
using GHPC.Mission.Data;
using GHPC.Mission;
using GHPC;
using UnityEngine;
using HarmonyLib;
using System;
using ModUtil;

namespace PactIncreasedLethality
{
    internal class T72MtoT72M1
    {
        internal class PreviouslyT72M : MonoBehaviour { }

        internal class TransmogState<T>
        {
            public bool TransmogNeeded { get; set; }
            public T Preset { get; set; }
        }

        [HarmonyPatch(typeof(UnitSpawner), "SpawnUnit", new Type[] { typeof(string), typeof(UnitMetaData), typeof(WaypointHolder), typeof(Transform) })]
        public static class OverrideT72M
        {
            private static void Prefix(out TransmogState<object> __state, UnitSpawner __instance, ref string uniqueName)
            {
                __state = new TransmogState<object>();

                if (uniqueName == "T72M")
                {
                    bool transmog = T72.t72m_composite_cheeks.Value
                        || T72.t72m_super_composite_cheeks.Value
                        || T72.era_t72m.Value
                        || T72.k5_t72m.Value;

                    if (transmog)
                    {
                        __state.TransmogNeeded = true;
                        AssetUtil.TempLoadVanillaVehicle("T72M");
                        uniqueName = "T72M1";
                    }
                }
            }

            private static void Postfix(TransmogState<object> __state, ref IUnit __result)
            {
                if (__state.TransmogNeeded)
                {
                    PreviouslyT72M comp = __result.transform.gameObject.AddComponent<PreviouslyT72M>();
                    comp.enabled = false;
                }
            }
        }

    }
}
