using GHPC.AI;
using GHPC.Mission.Data;
using GHPC.Mission;
using UnityEngine;
using System;
using HarmonyLib;

namespace CustomVicUtil
{
    [HarmonyPatch(typeof(UnitSpawner), "SpawnUnit", new Type[] { typeof(string), typeof(UnitMetaData), typeof(WaypointHolder), typeof(Transform) })]
    internal static class SpawnedIdCatcher
    {
        public static string CurrentSpawnId { get; set; }

        private static void Prefix(UnitSpawner __instance, UnitMetaData metaData)
        {
            CurrentSpawnId = metaData.Name;
        }
    }
}
