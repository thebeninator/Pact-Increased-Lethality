using System.Linq;
using UnityEngine;
using GHPC.Equipment;
using GHPC;
using GHPC.Audio;
using GHPC.Effects;
using HarmonyLib;

namespace PactIncreasedLethality
{
    public class Kontakt5
    {
        public static ArmorCodexScriptable kontakt5_so = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
        public static ArmorType kontakt5_armour = new ArmorType();
        public static AmmoType dummy_he;


        public class Kontakt5Visual : MonoBehaviour
        {
            public MeshRenderer visual;
            public Material destroyed_mat;
            public bool hide_on_detonate = true;
        }

        public static void Setup(Transform k5_armour_parent, Transform visual_parent, bool hide_on_detonate = true, Material destroyed_mat = null)
        {
            if (kontakt5_so.ArmorType == null)
            {
                kontakt5_armour.RhaeMultiplierKe = 1f;
                kontakt5_armour.RhaeMultiplierCe = 1f;
                kontakt5_armour.CanRicochet = true;
                kontakt5_armour.CrushThicknessModifier = 1f;
                kontakt5_armour.NormalizesHits = true;
                kontakt5_armour.CanShatterLongRods = true;
                kontakt5_armour.ThicknessSource = ArmorType.RhaSource.Multipliers;

                kontakt5_so.name = "kontakt-5 armour";
                kontakt5_so.ArmorType = kontakt5_armour;
            }

            foreach (Transform k5 in k5_armour_parent)
            {
                UniformArmor k5_armour = k5.gameObject.AddComponent<UniformArmor>();
                k5_armour._name = "Kontakt-5";
                k5_armour.PrimaryHeatRha = 550f;
                k5_armour.PrimarySabotRha = 200f;
                k5_armour.SecondaryHeatRha = 0f;
                k5_armour.SecondarySabotRha = 0f;
                k5_armour._canShatterLongRods = true;
                k5_armour._normalizesHits = true;
                k5_armour.AngleMatters = true;
                k5_armour._isEra = true;
                k5_armour._armorType = kontakt5_so;

                k5.gameObject.layer = 8;
                k5.gameObject.tag = "Penetrable";

                Kontakt5Visual vis = k5.gameObject.AddComponent<Kontakt5Visual>();

                if (!hide_on_detonate) {
                    vis.hide_on_detonate = false;
                    vis.destroyed_mat = destroyed_mat;
                }

                vis.visual = visual_parent.transform.GetChild(k5.GetSiblingIndex()).GetComponent<MeshRenderer>();
                vis.enabled = false;
            }
        }

        [HarmonyPatch(typeof(GHPC.UniformArmor), "Detonate")]
        public static class K5Detonate
        {
            private static void Postfix(GHPC.UniformArmor __instance)
            {
                if (__instance.transform.GetComponent<Kontakt5Visual>() == null) return;
                Kontakt5Visual vis = __instance.transform.GetComponent<Kontakt5Visual>();

                vis.visual.enabled = !vis.hide_on_detonate;

                if (!vis.hide_on_detonate && vis.destroyed_mat != null) {
                    Material[] vis_mats = vis.visual.materials;
                    vis_mats[0] = vis.destroyed_mat;
                    vis.visual.materials = vis_mats;
                }

                ParticleEffectsManager.Instance.CreateImpactEffectOfType(
                    dummy_he, ParticleEffectsManager.FusedStatus.Fuzed, ParticleEffectsManager.SurfaceMaterial.Steel, false, __instance.transform.position);
                ImpactSFXManager.Instance.PlaySimpleImpactAudio(ImpactAudioType.MainGunHeat, __instance.transform.position);

                __instance.gameObject.SetActive(!vis.hide_on_detonate);
            }
        }
        public static void Init() {
            if (dummy_he == null)
            {
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
    }
}