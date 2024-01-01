using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using GHPC;
using MelonLoader;
using MelonLoader.Utils;
using Thermals;
using UnityEngine;
using GHPC.UI.Tips;

namespace PactIncreasedLethality
{

    public static class Kontakt1
    {
        public static GameObject kontakt_1_hull_array;
        public static GameObject kontakt_1_turret_array;
        private static Texture concrete_tex;
        private static Texture concrete_tex_normal;
        private static UnityEngine.Color colour_primary = new UnityEngine.Color(0.6165f, 0.6996f, 0.5015f);
        private static UnityEngine.Color colour_2 = new UnityEngine.Color(0.4565f, 0.5426f, 0.3762f);
        private static UnityEngine.Color colour_3 = new UnityEngine.Color(0.4565f, 0.5226f, 0.3762f);
        private static UnityEngine.Color colour_4 =  new UnityEngine.Color(0.4565f, 0.5555f, 0.3762f);
        private static UnityEngine.Color[] colours = new UnityEngine.Color[] {
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_2,
            colour_3,
            colour_4,
        };

        static MelonPreferences_Entry<bool> sabot_eater;
        public static void Config(MelonPreferences_Category cfg)
        {
            sabot_eater = cfg.CreateEntry<bool>("Super Kontakt-1", false);
            sabot_eater.Description = "///////////////";
            sabot_eater.Comment = "Drastically increases Kontakt-1's ability to stop AP rounds";
        }

        private static void ERA_Setup(Transform[] era_transforms) {
            foreach (Transform transform in era_transforms)
            {
                if (!transform.gameObject.name.Contains("kontakt")) continue;

                transform.gameObject.AddComponent<UniformArmor>();
                UniformArmor armor = transform.gameObject.GetComponent<UniformArmor>();
                armor.SetName("Kontakt-1");
                armor.PrimaryHeatRha = 400f;
                armor.PrimarySabotRha = sabot_eater.Value ? 100f : 40f;
                armor.SecondaryHeatRha = 0f;
                armor.SecondarySabotRha = 0f;
                armor._canShatterLongRods = true;
                armor._crushThicknessModifier = 1f;
                armor._isEra = true;

                foreach (GameObject s in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (s.name == "Autocannon HE Armor Impact") { armor.DetonateEffect = s; break; }
                }

                armor.UndetonatedObjects = new GameObject[] { armor.gameObject };

                MeshRenderer mesh_renderer = transform.gameObject.GetComponent<MeshRenderer>();
                mesh_renderer.material = new Material(Shader.Find("Standard (FLIR)"));
                mesh_renderer.material.mainTexture = concrete_tex;
                mesh_renderer.material.mainTextureScale = new Vector2(0.07f, 0.07f);
                mesh_renderer.material.mainTextureOffset = new Vector2(0f, 0f);
                mesh_renderer.material.EnableKeyword("_NORMALMAP");
                mesh_renderer.material.SetTexture("_BumpMap", concrete_tex_normal);

                mesh_renderer.material.color = colours[UnityEngine.Random.Range(0, colours.Length)];

                transform.gameObject.AddComponent<HeatSource>();
            }
        }

        public static void Init() {
            if (kontakt_1_hull_array == null)
            {
                var kontakt_bundle_hull = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/kontakt1assets", "hull"));
                var kontakt_bundle_turret = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/kontakt1assets", "turret"));


                if (kontakt_bundle_hull == null || kontakt_bundle_turret == null)
                {
                    MelonLogger.Msg(ConsoleColor.DarkCyan, "COULD NOT FIND ASSET FILE(S) FOR KONTAKT-1! (<mods>/kontakt1assets");
                    return;
                }

                foreach (Texture t in Resources.FindObjectsOfTypeAll<Texture>())
                {
                    if (t.name == "GHPC_ConcretePanels_Diffuse") { concrete_tex = t; break; }
                }

                foreach (Texture t in Resources.FindObjectsOfTypeAll<Texture>())
                {
                    if (t.name == "GHPC_ConcretePanels_Normal") { concrete_tex_normal = t; break; }
                }

                kontakt_1_hull_array = kontakt_bundle_hull.LoadAsset<GameObject>("hull era array.prefab");
                kontakt_1_hull_array.transform.localScale = new Vector3(10f, 10f, 10f);

                kontakt_1_turret_array = kontakt_bundle_turret.LoadAsset<GameObject>("turret era array.prefab");
                kontakt_1_turret_array.transform.localScale = new Vector3(10f, 10f, 10f);

                kontakt_1_hull_array.hideFlags = HideFlags.DontUnloadUnusedAsset;
                kontakt_1_turret_array.hideFlags = HideFlags.DontUnloadUnusedAsset;

                ERA_Setup(kontakt_1_hull_array.GetComponentsInChildren<Transform>());
                ERA_Setup(kontakt_1_turret_array.GetComponentsInChildren<Transform>());
            }
        }
    }
}
