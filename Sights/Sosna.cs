using System;
using System.Collections.Generic;
using System.Linq;
using GHPC.Equipment.Optics;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using Reticle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PactIncreasedLethality
{
    public class BLYAT : MonoBehaviour
    {
        public GameObject square;
    }

    public class Sosna
    {
        static GameObject range_readout;
        static GameObject thermal_canvas;

        static ReticleSO reticleSO_sosna;
        static ReticleMesh.CachedReticle reticle_cached_sosna;

        static GameObject square;

        public static void Add(UsableOptic day_optic, UsableOptic night_optic, WeaponSystemInfo coax) {
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
            fcs.RegisteredRangeLimits = new Vector2(200f, 4000f);
            fcs._currentRange = 200f;
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
            //fcs.transform.localPosition = new Vector3(-0.603f, 0.6288f, -5.547f);
            //fcs.transform.localPosition = new Vector3(-0.803f, 0.32f, -5.547f);
            //fcs.LaserOrigin = fcs.transform;

            coax.ExcludeFromFcsUpdates = false;
            coax.PreAimWeapon = WeaponSystemRole.Coaxial;

            night_optic.Alignment = OpticAlignment.BoresightStabilized;
            night_optic.RotateAzimuth = true;
            night_optic.CantCorrect = true;
            night_optic.CantCorrectMaxSpeed = 5f;

            day_optic.slot.DefaultFov = 15f;
            //day_optic.slot.SpriteType = CameraSpriteManager.SpriteType.NightVisionGoggles;

            List<float> fovs = new List<float>();
            for (float i = 12; i >= 4; i--)
            {
                fovs.Add(i);
            }
            day_optic.slot.OtherFovs = fovs.ToArray<float>();

            if (day_optic.GetComponent<DigitalZoomSnapper>() == null) 
                day_optic.gameObject.AddComponent<DigitalZoomSnapper>();

            day_optic.UseRotationForShake = true;
            day_optic.CantCorrect = true;
            day_optic.CantCorrectMaxSpeed = 5f;
            day_optic.Alignment = OpticAlignment.FcsRange;
            day_optic.slot.VibrationShakeMultiplier = 0f;
            day_optic.slot.VibrationBlurScale = 0f;
            day_optic.slot.fovAspect = false;
            day_optic.RotateAzimuth = true;
            day_optic.ForceHorizontalReticleAlign = true;
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
            //rangebox.transform.GetChild(0).transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            //rangebox.transform.GetChild(0).transform.localScale = new Vector3(1f, 1f, 1f);

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

           
            GameObject _square = GameObject.Instantiate(square, rangebox.transform);

            BLYAT b = fcs.gameObject.AddComponent<BLYAT>();
            b.square = _square;
            _square.SetActive(false);           
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
            reticle_cached_sosna.tree.lights[0].color = new RGB(2f, -0.3f, -0.3f, true);
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
                chev_line.thickness.mrad = 0.1833f;
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
            middle_line.thickness.mrad = 0.1833f;
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

        public static void Init() {
            if (reticleSO_sosna != null) return;

            Reticle();

            if (!range_readout)
            {
                foreach (Vehicle obj in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
                {
                    if (obj.gameObject.name == "M1IP")
                    {
                        square = obj.transform.Find("Turret Scripts/GPS/Optic/Abrams GPS canvas/ready indicator").gameObject;

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
            }
        }
    }
}
