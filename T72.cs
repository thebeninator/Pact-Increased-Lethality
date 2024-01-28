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

namespace PactIncreasedLethality
{
    public class T72
    {
        static GameObject thermal_canvas;
        static GameObject scanline_canvas;
        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static AmmoClipCodexScriptable clip_codex_3bm22;

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

        static AmmoType ammo_3bm15;

        static MelonPreferences_Entry<bool> t72_patch;
        static MelonPreferences_Entry<string> t72m_ammo_type;
        static MelonPreferences_Entry<string> t72m1_ammo_type;


        static MelonPreferences_Entry<bool> thermals;
        static MelonPreferences_Entry<bool> thermals_boxing;
        static MelonPreferences_Entry<float> thermals_blur;

        static MelonPreferences_Entry<bool> era_t72m1;
        static MelonPreferences_Entry<bool> era_t72m;

        static Dictionary<string, AmmoClipCodexScriptable> ap;

        public static void Config(MelonPreferences_Category cfg)
        {
            t72_patch = cfg.CreateEntry<bool>("T-72 Patch", true);
            t72_patch.Description = "///////////////";

            t72m_ammo_type = cfg.CreateEntry<string>("AP Round (T-72M)", "3BM22");
            t72m_ammo_type.Comment = "3BM15, 3BM22, 3BM26, ???";

            t72m1_ammo_type = cfg.CreateEntry<string>("AP Round (T-72M1)", "3BM22");
            t72m1_ammo_type.Comment = "3BM15, 3BM22, 3BM26, ???";

            thermals = cfg.CreateEntry<bool>("Has Thermals", true);
            thermals.Comment = "Replaces night vision sight with thermal sight";

            thermals_boxing = cfg.CreateEntry<bool>("Disable Thermal Boxing", false);
            thermals_boxing.Comment = "Removes the box border around the thermal sight";

            thermals_blur = cfg.CreateEntry<float>("Thermals Blur", 0.30f);
            thermals_blur.Comment = "Default: 0.30 (higher = more blurry, lower = less blurry)"; 

            era_t72m1 = cfg.CreateEntry<bool>("Kontakt-1 ERA (T-72M1)", true);
            era_t72m1.Comment = "BRICK ME UP LADS";

            era_t72m = cfg.CreateEntry<bool>("Kontakt-1 ERA (T-72M)", false);
            era_t72m.Comment = "BRICK ME UP LADS (gill variants will not have side-skirt ERA)";
        }

        public static IEnumerator Convert(GameState _) {
            foreach (GameObject armor_go in GameObject.FindGameObjectsWithTag("Penetrable")) {
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

                    if (t72m && parent.transform.name.ToLower().Contains("gill")) {
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

                if ((era_t72m1.Value && vic.FriendlyName == "T-72M1") || (vic.FriendlyName == "T-72M" && era_t72m.Value)) {
                    vic.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    vic._friendlyName += "V";
                }     

                vic.AimablePlatforms[1].transform.Find("optic cover parent").gameObject.SetActive(false);

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                UsableOptic night_optic = fcs.NightOptic;

                // set night optic to thermal 
                if (thermals.Value)
                {
                    night_optic.slot.VisionType = NightVisionType.Thermal;
                    night_optic.slot.BaseBlur = thermals_blur.Value;

                    PostProcessProfile post = night_optic.post.profile;
                    ColorGrading color_grading = post.settings[1] as ColorGrading;
                    color_grading.postExposure.value = 2f;
                    color_grading.colorFilter.value = new Color(0.75f, 0.75f, 0.75f);
                    color_grading.lift.value = new Vector4(0f, 0f, 0f, -1.2f);
                    color_grading.lift.overrideState = true;
                }

                string ammo_str = (vic.UniqueName == "T72M") ? t72m_ammo_type.Value : t72m1_ammo_type.Value;

                try
                {
                    AmmoClipCodexScriptable codex = ap[ammo_str];
                    loadout_manager.LoadedAmmoTypes[0] = codex;
                    for (int i = 0; i <= 2; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        rack.ClipTypes[0] = codex.ClipType;
                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();
                } catch (Exception) {
                    MelonLogger.Msg("Loading default AP round for " + vic.FriendlyName);
                }
                
                // everything below is for creating the border & reticle for the thermal sight
                if (night_optic.transform.Find("t72 thermal canvas(Clone)") || !thermals.Value)
                {
                    continue;
                }

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
                    reticle_cached.tree.lights[0].color = new RGB(-255f, -255f, -255f, true);

                    reticle = (reticleSO.planes[0].elements[0] as ReticleTree.Angular).elements[0] as ReticleTree.Angular;
                    reticle_cached.mesh = null;

                    reticle.elements.RemoveAt(4);
                    reticle.elements.RemoveAt(1);
                    reticle.elements.RemoveAt(0);
                    reticle.elements[0].rotation.mrad = 0;
                    reticle.elements[0].position.x = 0;
                    reticle.elements[0].position.y = -0.9328f;
                    (reticle.elements[0] as ReticleTree.Line).length.mrad = 4.0944f;
                    (reticle.elements[1] as ReticleTree.Line).length.mrad = 4.0944f;
                }

                night_optic.reticleMesh.reticleSO = reticleSO;
                night_optic.reticleMesh.reticle = reticle_cached;
                night_optic.reticleMesh.SMR = null;
                night_optic.reticleMesh.Load();
            }

            yield break;
        }

        public static void Init() {

            if (!t72_patch.Value) return;

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
                    if (s.AmmoType.Name == "3BM15 APFSDS-T") { ammo_3bm15 = s.AmmoType; break; }
                }

                foreach (AmmoClipCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoClipCodexScriptable)))
                {
                    if (s.name == "clip_3BM22") {clip_codex_3bm22 = s; break; }
                }

                ammo_3bm26 = new AmmoType();
                Util.ShallowCopy(ammo_3bm26, ammo_3bm15);
                ammo_3bm26.Name = "3BM26 APFSDS-T";
                ammo_3bm26.Caliber = 125;
                ammo_3bm26.RhaPenetration = 490f;
                ammo_3bm26.Mass = 4.8f;
                ammo_3bm26.MuzzleVelocity = 1720f;

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
                ammo_3bm42.RhaPenetration = 585f;
                ammo_3bm42.Mass = 4.85f;
                ammo_3bm42.MuzzleVelocity = 1700f;

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

                ap = new Dictionary<string, AmmoClipCodexScriptable>()
                {
                    ["3BM22"] = clip_codex_3bm22,
                    ["3BM26"] = clip_codex_3bm26,
                    ["3BM42"] = clip_codex_3bm42,
                };
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }
}
