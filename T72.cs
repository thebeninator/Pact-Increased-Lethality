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
using MelonLoader;
using GHPC;
using TMPro;
using HarmonyLib;
using UnityEngine.UI;
using GHPC.Camera;
using GHPC.Effects.Voices;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using System.Reflection;

namespace PactIncreasedLethality
{
    public class T72
    {
        static GameObject range_readout;
        static GameObject thermal_canvas;
        static GameObject scanline_canvas;
        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static ReticleSO reticleSO_sosna;
        static ReticleMesh.CachedReticle reticle_cached_sosna;

        static AmmoClipCodexScriptable clip_codex_3bm22;
        static AmmoClipCodexScriptable clip_codex_3bm32;

        static AmmoClipCodexScriptable clip_codex_3bm26;
        static AmmoType.AmmoClip clip_3bm26;
        static AmmoCodexScriptable ammo_codex_3bm26;
        static AmmoType ammo_3bm26;
        static GameObject ammo_3bm26_vis = null;

        static AmmoClipCodexScriptable clip_codex_3bm42;
        static AmmoType.AmmoClip clip_3bm42;
        static AmmoCodexScriptable ammo_codex_3bm42;
        static AmmoType ammo_3bm42;
        static GameObject ammo_3bm42_vis = null;

        static AmmoClipCodexScriptable clip_codex_3of26_vt;
        static AmmoType.AmmoClip clip_3of26_vt;
        static AmmoCodexScriptable ammo_codex_3of26_vt;
        static AmmoType ammo_3of26_vt;
        static GameObject ammo_3of26_vt_vis = null;

        static AmmoType ammo_3bm15;
        static AmmoType ammo_3of26;

        static MelonPreferences_Entry<bool> t72_patch;
        static MelonPreferences_Entry<string> t72m_ammo_type;
        static MelonPreferences_Entry<string> t72m1_ammo_type;

        static MelonPreferences_Entry<bool> t72m_random_ammo;
        static MelonPreferences_Entry<bool> t72m1_random_ammo;

        static MelonPreferences_Entry<bool> thermals;
        static MelonPreferences_Entry<bool> thermals_boxing;
        static MelonPreferences_Entry<float> thermals_blur;

        static MelonPreferences_Entry<bool> only_carousel;

        static MelonPreferences_Entry<bool> era_t72m1;
        static MelonPreferences_Entry<bool> era_t72m;

        static MelonPreferences_Entry<bool> soviet_t72m;
        static MelonPreferences_Entry<bool> soviet_t72m1;

        static MelonPreferences_Entry<bool> super_fcs_t72m;
        static MelonPreferences_Entry<bool> super_fcs_t72m1;

        static MelonPreferences_Entry<List<string>> empty_ammo_t72m;
        static MelonPreferences_Entry<List<string>> empty_ammo_t72m1;

        static Dictionary<string, AmmoClipCodexScriptable> ap;
        static Dictionary<string, int> ammo_racks = new Dictionary<string, int>() {
            ["Hull Wet"] = 1,
            ["Hull Rear"] = 2,
            ["Hull Front"] = 3,
            ["Turret Spare"] = 4
        };

        static GameObject soviet_crew_voice;

        public static void Config(MelonPreferences_Category cfg)
        {
            var racks = new List<string>()
            {
                "Hull Wet", 
                "Hull Rear",
                "Hull Front",
                "Turret Spare",
            }; 

            t72_patch = cfg.CreateEntry<bool>("T-72 Patch", true);
            t72_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";

            t72m_ammo_type = cfg.CreateEntry<string>("AP Round (T-72M)", "3BM22");
            t72m_ammo_type.Comment = "3BM15, 3BM22, 3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized)";

            t72m1_ammo_type = cfg.CreateEntry<string>("AP Round (T-72M1)", "3BM32");

            t72m_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-72M)", false);
            t72m_random_ammo.Comment = "Randomizes ammo selection for T-72Ms (3BM22, 3BM26, 3BM32, 3BM42)";

            t72m1_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-72M1)", false);

            thermals = cfg.CreateEntry<bool>("Has Thermals", true);
            thermals.Comment = "Replaces night vision sight with thermal sight";
            thermals.Description = " ";

            thermals_boxing = cfg.CreateEntry<bool>("Disable Thermal Boxing", false);
            thermals_boxing.Comment = "Removes the box border around the thermal sight";

            thermals_blur = cfg.CreateEntry<float>("Thermals Blur", 0.30f);
            thermals_blur.Comment = "Default: 0.30 (higher = more blurry, lower = less blurry)";

            era_t72m1 = cfg.CreateEntry<bool>("Kontakt-1 ERA (T-72M1)", true);
            era_t72m1.Comment = "BRICK ME UP LADS";
            era_t72m1.Description = " ";

            era_t72m = cfg.CreateEntry<bool>("Kontakt-1 ERA (T-72M)", false);
            era_t72m.Comment = "BRICK ME UP LADS (gill variants will not have side-skirt ERA)";

            only_carousel = cfg.CreateEntry<bool>("Reduced Ammo Load", false);
            only_carousel.Comment = "Allows you to specify which ammo racks should be emptied (except carousel)";
            only_carousel.Description = " ";

            empty_ammo_t72m = cfg.CreateEntry<List<string>>("Empty Ammo Racks (T-72M)", racks);
            empty_ammo_t72m1 = cfg.CreateEntry<List<string>>("Empty Ammo Racks (T-72M1)", racks);
            empty_ammo_t72m.Comment = "Hull Wet, Hull Rear, Hull Front, Turret Spare";

            soviet_t72m = cfg.CreateEntry<bool>("Soviet Crew (T-72M)", false);
            soviet_t72m.Comment = "Also renames the tank to T-72 and removes NVA decals";
            soviet_t72m.Description = " ";

            soviet_t72m1 = cfg.CreateEntry<bool>("Soviet Crew (T-72M1)", false);
            soviet_t72m1.Comment = "Also renames the tank to T-72A and removes NVA decals";

            super_fcs_t72m = cfg.CreateEntry<bool>("Super FCS (T-72M)", false);
            super_fcs_t72m.Comment = "basically sosna-u lol (digital 4x-12x zoom, 2-axis stabilizer w/ lead, point-n-shoot)";
            super_fcs_t72m.Description = " ";

            super_fcs_t72m1 = cfg.CreateEntry<bool>("Super FCS (T-72M1)", false);
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (GameObject armor_go in GameObject.FindGameObjectsWithTag("Penetrable"))
            {
                if (!era_t72m1.Value && !era_t72m.Value) break;

                if (Kontakt1.kontakt_1_hull_array == null) continue;
                if (Kontakt1.kontakt_1_turret_array == null) continue;
                if (!armor_go.GetComponent<LateFollow>()) continue;

                IUnit parent = armor_go.GetComponent<LateFollow>().ParentUnit;
                string name = parent.FriendlyName;

                bool t72m = era_t72m.Value && name == "T-72M";
                bool t72m1 = era_t72m1.Value && name == "T-72M1";

                if (!t72m && !t72m1) continue;

                if (armor_go.name == "T-72 HULL COLLIDERS")
                {
                    if (armor_go.transform.Find("ARMOR/hull era array(Clone)")) continue;
                    GameObject hull_array = GameObject.Instantiate(Kontakt1.kontakt_1_hull_array, armor_go.transform.Find("ARMOR"));
                    hull_array.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                    hull_array.transform.localPosition = new Vector3(-0.8219f, 0.7075f, 2.4288f);

                    if (t72m && parent.transform.name.ToLower().Contains("gill"))
                    {
                        GameObject.Destroy(hull_array.transform.Find("left side skirt array").gameObject);
                        GameObject.Destroy(hull_array.transform.Find("right side skirt array").gameObject);
                    }
                }

                if (armor_go.name == "T-72 TURRET COLLIDERS")
                {
                    if (armor_go.transform.Find("ARMOR/turret era array(Clone)")) continue;
                    GameObject turret_array = GameObject.Instantiate(Kontakt1.kontakt_1_turret_array, armor_go.transform.Find("ARMOR"));
                    turret_array.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                    turret_array.transform.localPosition = new Vector3(0.0199f, 2.1973f, -0.8363f);
                }
            }

            foreach (GameObject vic_go in PactIncreasedLethalityMod.vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (!vic.FriendlyName.Contains("T-72")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                // SOVIET CREW 
                if ((soviet_t72m1.Value && vic.UniqueName == "T72A") || (soviet_t72m.Value && vic.UniqueName == "T72M"))
                {
                    if (vic.FriendlyName == "T-72M1")
                    {
                        vic._friendlyName = "T-72A";
                    }
                    else if (vic.FriendlyName == "T-72M")
                    {
                        vic._friendlyName = "T-72";
                    }

                    vic.transform.Find("DE Tank Voice").gameObject.SetActive(false);
                    GameObject crew_voice = GameObject.Instantiate(soviet_crew_voice, vic.transform);
                    crew_voice.transform.localPosition = new Vector3(0, 0, 0);
                    crew_voice.transform.localEulerAngles = new Vector3(0, 0, 0);
                    CrewVoiceHandler handler = crew_voice.GetComponent<CrewVoiceHandler>();
                    handler._chassis = vic._chassis as NwhChassis;
                    handler._reloadType = CrewVoiceHandler.ReloaderType.AutoLoaderAZ;
                    vic._crewVoiceHandler = handler;
                    crew_voice.SetActive(true);


                    if (vic.UniqueName == "T72A")
                    {
                        vic.AimablePlatforms[1].transform.parent.Find("T72_markings").GetChild(2).gameObject.SetActive(false);
                        vic.AimablePlatforms[1].transform.parent.Find("T72_markings").GetChild(4).gameObject.SetActive(false);
                    }
                    else
                    {
                        vic.AimablePlatforms[1].transform.parent.Find("T72_markings").GetChild(2).gameObject.SetActive(false);
                        vic.AimablePlatforms[1].transform.parent.Find("T72_markings").GetChild(3).gameObject.SetActive(false);
                    }
                }

                if ((era_t72m1.Value && (vic.FriendlyName == "T-72M1" || vic.FriendlyName == "T-72A"))
                    || (era_t72m.Value && (vic.FriendlyName == "T-72M" || vic.FriendlyName == "T-72")))
                {
                    vic.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    vic._friendlyName += "V";
                }

                vic.AimablePlatforms[1].transform.Find("optic cover parent").gameObject.SetActive(false);

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                UsableOptic night_optic = fcs.NightOptic;
                UsableOptic day_optic = Util.GetDayOptic(fcs);

                string ammo_str = (vic.UniqueName == "T72M") ? t72m_ammo_type.Value : t72m1_ammo_type.Value;
                int rand = UnityEngine.Random.Range(0, ap.Count);

                if (t72m_random_ammo.Value && vic.UniqueName == "T72M")
                    ammo_str = ap.ElementAt(rand).Key;

                if (t72m1_random_ammo.Value && vic.UniqueName == "T72A")
                    ammo_str = ap.ElementAt(rand).Key;

                try
                {
                    AmmoClipCodexScriptable codex = ap[ammo_str];
                    loadout_manager.LoadedAmmoTypes[0] = codex;
                    //loadout_manager.LoadedAmmoTypes[2] = clip_codex_3of26_vt;
                    for (int i = 0; i <= 4; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        rack.ClipTypes[0] = codex.ClipType;
                        //rack.ClipTypes[2] = clip_codex_3of26_vt.ClipType;
                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();

                    if (only_carousel.Value)
                    {
                        var to_empty = vic.UniqueName == "T72M" ? empty_ammo_t72m.Value : empty_ammo_t72m1.Value;

                        foreach (string rack in to_empty) {
                            int idx = ammo_racks[rack];
                            Util.EmptyRack(loadout_manager.RackLoadouts[idx].Rack);
                        }
                    }
                }
                catch (Exception)
                {
                    MelonLogger.Msg("Loading default 3BM15 for " + vic.FriendlyName);
                }

                // THERMALS
                if (thermals.Value)
                {
                    // set night optic to thermal 
                    night_optic.slot.VisionType = NightVisionType.Thermal;
                    night_optic.slot.BaseBlur = thermals_blur.Value;

                    PostProcessProfile post = night_optic.post.profile;
                    ColorGrading color_grading = post.settings[1] as ColorGrading;
                    color_grading.postExposure.value = 2f;
                    color_grading.colorFilter.value = new Color(0.75f, 0.75f, 0.75f);
                    color_grading.lift.value = new Vector4(0f, 0f, 0f, -1.2f);
                    color_grading.lift.overrideState = true;

                    GameObject s = GameObject.Instantiate(scanline_canvas);
                    s.GetComponent<Reparent>().NewParent = night_optic.transform;
                    s.SetActive(true);

                    if (!thermals_boxing.Value)
                    {
                        // pos, rot, scale
                        List<List<Vector3>> backdrop_locs = new List<List<Vector3>>() {
                        new List<Vector3> {new Vector3(0f, -318.7f, 0f), new Vector3(0f, 0f, 180f)},
                        new List<Vector3> {new Vector3(0f, 318.7f, 0f), new Vector3(0f, 0f, 0f)},
                        new List<Vector3> {new Vector3(330f, 0f, 0f), new Vector3(0f, 0f, 90f)},
                        new List<Vector3> {new Vector3(-330f, 0f, 0f), new Vector3(0f, 0f, 270f)}
                    };

                        for (int i = 0; i <= 3; i++)
                        {
                            GameObject t = GameObject.Instantiate(thermal_canvas);
                            t.GetComponent<Reparent>().NewParent = night_optic.transform;
                            t.transform.GetChild(0).localPosition = backdrop_locs[i][0];
                            t.transform.GetChild(0).localEulerAngles = backdrop_locs[i][1];
                            if (i == 2 || i == 3)
                                t.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
                            t.SetActive(true);
                        }
                    }

                    GameObject reticle_go = GameObject.Instantiate(night_optic.reticleMesh.gameObject);
                    GameObject.Destroy(night_optic.reticleMesh.gameObject);
                    reticle_go.AddComponent<Reparent>();
                    reticle_go.GetComponent<Reparent>().NewParent = night_optic.transform;
                    reticle_go.GetComponent<Reparent>().Awake();
                    reticle_go.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    night_optic.reticleMesh = reticle_go.GetComponent<ReticleMesh>();

                    // generate reticle for t72 thermal sight
                    if (!reticleSO)
                    {
                        ReticleTree.Angular reticle = null;
                        reticleSO = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55-NVS"].tree);
                        reticleSO.name = "T72-TIS";

                        Util.ShallowCopy(reticle_cached, ReticleMesh.cachedReticles["T55-NVS"]);
                        reticle_cached.tree = reticleSO;

                        reticle_cached.tree.lights = new List<ReticleTree.Light>() {
                            new ReticleTree.Light()
                        };

                        reticle_cached.tree.lights[0].type = ReticleTree.Light.Type.Powered;
                        reticle_cached.tree.lights[0].color = new RGB(1.1f, -0.35f, -0.35f, true);

                        reticle = (reticleSO.planes[0].elements[0] as ReticleTree.Angular).elements[0] as ReticleTree.Angular;
                        (reticleSO.planes[0].elements[0] as ReticleTree.Angular).align = ReticleTree.GroupBase.Alignment.Boresight;
                        reticle.align = ReticleTree.GroupBase.Alignment.Boresight;
                        reticle_cached.mesh = null;

                        reticle.elements.RemoveAt(4);
                        reticle.elements.RemoveAt(1);
                        reticle.elements.RemoveAt(0);
                        reticle.elements[0].rotation.mrad = 0;
                        reticle.elements[0].position.x = 0;
                        reticle.elements[0].position.y = 0;
                        reticle.elements[1].position.y = 0;
                        (reticle.elements[0] as ReticleTree.Line).length.mrad = 10.0944f;
                        (reticle.elements[1] as ReticleTree.Line).length.mrad = 4.0944f;
                    }

                    night_optic.reticleMesh.reticleSO = reticleSO;
                    night_optic.reticleMesh.reticle = reticle_cached;
                    night_optic.reticleMesh.SMR = null;
                    night_optic.reticleMesh.Load();
                }

                // SOSNA U
                if ((super_fcs_t72m1.Value && vic.UniqueName == "T72A") || (super_fcs_t72m.Value && vic.UniqueName == "T72M"))
                {
                    if (!ReticleMesh.cachedReticles.ContainsKey("T55"))
                    {
                        foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
                        {
                            if (obj.name == "T55A")
                            {
                                obj.GetComponent<Vehicle>().GetComponent<WeaponsManager>().Weapons[0].FCS.AuthoritativeOptic.reticleMesh.Load();
                                break;
                            }
                        }
                    }

                    if (!reticleSO_sosna)
                    {
                        reticleSO_sosna = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55"].tree);
                        reticleSO_sosna.name = "sosna u";

                        Util.ShallowCopy(reticle_cached_sosna, ReticleMesh.cachedReticles["T55"]);
                        reticle_cached_sosna.tree = reticleSO_sosna;

                        reticle_cached_sosna.tree.lights = new List<ReticleTree.Light>() {
                            new ReticleTree.Light(),
                        };

                        Util.ShallowCopy(reticle_cached_sosna.tree.lights[0], ReticleMesh.cachedReticles["T55"].tree.lights[0]);
                        reticle_cached_sosna.tree.lights[0].type = ReticleTree.Light.Type.Powered;
                        reticle_cached_sosna.tree.lights[0].color = new RGB(2f, -0.3f, -0.3f, true);
                        reticle_cached_sosna.mesh = null;

                        ReticleTree.Angular impact = new ReticleTree.Angular(new Vector2(), null);
                        impact.name = "Impact";
                        impact.align = ReticleTree.GroupBase.Alignment.Impact;

                        reticleSO_sosna.planes[0].elements = new List<ReticleTree.TransformElement>();
                        ReticleTree.Angular eeeee = new ReticleTree.Angular(new Vector2(), null);
                        eeeee.name = "Angular";
                        eeeee.align = ReticleTree.GroupBase.Alignment.None;


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
                    fcs.CurrentRange = 200f;
                    fcs.GatedAimablePlatforms = fcs.Mounts;
                    fcs.FireGateOverrideTransform = null;
                    //fcs.transform.localPosition = new Vector3(-0.603f, 0.6288f, -5.547f);
                    fcs.transform.localPosition = new Vector3(-0.803f, 0.32f, -5.547f);
                    fcs.LaserOrigin = fcs.transform;

                    vic.GetComponent<WeaponsManager>().Weapons[1].ExcludeFromFcsUpdates = false;
                    vic.GetComponent<WeaponsManager>().Weapons[1].PreAimWeapon = WeaponSystemRole.Coaxial;

                    night_optic.Alignment = OpticAlignment.BoresightStabilized;
                    night_optic.RotateAzimuth = true;

                    day_optic.slot.DefaultFov = 13f;
                    day_optic.slot.SpriteType = CameraSpriteManager.SpriteType.NightVisionGoggles;

                    List<float> fovs = new List<float>();
                    for (float i = 12; i >= 4; i--)
                    {
                        fovs.Add(i);
                    }
                    day_optic.slot.OtherFovs = fovs.ToArray<float>();

                    day_optic.gameObject.AddComponent<DigitalZoomSnapper>();
                    day_optic.UseRotationForShake = true;
                    day_optic.CantCorrect = true;
                    day_optic.CantCorrectMaxSpeed = 5f;
                    day_optic.Alignment = OpticAlignment.FcsRange;
                    day_optic.slot.VibrationShakeMultiplier = 0f;
                    day_optic.slot.VibrationBlurScale = 0f;
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
                    rangebox.transform.GetChild(0).transform.localPosition = new Vector3(-2.1709f, -393.7738f, 0f);
                    rangebox.transform.GetChild(0).transform.localEulerAngles = new Vector3(0f, 0f, 180f);
                    rangebox.transform.GetChild(0).transform.localScale = new Vector3(0.1f, 1f, 1f);

                    GameObject range = GameObject.Instantiate(range_readout);
                    range.GetComponent<Reparent>().NewParent = rangebox.transform;
                    range.GetComponent<Reparent>().Awake();
                    range.SetActive(true);
                    range.transform.localPosition = new Vector3(0f, 0f, 0f);
                    range.transform.GetChild(1).transform.localPosition = new Vector3(-10f, -285.2727f, 0f);
                    day_optic.RangeText = range.GetComponentInChildren<TMP_Text>();
                    range.GetComponentInChildren<TMP_Text>().outlineWidth = 1f;
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
                }
            }

            yield break;
        }

        public static void Init()
        {
            if (!t72_patch.Value) return;

            if (!range_readout)
            {
                foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
                {
                    if (obj.name == "M1IP")
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

                        break;
                    }
                }
            }

            if (soviet_crew_voice == null)
            {
                foreach (CrewVoiceHandler obj in Resources.FindObjectsOfTypeAll(typeof(CrewVoiceHandler)))
                {
                    if (obj.name == "RU Tank Voice")
                    {
                        soviet_crew_voice = obj.gameObject;
                        break;
                    }
                }
            }

            // thermal border
            if (thermal_canvas == null)
            {
                foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
                {
                    if (obj.name == "M2 Bradley")
                    {
                        thermal_canvas = GameObject.Instantiate(obj.transform.Find("FCS and sights/GPS Optic/M2 Bradley GPS canvas").gameObject);
                        GameObject.Destroy(thermal_canvas.transform.GetChild(2).gameObject);
                        thermal_canvas.AddComponent<Reparent>();
                        thermal_canvas.SetActive(false);
                        thermal_canvas.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        thermal_canvas.name = "t72 thermal canvas";
                    }

                    if (obj.name == "M60A3")
                    {
                        scanline_canvas = GameObject.Instantiate(obj.transform.Find("Turret Scripts/Sights/FLIR/Canvas Scanlines").gameObject);
                        scanline_canvas.AddComponent<Reparent>();
                        scanline_canvas.SetActive(false);
                        scanline_canvas.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        scanline_canvas.name = "t72 scanline canvas";
                    }

                    if (scanline_canvas != null && thermal_canvas != null) break;
                }
            }

            if (ammo_3bm26 == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "3BM15 APFSDS-T") { ammo_3bm15 = s.AmmoType; }
                    if (s.AmmoType.Name == "3OF26 HEF-FS-T") { ammo_3of26 = s.AmmoType; }

                    if (ammo_3bm15 != null && ammo_3of26 != null) break;
                }

                foreach (AmmoClipCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoClipCodexScriptable)))
                {
                    if (s.name == "clip_3BM22") { clip_codex_3bm22 = s; }
                    if (s.name == "clip_3BM32") { clip_codex_3bm32 = s; }

                    if (clip_codex_3bm22 != null && clip_codex_3bm32 != null) break;
                }

                var composite_optimizations_3bm26 = new List<AmmoType.ArmorOptimization>() { };
                var composite_optimizations_3bm42 = new List<AmmoType.ArmorOptimization>() { };

                string[] composite_names = new string[] {
                    "Abrams special armor gen 1 hull front",
                    "Abrams special armor gen 1 mantlet",
                    "Abrams special armor gen 1 turret cheeks",
                    "Abrams special armor gen 1 turret sides",
                    "Abrams special armor gen 0 turret cheeks",
                    "Corundum ball armor",
                    "Kvartz"
                };

                foreach (ArmorCodexScriptable s in Resources.FindObjectsOfTypeAll<ArmorCodexScriptable>())
                {
                    if (composite_names.Contains(s.name) || (s.name.Contains("Abrams") && s.name.Contains("composite")))
                    {
                        AmmoType.ArmorOptimization optimization_3bm26 = new AmmoType.ArmorOptimization();
                        optimization_3bm26.Armor = s;
                        optimization_3bm26.RhaRatio = 0.80f;
                        composite_optimizations_3bm26.Add(optimization_3bm26);

                        AmmoType.ArmorOptimization optimization_3bm42 = new AmmoType.ArmorOptimization();
                        optimization_3bm42.Armor = s;
                        optimization_3bm42.RhaRatio = 0.78f;
                        composite_optimizations_3bm42.Add(optimization_3bm42);
                    }

                    if (composite_optimizations_3bm26.Count == composite_names.Length) break;
                }

                ammo_3bm26 = new AmmoType();
                Util.ShallowCopy(ammo_3bm26, ammo_3bm15);
                ammo_3bm26.Name = "3BM26 APFSDS-T";
                ammo_3bm26.Caliber = 125;
                ammo_3bm26.RhaPenetration = 440f;
                ammo_3bm26.Mass = 4.8f;
                ammo_3bm26.MuzzleVelocity = 1720f;
                ammo_3bm26.ArmorOptimizations = composite_optimizations_3bm26.ToArray<AmmoType.ArmorOptimization>();
                ammo_3bm26.SpallMultiplier = 0.9f;

                ammo_codex_3bm26 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3bm26.AmmoType = ammo_3bm26;
                ammo_codex_3bm26.name = "ammo_3bm26";

                clip_3bm26 = new AmmoType.AmmoClip();
                clip_3bm26.Capacity = 1;
                clip_3bm26.Name = "3BM26 APFSDS-T";
                clip_3bm26.MinimalPattern = new AmmoCodexScriptable[1];
                clip_3bm26.MinimalPattern[0] = ammo_codex_3bm26;

                clip_codex_3bm26 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3bm26.name = "clip_3bm26";
                clip_codex_3bm26.ClipType = clip_3bm26;

                ammo_3bm26_vis = GameObject.Instantiate(ammo_3bm15.VisualModel);
                ammo_3bm26_vis.name = "3bm26 visual";
                ammo_3bm26.VisualModel = ammo_3bm26_vis;
                ammo_3bm26.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm26;
                ammo_3bm26.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm26;

                ammo_3bm42 = new AmmoType();
                Util.ShallowCopy(ammo_3bm42, ammo_3bm15);
                ammo_3bm42.Name = "3BM42 APFSDS-T";
                ammo_3bm42.Caliber = 125;
                ammo_3bm42.RhaPenetration = 520f;
                ammo_3bm42.Mass = 4.85f;
                ammo_3bm42.MuzzleVelocity = 1700f;
                ammo_3bm42.SpallMultiplier = 0.95f;
                ammo_3bm42.ArmorOptimizations = composite_optimizations_3bm42.ToArray<AmmoType.ArmorOptimization>();

                ammo_codex_3bm42 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3bm42.AmmoType = ammo_3bm42;
                ammo_codex_3bm42.name = "ammo_3bm42";

                clip_3bm42 = new AmmoType.AmmoClip();
                clip_3bm42.Capacity = 1;
                clip_3bm42.Name = "3BM42 APFSDS-T";
                clip_3bm42.MinimalPattern = new AmmoCodexScriptable[1];
                clip_3bm42.MinimalPattern[0] = ammo_codex_3bm42;

                clip_codex_3bm42 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3bm42.name = "clip_3bm42";
                clip_codex_3bm42.ClipType = clip_3bm42;

                ammo_3bm42_vis = GameObject.Instantiate(ammo_3bm15.VisualModel);
                ammo_3bm42_vis.name = "3bm42 visual";
                ammo_3bm42.VisualModel = ammo_3bm42_vis;
                ammo_3bm42.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm42;
                ammo_3bm42.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm42;

                ammo_3of26_vt = new AmmoType();
                Util.ShallowCopy(ammo_3of26_vt, ammo_3of26);
                ammo_3of26_vt.Name = "3OF26M HE-T PF";
                ammo_3of26_vt.MinSpallRha = 20f;
                ammo_3of26_vt.MaxSpallRha = 60f;
                //ammo_3of26_vt.MuzzleVelocity = 1360f;
                ammo_3of26_vt.Coeff = ammo_3bm42.Coeff / 2f;

                ammo_codex_3of26_vt = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3of26_vt.AmmoType = ammo_3of26_vt;
                ammo_codex_3of26_vt.name = "ammo_3of26_vt";

                clip_3of26_vt = new AmmoType.AmmoClip();
                clip_3of26_vt.Capacity = 1;
                clip_3of26_vt.Name = "3OF26M HE-T PF";
                clip_3of26_vt.MinimalPattern = new AmmoCodexScriptable[1];
                clip_3of26_vt.MinimalPattern[0] = ammo_codex_3of26_vt;

                clip_codex_3of26_vt = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3of26_vt.name = "clip_3of26_vt";
                clip_codex_3of26_vt.ClipType = clip_3of26_vt;

                ammo_3of26_vt_vis = GameObject.Instantiate(ammo_3of26.VisualModel);
                ammo_3of26_vt_vis.name = "3OF26 VT visual";
                ammo_3of26_vt.VisualModel = ammo_3of26_vt_vis;
                ammo_3of26_vt.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3of26_vt;
                ammo_3of26_vt.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3of26_vt;

                ProximityFuse.AddProximityFuse(ammo_3of26_vt);

                ap = new Dictionary<string, AmmoClipCodexScriptable>()
                {
                    ["3BM22"] = clip_codex_3bm22,
                    ["3BM26"] = clip_codex_3bm26,
                    ["3BM32"] = clip_codex_3bm32,
                    ["3BM42"] = clip_codex_3bm42,
                };
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
