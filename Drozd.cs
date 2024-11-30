using System;
using System.IO;
using System.Linq;
using MelonLoader.Utils;
using UnityEngine;
using HarmonyLib;
using MelonLoader;
using GHPC.Vehicle;
using GHPC.Utility;
using GHPC.Effects;
using GHPC.Audio;
using GHPC;
using System.Xml.Linq;

namespace PactIncreasedLethality
{
    struct LauncherArray
    {
        public LauncherArray(DrozdLauncher[] _launchers, int _idx)
        {
            launchers = _launchers;
            current_launcher_index = _idx;
        }
        public DrozdLauncher[] launchers { get; set; }
        public int current_launcher_index { get; set; }

        public DrozdLauncher current_launcher { get => launchers[current_launcher_index]; }
        public bool IsEmpty { get => launchers.Last().IsEmpty; }
    }

    public class Drozd : MonoBehaviour
    {
        static float MIN_ENGAGEMENT_SPEED = 70f;
        static float MAX_ENGAGEMENT_SPEED = 700f;
        static float MIN_ENGAGEMENT_ANGLE = 3f;
        static float MAX_ENGAGEMENT_ANGLE = 22f;
        static float INTERCEPTION_COOLDOWN = 2f;
        static string[] FAILURES = new string[4] { "ignore", "ignore", "miss", "miss" };

        static GameObject drozd_go;

        static MelonPreferences_Entry<bool> high_velocity;
        static MelonPreferences_Entry<int> intercept_chance_low_med;
        static MelonPreferences_Entry<int> intercept_chance_high;
        static MelonPreferences_Entry<int> min_diameter;

        public Vehicle unit;
        private float cd = 0f;
        private LauncherArray l_launchers;
        private LauncherArray r_launchers;

        private static AmmoType dummy_he; 

        public static void Config(MelonPreferences_Category cfg)
        {
            high_velocity = cfg.CreateEntry<bool>("Intercept High Velocity Projectiles (Drozd)", false);
            high_velocity.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            high_velocity.Comment = "Allow Drozd to intercept projectiles going faster than 700 m/s";

            intercept_chance_low_med = cfg.CreateEntry<int>("Intercept Chance (Drozd)", 80);
            intercept_chance_low_med.Comment = "Probability of Drozd detecting or successfully intercepting a projectile going 70-700 m/s";

            intercept_chance_high = cfg.CreateEntry<int>("Intercept Chance, High Velocity (Drozd)", 30);
            intercept_chance_high.Comment = "Probability of Drozd detecting or successfully intercepting a projectile going >700 m/s";

            min_diameter = cfg.CreateEntry<int>("Minimum Projectile Caliber (Drozd)", 73);
            min_diameter.Comment = "Minimum projectile caliber that Drozd can detect";
        }

        public static void Init()
        {
            if (drozd_go != null) return;

            dummy_he = new AmmoType();
            dummy_he.DetonateEffect = Resources.FindObjectsOfTypeAll<GameObject>().Where(o => o.name == "HE_explosion").First();
            dummy_he.ImpactEffectDescriptor = new ParticleEffectsManager.ImpactEffectDescriptor()
            {
                HasImpactEffect = true,
                ImpactCategory = ParticleEffectsManager.Category.HighExplosive,
                EffectSize = ParticleEffectsManager.EffectSize.MainGun,
                RicochetType = ParticleEffectsManager.RicochetType.None,
                Flags = ParticleEffectsManager.ImpactModifierFlags.Large,
                MinFilterStrictness = ParticleEffectsManager.FilterStrictness.Low
            };

            var bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "testcollider"));
            drozd_go = bundle.LoadAsset<GameObject>("test collider.prefab");
            drozd_go.name = "drozd";
            drozd_go.layer = 7;
            drozd_go.tag = "Untagged";
            drozd_go.GetComponent<MeshRenderer>().enabled = false;   
            drozd_go.AddComponent<Drozd>();
            drozd_go.AddComponent<LateFollow>();

            DrozdLauncher.Init();
        }

        public static void AttachDrozd(Transform collider_attachment_point, Vehicle unit, Vector3 collider_offset, DrozdLauncher[] left, DrozdLauncher[] right)
        {
            GameObject drozd = GameObject.Instantiate(drozd_go);
            drozd.transform.localScale = new Vector3(20f, 5f, 1f);

            drozd.GetComponent<Drozd>().unit = unit;
            drozd.GetComponent<Drozd>().l_launchers = new LauncherArray(left, 0);
            drozd.GetComponent<Drozd>().r_launchers = new LauncherArray(right, 0);

            LateFollow late_follow = drozd.GetComponent<LateFollow>();
            late_follow.FollowTarget = collider_attachment_point;
            late_follow.enabled = true;
            late_follow.Awake();
            late_follow._localRotShift = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            late_follow._localPosShift = collider_offset;
        }

        void Update()
        {
            if (cd > 0f) cd -= Time.deltaTime;

            if (cd < 0f) cd = 0f;
        }
        
        [HarmonyPatch(typeof(GHPC.Weapons.LiveRound), "penCheck")]
        public static class Interception
        {
            private static bool Prefix(GHPC.Weapons.LiveRound __instance, object[] __args)
            {
                /*
                if (!__instance.IsSpall && ((Collider)__args[0]).GetComponent<IArmor>() != null) {
                    __instance.doRicochet((Collider)__args[0], ((Collider)__args[0]).GetComponent<IArmor>(), (Vector3)__args[2], (Vector3)__args[1], 90f, false);
                    return true;
                }
                */

                if (__instance.IsSpall) return true;
                if (__instance.ShotInfo.TypeInfo.Caliber < min_diameter.Value && __instance.ShotInfo.TypeInfo.Caliber != 0f) return true;
                if (__instance.CurrentSpeed < MIN_ENGAGEMENT_SPEED || (__instance.CurrentSpeed > MAX_ENGAGEMENT_SPEED && !high_velocity.Value)) return true;

                Collider collider = (Collider)__args[0];
                Drozd drozd = collider.GetComponent<Drozd>();

                if (!collider.gameObject.name.Contains("drozd")) return true;
                if (drozd == null || drozd.cd > 0 || drozd.unit.Neutralized) return true;

                if (drozd.l_launchers.IsEmpty && drozd.r_launchers.IsEmpty) return true;

                Vector3 impact_point = (Vector3)__args[3];
                Vector3 impact_path = (Vector3)__args[2];
                Vector3 normal = (Vector3)__args[1];

                bool hit_front_face = Vector3.Angle(normal, collider.gameObject.transform.forward) == 0;
                bool hit_side_face = Vector3.Angle(normal, collider.gameObject.transform.forward) == 90;
                float angle_of_impact = Vector3.SignedAngle(impact_path, drozd.unit.transform.position - impact_point, Vector3.up);

                if ((hit_front_face || hit_side_face) && Math.Abs(angle_of_impact) >= MIN_ENGAGEMENT_ANGLE && Math.Abs(angle_of_impact) <= MAX_ENGAGEMENT_ANGLE)
                {
                    string failure = "";
                    LauncherArray launcher_array = Math.Sign(angle_of_impact) == -1 ? drozd.l_launchers : drozd.r_launchers;

                    int rand = UnityEngine.Random.Range(1, 100);
                    if ((__instance.CurrentSpeed > MAX_ENGAGEMENT_SPEED && rand > intercept_chance_high.Value) ||
                        (__instance.CurrentSpeed <= MAX_ENGAGEMENT_SPEED && rand > intercept_chance_low_med.Value))
                    {
                        failure = FAILURES[UnityEngine.Random.Range(0, 4)];
                    }

                    if (failure == "ignore")
                    {
                        __instance.Story.builder.AppendLine("Not detected by APS radar");
                        return true;
                    }

                    if (launcher_array.IsEmpty) return true;
                    if (launcher_array.current_launcher.IsEmpty) launcher_array.current_launcher_index++;
                    
                    launcher_array.current_launcher.FireCurrentRocket();
                    ParticleEffectsManager.Instance.CreateImpactEffectOfType(dummy_he, ParticleEffectsManager.FusedStatus.Fuzed, ParticleEffectsManager.SurfaceMaterial.None, false, __instance._lastFramePosition);
                    ImpactSFXManager.Instance.PlaySimpleImpactAudio(ImpactAudioType.Missile, __instance._lastFramePosition);
                    drozd.cd = INTERCEPTION_COOLDOWN;

                    if (failure == "miss")
                    {
                        __instance.Story.builder.AppendLine("Degraded by APS");

                        if (__instance._isPureAp)
                        {
                            // hoping the GC takes care of the copy lol 
                            AmmoType copy = new AmmoType();
                            Util.ShallowCopy(copy, __instance.Info);
                            copy.RhaPenetration /= 1.8f;
                            __instance.Info = copy;
                        }
                        else {
                            AmmoType copy = new AmmoType();
                            Util.ShallowCopy(copy, __instance.Info);

                            copy.RhaPenetration /= 2f;
                            copy.TntEquivalentKg /= 1.3f;
                            copy.ShatterOnRicochet = true;
                            copy.CertainRicochetAngle = 20f;
                            __instance.Info = copy;
                        }

                        return true;
                     }

                    __instance.Story.builder.AppendLine("Intercepted by APS");
                    __instance._parentUnit = drozd.unit;
                    __instance._frameData.VehicleStruck = drozd.unit;
                    __instance.ShotInfo.Distance = Vector3.Distance(impact_point, __instance.Shooter.transform.position);
                    __instance.reportShotTraceFrame();
                    //__instance._fuzeCompleted = true;
                    __instance.Detonate();
                    __instance.createExplosion(false, 0f, Vector3.zero, 0.03f, 55);

                    return false;
                }

                return true;
            }
        }
    }
    public class DrozdLauncher : MonoBehaviour
    {
        public static GameObject drozd_launcher_visual;
        static Material bradley_launcher_mat;
        static Mesh bradley_launcher_mesh;
        static GameObject fx;
        static GameObject drozd_rocket;

        public int current_rocket_idx = 0;
        public Transform[] rockets = new Transform[2];

        public bool IsEmpty { get => current_rocket_idx == 2; }

        public void FireCurrentRocket()
        {
            Transform curr = rockets[current_rocket_idx];
            curr.gameObject.GetComponent<Renderer>().enabled = false;
            curr.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>().Play();
            curr.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().Play();

            FmodGenericAudioManager.Instance.PlayOneShot(
                "event:/Weapons/launcher_9M14",
                transform.position,
                new ValueTuple<string, float>[] { new ValueTuple<string, float>("IsInterior", 0f) }
            );

            current_rocket_idx++;
        }

        void Awake()
        {
            rockets[0] = transform.GetChild(0).GetChild(0);
            rockets[1] = transform.GetChild(0).GetChild(1);
        }

        public static void Init()
        {
            if (drozd_launcher_visual != null) return;

            foreach (Vehicle s in Resources.FindObjectsOfTypeAll<Vehicle>())
            {
                if (s.name != "M2 Bradley") continue;

                GameObject rig = s.transform.Find("M2BRADLEY_rig/lp_hull005").gameObject;
                SkinnedMeshRenderer rig_renderer = rig.GetComponent<SkinnedMeshRenderer>();

                bradley_launcher_mat = Material.Instantiate(rig_renderer.sharedMaterial);
                bradley_launcher_mesh = Mesh.Instantiate(rig_renderer.sharedMesh);

                fx = GameObject.Instantiate(s.transform.Find("Gun Scripts/Launcher M2 TOW/muzzle effects L").gameObject);
                GameObject.Destroy(fx.transform.GetChild(0).GetChild(6).gameObject);
                GameObject.Destroy(fx.transform.GetChild(1).GetChild(7).gameObject);

                drozd_rocket = s.transform.Find("placeholder missile R/lp_rocket").gameObject;

                break;
            }

            drozd_launcher_visual = new GameObject("drozd lawnchair");
            Transform launcher_ref = GameObject.Instantiate(new GameObject("launcher ref"), drozd_launcher_visual.transform).transform;
            SkinnedMeshRenderer rend = drozd_launcher_visual.AddComponent<SkinnedMeshRenderer>();
            rend.sharedMaterial = bradley_launcher_mat;
            rend.sharedMesh = bradley_launcher_mesh;
            rend.rootBone = launcher_ref;

            Transform[] bones = new Transform[53];
            for (int i = 0; i < 53; i++)
                bones[i] = launcher_ref;
            rend.bones = bones;

            Matrix4x4[] binds = new Matrix4x4[53];
            for (int i = 0; i < 53; i++)
                binds[i] = new Matrix4x4();
            binds[40] = new Matrix4x4(
                new Vector4(1f, 0f, 0f, 0f),
                new Vector4(0f, 0f, -1f, 0f),
                new Vector4(0f, 1f, 0f, 0f),
                new Vector4(0.7522f, -1.8705f, -2.6309f, 1f)
            );
            rend.sharedMesh.bindposes = binds;

            drozd_launcher_visual.transform.localScale = new Vector3(0.7039f, 0.7039f, 0.7f);

            GameObject rocket1 = GameObject.Instantiate(drozd_rocket, drozd_launcher_visual.transform.GetChild(0));
            rocket1.transform.localScale = new Vector3(23.5289f, 23.3988f, 19f);
            rocket1.transform.localEulerAngles = Vector3.zero;
            rocket1.transform.localPosition = new Vector3(-0.15f, 0.2582f, -0.05f);
            rocket1.GetComponent<SkinnedMeshRenderer>().material.color = new Color(1f, 1f, 0.5481f, 1f);

            GameObject rocket1_fx = GameObject.Instantiate(fx, rocket1.transform);
            rocket1_fx.transform.localEulerAngles = Vector3.zero;
            rocket1_fx.transform.localPosition = Vector3.zero;
            rocket1_fx.transform.GetChild(1).localPosition = Vector3.zero;

            GameObject rocket2 = GameObject.Instantiate(drozd_rocket, drozd_launcher_visual.transform.GetChild(0));
            rocket2.transform.localScale = new Vector3(23.5289f, 23.3988f, 19f);
            rocket2.transform.localEulerAngles = Vector3.zero;
            rocket2.transform.localPosition = new Vector3(-0.43f, 0.2582f, -0.05f);
            rocket2.GetComponent<SkinnedMeshRenderer>().material.color = new Color(1f, 1f, 0.5481f, 1f);

            GameObject rocket2_fx = GameObject.Instantiate(fx, rocket2.transform);
            rocket2_fx.transform.localEulerAngles = Vector3.zero;
            rocket2_fx.transform.localPosition = Vector3.zero;
            rocket2_fx.transform.GetChild(1).localPosition = Vector3.zero;

            drozd_launcher_visual.AddComponent<DrozdLauncher>();
        }
    }
}