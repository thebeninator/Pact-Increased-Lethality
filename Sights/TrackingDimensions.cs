using System.Linq;
using UnityEngine;
using HarmonyLib;
using System;
using GHPC.Mission.Data;
using GHPC.AI;
using System.Collections.Generic;
using GHPC.UI.Tips;

namespace PactIncreasedLethality
{
    public static class TrackingDimensions
    {
        static bool done = false;

        private static void CreateTrackingObject(GameObject vic, Dim dim) 
        {
            GameObject t = GameObject.CreatePrimitive(PrimitiveType.Cube);
            t.layer = 8;
            t.name = "TRACKING OBJECT";
            t.transform.parent = vic.transform;
            t.transform.localPosition = dim.pos;
            t.transform.localScale = dim.scale;       
            t.GetComponent<MeshRenderer>().material = null;
            t.GetComponent<MeshRenderer>().materials = new Material[0];            
            t.GetComponent<BoxCollider>().enabled = false;
        }

        private struct Dim
        {
            public Dim(Vector3 pos, Vector3 scale)
            {
                this.pos = pos;
                this.scale = scale;
            }

            public Vector3 pos;
            public Vector3 scale;
        }

        private static Dictionary<string, Dim> DIMS = new Dictionary<string, Dim>() {
            ["Marder Bradley"] = new Dim(new Vector3(0f, 1.38f, 0.45f), new Vector3(3f, 2.5f, 6f)),
            ["M60 LEO"]        = new Dim(new Vector3(0f, 1.32f, 0f), new Vector3(3.6f, 3f, 7f)),
            ["_M1"]            = new Dim(new Vector3(0f, 1.21f, -0.25f), new Vector3(3.5f, 2.4f, 7.8f)),
            ["M113 M901"]      = new Dim(new Vector3(0f, 1.17f, 0.35f), new Vector3(2.5f, 2.4f, 4.7f)),
            ["M151"]           = new Dim(new Vector3(0f, 0.77f, -0.25f), new Vector3(0.5f, 1.5f, 3f)),
            ["M923"]           = new Dim(new Vector3(0f, 1.37f, -0.78f), new Vector3(2.3f, 2.8f, 7.5f)),
            ["T72 T80 T64"]    = new Dim(new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f)),
            ["T55A T62A T54A"] = new Dim(new Vector3(0f, 1.124f, 0.087f), new Vector3(3.25f, 2.35f, 6f)),
            ["Ural"]           = new Dim(new Vector3(0f, 1.471f, -1.215f), new Vector3(2.65f, 3f, 7.3f)),
            ["BMP"]            = new Dim(new Vector3(0f, 1.002f, 0.12f), new Vector3(3f, 2f, 6.95f)),
            ["BTR"]            = new Dim(new Vector3(0f, 1.35f, 0f), new Vector3(3f, 2.6f, 7.5f)),
            ["BRDM2"]          = new Dim(new Vector3(0f, 1.228f, -0.221f), new Vector3(2.25f, 2.3f, 5.75f)),
            ["T-34-85"]        = new Dim(new Vector3(0f, 1.34f, 0.36f), new Vector3(3f, 2.6f, 6f)),
            ["PT76B"]          = new Dim(new Vector3(0f, 1.14f, 0.32f), new Vector3(3f, 2.25f, 7f)),
            ["Mi-8"]           = new Dim(new Vector3(0f, 1.33f, -0.62f), new Vector3(3.7f, 3.5f, 11f)),
            ["Mi-2"]           = new Dim(new Vector3(0f, 1.228f, -0.221f), new Vector3(2.25f, 2.3f, 5.75f)),
            ["Mi-24"]          = new Dim(new Vector3(0f, 1.87f, -1.2f), new Vector3(2.7f, 3f, 10f)),
            ["AH-1"]           = new Dim(new Vector3(0f, 1.83f, 0.73f), new Vector3(3.4f, 2.5f, 7f)),
            ["OH-58A"]         = new Dim(new Vector3(0f, 2.01f, 0.22f), new Vector3(2f, 2f, 5f))
        };

        private static bool MatchesDimKey(string prefab_name, string key) {
            string[] subkeys = key.Split(' ');

            foreach (string subkey in subkeys) 
            {
                if (prefab_name.Contains(subkey)) return true;
            }

            return false;
        }

        private static Dim? GetDim(string prefab_name) {
            foreach (string key in DIMS.Keys)
            {
                if (!MatchesDimKey(prefab_name, key)) continue;
                return DIMS[key];
            }

            return null;
        }

        [HarmonyPatch(typeof(GHPC.Mission.UnitSpawner), "SpawnUnit")]
        [HarmonyPatch(new Type[] { typeof(GameObject), typeof(UnitMetaData), typeof(WaypointHolder), typeof(Transform) })]
        public static class GenerateTrackingDimension
        {
            private static void Prefix(GHPC.Mission.UnitSpawner __instance, ref GameObject prefab)
            {
                if (prefab.transform.Find("TRACKING OBJECT")) return;

                Dim? dim = GetDim(prefab.name);

                if (dim.HasValue)
                {
                    CreateTrackingObject(prefab, dim.Value);
                }
            }
        }
    }
}
