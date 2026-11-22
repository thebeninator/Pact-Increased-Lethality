using System.Collections.Generic;
using GHPC.Effects;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class CustomEjectorPrefab : MonoBehaviour
    {
        private static HashSet<string> prefabs = new HashSet<string>();

        public string Id = "";
        public bool HasSmoke = false; 

        void Awake()
        {
            if (!prefabs.Contains(Id))
            {
                prefabs.Add(Id);
                return;
            } 

            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<DestroyInSeconds>().enabled = true;
            if (HasSmoke)
            {
                ParticleSystem smoke = GetComponentInChildren<ParticleSystem>(true);
                smoke.gameObject.SetActive(true);
                smoke.Play();
            }
        }

        public static void Remove(string id)
        {
            prefabs.Remove(id);
        }
    }
}
