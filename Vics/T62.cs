using System.Collections;
using System.Collections.Generic;
using System.IO;
using GHPC;
using GHPC.Equipment.Optics;
using GHPC.State;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using MelonLoader;
using MelonLoader.Utils;
using NWH.VehiclePhysics;
using Reticle;
using GHPC.Thermals;
using TMPro;
using UnityEngine;
using GHPC.Weaponry;

namespace PactIncreasedLethality
{
    public class T62
    {
        static MelonPreferences_Entry<bool> t62_patch;
        static MelonPreferences_Entry<bool> better_stab;
        static MelonPreferences_Entry<bool> has_lrf;
        static MelonPreferences_Entry<bool> has_drozd;
        static MelonPreferences_Entry<bool> use_9m117;
        static MelonPreferences_Entry<bool> tpn3;
        static MelonPreferences_Entry<bool> applique;
        static MelonPreferences_Entry<bool> engine_upr;

        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static Texture2D t62m_turret;
        static GameObject t62m_turret_parts;
        static GameObject t62m_lrf;
        static GameObject t62m_hull_parts;
        static GameObject t62m_skirts;
        static Mesh t62m_hull;

        private static bool assets_loaded = false;

        private class HideLeftCheek : MonoBehaviour {
            public Transform cheek;

            void OnEnable() {
                cheek.gameObject.SetActive(false);
            }

            void OnDisable() {
                cheek.gameObject.SetActive(true);
            }
        }

        public static void Config(MelonPreferences_Category cfg)
        {
            t62_patch = cfg.CreateEntry<bool>("T-62 Patch", true);
            t62_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            better_stab = cfg.CreateEntry<bool>("Better Stabilizer (T-62)", true);
            better_stab.Comment = "Less reticle blur, shake while on the move";

            use_9m117 = cfg.CreateEntry<bool>("Use 9M117 (T-62)", true);
            use_9m117.Comment = "GLATGM, has its own sight with fixed 8x magnification";

            has_lrf = cfg.CreateEntry<bool>("Laser Rangefinder (T-62)", true);
            has_lrf.Comment = "Only gives range: user will need to set range manually";

            //has_drozd = cfg.CreateEntry<bool>("Drozd APS (T-62)", false);
            //has_drozd.Comment = "Intercepts incoming projectiles; covers the frontal arc of the tank relative to where the turret is facing";

            tpn3 = cfg.CreateEntry<bool>("TPN-3 Night Sight (T-62)", true);
            tpn3.Comment = "Replaces the night sight with the one found on the T-80B/T-64B";

            applique = cfg.CreateEntry<bool>("BDD Applique (T-62)", true);
            applique.Comment = "Composite applique hull and turret cheek armour";

            engine_upr = cfg.CreateEntry<bool>("Engine Upgrade (T-62)", true);
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (vic.FriendlyName != "T-62") continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                UsableOptic day_optic = Util.GetDayOptic(fcs);
                Transform lrf_canvas = null;

                if (has_lrf.Value)
                {
                    GameObject lrf = GameObject.Instantiate(t62m_lrf, vic.transform.Find("---T62_rig---/HULL/TURRET/turret"));
                    lrf.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

                    Transform laser = lrf.transform.Find("LRF");
                    laser.gameObject.AddComponent<LateFollowTarget>();

                    LateFollow laser_armor = laser.transform.Find("LRF").gameObject.AddComponent<LateFollow>();
                    laser_armor.gameObject.SetActive(true);
                    laser_armor.FollowTarget = laser;
                    laser_armor.ForceToRoot = true;
                    laser_armor.enabled = true;
                    laser_armor.Awake();
                    laser_armor.gameObject.AddComponent<Reparent>();
                    laser.parent = vic.transform.Find("---T62_rig---/HULL/TURRET/GUN");

                    weapon.FCS.gameObject.AddComponent<LimitedLRF>();
                    fcs.MaxLaserRange = 4000f;

                    GameObject t = GameObject.Instantiate(T55.range_readout);
                    lrf_canvas = t.transform;
                    t.GetComponent<Reparent>().NewParent = day_optic.transform;        
                    t.transform.GetChild(0).transform.localPosition = new Vector3(-284.1897f, -5.5217f, 0.1f);
                    t.SetActive(true);

                    weapon.FCS.GetComponent<LimitedLRF>().canvas = t.transform;

                    if (!reticleSO)
                    {
                        ReticleTree.Angular reticle = null;

                        reticleSO = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T62 Corrected"].tree);
                        reticleSO.name = "T62withdalaser";

                        Util.ShallowCopy(reticle_cached, ReticleMesh.cachedReticles["T62 Corrected"]);
                        reticle_cached.tree = reticleSO;

                        reticle_cached.tree.lights = new List<ReticleTree.Light>() {
                        new ReticleTree.Light(),
                        new ReticleTree.Light()
                    };

                        reticle_cached.tree.lights[0] = ReticleMesh.cachedReticles["T62 Corrected"].tree.lights[0];
                        reticle_cached.tree.lights[1].type = ReticleTree.Light.Type.Powered;
                        reticle_cached.tree.lights[1].color = new RGB(15f, 0f, 0f, true);

                        reticleSO.planes[0].elements.Add(new ReticleTree.Angular(new Vector2(0, 0), null, ReticleTree.GroupBase.Alignment.LasePoint));
                        reticle = reticleSO.planes[0].elements[2] as ReticleTree.Angular;
                        reticle_cached.mesh = null;

                        reticle.elements.Add(new ReticleTree.Circle());
                        reticle.name = "LasePoint";
                        reticle.position = new ReticleTree.Position(0, 0, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                        ReticleTree.Circle circle = reticle.elements[0] as ReticleTree.Circle;
                        circle.radius.mrad = 0.5236f;
                        circle.thickness.mrad = 0.16f;
                        circle.illumination = ReticleTree.Light.Type.Powered;
                        circle.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;
                        circle.position = new ReticleTree.Position(0, 0, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                        circle.position.x = 0;
                        circle.position.y = 0;
                    }

                    day_optic.reticleMesh.reticleSO = reticleSO;
                    day_optic.reticleMesh.reticle = reticle_cached;
                    day_optic.reticleMesh.SMR = null;
                    day_optic.reticleMesh.Load();
                }

                if (better_stab.Value)
                {
                    day_optic.slot.VibrationBlurScale = 0.1f;
                    day_optic.slot.VibrationShakeMultiplier = 0.2f;
                }

                if (use_9m117.Value)
                {
                    weapon.Feed.ReloadDuringMissileTracking = false;
                    GameObject guidance_computer_obj = new GameObject("guidance computer");
                    guidance_computer_obj.transform.parent = vic.transform;
                    guidance_computer_obj.AddComponent<MissileGuidanceUnit>();

                    guidance_computer_obj.AddComponent<Reparent>();
                    Reparent reparent = guidance_computer_obj.GetComponent<Reparent>();
                    reparent.NewParent = vic_go.transform.Find("---T62_rig---/HULL/TURRET").gameObject.transform;
                    reparent.Awake();

                    MissileGuidanceUnit computer = guidance_computer_obj.GetComponent<MissileGuidanceUnit>();
                    computer.AimElement = weapon.FCS.AimTransform;
                    weapon.GuidanceUnit = computer;

                    BOM.Add(day_optic.transform, lrf_canvas);

                    loadout_manager.LoadedAmmoList.AmmoClips = Util.AppendToArray(loadout_manager.LoadedAmmoList.AmmoClips, T55.clip_codex_9m117);
                    loadout_manager._totalAmmoTypes = 4;
                    loadout_manager.TotalAmmoCounts = new int[] { 20, 10, 6, 4 };
                }

                for (int i = 0; i < loadout_manager.RackLoadouts.Length; i++)
                {
                    GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;

                    if (use_9m117.Value)
                    {
                        if (i == 0 || i == 3)
                        {
                            loadout_manager.RackLoadouts[i].FixedChoices = new LoadoutManager.RackLoadoutFixedChoice[] {
                                new LoadoutManager.RackLoadoutFixedChoice() {
                                    AmmoClipIndex = 3,
                                    RackSlotIndex = 0,
                                },
                                new LoadoutManager.RackLoadoutFixedChoice() {
                                    AmmoClipIndex = 3,
                                    RackSlotIndex = 1,
                                }
                            };
                        }
                    }

                    Util.EmptyRack(rack);
                }

                loadout_manager.SpawnCurrentLoadout();
                weapon.Feed.AmmoTypeInBreech = null;
                weapon.Feed.Start();
                loadout_manager.RegisterAllBallistics();

                //if (has_drozd.Value)
                //{
                //    List<DrozdLauncher> launchers = new List<DrozdLauncher>();
                //    vic.transform.Find("---T62_rig---/HULL/TURRET/ammobox").gameObject.SetActive(false);

                //    Vector3[] launcher_positions = new Vector3[] {
                //        new Vector3(-1.2952f, -0.1383f, -0.2131f),
                //        new Vector3(-1.2543f, 0.1291f, -0.2131f),
                //        new Vector3(1.2952f, -0.1383f, -0.2131f),
                //        new Vector3(1.2543f, 0.1291f, -0.2131f),
                //    };

                //    Vector3[] launcher_rots = new Vector3[] 
                //    {
                //        new Vector3(0f, 0f, 0f),
                //        new Vector3(0f, 335.8091f, 0f),
                //        new Vector3(0f, 0f, 0f),
                //        new Vector3(0f, -335.8091f, 0f)
                //    };

                //    for (var i = 0; i < launcher_positions.Length; i++)
                //    {
                //        GameObject launcher = GameObject.Instantiate(DrozdLauncher.drozd_launcher_visual, vic.transform.Find("---T62_rig---/HULL/TURRET"));
                //        launcher.transform.localPosition = launcher_positions[i];
                //        launcher.transform.localEulerAngles = launcher_rots[i];

                //        if (i > 1)
                //        {
                //            launcher.transform.localScale = Vector3.Scale(launcher.transform.localScale, new Vector3(-1f, 1f, 1f));
                //        }

                //        launchers.Add(launcher.GetComponent<DrozdLauncher>());
                //    }

                //    Drozd.AttachDrozd(
                //        vic.transform.Find("---T62_rig---/HULL/TURRET"), vic, new Vector3(0f, 0f, 9.5f),
                //        launchers.GetRange(0, 2).ToArray(), launchers.GetRange(2, 2).ToArray()
                //    );

                //    vic._friendlyName += "D";
                //}

                if (tpn3.Value)
                {
                    TPN3.Add(fcs, day_optic.slot.LinkedNightSight.PairedOptic, day_optic.slot.LinkedNightSight);
                }

                if (engine_upr.Value)
                {
                    vic.transform.GetComponent<VehicleController>().engine.maxPower = 650f;
                }

                if (applique.Value)
                {
                    MeshRenderer turret_renderer = vic.transform.Find("---T62_rig---/HULL/TURRET/turret").GetComponent<MeshRenderer>();
                    Material[] new_materials = turret_renderer.materials;
                    Material mat = new_materials[0];

                    // https://github.com/Unity-Technologies/UnityCsReference/blob/1d7b2b49b93ea5773aa4e8dfa504e3c1533ce282/Editor/Mono/Inspector/StandardShaderGUI.cs#L369
                    mat.SetTexture("_Albedo", t62m_turret);
                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                    mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                    mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetFloat("_ZWrite", 0.0f);
                    mat.EnableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    //new_materials[1] = mat;
                    //turret_skinned.materials = new_materials;

                    Transform turret = vic.transform.Find("---T62_rig---/HULL/TURRET/turret");
                    turret.gameObject.AddComponent<LateFollowTarget>();

                    foreach (Transform t in vic.transform.Find("---T62_rig---/HULL/TURRET"))
                    {
                        if (t.name == "night sight cover") t.gameObject.SetActive(false);
                    }

                    GameObject turret_parts = GameObject.Instantiate(t62m_turret_parts, turret);
                    turret_parts.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

                    LateFollow turret_armour = turret_parts.transform.Find("ARMOUR").gameObject.AddComponent<LateFollow>();
                    turret_armour.gameObject.SetActive(true);
                    turret_armour.FollowTarget = turret;
                    turret_armour.ForceToRoot = true;
                    turret_armour.enabled = true;
                    turret_armour.Awake();
                    turret_armour.gameObject.AddComponent<Reparent>();

                    HideLeftCheek hlc = day_optic.gameObject.AddComponent<HideLeftCheek>();
                    hlc.cheek = turret_parts.transform.Find("left");

                    Transform hull = vic.transform.Find("T62_base/hull");
                    hull.GetComponent<MeshFilter>().sharedMesh = t62m_hull;

                    hull.gameObject.AddComponent<LateFollowTarget>();
                    GameObject hull_parts = GameObject.Instantiate(t62m_hull_parts, hull);
                    hull_parts.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

                    LateFollow hull_armour = hull_parts.transform.Find("ARMOUR").gameObject.AddComponent<LateFollow>();
                    hull_armour.gameObject.SetActive(true);
                    hull_armour.FollowTarget = hull;
                    hull_armour.ForceToRoot = true;
                    hull_armour.enabled = true;
                    hull_armour.Awake();
                    hull_armour.gameObject.AddComponent<Reparent>();

                    GameObject skirts = GameObject.Instantiate(t62m_skirts, hull);
                    skirts.transform.localEulerAngles = Vector3.zero;

                    vic._friendlyName = "T-62M";
                    if (engine_upr.Value) vic._friendlyName += "-1";
                }
            }

            yield break;
        }

        public static void LoadAssets()
        {
            if (assets_loaded) return;
            if (!t62_patch.Value) return;

            var t62m_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t62m"));
            t62m_turret = t62m_bundle.LoadAsset<Texture2D>("turret_cleaned.png");
            t62m_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t62m_turret_parts = t62m_bundle.LoadAsset<GameObject>("T62M TURRET PARTS.prefab");
            t62m_turret_parts.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t62m_hull_parts = t62m_bundle.LoadAsset<GameObject>("T62M HULL PARTS.prefab");
            t62m_hull_parts.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t62m_lrf = t62m_bundle.LoadAsset<GameObject>("T62M LRF.prefab");
            t62m_lrf.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t62m_hull = t62m_bundle.LoadAsset<Mesh>("hull.asset");
            t62m_hull.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t62m_skirts = t62m_bundle.LoadAsset<GameObject>("T62M SKIRTS.prefab");
            t62m_skirts.hideFlags = HideFlags.DontUnloadUnusedAsset;

            Util.SetupFLIRShaders(t62m_turret_parts);
            Util.SetupFLIRShaders(t62m_hull_parts);
            Util.SetupFLIRShaders(t62m_lrf);
            Util.SetupFLIRShaders(t62m_skirts);

            Transform armour = t62m_turret_parts.transform.Find("ARMOUR");
            foreach (Transform t in armour.GetComponentsInChildren<Transform>())
            {
                t.gameObject.tag = "Penetrable";
                t.gameObject.layer = 8;
            }

            Transform hull_armour = t62m_hull_parts.transform.Find("ARMOUR");
            foreach (Transform t in hull_armour.GetComponentsInChildren<Transform>())
            {
                t.gameObject.tag = "Penetrable";
                t.gameObject.layer = 8;
            }

            GameObject lrf = t62m_lrf.transform.Find("LRF/LRF").gameObject;
            lrf.gameObject.tag = "Penetrable";
            lrf.gameObject.layer = 8;

            UniformArmor armor_lrf = lrf.AddComponent<UniformArmor>();
            armor_lrf.SetName("laser rangefinder");
            armor_lrf.PrimaryHeatRha = 15f;
            armor_lrf.PrimarySabotRha = 15f;

            GameObject turret_left_cheek = armour.transform.Find("L COMP CHEEK").gameObject;
            VariableArmor armor_turret_l_cheek = turret_left_cheek.AddComponent<VariableArmor>();
            armor_turret_l_cheek.SetName("metal-polymer block");
            armor_turret_l_cheek._armorType = Armour.cheek_metal_polymer;
            armor_turret_l_cheek._spallForwardRatio = 0.2f;
            AarVisual aar_l_cheek = turret_left_cheek.AddComponent<AarVisual>();
            aar_l_cheek.SwitchMaterials = false;
            aar_l_cheek.HideUntilAar = true;

            GameObject turret_right_cheek = armour.transform.Find("R COMP CHEEK").gameObject;
            VariableArmor armor_turret_r_cheek = turret_right_cheek.AddComponent<VariableArmor>();
            armor_turret_r_cheek.SetName("metal-polymer block");
            armor_turret_r_cheek._armorType = Armour.cheek_metal_polymer;
            armor_turret_r_cheek._spallForwardRatio = 0.2f;
            AarVisual aar_r_cheek = turret_right_cheek.AddComponent<AarVisual>();
            aar_r_cheek.SwitchMaterials = false;
            aar_r_cheek.HideUntilAar = true;

            GameObject turret_left_applique = armour.transform.Find("L APPLIQUE CHEEK").gameObject;
            VariableArmor armor_turret_l_applique = turret_left_applique.AddComponent<VariableArmor>();
            armor_turret_l_applique.SetName("applique cheek armor");
            armor_turret_l_applique._armorType = Armour.bdd_cast_armor;
            armor_turret_l_applique._spallForwardRatio = 0.5f;

            GameObject turret_right_applique = armour.transform.Find("R APPLIQUE CHEEK").gameObject;
            VariableArmor armor_turret_r_applique = turret_right_applique.AddComponent<VariableArmor>();
            armor_turret_r_applique.SetName("applique cheek armor");
            armor_turret_r_applique._armorType = Armour.bdd_cast_armor;
            armor_turret_r_applique._spallForwardRatio = 0.5f;

            GameObject hull_mpoly_block = hull_armour.transform.Find("MPOLY BLOCK").gameObject;
            VariableArmor armor_mpoly_block = hull_mpoly_block.AddComponent<VariableArmor>();
            armor_mpoly_block.SetName("metal-polymer block");
            armor_mpoly_block._armorType = Armour.hull_metal_polymer;
            armor_mpoly_block._spallForwardRatio = 0.2f;
            AarVisual aar_mpoly_block = hull_mpoly_block.AddComponent<AarVisual>();
            aar_mpoly_block.SwitchMaterials = false;
            aar_mpoly_block.HideUntilAar = true;

            GameObject hull_casing = hull_armour.transform.Find("OUTER CASING ").gameObject;
            VariableArmor armor_casing = hull_casing.AddComponent<VariableArmor>();
            armor_casing.SetName("upper glacis applique armor");
            armor_casing._armorType = Armour.ru_welded_armor;
            armor_casing._spallForwardRatio = 0.2f;

            assets_loaded = true;
        }

        public static void Init()
        {
            if (!t62_patch.Value) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
