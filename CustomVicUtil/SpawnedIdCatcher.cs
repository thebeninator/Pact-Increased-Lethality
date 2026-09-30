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
        private static string current_spawn_id;
        public static string CurrentSpawnId { get => current_spawn_id; }

        private static void Prefix(UnitSpawner __instance, UnitMetaData metaData)
        {
            current_spawn_id = metaData.Name;
        }
    }
}
