using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Vehicle;
using UnityEngine;

namespace PactIncreasedLethality
{
    public static class TrackingDimensions
    {
        static bool done = false;
        static Vehicle[] vics;

        private static void CreateTrackingObject(string parent, Vector3 pos, Vector3 scale) {
            GameObject vic = vics.Where(o => o.name == parent).First().gameObject;
            GameObject t = GameObject.CreatePrimitive(PrimitiveType.Cube);
            t.name = "TRACKING OBJECT";
            t.GetComponent<BoxCollider>().enabled = false;
            t.GetComponent<MeshRenderer>().forceRenderingOff = false;
            t.transform.parent = vic.transform;
            t.transform.localPosition = pos;
            t.transform.localScale = scale;
        }

        public static void Generate() {
            vics = Resources.FindObjectsOfTypeAll<Vehicle>();

            if (vics.Length == 0 || done) return;

            CreateTrackingObject("M2 Bradley", new Vector3(0f, 1.38f, 0.45f), new Vector3(3f, 2.5f, 6f));

            done = true;
        }
    }
}
