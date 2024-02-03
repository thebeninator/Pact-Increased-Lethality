using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Equipment.Optics;
using GHPC.State;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using MelonLoader;
using Reticle;
using TMPro;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class BMP2
    {
        static AmmoClipCodexScriptable clip_codex_3ubr8;
        static AmmoType.AmmoClip clip_3ubr8;
        static AmmoCodexScriptable ammo_codex_3ubr8;
        static AmmoType ammo_3ubr8;

        static AmmoType ammo_3ubr6;
        static AmmoClipCodexScriptable clip_codex_3uor6;


        static MelonPreferences_Entry<bool> bmp2_patch;
        static MelonPreferences_Entry<bool> use_3ubr8;


        public static void Config(MelonPreferences_Category cfg)
        {
            bmp2_patch = cfg.CreateEntry<bool>("BMP-2 Patch", true);
            bmp2_patch.Description = "///////////////";
            use_3ubr8 = cfg.CreateEntry<bool>("Use 3UBR8", true);
            use_3ubr8.Comment = "Replaces 3UBR6; has improved penetration and better ballistics";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (GameObject vic_go in PactIncreasedLethalityMod.vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (vic.FriendlyName != "BMP-2") continue;

                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;


                if (use_3ubr8.Value)
                {
                    loadout_manager.LoadedAmmoTypes = new AmmoClipCodexScriptable[] { clip_codex_3ubr8, clip_codex_3uor6 };

                    GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[0].Rack;
                    loadout_manager.RackLoadouts[0].OverrideInitialClips = new AmmoClipCodexScriptable[] { clip_codex_3ubr8, clip_codex_3uor6 };
                    rack.ClipTypes = new AmmoType.AmmoClip[] { clip_3ubr8, clip_codex_3uor6.ClipType };
                    Util.EmptyRack(rack);

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.LoadedClipType = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();
                }
            }

            yield break;
        }

        public static void Init()
        {
            if (!bmp2_patch.Value) return;

            if (ammo_3ubr8 == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "3UBR6 APBC-T") { ammo_3ubr6 = s.AmmoType; break; } 
                }

                foreach (AmmoClipCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoClipCodexScriptable)))
                {
                    if (s.name == "clip_3UOR6_340rd_load") { clip_codex_3uor6 = s; break; }
                }

                ammo_3ubr8 = new AmmoType();
                Util.ShallowCopy(ammo_3ubr8, ammo_3ubr6);
                ammo_3ubr8.Name = "3UBR8 APDS-T";
                ammo_3ubr8.Mass = 0.222f;
                ammo_3ubr8.Coeff = 0.012f;
                ammo_3ubr8.MuzzleVelocity = 1120f;
                ammo_3ubr8.RhaPenetration = 72f;

                ammo_codex_3ubr8 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3ubr8.AmmoType = ammo_3ubr8;
                ammo_codex_3ubr8.name = "ammo_3ubr8";

                clip_3ubr8 = new AmmoType.AmmoClip();
                clip_3ubr8.Capacity = 160;
                clip_3ubr8.Name = "3UBR8 APDS-T";
                clip_3ubr8.MinimalPattern = new AmmoCodexScriptable[1];
                clip_3ubr8.MinimalPattern[0] = ammo_codex_3ubr8;

                clip_codex_3ubr8 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3ubr8.name = "clip_3ubr8";
                clip_codex_3ubr8.ClipType = clip_3ubr8;

            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }
}
