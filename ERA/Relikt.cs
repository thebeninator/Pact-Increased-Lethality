using System.Linq;
using UnityEngine;
using GHPC.Equipment;
using GHPC;
using GHPC.Audio;
using GHPC.Effects;
using HarmonyLib;

namespace PactIncreasedLethality
{
    public class Relikt
    {
        public static ArmorCodexScriptable rlkt_so = null;
        public static ArmorType rlkt_armour = new ArmorType();


        public class ReliktVisual : MonoBehaviour
        {
            public MeshRenderer visual;
            public Material destroyed_mat;
            public bool hide_on_detonate = true;
        }

        public static void Setup(Transform rlkt_armour_parent, Transform visual_parent, bool hide_on_detonate = true, Material destroyed_mat = null)
        {
            if (rlkt_so == null)
            {
                rlkt_armour.RhaeMultiplierKe = 1f;
                rlkt_armour.RhaeMultiplierCe = 1f;
                rlkt_armour.CanRicochet = true;
                rlkt_armour.CrushThicknessModifier = 1f;
                rlkt_armour.NormalizesHits = true;
                rlkt_armour.CanShatterLongRods = true;
                rlkt_armour.ThicknessSource = ArmorType.RhaSource.Multipliers;

                rlkt_so = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                rlkt_so.name = "relikt armour";
                rlkt_so.ArmorType = rlkt_armour;
            }

            foreach (Transform rlkt in rlkt_armour_parent)
            {
                UniformArmor rlkt_armour = rlkt.gameObject.AddComponent<UniformArmor>();
                rlkt_armour._name = "Relikt";
                rlkt_armour.PrimaryHeatRha = 400f;
                rlkt_armour.PrimarySabotRha = 250f;
                rlkt_armour.SecondaryHeatRha = 0f;
                rlkt_armour.SecondarySabotRha = 0f;
                rlkt_armour._canShatterLongRods = true;
                rlkt_armour._normalizesHits = false;
                rlkt_armour.AngleMatters = false;
                rlkt_armour._isEra = true;
                rlkt_armour._armorType = rlkt_so;

                rlkt.gameObject.layer = 8;
                rlkt.gameObject.tag = "Penetrable";

                ReliktVisual vis = rlkt.gameObject.AddComponent<ReliktVisual>();

                if (!hide_on_detonate) {
                    vis.hide_on_detonate = false;
                    vis.destroyed_mat = destroyed_mat;
                }

                vis.visual = visual_parent.transform.GetChild(rlkt.GetSiblingIndex()).GetComponent<MeshRenderer>();
            }
        }

        [HarmonyPatch(typeof(GHPC.UniformArmor), "Detonate")]
        public static class ReliktDetonate
        {
            private static void Postfix(GHPC.UniformArmor __instance)
            {
                if (__instance.transform.GetComponent<ReliktVisual>() == null) return;
                ReliktVisual vis = __instance.transform.GetComponent<ReliktVisual>();

                vis.visual.enabled = !vis.hide_on_detonate;

                if (!vis.hide_on_detonate && vis.destroyed_mat != null) {
                    Material[] vis_mats = vis.visual.materials;
                    vis_mats[0] = vis.destroyed_mat;
                    vis.visual.materials = vis_mats;
                }

                ParticleEffectsManager.Instance.CreateImpactEffectOfType(
                    Kontakt5.dummy_he, ParticleEffectsManager.FusedStatus.Fuzed, ParticleEffectsManager.SurfaceMaterial.Steel, false, __instance.transform.position);
                ImpactSFXManager.Instance.PlaySimpleImpactAudio(ImpactAudioType.MainGunHeat, __instance.transform.position);

                __instance.gameObject.SetActive(!vis.hide_on_detonate);
            }
        }
    }
}