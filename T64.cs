﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Camera;
using GHPC.Equipment.Optics;
using GHPC.Player;
using GHPC.State;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using GHPC;
using MelonLoader;
using Reticle;
using TMPro;
using UnityEngine;
using NWH;
using NWH.VehiclePhysics;

namespace PactIncreasedLethality
{
    public class T64
    {
        static MelonPreferences_Entry<bool> t64_patch;
        static MelonPreferences_Entry<bool> super_engine;
        static VehicleController abrams_vic_controller;
        static Dictionary<string, AmmoClipCodexScriptable> ap;
        static MelonPreferences_Entry<string> t64_ammo_type;
        static MelonPreferences_Entry<bool> t64_random_ammo;
        static MelonPreferences_Entry<bool> has_drozd;

        public static void Config(MelonPreferences_Category cfg)
        {
            t64_patch = cfg.CreateEntry<bool>("T-64 Patch", true);
            t64_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            super_engine = cfg.CreateEntry<bool>("Super Engine/Transmission", true);
            super_engine.Comment = "vrrrrrrrrrrooooooooom";

            t64_ammo_type = cfg.CreateEntry<string>("AP Round (T-64A)", "3BM32");
            t64_ammo_type.Comment = "3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized)";

            t64_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-64A)", false);
            t64_random_ammo.Comment = "Randomizes ammo selection for T-64As (3BM26, 3BM32, 3BM42)";

            has_drozd = cfg.CreateEntry<bool>("Drozd APS (T-64A)", false);
            has_drozd.Comment = "Intercepts incoming projectiles; covers the frontal arc of the tank relative to where the turret is facing";
        }

        public static IEnumerator Convert(GameState _)
        {
            if (ap == null)
                ap = new Dictionary<string, AmmoClipCodexScriptable>()
                {
                    ["3BM32"] = APFSDS_125mm.clip_codex_3bm32,
                    ["3BM26"] = APFSDS_125mm.clip_codex_3bm26,
                    ["3BM42"] = APFSDS_125mm.clip_codex_3bm42,
                };

            foreach (GameObject vic_go in PactIncreasedLethalityMod.vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (!vic.FriendlyName.Contains("T-64")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();

                int rand = UnityEngine.Random.Range(0, ap.Count);
                string ammo_str = t64_random_ammo.Value ? ammo_str = ap.ElementAt(rand).Key : t64_ammo_type.Value;

                try
                {
                    AmmoClipCodexScriptable codex = ap[ammo_str];
                    loadout_manager.LoadedAmmoTypes[0] = codex;
                    for (int i = 0; i < loadout_manager.RackLoadouts.Length; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        rack.ClipTypes[0] = codex.ClipType;
                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();
                }
                catch (Exception)
                {
                    MelonLogger.Msg("Loading default 3BM32 for " + vic.FriendlyName);
                }

                if (super_engine.Value)
                {
                    VehicleController this_vic_controller = vic_go.GetComponent<VehicleController>();
                    NwhChassis chassis = vic_go.GetComponent<NwhChassis>();

                    Util.ShallowCopy(this_vic_controller.engine, abrams_vic_controller.engine);
                    Util.ShallowCopy(this_vic_controller.transmission, abrams_vic_controller.transmission);

                    this_vic_controller.engine.vc = vic_go.GetComponent<VehicleController>();
                    this_vic_controller.transmission.vc = vic_go.GetComponent<VehicleController>();
                    this_vic_controller.engine.Initialize(this_vic_controller);
                    this_vic_controller.engine.Start();
                    this_vic_controller.transmission.Initialize(this_vic_controller);

                    chassis._maxForwardSpeed = 20f;
                    chassis._maxReverseSpeed = 11.176f;
                    chassis._originalEnginePower = 1400.99f;
                }

                if (has_drozd.Value)
                {
                    List<DrozdLauncher> launchers = new List<DrozdLauncher>();

                    Vector3[] launcher_positions = new Vector3[] {
                        new Vector3(-1.2953f, -0.1483f, 0.3166f),
                        new Vector3(-1.2243f, 0.0691f, 0.2969f),
                        new Vector3(1.2953f, -0.1483f, 0.3166f),
                        new Vector3(1.2243f, 0.0691f, 0.2969f),
                    };

                    Vector3[] launcher_rots = new Vector3[] {
                        new Vector3(0f, 0f, 0f),
                        new Vector3(0f, -17.8007f, 0f),
                        new Vector3(0f, 0f, 0f),
                        new Vector3(0f, 17.8007f, 0f)
                    };

                    for (var i = 0; i < launcher_positions.Length; i++)
                    {
                        GameObject launcher = GameObject.Instantiate(DrozdLauncher.drozd_launcher_visual, vic.transform.Find("---T64A_MESH---/HULL/TURRET"));
                        launcher.transform.localPosition = launcher_positions[i];
                        launcher.transform.localEulerAngles = launcher_rots[i];

                        if (i > 1)
                        {
                            launcher.transform.localScale = Vector3.Scale(launcher.transform.localScale, new Vector3(-1f, 1f, 1f));
                        }

                        launchers.Add(launcher.GetComponent<DrozdLauncher>());
                    }

                    Drozd.AttachDrozd(
                        vic.transform.Find("---T64A_MESH---/HULL/TURRET"), vic, new Vector3(0f, 0f, 9.5f),
                        launchers.GetRange(0, 2).ToArray(), launchers.GetRange(2, 2).ToArray()
                    );

                    vic._friendlyName += "D";
                }
            }

            yield break;
        }

        public static void Init()
        {
            if (!t64_patch.Value) return;

            if (abrams_vic_controller == null)
            {
                foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
                {
                    if (obj.name == "M1IP")
                    {
                        abrams_vic_controller = obj.GetComponent<VehicleController>();
                        break;
                    }
                }
            }

            APFSDS_125mm.Init();

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
