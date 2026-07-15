using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Camera;
using UnityEngine;

namespace PactIncreasedLethality.PDL
{
    internal class CommanderWeapon : MonoBehaviour
    {
        private bool weapon_equipped = false;
        private float cd = 0.0f;

        void Update()
        {
            if (CameraManager.Instance._allFreeLookCamSlots == null) return;
            if (CameraManager.Instance._allFreeLookCamSlots.Length == 0) return;

            bool in_commander_view = CameraManager.Instance._currentCamSlot.AllowFreeLook;

            if (in_commander_view)
            {

            }
        }
    }
}
