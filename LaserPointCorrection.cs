using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Equipment.Optics;
using UnityEngine;

namespace PactIncreasedLethality
{
    internal class LaserPointCorrection : MonoBehaviour
    {
        public Transform laser;

        public Vector3 preserved_spot_day;
        public Quaternion preserved_rotation_day;

        public Vector3 preserved_spot_night;
        public Quaternion preserved_rotation_night;

        public UsableOptic day_optic;
        public UsableOptic night_optic;

        void Start() {
            laser.SetParent(day_optic.transform, true);
            preserved_spot_day = laser.transform.localPosition;
            preserved_rotation_day = laser.transform.localRotation;

            laser.SetParent(night_optic.transform, true);
            preserved_spot_night = laser.transform.localPosition;
            preserved_rotation_night = laser.transform.localRotation;

            laser.SetParent(day_optic.transform, true);
        }

        void Update() {
            if (day_optic.isActiveAndEnabled)
            {
                laser.SetParent(day_optic.transform, true);
                laser.transform.SetLocalPositionAndRotation(preserved_spot_day, preserved_rotation_day);
            }

            if (night_optic.isActiveAndEnabled) {
                laser.SetParent(night_optic.transform, true);
                laser.transform.SetLocalPositionAndRotation(preserved_spot_night, preserved_rotation_night);
            }
        }
    }
}
