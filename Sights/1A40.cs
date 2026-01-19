using System;
using System.Linq;
using GHPC.Equipment.Optics;
using GHPC.Weapons;
using TMPro;
using UnityEngine;
using Reticle;
using static Reticle.ReticleTree;

namespace PactIncreasedLethality
{
    public class FireControlSystem1A40
    {
        private static GameObject lead_readout_canvas;

        private static ReticleSO reticleSO;
        private static ReticleMesh.CachedReticle reticle_cached;

        private static bool assets_loaded = false;

        public static void Add(FireControlSystem fcs, UsableOptic optic, Vector3 offset) {
            fcs.RecordTraverseRateBuffer = true;
            fcs.TraverseBufferSeconds = 1f;
            fcs.DynamicLead = true;
            fcs._fixParallaxForVectorMode = true;
            fcs.InertialCompensation = false;
            optic.CantCorrect = true;
            optic.CantCorrectMaxSpeed = 0f;
            fcs._autoDumpViaPalmSwitches = false;
            fcs.EngageLead();

            GameObject readout = GameObject.Instantiate(lead_readout_canvas, optic.transform);
            readout.transform.GetChild(0).transform.localPosition = offset;
            readout.SetActive(false);

            UVBU lead = optic.gameObject.AddComponent<UVBU>();
            lead.fcs = fcs;
            lead.readout = readout.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            lead.readout.text = "000";
            lead.readout_go = readout;

            optic.reticleMesh.reticleSO = reticleSO;
            optic.reticleMesh.reticle = reticle_cached;
            optic.reticleMesh.SMR = null;
            optic.reticleMesh.Load();
        }

        private static void Reticle() {
            reticleSO = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T72"].tree);
            reticleSO.name = "1A40";

            Util.ShallowCopy(reticle_cached, ReticleMesh.cachedReticles["T72"]);
            reticle_cached.tree = reticleSO;

            ReticleTree.Angular angular = (reticle_cached.tree.planes[0].elements[1] as ReticleTree.Angular);
            ReticleTree.Angular horizontal = angular.elements[0] as ReticleTree.Angular;
            ReticleTree.VerticalBallistic mg_vertical = (angular.elements[2] as ReticleTree.Angular).elements[0] as ReticleTree.VerticalBallistic;
            ReticleTree.Stadia stadia = (angular.elements[1] as ReticleTree.Angular).elements[1] as ReticleTree.Stadia;

            for (int i = 0; i < mg_vertical.elements.Count(); i++)
            {
                ReticleTree.Angular ang = mg_vertical.elements[i] as ReticleTree.Angular;

                if (ang.elements.Count > 1)
                {
                    (ang.elements[0] as ReticleTree.Text).font = Assets.tpd_etch_sdf;
                    (ang.elements[0] as ReticleTree.Text).fontSize = 9f;
                }
            }

            for (int i = 0; i < stadia.elements.Count(); i++)
            {
                ReticleTree.Angular ang = stadia.elements[i] as ReticleTree.Angular;

                (ang.elements[0] as ReticleTree.Text).font = Assets.tpd_etch_sdf;
                (ang.elements[0] as ReticleTree.Text).fontSize = 9f;
            }

            for (int i = 1; i < horizontal.elements.Count(); i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    int idx = Math.Abs(j - 1);
                    int n = 4 * (j + 1) + 8 * ((i - 1) % 4);
                    ReticleTree.Text number = new ReticleTree.Text();
                    number.alignment = TextAlignmentOptions.Center;
                    number.font = Assets.tpd_etch_sdf;
                    number.fontSize = 9f;
                    number.text = n.ToString();
                    number.illumination = ReticleTree.Light.Type.NightIllumination;
                    number.visualType = VisualElement.Type.Painted;
                    number.rotation = new Reticle.AngularLength();
                    number.position.x = (horizontal.elements[i] as ReticleTree.Angular).elements[idx].position.x;
                    number.position.y = 1.25f;
                    (horizontal.elements[i] as ReticleTree.Angular).elements.Add(number);
                }

                for (int k = 2; k <= 3; k++)
                {
                    int n = 8 + 8 * ((i - 1) % 4) - (i >= 5 ? (k == 2 ? 2 : 6) : (k == 2 ? 6 : 2));

                    if (n < 10) continue;

                    ReticleTree.Text number = new ReticleTree.Text();
                    number.alignment = TextAlignmentOptions.Center;
                    number.font = Assets.tpd_etch_sdf;
                    number.fontSize = 9f;
                    number.text = (n).ToString();
                    number.illumination = ReticleTree.Light.Type.NightIllumination;
                    number.visualType = VisualElement.Type.Painted;
                    number.rotation = new Reticle.AngularLength();
                    number.position.x = (horizontal.elements[i] as ReticleTree.Angular).elements[k].position.x;
                    number.position.y = -1.72f;
                    (horizontal.elements[i] as ReticleTree.Angular).elements.Add(number);
                }
            }
        }

        public static void LoadAssets()
        {
            if (assets_loaded) return;

            Reticle();

            lead_readout_canvas = GameObject.Instantiate(Assets.t80b_canvas);
            GameObject.DestroyImmediate(lead_readout_canvas.transform.GetChild(3).gameObject);
            GameObject.DestroyImmediate(lead_readout_canvas.transform.GetChild(2).gameObject);
            GameObject.DestroyImmediate(lead_readout_canvas.transform.GetChild(0).gameObject);
            GameObject.DestroyImmediate(lead_readout_canvas.transform.GetChild(0).GetChild(5).gameObject);
            GameObject.DestroyImmediate(lead_readout_canvas.transform.GetChild(0).GetChild(4).gameObject);
            GameObject.DestroyImmediate(lead_readout_canvas.transform.GetChild(0).GetChild(3).gameObject);
            GameObject.DestroyImmediate(lead_readout_canvas.transform.GetChild(0).GetChild(2).gameObject);
            GameObject.DestroyImmediate(lead_readout_canvas.transform.GetChild(0).GetChild(1).gameObject);
            lead_readout_canvas.SetActive(false);

            assets_loaded = true;
        }
    }
}
