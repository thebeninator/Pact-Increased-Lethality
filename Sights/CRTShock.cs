using GHPC.Vehicle;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class CRTShock
    {
        public static void Add(Transform transform, float z_offset, Vector3 scale)
        {
            GameObject go = GameObject.Instantiate(Assets.crt_shock_go, transform);
            go.transform.localPosition += new Vector3(0f, 0f, z_offset);
            go.transform.localScale = scale;
        }
    }
}
