using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Equipment.Optics;
using GHPC.UI.Tips;
using GHPC.Utility;
using GHPC.Vehicle;
using MelonLoader;
using Reticle;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static Reticle.ReticleTree;

namespace PactIncreasedLethality
{
    public class PactThermal
    {
        private static GameObject thermal_canvas;
        private static GameObject scanline_canvas;
        private static ReticleSO reticleSO_lq;
        private static ReticleMesh.CachedReticle reticle_cached_lq;
        private static ReticleSO reticleSO_hq;
        private static ReticleMesh.CachedReticle reticle_cached_hq;

        private static ReticleSO reticleSO_hq_wide;
        private static ReticleMesh.CachedReticle reticle_cached_hq_wide;
        private static PostProcessVolume post_og; 
        private static PostProcessVolume post_lq;
        private static PostProcessVolume post_hq;
        private static TMP_FontAsset tpd_etch_sdf;
        private static AmmoCodexScriptable ammo_3bk14m;

        static MelonPreferences_Entry<float> lq_blur;
        static MelonPreferences_Entry<float> hq_blur;

        static MelonPreferences_Entry<bool> lq_boxing;
        static MelonPreferences_Entry<bool> hq_boxing;

        public static void Config(MelonPreferences_Category cfg)
        {
            lq_blur = cfg.CreateEntry<float>("Low Quality Thermals Blur", 0.30f);
            lq_blur.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            lq_blur.Comment = "Default: 0.30";

            hq_blur = cfg.CreateEntry<float>("High Quality Thermals Blur", 0.15f);
            hq_blur.Comment = "Default: 0.15";

            lq_boxing = cfg.CreateEntry<bool>("Low Quality Thermals Boxing", true);
            lq_boxing.Comment = "Creates a box border around the sight";

            hq_boxing = cfg.CreateEntry<bool>("High Quality Thermals Boxing", true);
        }

        private static List<List<Vector3>> borders = new List<List<Vector3>>() {
            new List<Vector3> {new Vector3(0f, -318.7f, 0f), new Vector3(0f, 0f, 180f)},
            new List<Vector3> {new Vector3(0f, 318.7f, 0f), new Vector3(0f, 0f, 0f)},
            new List<Vector3> {new Vector3(330f, 0f, 0f), new Vector3(0f, 0f, 90f)},
            new List<Vector3> {new Vector3(-330f, 0f, 0f), new Vector3(0f, 0f, 270f)}
        };

        public static void Add(UsableOptic optic, string quality) {
            optic.slot.VisionType = NightVisionType.Thermal;
            optic.slot.BaseBlur = quality == "low" ? lq_blur.Value : hq_blur.Value;
            PostProcessVolume vol = PostProcessVolume.Instantiate(quality == "low" ? post_lq : post_hq, optic.transform);
            vol.gameObject.SetActive(true);
            optic.post = vol;

            if (quality == "low") {
                GameObject s = GameObject.Instantiate(scanline_canvas, optic.transform);
                s.SetActive(true);
            }

            if ((quality == "low" && lq_boxing.Value) || (quality == "high" && hq_boxing.Value))
            {
                for (int i = 0; i <= 3; i++)
                {
                    if (quality == "high" && (i == 2 || i == 3)) continue;
                    GameObject t = GameObject.Instantiate(thermal_canvas, optic.transform);
                    t.transform.GetChild(0).localPosition = borders[i][0];
                    t.transform.GetChild(0).localEulerAngles = borders[i][1];
                    if (i == 2 || i == 3)
                        t.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
                    t.SetActive(true);
                }
            }

            optic.reticleMesh.reticleSO = quality == "low" ? reticleSO_lq : reticleSO_hq;
            optic.reticleMesh.reticle = quality == "low" ? reticle_cached_lq : reticle_cached_hq;
            optic.reticleMesh.SMR = null;
            optic.reticleMesh.Load();

            if (quality == "high")
            {
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

                optic.slot.DefaultFov = 8.5f;
                optic.slot.OtherFovs = new float[1] { 3.8f };
                optic.slot.VibrationBlurScale = 0.05f;
                optic.slot.VibrationShakeMultiplier = 0.01f;
                optic.slot.VibrationPreBlur = true;
                optic.slot.SpriteType = GHPC.Camera.CameraSpriteManager.SpriteType.NightVisionGoggles;
            }
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
            reticle_cached_lq.tree.lights[0].color = new RGB(3f, -0.35f, -0.35f, true);

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
            line1.length.mrad = 10.0944f;
            line1.illumination = ReticleTree.Light.Type.Powered;

            ReticleTree.Line line2 = reticle_lq.elements[1] as ReticleTree.Line;
            line2.position.y = 0;
            line2.length.mrad = 4.0944f;
            line2.illumination = ReticleTree.Light.Type.Powered;
        }

        private static void HQThermalReticle()
        {
            reticleSO_hq = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55-NVS"].tree);
            reticleSO_hq.name = "PACT-TIS-HQ";

            Util.ShallowCopy(reticle_cached_hq, ReticleMesh.cachedReticles["T55-NVS"]);
            reticle_cached_hq.tree = reticleSO_hq;

            reticle_cached_hq.tree.lights = new List<ReticleTree.Light>() {
                new ReticleTree.Light()
            };

            reticle_cached_hq.tree.lights[0].type = ReticleTree.Light.Type.Powered;
            //reticle_cached_hq.tree.lights[0].color = new RGB(1.5f, -0.5f, -0.3f, true);
            //reticle_cached_hq.tree.lights[0].color = new RGB(5f, 0.1f, 0.1f, false);
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
            line1.position.x = -9;
            line1.position.y = 0;
            line1.length.mrad = 15.0944f;
            line1.thickness.mrad /= 1.7f;
            line1.illumination = ReticleTree.Light.Type.Powered;
            line1.visualType = ReticleTree.VisualElement.Type.Painted;

            ReticleTree.Line line2 = reticle_hq.elements[1] as ReticleTree.Line;
            line2.position.y = 6;
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
            line3.position = new ReticleTree.Position(9f, 0, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
            line3.visualType = ReticleTree.VisualElement.Type.Painted;
            line3.illumination = ReticleTree.Light.Type.Powered;

            ReticleTree.Line line4 = new ReticleTree.Line();
            line4.roundness = line2.roundness;
            line4.thickness.mrad = line2.thickness.mrad;
            line4.length.mrad = line2.length.mrad;
            line4.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
            line4.length.unit = AngularLength.AngularUnit.MIL_USSR;
            line4.rotation.mrad = line2.rotation.mrad;
            line4.position = new ReticleTree.Position(0f, -6f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
            line4.visualType = ReticleTree.VisualElement.Type.Painted;
            line4.illumination = ReticleTree.Light.Type.Powered;

            List<Vector3> box_pos = new List<Vector3>() {
                new Vector3(0, -13.344f),
                new Vector3(0, 13.344f),
                new Vector3(18.654f, 0),
                new Vector3(-18.654f,0),
            };

            foreach (Vector3 pos in box_pos) {
                ReticleTree.Line box = new ReticleTree.Line();
                box.roundness = 0f;
                box.thickness.mrad = line2.thickness.mrad * 2.8f;
                box.length.mrad = 8f;
                box.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                box.length.unit = AngularLength.AngularUnit.MIL_USSR;    
                box.rotation.mrad = pos.x == 0 ? line2.rotation.mrad : line1.rotation.mrad;
     
                box.position = new ReticleTree.Position(pos.x, pos.y, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
                box.visualType = ReticleTree.VisualElement.Type.Painted;
                box.illumination = ReticleTree.Light.Type.Powered;

                reticle_hq.elements.Add(box);
            }

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
            //reticle_hq.elements.Add(centre);

            for (float i = 0f; i < 0.6f; i += 0.05f)
            {
                ReticleTree.Circle circle = new ReticleTree.Circle();
                circle.position = new ReticleTree.Position(0f, -i);
                circle.radius = i;
                circle.segments = 3;
                circle.thickness.mrad = line2.thickness.mrad;
                //circle.rotation.mrad = 785.398f;
                circle.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                circle.visualType = ReticleTree.VisualElement.Type.Painted;
                circle.illumination = ReticleTree.Light.Type.Powered;
                reticle_hq.elements.Add(circle);
            }

            ReticleTree.VerticalBallistic markings_sabot = new ReticleTree.VerticalBallistic(new Vector2(0f, 0f), null);
            markings_sabot.projectile = APFSDS_125mm.clip_codex_3bm32.ClipType.MinimalPattern[0];
            markings_sabot.UpdateBC();

            ReticleTree.Angular sabot_holder = new ReticleTree.Angular(new Vector2(2f, -11f), null);
            ReticleTree.Text sabot = new ReticleTree.Text();
            sabot.alignment = TextAlignmentOptions.Center;
            sabot.font = tpd_etch_sdf;
            sabot.fontSize = 11f;
            sabot.text = "B";
            sabot.illumination = ReticleTree.Light.Type.Powered;
            sabot.visualType = VisualElement.Type.Painted;
            sabot.rotation = new Reticle.AngularLength();
            sabot_holder.elements.Add(sabot);
            reticle_hq.elements.Add(sabot_holder);

            List<float> ranges = new List<float>() {800f, 1500f, 2000f, 2500f, 3000f, 3500f};
            for(int i = 0; i < ranges.Count; i++) {
                float range = ranges[i];
                ReticleTree.Angular angular = new ReticleTree.Angular(new Vector3(0f, range), markings_sabot);
                angular.name = range + "M";

                ReticleTree.Text text = new ReticleTree.Text();
                text.alignment = TextAlignmentOptions.Center;
                text.font = tpd_etch_sdf;
                text.fontSize = 7f;
                text.text = ((int)ranges[i] / 100).ToString();
                text.illumination = ReticleTree.Light.Type.Powered;
                text.visualType = VisualElement.Type.Painted;
                text.position = new Position(i % 2 == 0 ? 2.4f : 4f, 0f);
                text.rotation = new Reticle.AngularLength();

                ReticleTree.Line line = new ReticleTree.Line();
                line.roundness = line1.roundness;
                line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                line.length.unit = AngularLength.AngularUnit.MIL_USSR;
                line.thickness.mrad = line1.thickness.mrad / 2f;
                line.length.mrad = i % 2 == 0 ? 1.5f : 2.5f;
                line.position = new ReticleTree.Position(i % 2 == 0 ? 0.8f : 1.18f, 0f, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                line.visualType = ReticleTree.VisualElement.Type.Painted;
                line.illumination = ReticleTree.Light.Type.Powered;
                angular.elements.Add(text);
                angular.elements.Add(line);
                markings_sabot.elements.Add(angular);
            }

            reticle_hq.elements.Add(markings_sabot);
            
            ReticleTree.VerticalBallistic markings_heat = new ReticleTree.VerticalBallistic(new Vector2(0f, 0f), null);
            markings_heat.projectile = ammo_3bk14m;
            markings_heat.UpdateBC();

            ReticleTree.Angular heat_holder = new ReticleTree.Angular(new Vector2(-2f, -11f), null);
            ReticleTree.Text heat = new ReticleTree.Text();
            heat.alignment = TextAlignmentOptions.Center;
            heat.font = tpd_etch_sdf;
            heat.fontSize = 11f;
            heat.text = "K";
            heat.illumination = ReticleTree.Light.Type.Powered;
            heat.visualType = VisualElement.Type.Painted;
            heat.rotation = new Reticle.AngularLength();
            heat_holder.elements.Add(heat);
            reticle_hq.elements.Add(heat_holder);

            List<float> hranges = new List<float>() { 300f, 500f, 700f, 800f, 1000f, 1200f };
            for (int i = 0; i < hranges.Count; i++)
            {
                float range = hranges[i];
                ReticleTree.Angular angular = new ReticleTree.Angular(new Vector3(0f, range), markings_heat);
                angular.name = range + "M";

                ReticleTree.Text text = new ReticleTree.Text();
                text.alignment = TextAlignmentOptions.Center;
                text.font = tpd_etch_sdf;
                text.fontSize = 7f;
                text.text = ((int)hranges[i] / 100).ToString();
                text.illumination = ReticleTree.Light.Type.Powered;
                text.visualType = VisualElement.Type.Painted;
                text.position = new Position(i % 2 == 0 ? -2.4f : -4f, 0f);
                text.rotation = new Reticle.AngularLength();

                ReticleTree.Line line = new ReticleTree.Line();
                line.roundness = line1.roundness;
                line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                line.length.unit = AngularLength.AngularUnit.MIL_USSR;
                line.thickness.mrad = line1.thickness.mrad / 2f;
                line.length.mrad = i % 2 == 0 ? 1.5f : 2.5f;
                line.position = new ReticleTree.Position(i % 2 == 0 ? -0.8f : -1.18f, 0f, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                line.visualType = ReticleTree.VisualElement.Type.Painted;
                line.illumination = ReticleTree.Light.Type.Powered;
                angular.elements.Add(text);
                angular.elements.Add(line);
                markings_heat.elements.Add(angular);
            }

            reticle_hq.elements.Add(markings_heat);

            reticleSO_hq_wide = ScriptableObject.Instantiate(reticleSO_hq);
            reticleSO_hq_wide.name = "PACT-TIS-HQ-WIDE";
            Util.ShallowCopy(reticle_cached_hq_wide, reticle_cached_hq);
            reticle_cached_hq_wide.tree = reticleSO_hq_wide;
            reticle_cached_hq_wide.mesh = null;

            ReticleTree.Angular reticle_hq_wide = (reticleSO_hq_wide.planes[0].elements[0] as ReticleTree.Angular).elements[0] as ReticleTree.Angular;
            reticle_hq_wide.elements.RemoveRange(8, reticle_hq_wide.elements.Count - 8);

            foreach (int i in new int[] {0, 1, 6, 7})
            {
                ReticleTree.Line line = reticle_hq_wide.elements[i] as ReticleTree.Line;

                if (i == 0 || i == 6) {
                    line.rotation.mrad = 1570.8f;
                    line.position.x = Math.Sign(line.position.x) * 18.654f - Math.Sign(line.position.x) * 4f;
                    continue;
                }

                line.position.y = Math.Sign(line.position.y) * 13.344f - Math.Sign(line.position.y) * 4f;
                line.rotation.mrad = 0f;
            }

            reticle_hq.elements.RemoveAt(1);
        }

        public static void Init() {
            if (reticleSO_lq != null) return;

            foreach (Vehicle obj in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
            {
                if (!ReticleMesh.cachedReticles.ContainsKey("T55-NVS") && obj.gameObject.name == "T55A")
                {
                    UsableOptic night_optic = obj.transform.Find("Gun Scripts/Sights (and FCS)/NVS").GetComponent<UsableOptic>();
                    night_optic.reticleMesh.Load();
                }

                if (post_og == null && obj.name == "T72M1")
                {
                    post_og = obj.transform.Find("---MAIN GUN SCRIPTS---/2A46/TPN-1-49-23 night sight").GetComponent<UsableOptic>().post;
                }

                if (thermal_canvas == null && obj.name == "M2 Bradley")
                {
                    thermal_canvas = GameObject.Instantiate(obj.transform.Find("FCS and sights/GPS Optic/M2 Bradley GPS canvas").gameObject);
                    GameObject.Destroy(thermal_canvas.transform.GetChild(2).gameObject);
                    thermal_canvas.SetActive(false);
                    thermal_canvas.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    thermal_canvas.name = "pact thermal canvas";
                }

                if (scanline_canvas == null && obj.name == "M60A3")
                {
                    scanline_canvas = GameObject.Instantiate(obj.transform.Find("Turret Scripts/Sights/FLIR/Canvas Scanlines").gameObject);
                    scanline_canvas.SetActive(false);
                    scanline_canvas.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    scanline_canvas.name = "pact scanline canvas";
                }

                if (scanline_canvas && thermal_canvas && post_og && ReticleMesh.cachedReticles.ContainsKey("T55-NVS")) break; 
            }

            foreach (TMP_FontAsset font in Resources.FindObjectsOfTypeAll(typeof(TMP_FontAsset))) {
                if (font.name == "TPD_Etch SDF") {
                    tpd_etch_sdf = font;
                    break;
                }
            }

            post_lq = PostProcessVolume.Instantiate(post_og);
            ColorGrading color_grading = post_lq.profile.settings[1] as ColorGrading;
            color_grading.postExposure.value = 2f;
            color_grading.colorFilter.value = new Color(0.70f, 0.75f, 0.70f);
            color_grading.lift.value = new Vector4(0f, 0f, 0f, -1.2f);
            color_grading.lift.overrideState = true;
            post_lq.sharedProfile = post_lq.profile;
            post_lq.gameObject.SetActive(false);

            post_hq = PostProcessVolume.Instantiate(post_og);
            color_grading = post_hq.profile.settings[1] as ColorGrading;
            color_grading.postExposure.value = 0f;
            color_grading.contrast.value = 100f;
            color_grading.colorFilter.value = new Color(0.65f, 0.90f, 0.65f);
           //color_grading.lift.value = new Vector4(0f, 0f, 0f, -1.2f);
            color_grading.lift.overrideState = false;
            (post_hq.profile.settings[2] as Grain).intensity.value = 0.111f;
            (post_hq.profile.settings[0] as Bloom).intensity.value = 1;
            post_hq.sharedProfile = post_hq.profile;

            post_hq.gameObject.SetActive(false);

            foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
            {
                if (s.AmmoType.Name == "3BK14M HEAT-FS-T") { ammo_3bk14m = s; }

                if (ammo_3bk14m != null) break;
            }

            LQThermalReticle();
            HQThermalReticle();
        }
    }
}
