using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GHPC.Equipment.Optics;
using GHPC.State;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using GHPC;
using MelonLoader;
using UnityEngine;
using NWH.VehiclePhysics;
using MelonLoader.Utils;
using System.IO;
using GHPC.Equipment;
using Reticle;

namespace PactIncreasedLethality
{
    public class T80
    {
        static MelonPreferences_Entry<bool> t80_patch;
        static MelonPreferences_Entry<bool> super_engine;
        static MelonPreferences_Entry<string> t80_ammo_type;
        static MelonPreferences_Entry<bool> t80_random_ammo;
        static MelonPreferences_Entry<string> t80_atgm_type;
        static MelonPreferences_Entry<List<string>> t80_random_ammo_pool;
        static MelonPreferences_Entry<bool> thermals;
        static MelonPreferences_Entry<string> thermals_quality;
        static MelonPreferences_Entry<bool> zoom_snapper;
        static MelonPreferences_Entry<bool> super_fcs_t80;
        static MelonPreferences_Entry<bool> kontakt1;
        static MelonPreferences_Entry<bool> kontakt5;
        static MelonPreferences_Entry<bool> super_comp_cheeks;

        static GameObject t80u_turret;
        static GameObject t80u_hull;
        static GameObject t80u_hull_sides;
        static GameObject t80u_front_flaps;

        static Mesh t80u_turret_inserts; 

        static Mesh turret_cleaned_mesh;
        static Mesh hull_cleaned_mesh;
        static Mesh skirts_cleaned_mesh;

        static Mesh t80bv_turret;
        static Mesh t80bv_hull;
        static GameObject t80bv_full;

        static Material hull_destroyed;
        static Material turret_destroyed;
        static Material sides_destroyed;
        static Material t80u_turret_mat;

        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static WeaponSystemCodexScriptable gun_2a46m4;

        private static bool assets_loaded = false;

        public static void Config(MelonPreferences_Category cfg)
        {
            var random_ammo_pool = new List<string>()
            {
                "3BM26",
                "3BM32",
                "3BM42",
                "3BM46",
                "3BM60"
            };

            t80_patch = cfg.CreateEntry<bool>("T-80B Patch", true);
            t80_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            super_engine = cfg.CreateEntry<bool>("Super Engine/Transmission (T-80B)", false);
            super_engine.Comment = "vrrrrrrrrrrooooooooom";

            t80_ammo_type = cfg.CreateEntry<string>("AP Round (T-80B)", "3BM32");
            t80_ammo_type.Comment = "3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized), 3BM46, 3BM60";
            t80_ammo_type.Description = " ";

            t80_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-80B)", false);
            t80_random_ammo_pool = cfg.CreateEntry<List<string>>("Random AP Round Pool (T-80B)", random_ammo_pool);
            t80_random_ammo_pool.Comment = "3BM26, 3BM32, 3BM42, 3BM46, 3B60";

            t80_atgm_type = cfg.CreateEntry<string>("GLATGM (T-80B)", "9M119");
            t80_atgm_type.Comment = "9M112M, 9M119, 9M119M1";
            t80_atgm_type.Description = " ";

            zoom_snapper = cfg.CreateEntry<bool>("Quick Zoom Switch (T-80B)", true);
            zoom_snapper.Description = " ";
            zoom_snapper.Comment = "Press middle mouse to instantly switch between low and high magnification on the daysight";

            super_fcs_t80 = cfg.CreateEntry<bool>("Super FCS (T-80B)", false);
            super_fcs_t80.Comment = "Sosna-U: point-n-shoot, thermal sight, autotracking";

            thermals = cfg.CreateEntry<bool>("Has Thermals (T-80B)", false);
            thermals.Comment = "Replaces night vision sight with thermal sight";
            thermals_quality = cfg.CreateEntry<string>("Thermals Quality (T-80B)", "High");
            thermals_quality.Comment = "Low, High";

            kontakt1 = cfg.CreateEntry<bool>("Kontakt-1 ERA (T-80B)", true);
            kontakt1.Comment = "bricks";

            kontakt5 = cfg.CreateEntry<bool>("Kontakt-5 ERA (T-80B)", false);
            kontakt5.Comment = "T-80U conversion (comes with rubber flaps, minor cosmetic changes to default day sight)";

            super_comp_cheeks = cfg.CreateEntry<bool>("Improved Turret Composite (T-80B)", false);
            super_comp_cheeks.Comment = "thicker, more effective turret composite array";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (!vic.FriendlyName.Contains("T-80")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                UsableOptic day_optic = Util.GetDayOptic(weapon.FCS);
                Transform turret = vic.transform.Find("T80B_rig/HULL/TURRET");

                if (zoom_snapper.Value && !super_fcs_t80.Value)
                    day_optic.gameObject.AddComponent<DigitalZoomSnapper>();

                int rand = UnityEngine.Random.Range(0, Ammo_125mm.ap.Count);
                string ammo_str = t80_random_ammo.Value ? t80_random_ammo_pool.Value.ElementAt(rand) : t80_ammo_type.Value;

                vic.AimablePlatforms[1].transform.Find("optic cover parent").gameObject.SetActive(false);

                if (thermals.Value && !super_fcs_t80.Value)
                {
                    PactThermal.Add(weapon.FCS.NightOptic, thermals_quality.Value.ToLower(), true);
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);
    
                    weapon.FCS.NightOptic.Alignment = OpticAlignment.BoresightStabilized;
                    weapon.FCS.NightOptic.RotateAzimuth = true;
                }

                try
                {
                    if (ammo_str != "3BM15")
                        loadout_manager.LoadedAmmoList.AmmoClips[0] = Ammo_125mm.ap[ammo_str];

                    if (t80_atgm_type.Value != "9M112M")
                        loadout_manager.LoadedAmmoList.AmmoClips[3] = Ammo_125mm.atgm[t80_atgm_type.Value];

                    for (int i = 0; i < loadout_manager.RackLoadouts.Length; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;

                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();
                }
                catch (Exception)
                {
                    MelonLogger.Msg("Loading default ammo for " + vic.FriendlyName);
                }

                Transform canvas = vic.transform.Find("T80B_rig/HULL/TURRET/gun/---MAIN GUN SCRIPTS---/2A46-2/1G42 gunner's sight/GPS/1G42 Canvas/GameObject");
                canvas.Find("ammo text APFSDS (TMP)").gameObject.SetActive(true);

                if (super_engine.Value)
                {
                    VehicleController this_vic_controller = vic_go.GetComponent<VehicleController>();
                    NwhChassis chassis = vic_go.GetComponent<NwhChassis>();

                    Util.ShallowCopy(this_vic_controller.engine, Assets.abrams_vic_controller.engine);
                    Util.ShallowCopy(this_vic_controller.transmission, Assets.abrams_vic_controller.transmission);

                    this_vic_controller.engine.vc = vic_go.GetComponent<VehicleController>();
                    this_vic_controller.transmission.vc = vic_go.GetComponent<VehicleController>();
                    this_vic_controller.engine.Initialize(this_vic_controller);
                    this_vic_controller.engine.Start();
                    this_vic_controller.transmission.Initialize(this_vic_controller);

                    chassis._maxForwardSpeed = 25f;
                    chassis._maxReverseSpeed = 11.176f;
                    chassis._originalEnginePower = 1750.99f;
                }

                if (super_fcs_t80.Value)
                {
                    weapon.FCS.transform.Find("GPS/1G42 Canvas").gameObject.SetActive(false);

                    CustomGuidanceComputer gc = weapon.FCS.gameObject.AddComponent<CustomGuidanceComputer>();
                    gc.fcs = weapon.FCS;
                    gc.mgu = weapon.FCS.GetComponent<MissileGuidanceUnit>();

                    SuperFCS.Add(day_optic, weapon.FCS.NightOptic, vic.WeaponsManager.Weapons[1], vic.WeaponsManager.Weapons[0], gc);

                    day_optic.transform.Find("Quad").gameObject.SetActive(false);
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);
                }

                if (super_comp_cheeks.Value) {
                    Transform cheek_inserts = turret.GetComponent<LateFollowTarget>()._lateFollowers[0].transform.Find("T80_armour_turret/cheek inserts");
                    cheek_inserts.GetComponent<MeshFilter>().sharedMesh = t80u_turret_inserts;
                    cheek_inserts.GetComponent<MeshCollider>().sharedMesh = t80u_turret_inserts;
                    GameObject.DestroyImmediate(cheek_inserts.GetChild(0).gameObject);
                    cheek_inserts.GetComponent<VariableArmor>().cloneMesh();
                    cheek_inserts.GetComponent<VariableArmor>().invertMesh();
                    cheek_inserts.GetComponent<VariableArmor>().setupCollider();
                    cheek_inserts.GetComponent<VariableArmor>()._armorType = Armour.t80u_composite_armor;
                    cheek_inserts.GetComponent<AarVisual>().AarMaterial = t80u_turret_mat;
                }

                if (kontakt1.Value) {
                    Transform turret_rend = turret.Find("turret");
                    turret_rend.GetComponent<MeshFilter>().sharedMesh = t80bv_turret;
                    turret_rend.GetComponent<MeshRenderer>().materials[1].color = new Color(0, 0, 0, 0);

                    turret.Find("turret numbers").gameObject.SetActive(false);
                    vic.transform.Find("T80B_stowage/towropes_front").gameObject.SetActive(false);

                    for (int i = 1; i <= 5; i++)
                        turret.Find("smoke_l_" + i).localScale = Vector3.zero;
                    for (int i = 1; i <= 3; i++)
                        turret.Find("smoke_r_" + i).localScale = Vector3.zero;

                    Transform hull_rend = vic.transform.Find("T80_mesh/body");
                    hull_rend.GetComponent<MeshFilter>().sharedMesh = t80bv_hull;

                    GameObject k1_full = GameObject.Instantiate(t80bv_full, turret.Find("turret"));
                    k1_full.transform.localEulerAngles = new Vector3(0f, 270f, 90f);
                    k1_full.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                    LateFollow k1_turret_follow = k1_full.transform.Find("TURRET K1/TURRET K1 ARMOUR").gameObject.AddComponent<LateFollow>();
                    k1_turret_follow.FollowTarget = turret;
                    k1_turret_follow.enabled = true;
                    k1_turret_follow.Awake();
                    k1_full.transform.Find("TURRET K1/TURRET K1 ARMOUR").parent = null;

                    LateFollow k1_hull_follow = k1_full.transform.Find("HULL K1/HULL K1 ARMOUR").gameObject.AddComponent<LateFollow>();
                    k1_hull_follow.FollowTarget = turret.parent;
                    k1_hull_follow.enabled = true;
                    k1_hull_follow.Awake();
                    k1_full.transform.Find("HULL K1/HULL K1 ARMOUR").parent = null;

                    k1_full.transform.Find("HULL K1").parent = turret.parent;
                    k1_full.transform.Find("HULL MOUNTING FRAMES").parent = turret.parent;

                    Material[] smokes_mat = new Material[1];
                    smokes_mat[0] = vic.transform.Find("T80_mesh/skirts").GetComponent<MeshRenderer>().materials[0];
                    k1_full.transform.Find("SMOKE TUBES 1").GetComponent<MeshRenderer>().materials = smokes_mat;
                    k1_full.transform.Find("SMOKE TUBES 2").GetComponent<MeshRenderer>().materials = smokes_mat;

                    foreach (Transform smoke_cap in k1_full.transform.Find("SMOKES")) {
                        smoke_cap.GetComponent<MeshRenderer>().materials = smokes_mat;
                    }

                    VehicleSmokeManager smoke_manager = vic.transform.Find("T80 -Smoke Launcher System").GetComponent<VehicleSmokeManager>();
                    for (int i = 0; i < smoke_manager._smokeSlots.Length; i++)
                    {
                        VehicleSmokeManager.SmokeSlot slot = smoke_manager._smokeSlots[i];
                        Transform smoke_cap = k1_full.transform.Find("SMOKES").GetChild(i);
                        slot.DisplayBone = smoke_cap;
                        slot.SpawnLocation.transform.SetParent(smoke_cap);
                        slot.SpawnLocation.transform.position = smoke_cap.GetComponent<Renderer>().bounds.center;
                    }

                    vic.transform.Find("T80_camonet").gameObject.SetActive(false);
                    vic.transform.Find("T80B_rig/HULL/TURRET/---TURRET SCRIPTS---/net_turret").gameObject.SetActive(false);
                    vic.transform.Find("T80B_rig/HULL/TURRET/gun/---MAIN GUN SCRIPTS---/net_barrel").gameObject.SetActive(false);

                    vic._friendlyName = "T-80BV";
                }

                if (kontakt5.Value)
                {
                    weapon.CodexEntry = gun_2a46m4;
                    weapon.BaseDeviationAngle = 0.005f;

                    Transform turret_rend = turret.Find("turret");
                    turret_rend.GetComponent<MeshFilter>().sharedMesh = turret_cleaned_mesh;
                    turret_rend.GetComponent<MeshRenderer>().materials[1].color = new Color(0, 0, 0, 0);

                    turret.Find("turret numbers").gameObject.SetActive(false);
                    vic.transform.Find("T80B_stowage/towropes_front").gameObject.SetActive(false);

                    for (int i = 1; i <= 5; i++)
                        turret.Find("smoke_l_" + i).localScale = Vector3.zero;
                    for (int i = 1; i <= 3; i++)
                        turret.Find("smoke_r_" + i).localScale = Vector3.zero;

                    Transform hull_rend = vic.transform.Find("T80_mesh/body");
                    hull_rend.GetComponent<MeshFilter>().sharedMesh = hull_cleaned_mesh;

                    Transform skirts_rend = vic.transform.Find("T80_mesh/skirts");
                    skirts_rend.GetComponent<MeshFilter>().sharedMesh = skirts_cleaned_mesh;

                    GameObject k5_turret = GameObject.Instantiate(t80u_turret, turret_rend);
                    k5_turret.transform.localEulerAngles = new Vector3(90f, 0f, 180f);

                    LateFollow k5_turret_follow = k5_turret.AddComponent<LateFollow>();
                    k5_turret_follow.FollowTarget = turret;
                    k5_turret_follow.enabled = true;
                    k5_turret_follow.Awake();
                    k5_turret.transform.parent = null;

                    GameObject k5_hull = GameObject.Instantiate(t80u_hull, hull_rend);
                    k5_hull.transform.localEulerAngles = new Vector3(0f, -180f, 0f);

                    LateFollow k5_hull_follow = k5_hull.AddComponent<LateFollow>();
                    k5_hull_follow.FollowTarget = vic.transform;
                    k5_hull_follow.enabled = true;
                    k5_hull_follow.Awake();
                    k5_hull.transform.parent = null;

                    GameObject k5_sides = GameObject.Instantiate(t80u_hull_sides, skirts_rend);
                    k5_sides.transform.localEulerAngles = new Vector3(0f, -180f, 0f);

                    LateFollow k5_sides_follow = k5_sides.AddComponent<LateFollow>();
                    k5_sides_follow.FollowTarget = vic.transform;
                    k5_sides_follow.enabled = true;
                    k5_sides_follow.Awake();
                    k5_sides.transform.parent = null;

                    GameObject front_flaps = GameObject.Instantiate(t80u_front_flaps, hull_rend);
                    front_flaps.transform.localEulerAngles = new Vector3(0f, 270f, 0f);

                    LateFollow front_flaps_follow = front_flaps.AddComponent<LateFollow>();
                    front_flaps_follow.FollowTarget = vic.transform;
                    front_flaps_follow.enabled = true;
                    front_flaps_follow.Awake();
                    front_flaps.transform.parent = null;

                    Material[] front_flaps_mat = front_flaps.GetComponent<MeshRenderer>().materials;
                    front_flaps_mat[0] = skirts_rend.GetComponent<MeshRenderer>().materials[0];
                    front_flaps.GetComponent<MeshRenderer>().materials = front_flaps_mat;

                    foreach (Transform t in k5_turret.transform.Find("t80u_smokes"))
                    {
                        Material[] smokes_mat = t.GetComponent<MeshRenderer>().materials;
                        smokes_mat[0] = skirts_rend.GetComponent<MeshRenderer>().materials[0];
                        t.GetComponent<MeshRenderer>().materials = smokes_mat;
                    }

                    VehicleSmokeManager smoke_manager = vic.transform.Find("T80 -Smoke Launcher System").GetComponent<VehicleSmokeManager>();
                    for (int i = 0; i < smoke_manager._smokeSlots.Length; i++) {
                        VehicleSmokeManager.SmokeSlot slot = smoke_manager._smokeSlots[i];
                        Transform smoke_cap = k5_turret.transform.Find("t80u_smokes").GetChild(i);
                        slot.DisplayBone = smoke_cap;
                        slot.SpawnLocation.transform.SetParent(smoke_cap);
                        slot.SpawnLocation.transform.position = smoke_cap.GetComponent<Renderer>().bounds.center;
                    }

                    vic.transform.Find("T80_camonet").gameObject.SetActive(false);
                    vic.transform.Find("T80B_rig/HULL/TURRET/---TURRET SCRIPTS---/net_turret").gameObject.SetActive(false);
                    vic.transform.Find("T80B_rig/HULL/TURRET/gun/---MAIN GUN SCRIPTS---/net_barrel").gameObject.SetActive(false);
                    vic.transform.GetComponent<LateFollowTarget>()._lateFollowers[0].transform.Find("T80_armour_hull/16mm applique").gameObject.SetActive(false);

                    if (!reticleSO)
                    {
                        reticleSO = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["1G42"].tree);
                        reticleSO.name = "1G46";

                        Util.ShallowCopy(reticle_cached, ReticleMesh.cachedReticles["1G42"]);
                        reticle_cached.tree = reticleSO;

                        ReticleTree.FocalPlane plane = reticle_cached.tree.planes[0];
                        ReticleTree.Angular boresight = plane.elements[0] as ReticleTree.Angular;

                        for (int i = -1; i <= 1; i += 2) {
                            ReticleTree.Line line = new ReticleTree.Line(
                                position: new Vector2(1.3f * i, 0f), degrees: 0f, length: 1.45f, thickness: 0.1571f);
                            line.length.unit = AngularLength.AngularUnit.MIL_USSR;
                            line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                            line.visualType = ReticleTree.VisualElement.Type.Painted;
                            line.illumination = ReticleTree.Light.Type.NightIllumination;
                            boresight.elements.Add(line);
                        }

                        ReticleTree.VerticalBallistic mg_ballistic = (boresight.elements[3] as ReticleTree.Angular).elements[0] as ReticleTree.VerticalBallistic;

                        for (int i = 1; i < mg_ballistic.elements.Count; i++)
                        {
                            ReticleTree.Angular angular = mg_ballistic.elements[i] as ReticleTree.Angular;

                            foreach (ReticleTree.TransformElement e in angular.elements)
                            {
                                e.position.x *= -1;
                            }
                        }
                    }

                    if (!super_fcs_t80.Value)
                    {
                        day_optic.reticleMesh.reticleSO = reticleSO;
                        day_optic.reticleMesh.reticle = reticle_cached;
                        day_optic.reticleMesh.SMR = null;
                        day_optic.reticleMesh.Load();
                    }

                    vic._friendlyName = "T-80U";
                }
            }

            yield break;
        }

        public static void LoadAssets()
        {
            if (assets_loaded) return;
            if (!t80_patch.Value) return;

            gun_2a46m4 = ScriptableObject.CreateInstance<WeaponSystemCodexScriptable>();
            gun_2a46m4.name = "gun_2a46m4";
            gun_2a46m4.CaliberMm = 125;
            gun_2a46m4.FriendlyName = "125mm Gun 2A46M-4";
            gun_2a46m4.Type = WeaponSystemCodexScriptable.WeaponType.LargeCannon;        

            var t80u_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t80u"));
            var t80bv_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t80bv"));

            t80u_turret_mat = t80u_bundle.LoadAsset<Material>("inserts mat.mat");
            t80u_turret_mat.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t80u_turret_inserts = t80u_bundle.LoadAsset<Mesh>("t80u_cheek_inserts.asset");
            t80u_turret_inserts.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t80u_hull = t80u_bundle.LoadAsset<GameObject>("hull plate.prefab");
            t80u_hull.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t80u_hull_sides = t80u_bundle.LoadAsset<GameObject>("hull sides.prefab");
            t80u_hull_sides.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t80u_turret = t80u_bundle.LoadAsset<GameObject>("turret stuff.prefab");
            t80u_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t80u_front_flaps = t80u_bundle.LoadAsset<GameObject>("t80u_front_flaps.prefab");
            t80u_front_flaps.hideFlags = HideFlags.DontUnloadUnusedAsset;

            turret_cleaned_mesh = t80u_bundle.LoadAsset<Mesh>("turret cleaned.asset");
            turret_cleaned_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;

            hull_cleaned_mesh = t80u_bundle.LoadAsset<Mesh>("hull cleaned.asset");
            hull_cleaned_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;

            skirts_cleaned_mesh = t80u_bundle.LoadAsset<Mesh>("skirts cleaned.asset");
            skirts_cleaned_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;

            hull_destroyed = t80u_bundle.LoadAsset<Material>("hp plate destroyed");
            hull_destroyed.hideFlags = HideFlags.DontUnloadUnusedAsset;

            turret_destroyed = t80u_bundle.LoadAsset<Material>("turret plate destroyed");
            turret_destroyed.hideFlags = HideFlags.DontUnloadUnusedAsset;

            sides_destroyed = t80u_bundle.LoadAsset<Material>("hs plate destroyed");
            sides_destroyed.hideFlags = HideFlags.DontUnloadUnusedAsset;

            UniformArmor hull_plate = t80u_hull.transform.Find("PLATE/ARMOUR/plate").gameObject.AddComponent<UniformArmor>();
            hull_plate.tag = "Penetrable";
            hull_plate.gameObject.layer = 8;
            hull_plate._name = "glacis addon plate";
            hull_plate._armorType = Armour.ru_hhs_armor;
            hull_plate.PrimaryHeatRha = 30f;
            hull_plate.PrimarySabotRha = 30f;
            hull_plate._canShatterLongRods = true;
            hull_plate._normalizesHits = true;
            hull_plate.AngleMatters = true;

            UniformArmor hull_splash_guard = t80u_hull.transform.Find("PLATE/ARMOUR/splash").gameObject.AddComponent<UniformArmor>();
            hull_splash_guard.tag = "Penetrable";
            hull_splash_guard.gameObject.layer = 8;
            hull_splash_guard._name = "glacis splash guard";
            hull_splash_guard._armorType = Armour.ru_welded_armor;
            hull_splash_guard.PrimaryHeatRha = 3f;
            hull_splash_guard.PrimarySabotRha = 3f;
            hull_splash_guard._canShatterLongRods = true;
            hull_splash_guard._normalizesHits = true;
            hull_splash_guard.AngleMatters = true;

            GameObject t80u_front_flaps_armour = t80u_front_flaps.transform.Find("ARMOUR").gameObject;
            t80u_front_flaps_armour.tag = "Penetrable";
            t80u_front_flaps_armour.layer = 8;
            UniformArmor front_rubber_flaps = t80u_front_flaps_armour.AddComponent<UniformArmor>();
            front_rubber_flaps._name = "rubber flap";
            front_rubber_flaps.PrimaryHeatRha = 5f;
            front_rubber_flaps.PrimarySabotRha = 5f;
            front_rubber_flaps._canShatterLongRods = false;
            front_rubber_flaps._normalizesHits = false;
            front_rubber_flaps.AngleMatters = false;

            foreach (Transform rubber_transform in t80u_turret.transform.Find("TURRET RUBBER/ARMOUR")) {
                rubber_transform.gameObject.tag = "Penetrable";
                rubber_transform.gameObject.layer = 8;

                UniformArmor rubber_flaps = rubber_transform.gameObject.AddComponent<UniformArmor>();
                rubber_flaps._name = "rubber flap";
                rubber_flaps.PrimaryHeatRha = 5f;
                rubber_flaps.PrimarySabotRha = 5f;
                rubber_flaps._canShatterLongRods = false;
                rubber_flaps._normalizesHits = false;
                rubber_flaps.AngleMatters = false;
            }

            foreach (Transform plate_transform in t80u_turret.transform.Find("TURRET PLATE/ARMOUR"))
            {
                plate_transform.gameObject.tag = "Penetrable";
                plate_transform.gameObject.layer = 8;

                UniformArmor steel_plate = plate_transform.gameObject.AddComponent<UniformArmor>();
                steel_plate._name = "steel plate";
                steel_plate.PrimaryHeatRha = 5f;
                steel_plate.PrimarySabotRha = 5f;
                steel_plate._canShatterLongRods = true;
                steel_plate._normalizesHits = true;
                steel_plate.AngleMatters = true;
            }

            Transform turret_k5 = t80u_turret.transform.Find("TURRET ERA/ARMOUR");
            Transform hull_k5 = t80u_hull.transform.Find("HULL ERA/ARMOUR");
            Transform hull_sides_k5 = t80u_hull_sides.transform.Find("HULL SIDE ERA/ARMOUR");

            Kontakt5.Setup(turret_k5, turret_k5.parent, hide_on_detonate: false, destroyed_mat: turret_destroyed, destroyed_target: "turret plate");
            Kontakt5.Setup(hull_k5, hull_k5.parent, hide_on_detonate: false, destroyed_mat: hull_destroyed, destroyed_target: "hp plate");
            Kontakt5.Setup(hull_sides_k5, hull_sides_k5.parent, hide_on_detonate: false, destroyed_mat: sides_destroyed, destroyed_target: "hs plate");

            Util.SetupFLIRShaders(t80u_front_flaps);
            Util.SetupFLIRShaders(t80u_hull);
            Util.SetupFLIRShaders(t80u_hull_sides);
            Util.SetupFLIRShaders(t80u_turret);

            t80bv_turret = t80bv_bundle.LoadAsset<Mesh>("t80bv_turret.asset");
            t80bv_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t80bv_hull = t80bv_bundle.LoadAsset<Mesh>("t80bv_hull.asset");
            t80bv_hull.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t80bv_full = t80bv_bundle.LoadAsset<GameObject>("t80bv_k1_FULL.prefab");
            t80bv_full.hideFlags = HideFlags.DontUnloadUnusedAsset;

            Transform hull_k1 = t80bv_full.transform.Find("HULL K1/HULL K1 ARMOUR");
            Transform turret_k1 = t80bv_full.transform.Find("TURRET K1/TURRET K1 ARMOUR");
            Kontakt1.Setup(hull_k1, hull_k1.parent);
            Kontakt1.Setup(turret_k1, turret_k1.parent);

            Util.SetupFLIRShaders(t80bv_full);

            assets_loaded = true;
        }

        public static void Init()
        {
            if (!t80_patch.Value) return;

            StateController.WaitForComplete(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
