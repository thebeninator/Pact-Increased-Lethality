using GHPC.Weapons;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class CustomGuidanceComputer : MonoBehaviour
    {
        public FireControlSystem fcs;
        public MissileGuidanceUnit mgu;
        public bool autotrackingEnabled = false;

        void Update()
        {
            mgu.AimElement = fcs.MainOptic.transform;
        }
    }
}
