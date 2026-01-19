using System.Collections.Generic;
using System.IO;
using System.Linq;
using GHPC.Camera;
using GHPC.Equipment.Optics;
using GHPC.Vehicle;
using GHPC.Weaponry;
using GHPC.Weapons;
using MelonLoader.Utils;
using Reticle;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using static Reticle.ReticleTree;

namespace PactIncreasedLethality
{
    public class PactThermal
    {
        private static GameObject thermal_canvas;
        private static ReticleSO reticleSO_lq;
        private static ReticleMesh.CachedReticle reticle_cached_lq;

        private static ReticleSO reticleSO_hq;
        private static ReticleMesh.CachedReticle reticle_cached_hq;

        private static ReticleSO reticleSO_tpdk1_hq;
        private static ReticleMesh.CachedReticle reticle_tpdk1_cached_hq;

        private static ReticleSO reticleSO_hq_wide;
        private static ReticleMesh.CachedReticle reticle_cached_hq_wide;

        private static Material white_flir_mat;
        internal static Material white_flir_mat_no_scope;

        private static bool assets_loaded = false;

        public class UpdateRange : MonoBehaviour
        {
            public FireControlSystem fcs;
            ReticleMesh reticle;
            void Awake()
            {
                reticle = GetComponent<UsableOptic>().reticleMesh;
            }

            void Update()
            {
                if (reticle.curReticleRange != fcs.CurrentRange)
                    reticle.targetReticleRange = fcs.CurrentRange;
            }
        }

        private static List<List<Vector3>> borders = new List<List<Vector3>>() {
            new List<Vector3> {new Vector3(0f, -318.7f, 0f), new Vector3(0f, 0f, 180f)},
            new List<Vector3> {new Vector3(0f, 318.7f, 0f), new Vector3(0f, 0f, 0f)},
            new List<Vector3> {new Vector3(330f, 0f, 0f), new Vector3(0f, 0f, 90f)},
            new List<Vector3> {new Vector3(-330f, 0f, 0f), new Vector3(0f, 0f, 270f)}
        };

        public static void Add(UsableOptic optic, string quality, bool is_point_n_shoot = false) {
            //CRTShock.Add(optic.transform, 0f, new Vector3(1f, 1f, 1f));
            optic.slot.VisionType = NightVisionType.Thermal;
            optic.slot.BaseBlur = 0f;
            optic.post = null;

            GameObject post = GameObject.Instantiate(Assets.flir_post_green, optic.transform);
            PostProcessProfile profile = post.transform.Find("FLIR Only Volume").GetComponent<PostProcessVolume>().profile;

            optic.slot.FLIRBlitMaterialOverride = white_flir_mat;

            if (quality == "low") {
                optic.Alignment = OpticAlignment.BoresightStabilized;
                optic.slot.OverrideFLIRResolution = true;
                optic.slot.FLIRWidth = 200;
                optic.slot.FLIRHeight = 200;

                for (int i = 0; i <= 3; i++)
                {
                    GameObject t = GameObject.Instantiate(thermal_canvas, optic.transform);
                    t.transform.GetChild(0).localPosition = borders[i][0];
                    t.transform.GetChild(0).localEulerAngles = borders[i][1];
                    if (i == 2 || i == 3)
                        t.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
                    t.SetActive(true);
                }
            }

            if (quality == "high")
            {
                if (!is_point_n_shoot)
                {
                    optic.reticleMesh.maxSpeed = 1000f;

                    UpdateRange ur = optic.gameObject.AddComponent<UpdateRange>();
                    ur.fcs = optic.FCS;
                }

                optic.slot.OverrideFLIRResolution = true;
                optic.slot.FLIRWidth = 800;
                optic.slot.FLIRHeight = 450;
                optic.slot.CanToggleFlirPolarity = true;

                GameObject wide = GameObject.Instantiate(optic.reticleMesh.gameObject, optic.transform);
                wide.gameObject.SetActive(true);
                ReticleMesh wide_reticle_mesh = wide.GetComponent<ReticleMesh>();
                wide_reticle_mesh.reticleSO = reticleSO_hq_wide;
                wide_reticle_mesh.reticle = reticle_cached_hq_wide;
                wide_reticle_mesh.SMR = null;
                wide_reticle_mesh.Load();

                UsableOptic.FovLimitedItem wide_lim = new UsableOptic.FovLimitedItem();
                wide_lim.FovRange = new Vector2(7f, 360f);
                wide_lim.ExclusiveObjects = new GameObject[] { wide };
                UsableOptic.FovLimitedItem zoomed_lim = new UsableOptic.FovLimitedItem();
                zoomed_lim.FovRange = new Vector2(0f, 7f);
                zoomed_lim.ExclusiveObjects = new GameObject[] { optic.reticleMesh.gameObject };

                optic._reticleMeshLocalPositions = new Vector2[] { Vector3.zero, Vector3.zero };
                optic.FovLimitedItems = new UsableOptic.FovLimitedItem[] { wide_lim, zoomed_lim };
                optic.AdditionalReticleMeshes = new ReticleMesh[] { wide_reticle_mesh };
                optic.Alignment = OpticAlignment.BoresightStabilized;

                optic.slot.DefaultFov = 9.5f;
                optic.slot.OtherFovs = new float[1] { 4.04f };
                optic.slot.VibrationBlurScale = 0.05f;
                optic.slot.VibrationShakeMultiplier = 0.01f;
                optic.slot.VibrationPreBlur = true;
            }

            ReticleSO so_hq = is_point_n_shoot ? reticleSO_hq : reticleSO_tpdk1_hq;
            ReticleMesh.CachedReticle cached_hq = is_point_n_shoot ? reticle_cached_hq : reticle_tpdk1_cached_hq;

            optic.reticleMesh.reticleSO = quality == "low" ? reticleSO_lq : so_hq;
            optic.reticleMesh.reticle = quality == "low" ? reticle_cached_lq : cached_hq;
            optic.reticleMesh.SMR = null;
            optic.reticleMesh.Load();
        }

        private static void LQThermalReticle() {
            reticleSO_lq = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55-NVS"].tree);
            reticleSO_lq.name = "PACT-TIS-LQ";

            Util.ShallowCopy(reticle_cached_lq, ReticleMesh.cachedReticles["T55-NVS"]);
            reticle_cached_lq.tree = reticleSO_lq;

            reticle_cached_lq.tree.lights = new List<ReticleTree.Light>() {
                new ReticleTree.Light()
            };

            reticle_cached_lq.tree.lights[0].type = ReticleTree.Light.Type.Powered;
            reticle_cached_lq.tree.lights[0].color = new RGB(1f, 1f, 1f, true);

            ReticleTree.Angular reticle_lq = (reticleSO_lq.planes[0].elements[0] as ReticleTree.Angular).elements[0] as ReticleTree.Angular;
            (reticleSO_lq.planes[0].elements[0] as ReticleTree.Angular).align = ReticleTree.GroupBase.Alignment.Boresight;
            reticle_lq.align = ReticleTree.GroupBase.Alignment.Boresight;
            reticle_cached_lq.mesh = null;

            reticle_lq.elements.RemoveAt(4);
            reticle_lq.elements.RemoveAt(1);
            reticle_lq.elements.RemoveAt(0);

            ReticleTree.Line line1 = reticle_lq.elements[0] as ReticleTree.Line;
            line1.rotation.mrad = 0;
            line1.position.x = 0;
            line1.position.y = 0;
            line1.length.mrad = 22.0944f;
            line1.illumination = ReticleTree.Light.Type.Powered;

            ReticleTree.Line line2 = reticle_lq.elements[1] as ReticleTree.Line;
            line2.position.y = 0;
            line2.length.mrad = 9.0944f;
            line2.illumination = ReticleTree.Light.Type.Powered;
        }

        private static void HQThermalReticle()
        {
            reticleSO_tpdk1_hq = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["TPN3"].tree);
            reticleSO_tpdk1_hq.name = "PACT-TPDK1-TIS-HQ";

            Util.ShallowCopy(reticle_tpdk1_cached_hq, ReticleMesh.cachedReticles["TPN3"]);
            reticle_tpdk1_cached_hq.tree = reticleSO_tpdk1_hq;
            reticle_tpdk1_cached_hq.mesh = null;

            reticle_tpdk1_cached_hq.tree.lights = new List<ReticleTree.Light>() {
                new ReticleTree.Light(),
                new ReticleTree.Light(),
            };

            reticle_tpdk1_cached_hq.tree.lights[0].type = ReticleTree.Light.Type.Powered;
            reticle_tpdk1_cached_hq.tree.lights[0].color = new RGB(2.8f, 3f, 2.8f, true);

            reticle_tpdk1_cached_hq.tree.lights[1].type = ReticleTree.Light.Type.LaserReady;
            reticle_tpdk1_cached_hq.tree.lights[1].color = new RGB(2.8f, 3f, 2.8f, true);

            ReticleTree.Angular lase_point = new ReticleTree.Angular(Vector2.zero, null);
            lase_point.align = GroupBase.Alignment.LasePoint;

            ReticleTree.Circle lase_circle = new ReticleTree.Circle();
            lase_circle.segments = 32;
            lase_circle.radius = 0.5f;
            lase_circle.illumination = ReticleTree.Light.Type.Powered;
            lase_circle.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;
            lase_circle.position = new Position(0f, 0f);
            lase_circle.thickness.mrad = 0.05f;
            lase_point.elements.Add(lase_circle);

            reticleSO_tpdk1_hq.planes[0].elements.Add(lase_point);

            ////////////////////////////////

            reticleSO_hq = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55-NVS"].tree);
            reticleSO_hq.name = "PACT-TIS-HQ";

            Util.ShallowCopy(reticle_cached_hq, ReticleMesh.cachedReticles["T55-NVS"]);
            reticle_cached_hq.tree = reticleSO_hq;

            reticle_cached_hq.tree.lights = new List<ReticleTree.Light>() {
                new ReticleTree.Light()
            };

            reticle_cached_hq.tree.lights[0].type = ReticleTree.Light.Type.Powered;
            reticle_cached_hq.tree.lights[0].color = new RGB(2.8f, 3f, 2.8f, true);

            ReticleTree.Angular reticle_hq = (reticleSO_hq.planes[0].elements[0] as ReticleTree.Angular).elements[0] as ReticleTree.Angular;
            (reticleSO_hq.planes[0].elements[0] as ReticleTree.Angular).align = ReticleTree.GroupBase.Alignment.Impact;
            reticle_hq.align = ReticleTree.GroupBase.Alignment.Impact;
            reticle_cached_hq.mesh = null;

            reticle_hq.elements.RemoveAt(4);
            reticle_hq.elements.RemoveAt(1);
            reticle_hq.elements.RemoveAt(0);

            ReticleTree.Line line1 = reticle_hq.elements[0] as ReticleTree.Line;
            line1.rotation.mrad = 0;
            line1.position.x = -8f;
            line1.position.y = 0;
            line1.length.mrad = 15.0944f;
            line1.thickness.mrad /= 1.7f;
            line1.illumination = ReticleTree.Light.Type.Powered;
            line1.visualType = ReticleTree.VisualElement.Type.Painted;

            ReticleTree.Line line2 = reticle_hq.elements[1] as ReticleTree.Line;
            line2.position.y = 5.5f;
            line2.length.mrad = 10.0944f;
            line2.thickness.mrad /= 1.7f;
            line2.illumination = ReticleTree.Light.Type.Powered;
            line2.visualType = ReticleTree.VisualElement.Type.Painted;

            ReticleTree.Line line3 = new ReticleTree.Line();
            line3.roundness = line1.roundness;
            line3.thickness.mrad = line1.thickness.mrad;
            line3.length.mrad = line1.length.mrad;
            line3.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
            line3.length.unit = AngularLength.AngularUnit.MIL_USSR;
            line3.rotation.mrad = 0f;
            line3.position = new ReticleTree.Position(8f, 0, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
            line3.visualType = ReticleTree.VisualElement.Type.Painted;
            line3.illumination = ReticleTree.Light.Type.Powered;

            ReticleTree.Line line4 = new ReticleTree.Line();
            line4.roundness = line2.roundness;
            line4.thickness.mrad = line2.thickness.mrad;
            line4.length.mrad = line2.length.mrad;
            line4.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
            line4.length.unit = AngularLength.AngularUnit.MIL_USSR;
            line4.rotation.mrad = line2.rotation.mrad;
            line4.position = new ReticleTree.Position(0f, -5.5f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
            line4.visualType = ReticleTree.VisualElement.Type.Painted;
            line4.illumination = ReticleTree.Light.Type.Powered;

            ReticleTree.Line centre = new ReticleTree.Line();
            centre.roundness = 0f;
            centre.thickness.mrad = 0.2f;
            centre.length.mrad = 0.2f;
            centre.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
            centre.rotation.mrad = line2.rotation.mrad;
            centre.position = new ReticleTree.Position(0f, 0f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
            centre.visualType = ReticleTree.VisualElement.Type.Painted;
            centre.illumination = ReticleTree.Light.Type.Powered;

            reticle_hq.elements.Add(line3);
            reticle_hq.elements.Add(line4);

            ReticleTree.Circle circle = new ReticleTree.Circle();
            circle.position = new ReticleTree.Position(0f, -0.6f);
            circle.radius = 0.6f;
            circle.segments = 3;
            circle.thickness.mrad = line2.thickness.mrad;
            circle.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
            circle.visualType = ReticleTree.VisualElement.Type.Painted;
            circle.illumination = ReticleTree.Light.Type.Powered;
            
            reticleSO_hq_wide = ScriptableObject.Instantiate(reticleSO_hq);
            reticleSO_hq_wide.name = "PACT-TIS-HQ-WIDE";
            Util.ShallowCopy(reticle_cached_hq_wide, reticle_cached_hq);
            reticle_cached_hq_wide.tree = reticleSO_hq_wide;
            reticle_cached_hq_wide.mesh = null;

            ReticleTree.Angular reticle_hq_wide = (reticleSO_hq_wide.planes[0].elements[0] as ReticleTree.Angular).elements[0] as ReticleTree.Angular;
            reticle_hq_wide.elements.RemoveRange(0, reticle_hq_wide.elements.Count);

            ReticleTree.Line l1 = new ReticleTree.Line();
            l1.length = new AngularLength(3f, unit: AngularLength.AngularUnit.MIL_USSR);
            l1.roundness = 1f;
            l1.thickness = new AngularLength(0.4f, AngularLength.AngularUnit.MIL_USSR);
            l1.illumination = ReticleTree.Light.Type.Powered;
            l1.visualType = VisualElement.Type.ReflectedAdditive;
            l1.position = new ReticleTree.Position(angUnit: AngularLength.AngularUnit.MIL_NATO, linUnit: LinearLength.LinearUnit.M, x: -0.7461f, y: -1.237f);
            l1.rotation = new AngularLength(240f, unit: AngularLength.AngularUnit.DEG);

            ReticleTree.Line l2 = new ReticleTree.Line();
            l2.length = new AngularLength(3f, unit: AngularLength.AngularUnit.MIL_USSR);
            l2.roundness = 1f;
            l2.thickness = new AngularLength(0.4f, AngularLength.AngularUnit.MIL_USSR);
            l2.illumination = ReticleTree.Light.Type.Powered;
            l2.visualType = VisualElement.Type.ReflectedAdditive;
            l2.position = new ReticleTree.Position(angUnit: AngularLength.AngularUnit.MIL_NATO, linUnit: LinearLength.LinearUnit.M, x: 0.7461f, y: -1.237f);
            l2.rotation = new AngularLength(300f, unit: AngularLength.AngularUnit.DEG);

            reticle_hq_wide.elements.Add(l1);
            reticle_hq_wide.elements.Add(l2);
        }

        public static void LoadAssets() 
        {
            if (assets_loaded) return;

            AssetBundle thermal_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "pilthermals"));
            Texture colour_ramp_white = thermal_bundle.LoadAsset<Texture>("FLIR White");

            white_flir_mat = new Material(Shader.Find("Blit (FLIR)/Blit Simple"));
            white_flir_mat.SetTexture("_Noise", Assets.green_flir_mat.GetTexture("_Noise"));
            white_flir_mat.SetTexture("_ScopeEdge", Assets.green_flir_mat.GetTexture("_ScopeEdge"));
            white_flir_mat.SetTexture("_ColorRamp", colour_ramp_white);
            white_flir_mat.EnableKeyword("_USE_COLOR_RAMP");
            white_flir_mat.EnableKeyword("_TONEMAP");
            white_flir_mat.EnableKeyword("_FLIR_POLARITY");

            white_flir_mat_no_scope = new Material(Shader.Find("Blit (FLIR)/Blit Simple"));
            white_flir_mat_no_scope.SetTexture("_Noise", Assets.green_flir_mat.GetTexture("_Noise"));
            white_flir_mat_no_scope.SetTexture("_ColorRamp", colour_ramp_white);
            white_flir_mat_no_scope.EnableKeyword("_USE_COLOR_RAMP");
            white_flir_mat_no_scope.EnableKeyword("_TONEMAP");
            white_flir_mat_no_scope.EnableKeyword("_FLIR_POLARITY");

            thermal_canvas = GameObject.Instantiate(Assets.m2_bradley_canvas);
            GameObject.Destroy(thermal_canvas.transform.GetChild(2).gameObject);
            thermal_canvas.SetActive(false);
            thermal_canvas.hideFlags = HideFlags.DontUnloadUnusedAsset;
            thermal_canvas.name = "pact thermal canvas";

            LQThermalReticle();
            HQThermalReticle();

            assets_loaded = true;
        }
    }
}
