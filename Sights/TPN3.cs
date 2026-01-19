using UnityEngine;
using GHPC.Vehicle;
using GHPC.Equipment.Optics;
using GHPC.Camera;
using Reticle;
using GHPC.Weapons;

namespace PactIncreasedLethality
{
    public class TPN3
    {
        public static void Add(FireControlSystem fcs, UsableOptic optic, CameraSlot camera) {
            optic.reticleMesh.reticleSO = ReticleMesh.cachedReticles["TPN3"].tree;
            optic.reticleMesh.reticle = ReticleMesh.cachedReticles["TPN3"];
            optic.reticleMesh.SMR = null;
            optic.reticleMesh.Load();

            optic.Alignment = OpticAlignment.BoresightStabilized;
            optic.RotateElevation = true;
            optic.RotateAzimuth = true;

            camera.DefaultFov = 6f;
            camera.BaseBlur = 0.2f;
            camera.VibrationBlurScale = 0.2f;
            camera.VibrationShakeMultiplier = 0.4f;
            camera.fovAspect = false;

            UpdateVerticalRangeScale uvrs = optic.gameObject.AddComponent<UpdateVerticalRangeScale>();
            uvrs.fcs = fcs;
            uvrs.reticle = optic.reticleMesh;
            optic.reticleMesh.smoothTime = 0.1f;
            optic.reticleMesh.maxSpeed = 2000f;
        }
    }
}
