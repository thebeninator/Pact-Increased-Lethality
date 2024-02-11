using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Camera;
using GHPC.Equipment.Optics;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class DigitalZoomSnapper : MonoBehaviour
    {
        private float cd = 0f;
        private CameraManager camera_manager;

        void Awake() { 
            camera_manager = GameObject.Find("_APP_GHPC_").GetComponent<CameraManager>();
        }

        void Update()
        {
            cd -= Time.deltaTime;

            if (Input.GetKey(KeyCode.Mouse2) && cd <= 0f)
            {
                cd = 0.2f;

                CameraSlot cam = this.GetComponent<UsableOptic>().slot;

                cam.FovIndex = cam.FovIndex < cam.OtherFovs.Length ? cam.OtherFovs.Length : 0;

                camera_manager.ZoomChanged();
            }
        }
    }
}
