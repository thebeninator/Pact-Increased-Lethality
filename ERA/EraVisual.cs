using UnityEngine;

namespace PactIncreasedLethality
{
    internal class EraVisual : MonoBehaviour
    {
        public MeshRenderer visual;
        public Material destroyed_mat;
        public bool hide_on_detonate = true;
        public string destroyed_target;
    }
}
