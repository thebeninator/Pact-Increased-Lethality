using System.Collections;
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
using UnityEngine;
using ModUtil;
using System.Collections.Generic;

namespace PactIncreasedLethality
{
    public class T62 : Module
    {
        static MelonPreferences_Entry<bool> t62_patch;
        static MelonPreferences_Entry<bool> better_stab;
        internal static MelonPreferences_Entry<bool> has_lrf;
        static MelonPreferences_Entry<bool> has_drozd;
        internal static MelonPreferences_Entry<bool> use_9m117;
        static MelonPreferences_Entry<bool> tpn3;
        static MelonPreferences_Entry<bool> applique;
        static MelonPreferences_Entry<bool> engine_upr;

        static GameObject t62m_kit;
        static GameObject t62m_lrf;
        static Mesh t62m_hull;
        static Mesh t62m_turret;

        private class HideCheeks : MonoBehaviour {
            public MeshRenderer mesh_rend;

            void OnEnable() {
                mesh_rend.enabled = false;
            }

            void OnDisable() {
                mesh_rend.enabled = true;
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
                    GameObject lrf_holder = GameObject.Instantiate(t62m_lrf, vic.transform.Find("T62_base/hull"));
                    Transform lrf = lrf_holder.transform.Find("LRF");
                    Transform gun = vic.transform.Find("---T62_rig---/HULL/TURRET/GUN");
                    lrf_holder.transform.SetParent(gun.GetComponent<LateFollowTarget>()._lateFollowers[0].transform);

                    Transform laser_armour = lrf.transform.Find("ARMOUR");
                    GHPC.Equipment.DestructibleComponent laser_destr = laser_armour.gameObject.AddComponent<GHPC.Equipment.DestructibleComponent>();
                    laser_destr._health = 5f;
                    laser_destr._fullHealth = 5f;
                    laser_destr._pressureTolerance = 1f;
                    laser_destr._shockResistance = 0.30f;
                    laser_destr._name = "laser rangefinder";

                    fcs.LaserComponent = laser_destr;
                    laser_destr.Destroyed += fcs.LaserDestroyed;

                    weapon.FCS.gameObject.AddComponent<LimitedLRF>();
                    fcs.MaxLaserRange = 4000f;

                    GameObject t = GameObject.Instantiate(T55.range_readout);
                    lrf_canvas = t.transform;
                    t.GetComponent<Reparent>().NewParent = day_optic.transform;        
                    t.transform.GetChild(0).transform.localPosition = new Vector3(-284.1897f, -5.5217f, 0.1f);
                    t.SetActive(true);

                    weapon.FCS.GetComponent<LimitedLRF>().canvas = t.transform;

                    day_optic.reticleMesh.reticleSO = T55.reticleSO;
                    day_optic.reticleMesh.reticle = T55.reticle_cached;
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
                    foreach (Transform t in vic.transform.Find("---T62_rig---/HULL/TURRET"))
                    {
                        if (t.name == "night sight cover") t.gameObject.SetActive(false);
                    }

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

                    vic.transform.Find("T62_base/hull").GetComponent<MeshFilter>().sharedMesh = t62m_hull;
                    vic.transform.Find("---T62_rig---/HULL/TURRET/turret").GetComponent<MeshFilter>().sharedMesh = t62m_turret;

                    GameObject _t62m_kit = GameObject.Instantiate(t62m_kit, vic.transform.Find("T62_base/hull"));
                    _t62m_kit.transform.Find("HULL/SKIRTS").GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { SharedAssets.t80b_mat });

                    Transform hull_parts = _t62m_kit.transform.Find("HULL");
                    hull_parts.SetParent(vic.GetComponent<LateFollowTarget>()._lateFollowers[0].transform);

                    Transform turret_parts = _t62m_kit.transform.Find("TURRET");
                    turret_parts.SetParent(vic.transform.Find("---T62_rig---/HULL/TURRET").GetComponent<LateFollowTarget>()._lateFollowers[0].transform);

                    Transform gun_parts = _t62m_kit.transform.Find("GUN");
                    Transform gun = vic.transform.Find("---T62_rig---/HULL/TURRET/GUN");
                    gun_parts.transform.Find("SLEEVE").SetParent(gun.Find("muzzle"));
                    gun_parts.SetParent(gun.GetComponent<LateFollowTarget>()._lateFollowers[0].transform);

                    HideCheeks hc = day_optic.gameObject.AddComponent<HideCheeks>();
                    hc.mesh_rend = turret_parts.Find("CHEEKS").GetComponent<MeshRenderer>();

                    Transform laser_origin = turret_parts.Find("CHEEKS/LASER ORIGIN");
                    laser_origin.SetParent(fcs.transform);
                    fcs.LaserOrigin = laser_origin;

                    vic._friendlyName = "T-62M";

                    if (engine_upr.Value) vic._friendlyName += "-1";
                }
            }

            yield break;
        }

        public override void LoadStaticAssets()
        {
            if (!t62_patch.Value) return;

            var t62m_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t62mv2"));

            t62m_kit = t62m_bundle.LoadAsset<GameObject>("t62m1_full.prefab");
            t62m_kit.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t62m_lrf = t62m_bundle.LoadAsset<GameObject>("t62_lrf.prefab");
            t62m_lrf.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t62m_hull = t62m_bundle.LoadAsset<Mesh>("t62m_hull.asset");
            t62m_hull.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t62m_turret = t62m_bundle.LoadAsset<Mesh>("t62m_turret.asset");
            t62m_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

            Util.SetupFLIRShaders(t62m_kit);
            Util.SetupFLIRShaders(t62m_lrf);

            Transform hull_armour = t62m_kit.transform.Find("HULL/HULL PLATE/ARMOUR");
            Transform turret_armour = t62m_kit.transform.Find("TURRET/CHEEKS/ARMOUR");
            Transform lrf = t62m_lrf.transform.Find("LRF");
            Transform[] all_armour_transforms = new Transform[] { hull_armour, turret_armour };

            foreach (Transform t in all_armour_transforms)
            {
                foreach (Transform t_child in t.GetComponentsInChildren<Transform>())
                {
                    t_child.gameObject.tag = "Penetrable";
                    t_child.gameObject.layer = 8;
                }
            }

            lrf.Find("ARMOUR").gameObject.tag = "Penetrable";
            lrf.Find("ARMOUR").gameObject.layer = 8;
            UniformArmor armor_lrf = lrf.Find("ARMOUR").gameObject.AddComponent<UniformArmor>();
            armor_lrf.SetName("laser rangefinder box");
            armor_lrf.PrimaryHeatRha = 15f;
            armor_lrf.PrimarySabotRha = 15f;

            GameObject turret_mpoly = turret_armour.transform.Find("MPOLY").gameObject;
            VariableArmor armour_turret_mpoly = turret_mpoly.AddComponent<VariableArmor>();
            armour_turret_mpoly.SetName("metal-polymer block");
            armour_turret_mpoly._armorType = Armour.cheek_metal_polymer;
            armour_turret_mpoly._spallForwardRatio = 0.2f;
            armour_turret_mpoly._noImpactDecals = true;
            AarVisual aar_cheek = turret_mpoly.AddComponent<AarVisual>();
            aar_cheek.SwitchMaterials = false;
            aar_cheek.HideUntilAar = true;

            GameObject turret_casing = turret_armour.transform.Find("CHEEKS").gameObject;
            UniformArmor armour_turret_casing = turret_casing.AddComponent<UniformArmor>();
            armour_turret_casing.SetName("applique cheek armor");
            armour_turret_casing._armorType = Armour.ru_cast_armor;
            armour_turret_casing.PrimaryHeatRha = 20f;
            armour_turret_casing.PrimarySabotRha = 20f;
            armour_turret_casing._normalizesHits = true;

            GameObject hull_mpoly_block = hull_armour.transform.Find("MPOLY BLOCK").gameObject;
            VariableArmor armor_mpoly_block = hull_mpoly_block.AddComponent<VariableArmor>();
            armor_mpoly_block.SetName("metal-polymer block");
            armor_mpoly_block._armorType = Armour.hull_metal_polymer;
            armor_mpoly_block._spallForwardRatio = 0.01f;
            AarVisual aar_mpoly_block = hull_mpoly_block.AddComponent<AarVisual>();
            aar_mpoly_block.SwitchMaterials = false;
            aar_mpoly_block.HideUntilAar = true;

            GameObject hull_casing = hull_armour.transform.Find("OUTER CASING").gameObject;
            VariableArmor armor_casing = hull_casing.AddComponent<VariableArmor>();
            armor_casing.SetName("upper glacis applique armor");
            armor_casing._armorType = Armour.ru_welded_armor;
            armor_casing._spallForwardRatio = 0.01f;
            armor_casing._normalizesHits = true;
        }

        public static void Init()
        {
            if (!t62_patch.Value) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
