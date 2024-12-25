using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class ScreenShake
    {
        public class CameraShake : MonoBehaviour
        {
            // Transform of the camera to shake. Grabs the gameObject's transform
            // if null.
            public Transform camTransform;

            // How long the object should shake for.
            public float shakeDuration = 0f;

            // Amplitude of the shake. A larger value shakes the camera harder.
            public float shakeAmount = 3.2f;
            public float decreaseFactor = 1f;

            Vector3 originalPos;

            void Awake()
            {
                if (camTransform == null)
                {
                    camTransform = GetComponent(typeof(Transform)) as Transform;
                }
            }

            void Update()
            {
                if (shakeDuration > 0)
                {
                    Vector3 rand = UnityEngine.Random.insideUnitSphere * shakeAmount;
                    camTransform.localPosition = new Vector3(originalPos.x + rand.x, originalPos.y + rand.y, originalPos.z + rand.z);
                    shakeDuration -= Time.deltaTime * decreaseFactor;
                }
                else
                {
                    shakeDuration = 0f;
                    camTransform.localPosition = Vector3.zero;
                }
            }
        }
    }
}
