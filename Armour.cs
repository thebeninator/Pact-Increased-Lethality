﻿using GHPC.Equipment;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class Armour
    {
        public static ArmorCodexScriptable ru_welded_armor;
        public static ArmorCodexScriptable ru_cast_armor;
        public static ArmorCodexScriptable bdd_cast_armor;
        public static ArmorCodexScriptable composite_armor;
        public static ArmorCodexScriptable hull_metal_polymer;
        public static ArmorCodexScriptable cheek_metal_polymer;

        public static void Init()
        {
            if (ru_welded_armor == null)
            {
                ru_welded_armor = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                ru_welded_armor.name = "ru welded armor";
                ArmorType ru_welded = new ArmorType();
                ru_welded.Name = "welded steel";
                ru_welded.CanRicochet = true;
                ru_welded.CanShatterLongRods = true;
                ru_welded.NormalizesHits = true;
                ru_welded.ThicknessSource = ArmorType.RhaSource.Multipliers;
                ru_welded.SpallAngleMultiplier = 1f;
                ru_welded.SpallPowerMultiplier = 1f;
                ru_welded.RhaeMultiplierCe = 1f;
                ru_welded.RhaeMultiplierKe = 1f;
                ru_welded.CrushThicknessModifier = 1f;
                ru_welded_armor.ArmorType = ru_welded;

                ru_cast_armor = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                ru_cast_armor.name = "ru cast armor";
                ArmorType ru_cast = new ArmorType();
                ru_cast.Name = "cast steel";
                ru_cast.CanRicochet = true;
                ru_cast.CanShatterLongRods = true;
                ru_cast.NormalizesHits = true;
                ru_cast.ThicknessSource = ArmorType.RhaSource.Multipliers;
                ru_cast.SpallAngleMultiplier = 1f;
                ru_cast.SpallPowerMultiplier = 1f;
                ru_cast.RhaeMultiplierCe = 0.95f;
                ru_cast.RhaeMultiplierKe = 0.95f;
                ru_cast.CrushThicknessModifier = 1f;
                ru_cast_armor.ArmorType = ru_cast;

                composite_armor = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                composite_armor.name = "ru composite";
                ArmorType composite = new ArmorType();
                composite.Name = "composite";
                composite.CanRicochet = true;
                composite.CanShatterLongRods = true;
                composite.NormalizesHits = true;
                composite.ThicknessSource = ArmorType.RhaSource.Multipliers;
                composite.SpallAngleMultiplier = 1f;
                composite.SpallPowerMultiplier = 0.2f;
                composite.RhaeMultiplierCe = 1.67f;
                composite.RhaeMultiplierKe = 0.9f;
                composite.CrushThicknessModifier = 1f;
                composite_armor.ArmorType = composite;

                bdd_cast_armor = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                bdd_cast_armor.name = "bdd cast armor";
                ArmorType bdd_cast = new ArmorType();
                bdd_cast.Name = "bdd cast steel";
                bdd_cast.CanRicochet = true;
                bdd_cast.CanShatterLongRods = true;
                bdd_cast.NormalizesHits = true;
                bdd_cast.ThicknessSource = ArmorType.RhaSource.Multipliers;
                bdd_cast.SpallAngleMultiplier = 1f;
                bdd_cast.SpallPowerMultiplier = 1f;
                bdd_cast.RhaeMultiplierCe = 0.3f; // super low multipliers b/c i kinda fucked up on the scale of the outer shell 
                bdd_cast.RhaeMultiplierKe = 0.3f;
                bdd_cast.CrushThicknessModifier = 1f;
                bdd_cast_armor.ArmorType = bdd_cast;

                cheek_metal_polymer = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                cheek_metal_polymer.name = "ru metal polymer";
                ArmorType mpoly_cheek = new ArmorType();
                mpoly_cheek.Name = "mpoly";
                mpoly_cheek.CanRicochet = true;
                mpoly_cheek.CanShatterLongRods = true;
                mpoly_cheek.NormalizesHits = true;
                mpoly_cheek.ThicknessSource = ArmorType.RhaSource.Multipliers;
                mpoly_cheek.SpallAngleMultiplier = 1f;
                mpoly_cheek.SpallPowerMultiplier = 0.2f;
                mpoly_cheek.RhaeMultiplierCe = 1.45f;
                mpoly_cheek.RhaeMultiplierKe = 1.05f;
                mpoly_cheek.CrushThicknessModifier = 1f;
                cheek_metal_polymer.ArmorType = mpoly_cheek;

                hull_metal_polymer = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                hull_metal_polymer.name = "ru metal polymer";
                ArmorType mpoly_hull = new ArmorType();
                mpoly_hull.Name = "mpoly";
                mpoly_hull.CanRicochet = true;
                mpoly_hull.CanShatterLongRods = true;
                mpoly_hull.NormalizesHits = true;
                mpoly_hull.ThicknessSource = ArmorType.RhaSource.Multipliers;
                mpoly_hull.SpallAngleMultiplier = 1f;
                mpoly_hull.SpallPowerMultiplier = 0.2f;
                mpoly_hull.RhaeMultiplierCe = 2.2f;
                mpoly_hull.RhaeMultiplierKe = 1.05f;
                mpoly_hull.CrushThicknessModifier = 1f;
                hull_metal_polymer.ArmorType = mpoly_hull;
            }
        }
    }
}