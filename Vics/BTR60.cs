using System;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;
using MelonLoader.Utils;
using System.IO;
using GHPC.State;
using System.Collections;
using GHPC.Vehicle;
using GHPC.Utility;
using GHPC.Weapons;
using Reticle;
using GHPC.Equipment.Optics;
using GHPC;
using GHPC.Thermals;
using GHPC.Camera;
using UnityEngine.Rendering.PostProcessing;
using GHPC.Effects;
using GHPC.Weaponry;

namespace PactIncreasedLethality
{
    public class BTR60
    {
        static GameObject btr60a_turret_complete;

        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static GameObject casing;
        static GameObject m60a1_nvs;

        static MelonPreferences_Entry<bool> btr60_patch;
        static MelonPreferences_Entry<bool> autocannon;
        static MelonPreferences_Entry<bool> use_3ubr8;
        static MelonPreferences_Entry<bool> use_3uof8;
        static MelonPreferences_Entry<bool> stab;

        private static bool assets_loaded = false;

        public static void Config(MelonPreferences_Category cfg)
        {
            btr60_patch = cfg.CreateEntry<bool>("BTR-60 Patch", true);
            btr60_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            stab = cfg.CreateEntry<bool>("Stabilizer (BTR-60)", false);
            autocannon = cfg.CreateEntry<bool>("Use 30mm 2A72 Autocannon (BTR-60)", true);
            autocannon.Comment = "BTR-60A conversion; has fixed 6x magnification day sight and passive night vision sight";
            use_3ubr8 = cfg.CreateEntry<bool>("Use 3UBR8 (BTR-60A)", false);
            use_3ubr8.Comment = "Replaces 3UBR6; has improved penetration and better ballistics";
            use_3uof8 = cfg.CreateEntry<bool>("Use 3UOF8 (BTR-60A)", false);
            use_3uof8.Comment = "Mixed belt of 3UOR6 and 3UOF8 (1:2); 3UOF8 has more explosive filler but no tracer";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (!vic.UniqueName.Contains("BTR60PB")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                WeaponSystemInfo weapon_info = vic.WeaponsManager.Weapons[0];
                WeaponSystem weapon = weapon_info.Weapon;
                AmmoFeed feed = weapon.Feed;

                if (autocannon.Value)
                {
                    vic._friendlyName = "BTR-60A";
                    vic.GetComponent<Rigidbody>().mass += 1500f;

                    SkinnedMeshRenderer mesh_rend = vic.transform.Find("btr60_rig/lp_hull002").GetComponent<SkinnedMeshRenderer>();
                    Matrix4x4[] bindposes = mesh_rend.sharedMesh.bindposes;
                    bindposes[3] = Matrix4x4.zero;
                    bindposes[4] = Matrix4x4.zero;
                    bindposes[5] = Matrix4x4.zero;
                    bindposes[6] = Matrix4x4.zero;
                    mesh_rend.sharedMesh.bindposes = bindposes;

                    if (vic.transform.Find("btr60_rig/HULL/TURRET/Object266"))
                        vic.transform.Find("btr60_rig/HULL/TURRET/Object266").gameObject.SetActive(false);
                    vic.transform.Find("btr60_rig/HULL/TURRET/tyre_on_head").gameObject.SetActive(false);
                    vic.transform.Find("btr60_rig/HULL/commander scope").transform.localScale = Vector3.zero;
                    vic.transform.Find("transforms/commander head").transform.localPosition = new Vector3(0.355f, 2.5692f, 1.933f);

                    Transform btr_gun = vic.transform.Find("btr60_rig/HULL/TURRET/GUN");

                    GameObject full_turret = GameObject.Instantiate(btr60a_turret_complete);
                    GameObject turret = full_turret.transform.Find("BTR_80_B").gameObject;
                    GameObject gun = full_turret.transform.Find("BTR_80_C").gameObject;
                    LateFollow turret_late_follow = vic.transform.Find("btr60_rig/HULL/TURRET").GetComponent<LateFollowTarget>()._lateFollowers[0];

                    turret_late_follow.transform.Find("turret_sides_7mm").gameObject.SetActive(false);
                    btr_gun.Find("MG_KPVT-1_Breach").gameObject.SetActive(false);

                    Reparent turret_reparent = full_turret.AddComponent<Reparent>();
                    turret_reparent.NewParent = turret_late_follow.transform;
                    turret_reparent.Awake();
                    full_turret.transform.localPosition = new Vector3(-0.0709f, 0.97f, 0.2436f);
                    full_turret.transform.localEulerAngles = Vector3.zero;
                    turret.transform.localEulerAngles = new Vector3(270f, 0f, 0f);

                    Reparent gun_reparent = gun.AddComponent<Reparent>();
                    gun_reparent.NewParent = btr_gun.GetComponent<LateFollowTarget>()._lateFollowers[0].transform;
                    gun_reparent.Awake();
                    gun.transform.localPosition = new Vector3(-0.0809f, 2.6379f, 1.0495f);
                    gun.transform.localEulerAngles = new Vector3(270f, 0f, 0f);

                    // pivot point gets all messed up 
                    btr_gun.localPosition = new Vector3(-0.0181f, 0.1099f, -0.6041f);
                    btr_gun.Find("Gun Aimable/gunner sight").transform.localPosition = new Vector3(-0.2186f, 2.3638f, 2.0156f);
                    btr_gun.Find("gun_recoil/14.5mm Machine Gun KPVT").transform.localPosition = new Vector3(0.0151f, 0.2184f, 1.8515f);
                    btr_gun.Find("gun_recoil/14.5mm Machine Gun KPVT/KPVT Muzzle Flash").Find("Gunsmoke Booster").gameObject.SetActive(false);
                    btr_gun.Find("pkt/7.62mm Machine Gun PKT").transform.localPosition = new Vector3(0.3045f, 0.3216f, 0.5704f);
                    btr_gun.Find("Gun Aimable/ejection").transform.localPosition = new Vector3(-0.1397f, 2.6567f, 1.3654f);
                    btr_gun.Find("Gun Aimable/ejection").transform.localEulerAngles = new Vector3(0f, 316.5237f, 0f);

                    UsableOptic day_optic = btr_gun.Find("Gun Aimable/gunner sight/GPS").GetComponent<UsableOptic>();
                    day_optic.slot.DefaultFov = 6f;

                    weapon_info.Name = "30mm gun 2A72";
                    weapon.BaseDeviationAngle = 0.17f;
                    weapon._cycleTimeSeconds = 0.13f;
                    weapon.Feed._totalCycleTime = 0.13f;
                    weapon.WeaponSound.SingleShotByDefault = true;
                    weapon.WeaponSound.SingleShotMode = true;
                    weapon.WeaponSound.SingleShotEventPaths = new string[] { "event:/Weapons/autocannon_2a42_single_actually_2a72" };
                    weapon._impulseLocation = btr_gun.Find("gun_recoil");
                    weapon.Impulse = 35f;
                    weapon.FCS.RegisteredRangeLimits = new Vector2(0f, 4000f);
                    weapon.FCS._originalRangeLimits = new Vector2(0f, 4000f);

                    day_optic.FCS = weapon.FCS;
                    weapon.FCS.RegisterOptic(day_optic);
                    UpdateVerticalRangeScale uvrs = day_optic.gameObject.AddComponent<UpdateVerticalRangeScale>();
                    uvrs.reticle = day_optic.reticleMesh;
                    uvrs.fcs = weapon.FCS;

                    GameObject nvs = GameObject.Instantiate(m60a1_nvs, day_optic.transform);
                    nvs.SetActive(true);

                    SharedNightSight night_sight = day_optic.gameObject.AddComponent<SharedNightSight>();
                    night_sight.nvs = nvs;

                    vic._nightVisionType = NightVisionType.Intensifier;

                    day_optic.slot.VibrationBlurScale = 0.2f;
                    day_optic.slot.VibrationShakeMultiplier = 0.5f;

                    btr_gun.Find("Gun Aimable/gunner sight/GPS/Quad").gameObject.SetActive(false);

                    AmmoClipCodexScriptable ap = use_3ubr8.Value ? Ammo_30mm.clip_codex_3ubr8 : Assets.clip_codex_3ubr6;
                    AmmoClipCodexScriptable he = use_3uof8.Value ? Ammo_30mm.clip_codex_3uof8 : Assets.clip_codex_3uor6;

                    feed.AmmoTypeInBreech = null;
                    feed.ReadyRack.ClipTypes = new AmmoType.AmmoClip[] { ap.ClipType, he.ClipType };
                    feed.ReadyRack._initialClipCounts = new int[] { 1, 1 };
                    feed.DualFeed = true;
                    feed.ReadyRack.Awake();
                    feed.Start();
                    feed.HumanLoaded = false;

                    if (casing == null)
                    {
                        CasingFix.definitely_a_prefab = true;
                        feed.RoundCycleStages[0].EjectedPrefab.GetComponent<DestroyInSeconds>().enabled = false;
                        casing = GameObject.Instantiate(feed.RoundCycleStages[0].EjectedPrefab);
                        casing.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        casing.transform.localScale = new Vector3(2f, 2f, 2f);
                        casing.GetComponent<Rigidbody>().useGravity = false;
                        CasingFix fix = casing.AddComponent<CasingFix>();
                        feed.RoundCycleStages[0].EjectedPrefab.GetComponent<DestroyInSeconds>().enabled = true;
                    }

                    feed.RoundCycleStages[0].EjectedPrefab = casing;

                    day_optic.reticleMesh.smoothTime = 0.1f;
                    day_optic.reticleMesh.reticleSO = reticleSO;
                    day_optic.reticleMesh.reticle = reticle_cached;
                    day_optic.reticleMesh.SMR = null;
                    day_optic.reticleMesh.Load();
                }

                if (stab.Value)
                {
                    weapon.FCS.CurrentStabMode = StabilizationMode.Vector;
                    weapon.FCS.StabsActive = true;

                    for (int i = 0; i <= 1; i++)
                    {
                        vic.AimablePlatforms[i]._stabMode = StabilizationMode.Vector;
                        vic.AimablePlatforms[i].StabilizerActive = true;
                        vic.AimablePlatforms[i].Stabilized = true;
                    }
                }
            }

            yield break;
        }

        public class SharedNightSight : MonoBehaviour
        {
            public GameObject nvs;
            UsableOptic day_optic;
            bool in_day_sight = true;
            PostProcessVolume ppv;

            void Awake()
            {
                day_optic = GetComponent<UsableOptic>();
                ppv = nvs.GetComponent<PostProcessVolume>();
            }

            void Update()
            {
                bool button = InputUtil.MainPlayer.GetButtonDown("Toggle Night Sight");

                if (!button) return;

                in_day_sight = !in_day_sight;

                if (!in_day_sight)
                {
                    ppv.enabled = true;
                    day_optic.slot.BaseBlur = 0.1f;
                }
                else
                {
                    ppv.enabled = false;
                    day_optic.slot.BaseBlur = 0f;
                }
            }
        }

        public class CasingFix : MonoBehaviour
        {
            public static bool definitely_a_prefab = true;

            void Awake()
            {
                if (definitely_a_prefab)
                {
                    definitely_a_prefab = false;
                    return;
                }

                GetComponent<Rigidbody>().useGravity = true;
                GetComponent<DestroyInSeconds>().enabled = true;
            }
        }

        private static void Reticle()
        {
            reticleSO = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["BMP-2_BPK-1-42"].tree);
            reticleSO.name = "btr80a";

            Util.ShallowCopy(reticle_cached, ReticleMesh.cachedReticles["BMP-2_BPK-1-42"]);
                
            reticle_cached.tree = reticleSO;

            reticle_cached.tree.lights = new List<ReticleTree.Light>() {
                new ReticleTree.Light(),
            };

            reticle_cached.tree.lights[0].color = new RGB(5.9922f, 0.502f, 0f, true);
            reticle_cached.tree.lights[0].type = ReticleTree.Light.Type.NightIllumination;

            ReticleTree.Angular angular1 = (reticle_cached.tree.planes[0].elements[0] as ReticleTree.Angular);
            ReticleTree.Angular angular2 = (reticle_cached.tree.planes[0].elements[1] as ReticleTree.Angular);

            ReticleTree.Line range_line_l = angular1.elements[0] as ReticleTree.Line;
            ReticleTree.Line range_line_r = angular1.elements[1] as ReticleTree.Line;

            range_line_l.visualType = ReticleTree.VisualElement.Type.Painted;
            range_line_l.illumination = ReticleTree.Light.Type.NightIllumination;
            range_line_r.visualType = ReticleTree.VisualElement.Type.Painted;
            range_line_r.illumination = ReticleTree.Light.Type.NightIllumination;

            range_line_l.thickness.mrad = 0.1745f;
            range_line_r.thickness.mrad = 0.1745f;

            range_line_l.length.mrad = 20f;
            range_line_r.length.mrad = 20f;

            range_line_l.position.x += 26.1f;
            range_line_r.position.x -= 27.1f;

            for (int i = 0; i <= 2; i++)
            {
                ReticleTree.Angular ammo = angular2.elements[i] as ReticleTree.Angular;
                (ammo.elements[0] as ReticleTree.Text).visualType = ReticleTree.VisualElement.Type.Painted;
                (ammo.elements[0] as ReticleTree.Text).illumination = ReticleTree.Light.Type.NightIllumination;

                ammo.position.x -= Math.Sign(ammo.position.x) * 20f;

                ReticleTree.VerticalBallistic ballistic = ammo.elements[1] as ReticleTree.VerticalBallistic;

                for (int j = 0; j < ballistic.elements.Count; j++)
                {
                    ReticleTree.Angular marking = ballistic.elements[j] as ReticleTree.Angular;

                    for (int k = 0; k < marking.elements.Count; k++)
                    {
                        (marking.elements[k] as ReticleTree.VisualElement).visualType = ReticleTree.VisualElement.Type.Painted;
                        (marking.elements[k] as ReticleTree.VisualElement).illumination = ReticleTree.Light.Type.NightIllumination;
                    }
                }
            }

            ReticleTree.Angular horizontal = angular2.elements[3] as ReticleTree.Angular;
            for (int i = 0; i < horizontal.elements.Count; i++)
            {
                ReticleTree.Angular marking = horizontal.elements[i] as ReticleTree.Angular;

                for (int j = 0; j < marking.elements.Count; j++)
                {
                    ReticleTree.Line line = marking.elements[j] as ReticleTree.Line;
                    line.length.mrad = 3.1416f;
                    line.thickness.mrad /= 1.2f;
                    line.visualType = ReticleTree.VisualElement.Type.Painted;
                    line.illumination = ReticleTree.Light.Type.NightIllumination;

                    if (i > 0)
                    {
                        line.position.y = -1.46f;
                    }
                }
            }

            foreach (int i in new int[] { 1, 3 })
            {
                ReticleTree.Angular line = new ReticleTree.Angular(new Vector2(0f, 0f), null);
                Util.ShallowCopy(line, horizontal.elements[i] as ReticleTree.Angular);
                line.position = new ReticleTree.Position(horizontal.elements[i].position.x * 3f, 0);
                horizontal.elements.Add(line);

                line = new ReticleTree.Angular(new Vector2(0f, 0f), null);
                Util.ShallowCopy(line, horizontal.elements[i] as ReticleTree.Angular);
                line.position = new ReticleTree.Position(horizontal.elements[i].position.x * 4f, 0);
                horizontal.elements.Add(line);
            }

            ReticleTree.Angular stadia = angular2.elements[4] as ReticleTree.Angular;
            stadia.position.x -= 18f;
            (stadia.elements[0] as ReticleTree.Line).visualType = ReticleTree.VisualElement.Type.Painted;
            (stadia.elements[0] as ReticleTree.Line).illumination = ReticleTree.Light.Type.NightIllumination;

            (stadia.elements[1] as ReticleTree.Line).visualType = ReticleTree.VisualElement.Type.Painted;
            (stadia.elements[1] as ReticleTree.Line).illumination = ReticleTree.Light.Type.NightIllumination;

            ReticleTree.Stadia the_real_stadia = stadia.elements[2] as ReticleTree.Stadia;
            for (int i = 0; i < the_real_stadia.elements.Count; i++)
            {
                ReticleTree.Angular marking = the_real_stadia.elements[i] as ReticleTree.Angular;

                for (int k = 0; k < marking.elements.Count; k++)
                {
                    (marking.elements[k] as ReticleTree.VisualElement).visualType = ReticleTree.VisualElement.Type.Painted;
                    (marking.elements[k] as ReticleTree.VisualElement).illumination = ReticleTree.Light.Type.NightIllumination;
                }
            }
        }

        public static void LoadAssets()
        {
            if (assets_loaded) return;
            if (!btr60_patch.Value) return;

            m60a1_nvs = GameObject.Instantiate(Assets.m60a1_nvs);
            m60a1_nvs.SetActive(false);
            UnityEngine.Object.Destroy(m60a1_nvs.transform.GetChild(0).gameObject);
            GameObject.Destroy(m60a1_nvs.transform.GetChild(0).gameObject);
            Component.Destroy(m60a1_nvs.GetComponent<UsableOptic>());
            Component.Destroy(m60a1_nvs.GetComponent<CameraSlot>());
            m60a1_nvs.GetComponent<PostProcessVolume>().enabled = false;
            m60a1_nvs.GetComponent<PostProcessVolume>().priority = 0;

            var bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/btr60a", "btr80a"));
            btr60a_turret_complete = bundle.LoadAsset<GameObject>("BTR80A_TURRET.prefab");
            btr60a_turret_complete.hideFlags = HideFlags.DontUnloadUnusedAsset;
            btr60a_turret_complete.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

            Transform turret = btr60a_turret_complete.transform.Find("BTR_80_B");
            Transform gun = btr60a_turret_complete.transform.Find("BTR_80_C");

            GameObject turret_armour = turret.transform.Find("ARMOUR").gameObject;
            GameObject gun_armour = gun.transform.Find("ARMOUR").gameObject;
            turret_armour.tag = "Penetrable";
            gun_armour.tag = "Penetrable";
            turret_armour.layer = 8;
            gun_armour.layer = 8;

            UniformArmor turret_u_armour = turret_armour.AddComponent<UniformArmor>();
            turret_u_armour.PrimaryHeatRha = 20f;
            turret_u_armour.PrimarySabotRha = 20f;
            turret_u_armour.SetName("turret");

            UniformArmor gun_u_armour = gun_armour.AddComponent<UniformArmor>();
            gun_u_armour.PrimaryHeatRha = 10f;
            gun_u_armour.PrimarySabotRha = 10f;
            gun_u_armour.SetName("weapons assembly");

            turret.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard (FLIR)");
            gun.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard (FLIR)");
            btr60a_turret_complete.gameObject.AddComponent<HeatSource>().heat = 5f;

            Reticle();

            assets_loaded = true;
        }

        public static void Init()
        {
            if (!btr60_patch.Value) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
