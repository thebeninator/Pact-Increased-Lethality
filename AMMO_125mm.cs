using System.Collections.Generic;
using System.Linq;
using GHPC.Weapons;
using UnityEngine;
using GHPC.Weaponry;
using System;

namespace PactIncreasedLethality
{
    public class Ammo_125mm
    {
        private static bool assets_loaded = false;

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

        public static AmmoClipCodexScriptable clip_codex_3bm46;
        public static AmmoType.AmmoClip clip_3bm46;
        public static AmmoCodexScriptable ammo_codex_3bm46;
        public static AmmoType ammo_3bm46;
        public static GameObject ammo_3bm46_vis = null;

        public static AmmoClipCodexScriptable clip_codex_3bm60;
        public static AmmoType.AmmoClip clip_3bm60;
        public static AmmoCodexScriptable ammo_codex_3bm60;
        public static AmmoType ammo_3bm60;
        public static GameObject ammo_3bm60_vis = null;

        public static AmmoClipCodexScriptable clip_codex_9m119m1;
        public static AmmoType.AmmoClip clip_9m119m1;
        public static AmmoCodexScriptable ammo_codex_9m119m1;
        public static AmmoType ammo_9m119m1;
        public static GameObject ammo_9m119m1_vis = null;

        public static AmmoClipCodexScriptable clip_codex_9m119;
        public static AmmoType.AmmoClip clip_9m119;
        public static AmmoCodexScriptable ammo_codex_9m119;
        public static AmmoType ammo_9m119;
        public static GameObject ammo_9m119_vis = null;

        public static Dictionary<string, AmmoClipCodexScriptable> ap;

        public static Dictionary<string, AmmoClipCodexScriptable> atgm;

        public static void LoadAssets() {
            if (assets_loaded) return;

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

            ArmorCodexScriptable[] armours = Resources.FindObjectsOfTypeAll<ArmorCodexScriptable>();

            foreach (string name in composite_names)
            {
                ArmorCodexScriptable armour = armours.Where(o => o.name == name).First();
                AmmoType.ArmorOptimization optimization_3bm26 = new AmmoType.ArmorOptimization();
                optimization_3bm26.Armor = armour;
                optimization_3bm26.RhaRatio = 0.80f;
                composite_optimizations_3bm26.Add(optimization_3bm26);

                AmmoType.ArmorOptimization optimization_3bm42 = new AmmoType.ArmorOptimization();
                optimization_3bm42.Armor = armour;
                optimization_3bm42.RhaRatio = 0.75f;
                composite_optimizations_3bm42.Add(optimization_3bm42);
            }

            ammo_3bm26 = new AmmoType();
            Util.ShallowCopy(ammo_3bm26, Assets.ammo_3bm32);
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

            ammo_3bm26_vis = GameObject.Instantiate(Assets.ammo_3bm32.VisualModel);
            ammo_3bm26_vis.name = "3bm26 visual";
            ammo_3bm26.VisualModel = ammo_3bm26_vis;
            ammo_3bm26.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm26;
            ammo_3bm26.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm26;

            ammo_3bm42 = new AmmoType();
            Util.ShallowCopy(ammo_3bm42, Assets.ammo_3bm32);
            ammo_3bm42.Name = "3BM42 APFSDS-T";
            ammo_3bm42.Coeff = 0.152f;
            ammo_3bm42.Caliber = 125;
            ammo_3bm42.RhaPenetration = 540f;
            ammo_3bm42.Mass = 4.85f;
            ammo_3bm42.MuzzleVelocity = 1700f;
            ammo_3bm42.MaxSpallRha = 24f;
            ammo_3bm42.MinSpallRha = 6f;
            ammo_3bm26.SpallMultiplier = 0.9f;
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

            ammo_3bm42_vis = GameObject.Instantiate(Assets.ammo_3bm32.VisualModel);
            ammo_3bm42_vis.name = "3bm42 visual";
            ammo_3bm42.VisualModel = ammo_3bm42_vis;
            ammo_3bm42.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm42;
            ammo_3bm42.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm42;

            ammo_3bm46 = new AmmoType();
            Util.ShallowCopy(ammo_3bm46, Assets.ammo_3bm32);
            ammo_3bm46.Name = "3BM46 APFSDS-T";
            ammo_3bm46.Caliber = 125;
            ammo_3bm46.RhaPenetration = 615f;
            ammo_3bm46.Mass = 4.85f;
            ammo_3bm46.MuzzleVelocity = 1700f;
            ammo_3bm46.SpallMultiplier = 1.25f;
            ammo_3bm46.MaxSpallRha = 24f;
            ammo_3bm46.MinSpallRha = 6f;
          
            ammo_codex_3bm46 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_3bm46.AmmoType = ammo_3bm46;
            ammo_codex_3bm46.name = "ammo_3bm46";

            clip_3bm46 = new AmmoType.AmmoClip();
            clip_3bm46.Capacity = 1;
            clip_3bm46.Name = "3BM46 APFSDS-T";
            clip_3bm46.MinimalPattern = new AmmoCodexScriptable[1];
            clip_3bm46.MinimalPattern[0] = ammo_codex_3bm46;

            clip_codex_3bm46 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_3bm46.name = "clip_3bm46";
            clip_codex_3bm46.ClipType = clip_3bm46;

            ammo_3bm46_vis = GameObject.Instantiate(Assets.ammo_3bm32.VisualModel);
            ammo_3bm46_vis.name = "3bm46 visual";
            ammo_3bm46.VisualModel = ammo_3bm46_vis;
            ammo_3bm46.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm46;
            ammo_3bm46.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm46;

            ammo_3bm60 = new AmmoType();
            Util.ShallowCopy(ammo_3bm60, Assets.ammo_3bm32);
            ammo_3bm60.Name = "3BM60 APFSDS-T";
            ammo_3bm60.Caliber = 125;
            ammo_3bm60.RhaPenetration = 728f;
            ammo_3bm60.Mass = 4.85f;
            ammo_3bm60.MuzzleVelocity = 1700f;
            ammo_3bm60.SpallMultiplier = 1.25f;
            ammo_3bm60.MaxSpallRha = 24f;
            ammo_3bm60.MinSpallRha = 6f;

            ammo_codex_3bm60 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_3bm60.AmmoType = ammo_3bm60;
            ammo_codex_3bm60.name = "ammo_3bm60";

            clip_3bm60 = new AmmoType.AmmoClip();
            clip_3bm60.Capacity = 1;
            clip_3bm60.Name = "3BM60 APFSDS-T";
            clip_3bm60.MinimalPattern = new AmmoCodexScriptable[1];
            clip_3bm60.MinimalPattern[0] = ammo_codex_3bm60;

            clip_codex_3bm60 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_3bm60.name = "clip_3bm60";
            clip_codex_3bm60.ClipType = clip_3bm60;

            ammo_3bm60_vis = GameObject.Instantiate(Assets.ammo_3bm32.VisualModel);
            ammo_3bm60_vis.name = "3bm60 visual";
            ammo_3bm60.VisualModel = ammo_3bm60_vis;
            ammo_3bm60.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm60;
            ammo_3bm60.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm60;

            ammo_9m119m1 = new AmmoType();
            Util.ShallowCopy(ammo_9m119m1, Assets.ammo_kobra);
            ammo_9m119m1.Name = "9M119M1 Invar-M";
            ammo_9m119m1.Caliber = 125;
            ammo_9m119m1.RhaPenetration = 960f;
            ammo_9m119m1.MuzzleVelocity = 350f;
            ammo_9m119m1.RangedFuseTime = 17.7f;
            ammo_9m119m1.TntEquivalentKg = 5.72f;
            ammo_9m119m1.MaxSpallRha = 12f;
            ammo_9m119m1.MinSpallRha = 2f;
            ammo_9m119m1.TurnSpeed = 2f;
            ammo_9m119m1.Guidance = AmmoType.GuidanceType.Laser;

            ammo_codex_9m119m1 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_9m119m1.AmmoType = ammo_9m119m1;
            ammo_codex_9m119m1.name = "ammo_9m119m1_refleks";

            clip_9m119m1 = new AmmoType.AmmoClip();
            clip_9m119m1.Capacity = 1;
            clip_9m119m1.Name = "9M119M1 Invar-M";
            clip_9m119m1.MinimalPattern = new AmmoCodexScriptable[1];
            clip_9m119m1.MinimalPattern[0] = ammo_codex_9m119m1;

            clip_codex_9m119m1 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_9m119m1.name = "clip_9m119m1_refleks";
            clip_codex_9m119m1.ClipType = clip_9m119m1;

            ammo_9m119m1_vis = GameObject.Instantiate(Assets.ammo_kobra.VisualModel);
            ammo_9m119m1_vis.name = "9m119m1 visual";
            ammo_9m119m1.VisualModel = ammo_9m119m1_vis;
            ammo_9m119m1.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_9m119m1;
            ammo_9m119m1.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_9m119m1;

            ammo_9m119 = new AmmoType();
            Util.ShallowCopy(ammo_9m119, Assets.ammo_kobra);
            ammo_9m119.Name = "9M119 Refleks";
            ammo_9m119.Caliber = 125;
            ammo_9m119.RhaPenetration = 750f;
            ammo_9m119.MuzzleVelocity = 340f;
            ammo_9m119.RangedFuseTime = 17.7f;
            ammo_9m119.TntEquivalentKg = 5.72f;
            ammo_9m119.MaxSpallRha = 12f;
            ammo_9m119.MinSpallRha = 2f;
            ammo_9m119.TurnSpeed = 2f;
            ammo_9m119.Guidance = AmmoType.GuidanceType.Laser;

            ammo_codex_9m119 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_9m119.AmmoType = ammo_9m119;
            ammo_codex_9m119.name = "ammo_9m119_refleks";

            clip_9m119 = new AmmoType.AmmoClip();
            clip_9m119.Capacity = 1;
            clip_9m119.Name = "9M119 Refleks";
            clip_9m119.MinimalPattern = new AmmoCodexScriptable[1];
            clip_9m119.MinimalPattern[0] = ammo_codex_9m119;

            clip_codex_9m119 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_9m119.name = "clip_9m119_refleks";
            clip_codex_9m119.ClipType = clip_9m119;

            ammo_9m119_vis = GameObject.Instantiate(Assets.ammo_kobra.VisualModel);
            ammo_9m119_vis.name = "9m119 visual";
            ammo_9m119.VisualModel = ammo_9m119_vis;
            ammo_9m119.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_9m119;
            ammo_9m119.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_9m119;

            ap = new Dictionary<string, AmmoClipCodexScriptable>()
            {
                ["3BM22"] = Assets.clip_codex_3bm22,
                ["3BM26"] = clip_codex_3bm26,
                ["3BM32"] = Assets.clip_codex_3bm32,
                ["3BM42"] = clip_codex_3bm42,
                ["3BM46"] = clip_codex_3bm46,
                ["3BM60"] = clip_codex_3bm60,
            };

            atgm = new Dictionary<string, AmmoClipCodexScriptable>()
            {
                ["9M119"] = clip_codex_9m119,
                ["9M119M1"] = clip_codex_9m119m1,
            };

            assets_loaded = true;
        }
    }
}
