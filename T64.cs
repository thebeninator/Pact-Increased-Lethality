using System;
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

        public static void Config(MelonPreferences_Category cfg)
        {
            t64_patch = cfg.CreateEntry<bool>("T-64 Patch", true);
            t64_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            super_engine = cfg.CreateEntry<bool>("Super Engine/Transmission", true);
            super_engine.Comment = "vrrrrrrrrrrooooooooom";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (GameObject vic_go in PactIncreasedLethalityMod.vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (!vic.FriendlyName.Contains("T-64")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

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
                    chassis._originalEnginePower = 1500.99f;
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

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
