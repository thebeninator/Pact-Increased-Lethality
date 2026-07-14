using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactIncreasedLethality.APS
{
    internal class APS
    {
        public static void Add(Transform[] launchers, Transform[] colliders, int[][] assignments, Schema schema)
        {
            APSLauncher[] all_launchers = new APSLauncher[launchers.Length];

            for (int i = 0; i < launchers.Length; i++)
            {
                all_launchers[i] = new APSLauncher(launchers[i]);
            }

            for (int i = 0; i < colliders.Length; i++)
            {                
                Transform collider = colliders[i];
                APSCollider aps_collider = collider.gameObject.AddComponent<APSCollider>();
                aps_collider.schema = schema;

                List<APSLauncher> to_add = new List<APSLauncher>();

                for (int j = 0; j < assignments[i].Length; j++)
                {
                    APSLauncher launcher = all_launchers[assignments[i][j]];
                    to_add.Add(launcher);
                }
                aps_collider.Init(to_add.ToArray());
            }
        }
    }
}

