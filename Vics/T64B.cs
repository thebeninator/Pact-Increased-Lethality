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
using UnityEngine;
using NWH.VehiclePhysics;
using static PactIncreasedLethality.T80;
using GHPC.Weaponry;

namespace PactIncreasedLethality
{
    public class T64B
    {
        static MelonPreferences_Entry<bool> t64_patch;
        static MelonPreferences_Entry<bool> super_engine;
        static MelonPreferences_Entry<string> t64_ammo_type;
        static MelonPreferences_Entry<bool> t64_random_ammo;
        static MelonPreferences_Entry<List<string>> t64_random_ammo_pool;
        static MelonPreferences_Entry<bool> has_drozd;
        static MelonPreferences_Entry<bool> thermals;
        static MelonPreferences_Entry<string> thermals_quality;
        static MelonPreferences_Entry<bool> du_armour;
        static MelonPreferences_Entry<bool> zoom_snapper;

        public static void Config(MelonPreferences_Category cfg)
        {
            var random_ammo_pool = new List<string>()
            {
                "3BM26",
                "3BM32",
                "3BM42",
                "3BM46"
            };

            t64_patch = cfg.CreateEntry<bool>("T-64B Patch", true);
            t64_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            super_engine = cfg.CreateEntry<bool>("Super Engine/Transmission (T-64B)", true);
            super_engine.Comment = "vrrrrrrrrrrooooooooom";

            t64_ammo_type = cfg.CreateEntry<string>("AP Round (T-64B)", "3BM32");
            t64_ammo_type.Comment = "3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized), 3BM46";
            t64_ammo_type.Description = " ";

            t64_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-64B)", false);
            t64_random_ammo_pool = cfg.CreateEntry<List<string>>("Random AP Round Pool (T-64B)", random_ammo_pool);
            t64_random_ammo_pool.Comment = "3BM26, 3BM32, 3BM42, 3BM46";

            thermals = cfg.CreateEntry<bool>("Has Thermals (T-64B)", false);
            thermals.Description = " ";
            thermals.Comment = "Replaces night vision sight with thermal sight";
            thermals_quality = cfg.CreateEntry<string>("Thermals Quality (T-64B)", "High");
            thermals_quality.Comment = "Low, High";

            //has_drozd = cfg.CreateEntry<bool>("Drozd APS (T-64B)", false);
            //has_drozd.Comment = "Intercepts incoming projectiles; covers the frontal arc of the tank relative to where the turret is facing";

            zoom_snapper = cfg.CreateEntry<bool>("Quick Zoom Switch (T-64B)", true);
            zoom_snapper.Comment = "Press middle mouse to instantly switch between low and high magnification on the daysight";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (!vic.FriendlyName.Contains("T-64B")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();


                int rand = UnityEngine.Random.Range(0, Ammo_125mm.ap.Count);
                string ammo_str = t64_random_ammo.Value ? t64_random_ammo_pool.Value.ElementAt(rand) : t64_ammo_type.Value;

                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                UsableOptic day_optic = Util.GetDayOptic(fcs);
                UsableOptic night_optic = day_optic.slot.LinkedNightSight.PairedOptic;

                if (zoom_snapper.Value)
                    day_optic.gameObject.AddComponent<DigitalZoomSnapper>();

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

                Transform canvas = vic.transform.Find("---T64A_MESH---/HULL/TURRET/Main gun/---MAIN GUN SCRIPTS---/2A46/1G42 gunner's sight/GPS/1G42 Canvas/GameObject");
                canvas.Find("ammo text APFSDS (TMP)").gameObject.SetActive(true);

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
                    PactThermal.Add(night_optic, thermals_quality.Value.ToLower(), true);
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);

                    night_optic.Alignment = OpticAlignment.BoresightStabilized;
                    night_optic.RotateAzimuth = true;
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
