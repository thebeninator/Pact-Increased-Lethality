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
using GHPC.Equipment;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib.Tools;
using static HarmonyLib.Tools.Logger;

namespace PactIncreasedLethality
{
    public class RandomColor : MonoBehaviour {
        private static UnityEngine.Color colour_primary = new UnityEngine.Color(0.6165f, 0.6996f, 0.5015f);
        private static UnityEngine.Color colour_2 = new UnityEngine.Color(0.4565f, 0.5426f, 0.3762f);
        private static UnityEngine.Color colour_3 = new UnityEngine.Color(0.4565f, 0.5226f, 0.3762f);
        private static UnityEngine.Color colour_4 = new UnityEngine.Color(0.4565f, 0.5555f, 0.3762f);
        private static UnityEngine.Color[] colours = new UnityEngine.Color[] {
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary, colour_primary,
            colour_2,
            colour_3,
            colour_4,
        };

        void Awake() { 
            this.GetComponent<MeshRenderer>().material.color = colours[UnityEngine.Random.Range(0, colours.Length)];
        }
    } 

    public static class Kontakt1
    {
        public static GameObject kontakt_1_hull_array;
        public static GameObject kontakt_1_turret_array;
        private static Texture concrete_tex;
        private static Texture concrete_tex_normal;
        static MelonPreferences_Entry<bool> sabot_eater;
        static MelonPreferences_Entry<bool> sphere_colliders;

        public static void Config(MelonPreferences_Category cfg)
        {
            sabot_eater = cfg.CreateEntry<bool>("Super Kontakt-1", false);
            sabot_eater.Description = "///////////////";
            sabot_eater.Comment = "Drastically increases Kontakt-1's ability to stop AP rounds";

            sphere_colliders = cfg.CreateEntry<bool>("Spherical Colliders", false);
            sphere_colliders.Comment = "Changes how Kontakt-1 handles collisions. FPS increase at the cost of jankier hit detection";
        }

        public static void LoadTex() {
            if (concrete_tex == null)
            {
                foreach (Texture t in Resources.FindObjectsOfTypeAll<Texture>())
                {
                    if (t.name == "GHPC_ConcretePanels_Diffuse") { concrete_tex = t; break; }
                }

                foreach (Texture t in Resources.FindObjectsOfTypeAll<Texture>())
                {
                    if (t.name == "GHPC_ConcretePanels_Normal") { concrete_tex_normal = t; break; }
                }
            }
        }

        private static void ERA_Setup(Transform[] era_transforms) {
            foreach (Transform transform in era_transforms)
            {
                if (!transform.gameObject.name.Contains("kontakt")) continue;

                Component.Destroy(transform.gameObject.GetComponent<MeshCollider>());

                if (sphere_colliders.Value)
                {
                    transform.gameObject.AddComponent<SphereCollider>();
                }
                else
                {
                    transform.gameObject.AddComponent<BoxCollider>();
                }

                transform.gameObject.AddComponent<UniformArmor>();
                UniformArmor armor = transform.gameObject.GetComponent<UniformArmor>();
                armor.SetName("Kontakt-1");
                armor.PrimaryHeatRha = 350f;
                armor.PrimarySabotRha = sabot_eater.Value ? 100f : 40f;
                armor.SecondaryHeatRha = 0f;
                armor.SecondarySabotRha = 0f;
                armor._canShatterLongRods = true;
                armor._crushThicknessModifier = 1f;
                armor._normalizesHits = true;
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
                mesh_renderer.staticShadowCaster = true;
                mesh_renderer.allowOcclusionWhenDynamic = true;
                mesh_renderer.receiveShadows = false;
                transform.gameObject.AddComponent<RandomColor>();
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
                    MelonLogger.Msg(ConsoleColor.DarkCyan, "COULD NOT FIND ASSET FILE(S) FOR KONTAKT-1! (<mods>/kontakt1assets)");
                    return;
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
