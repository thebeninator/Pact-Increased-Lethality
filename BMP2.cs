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
using HarmonyLib;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class BMP2
    {
        static AmmoClipCodexScriptable clip_codex_3ubr8;
        static AmmoType.AmmoClip clip_3ubr8;
        static AmmoCodexScriptable ammo_codex_3ubr8;
        static AmmoType ammo_3ubr8;

        static AmmoClipCodexScriptable clip_codex_3uof8;
        static AmmoType.AmmoClip clip_3uof8;
        static AmmoCodexScriptable ammo_codex_3uof8;
        static AmmoType ammo_3uof8;

        static AmmoClipCodexScriptable clip_codex_9m113_as;
        static AmmoType.AmmoClip clip_9m113_as;
        static AmmoCodexScriptable ammo_codex_9m113_as;
        static AmmoType ammo_9m113_as;

        static AmmoType ammo_9m113; 

        static AmmoType ammo_3ubr6;
        static AmmoClipCodexScriptable clip_codex_3ubr6;

        static AmmoType ammo_3uor6;
        static AmmoCodexScriptable ammo_codex_3uor6;
        static AmmoClipCodexScriptable clip_codex_3uor6;

        static MelonPreferences_Entry<bool> bmp2_patch;
        static MelonPreferences_Entry<bool> use_3ubr8;
        static MelonPreferences_Entry<bool> use_3uof8;
        static MelonPreferences_Entry<bool> use_9m113as;

        public static void Config(MelonPreferences_Category cfg)
        {
            bmp2_patch = cfg.CreateEntry<bool>("BMP-2 Patch", true);
            bmp2_patch.Description = "///////////////";
            use_3ubr8 = cfg.CreateEntry<bool>("Use 3UBR8", true);
            use_3ubr8.Comment = "Replaces 3UBR6; has improved penetration and better ballistics";

            use_3uof8 = cfg.CreateEntry<bool>("Use 3UOF8", true);
            use_3uof8.Comment = "Mixed belt of 3UOR6 and 3UOF8 (1:2); 3UOF8 has more explosive filler but no tracer";

            use_9m113as = cfg.CreateEntry<bool>("Use 9M113AS", true);
            use_9m113as.Comment = "Fictional overfly-top-attack ATGM with dual warhead; aim above target";
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

                AmmoClipCodexScriptable ap = use_3ubr8.Value ? clip_codex_3ubr8 : clip_codex_3ubr6;
                AmmoClipCodexScriptable he = use_3uof8.Value ? clip_codex_3uof8 : clip_codex_3uor6;

                loadout_manager.LoadedAmmoTypes = new AmmoClipCodexScriptable[] { ap, he };

                GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[0].Rack;
                loadout_manager.RackLoadouts[0].OverrideInitialClips = new AmmoClipCodexScriptable[] { ap, he };
                rack.ClipTypes = new AmmoType.AmmoClip[] { ap.ClipType, he.ClipType };
                Util.EmptyRack(rack);

                loadout_manager.SpawnCurrentLoadout();
                weapon.Feed.AmmoTypeInBreech = null;
                weapon.Feed.LoadedClipType = null;
                weapon.Feed.Start();
                loadout_manager.RegisterAllBallistics();

                if (use_9m113as.Value) {
                    WeaponSystem atgm = vic.GetComponent<WeaponsManager>().Weapons[1].Weapon;
                    GHPC.Weapons.AmmoRack atgm_rack = atgm.Feed.ReadyRack;

                    atgm_rack.ClipTypes[0] = clip_9m113_as;
                    atgm_rack.StoredClips = new List<AmmoType.AmmoClip>()
                    {
                        clip_9m113_as,
                        clip_9m113_as,
                        clip_9m113_as,
                        clip_9m113_as,
                        clip_9m113_as,
                    };

                    atgm.Feed.AmmoTypeInBreech = null;
                    atgm.Feed.Start();
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
                    if (s.AmmoType.Name == "3UBR6 APBC-T") { 
                        ammo_3ubr6 = s.AmmoType;
                    }

                    if (s.AmmoType.Name == "3UOR6 HE-T") { 
                        ammo_3uor6 = s.AmmoType;
                        ammo_codex_3uor6 = s; 
                    }

                    if (s.AmmoType.Name == "9M113 Konkurs")
                    {
                        ammo_9m113 = s.AmmoType;
                    }

                    if (ammo_3ubr6 != null && ammo_3uor6 != null && ammo_9m113 != null) break; 
                }

                foreach (AmmoClipCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoClipCodexScriptable)))
                {
                    if (s.name == "clip_3UOR6_340rd_load") { clip_codex_3uor6 = s; }
                    if (s.name == "clip_3UBR6_160rd_load") { clip_codex_3ubr6 = s; }
                    if (clip_codex_3ubr6 != null && clip_codex_3uor6 != null) break;
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

                /////////////////////

                ammo_3uof8 = new AmmoType();
                Util.ShallowCopy(ammo_3uof8, ammo_3uor6);
                ammo_3uof8.Name = "3UOF8 HEFI";
                ammo_3uof8.UseTracer = false;
                ammo_3uof8.TntEquivalentKg = 0.049f;

                ammo_codex_3uof8 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3uof8.AmmoType = ammo_3uof8;
                ammo_codex_3uof8.name = "ammo_3uof8";

                clip_3uof8 = new AmmoType.AmmoClip();
                clip_3uof8.Capacity = 340;
                clip_3uof8.Name = "3UOR6 HE-T/3UOF8 HEFI";
                clip_3uof8.MinimalPattern = new AmmoCodexScriptable[] {
                    ammo_codex_3uof8,
                    ammo_codex_3uof8,
                    ammo_codex_3uor6,
                };
                clip_3uof8.MinimalPattern[0] = ammo_codex_3uof8;

                clip_codex_3uof8 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3uof8.name = "clip_3uof8";
                clip_codex_3uof8.ClipType = clip_3uof8;

                /////////////////////

                ammo_9m113_as = new AmmoType();
                Util.ShallowCopy(ammo_9m113_as, ammo_9m113);
                ammo_9m113_as.Name = "9M113AS Konkurs";
                ammo_9m113_as.Category = AmmoType.AmmoCategory.Explosive;
                ammo_9m113_as.RhaPenetration = 10f;
                ammo_9m113_as.NoisePowerX = 1f;
                ammo_9m113_as.NoisePowerY = 1f;

                ammo_codex_9m113_as = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_9m113_as.AmmoType = ammo_9m113_as;
                ammo_codex_9m113_as.name = "ammo_9m113_as";

                clip_9m113_as = new AmmoType.AmmoClip();
                clip_9m113_as.Capacity = 1;
                clip_9m113_as.Name = "9M113AS Konkurs";
                clip_9m113_as.MinimalPattern = new AmmoCodexScriptable[] { ammo_codex_9m113_as };
                clip_9m113_as.MinimalPattern[0] = ammo_codex_9m113_as;

                clip_codex_9m113_as = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_9m113_as.name = "clip_9m113_as";
                clip_codex_9m113_as.ClipType = clip_9m113_as;

                AmmoType ammo_9m113_efp = new AmmoType();
                ammo_9m113_efp.Name = "9M113AS Konkurs EFP";
                ammo_9m113_efp.Category = AmmoType.AmmoCategory.Explosive;
                ammo_9m113_efp.RhaPenetration = 500f;
                ammo_9m113_efp.Mass = 3f;
                ammo_9m113_efp.TntEquivalentKg = 0.9f;
                ammo_9m113_efp.ImpactFuseTime = 0.0001f;
                ammo_9m113_efp.SectionalArea = ammo_9m113_as.SectionalArea / 1.5f;

                EFP.AddEFP(ammo_9m113_as, ammo_9m113_efp, true);
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }    
}
