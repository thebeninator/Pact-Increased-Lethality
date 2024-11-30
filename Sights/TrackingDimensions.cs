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
            t.layer = 8;
            t.name = "TRACKING OBJECT";
            t.transform.parent = vic.transform;
            t.transform.localPosition = pos;
            t.transform.localScale = scale;       
            t.GetComponent<MeshRenderer>().material = null;
            t.GetComponent<MeshRenderer>().materials = new Material[0];
            
            t.GetComponent<BoxCollider>().enabled = false;
        }

        public static void Generate() {
            vics = Resources.FindObjectsOfTypeAll<Vehicle>();

            if (vics.Length == 0 || done) return;

            CreateTrackingObject("M2 Bradley", new Vector3(0f, 1.38f, 0.45f), new Vector3(3f, 2.5f, 6f));

            CreateTrackingObject("M60A1 RISE Passive Late", new Vector3(0f, 1.32f, 0f), new Vector3(3.6f, 3f, 7f));
            CreateTrackingObject("M60A1 RISE Passive Early", new Vector3(0f, 1.32f, 0f), new Vector3(3.6f, 3f, 7f));
            CreateTrackingObject("M60A1 AOS", new Vector3(0f, 1.32f, 0f), new Vector3(3.6f, 3f, 7f));
            CreateTrackingObject("M60A1", new Vector3(0f, 1.32f, 0f), new Vector3(3.6f, 3f, 7f));
            CreateTrackingObject("M60A3", new Vector3(0f, 1.32f, 0f), new Vector3(3.6f, 3f, 7f));
            CreateTrackingObject("M60A3 TTS", new Vector3(0f, 1.32f, 0f), new Vector3(3.6f, 3f, 7f));

            CreateTrackingObject("M1", new Vector3(0f, 1.21f, -0.25f), new Vector3(3.5f, 2.4f, 7.8f));
            CreateTrackingObject("M1IP", new Vector3(0f, 1.21f, -0.25f), new Vector3(3.5f, 2.4f, 7.8f));

            CreateTrackingObject("M113", new Vector3(0f, 1.17f, 0.35f), new Vector3(2.5f, 2.4f, 4.7f));
            CreateTrackingObject("M901", new Vector3(0f, 1.17f, 0.35f), new Vector3(2.5f, 2.4f, 4.7f));

            CreateTrackingObject("M923", new Vector3(0f, 1.37f, -0.78f), new Vector3(2.3f, 2.8f, 7.5f));

            CreateTrackingObject("M151", new Vector3(0f, 0.77f, -0.25f), new Vector3(0.5f, 1.5f, 3f));
            CreateTrackingObject("M151_M232", new Vector3(0f, 0.77f, -0.25f), new Vector3(0.5f, 1.5f, 3f));

            CreateTrackingObject("T72 UV2 Variant", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T72 Ural LEM", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T72 UV1 (Variant)", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T72 Ural", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T72 Gill (Variant)", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T72M", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T72M1", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));

            CreateTrackingObject("T80B", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));

            CreateTrackingObject("T64A 1981", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T64A 1979", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T64A 1974", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T64B", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T64R", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T64A 1984", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));
            CreateTrackingObject("T64A 1983", new Vector3(0f, 1.096f, -0.523f), new Vector3(3.6f, 2.2f, 6.5f));

            CreateTrackingObject("T55A", new Vector3(0f, 1.124f, 0.087f), new Vector3(3.25f, 2.35f, 6f));
            CreateTrackingObject("T54A", new Vector3(0f, 1.124f, 0.087f), new Vector3(3.25f, 2.35f, 6f));

            CreateTrackingObject("T62", new Vector3(0f, 1.124f, 0.087f), new Vector3(3.25f, 2.35f, 6f));

            CreateTrackingObject("Ural", new Vector3(0f, 1.471f, -1.215f), new Vector3(2.65f, 3f, 7.3f));
            CreateTrackingObject("Ural Soviet", new Vector3(0f, 1.471f, -1.215f), new Vector3(2.65f, 3f, 7.3f));

            CreateTrackingObject("BMP1", new Vector3(0f, 1.002f, 0.12f), new Vector3(3f, 2f, 6.95f));
            CreateTrackingObject("BMP1P (Variant) Soviet", new Vector3(0f, 1.002f, 0.12f), new Vector3(3f, 2f, 6.95f));
            CreateTrackingObject("BMP1P (Variant)", new Vector3(0f, 1.002f, 0.12f), new Vector3(3f, 2f, 6.95f));
            CreateTrackingObject("BMP1 Soviet", new Vector3(0f, 1.002f, 0.12f), new Vector3(3f, 2f, 6.95f));
            CreateTrackingObject("BMP2", new Vector3(0f, 1.002f, 0.12f), new Vector3(3f, 2f, 6.95f));
            CreateTrackingObject("BMP2 Soviet", new Vector3(0f, 1.002f, 0.12f), new Vector3(3f, 2f, 6.95f));

            CreateTrackingObject("BTR60PB", new Vector3(0f, 1.35f, 0f), new Vector3(3f, 2.6f, 7.5f));
            CreateTrackingObject("BTR60PB Soviet", new Vector3(0f, 1.35f, 0f), new Vector3(3f, 2.6f, 7.5f));
            CreateTrackingObject("BTR70", new Vector3(0f, 1.35f, 0f), new Vector3(3f, 2.6f, 7.5f));

            CreateTrackingObject("PT76B", new Vector3(0f, 1.14f, 0.32f), new Vector3(3f, 2.25f, 7f));
            CreateTrackingObject("T-34-85", new Vector3(0f, 1.34f, 0.36f), new Vector3(3f, 2.6f, 6f));

            CreateTrackingObject("BRDM2", new Vector3(0f, 1.228f, -0.221f), new Vector3(2.25f, 2.3f, 5.75f));
            CreateTrackingObject("BRDM2 Soviet", new Vector3(0f, 1.228f, -0.221f), new Vector3(2.25f, 2.3f, 5.75f));

            CreateTrackingObject("Mi-8", new Vector3(0f, 1.33f, -0.62f), new Vector3(3.7f, 3.5f, 11f));
            CreateTrackingObject("Mi-2", new Vector3(0f, 1.78f, 0.56f), new Vector3(3f, 2.5f, 5f));

            CreateTrackingObject("Mi24", new Vector3(0f, 1.87f, -1.2f), new Vector3(2.7f, 3f, 10f));
            CreateTrackingObject("Mi24V soviet", new Vector3(0f, 1.87f, -1.2f), new Vector3(2.7f, 3f, 10f));
            CreateTrackingObject("Mi24V soviet Rockets", new Vector3(0f, 1.87f, -1.2f), new Vector3(2.7f, 3f, 10f));
            CreateTrackingObject("Mi24V NVA Rockets", new Vector3(0f, 1.87f, -1.2f), new Vector3(2.7f, 3f, 10f));
            CreateTrackingObject("Mi24 Rockets", new Vector3(0f, 1.87f, -1.2f), new Vector3(2.7f, 3f, 10f));
            CreateTrackingObject("Mi24V NVA", new Vector3(0f, 1.87f, -1.2f), new Vector3(2.7f, 3f, 10f));

            CreateTrackingObject("AH-1", new Vector3(0f, 1.83f, 0.73f), new Vector3(3.4f, 2.5f, 7f));
            CreateTrackingObject("AH-1 rockets", new Vector3(0f, 1.83f, 0.73f), new Vector3(3.4f, 2.5f, 7f));

            CreateTrackingObject("OH-58A", new Vector3(0f, 2.01f, 0.22f), new Vector3(2f, 2f, 5f));

            done = true;
        }
    }
}
