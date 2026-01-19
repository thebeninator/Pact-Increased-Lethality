using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GHPC.Equipment.Optics;
using GHPC.State;
using GHPC.Vehicle;
using GHPC.Weapons;
using GHPC;
using MelonLoader;
using Reticle;
using UnityEngine;
using NWH.VehiclePhysics;
using GHPC.Weaponry;

namespace PactIncreasedLethality
{
    public class T64A
    {
        static MelonPreferences_Entry<bool> t64_patch;
        static MelonPreferences_Entry<bool> super_engine;
        static MelonPreferences_Entry<string> t64_ammo_type;
        static MelonPreferences_Entry<bool> t64_random_ammo;
        static MelonPreferences_Entry<bool> has_drozd;
        static MelonPreferences_Entry<bool> has_lrf;
        static MelonPreferences_Entry<bool> thermals;
        static MelonPreferences_Entry<string> thermals_quality;
        static MelonPreferences_Entry<bool> lead_calculator_t64;
        static MelonPreferences_Entry<bool> du_armour;
        static MelonPreferences_Entry<bool> better_stab;
        static MelonPreferences_Entry<bool> tpn3;
        static MelonPreferences_Entry<List<string>> t64_random_ammo_pool;

        public static void Config(MelonPreferences_Category cfg)
        {
            var random_ammo_pool = new List<string>()
            {
                "3BM26",
                "3BM32",
                "3BM42",
                "3BM46"
            };

            t64_patch = cfg.CreateEntry<bool>("T-64A Patch", true);
            t64_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            super_engine = cfg.CreateEntry<bool>("Super Engine/Transmission (T-64A)", true);
            super_engine.Comment = "vrrrrrrrrrrooooooooom";

            has_lrf = cfg.CreateEntry<bool>("Laser Rangefinder (T-64A)", true);
            has_lrf.Comment = "Replaces the coincidence rangefinder with a laser rangefinder";

            t64_ammo_type = cfg.CreateEntry<string>("AP Round (T-64A)", "3BM32");
            t64_ammo_type.Description = " ";
            t64_ammo_type.Comment = "3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized)";

            t64_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-64A)", false);
            t64_random_ammo.Comment = "Randomizes ammo selection for T-64As (3BM26, 3BM32, 3BM42), 3BM46";
            t64_random_ammo_pool = cfg.CreateEntry<List<string>>("Random AP Round Pool (T-64A)", random_ammo_pool);
            t64_random_ammo_pool.Comment = "3BM26, 3BM32, 3BM42, 3BM46";

            lead_calculator_t64 = cfg.CreateEntry<bool>("Lead Calculator (T-64A)", true);
            lead_calculator_t64.Comment = "For use with the standard sight; displays a number that corresponds to the horizontal markings on the sight (LRF required)";
            lead_calculator_t64.Description = " ";

            //has_drozd = cfg.CreateEntry<bool>("Drozd APS (T-64A)", false);
            //has_drozd.Comment = "Intercepts incoming projectiles; covers the frontal arc of the tank relative to where the turret is facing";

            better_stab = cfg.CreateEntry<bool>("Better Stabilizer (T-64A)", true);
            better_stab.Comment = "Less reticle blur, shake while on the move";

            tpn3 = cfg.CreateEntry<bool>("TPN-3 Night Sight (T-64A)", true);
            tpn3.Comment = "Replaces the night sight with the one found on the T-80B/T-64B";

            thermals = cfg.CreateEntry<bool>("Has Thermals (T-64A)", false);
            thermals.Comment = "Replaces night vision sight with thermal sight";
            thermals_quality = cfg.CreateEntry<string>("Thermals Quality (T-64A)", "High");
            thermals_quality.Comment = "Low, High";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (vic.UniqueName != "T64A") continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();

                int rand = UnityEngine.Random.Range(0, Ammo_125mm.ap.Count);
                string ammo_str = t64_random_ammo.Value ? t64_random_ammo_pool.Value.ElementAt(rand) : t64_ammo_type.Value;

                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                UsableOptic day_optic = Util.GetDayOptic(fcs);

                if (better_stab.Value)
                {
                    day_optic.slot.VibrationBlurScale = 0.05f;
                    day_optic.slot.VibrationShakeMultiplier = 0.1f;
                }

                if (tpn3.Value)
                {
                    TPN3.Add(fcs, day_optic.slot.LinkedNightSight.PairedOptic, day_optic.slot.LinkedNightSight);
                }

                if (has_lrf.Value)
                {
                    GameObject lase = GameObject.Instantiate(new GameObject("lase"), fcs.transform);

                    fcs.LaserAim = LaserAimMode.Fixed;
                    fcs.LaserOrigin = lase.transform;
                    fcs.MaxLaserRange = 4000f;

                    day_optic.reticleMesh.reticleSO = ReticleMesh.cachedReticles["T72"].tree;
                    day_optic.reticleMesh.reticle = ReticleMesh.cachedReticles["T72"];
                    day_optic.reticleMesh.SMR = null;
                    day_optic.reticleMesh.Load();

                    if (lead_calculator_t64.Value) 
                        FireControlSystem1A40.Add(fcs, day_optic, new Vector3(-308.8629f, -6.6525f, 0f));

                    fcs.Start();

                    fcs.OpticalRangefinder = null;
                }

                try
                {
                    if (ammo_str != "3BM15")
                        loadout_manager.LoadedAmmoList.AmmoClips[0] = Ammo_125mm.ap[ammo_str];

                    for (int i = 0; i < loadout_manager.RackLoadouts.Length; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();
                }
                catch (Exception)
                {
                    MelonLogger.Msg("Loading default ammo for " + vic.FriendlyName);
                }

                if (super_engine.Value)
                {
                    VehicleController this_vic_controller = vic_go.GetComponent<VehicleController>();
                    NwhChassis chassis = vic_go.GetComponent<NwhChassis>();

                    Util.ShallowCopy(this_vic_controller.engine, Assets.abrams_vic_controller.engine);
                    Util.ShallowCopy(this_vic_controller.transmission, Assets.abrams_vic_controller.transmission);

                    this_vic_controller.engine.vc = vic_go.GetComponent<VehicleController>();
                    this_vic_controller.transmission.vc = vic_go.GetComponent<VehicleController>();
                    this_vic_controller.engine.Initialize(this_vic_controller);
                    this_vic_controller.engine.Start();
                    this_vic_controller.transmission.Initialize(this_vic_controller);

                    chassis._maxForwardSpeed = 22f;
                    chassis._maxReverseSpeed = 15.176f;
                    chassis._originalEnginePower = 1430.99f;
                }

                //if (has_drozd.Value)
                //{
                //    List<DrozdLauncher> launchers = new List<DrozdLauncher>();

                //    Vector3[] launcher_positions = new Vector3[] {
                //        new Vector3(-1.2953f, -0.1483f, 0.3166f),
                //        new Vector3(-1.2243f, 0.0691f, 0.2969f),
                //        new Vector3(1.2953f, -0.1483f, 0.3166f),
                //        new Vector3(1.2243f, 0.0691f, 0.2969f),
                //    };

                //    Vector3[] launcher_rots = new Vector3[] {
                //        new Vector3(0f, 0f, 0f),
                //        new Vector3(0f, -17.8007f, 0f),
                //        new Vector3(0f, 0f, 0f),
                //        new Vector3(0f, 17.8007f, 0f)
                //    };

                //    for (var i = 0; i < launcher_positions.Length; i++)
                //    {
                //        GameObject launcher = GameObject.Instantiate(DrozdLauncher.drozd_launcher_visual, vic.transform.Find("---T64A_MESH---/HULL/TURRET"));
                //        launcher.transform.localPosition = launcher_positions[i];
                //        launcher.transform.localEulerAngles = launcher_rots[i];

                //        if (i > 1)
                //        {
                //            launcher.transform.localScale = Vector3.Scale(launcher.transform.localScale, new Vector3(-1f, 1f, 1f));
                //        }

                //        launchers.Add(launcher.GetComponent<DrozdLauncher>());
                //    }

                //    Drozd.AttachDrozd(
                //        vic.transform.Find("---T64A_MESH---/HULL/TURRET"), vic, new Vector3(0f, 0f, 9.5f),
                //        launchers.GetRange(0, 2).ToArray(), launchers.GetRange(2, 2).ToArray()
                //    );

                //    vic._friendlyName += "D";
                //}

                vic.AimablePlatforms[3].transform.Find("optic cover parent").gameObject.SetActive(false);

                if (thermals.Value)
                {
                    PactThermal.Add(day_optic.slot.LinkedNightSight.PairedOptic, thermals_quality.Value.ToLower());
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);
                }
            }

            yield break;
        }

        public static void Init()
        {
            if (!t64_patch.Value) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
