using System;
using System.Collections.Generic;
using System.Linq;
using GHPC.Equipment.Optics;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using UnityEngine;
using GHPC.State;
using System.Collections;
using MelonLoader;
using GHPC;
using GHPC.Effects.Voices;
using MelonLoader.Utils;
using System.IO;
using Thermals;
using NWH.VehiclePhysics;
using static UnityEngine.GraphicsBuffer;
using GHPC.Audio;
using GHPC.Effects;
using static PactIncreasedLethality.T80;
using HarmonyLib;
using UnityEngine.Rendering.PostProcessing;
using GHPC.Equipment;
using GHPC.Mission;
using GHPC.AI;
using GHPC.Mission.Data;
using GHPC.Camera;

namespace PactIncreasedLethality
{
    public class T72
    {
        static MelonPreferences_Entry<bool> t72_patch;
        static MelonPreferences_Entry<string> t72m_ammo_type;
        static MelonPreferences_Entry<string> t72m1_ammo_type;

        static MelonPreferences_Entry<string> t72m_heat_type;
        static MelonPreferences_Entry<string> t72m1_heat_type;

        static MelonPreferences_Entry<bool> t72m_random_ammo;
        static MelonPreferences_Entry<bool> t72m1_random_ammo;

        static MelonPreferences_Entry<List<string>> t72m_random_ammo_pool;
        static MelonPreferences_Entry<List<string>> t72m1_random_ammo_pool;

        static MelonPreferences_Entry<bool> thermals_t72m;
        static MelonPreferences_Entry<bool> thermals_t72m1;
        static MelonPreferences_Entry<string> thermals_quality;

        static MelonPreferences_Entry<bool> only_carousel;

        static MelonPreferences_Entry<bool> k5_t72m1;
        static MelonPreferences_Entry<bool> k5_t72m;

        static MelonPreferences_Entry<bool> era_t72m1;
        static MelonPreferences_Entry<bool> era_t72m;

        static MelonPreferences_Entry<bool> soviet_t72m;
        static MelonPreferences_Entry<bool> soviet_t72m1;

        static MelonPreferences_Entry<bool> super_fcs_t72m;
        static MelonPreferences_Entry<bool> super_fcs_t72m1;

        static MelonPreferences_Entry<bool> lead_calculator_t72m;
        static MelonPreferences_Entry<bool> lead_calculator_t72m1;

        static MelonPreferences_Entry<bool> tpn3_t72m;
        static MelonPreferences_Entry<bool> tpn3_t72m1;

        static MelonPreferences_Entry<bool> t72m_composite_cheeks;
        static MelonPreferences_Entry<bool> t72m_super_composite_cheeks;
        static MelonPreferences_Entry<bool> t72m1_super_composite_cheeks;

        static MelonPreferences_Entry<List<string>> empty_ammo_t72m;
        static MelonPreferences_Entry<List<string>> empty_ammo_t72m1;

        static MelonPreferences_Entry<bool> super_engine;

        static MelonPreferences_Entry<bool> better_stab;

        static VehicleController abrams_vic_controller;

        static AmmoClipCodexScriptable clip_codex_3bk18m;

        static Dictionary<string, int> ammo_racks = new Dictionary<string, int>() {
            ["Hull Wet"] = 1,
            ["Hull Rear"] = 2,
            ["Hull Front"] = 3,
            ["Turret Spare"] = 4
        };

        static GameObject soviet_crew_voice;
        static Mesh b3_turret_cleaned_mesh;
        static Mesh turret_cleaned_mesh;
        static Mesh hull_cleaned_mesh;

        static Mesh t72b_vis_turret;
        static Mesh t72b_armour_turret;
        static GameObject t72b_turret_nera;
        static GameObject t72b_k1_full;
        static Mesh t72b_hull;

        static GameObject t72av_k1_full;
        static Mesh t72av_turret;

        static GameObject t72b_k5_1989_full;
        static GameObject t72b_k5_b3_full;
        static GameObject t72b3m_ubh_kit;
        static Material t72b_k5_plate_destroyed;

        static Mesh t72b3_turret_mesh;
        static Mesh t72b_k5_hull_mesh;
        static Mesh t72b3ubh_turret_mesh;

        static ArmorCodexScriptable kontakt1_so;
        static ArmorType kontakt1_armour = new ArmorType();

        public static void Config(MelonPreferences_Category cfg)
        {
            var racks = new List<string>()
            {
                "Hull Wet",
                "Hull Rear",
                "Hull Front",
                "Turret Spare",
            };

            var random_ammo_pool = new List<string>()
            {
                "3BM22",
                "3BM26",
                "3BM32",
                "3BM42",
                "3BM46"
            };

            t72_patch = cfg.CreateEntry<bool>("T-72 Patch", true);
            t72_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";

            t72m_ammo_type = cfg.CreateEntry<string>("AP Round (T-72M)", "3BM22");
            t72m_ammo_type.Comment = "3BM15, 3BM22, 3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized), 3BM46";

            t72m1_ammo_type = cfg.CreateEntry<string>("AP Round (T-72M1)", "3BM32");

            t72m_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-72M)", false);
            t72m_random_ammo_pool = cfg.CreateEntry<List<string>>("Random AP Round Pool (T-72M)", random_ammo_pool);
            t72m_random_ammo_pool.Comment = "3BM22, 3BM26, 3BM32, 3BM42, 3BM46";

            t72m1_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-72M1)", false);
            t72m1_random_ammo_pool = cfg.CreateEntry<List<string>>("Random AP Round Pool (T-72M1)", random_ammo_pool);

            t72m_heat_type = cfg.CreateEntry<string>("HEAT Round (T-72M)", "3BK18M");
            t72m_heat_type.Comment = "3BK14M, 3BK18M";
            t72m1_heat_type = cfg.CreateEntry<string>("HEAT Round (T-72M1)", "3BK18M");

            tpn3_t72m = cfg.CreateEntry<bool>("TPN-3 Night Sight (T-72)", false);
            tpn3_t72m.Description = " ";
            tpn3_t72m.Comment = "Replaces the night sight with the one found on the T-80B/T-64B";
            tpn3_t72m1 = cfg.CreateEntry<bool>("TPN-3 Night Sight (T-72M1)", false);

            thermals_t72m = cfg.CreateEntry<bool>("Has Thermals (T-72M)", true);
            thermals_t72m.Comment = "Replaces night vision sight with thermal sight";
            thermals_t72m.Description = " ";
            thermals_t72m1 = cfg.CreateEntry<bool>("Has Thermals (T-72M1)", true);

            thermals_quality = cfg.CreateEntry<string>("Thermals Quality (T-72)", "Low");
            thermals_quality.Comment = "Low, High";

            k5_t72m1 = cfg.CreateEntry<bool>("Kontakt-5 ERA (T-72M1)", false);
            k5_t72m1.Comment = "    B           I           G           brick";
            k5_t72m1.Description = " ";

            k5_t72m = cfg.CreateEntry<bool>("Kontakt-5 ERA (T-72M)", false);

            era_t72m1 = cfg.CreateEntry<bool>("Kontakt-1 ERA (T-72M1)", true);
            era_t72m1.Comment = "BRICK ME UP LADS";

            era_t72m = cfg.CreateEntry<bool>("Kontakt-1 ERA (T-72M)", false);
            era_t72m.Comment = "BRICK ME UP LADS (gill variants will not have side-skirt ERA)";

            only_carousel = cfg.CreateEntry<bool>("Reduced Ammo Load", false);
            only_carousel.Comment = "Allows you to specify which ammo racks should be emptied (except carousel)";
            only_carousel.Description = " ";

            empty_ammo_t72m = cfg.CreateEntry<List<string>>("Empty Ammo Racks (T-72M)", racks);
            empty_ammo_t72m1 = cfg.CreateEntry<List<string>>("Empty Ammo Racks (T-72M1)", racks);
            empty_ammo_t72m.Comment = "Hull Wet, Hull Rear, Hull Front, Turret Spare";

            soviet_t72m = cfg.CreateEntry<bool>("Soviet Crew (T-72M)", false);
            soviet_t72m.Comment = "Also renames the tank to T-72 and removes NVA decals";
            soviet_t72m.Description = " ";

            soviet_t72m1 = cfg.CreateEntry<bool>("Soviet Crew (T-72M1)", false);
            soviet_t72m1.Comment = "Also renames the tank to T-72A and removes NVA decals";

            super_fcs_t72m = cfg.CreateEntry<bool>("Super FCS (T-72M)", false);
            super_fcs_t72m.Comment = "basically sosna-u lol (digital 4x-12x zoom, 2-axis stabilizer w/ lead, point-n-shoot)";
            super_fcs_t72m.Description = " ";
            super_fcs_t72m1 = cfg.CreateEntry<bool>("Super FCS (T-72M1)", false);

            lead_calculator_t72m = cfg.CreateEntry<bool>("Lead Calculator (T-72M)", true);
            lead_calculator_t72m.Comment = "For use with the standard sight; displays a number that corresponds to the horizontal markings on the sight";
            lead_calculator_t72m.Description = " ";
            lead_calculator_t72m1 = cfg.CreateEntry<bool>("Lead Calculator (T-72M1)", true);

            t72m_composite_cheeks = cfg.CreateEntry<bool>("Composite Cheeks (T-72M)", false);
            t72m_composite_cheeks.Comment = "Adds composite cheeks to the turret of T-72Ms (not required if using Kontakt-5 for T-72Ms)";
            t72m_composite_cheeks.Description = " ";

            t72m_super_composite_cheeks = cfg.CreateEntry<bool>("Super Composite Cheeks (T-72M)", false);
            t72m_super_composite_cheeks.Comment = "ultra thick";
            t72m1_super_composite_cheeks = cfg.CreateEntry<bool>("Super Composite Cheeks (T-72M1)", false);

            super_engine = cfg.CreateEntry<bool>("Super Engine/Transmission (T-72)", true);
            super_engine.Comment = "vrrrrrrrrrrooooooooom";
            super_engine.Description = " ";

            better_stab = cfg.CreateEntry<bool>("Better Stabilizer (T-72)", true);
            better_stab.Comment = "Less reticle blur, shake while on the move";
        }

        public class PreviouslyT72M : MonoBehaviour { }

        [HarmonyPatch(typeof(UnitSpawner), "SpawnUnit", new Type[] { typeof(GameObject), typeof(UnitMetaData), typeof(WaypointHolder), typeof(Transform)})]
        public static class OverrideT72M
        {
            private static void Prefix(out bool __state, UnitSpawner __instance, ref GameObject prefab)
            {
                __state = false;

                if (prefab.name == "T72M") {
                    __state = true;
                    prefab = __instance.GetPrefabByUniqueName("T72M1"); 
                }
            }

            private static void Postfix(bool __state, ref IUnit __result) {
                if (__state) {
                    __result.transform.gameObject.AddComponent<PreviouslyT72M>();
                }
            }
        }

        public class CustomGuidanceComputer : MonoBehaviour
        {
            public FireControlSystem fcs;
            public MissileGuidanceUnit mgu;
            public bool autotrackingEnabled = false; 
         
            void Update() {
                if (!autotrackingEnabled)
                {
                    Quaternion rot = Quaternion.LookRotation(fcs.AimWorldVector);
                    transform.rotation = rot;
                }
            }
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in PactIncreasedLethalityMod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (!vic.FriendlyName.Contains("T-72")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                vic.AimablePlatforms[1].transform.Find("optic cover parent").gameObject.SetActive(false);

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                UsableOptic night_optic = fcs.NightOptic;
                UsableOptic day_optic = Util.GetDayOptic(fcs);

                // SOVIET CREW 
                if ((soviet_t72m1.Value && vic.UniqueName == "T72M1") || (soviet_t72m.Value && vic.UniqueName == "T72M"))
                {
                    vic._friendlyName = vic.FriendlyName == "KPz T-72M1" ? "T-72A" : "T-72";

                    vic.transform.Find("DE Tank Voice").gameObject.SetActive(false);
                    GameObject crew_voice = GameObject.Instantiate(soviet_crew_voice, vic.transform);
                    crew_voice.transform.localPosition = new Vector3(0, 0, 0);
                    crew_voice.transform.localEulerAngles = new Vector3(0, 0, 0);
                    CrewVoiceHandler handler = crew_voice.GetComponent<CrewVoiceHandler>();
                    handler._chassis = vic._chassis as NwhChassis;
                    handler._reloadType = CrewVoiceHandler.ReloaderType.AutoLoaderAZ;
                    vic._crewVoiceHandler = handler;
                    crew_voice.SetActive(true);

                    vic.AimablePlatforms[1].transform.parent.Find("T72_markings").Find("roundels_72M1").gameObject.SetActive(false);
                    vic.AimablePlatforms[1].transform.parent.Find("T72_markings").Find("roundels_72M").gameObject.SetActive(false);
                }

                if (better_stab.Value)
                {
                    day_optic.slot.VibrationBlurScale = 0.05f;
                    day_optic.slot.VibrationShakeMultiplier = 0.1f;
                }

                string ammo_str = (vic.UniqueName == "T72M") ? t72m_ammo_type.Value : t72m1_ammo_type.Value;
                string heat_str = (vic.UniqueName == "T72M") ? t72m_heat_type.Value : t72m1_heat_type.Value;

                if (t72m_random_ammo.Value && vic.UniqueName == "T72M")
                {
                    int rand = UnityEngine.Random.Range(0, t72m_random_ammo_pool.Value.Count);
                    ammo_str = t72m_random_ammo_pool.Value.ElementAt(rand);
                }

                if (t72m1_random_ammo.Value && vic.UniqueName == "T72M1")
                {
                    int rand = UnityEngine.Random.Range(0, t72m1_random_ammo_pool.Value.Count);
                    ammo_str = t72m1_random_ammo_pool.Value.ElementAt(rand);
                }

                try
                {
                    AmmoClipCodexScriptable codex = AMMO_125mm.ap[ammo_str];
                    loadout_manager.LoadedAmmoTypes[0] = codex;

                    if (heat_str == "3BK18M")
                        loadout_manager.LoadedAmmoTypes[1] = AMMO_125mm.clip_codex_9m119;

                    //loadout_manager.LoadedAmmoTypes[2] = clip_codex_3of26_vt;
                    for (int i = 0; i <= 4; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        rack.ClipTypes[0] = codex.ClipType;
                        if (heat_str == "3BK18M")
                            rack.ClipTypes[1] = AMMO_125mm.clip_codex_9m119.ClipType;

                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();

                    if (only_carousel.Value)
                    {
                        var to_empty = vic.UniqueName == "T72M" ? empty_ammo_t72m.Value : empty_ammo_t72m1.Value;

                        foreach (string rack in to_empty) {
                            int idx = ammo_racks[rack];
                            Util.EmptyRack(loadout_manager.RackLoadouts[idx].Rack);
                        }
                    }
                }
                catch (Exception)
                {
                    MelonLogger.Msg("Loading default 3BM15 for " + vic.FriendlyName);
                }

                if ((tpn3_t72m1.Value && vic.UniqueName == "T72M1") || (tpn3_t72m.Value && vic.UniqueName == "T72M"))
                {
                    TPN3.Add(fcs, day_optic.slot.LinkedNightSight.PairedOptic, day_optic.slot.LinkedNightSight);
                }

                /*
                if ((thermals_t72m1.Value && vic.UniqueName == "T72M1") || (thermals_t72m.Value && vic.UniqueName == "T72M"))
                {
                    PactThermal.Add(weapon.FCS.NightOptic, thermals_quality.Value.ToLower(), (super_fcs_t72m1.Value && vic.UniqueName == "T72M1") || (super_fcs_t72m.Value && vic.UniqueName == "T72M"));
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);
                }
                */

                if ((super_fcs_t72m1.Value && vic.UniqueName == "T72M1") || (super_fcs_t72m.Value && vic.UniqueName == "T72M"))
                {
                    day_optic.transform.parent.localPosition = new Vector3(-0.727f, 0.4631f, -5.9849f);
                    night_optic.transform.localPosition = new Vector3(-0.727f, 0.4631f, -5.9849f);

                    Sosna.Add(day_optic, night_optic, vic.WeaponsManager.Weapons[1], true);
                }
           
                weapon.Feed.ReloadDuringMissileTracking = true;
                weapon.FireWhileGuidingMissile = false;
                GameObject guidance_computer_obj = GameObject.Instantiate(new GameObject("guidance computer"), fcs.transform.parent);
                guidance_computer_obj.transform.localPosition = fcs.transform.localPosition + new Vector3(0, 0, 5f);
                MissileGuidanceUnit computer = guidance_computer_obj.AddComponent<MissileGuidanceUnit>();
                computer.AimElement = guidance_computer_obj.transform;
                weapon.GuidanceUnit = computer;

                CustomGuidanceComputer gc = guidance_computer_obj.AddComponent<CustomGuidanceComputer>();
                gc.fcs = fcs;
                gc.mgu = computer;

                LockOnLead s = fcs.gameObject.AddComponent<LockOnLead>();
                s.fcs = fcs;
                s.guidance_computer = gc;

                if (super_engine.Value)
                {
                    VehicleController this_vic_controller = vic_go.GetComponent<VehicleController>();
                    NwhChassis chassis = vic_go.GetComponent<NwhChassis>();

                    Util.ShallowCopy(this_vic_controller.engine, abrams_vic_controller.engine);
                    Util.ShallowCopy(this_vic_controller.transmission, abrams_vic_controller.transmission);

                    this_vic_controller.engine.vc = vic_go.GetComponent<VehicleController>();
                    this_vic_controller.transmission.vc = vic_go.GetComponent<VehicleController>();
                    this_vic_controller.engine.Initialize(this_vic_controller);
                    this_vic_controller.engine.Start();
                    this_vic_controller.transmission.Initialize(this_vic_controller);

                    chassis._maxForwardSpeed = 25f;
                    chassis._maxReverseSpeed = 7.176f;
                    chassis._originalEnginePower = 1500.99f;
                }

                if (
                    (!super_fcs_t72m.Value && lead_calculator_t72m.Value && vic.UniqueName == "T72M") || 
                    (!super_fcs_t72m1.Value && lead_calculator_t72m1.Value && vic.UniqueName == "T72M1")
                ){
                    FireControlSystem1A40.Add(fcs, day_optic, new Vector3(-308.8629f, -6.6525f, 0f));
                }


                if (vic.UniqueName == "T72M1")
                {
                    Transform turret = vic.transform.Find("---MESH---/HULL/TURRET");
                    Transform turret_rend = turret.Find("T72M1_turret");

                    turret.Find("smoke rack").localScale = Vector3.zero;

                    turret_rend.GetComponent<MeshFilter>().sharedMesh = t72b3ubh_turret_mesh;

                    Transform turret_armour_transform = turret.GetComponent<LateFollowTarget>()._lateFollowers[0].transform.Find("ARMOR/Turret.002");

                    VariableArmor turret_armour = turret_armour_transform.GetComponent<VariableArmor>();
                    GameObject.DestroyImmediate(turret_armour_transform.GetChild(0).gameObject);
                    turret_armour_transform.GetComponent<MeshCollider>().sharedMesh = t72b_armour_turret;
                    turret_armour_transform.GetComponent<MeshFilter>().sharedMesh = t72b_armour_turret;
                    turret_armour.cloneMesh();
                    turret_armour.invertMesh();
                    turret_armour.setupCollider();
                    turret_armour._debug = true;

                    GameObject.DestroyImmediate(turret_armour_transform.parent.Find("Composite Armor Array").gameObject);

                    GameObject nera = GameObject.Instantiate(t72b_turret_nera, turret_armour_transform);
                    nera.transform.Find("REFLECTIVE PLATES").GetComponent<VariableArmor>().Unit = vic;
                    nera.transform.Find("BACKING PLATE").GetComponent<VariableArmor>().Unit = vic;
                    nera.transform.parent = turret_armour_transform.parent;
                    nera.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    nera.transform.localEulerAngles = new Vector3(0f, 90f, 0f);

                    GameObject k1 = GameObject.Instantiate(t72b_k5_1989_full, vic.transform.Find("T72M1_mesh (1)/T72M1_hull"));
                    k1.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                    Transform hull_k1 = k1.transform.Find("HULL ERA");
                    Transform turret_k1 = k1.transform.Find("TURRET ERA");
                    //Transform mantlet_k1 = k1.transform.Find("MANTLET MOUNT");

                    Material t72_material = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret").GetComponent<MeshRenderer>().materials[0];

                    for (int i = 1; i < k1.transform.Find("SMOKE LAUNCHER").childCount; i++)
                    {
                        Transform smoke = k1.transform.Find("SMOKE LAUNCHER").GetChild(i);
                        Material[] smokes_mat = smoke.GetComponent<MeshRenderer>().materials;
                        smokes_mat[0] = t72_material;
                        smoke.GetComponent<MeshRenderer>().materials = smokes_mat;
                    }

                    turret_k1.parent = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret");
                    //mantlet_k1.parent = vic.transform.Find("---MESH---/HULL/TURRET/GUN");
                    k1.transform.Find("SMOKE LAUNCHER").parent = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret");

                    LateFollow k1_hull_follow = hull_k1.Find("ARMOUR").gameObject.AddComponent<LateFollow>();
                    k1_hull_follow.FollowTarget = vic.transform;
                    k1_hull_follow.enabled = true;
                    k1_hull_follow.Awake();
                    hull_k1.Find("ARMOUR").parent = null;
                    k1.transform.Find("HULL PLATE").parent = k1_hull_follow.transform;
                    k1.transform.Find("HULL ERA FRONT HULL/ARMOUR").parent = k1_hull_follow.transform;

                    LateFollow k1_turret_follow = turret_k1.Find("ARMOUR").gameObject.AddComponent<LateFollow>();
                    k1_turret_follow.FollowTarget = vic.transform.Find("---MESH---/HULL/TURRET");
                    k1_turret_follow.enabled = true;
                    k1_turret_follow.Awake();
                    turret_k1.Find("ARMOUR").parent = null;
                    k1.transform.Find("TURRET PLATE").parent = k1_turret_follow.transform;

                    if (super_fcs_t72m1.Value)
                    {
                        GameObject b3_k5 = GameObject.Instantiate(t72b_k5_b3_full, vic.transform.Find("T72M1_mesh (1)/T72M1_hull"));
                        b3_k5.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                        Transform b3_k5_turret = b3_k5.transform.Find("TURRET ERA");
                        b3_k5_turret.parent = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret");
                        b3_k5_turret.transform.Find("ARMOUR").parent = k1_turret_follow.transform;
                        b3_k5.transform.Find("SOSNA U").parent = k1_turret_follow.transform;

                        turret.Find("LUNA").localScale = new Vector3(0f, 0f, 0f);
                        turret.Find("night sight cover").localScale = new Vector3(0f, 0f, 0f);
                        /*
                        GameObject ubh_kit = GameObject.Instantiate(t72b3m_ubh_kit, vic.transform.Find("T72M1_mesh (1)/T72M1_hull"));
                        ubh_kit.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                        Transform ubh_turret = ubh_kit.transform.Find("TURRET STUFF");
                        Transform ubh_hull = ubh_kit.transform.Find("HULL STUFF");

                        ubh_turret.parent = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret");

                        hull_k1.gameObject.SetActive(false);
                        */
                    }

                    /*
                    LateFollow k1_mantlet_follow = mantlet_k1.Find("MANTLET K1 ARMOUR").gameObject.AddComponent<LateFollow>();
                    k1_mantlet_follow.FollowTarget = vic.transform.Find("---MESH---/HULL/TURRET/GUN");
                    k1_mantlet_follow.enabled = true;
                    k1_mantlet_follow.Awake();
                    mantlet_k1.Find("MANTLET K1 ARMOUR").parent = null;
                    */

                    Transform hull_rend = vic.transform.Find("T72M1_mesh (1)/T72M1_hull");
                    hull_rend.GetComponent<MeshFilter>().sharedMesh = t72b_k5_hull_mesh;
                    vic.transform.Find("---MESH---/equipment").gameObject.SetActive(false);
                    vic.AimablePlatforms[1].transform.parent.Find("T72_markings").Find("roundels_72M1").gameObject.SetActive(false);
                    vic._friendlyName = "T-72B3";

                    continue;
                }
            }

            yield break;
        }

        public static void Init()
        {
            if (!t72_patch.Value) return;

            if (abrams_vic_controller == null)
            {
                foreach (Vehicle obj in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
                {
                    if (obj.gameObject.name == "M1IP")
                    {
                        abrams_vic_controller = obj.GetComponent<VehicleController>();
                        break;
                    }
                }
            }

            foreach (AmmoClipCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoClipCodexScriptable)))
            {
                if (s.name == "clip_3BK18M") { clip_codex_3bk18m = s; break; }
            }

            if (soviet_crew_voice == null)
            {
                foreach (CrewVoiceHandler obj in Resources.FindObjectsOfTypeAll(typeof(CrewVoiceHandler)))
                {
                    if (obj.name != "RU Tank Voice") continue;
                    soviet_crew_voice = obj.gameObject;
                    break;
                }
            }

            if (turret_cleaned_mesh == null)
            {
                
                AssetBundle t72b_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t72b"));
                AssetBundle t72av_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t72av"));

                t72b_k5_plate_destroyed = t72b_bundle.LoadAsset<Material>("DESTROYED K5 PLATE");
                t72b_k5_plate_destroyed.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b_vis_turret = t72b_bundle.LoadAsset<Mesh>("t72b_tur.asset");
                t72b_vis_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b_k5_hull_mesh = t72b_bundle.LoadAsset<Mesh>("t72b_k5_hull_mesh");
                t72b_k5_hull_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b3_turret_mesh = t72b_bundle.LoadAsset<Mesh>("t72b3 turret");
                t72b3_turret_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b3ubh_turret_mesh = t72b_bundle.LoadAsset<Mesh>("t72b3m_turret_mesh");
                t72b3ubh_turret_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b_armour_turret = t72b_bundle.LoadAsset<Mesh>("t72b_tur_collider.asset");
                t72b_armour_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b_turret_nera = t72b_bundle.LoadAsset<GameObject>("t72n_nera_cheeks.prefab");
                t72b_turret_nera.hideFlags = HideFlags.DontUnloadUnusedAsset;
  
                GameObject reflective_plates = t72b_turret_nera.transform.Find("REFLECTIVE PLATES").gameObject;
                GameObject backing_plate = t72b_turret_nera.transform.Find("BACKING PLATE").gameObject;

                reflective_plates.tag = "Penetrable";
                reflective_plates.layer = 8;
                VariableArmor t72b_turret_nera_va = reflective_plates.AddComponent<VariableArmor>();
                t72b_turret_nera_va.SetName("reflective plate array");
                t72b_turret_nera_va._armorType = Armour.composite_armor;
                t72b_turret_nera_va._spallForwardRatio = 0.2f;
                AarVisual t72b_turret_nera_aar = reflective_plates.AddComponent<AarVisual>();
                t72b_turret_nera_aar.SwitchMaterials = false;
                t72b_turret_nera_aar.HideUntilAar = true;

                backing_plate.tag = "Penetrable";
                backing_plate.layer = 8;
                VariableArmor backing_plate_va = backing_plate.AddComponent<VariableArmor>();
                backing_plate_va.SetName("steel backing plate");
                backing_plate_va._armorType = Armour.ru_hhs_armor;
                backing_plate_va._spallForwardRatio = 0.5f;
                AarVisual backing_plate_aar = backing_plate.AddComponent<AarVisual>();
                backing_plate_aar.SwitchMaterials = false;
                backing_plate_aar.HideUntilAar = true;

                t72b_k1_full = t72b_bundle.LoadAsset<GameObject>("T72B_K1");
                t72b_k1_full.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b_hull = t72b_bundle.LoadAsset<Mesh>("t72b_hull");
                t72b_hull.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72av_k1_full = t72av_bundle.LoadAsset<GameObject>("T72AV");
                t72av_k1_full.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72av_turret = t72av_bundle.LoadAsset<Mesh>("t72av_turret");
                t72av_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b_k5_1989_full = t72b_bundle.LoadAsset<GameObject>("T72B 1989 K5");
                t72b_k5_1989_full.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b_k5_b3_full = t72b_bundle.LoadAsset<GameObject>("T72B3 K5");
                t72b_k5_b3_full.hideFlags = HideFlags.DontUnloadUnusedAsset;

                t72b3m_ubh_kit = t72b_bundle.LoadAsset<GameObject>("T72B UBH KIT");
                t72b3m_ubh_kit.hideFlags = HideFlags.DontUnloadUnusedAsset;

                Util.SetupFLIRShaders(t72b_k1_full);
                Util.SetupFLIRShaders(t72av_k1_full);
                Util.SetupFLIRShaders(t72b_k5_1989_full);
                Util.SetupFLIRShaders(t72b_k5_b3_full);
                Util.SetupFLIRShaders(t72b3m_ubh_kit);

                Transform hull_k1 = t72b_k1_full.transform.Find("HULL K1/HULL K1 ARMOUR");
                Transform turret_k1 = t72b_k1_full.transform.Find("K1 TURRET/TURRET K1 ARMOUR");
                Transform mantlet_k1 = t72b_k1_full.transform.Find("MANTLET MOUNT/MANTLET K1 ARMOUR");
                Kontakt1.Setup(hull_k1, hull_k1.parent);
                Kontakt1.Setup(turret_k1, turret_k1.parent);
                Kontakt1.Setup(mantlet_k1, mantlet_k1.parent.Find("K1 MANTLET"));

                hull_k1 = t72av_k1_full.transform.Find("HULL K1/HULL K1 ARMOUR");
                turret_k1 = t72av_k1_full.transform.Find("K1 TURRET/TURRET K1 ARMOUR");
                mantlet_k1 = t72av_k1_full.transform.Find("MANTLET MOUNT/MANTLET K1 ARMOUR");
                Kontakt1.Setup(hull_k1, hull_k1.parent);
                Kontakt1.Setup(turret_k1, turret_k1.parent);
                Kontakt1.Setup(mantlet_k1, mantlet_k1.parent.Find("K1 MANTLET"));

                GameObject turret_plate = t72b_k5_1989_full.transform.Find("TURRET PLATE/ARMOUR").gameObject;
                turret_plate.tag = "Penetrable";
                turret_plate.layer = 8;
                UniformArmor turret_plate_armor = turret_plate.AddComponent<UniformArmor>();
                turret_plate_armor._name = "plate";
                turret_plate_armor.PrimaryHeatRha = 5f;
                turret_plate_armor.PrimarySabotRha = 5f;

                GameObject hull_plate = t72b_k5_1989_full.transform.Find("HULL PLATE/ARMOUR").gameObject;
                hull_plate.tag = "Penetrable";
                hull_plate.layer = 8;
                UniformArmor hull_plate_armor = hull_plate.AddComponent<UniformArmor>();
                hull_plate_armor._name = "mounting plate";
                hull_plate_armor.PrimaryHeatRha = 10f;
                hull_plate_armor.PrimarySabotRha = 10f;

                GameObject splash_guard = t72b_k5_1989_full.transform.Find("HULL PLATE/SPLASH GUARD ARMOUR").gameObject;
                splash_guard.tag = "Penetrable";
                splash_guard.layer = 8;
                UniformArmor splash_guard_armor = hull_plate.AddComponent<UniformArmor>();
                splash_guard_armor._name = "splash guard";
                splash_guard_armor.PrimaryHeatRha = 2f;
                splash_guard_armor.PrimarySabotRha = 2f;

                Transform hull_k5 = t72b_k5_1989_full.transform.Find("HULL ERA/ARMOUR");
                Transform hull_front_k5 = t72b_k5_1989_full.transform.Find("HULL ERA FRONT HULL/ARMOUR");
                Transform turret_k5 = t72b_k5_1989_full.transform.Find("TURRET ERA/ARMOUR");
                Kontakt5.Setup(hull_k5, hull_k5.parent);
                Kontakt5.Setup(hull_front_k5, hull_front_k5.parent, hide_on_detonate: false, destroyed_mat: t72b_k5_plate_destroyed);
                Kontakt5.Setup(turret_k5, turret_k5.parent);

                Transform b3_turret_k5 = t72b_k5_b3_full.transform.Find("TURRET ERA/ARMOUR");
                Kontakt5.Setup(b3_turret_k5, b3_turret_k5.parent);

                GameObject sosna_sight_complex = t72b_k5_b3_full.transform.Find("SOSNA U/SIGHT COMPLEX").gameObject;
                sosna_sight_complex.tag = "Penetrable";
                sosna_sight_complex.layer = 8;
                UniformArmor sosna_sight_complex_armor = sosna_sight_complex.AddComponent<UniformArmor>();
                sosna_sight_complex_armor._name = "Sosna-U sight complex";
                sosna_sight_complex_armor.PrimaryHeatRha = 30f;
                sosna_sight_complex_armor.PrimarySabotRha = 30f;

                GameObject sosna_cover = t72b_k5_b3_full.transform.Find("SOSNA U/FRONT COVER").gameObject;
                sosna_cover.tag = "Penetrable";
                sosna_cover.layer = 8;
                UniformArmor sosna_cover_armor = sosna_cover.AddComponent<UniformArmor>();
                sosna_cover_armor._name = "front cover";
                sosna_cover_armor.PrimaryHeatRha = 10f;
                sosna_cover_armor.PrimarySabotRha = 10f;

                GameObject sosna_shield = t72b_k5_b3_full.transform.Find("SOSNA U/SHIELD").gameObject;
                sosna_shield.tag = "Penetrable";
                sosna_shield.layer = 8;
                UniformArmor sosna_shield_armor = sosna_shield.AddComponent<UniformArmor>();
                sosna_shield_armor._name = "shield";
                sosna_shield_armor.PrimaryHeatRha = 15f;
                sosna_shield_armor.PrimarySabotRha = 15f;

                var t72_cleaned_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t72_turret_cleaned"));
                turret_cleaned_mesh = t72_cleaned_bundle.LoadAsset<Mesh>("t72m1turret_front_cleaned.asset");
                turret_cleaned_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;

                var t72b3_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t72b3_turret_cleaned"));
                b3_turret_cleaned_mesh = t72b3_bundle.LoadAsset<Mesh>("t72b3_turret.asset");
                b3_turret_cleaned_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;

                var t72_cleaned_hull_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t72_hull_cleaned"));
                hull_cleaned_mesh = t72_cleaned_hull_bundle.LoadAsset<Mesh>("T72M1_hull.asset");
                hull_cleaned_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}