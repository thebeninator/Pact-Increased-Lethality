using GHPC.Weapons;
using Reticle;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class UpdateVerticalRangeScale : MonoBehaviour
    { 
        public FireControlSystem fcs;
        public ReticleMesh reticle;

        void Update()
        {
            reticle.CurrentAmmo = fcs.CurrentAmmoType;

            if (reticle.curReticleRange != fcs.CurrentRange)
            {
                reticle.targetReticleRange = fcs.CurrentRange;
            }
        }
    }
}
