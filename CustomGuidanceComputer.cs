using GHPC.Weapons;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class CustomGuidanceComputer : MonoBehaviour
    {
        public FireControlSystem fcs;
        public MissileGuidanceUnit mgu;

        void Update()
        {
            mgu.AimElement.LookAt(mgu.AimElement.position + fcs.TargetAimWorldVectorScaled.normalized);
        }
    }
}
