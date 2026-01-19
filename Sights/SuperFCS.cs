using System;
using System.Collections.Generic;
using System.IO;
using GHPC.Camera;
using GHPC.Crew;
using GHPC.Equipment.Optics;
using GHPC.Utility;
using GHPC.Weapons;
using MelonLoader.Utils;
using Reticle;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace PactIncreasedLethality
{
    public class SuperFCS
    {
        static GameObject range_readout;
        static GameObject thermal_canvas;

        static GameObject sosna_monitor;
        static GameObject vesna_monitor;

        static ReticleSO reticleSO_sosna;
        static ReticleMesh.CachedReticle reticle_cached_sosna;

        private static bool assets_loaded = false;

        public class ThermalMonitor : MonoBehaviour {
            private Transform wfov_ui;
            private Transform crosshair_ui;
            private Transform crosshair_holder;
            private Transform crosshairs;
            private Transform vesna_holder;
            private Transform active_vesna_crosshair;
            private Transform vesna_track_crosshair;
            private Transform vesna_no_track_crosshair;
            private Transform stab;
            private Transform apfsds;
            private Transform heat;
            private Transform pk;
            private Transform he;
            private Transform atgm;
            private TextMeshProUGUI range;
            private FireControlSystem fcs;
            private CameraSlot night_slot;
            private LockOnLead lockon;
            private bool guiding = false;
            private bool firing = false;
            private bool is_vesna = false;

            public Transform tracking_gates;

            void Awake() {
                crosshair_ui = transform.Find("CROSSHAIR UI");
                wfov_ui = transform.Find("WFOV UI");
                crosshair_holder = transform.Find("CROSSHAIR HOLDER");
                crosshairs = crosshair_holder.Find("CROSSHAIR");

                apfsds = crosshair_ui.Find("AMMO (AP)");
                atgm = crosshair_ui.Find("AMMO (ATGM)");
                heat = crosshair_ui.Find("AMMO (HEAT)");
                he = crosshair_ui.Find("AMMO (HE)");
                pk = crosshair_ui.Find("AMMO (COAX)");
                range = crosshair_ui.Find("RANGE").GetComponentInChildren<TextMeshProUGUI>();
                stab = wfov_ui.Find("STAB");
                tracking_gates = crosshair_ui.Find("TRACKING GATE HOLDER");

                UsableOptic night_optic = GetComponentInParent<UsableOptic>();
                fcs = night_optic.FCS;
                night_slot = night_optic.slot;
                lockon = fcs.GetComponent<LockOnLead>();
                GunnerBrain gunner_brain = GetComponentInParent<CrewManager>().GetCrewBrain(CrewPosition.Gunner) as GunnerBrain;

                night_slot.ZoomChanged += CameraSlot_ZoomChanged;
                fcs.StabsActiveChanged += FCS_StabsActiveChanged;
                fcs.AmmoTypeChanged += FCS_AmmoTypeChanged;
                gunner_brain.WeaponsModule.WeaponSystemChanged += GunnerBrain_WeaponSystemChanged;
                lockon.TargetLockChanged += LockOnLead_TargetLockChanged;

                is_vesna = transform.Find("VESNA CROSSHAIRS") != null;

                if (is_vesna)
                {
                    vesna_holder = transform.Find("VESNA CROSSHAIRS");
                    vesna_no_track_crosshair = vesna_holder.Find("CROSSHAIR NO TRACK VESNA");
                    vesna_track_crosshair = vesna_holder.Find("CROSSHAIR TRACK VESNA");
                    vesna_holder.gameObject.SetActive(true);
                    crosshairs.gameObject.SetActive(false);
                    active_vesna_crosshair = vesna_no_track_crosshair;

                    foreach (WeaponSystemInfo weapon_info in gunner_brain.WeaponsModule.Weapons)
                    {
                        WeaponSystem weapon = weapon_info.Weapon;

                        weapon.TriggerDown += WeaponSystem_TriggerDown;
                        weapon.TriggerUp += WeaponSystem_TriggerUp;

                        if (weapon.GuidanceUnit)
                        {
                            weapon.GuidanceUnit.GuidanceStarted += GuidanceUnit_Started;
                            weapon.GuidanceUnit.GuidanceStopped += GuidanceUnit_Stopped;
                        }
                    }
                }

                pk.gameObject.SetActive(false);
                atgm.gameObject.SetActive(false);
                heat.gameObject.SetActive(false);
                he.gameObject.SetActive(false);
            }

            private void VesnaHide()
            {
                crosshairs.gameObject.SetActive(true);
                active_vesna_crosshair.gameObject.SetActive(false);
            }

            private void VesnaShow()
            {
                crosshairs.gameObject.SetActive(false);
                active_vesna_crosshair.gameObject.SetActive(true);
            }

            private void GuidanceUnit_Started()
            {
                guiding = true;
                VesnaHide();
            }

            private void GuidanceUnit_Stopped()
            {
                guiding = false;
                VesnaShow();
            }

            private void WeaponSystem_TriggerDown()
            {
                firing = true;
                if (guiding) return;
                VesnaHide();
            }

            private void WeaponSystem_TriggerUp()
            {
                firing = false;
                if (guiding) return;
                VesnaShow();
            }

            private void LockOnLead_TargetLockChanged(bool has_target)
            {
                tracking_gates.gameObject.SetActive(has_target);

                if (!is_vesna) return;

                active_vesna_crosshair = has_target ? vesna_track_crosshair : vesna_no_track_crosshair;
                if (guiding || firing) return;
                vesna_no_track_crosshair.gameObject.SetActive(!has_target);
                vesna_track_crosshair.gameObject.SetActive(has_target);
            }

            private void GunnerBrain_WeaponSystemChanged(WeaponSystemInfo old_weapon, WeaponSystemInfo new_weapon)
            {
                pk.gameObject.SetActive(new_weapon.Weapon.MetaName == "Coaxial MG");
            }

            private void FCS_StabsActiveChanged(bool active)
            {
                stab.gameObject.SetActive(active);
            }

            private void FCS_AmmoTypeChanged(AmmoType ammo_type)
            {
                apfsds.gameObject.SetActive(ammo_type.Category == AmmoType.AmmoCategory.Penetrator);
                heat.gameObject.SetActive(ammo_type.Category == AmmoType.AmmoCategory.ShapedCharge && ammo_type.Guidance == AmmoType.GuidanceType.Unguided);
                he.gameObject.SetActive(ammo_type.Category == AmmoType.AmmoCategory.Explosive);
                atgm.gameObject.SetActive(ammo_type.Guidance > AmmoType.GuidanceType.Unguided);
            }

            private void CameraSlot_ZoomChanged()
            {
                Vector3 scale = night_slot.CurrentFov == 2.95f ?
                    new Vector3(0.42f, 0.42f, 1f) : new Vector3(0.35f, 0.35f, 1f);

                crosshairs.localScale = scale;

                if (is_vesna)
                {
                    vesna_no_track_crosshair.localScale = scale;
                    vesna_track_crosshair.localScale = scale;
                }
            }

            void LateUpdate()
            {
                if (fcs == null) return;
           
                range.text = ((int)MathUtil.RoundFloatToMultipleOf(fcs.CurrentRange, 5)).ToString("0000");
            }
        }

        public static void Add(UsableOptic day_optic, UsableOptic night_optic, WeaponSystemInfo coax, 
            WeaponSystemInfo main, CustomGuidanceComputer mgu, bool vesna = false) 
        {
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
            fcs.MaxLaserRange = 4000f;
            fcs.WeaponAuthoritative = false;
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

            //CRTShock.Add(night_optic.slot.transform, 0f, new Vector3(1f, 1f, 1f));
            night_optic.slot.VisionType = NightVisionType.Thermal;
            night_optic.slot.BaseBlur = 0f;
            night_optic.post = null;
            night_optic.reticleMesh.Clear();
            night_optic.reticleMesh = null;
            night_optic.slot.FLIRBlitMaterialOverride = PactThermal.white_flir_mat_no_scope;

            night_optic.slot.DefaultFov = 10.52f;
            night_optic.slot.OtherFovs = new float[2] { 6.25f, 2.95f };
            night_optic.slot.VibrationBlurScale = 0.05f;
            night_optic.slot.VibrationShakeMultiplier = 0.01f;
            night_optic.slot.VibrationPreBlur = true;
            night_optic.slot.OverrideFLIRResolution = true;
            night_optic.slot.FLIRWidth = 1024;
            night_optic.slot.FLIRHeight = 576;
            night_optic.slot.CanToggleFlirPolarity = true;
            night_optic.slot.FLIRFilterMode = FilterMode.Point;

            GameObject monitor_canvas = GameObject.Instantiate(vesna ? vesna_monitor : sosna_monitor, night_optic.transform);
            ThermalMonitor monitor = monitor_canvas.AddComponent<ThermalMonitor>();

            GameObject wfov_reticle = monitor_canvas.transform.Find("WFOV").gameObject;
            GameObject wfov_elements = monitor_canvas.transform.Find("WFOV UI").gameObject;
            GameObject crosshair_reticle = monitor_canvas.transform.Find("CROSSHAIR HOLDER").gameObject;
            GameObject crosshair_elements = monitor_canvas.transform.Find("CROSSHAIR UI").gameObject;

            List<GameObject> narrow_fov_items = new List<GameObject>() { crosshair_elements, crosshair_reticle };
            if (vesna) 
            {
                narrow_fov_items.Add(monitor_canvas.transform.Find("VESNA CROSSHAIRS").gameObject);
            }

            night_optic.FovLimitedItems = new UsableOptic.FovLimitedItem[] {
                new UsableOptic.FovLimitedItem() {
                    FovRange = new Vector2(10f, 15f),
                    ExclusiveObjects = new GameObject[] { wfov_reticle, wfov_elements }
                },

                new UsableOptic.FovLimitedItem() {
                    FovRange = new Vector2(2f, 7f),
                    ExclusiveObjects = narrow_fov_items.ToArray()
                }
            };

            night_optic.ReadyToFireObject = crosshair_elements.transform.Find("READY").gameObject;
            night_optic.OverridingObject = crosshair_elements.transform.Find("CMDR CONTROL").gameObject;
            night_optic.RangeText = crosshair_elements.transform.Find("RANGE").GetComponentInChildren<TMP_Text>();

            GameObject post = GameObject.Instantiate(Assets.flir_post_green, night_optic.transform);
            PostProcessProfile profile = post.transform.Find("FLIR Only Volume").GetComponent<PostProcessVolume>().profile;
            ColorGrading color_grading;
            profile.TryGetSettings<ColorGrading>(out color_grading);
            color_grading.postExposure.value = 0f;

            LockOnLead s = fcs.gameObject.AddComponent<LockOnLead>();
            s.fcs = fcs;
            s.guidance_computer = mgu;
            s.tracking_gates = crosshair_elements.transform.Find("TRACKING GATE HOLDER").GetComponent<RectTransform>();

            fcs.RegisteredRangeLimits = new Vector2(0f, 4000f);
            fcs._originalRangeLimits = new Vector2(0f, 4000f);
            fcs._currentRange = 0f;
            fcs.UpdateRange();

            night_optic.gameObject.SetActive(true);
            night_optic.gameObject.SetActive(false);
        }

        private static void Reticle() {
            reticleSO_sosna = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55"].tree);
            reticleSO_sosna.name = "sosna u";

            Util.ShallowCopy(reticle_cached_sosna, ReticleMesh.cachedReticles["T55"]);
            reticle_cached_sosna.tree = reticleSO_sosna;

            reticle_cached_sosna.tree.lights = new List<ReticleTree.Light>() { new ReticleTree.Light() };

            Util.ShallowCopy(reticle_cached_sosna.tree.lights[0], ReticleMesh.cachedReticles["T55"].tree.lights[0]);
            reticle_cached_sosna.tree.lights[0].type = ReticleTree.Light.Type.Powered;
            reticle_cached_sosna.tree.lights[0].color = new RGB(7.5f, 0f, 0f, true);
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

        public static void LoadAssets() 
        {
            if (assets_loaded) return;

            AssetBundle sosna_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "sosna_monitor"));
            sosna_monitor = sosna_bundle.LoadAsset<GameObject>("SOSNA MONITOR CANVAS.prefab");
            sosna_monitor.hideFlags = HideFlags.DontUnloadUnusedAsset;

            AssetBundle vesna_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "vesna_monitor"));
            vesna_monitor = vesna_bundle.LoadAsset<GameObject>("VESNA K CANVAS.prefab");
            vesna_monitor.hideFlags = HideFlags.DontUnloadUnusedAsset;

            range_readout = GameObject.Instantiate(Assets.m1ip_range_canvas);
            GameObject.Destroy(range_readout.transform.GetChild(2).gameObject);
            range_readout.AddComponent<Reparent>();
            range_readout.SetActive(false);
            range_readout.hideFlags = HideFlags.DontUnloadUnusedAsset;
            range_readout.name = "t72 range canvas";

            TextMeshProUGUI text = range_readout.GetComponentInChildren<TextMeshProUGUI>();
            text.color = new Color(255f, 0f, 0f);
            text.faceColor = new Color(255f, 0f, 0f);
            text.outlineColor = new Color(100f, 0f, 0f, 0.5f);

            thermal_canvas = GameObject.Instantiate(Assets.m2_bradley_canvas);
            GameObject.Destroy(thermal_canvas.transform.GetChild(2).gameObject);
            thermal_canvas.AddComponent<Reparent>();
            thermal_canvas.SetActive(false);
            thermal_canvas.hideFlags = HideFlags.DontUnloadUnusedAsset;
            thermal_canvas.name = "t72 thermal canvas";

            Reticle();

            assets_loaded = true;
        }
    }
}
