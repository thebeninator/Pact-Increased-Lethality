using GHPC.Weapons;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class ReducedATGMSmoke : MonoBehaviour
    {
        public FireControlSystem fcs;
        public Transform[] fx; 

        void Update()
        {
            foreach (Transform t in fx)
            {
                t.gameObject.SetActive(fcs.CurrentAmmoType.ShortName != AmmoType.AmmoShortName.Missile);
            }
        }
    }
}
