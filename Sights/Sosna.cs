using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GHPC.Camera;
using GHPC.Equipment.Optics;
using GHPC.Player;
using GHPC.State;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using MelonLoader.Utils;
using Reticle;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

namespace PactIncreasedLethality
{
    public class Sosna
    {
        static GameObject range_readout;
        static GameObject thermal_canvas;

        static GameObject sosna_monitor; 

        static ReticleSO reticleSO_sosna;
        static ReticleMesh.CachedReticle reticle_cached_sosna;

        private static ReticleSO reticleSO_sosna_thermal;
        private static ReticleMesh.CachedReticle reticle_cached_sosna_thermal;

        private static ReticleSO reticleSO_sosna_thermal_wide;
        private static ReticleMesh.CachedReticle reticle_cached_sosna_thermal_wide;

        private static PostProcessVolume post_sosna;

        private class HasThermalMonitor : MonoBehaviour {}

        public class ThermalMonitor : MonoBehaviour {
            public RenderTexture monitor_texture;
            GameObject monitor_screen;
            Camera cam;
            GameObject scope;

            Transform crosshair_ui;
            Transform wfov_ui;
            Transform ready;
            Transform cmdr_override;
            Transform crosshairs;
            Transform wfov;
            Transform stab;
            Transform apfsds;
            Transform heat;
            Transform pk;
            Transform he;
            Transform atgm;
            static public Transform tracking_gates;
            TextMeshProUGUI range;

            void Awake() {
                cam = CameraManager.MainCam;
                scope = cam.transform.Find("Scope").gameObject;

                monitor_screen = transform.Find("MONITOR SCREEN").gameObject;

                crosshair_ui = monitor_screen.transform.Find("CROSSHAIR UI");
                wfov_ui = monitor_screen.transform.Find("WFOV UI");

                ready = crosshair_ui.Find("READY");
                cmdr_override = crosshair_ui.Find("CMDR CONTROL");
                range = crosshair_ui.Find("RANGE").GetComponentInChildren<TextMeshProUGUI>();
                apfsds = crosshair_ui.Find("AMMO (AP)");
                atgm = crosshair_ui.Find("AMMO (ATGM)");
                heat = crosshair_ui.Find("AMMO (HEAT)");
                he = crosshair_ui.Find("AMMO (HE)");
                pk = crosshair_ui.Find("AMMO (COAX)");

                tracking_gates = crosshair_ui.Find("TRACKING GATE HOLDER");

                stab = wfov_ui.Find("STAB");

                wfov = monitor_screen.transform.Find("WFOV");
                crosshairs = monitor_screen.transform.Find("CROSSHAIR");
            }

            void LateUpdate()
            {
                FireControlSystem fcs = PlayerInput.Instance.CurrentPlayerWeapon.FCS;
                UsableOptic optic = PlayerInput.Instance.CurrentPlayerWeapon.FCS.MainOptic;

                if (!optic || (!optic.GetComponent<HasThermalMonitor>() || !optic.gameObject.activeSelf))
                {
                    cam.targetTexture = null;
                    monitor_screen.SetActive(false);
                    scope.gameObject.SetActive(true);
                    return;
                } else if (cam.targetTexture == null)
                {
                    scope.gameObject.SetActive(false);
                    cam.targetTexture = monitor_texture;
                    monitor_screen.SetActive(true);
                }

                crosshair_ui.gameObject.SetActive(fcs.NightOptic.slot.CurrentFov < 10.52f);
                crosshairs.gameObject.SetActive(fcs.NightOptic.slot.CurrentFov < 10.52f);
                ready.gameObject.SetActive(fcs.CurrentWeaponSystem.AbleToFire);
                cmdr_override.gameObject.SetActive(fcs.IsOverridingNow);
                tracking_gates.gameObject.SetActive(fcs.GetComponent<LockOnLead>().target != null);
                range.text = ((int)MathUtil.RoundFloatToMultipleOf(fcs.CurrentRange, 5)).ToString("0000");

                if (fcs.NightOptic.slot.CurrentFov == 2.95f)
                {
                    crosshairs.localScale = new Vector3(0.4f, 0.4f, 1f);
                }
                else
                {
                    crosshairs.localScale = new Vector3(0.3f, 0.3f, 1f);
                }

                apfsds.gameObject.SetActive(fcs.CurrentAmmoType.Category == AmmoType.AmmoCategory.Penetrator);
                heat.gameObject.SetActive(fcs.CurrentAmmoType.Category == AmmoType.AmmoCategory.ShapedCharge && fcs.CurrentAmmoType.Guidance == AmmoType.GuidanceType.Unguided);
                he.gameObject.SetActive(fcs.CurrentAmmoType.Category == AmmoType.AmmoCategory.Explosive);
                atgm.gameObject.SetActive(fcs.CurrentAmmoType.Guidance > AmmoType.GuidanceType.Unguided);
                pk.gameObject.SetActive(fcs.CurrentWeaponSystem.MetaName == "Coaxial MG");

                wfov_ui.gameObject.SetActive(fcs.NightOptic.slot.CurrentFov == 10.52f);
                wfov.gameObject.SetActive(fcs.NightOptic.slot.CurrentFov == 10.52f);
                stab.gameObject.SetActive(fcs.StabsActive);
            }
        }

        private static IEnumerator AddThermalMonitor(GameState _) {
            Camera cam = CameraManager.MainCam;

            if (PlayerInput.Instance.transform.Find("UIHUDCanvas").parent.Find("MONITOR CANVAS(Clone)")) yield break;

            GameObject monitor_canvas = GameObject.Instantiate(sosna_monitor);

            ThermalMonitor monitor = monitor_canvas.AddComponent<ThermalMonitor>();
            monitor.enabled = true;
            monitor.monitor_texture = (RenderTexture)monitor_canvas.GetComponentInChildren<RawImage>().mainTexture;

            Transform monitor_screen = monitor_canvas.transform.Find("MONITOR SCREEN");
            monitor_canvas.transform.parent = PlayerInput.Instance.transform.Find("UIHUDCanvas").parent;
            monitor_canvas.transform.SetAsFirstSibling();

            yield break;
        }

        public static void Add(UsableOptic day_optic, UsableOptic night_optic, WeaponSystemInfo coax, bool thermal = false) {
            FireControlSystem fcs = day_optic.FCS;
            fcs._fixParallaxForVectorMode = true;
            fcs.SuperelevateWeapon = true;
            fcs.LaserAim = LaserAimMode.ImpactPoint;
            fcs.SuperelevateFireGating = false;
            fcs.FireGateAngle = 0.5f;
            fcs.SuperleadWeapon = true;
            fcs.InertialCompensation = false;
            fcs.RecordTraverseRateBuffer = true;
            fcs.TraverseBufferSeconds = 0.5f;
            fcs._autoDumpViaPalmSwitches = true;
            fcs.UseDeltaD = false;
            fcs.ActiveDeltaD = false;
            fcs.ImperfectDeltaD = false;
            fcs.ManualModeTriggers = new GHPC.Equipment.FcsManualModeStartTrigger[] { };
            fcs.AutoModeTriggers = new GHPC.Equipment.FcsManualModeCancelTrigger[] { };
            fcs.GatedAimablePlatforms = fcs.Mounts;
            fcs.FireGateOverrideTransform = null;
            fcs._horizontalLeadOnly = false;
            fcs.HasManualMode = false;
            fcs._manualModeOnRangeSet = false;
            fcs._autoDumpViaPalmSwitches = true;
            fcs._autoModeOnLase = false;
            fcs.IgnoreHorizontalForFireGating = true;
            coax.ExcludeFromFcsUpdates = false;
            coax.PreAimWeapon = WeaponSystemRole.Coaxial;

            night_optic.Alignment = OpticAlignment.BoresightStabilized;
            night_optic.RotateAzimuth = true;
            night_optic.CantCorrect = true;
            night_optic.CantCorrectMaxSpeed = 5f;

            day_optic.slot.DefaultFov = 10f;

            day_optic.slot.OtherFovs = new float[] { 4f };

            /*
            if (day_optic.GetComponent<DigitalZoomSnapper>() == null) 
                day_optic.gameObject.AddComponent<DigitalZoomSnapper>();
            */

            day_optic.CantCorrect = true;
            day_optic.CantCorrectMaxSpeed = 5f;
            day_optic.Alignment = OpticAlignment.BoresightStabilized;
            day_optic.slot.VibrationShakeMultiplier = 0f;
            day_optic.slot.VibrationBlurScale = 0f;
            day_optic.slot.fovAspect = false;
            day_optic.RotateAzimuth = true;
            day_optic.ForceHorizontalReticleAlign = false;
            day_optic.ZeroOutInvalidRange = true;
            day_optic.reticleMesh.reticleSO = reticleSO_sosna;
            day_optic.reticleMesh.reticle = reticle_cached_sosna;
            day_optic.reticleMesh.SMR = null;
            day_optic.reticleMesh.Load();

            GameObject rangebox = GameObject.Instantiate(thermal_canvas);
            rangebox.GetComponent<Reparent>().NewParent = day_optic.transform;
            rangebox.GetComponent<Reparent>().Awake();
            rangebox.SetActive(true);
            rangebox.transform.localPosition = new Vector3(0f, 0f, 0f);
            rangebox.transform.GetChild(0).transform.localPosition = new Vector3(-2.1709f, -350.7738f, 0f);

            GameObject range = GameObject.Instantiate(range_readout);
            range.GetComponent<Reparent>().NewParent = rangebox.transform;
            range.GetComponent<Reparent>().Awake();
            range.SetActive(true);
            range.transform.localPosition = new Vector3(0f, 0f, 0f);
            range.transform.GetChild(1).transform.localPosition = new Vector3(-10f, -285.2727f, 0f);
            day_optic.RangeText = range.GetComponentInChildren<TMP_Text>();
            range.GetComponentInChildren<TMP_Text>().outlineWidth = 1f;
            day_optic.RangeTextPrefix = "<mspace=0.5em>";
            day_optic.RangeTextDivideBy = 1;
            day_optic.RangeTextQuantize = 1;

            Transform ready_backing = range.transform.GetChild(0);
            Component.DestroyImmediate(ready_backing.gameObject.GetComponent<Image>());
            Image image = ready_backing.gameObject.AddComponent<Image>();
            image.color = new Color(0.15f, 0f, 0f);
            ready_backing.localScale = new Vector3(5f, 0.3f, 1f);
            ready_backing.localPosition = new Vector3(-2.1511f, -256.7888f, -0.0001f);

            GameObject ready = GameObject.Instantiate(ready_backing.gameObject, range.transform);
            Image image2 = ready.gameObject.GetComponent<Image>();
            image2.color = new Color(1f, 0f, 0f);

            ready.transform.localPosition = new Vector3(-2.1511f, -256.7888f, -0.0001f);

            day_optic.ReadyToFireObject = ready.gameObject;

            if (thermal) {
                CRTShock.Add(night_optic.slot.transform, 0f, new Vector3(1f, 1f, 1f));
                night_optic.slot.VisionType = NightVisionType.Thermal;
                night_optic.slot.BaseBlur = 0f;
                PostProcessVolume vol = PostProcessVolume.Instantiate(post_sosna);
                vol.gameObject.SetActive(true);
                night_optic.post = vol;
                DepthOfFieldCustomSort dof;
                vol.profile.TryGetSettings<DepthOfFieldCustomSort>(out dof);
                dof.enabled.value = false;

                night_optic.reticleMesh.reticleSO = reticleSO_sosna_thermal;
                night_optic.reticleMesh.reticle = reticle_cached_sosna_thermal;
                night_optic.reticleMesh.SMR = null;
                night_optic.reticleMesh.Load();

                night_optic.reticleMesh.Clear();

                GameObject wide = GameObject.Instantiate(night_optic.reticleMesh.gameObject, night_optic.transform);
                wide.gameObject.SetActive(true);
                ReticleMesh wide_reticle_mesh = wide.GetComponent<ReticleMesh>();
                wide_reticle_mesh.reticleSO = reticleSO_sosna_thermal_wide;
                wide_reticle_mesh.reticle = reticle_cached_sosna_thermal_wide;
                wide_reticle_mesh.SMR = null;
                wide_reticle_mesh.Load();
                wide_reticle_mesh.Clear();

                UsableOptic.FovLimitedItem wide_lim = new UsableOptic.FovLimitedItem();
                wide_lim.FovRange = new Vector2(9f, 360f);
                wide_lim.ExclusiveObjects = new GameObject[] { wide };
                UsableOptic.FovLimitedItem zoomed_lim = new UsableOptic.FovLimitedItem();
                zoomed_lim.FovRange = new Vector2(0f, 8f);
                zoomed_lim.ExclusiveObjects = new GameObject[] { night_optic.reticleMesh.gameObject };

                night_optic._reticleMeshLocalPositions = new Vector2[] { Vector3.zero, Vector3.zero };
                night_optic.FovLimitedItems = new UsableOptic.FovLimitedItem[] { wide_lim, zoomed_lim };
                night_optic.AdditionalReticleMeshes = new ReticleMesh[] { wide_reticle_mesh };
                night_optic.reticleMesh = null;

                night_optic.slot.DefaultFov = 10.52f;
                night_optic.slot.OtherFovs = new float[2] { 6.25f, 2.95f };
                night_optic.slot.VibrationBlurScale = 0.05f;
                night_optic.slot.VibrationShakeMultiplier = 0.01f;
                night_optic.slot.VibrationPreBlur = true;

                night_optic.gameObject.AddComponent<HasThermalMonitor>();
            }

            fcs.RegisteredRangeLimits = new Vector2(100f, 4000f);
            fcs._currentRange = 100f;
            fcs.UpdateRange();
        }

        private static void Reticle() {
            if (!ReticleMesh.cachedReticles.ContainsKey("T55"))
            {
                foreach (Vehicle obj in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
                {
                    if (obj.gameObject.name == "T55A")
                    {
                        obj.WeaponsManager.Weapons[0].FCS.AuthoritativeOptic.reticleMesh.Load();
                        break;
                    }
                }
            }

            reticleSO_sosna = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55"].tree);
            reticleSO_sosna.name = "sosna u";

            Util.ShallowCopy(reticle_cached_sosna, ReticleMesh.cachedReticles["T55"]);
            reticle_cached_sosna.tree = reticleSO_sosna;

            reticle_cached_sosna.tree.lights = new List<ReticleTree.Light>() { new ReticleTree.Light() };

            Util.ShallowCopy(reticle_cached_sosna.tree.lights[0], ReticleMesh.cachedReticles["T55"].tree.lights[0]);
            reticle_cached_sosna.tree.lights[0].type = ReticleTree.Light.Type.Powered;
            reticle_cached_sosna.tree.lights[0].color = new RGB(12f, 0f, 0f, true);
            reticle_cached_sosna.mesh = null;

            ReticleTree.Angular impact = new ReticleTree.Angular(new Vector2(), null);
            impact.name = "Impact";
            impact.align = ReticleTree.GroupBase.Alignment.Boresight;

            reticleSO_sosna.planes[0].elements = new List<ReticleTree.TransformElement>();
            ReticleTree.Angular eeeee = new ReticleTree.Angular(new Vector2(), null);
            eeeee.name = "Angular";
            eeeee.align = ReticleTree.GroupBase.Alignment.Boresight;

            for (int i = -1; i <= 1; i += 2)
            {
                ReticleTree.Line chev_line = new ReticleTree.Line();
                chev_line.thickness.mrad = 0.1555f;
                chev_line.length.mrad = 2.0944f;
                chev_line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                chev_line.length.unit = AngularLength.AngularUnit.MIL_USSR;
                chev_line.rotation.mrad = i == 1 ? 5235.99f : 1047.2f;
                chev_line.position = new ReticleTree.Position(0.48f * i, -0.90f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
                chev_line.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;

                ReticleTree.Line side = new ReticleTree.Line();
                side.thickness.mrad = 0.1833f;
                side.length.mrad = 5.0944f;
                side.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                side.length.unit = AngularLength.AngularUnit.MIL_USSR;
                side.position = new ReticleTree.Position(5f * i, 0, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
                side.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;

                side.illumination = ReticleTree.Light.Type.Powered;
                chev_line.illumination = ReticleTree.Light.Type.Powered;

                eeeee.elements.Add(chev_line);
                eeeee.elements.Add(side);
            }

            foreach (Vector2 pos in new Vector2[] { new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1) })
            {
                ReticleTree.Line border_line = new ReticleTree.Line();
                border_line.thickness.mrad = 0.1833f;
                border_line.length.mrad = 2.0944f / 1.3f;
                border_line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                border_line.length.unit = AngularLength.AngularUnit.MIL_USSR;

                if (Math.Abs(pos.x) == 1)
                {
                    border_line.rotation = 1570.8f;
                }

                border_line.position = new ReticleTree.Position(18f * pos.x, 18f * pos.y, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
                border_line.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;
                border_line.illumination = ReticleTree.Light.Type.Powered;

                if (pos.y != -1)
                    eeeee.elements.Add(border_line);

                ReticleTree.Line border_line2 = new ReticleTree.Line();
                Util.ShallowCopy(border_line2, border_line);
                border_line2.length.mrad = 2.3944f;
                border_line2.thickness.mrad = 0.1833f * 2f;
                border_line2.position = new ReticleTree.Position(33f * pos.x, 33f * pos.y, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);

                eeeee.elements.Add(border_line2);
            }

            ReticleTree.Line middle_line = new ReticleTree.Line();
            middle_line.thickness.mrad = 0.1555f;
            middle_line.length.mrad = 5.0944f;
            middle_line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
            middle_line.length.unit = AngularLength.AngularUnit.MIL_USSR;
            middle_line.position = new ReticleTree.Position(0f, -5f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
            middle_line.rotation.mrad = 1570.8f;
            middle_line.illumination = ReticleTree.Light.Type.Powered;
            middle_line.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;

            ReticleTree.Line middle_line2 = new ReticleTree.Line();
            Util.ShallowCopy(middle_line2, middle_line);
            middle_line2.length.mrad = 5.0944f / 3f;
            middle_line2.position = new ReticleTree.Position(2f, -1.8f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);

            ReticleTree.Line middle_line3 = new ReticleTree.Line();
            Util.ShallowCopy(middle_line3, middle_line2);
            middle_line3.position = new ReticleTree.Position(-1.5f, -5.0944f / 3f / 2f - 1.3f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
            middle_line3.rotation.mrad = 0f;

            eeeee.elements.Add(middle_line);
            eeeee.elements.Add(middle_line2);
            eeeee.elements.Add(middle_line3);

            impact.elements.Add(eeeee);
            reticleSO_sosna.planes[0].elements.Add(impact);         
        }

        private static void ThermalReticle()
        {
            // WFOV
            reticleSO_sosna_thermal_wide = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["WFOV"].tree);
            reticleSO_sosna_thermal_wide.name = "PACT-SOSNA-WFOV";

            Util.ShallowCopy(reticle_cached_sosna_thermal_wide, ReticleMesh.cachedReticles["WFOV"]);
            reticle_cached_sosna_thermal_wide.tree = reticleSO_sosna_thermal_wide;

            reticle_cached_sosna_thermal_wide.tree.lights = new List<ReticleTree.Light>() {
                new ReticleTree.Light()
            };

            reticle_cached_sosna_thermal_wide.tree.lights[0].type = ReticleTree.Light.Type.Powered;
            reticle_cached_sosna_thermal_wide.tree.lights[0].color = new RGB(1f, 1f, 1f, true);

            ReticleTree.Angular wfov_angular = (reticleSO_sosna_thermal_wide.planes[0].elements[0] as ReticleTree.Angular).elements[0] as ReticleTree.Angular;

            for (int i = 0; i <= 3; i++) {
                (wfov_angular.elements[i] as ReticleTree.Line).length.mrad = 9.8175f;
            }

            // CROSSHAIRS
            reticleSO_sosna_thermal = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55-NVS"].tree);
            reticleSO_sosna_thermal.name = "PACT-SOSNA-CROSSHAIRS";

            Util.ShallowCopy(reticle_cached_sosna_thermal, ReticleMesh.cachedReticles["T55-NVS"]);
            reticle_cached_sosna_thermal.tree = reticleSO_sosna_thermal;

            reticle_cached_sosna_thermal.tree.lights = new List<ReticleTree.Light>() {
                new ReticleTree.Light()
            };

            reticle_cached_sosna_thermal.tree.lights[0].type = ReticleTree.Light.Type.Powered;
            reticle_cached_sosna_thermal.tree.lights[0].color = new RGB(1f, 1f, 1f, true);

            ReticleTree.Angular reticle_sosna_cross = (reticleSO_sosna_thermal.planes[0].elements[0] as ReticleTree.Angular).elements[0] as ReticleTree.Angular;
            (reticleSO_sosna_thermal.planes[0].elements[0] as ReticleTree.Angular).align = ReticleTree.GroupBase.Alignment.Boresight;
            reticle_sosna_cross.align = ReticleTree.GroupBase.Alignment.Boresight;
            reticle_cached_sosna_thermal.mesh = null;

            reticle_sosna_cross.elements.RemoveAt(4);
            reticle_sosna_cross.elements.RemoveAt(1);
            reticle_sosna_cross.elements.RemoveAt(0);

            ReticleTree.Line line1 = reticle_sosna_cross.elements[0] as ReticleTree.Line;
            line1.rotation.mrad = 0;
            line1.position.x = 0;
            line1.position.y = 0;
            line1.length.mrad = 22.0944f;
            line1.illumination = ReticleTree.Light.Type.Powered;

            ReticleTree.Line line2 = reticle_sosna_cross.elements[1] as ReticleTree.Line;
            line2.position.y = 0;
            line2.length.mrad = 9.0944f;
            line2.illumination = ReticleTree.Light.Type.Powered;
        }

        public static void Init() {
            if (reticleSO_sosna == null)
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "sosna_monitor"));
                sosna_monitor = bundle.LoadAsset<GameObject>("MONITOR CANVAS.prefab");
                sosna_monitor.hideFlags = HideFlags.DontUnloadUnusedAsset;

                foreach (Vehicle obj in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
                {
                    if (obj.gameObject.name == "M1IP")
                    {
                        range_readout = GameObject.Instantiate(obj.transform.Find("Turret Scripts/GPS/Optic/Abrams GPS canvas").gameObject);
                        GameObject.Destroy(range_readout.transform.GetChild(2).gameObject);
                        //GameObject.Destroy(range_readout.transform.GetChild(0).gameObject);
                        range_readout.AddComponent<Reparent>();
                        range_readout.SetActive(false);
                        range_readout.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        range_readout.name = "t72 range canvas";

                        TextMeshProUGUI text = range_readout.GetComponentInChildren<TextMeshProUGUI>();
                        text.color = new Color(255f, 0f, 0f);
                        text.faceColor = new Color(255f, 0f, 0f);
                        text.outlineColor = new Color(100f, 0f, 0f, 0.5f);

                        if (!ReticleMesh.cachedReticles.ContainsKey("WFOV"))
                        {
                            obj.transform.Find("Turret Scripts/GPS/FLIR/Reticle Mesh WFOV").GetComponent<ReticleMesh>().Load();
                        }
                    }

                    if (obj.gameObject.name == "M2 Bradley")
                    {
                        thermal_canvas = GameObject.Instantiate(obj.transform.Find("FCS and sights/GPS Optic/M2 Bradley GPS canvas").gameObject);
                        GameObject.Destroy(thermal_canvas.transform.GetChild(2).gameObject);
                        thermal_canvas.AddComponent<Reparent>();
                        thermal_canvas.SetActive(false);
                        thermal_canvas.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        thermal_canvas.name = "t72 thermal canvas";
                    }

                    if (thermal_canvas && range_readout) break;
                }

                Reticle();
                ThermalReticle();

                post_sosna = PostProcessVolume.Instantiate(PactThermal.post_og);
                ColorGrading color_grading = post_sosna.profile.settings[1] as ColorGrading;
                color_grading.postExposure.overrideState = false;
                color_grading.contrast.value = 2f;
                color_grading.colorFilter.value = new Color(0.4227f, 0.4227f, 0.4227f);
                color_grading.lift.overrideState = false;
                (post_sosna.profile.settings[2] as Grain).intensity.value = 0.111f;
                (post_sosna.profile.settings[0] as Bloom).intensity.value = 0.5f;
                post_sosna.sharedProfile = post_sosna.profile;
                post_sosna.gameObject.SetActive(false);
            }


            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(AddThermalMonitor), GameStatePriority.Medium);
        }
    }
}
