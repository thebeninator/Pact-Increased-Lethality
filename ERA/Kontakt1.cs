using System.Collections.Generic;
using GHPC;
using UnityEngine;
using GHPC.Equipment;
using HarmonyLib;
using System.Reflection.Emit;
using GHPC.Audio;
using GHPC.Effects;

namespace PactIncreasedLethality
{
    public static class Kontakt1
    {
        public static ArmorCodexScriptable kontakt1_so = null;
        public static ArmorType kontakt1_armour = new ArmorType();
        public class Kontakt1Visual : MonoBehaviour
        {
            public MeshRenderer visual;
        }

        public static void Setup(Transform k1_armour_parent, Transform visual_parent)
        {
            if (kontakt1_so == null)
            {
                kontakt1_armour.RhaeMultiplierKe = 1f;
                kontakt1_armour.RhaeMultiplierCe = 1f;
                kontakt1_armour.CanRicochet = true;
                kontakt1_armour.CrushThicknessModifier = 1f;
                kontakt1_armour.NormalizesHits = true;
                kontakt1_armour.CanShatterLongRods = true;
                kontakt1_armour.ThicknessSource = ArmorType.RhaSource.Multipliers;

                kontakt1_so = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                kontakt1_so.name = "kontakt-1 armour";
                kontakt1_so.ArmorType = kontakt1_armour;
            }

            foreach (Transform k1 in k1_armour_parent)
            {
                UniformArmor k1_armour = k1.gameObject.AddComponent<UniformArmor>();
                k1_armour._name = "Kontakt-1";
                k1_armour.PrimaryHeatRha = 350f;
                k1_armour.PrimarySabotRha = 20f;
                k1_armour.SecondaryHeatRha = 0f;
                k1_armour.SecondarySabotRha = 0f;
                k1_armour._canShatterLongRods = true;
                k1_armour._normalizesHits = true;
                k1_armour.AngleMatters = true;
                k1_armour._isEra = true;
                k1_armour._armorType = kontakt1_so;

                k1.gameObject.layer = 8;
                k1.gameObject.tag = "Penetrable";

                Kontakt1Visual vis = k1.gameObject.AddComponent<Kontakt1Visual>();
                vis.visual = visual_parent.transform.GetChild(k1.GetSiblingIndex()).GetComponent<MeshRenderer>();
                vis.enabled = false;
            }
        }

        [HarmonyPatch(typeof(GHPC.UniformArmor), "Detonate")]
        public static class K1Detonate
        {
            private static void Postfix(GHPC.UniformArmor __instance)
            {
                if (__instance.transform.GetComponent<Kontakt1Visual>() == null) return;
                Kontakt1Visual vis = __instance.transform.GetComponent<Kontakt1Visual>();

                vis.visual.enabled = false;

                ParticleEffectsManager.Instance.CreateImpactEffectOfType(
                    Kontakt5.dummy_he, ParticleEffectsManager.FusedStatus.Fuzed, ParticleEffectsManager.SurfaceMaterial.Steel, false, __instance.transform.position);
                ImpactSFXManager.Instance.PlaySimpleImpactAudio(ImpactAudioType.MainGunHeat, __instance.transform.position);

                __instance.gameObject.SetActive(false);
            }
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
            var is_era       = AccessTools.PropertyGetter(typeof(GHPC.IArmor), nameof(GHPC.IArmor.IsEra));
            var pen_rating   = AccessTools.PropertyGetter(typeof(GHPC.Weapons.LiveRound), nameof(GHPC.Weapons.LiveRound.CurrentPenRating));
            var debug        = AccessTools.Field(typeof(GHPC.Weapons.LiveRound), nameof(GHPC.Weapons.LiveRound.Debug));
            var shot_info    = AccessTools.Field(typeof(GHPC.Weapons.LiveRound), nameof(GHPC.Weapons.LiveRound.Info));
            var caliber      = AccessTools.Field(typeof(AmmoType), nameof(AmmoType.Caliber));

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

            var custom_instr = new List<CodeInstruction>();
            custom_instr.Add(new CodeInstruction(OpCodes.Ldarg_0));
            custom_instr.Add(new CodeInstruction(OpCodes.Ldfld, shot_info));
            custom_instr.Add(new CodeInstruction(OpCodes.Ldfld, caliber));
            custom_instr.Add(new CodeInstruction(OpCodes.Ldc_R4, caliber_threshold));
            custom_instr.Add(new CodeInstruction(OpCodes.Bge_S, exec));

            custom_instr.Add(new CodeInstruction(OpCodes.Ldarg_0));
            custom_instr.Add(new CodeInstruction(OpCodes.Call, pen_rating));
            custom_instr.Add(new CodeInstruction(OpCodes.Ldc_R4, pen_threshold));
            custom_instr.Add(new CodeInstruction(OpCodes.Ble_Un_S, endof));
            instr.InsertRange(idx, custom_instr);

            return instr;
        }
    }
}
