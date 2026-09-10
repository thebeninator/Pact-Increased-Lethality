using GHPC.Vehicle;
using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace CustomVicUtil
{
    internal class CustomSpawnInfo
    {
        public GameObject Prefab { get; set; }
        public string Id { get; set; }
        public string TargetId { get; set; }
        public Action<GameObject, Vehicle> OnSpawned { get; set; }
    }

    [HarmonyPatch(typeof(TrackedWheelNodeConfig), "Awake")]
    internal static class CustomSpawnHandler
    {
        private static Dictionary<string, CustomSpawnInfo> custom_vics = new Dictionary<string, CustomSpawnInfo>();

        public static void RegisterCustomVic(CustomSpawnInfo spawn_info)
        {
            custom_vics.Add(spawn_info.Id, spawn_info);
        }

        private static void Prefix(TrackedWheelNodeConfig __instance)
        {
            Vehicle original_vic = __instance.GetComponentInParent<Vehicle>();

            CustomSpawnInfo spawn_info = custom_vics["PIL_BMP3"];
            string maybe_custom_vic_id = SpawnedIdCatcher.CurrentSpawnId.Split(' ')[0];

            //if (custom_vics.ContainsKey(maybe_custom_vic_id))
            //{
            //    spawn_info = custom_vics[maybe_custom_vic_id];
            //}

            if (spawn_info != null && original_vic._uniqueName == spawn_info.TargetId)
            {
                GameObject instance = GameObject.Instantiate(spawn_info.Prefab, original_vic.transform);
                instance.transform.localPosition = Vector3.zero;

                spawn_info.OnSpawned?.Invoke(instance, original_vic);
            }
        }
    }
}
