using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Weapons;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class APFSDS_125mm
    {
        static AmmoType ammo_3bm15;
        public static AmmoClipCodexScriptable clip_codex_3bm26;
        public static AmmoType.AmmoClip clip_3bm26;
        public static AmmoCodexScriptable ammo_codex_3bm26;
        public static AmmoType ammo_3bm26;
        public static GameObject ammo_3bm26_vis = null;

        public static AmmoClipCodexScriptable clip_codex_3bm42;
        public static AmmoType.AmmoClip clip_3bm42;
        public static AmmoCodexScriptable ammo_codex_3bm42;
        public static AmmoType ammo_3bm42;
        public static GameObject ammo_3bm42_vis = null;

        public static AmmoClipCodexScriptable clip_codex_3bm22;
        public static AmmoClipCodexScriptable clip_codex_3bm32;

        public static AmmoClipCodexScriptable clip_codex_9m119_super;
        public static AmmoType.AmmoClip clip_9m119_super;
        public static AmmoCodexScriptable ammo_codex_9m119_super;
        public static AmmoType ammo_9m119_super;
        public static GameObject ammo_9m119_super_vis = null;

        public static AmmoType ammo_9m111;


        public static void Init() {
            if (ammo_3bm26 == null)
            {
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
                        optimization_3bm42.RhaRatio = 0.75f;
                        composite_optimizations_3bm42.Add(optimization_3bm42);
                    }

                    if (composite_optimizations_3bm26.Count == composite_names.Length) break;
                }

                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "3BM15 APFSDS-T") { ammo_3bm15 = s.AmmoType; }
                    //if (s.AmmoType.Name == "9M111 Fagot") { ammo_9m111 = s.AmmoType; }

                    if (/*ammo_9m111 != null &&*/ ammo_3bm15 != null) break;
                }

                foreach (AmmoClipCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoClipCodexScriptable)))
                {
                    if (s.name == "clip_3BM22") { clip_codex_3bm22 = s; }
                    if (s.name == "clip_3BM32") { clip_codex_3bm32 = s; }

                    if (clip_codex_3bm22 != null && clip_codex_3bm32 != null) break;
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
                ammo_3bm42.Coeff = ammo_3bm42.Coeff / 2f;
                ammo_3bm42.Caliber = 125;
                ammo_3bm42.RhaPenetration = 540f;
                ammo_3bm42.Mass = 4.85f;
                ammo_3bm42.MuzzleVelocity = 1700f;
                ammo_3bm42.SpallMultiplier = 1f;
                ammo_3bm42.MaxSpallRha = 24f;
                ammo_3bm42.MinSpallRha = 6f;

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
                
                /*
                ammo_9m119_super = new AmmoType();
                Util.ShallowCopy(ammo_9m119_super, ammo_9m111);
                ammo_9m119_super.Name = "blyat";
                ammo_9m119_super.Caliber = 125;
                ammo_9m119_super.TntEquivalentKg = 3f;
                ammo_9m119_super.SpallMultiplier = 1.5f;
                ammo_9m119_super.ClimbAngle = 40f;
                ammo_9m119_super.TurnSpeed = 2.5f;
                ammo_9m119_super.DiveAngle = 55f;
                ammo_9m119_super.LoiterAltitude = 1000f;
                ammo_9m119_super.AimPointMarch = 0.05f;
                ammo_9m119_super.MaxSpallRha = 55f;
                ammo_9m119_super.MinSpallRha = 20f;
                ammo_9m119_super.RangedFuseTime = 20f;
                ammo_9m119_super.UseTracer = false;
                ammo_9m119_super.EdgeSetback = 0.5f;
                ammo_9m119_super.NoisePowerX = 0f;
                ammo_9m119_super.NoisePowerY = 0f;
                ammo_9m119_super.SpiralAngularRate = 0f;
                ammo_9m119_super.Guidance = AmmoType.GuidanceType.Laser;

                ammo_codex_9m119_super = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_9m119_super.AmmoType = ammo_9m119_super;
                ammo_codex_9m119_super.name = "ammo_9m119_super";

                clip_9m119_super = new AmmoType.AmmoClip();
                clip_9m119_super.Capacity = 1;
                clip_9m119_super.Name = "blyat";
                clip_9m119_super.MinimalPattern = new AmmoCodexScriptable[1];
                clip_9m119_super.MinimalPattern[0] = ammo_codex_9m119_super;

                clip_codex_9m119_super = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_9m119_super.name = "clip_9m119_super";
                clip_codex_9m119_super.ClipType = clip_9m119_super;

                ammo_9m119_super_vis = GameObject.Instantiate(ammo_3bm15.VisualModel);
                ammo_9m119_super_vis.name = "9m119_super visual";
                ammo_9m119_super.VisualModel = ammo_9m119_super_vis;
                ammo_9m119_super.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_9m119_super;
                ammo_9m119_super.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_9m119_super;

                FireForget.AddFireForgetAmmo(ammo_9m119_super);   
                */
            }
        }
    }
}
