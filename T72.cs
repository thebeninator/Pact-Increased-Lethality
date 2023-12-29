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

namespace PactIncreasedLethality
{
    public class T72
    {
        static GameObject thermal_canvas;
        static GameObject scanline_canvas;
        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static AmmoClipCodexScriptable clip_codex_3bm22;
        static AmmoType.AmmoClip clip_3bm22;
        static AmmoCodexScriptable ammo_codex_3bm22;
        static AmmoType ammo_3bm22;
        static GameObject ammo_3bm22_vis = null;

        static AmmoType ammo_3bm15;

        static MelonPreferences_Entry<bool> t72_patch;
        static MelonPreferences_Entry<bool> use_3bm22;
        static MelonPreferences_Entry<bool> thermals;

        public static IEnumerator Convert(GameState _) {
            foreach (GameObject vic_go in PactIncreasedLethalityMod.vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (!vic.FriendlyName.Contains("T-72")) continue;

                vic.AimablePlatforms[1].transform.Find("optic cover parent").gameObject.SetActive(false);

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                UsableOptic night_optic = fcs.NightOptic;

                // set night optic to thermal 
                if (thermals.Value)
                {
                    night_optic.slot.VisionType = NightVisionType.Thermal;
                    night_optic.slot.BaseBlur = 0.30f;

                    PostProcessProfile post = night_optic.post.profile;
                    ColorGrading color_grading = post.settings[1] as ColorGrading;
                    color_grading.postExposure.value = 2f;
                    color_grading.colorFilter.value = new Color(0.75f, 0.75f, 0.75f);
                    color_grading.lift.value = new Vector4(0f, 0f, 0f, -1.2f);
                    color_grading.lift.overrideState = true;
                }

                if (use_3bm22.Value)
                {
                    loadout_manager.LoadedAmmoTypes[0] = clip_codex_3bm22;
                    for (int i = 0; i <= 2; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        rack.ClipTypes[0] = clip_codex_3bm22.ClipType;
                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();
                }

                // everything below is for creating the border & reticle for the thermal sight
                if (night_optic.transform.Find("t72 thermal canvas(Clone)") || !thermals.Value)
                {
                    continue;
                }

                GameObject s = GameObject.Instantiate(scanline_canvas);
                s.GetComponent<Reparent>().NewParent = night_optic.transform;
                s.SetActive(true);

                // pos, rot 
                List<List<Vector3>> backdrop_locs = new List<List<Vector3>>() {
                    new List<Vector3> {new Vector3(0f, -318.7f, 0f), new Vector3(0f, 0f, 180f)},
                    new List<Vector3> {new Vector3(0f, 318.7f, 0f), new Vector3(0f, 0f, 0f)},
                    new List<Vector3> {new Vector3(360f, 0f, 0f), new Vector3(0f, 0f, 90f)},
                    new List<Vector3> {new Vector3(-360f, 0f, 0f), new Vector3(0f, 0f, 270f)}
                };

                foreach (List<Vector3> b in backdrop_locs)
                {
                    GameObject t = GameObject.Instantiate(thermal_canvas);
                    t.GetComponent<Reparent>().NewParent = night_optic.transform;
                    t.transform.GetChild(0).localPosition = b[0];
                    t.transform.GetChild(0).localEulerAngles = b[1];
                    t.SetActive(true);
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

        public static void Config(MelonPreferences_Category cfg) {
            t72_patch = cfg.CreateEntry<bool>("T-72 Patch", true);
            t72_patch.Description = "///////////////";
            use_3bm22 = cfg.CreateEntry<bool>("Use 3BM22", true);
            use_3bm22.Comment = "Replaces 3BM15 (increased penetration)";
            thermals = cfg.CreateEntry<bool>("Has Thermals", true);
            thermals.Comment = "Replaces night vision sight with thermal sight";
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
                }
            }

            if (ammo_3bm22 == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "3BM15 APFSDS-T") ammo_3bm15 = s.AmmoType;           
                }

                ammo_3bm22 = new AmmoType();
                Util.ShallowCopy(ammo_3bm22, ammo_3bm15);
                ammo_3bm22.Name = "3BM22 APFSDS-T";
                ammo_3bm22.Caliber = 125;
                ammo_3bm22.RhaPenetration = 480f;
                ammo_3bm22.Mass = 4.6f;

                ammo_codex_3bm22 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3bm22.AmmoType = ammo_3bm22;
                ammo_codex_3bm22.name = "ammo_3bm22";

                clip_3bm22 = new AmmoType.AmmoClip();
                clip_3bm22.Capacity = 1;
                clip_3bm22.Name = "3BM22 APFSDS-T";
                clip_3bm22.MinimalPattern = new AmmoCodexScriptable[1];
                clip_3bm22.MinimalPattern[0] = ammo_codex_3bm22;

                clip_codex_3bm22 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3bm22.name = "clip_3bm22";
                clip_codex_3bm22.ClipType = clip_3bm22;

                ammo_3bm22_vis = GameObject.Instantiate(ammo_3bm15.VisualModel);
                ammo_3bm22_vis.name = "3BM22 visual";
                ammo_3bm22.VisualModel = ammo_3bm22_vis;
                ammo_3bm22.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm22;
                ammo_3bm22.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm22;
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }
}
