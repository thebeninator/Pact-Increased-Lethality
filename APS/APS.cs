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

                int assigned_launchers_count = assignments[i].Length;
                APSLauncher[] to_add = new APSLauncher[assigned_launchers_count];

                for (int j = 0; j < assigned_launchers_count; j++)
                {
                    to_add[j] = all_launchers[assignments[i][j]];
                }

                aps_collider.Init(to_add);
            }
        }
    }
}

