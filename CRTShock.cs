using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Vehicle;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class CRTShock
    {
        static GameObject crt_shock_go;

        public static void Add(Transform transform, float z_offset, Vector3 scale)
        {
            GameObject go = GameObject.Instantiate(crt_shock_go, transform);
            go.transform.localPosition += new Vector3(0f, 0f, z_offset);
            go.transform.localScale = scale;
        }

        public static void Init()
        {
            if (crt_shock_go == null)
            {
                foreach (Vehicle obj in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
                {
                    if (obj.gameObject.name == "M1IP")
                    {
                        crt_shock_go = obj.transform.Find("Turret Scripts/GPS/FLIR/Scanline FOV change").gameObject;
                        break;
                    }
                }
            }

        }
    }
}
