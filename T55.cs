using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Equipment.Optics;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using PactIncreasedLethality;
using Reticle;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;
using GHPC.Equipment;
using GHPC.State;
using System.Collections;
using GHPC.Player;
using MelonLoader;
using HarmonyLib;
using GHPC;
using static UnityEngine.GraphicsBuffer;
using TMPro;
using GHPC.UI.Tips;
using System.Reflection;
using GHPC.Camera;
using static NWH.WheelController3D.WheelController;

namespace PactIncreasedLethality
{
    public class T55
    {
        static GameObject range_readout;
        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static ReticleSO reticleSO_atgm;
        static ReticleMesh.CachedReticle reticle_cached_atgm;

        static AmmoClipCodexScriptable clip_codex_3bk17m;
        static AmmoType.AmmoClip clip_3bk17m;
        static AmmoCodexScriptable ammo_codex_3bk17m;
        static AmmoType ammo_3bk17m;
        static GameObject ammo_3bk17m_vis = null;

        static AmmoClipCodexScriptable clip_codex_9m117;
        static AmmoType.AmmoClip clip_9m117;
        static AmmoCodexScriptable ammo_codex_9m117;
        static AmmoType ammo_9m117;
        static GameObject ammo_9m117_vis = null;

        static AmmoType ammo_3bk5m;
        static AmmoType ammo_3of412;
        static AmmoType ammo_9m111;

        static MelonPreferences_Entry<bool> t55_patch;
        static MelonPreferences_Entry<bool> use_3bk17m;
        static MelonPreferences_Entry<bool> use_9m117;
        static MelonPreferences_Entry<bool> better_stab;
        static MelonPreferences_Entry<bool> has_lrf;
        static MelonPreferences_Entry<bool> has_drozd;

        public static void Config(MelonPreferences_Category cfg)
        {
            t55_patch = cfg.CreateEntry<bool>("T-55 Patch", true);
            use_3bk17m = cfg.CreateEntry<bool>("Use 3BK17M", true);
            use_3bk17m.Comment = "Replaces 3BK5M (improved ballistics, marginally better penetration)";
            use_9m117 = cfg.CreateEntry<bool>("Use 9M117", true);
            use_9m117.Comment = "Replaces 3OF412; GLATGM, has its own sight with fixed 8x magnification";
            better_stab = cfg.CreateEntry<bool>("Better Stabilizer", true);
            better_stab.Comment = "Less reticle blur, shake while on the move";
            has_lrf = cfg.CreateEntry<bool>("Laser Rangefinder", true);
            has_lrf.Comment = "Only gives range: user will need to set range manually";

            has_drozd = cfg.CreateEntry<bool>("Drozd APS (T-55)", true);
            has_drozd.Comment = "Intercepts incoming projectiles; covers the frontal arc of the tank relative to where the turret is facing";
        }

        public static void OnLateUpdate() {
            PlayerInput player_manager = PactIncreasedLethalityMod.player_manager;
            CameraManager camera_manager = PactIncreasedLethalityMod.camera_manager;

            if (player_manager == null) return;
            if (camera_manager == null) return; 
            if (player_manager.CurrentPlayerWeapon == null) return;
            if (player_manager.CurrentPlayerWeapon.Name != "100mm gun D-10T") return;
            if (!use_9m117.Value) return;

            WeaponSystem weapon = player_manager.CurrentPlayerWeapon.Weapon;

            if (weapon == null) return;

            if (weapon.FCS.CurrentAmmoType.Name == "9M117 Bastion")
            {
                weapon.MuzzleEffects[0].transform.GetChild(3).GetChild(0).gameObject.SetActive(false);

                if (!camera_manager.ExteriorMode)
                {
                    UsableOptic day_optic = Util.GetDayOptic(weapon.FCS);

                    if (day_optic.slot.DefaultFov == 4.2f) return;

                    day_optic.slot.DefaultFov = 4.2f;
                    day_optic.slot.OtherFovs[0] = 4.2f;
                    day_optic.reticleMesh = weapon.FCS.transform.GetChild(0).GetChild(3).gameObject.GetComponent<ReticleMesh>();
                    day_optic.transform.Find("t55 range canvas(Clone)").gameObject.SetActive(false);
                    weapon.FCS.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    weapon.FCS.transform.GetChild(0).GetChild(3).gameObject.SetActive(true);
                    camera_manager.ZoomChanged();
                }
            }
            else {
                weapon.MuzzleEffects[0].transform.GetChild(3).GetChild(0).gameObject.SetActive(true);

                if (!camera_manager.ExteriorMode)
                {
                    UsableOptic day_optic = Util.GetDayOptic(weapon.FCS);

                    if (day_optic.slot.DefaultFov == 10.64f) return;

                    day_optic.slot.DefaultFov = 10.64f;
                    day_optic.slot.OtherFovs[0] = 5.04f;
                    day_optic.reticleMesh = weapon.FCS.transform.GetChild(0).GetChild(0).gameObject.GetComponent<ReticleMesh>();
                    day_optic.transform.Find("t55 range canvas(Clone)").gameObject.SetActive(true);
                    weapon.FCS.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                    weapon.FCS.transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
                    camera_manager.ZoomChanged();
                }
            }
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (GameObject vic_go in PactIncreasedLethalityMod.vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (vic.FriendlyName != "T-55A") continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;

                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();

                if (has_lrf.Value)
                {
                    weapon.FCS.gameObject.AddComponent<LimitedLRF>();
                    fcs.MaxLaserRange = 6000f;
                }

                UsableOptic day_optic = Util.GetDayOptic(fcs);

                if (better_stab.Value)
                {
                    day_optic.slot.VibrationBlurScale = 0.1f;
                    day_optic.slot.VibrationShakeMultiplier = 0.2f;
                }

                if (use_3bk17m.Value) loadout_manager.LoadedAmmoTypes[1] = clip_codex_3bk17m;
                if (use_9m117.Value) loadout_manager.LoadedAmmoTypes[2] = clip_codex_9m117;

                for (int i = 0; i <= 4; i++)
                {
                    GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                    if (use_3bk17m.Value) rack.ClipTypes[1] = clip_codex_3bk17m.ClipType;
                    if (use_9m117.Value) rack.ClipTypes[2] = clip_codex_9m117.ClipType;
                    Util.EmptyRack(rack);
                }

                loadout_manager.SpawnCurrentLoadout();
                weapon.Feed.AmmoTypeInBreech = null;
                weapon.Feed.Start();
                loadout_manager.RegisterAllBallistics();
                
                weapon.Feed.ReloadDuringMissileTracking = true; 
                GameObject guidance_computer_obj = new GameObject("guidance computer");
                guidance_computer_obj.transform.parent = vic.transform;
                guidance_computer_obj.AddComponent<MissileGuidanceUnit>();

                guidance_computer_obj.AddComponent<Reparent>();
                Reparent reparent = guidance_computer_obj.GetComponent<Reparent>();
                reparent.NewParent = vic_go.transform.Find("T55A_skeleton/HULL/Turret").gameObject.transform;
                reparent.Awake();

                MissileGuidanceUnit computer = guidance_computer_obj.GetComponent<MissileGuidanceUnit>();
                computer.AimElement = weapon.FCS.AimTransform;
                weapon.GuidanceUnit = computer;

                if (!has_lrf.Value)
                {
                    continue;
                }
                
                GameObject t = GameObject.Instantiate(range_readout);
                t.GetComponent<Reparent>().NewParent = Util.GetDayOptic(fcs).transform;
                t.transform.GetChild(0).transform.localPosition = new Vector3(-284.1897f, -5.5217f, 0.1f);
                t.SetActive(true);

                weapon.FCS.GetComponent<LimitedLRF>().canvas = t.transform;
                
                if (use_9m117.Value)
                {
                    if (!reticleSO_atgm)
                    {
                        reticleSO_atgm = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55"].tree);
                        reticleSO_atgm.name = "T55_atgm";

                        Util.ShallowCopy(reticle_cached_atgm, ReticleMesh.cachedReticles["T55"]);
                        reticle_cached_atgm.tree = reticleSO_atgm;

                        reticle_cached_atgm.tree.lights = new List<ReticleTree.Light>() {
                            new ReticleTree.Light(),
                        };

                        reticle_cached_atgm.tree.lights[0] = ReticleMesh.cachedReticles["T55"].tree.lights[0];
                        reticle_cached_atgm.mesh = null;

                        reticleSO_atgm.planes[0].elements = new List<ReticleTree.TransformElement>();
                        ReticleTree.Angular eeeee = new ReticleTree.Angular(new Vector2(), null);
                        eeeee.name = "Boresight";
                        eeeee.align = ReticleTree.GroupBase.Alignment.Boresight;

                        // centre chevron

                        for (int i = -1; i <= 1; i += 2)
                        {
                            ReticleTree.Line chev_line = new ReticleTree.Line();
                            chev_line.thickness.mrad = 0.1833f;
                            chev_line.length.mrad = 2.0944f;
                            chev_line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                            chev_line.length.unit = AngularLength.AngularUnit.MIL_USSR;
                            chev_line.rotation.mrad = i == 1 ? 5497.787f : 785.398f;
                            chev_line.position = new ReticleTree.Position(0.6756f * i, -0.6756f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);

                            ReticleTree.Line side = new ReticleTree.Line();
                            side.thickness.mrad = 0.1833f;
                            side.length.mrad = 7.0944f;
                            side.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                            side.length.unit = AngularLength.AngularUnit.MIL_USSR;
                            side.position = new ReticleTree.Position(5f * i, 0, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);

                            side.illumination = ReticleTree.Light.Type.NightIllumination;
                            chev_line.illumination = ReticleTree.Light.Type.NightIllumination;

                            eeeee.elements.Add(chev_line);
                            eeeee.elements.Add(side);
                        }

                        ReticleTree.Line middle_line = new ReticleTree.Line();
                        middle_line.thickness.mrad = 0.1833f;
                        middle_line.length.mrad = 7.0944f;
                        middle_line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                        middle_line.length.unit = AngularLength.AngularUnit.MIL_USSR;
                        middle_line.position = new ReticleTree.Position(0f, -5f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
                        middle_line.rotation.mrad = 1570.8f;

                        ReticleTree.Line middle_line2 = new ReticleTree.Line();
                        middle_line2.thickness.mrad = 0.1833f;
                        middle_line2.length.mrad = 7.0944f / 2f;
                        middle_line2.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                        middle_line2.length.unit = AngularLength.AngularUnit.MIL_USSR;
                        middle_line2.position = new ReticleTree.Position(-3f, -5f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);

                        ReticleTree.Line middle_line3 = new ReticleTree.Line();
                        middle_line3.thickness.mrad = 0.1833f;
                        middle_line3.length.mrad = 5.0944f / 2f;
                        middle_line3.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                        middle_line3.length.unit = AngularLength.AngularUnit.MIL_USSR;
                        middle_line3.position = new ReticleTree.Position(5f, -7.0944f / 2, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
                        middle_line3.rotation.mrad = 1570.8f;

                        middle_line.illumination = ReticleTree.Light.Type.NightIllumination;
                        middle_line2.illumination = ReticleTree.Light.Type.NightIllumination;
                        middle_line3.illumination = ReticleTree.Light.Type.NightIllumination;

                        eeeee.elements.Add(middle_line);
                        eeeee.elements.Add(middle_line2);
                        eeeee.elements.Add(middle_line3);

                        reticleSO_atgm.planes[0].elements.Add(eeeee);
                    }

                    GameObject reticle_mesh_atgm = GameObject.Instantiate(weapon.FCS.transform.GetChild(0).GetChild(0).gameObject, weapon.FCS.transform.GetChild(0).transform);
                    reticle_mesh_atgm.SetActive(false);

                    reticle_mesh_atgm.GetComponent<ReticleMesh>().reticleSO = reticleSO_atgm;
                    reticle_mesh_atgm.GetComponent<ReticleMesh>().reticle = reticle_cached_atgm;
                    reticle_mesh_atgm.GetComponent<ReticleMesh>().SMR = null;
                    reticle_mesh_atgm.GetComponent<ReticleMesh>().Load();
                }

                if (!reticleSO)
                {
                    ReticleTree.Angular reticle = null;
                    ReticleTree.Angular reticle_heat = null;

                    reticleSO = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55"].tree);
                    reticleSO.name = "T55withdalaser";

                    Util.ShallowCopy(reticle_cached, ReticleMesh.cachedReticles["T55"]);
                    reticle_cached.tree = reticleSO;

                    reticle_cached.tree.lights = new List<ReticleTree.Light>() {
                        new ReticleTree.Light(),
                        new ReticleTree.Light()
                    };

                    reticle_cached.tree.lights[0] = ReticleMesh.cachedReticles["T55"].tree.lights[0];
                    reticle_cached.tree.lights[1].type = ReticleTree.Light.Type.Powered;
                    reticle_cached.tree.lights[1].color = new RGB(2f, 0f, 0f, true);

                    reticleSO.planes[0].elements.Add(new ReticleTree.Angular(new Vector2(0, 0), null, ReticleTree.GroupBase.Alignment.LasePoint));
                    reticle = reticleSO.planes[0].elements[2] as ReticleTree.Angular;
                    reticle_cached.mesh = null;

                    // AAAAAAAAAAAAAAA
                    reticle.elements.Add(new ReticleTree.Circle());
                    reticle.name = "LasePoint";
                    reticle.position = new ReticleTree.Position(0, 0, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                    (reticle.elements[0] as ReticleTree.Circle).radius.mrad = 0.5236f;
                    (reticle.elements[0] as ReticleTree.Circle).thickness.mrad = 0.16f;
                    (reticle.elements[0] as ReticleTree.Circle).illumination = ReticleTree.Light.Type.Powered;
                    (reticle.elements[0] as ReticleTree.Circle).visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;
                    (reticle.elements[0] as ReticleTree.Circle).position = new ReticleTree.Position(0, 0, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                    (reticle.elements[0] as ReticleTree.Circle).position.x = 0;
                    (reticle.elements[0] as ReticleTree.Circle).position.y = 0;

                    if (use_3bk17m.Value)
                    {
                        reticle_heat = ((reticleSO.planes[0].elements[1] as ReticleTree.Angular).elements[2]) as ReticleTree.Angular;
                        (reticle_heat.elements[1] as ReticleTree.VerticalBallistic).projectile = ammo_codex_3bk17m;
                        (reticle_heat.elements[1] as ReticleTree.VerticalBallistic).UpdateBC();
                    }
                }

                day_optic.reticleMesh.reticleSO = reticleSO;
                day_optic.reticleMesh.reticle = reticle_cached;
                day_optic.reticleMesh.SMR = null;
                day_optic.reticleMesh.Load();

                if (has_drozd.Value)
                {
                    List<DrozdLauncher> launchers = new List<DrozdLauncher>();

                    Vector3[] launcher_positions = new Vector3[] {
                        new Vector3(-1.2953f, -0.0083f, 0.1166f),
                        new Vector3(-1.3443f, 0.2091f, 0.0169f),
                        new Vector3(1.2153f, -0.0083f, 0.1166f),
                        new Vector3(1.2943f, 0.2091f, 0.0169f),
                    };

                    Vector3[] launcher_rots = new Vector3[] {
                        new Vector3(0f, 0f, 0f),
                        new Vector3(0f, -13.5494f, 0f),
                        new Vector3(0f, 0f, 0f),
                        new Vector3(0f, 13.5494f, 0f)
                    };

                    for (var i = 0; i < launcher_positions.Length; i++)
                    {
                        GameObject launcher = GameObject.Instantiate(DrozdLauncher.drozd_launcher_visual, vic.transform.Find("T55A_skeleton/HULL/Turret"));
                        launcher.transform.localPosition = launcher_positions[i];
                        launcher.transform.localEulerAngles = launcher_rots[i];

                        if (i > 1)
                        {
                            launcher.transform.localScale = Vector3.Scale(launcher.transform.localScale, new Vector3(-1f, 1f, 1f));
                        }

                        launchers.Add(launcher.GetComponent<DrozdLauncher>());
                    }

                    Drozd.AttachDrozd(
                        vic.transform.Find("T55A_skeleton/HULL/Turret"), vic, new Vector3(0f, 0f, 8f),
                        launchers.GetRange(0, 2).ToArray(), launchers.GetRange(2, 2).ToArray()
                    );

                    vic._friendlyName += "D";
                }
            }

            yield break;
        }

        public static void Init()
        {
            if (!t55_patch.Value) return;
            
            if (!range_readout)
            {
                foreach (Vehicle obj in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
                {
                    if (obj.name == "M1IP")
                    {
                        range_readout = GameObject.Instantiate(obj.transform.Find("Turret Scripts/GPS/Optic/Abrams GPS canvas").gameObject);
                        GameObject.Destroy(range_readout.transform.GetChild(2).gameObject);
                        GameObject.Destroy(range_readout.transform.GetChild(0).gameObject);
                        range_readout.AddComponent<Reparent>();
                        range_readout.SetActive(false);
                        range_readout.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        range_readout.name = "t55 range canvas";

                        TextMeshProUGUI text = range_readout.GetComponentInChildren<TextMeshProUGUI>();
                        text.color = new Color(255f, 0f, 0f);
                        text.faceColor = new Color(255f, 0f, 0f);
                        text.outlineColor = new Color(100f, 0f, 0f, 0.5f);

                        break;
                    }
                }
            }

            if (ammo_3bk17m == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "3BK5M HEAT-FS-T") ammo_3bk5m = s.AmmoType;
                    if (s.AmmoType.Name == "9M111 Fagot") ammo_9m111 = s.AmmoType;
                    if (s.AmmoType.Name == "3OF412 HE-T") ammo_3of412 = s.AmmoType;

                    if (ammo_3bk5m != null && ammo_9m111 != null && ammo_3of412 != null) break;
                }

                ammo_3bk17m = new AmmoType();
                Util.ShallowCopy(ammo_3bk17m, ammo_3bk5m);
                ammo_3bk17m.Name = "3BK17M HEAT-FS-T";
                ammo_3bk17m.Mass = 10.0f;
                ammo_3bk17m.Coeff = 0.25f;
                ammo_3bk17m.MuzzleVelocity = 1085f;
                ammo_3bk17m.RhaPenetration = 400f;
                ammo_3bk17m.TntEquivalentKg = 0.25f;

                ammo_codex_3bk17m = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3bk17m.AmmoType = ammo_3bk17m;
                ammo_codex_3bk17m.name = "ammo_3bk17m";

                clip_3bk17m = new AmmoType.AmmoClip();
                clip_3bk17m.Capacity = 1;
                clip_3bk17m.Name = "3BK17M HEAT-FS-T";
                clip_3bk17m.MinimalPattern = new AmmoCodexScriptable[1];
                clip_3bk17m.MinimalPattern[0] = ammo_codex_3bk17m;

                clip_codex_3bk17m = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3bk17m.name = "clip_3bk17m";
                clip_codex_3bk17m.ClipType = clip_3bk17m;

                ammo_3bk17m_vis = GameObject.Instantiate(ammo_3bk5m.VisualModel);
                ammo_3bk17m_vis.name = "3bk17m visual";
                ammo_3bk17m.VisualModel = ammo_3bk17m_vis;
                ammo_3bk17m.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bk17m;
                ammo_3bk17m.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bk17m;

                ammo_9m117 = new AmmoType();
                Util.ShallowCopy(ammo_9m117, ammo_3of412);
                ammo_9m117.Name = "9M117 Bastion";
                ammo_9m117.Mass = 18.8f;
                ammo_9m117.Coeff = 0.25f;
                ammo_9m117.MuzzleVelocity = 400f;
                ammo_9m117.RhaPenetration = 550f;
                ammo_9m117.TntEquivalentKg = 4.77f;
                ammo_9m117.ImpactFuseTime = ammo_9m111.ImpactFuseTime;
                ammo_9m117.RhaToFuse = ammo_9m111.RhaToFuse;
                ammo_9m117.Guidance = AmmoType.GuidanceType.Saclos;
                ammo_9m117.TurnSpeed = 0.18f;
                ammo_9m117.ShotVisual = ammo_9m111.ShotVisual;
                ammo_9m117.RangedFuseTime = 12.5f;
                ammo_9m117.Category = AmmoType.AmmoCategory.ShapedCharge;
                ammo_9m117.SpiralPower = 25f;
                ammo_9m117.SpiralAngularRate = 1800f;
                ammo_9m117.ArmingDistance = 45f;
                ammo_9m117.ImpactAudio = GHPC.Audio.ImpactAudioType.Missile;
                ammo_9m117.ShortName = AmmoType.AmmoShortName.Missile;

                ammo_codex_9m117 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_9m117.AmmoType = ammo_9m117;
                ammo_codex_9m117.name = "ammo_9m117";

                clip_9m117 = new AmmoType.AmmoClip();
                clip_9m117.Capacity = 1;
                clip_9m117.Name = "9M117 Bastion";
                clip_9m117.MinimalPattern = new AmmoCodexScriptable[1];
                clip_9m117.MinimalPattern[0] = ammo_codex_9m117;

                clip_codex_9m117 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_9m117.name = "clip_9m117";
                clip_codex_9m117.ClipType = clip_9m117;

                ammo_9m117_vis = GameObject.Instantiate(ammo_3of412.VisualModel);
                ammo_9m117_vis.name = "9M117 visual";
                ammo_9m117.VisualModel = ammo_9m117_vis;
                ammo_9m117.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_9m117;
                ammo_9m117.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_9m117;
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
