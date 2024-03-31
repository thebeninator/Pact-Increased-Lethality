using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader.Utils;
using UnityEngine;
using MelonLoader;
using GHPC.Equipment;
using GHPC;
using Thermals;
using GHPC.Audio;
using static MelonLoader.MelonLogger;
using GHPC.Effects;

namespace PactIncreasedLethality
{
    public class Kontakt5
    {
        public class OnDestroy : MonoBehaviour
        {
            private bool done = false; 

            void Update()
            {
                if (!GetComponent<UniformArmor>().IsDetonated || done) return;
                GetComponent<MeshRenderer>().material.color = new Color(0.6f, 0.6f, 0.6f);
                ImpactSFXManager.Instance.PlaySimpleImpactAudio(ImpactAudioType.MainGunHeat, transform.position);
                ParticleEffectsManager.Instance.CreateEffectOfType(ParticleEffectsManager.EffectVisualType.AutocannonImpactExplosive, transform.position, null);

                int rand = UnityEngine.Random.Range(0, 2);
                if (rand == 1)
                {
                    GetComponent<UniformArmor>()._isDetonated = false;
                }
                else {
                    done = true;
                }
            }
        }

        public static GameObject t80_kontakt_5_hull_array;
        public static GameObject t80_kontakt_5_turret_array;
        public static GameObject t80_kontakt_5_roof_array;

        public static GameObject t72_kontakt_5_hull_array;
        public static GameObject t72_kontakt_5_turret_array;
        public static GameObject t72b3_kontakt_5_turret_array;
        public static GameObject t72_kontakt_5_roof_array;

        public static GameObject kontakt_5_side_hull_array;
        public static GameObject kontakt_5_side_hull_array_ext; 

        public static ArmorCodexScriptable kontakt5_so = null;
        public static ArmorType kontakt5_armour = new ArmorType();

        private static void ERA_Setup(Transform[] era_transforms)
        {
            foreach (Transform transform in era_transforms)
            {
                if (transform.name != "k5_upper" && transform.name != "k5_lower" && !transform.name.Contains("k5hull")) continue;

                transform.gameObject.AddComponent<UniformArmor>();
                transform.gameObject.tag = "Penetrable";
                transform.gameObject.layer = 8;
                UniformArmor armor = transform.gameObject.GetComponent<UniformArmor>();
                armor.SetName("Kontakt-5");
                armor.PrimaryHeatRha = 50f;
                armor.PrimarySabotRha = 50f; 
                armor.SecondaryHeatRha = 5f;
                armor.SecondarySabotRha = 5f;
                armor.ThicknessListed = UniformArmor.ThicknessMode.ActualThickness;
                armor._canShatterLongRods = true;
                armor._crushThicknessModifier = 1f;
                armor._normalizesHits = true;
                armor._isEra = true;
                armor.AngleMatters = true;

                if (kontakt5_so == null)
                {
                    kontakt5_so = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                    kontakt5_so.name = "kontakt-5 armour";
                    kontakt5_armour.RhaeMultiplierKe = 4f;
                    kontakt5_armour.RhaeMultiplierCe = 5.5f;
                    kontakt5_armour.CanRicochet = true;
                    kontakt5_armour.CrushThicknessModifier = 1f;
                    kontakt5_armour.NormalizesHits = true;
                    kontakt5_armour.CanShatterLongRods = true;
                    kontakt5_armour.ThicknessSource = ArmorType.RhaSource.Multipliers;

                    kontakt5_so.ArmorType = kontakt5_armour;
                }

                armor._armorType = kontakt5_so;

                MeshRenderer mesh_renderer = transform.gameObject.GetComponent<MeshRenderer>();
                mesh_renderer.material.shader = Shader.Find("Standard (FLIR)");

                transform.gameObject.AddComponent<HeatSource>();
                transform.gameObject.AddComponent<OnDestroy>();
            }
        }

        public static void Init() {
            if (t80_kontakt_5_turret_array == null)
            {
                var kontakt5_bundle_turret = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/kontakt5", "t80_kontakt5_turret"));
                var kontakt5_bundle_hull = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/kontakt5", "t80_kontakt5_hull"));

                t80_kontakt_5_turret_array = kontakt5_bundle_turret.LoadAsset<GameObject>("t80_k5_turret_array.prefab");
                t80_kontakt_5_turret_array.hideFlags = HideFlags.DontUnloadUnusedAsset;
                t80_kontakt_5_turret_array.transform.localScale = new Vector3(-1f, -1f, 1f);

                t80_kontakt_5_roof_array = kontakt5_bundle_turret.LoadAsset<GameObject>("t80_k5_roof.prefab");
                t80_kontakt_5_roof_array.hideFlags = HideFlags.DontUnloadUnusedAsset;
                t80_kontakt_5_roof_array.transform.localScale = new Vector3(-1f, -1f, 1f);

                t80_kontakt_5_hull_array = kontakt5_bundle_hull.LoadAsset<GameObject>("t80_k5_hull_array.prefab");
                t80_kontakt_5_hull_array.hideFlags = HideFlags.DontUnloadUnusedAsset;

                var t72_kontakt5_bundle_turret = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/kontakt5", "t72_kontakt5_turret"));

                t72_kontakt_5_turret_array = t72_kontakt5_bundle_turret.LoadAsset<GameObject>("t72_k5_turret_array.prefab");
                t72_kontakt_5_turret_array.hideFlags = HideFlags.DontUnloadUnusedAsset;
                t72_kontakt_5_turret_array.transform.localScale = new Vector3(-1f, 1f, -1f);

                t72_kontakt_5_roof_array = t72_kontakt5_bundle_turret.LoadAsset<GameObject>("t72_k5_roof.prefab");
                t72_kontakt_5_roof_array.hideFlags = HideFlags.DontUnloadUnusedAsset;
                t72_kontakt_5_roof_array.transform.localScale = new Vector3(-1f, 1f, -1f);

                var t72b3_kontakt5_bundle_turret = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/kontakt5", "t72b3_kontakt5_turret"));
                t72b3_kontakt_5_turret_array = t72b3_kontakt5_bundle_turret.LoadAsset<GameObject>("t72b3_k5_turret_array.prefab");
                t72b3_kontakt_5_turret_array.hideFlags = HideFlags.DontUnloadUnusedAsset;
                t72b3_kontakt_5_turret_array.transform.localScale = new Vector3(-1f, 1f, -1f);

                var kontakt5_side_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/kontakt5", "k5_side_hull_array"));
                kontakt_5_side_hull_array = kontakt5_side_bundle.LoadAsset<GameObject>("k5_side_hull_array.prefab");
                kontakt_5_side_hull_array.hideFlags = HideFlags.DontUnloadUnusedAsset;

                var kontakt5_side_bundle_ext = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/kontakt5", "k5_side_hull_extended"));
                kontakt_5_side_hull_array_ext = kontakt5_side_bundle_ext.LoadAsset<GameObject>("k5_side_hull_array_extended.prefab");
                kontakt_5_side_hull_array_ext.hideFlags = HideFlags.DontUnloadUnusedAsset;

                ERA_Setup(t80_kontakt_5_turret_array.transform.Find("left_k5_array").GetComponentsInChildren<Transform>());
                ERA_Setup(t80_kontakt_5_turret_array.transform.Find("right_k5_array (1)").GetComponentsInChildren<Transform>());
                ERA_Setup(t80_kontakt_5_hull_array.GetComponentsInChildren<Transform>());
                ERA_Setup(t80_kontakt_5_roof_array.GetComponentsInChildren<Transform>());

                ERA_Setup(t72_kontakt_5_turret_array.transform.Find("left_k5_array").GetComponentsInChildren<Transform>());
                ERA_Setup(t72_kontakt_5_turret_array.transform.Find("right_k5_array").GetComponentsInChildren<Transform>());
                ERA_Setup(t72b3_kontakt_5_turret_array.transform.Find("left_k5_array").GetComponentsInChildren<Transform>());
                ERA_Setup(t72b3_kontakt_5_turret_array.transform.Find("right_k5_array").GetComponentsInChildren<Transform>());
                ERA_Setup(t72_kontakt_5_roof_array.GetComponentsInChildren<Transform>());

                ERA_Setup(kontakt_5_side_hull_array.GetComponentsInChildren<Transform>());
                ERA_Setup(kontakt_5_side_hull_array_ext.GetComponentsInChildren<Transform>());
            }
        }
    }
}