using System.Collections.Generic;
using GHPC.Effects;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class EjectorPrefabFix : MonoBehaviour
    {
        private static HashSet<string> prefabs = new HashSet<string>();
        public string Id = "";

        void Awake()
        {
            if (!prefabs.Contains(Id))
            {
                prefabs.Add(Id);
                return;
            } 

            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<DestroyInSeconds>().enabled = true;
        }

        public static void Remove(string id)
        {
            prefabs.Remove(id);
        }
    }
}
