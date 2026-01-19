using GHPC.Audio;
using GHPC.Effects;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using GHPC.Equipment;
using GHPC;

namespace PactIncreasedLethality
{
    public class EraSchema {
        public ArmorCodexScriptable era_so;
        public ArmorType era_armour;
        public string name;
        public float heat_rha;
        public float ke_rha;
    }

    public class PactEra
    {
        public static AmmoType dummy_he;

        public static void Setup(EraSchema schema, Transform era_armour_parent, Transform visual_parent, 
            bool hide_on_detonate = true, Material destroyed_mat = null, string destroyed_target = "")
        {
            if (schema.era_so.ArmorType == null)
            {
                schema.era_armour.RhaeMultiplierKe = 1f;
                schema.era_armour.RhaeMultiplierCe = 1f;
                schema.era_armour.CanRicochet = true;
                schema.era_armour.CrushThicknessModifier = 1f;
                schema.era_armour.NormalizesHits = true;
                schema.era_armour.CanShatterLongRods = true;
                schema.era_armour.ThicknessSource = ArmorType.RhaSource.Multipliers;

                schema.era_so.name = schema.name + " armour";
                schema.era_so.ArmorType = schema.era_armour;
            }

            foreach (Transform era in era_armour_parent)
            {
                UniformArmor era_armour = era.gameObject.AddComponent<UniformArmor>();
                era_armour._name = schema.name;
                era_armour.PrimaryHeatRha = schema.heat_rha;
                era_armour.PrimarySabotRha = schema.ke_rha;
                era_armour.SecondaryHeatRha = 0f;
                era_armour.SecondarySabotRha = 0f;
                era_armour._canShatterLongRods = true;
                era_armour._normalizesHits = true;
                era_armour.AngleMatters = true;
                era_armour._isEra = true;
                era_armour._armorType = schema.era_so;

                era.gameObject.layer = 8;
                era.gameObject.tag = "Penetrable";

                EraVisual vis = era.gameObject.AddComponent<EraVisual>();

                if (!hide_on_detonate)
                {
                    vis.hide_on_detonate = false;
                    vis.destroyed_mat = destroyed_mat;
                    vis.destroyed_target = destroyed_target;
                }

                vis.visual = visual_parent.transform.GetChild(era.GetSiblingIndex()).GetComponent<MeshRenderer>();
                vis.enabled = false;
            }
        }

        [HarmonyPatch(typeof(GHPC.UniformArmor), "Detonate")]
        public static class PactEraDetonate
        {
            private static void Postfix(GHPC.UniformArmor __instance)
            {
                if (__instance.transform.GetComponent<EraVisual>() == null) return;
                EraVisual vis = __instance.transform.GetComponent<EraVisual>();

                vis.visual.enabled = !vis.hide_on_detonate;

                if (!vis.hide_on_detonate && vis.destroyed_mat != null)
                {
                    int idx = 0;
                    Material[] vis_mats = vis.visual.materials;

                    if (vis.destroyed_target != "") 
                    {
                        for (int i = 0; i < vis_mats.Length; i++) 
                        {
                            if (vis_mats[i].name.Contains(vis.destroyed_target))
                            {
                                idx = i;
                                break;
                            }
                        }
                    } 

                    vis_mats[idx] = vis.destroyed_mat;
                    vis.visual.materials = vis_mats;
                }

                ParticleEffectsManager.Instance.CreateImpactEffectOfType(
                    dummy_he, ParticleEffectsManager.FusedStatus.Fuzed, ParticleEffectsManager.SurfaceMaterial.Steel, false, __instance.transform.position);
                ImpactSFXManager.Instance.PlaySimpleImpactAudio(ImpactAudioType.MainGunHeat, __instance.transform.position);

                __instance.gameObject.SetActive(!vis.hide_on_detonate);
            }
        }

        public static void Init()
        {
            if (dummy_he != null) return;
            
            dummy_he = new AmmoType();
            dummy_he.DetonateEffect = Resources.FindObjectsOfTypeAll<GameObject>().Where(o => o.name == "HEAT Impact").First();
            dummy_he.ImpactEffectDescriptor = new ParticleEffectsManager.ImpactEffectDescriptor()
            {
                HasImpactEffect = true,
                ImpactCategory = ParticleEffectsManager.Category.HighExplosive,
                EffectSize = ParticleEffectsManager.EffectSize.Autocannon,
                RicochetType = ParticleEffectsManager.RicochetType.None,
                Flags = ParticleEffectsManager.ImpactModifierFlags.Medium,
                MinFilterStrictness = ParticleEffectsManager.FilterStrictness.Low
            };
        }
    }

    [HarmonyPatch(typeof(GHPC.Weapons.LiveRound), "penCheck")]
    public class InsensitiveERA
    {
        private static float pen_threshold = 40f;
        private static float caliber_threshold = 25f;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var detonate_era = AccessTools.Method(typeof(GHPC.IArmor), "Detonate");
            var is_era = AccessTools.PropertyGetter(typeof(GHPC.IArmor), nameof(GHPC.IArmor.IsEra));
            var pen_rating = AccessTools.PropertyGetter(typeof(GHPC.Weapons.LiveRound), nameof(GHPC.Weapons.LiveRound.CurrentPenRating));
            var debug = AccessTools.Field(typeof(GHPC.Weapons.LiveRound), nameof(GHPC.Weapons.LiveRound.Debug));
            var shot_info = AccessTools.Field(typeof(GHPC.Weapons.LiveRound), nameof(GHPC.Weapons.LiveRound.Info));
            var caliber = AccessTools.Field(typeof(AmmoType), nameof(AmmoType.Caliber));

            var instr = new List<CodeInstruction>(instructions);
            int idx = -1;
            int debug_count = 0;
            Label endof = il.DefineLabel();
            Label exec = il.DefineLabel();

            // find location of if-statement for ERA det code 
            for (int i = 0; i < instr.Count; i++)
            {
                if (instr[i].opcode == OpCodes.Callvirt && instr[i].operand == (object)is_era)
                {
                    // ??????????? need to find out how to peek into the stack at runtime 
                    idx = i + 5; break;
                }
            }

            // find start of the next if-statement
            for (int i = idx; i < instr.Count; i++)
            {
                if (instr[i].opcode == OpCodes.Ldsfld && instr[i].operand == (object)debug)
                {
                    debug_count++;

                    // IL_0C26
                    if (debug_count == 1) instr[i].labels.Add(exec);


                    // IL_0C6C
                    if (debug_count == 2) { instr[i].labels.Add(endof); break; }
                }
            }

            var custom_instr = new List<CodeInstruction>() {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, shot_info),
                new CodeInstruction(OpCodes.Ldfld, caliber),
                new CodeInstruction(OpCodes.Ldc_R4, caliber_threshold),
                new CodeInstruction(OpCodes.Bge_S, exec),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, pen_rating),
                new CodeInstruction(OpCodes.Ldc_R4, pen_threshold),
                new CodeInstruction(OpCodes.Ble_Un_S, endof)
            };

            instr.InsertRange(idx, custom_instr);

            return instr;
        }
    }
}
