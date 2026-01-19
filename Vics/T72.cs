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
using NWH.VehiclePhysics;
using HarmonyLib;
using GHPC.Equipment;
using GHPC.Mission;
using GHPC.AI;
using GHPC.Mission.Data;

namespace PactIncreasedLethality
{
    public class T72
    {
        static MelonPreferences_Entry<bool> t72_patch;
        static MelonPreferences_Entry<string> t72m_ammo_type;
        static MelonPreferences_Entry<string> t72m1_ammo_type;

        static MelonPreferences_Entry<string> t72m_heat_type;
        static MelonPreferences_Entry<string> t72m1_heat_type;

        static MelonPreferences_Entry<string> t72m_atgm_type;
        static MelonPreferences_Entry<string> t72m1_atgm_type;

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

        static MelonPreferences_Entry<bool> ubh_t72m1;
        static MelonPreferences_Entry<bool> ubh_t72m;

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

        static Dictionary<string, int> ammo_racks = new Dictionary<string, int>() {
            ["Hull Wet"] = 1,
            ["Hull Rear"] = 2,
            ["Hull Front"] = 3,
            ["Turret Spare"] = 4
        };

        static Mesh t72b_vis_turret;
        static Mesh t72b_armour_turret;
        static GameObject t72b_turret_nera;
        static GameObject t72b_k1_full;
        static GameObject t72b_only_smoke; 
        static Mesh t72b_hull;

        static GameObject sosna_u;

        static GameObject t72av_k1_full;
        static Mesh t72av_turret;
        static Mesh t72av_sosna_turret;

        static GameObject t72b_k5_1989_full;
        static GameObject t72b_k5_b3_full;
        static GameObject t72b3m_ubh_kit;
        static Material t72b_k5_plate_destroyed;

        static Mesh t72b3_turret_mesh;
        static Mesh t72b_k5_hull_mesh;
        static Mesh t72b3ubh_turret_mesh;

        private static bool assets_loaded = false;

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
                "3BM46",
                "3BM60"
            };

            t72_patch = cfg.CreateEntry<bool>("T-72 Patch", true);
            t72_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";

            t72m_ammo_type = cfg.CreateEntry<string>("AP Round (T-72M)", "3BM32");
            t72m_ammo_type.Comment = "3BM15, 3BM22, 3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized), 3BM46, 3BM60";

            t72m1_ammo_type = cfg.CreateEntry<string>("AP Round (T-72M1)", "3BM32");

            t72m_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-72M)", false);
            t72m_random_ammo_pool = cfg.CreateEntry<List<string>>("Random AP Round Pool (T-72M)", random_ammo_pool);
            t72m_random_ammo_pool.Comment = "3BM22, 3BM26, 3BM32, 3BM42, 3BM46, 3BM60";

            t72m1_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-72M1)", false);
            t72m1_random_ammo_pool = cfg.CreateEntry<List<string>>("Random AP Round Pool (T-72M1)", random_ammo_pool);

            t72m_heat_type = cfg.CreateEntry<string>("HEAT Round (T-72M)", "3BK18M");
            t72m_heat_type.Comment = "3BK14M, 3BK18M";
            t72m1_heat_type = cfg.CreateEntry<string>("HEAT Round (T-72M1)", "3BK18M");

            t72m_atgm_type = cfg.CreateEntry<string>("GLATGM (T-72M)", "None");
            t72m_atgm_type.Comment = "None, 9M119, 9M119M1";
            t72m1_atgm_type = cfg.CreateEntry<string>("GLATGM (T-72M1)", "None");

            tpn3_t72m = cfg.CreateEntry<bool>("TPN-3 Night Sight (T-72)", false);
            tpn3_t72m.Description = " ";
            tpn3_t72m.Comment = "Replaces the night sight with the one found on the T-80B/T-64B";
            tpn3_t72m1 = cfg.CreateEntry<bool>("TPN-3 Night Sight (T-72M1)", true);

            thermals_t72m = cfg.CreateEntry<bool>("Has Thermals (T-72M)", false);
            thermals_t72m.Comment = "Replaces night vision sight with thermal sight";
            thermals_t72m.Description = " ";
            thermals_t72m1 = cfg.CreateEntry<bool>("Has Thermals (T-72M1)", false);

            thermals_quality = cfg.CreateEntry<string>("Thermals Quality (T-72)", "Low");
            thermals_quality.Comment = "Low, High";

            k5_t72m1 = cfg.CreateEntry<bool>("Kontakt-5 ERA (T-72M1)", false);
            k5_t72m1.Comment = "    B           I           G           brick";
            k5_t72m1.Description = " ";
            k5_t72m = cfg.CreateEntry<bool>("Kontakt-5 ERA (T-72M)", false);

            era_t72m1 = cfg.CreateEntry<bool>("Kontakt-1 ERA (T-72M1)", true);
            era_t72m1.Comment = "BRICK ME UP LADS";

            era_t72m = cfg.CreateEntry<bool>("Kontakt-1 ERA (T-72M)", true);
            era_t72m.Comment = "BRICK ME UP LADS";

            ubh_t72m1 = cfg.CreateEntry<bool>("UBH Package (T-72M1)", false);
            ubh_t72m1.Comment = "Adds slat armour to the turret rear and hull sides, Relikt ERA to the turret and hull sides (T-72B3 Exclusive)";
            ubh_t72m = cfg.CreateEntry<bool>("UBH Package (T-72M)", false);

            only_carousel = cfg.CreateEntry<bool>("Reduced Ammo Load", false);
            only_carousel.Comment = "Allows you to specify which ammo racks should be emptied (except carousel)";
            only_carousel.Description = " ";

            empty_ammo_t72m = cfg.CreateEntry<List<string>>("Empty Ammo Racks (T-72M)", racks);
            empty_ammo_t72m1 = cfg.CreateEntry<List<string>>("Empty Ammo Racks (T-72M1)", racks);
            empty_ammo_t72m.Comment = "Hull Wet, Hull Rear, Hull Front, Turret Spare";

            soviet_t72m = cfg.CreateEntry<bool>("Soviet Crew (T-72M)", false);
            soviet_t72m.Comment = "Also renames the tank to T-72 and removes NVA decals";
            soviet_t72m.Description = " ";

            soviet_t72m1 = cfg.CreateEntry<bool>("Soviet Crew (T-72M1)", true);
            soviet_t72m1.Comment = "Also renames the tank to T-72A and removes NVA decals";

            super_fcs_t72m = cfg.CreateEntry<bool>("Super FCS (T-72M)", false);
            super_fcs_t72m.Comment = "Sosna-U: point-n-shoot, thermal sight, autotracking";
            super_fcs_t72m.Description = " ";
            super_fcs_t72m1 = cfg.CreateEntry<bool>("Super FCS (T-72M1)", false);

            lead_calculator_t72m = cfg.CreateEntry<bool>("Lead Calculator (T-72M)", false);
            lead_calculator_t72m.Comment = "For use with the standard sight; displays a number that corresponds to the horizontal markings on the sight";
            lead_calculator_t72m.Description = " ";
            lead_calculator_t72m1 = cfg.CreateEntry<bool>("Lead Calculator (T-72M1)", true);

            t72m_composite_cheeks = cfg.CreateEntry<bool>("Composite Cheeks (T-72M)", false);
            t72m_composite_cheeks.Comment = "Adds kvartz composite cheeks to the turret of T-72Ms";
            t72m_composite_cheeks.Description = " ";

            t72m_super_composite_cheeks = cfg.CreateEntry<bool>("Super Composite Cheeks (T-72M)", false);
            t72m_super_composite_cheeks.Comment = "ultra thick";
            t72m1_super_composite_cheeks = cfg.CreateEntry<bool>("Super Composite Cheeks (T-72M1)", true);

            super_engine = cfg.CreateEntry<bool>("Super Engine/Transmission (T-72)", false);
            super_engine.Comment = "vrrrrrrrrrrooooooooom";
            super_engine.Description = " ";

            better_stab = cfg.CreateEntry<bool>("Better Stabilizer (T-72)", true);
            better_stab.Comment = "Less reticle blur, shake while on the move";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (vic.UniqueName != "T72M" && vic.UniqueName != "T72M1") continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                vic.AimablePlatforms[1].transform.Find("optic cover parent").gameObject.SetActive(false);
                vic.AimablePlatforms[1].transform.Find("shutter parent").GetChild(0).gameObject.SetActive(false);

                bool was_t72m = vic.GetComponent<PreviouslyT72M>() != null;
                bool is_t72m = vic.UniqueName == "T72M" || was_t72m;
                bool is_t72m1 = vic.UniqueName == "T72M1" && !was_t72m;
                bool has_k5 = (k5_t72m1.Value && is_t72m1) || (k5_t72m.Value && is_t72m);
                bool has_k1 = (era_t72m1.Value && is_t72m1) || (era_t72m.Value && is_t72m);
                has_k1 = has_k1 && !has_k5;
                bool has_reflective_plates = (t72m1_super_composite_cheeks.Value && is_t72m1) || (t72m_super_composite_cheeks.Value && is_t72m);
                bool has_sosna = (super_fcs_t72m1.Value && is_t72m1) || (super_fcs_t72m.Value && is_t72m);
                bool is_soviet = (soviet_t72m1.Value && is_t72m1) || (soviet_t72m.Value && is_t72m);
                bool has_ubh = (ubh_t72m1.Value && is_t72m1) || (ubh_t72m.Value && is_t72m);
                bool only_smoke = (has_reflective_plates && !has_k1 && !has_k5);

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                UsableOptic night_optic = fcs.NightOptic;
                UsableOptic day_optic = Util.GetDayOptic(fcs);

                day_optic.reticleMesh.smoothTime = 0.1f;
                day_optic.reticleMesh.maxSpeed = 1000f;
                day_optic.reticleMesh.rotaryCoef = -0.0015f;

                // SOVIET CREW 
                if (is_soviet)
                {
                    vic._friendlyName = vic.FriendlyName == "KPz T-72M1" ? "T-72A" : "T-72";

                    vic.transform.Find("DE Tank Voice").gameObject.SetActive(false);
                    GameObject crew_voice = GameObject.Instantiate(Assets.soviet_crew_voice, vic.transform);
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

                string ammo_str = (is_t72m) ? t72m_ammo_type.Value : t72m1_ammo_type.Value;
                string heat_str = (is_t72m) ? t72m_heat_type.Value : t72m1_heat_type.Value;
                string atgm_str = (is_t72m) ? t72m_atgm_type.Value : t72m1_atgm_type.Value;

                if (t72m_random_ammo.Value && is_t72m)
                {
                    int rand = UnityEngine.Random.Range(0, t72m_random_ammo_pool.Value.Count);
                    ammo_str = t72m_random_ammo_pool.Value.ElementAt(rand);
                }

                if (t72m1_random_ammo.Value && is_t72m1)
                {
                    int rand = UnityEngine.Random.Range(0, t72m1_random_ammo_pool.Value.Count);
                    ammo_str = t72m1_random_ammo_pool.Value.ElementAt(rand);
                }

                try
                {
                    if (ammo_str != "3BM15")
                        loadout_manager.LoadedAmmoList.AmmoClips[0] = Ammo_125mm.ap[ammo_str];

                    if (heat_str == "3BK18M")
                        loadout_manager.LoadedAmmoList.AmmoClips[1] = Assets.clip_codex_3bk18m;

                    if (atgm_str != "None")
                        loadout_manager.LoadedAmmoList.AmmoClips = Util.AppendToArray(loadout_manager.LoadedAmmoList.AmmoClips, Ammo_125mm.atgm[atgm_str]);

                    for (int i = 0; i <= 4; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;

                        if (atgm_str != "None" && (i == 0 || i == 2))
                        {
                            loadout_manager.RackLoadouts[i].FixedChoices = new LoadoutManager.RackLoadoutFixedChoice[] {
                                new LoadoutManager.RackLoadoutFixedChoice() {
                                    AmmoClipIndex = 3,
                                    RackSlotIndex = i == 0 ? 21 : 10,
                                },
                                new LoadoutManager.RackLoadoutFixedChoice() {
                                    AmmoClipIndex = 3,
                                    RackSlotIndex = i == 0 ? 20 : 9,
                                }
                            };
                        }

                        Util.EmptyRack(rack);
                    }

                    if (atgm_str != "None")
                    {
                        loadout_manager._totalAmmoTypes = 4;
                        loadout_manager.TotalAmmoCounts = new int[] { 28, 8, 4, 4 };
                    }

                    weapon.Feed.AmmoTypeInBreech = null;
                    loadout_manager.SpawnCurrentLoadout();
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
                    MelonLogger.Msg("Loading default ammo for " + vic.FriendlyName);
                }

                if (
                    ((tpn3_t72m1.Value && is_t72m1) || (tpn3_t72m.Value && is_t72m))
                    && !has_sosna
                ) {
                    TPN3.Add(fcs, day_optic.slot.LinkedNightSight.PairedOptic, day_optic.slot.LinkedNightSight);
                }
               
                if (
                    ((thermals_t72m1.Value && is_t72m1) || (thermals_t72m.Value && is_t72m)) 
                    && !has_sosna
                ) {
                    PactThermal.Add(weapon.FCS.NightOptic, thermals_quality.Value.ToLower());
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);

                    LaserPointCorrection lpc = fcs.transform.parent.gameObject.AddComponent<LaserPointCorrection>();
                    lpc.night_optic = weapon.FCS.NightOptic;
                    lpc.day_optic = day_optic;
                    lpc.laser = weapon.FCS.LaserOrigin;
                }

                if (has_sosna)
                {
                    day_optic.transform.parent.localPosition = new Vector3(-0.727f, 0.4631f, -5.7249f);
                    night_optic.transform.localPosition = new Vector3(-0.727f, 0.4631f, -5.7249f);
                }

                GameObject guidance_computer_obj = GameObject.Instantiate(new GameObject("guidance computer"), fcs.transform.parent);
                guidance_computer_obj.transform.localPosition = fcs.transform.localPosition + new Vector3(0, 0, 5f);
                guidance_computer_obj.transform.SetParent(day_optic.transform.parent, true);
                MissileGuidanceUnit computer = guidance_computer_obj.AddComponent<MissileGuidanceUnit>();
                computer.AimElement = guidance_computer_obj.transform;
                weapon.GuidanceUnit = computer;

                weapon.Feed.ReloadDuringMissileTracking = false;
                weapon.Feed._missileGuidance = computer;
                weapon.FireWhileGuidingMissile = false;

                CustomGuidanceComputer gc = fcs.transform.parent.gameObject.AddComponent<CustomGuidanceComputer>();
                gc.fcs = fcs;
                gc.mgu = computer;

                if (has_sosna)
                {
                    SuperFCS.Add(day_optic, night_optic, vic.WeaponsManager.Weapons[1], vic.WeaponsManager.Weapons[0], gc);

                    day_optic.transform.Find("Quad").gameObject.SetActive(false);
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);

                    string path = is_t72m1 || was_t72m ? "---MESH---/HULL/TURRET" : "T72M_skirts_rig/HULL/TURRET";
                    Transform turret = vic.transform.Find(path);
                    Transform late_follow = turret.GetComponent<LateFollowTarget>()._lateFollowers[0].transform;
                    late_follow.Find("ARMOR/Night Sight Cover").gameObject.SetActive(false);
                    late_follow.Find("ARMOR/Night Sight Glass").gameObject.SetActive(false);
                    late_follow.Find("ARMOR/NightSight Housing").gameObject.SetActive(false);
                }
                else {
                    BOM.Add(day_optic.transform);
                }

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

                    chassis._maxForwardSpeed = 22f;
                    chassis._maxReverseSpeed = 2.0f;
                    chassis._originalEnginePower = 1400.99f;
                }

                if (
                    (!super_fcs_t72m.Value && lead_calculator_t72m.Value && is_t72m) || 
                    (!super_fcs_t72m1.Value && lead_calculator_t72m1.Value && is_t72m1)
                ){
                    FireControlSystem1A40.Add(fcs, day_optic, new Vector3(-308.8629f, -6.6525f, 0f));
                }

                if (vic.UniqueName == "T72M1")
                {
                    GameObject kontakt_prefab = null;
                    Mesh turret_mesh = null;

                    if (has_k1 && !has_reflective_plates)
                    {
                        kontakt_prefab = t72av_k1_full;

                        turret_mesh = has_sosna? t72av_sosna_turret : t72av_turret;
                    }

                    if (has_reflective_plates || has_k5) {
                        if (has_k1) kontakt_prefab = t72b_k1_full;

                        if (has_k5) kontakt_prefab = t72b_k5_1989_full;

                        if (has_sosna) { turret_mesh = has_ubh ?  t72b3ubh_turret_mesh : t72b3_turret_mesh; } else { turret_mesh = t72b_vis_turret; };
                        
                        if (!has_k5 && !has_k1) kontakt_prefab = t72b_only_smoke;
                    }

                    Transform turret = vic.transform.Find("---MESH---/HULL/TURRET");
                    Transform turret_rend = turret.Find("T72M1_turret");

                    if (turret_mesh != null)
                    {
                        turret_rend.GetComponent<MeshFilter>().sharedMesh = turret_mesh;
                        turret.Find("smoke rack").localScale = Vector3.zero;
                        vic.transform.Find("---MESH---/equipment").gameObject.SetActive(false);
                        vic.AimablePlatforms[1].transform.parent.Find("T72_markings").Find("roundels_72M1").gameObject.SetActive(false);
                    }

                    if (has_reflective_plates || has_k5)
                    {
                        Transform turret_armour_transform = turret.GetComponent<LateFollowTarget>()._lateFollowers[0].transform.Find("ARMOR/Turret.002");

                        VariableArmor turret_armour = turret_armour_transform.GetComponent<VariableArmor>();
                        GameObject.DestroyImmediate(turret_armour_transform.GetChild(0).gameObject);
                        turret_armour_transform.GetComponent<MeshCollider>().sharedMesh = t72b_armour_turret;
                        turret_armour_transform.GetComponent<MeshFilter>().sharedMesh = t72b_armour_turret;
                        turret_armour.cloneMesh();
                        turret_armour.invertMesh();
                        turret_armour.setupCollider();

                        GameObject.DestroyImmediate(turret_armour_transform.parent.Find("Composite Armor Array").gameObject);

                        GameObject nera = GameObject.Instantiate(t72b_turret_nera, turret_armour_transform);
                        nera.transform.Find("REFLECTIVE PLATES").GetComponent<VariableArmor>().Unit = vic;
                        nera.transform.Find("BACKING PLATE").GetComponent<VariableArmor>().Unit = vic;
                        nera.transform.parent = turret_armour_transform.parent;
                        nera.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        nera.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                    }

                    if (kontakt_prefab != null)
                    {
                        GameObject kontakt = GameObject.Instantiate(kontakt_prefab, vic.transform.Find("T72M1_mesh (1)/T72M1_hull"));
                        kontakt.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                        Material t72_material = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret").GetComponent<MeshRenderer>().materials[0];
                        Transform smoke_launcher = kontakt.transform.Find("SMOKE LAUNCHER");

                        for (int i = 1; i < smoke_launcher.childCount; i++)
                        {
                            Transform smoke = smoke_launcher.GetChild(i);
                            Material[] smokes_mat = smoke.GetComponent<MeshRenderer>().materials;
                            smokes_mat[0] = t72_material;
                            smoke.GetComponent<MeshRenderer>().materials = smokes_mat;
                        }
                        smoke_launcher.parent = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret");

                        VehicleSmokeManager smoke_manager = vic.transform.Find("T72M1 -Smoke Launcher System").GetComponent<VehicleSmokeManager>();
                        for (int i = 0; i < 8; i++)
                        {
                            VehicleSmokeManager.SmokeSlot slot = smoke_manager._smokeSlots[i];
                            Transform smoke_cap = smoke_launcher.transform.GetChild(i+1);
                            slot.DisplayBone = smoke_cap;
                            slot.SpawnLocation.transform.SetParent(smoke_cap);
                            slot.SpawnLocation.transform.position = smoke_cap.GetComponent<Renderer>().bounds.center;
                        }

                        VehicleSmokeManager.SmokePattern[] patterns = new VehicleSmokeManager.SmokePattern[4];
                        int smoke_slot_idx = 0;

                        for (int i = 0; i < patterns.Length; i++) 
                        {
                            patterns[i] = new VehicleSmokeManager.SmokePattern() 
                            {
                                SmokePatternData = new VehicleSmokeManager.SmokePatternData[] {
                                    new VehicleSmokeManager.SmokePatternData() { SmokeSlotIndex = smoke_slot_idx++ },
                                    new VehicleSmokeManager.SmokePatternData() { SmokeSlotIndex = smoke_slot_idx++ }
                                }
                            };
                        }
 
                        smoke_manager._smokeGroups = patterns;

                        if (has_sosna)
                        {
                            GameObject _sosna_u = GameObject.Instantiate(sosna_u, vic.transform.Find("T72M1_mesh (1)/T72M1_hull"));
                            _sosna_u.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                            _sosna_u.transform.parent = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret");
                            _sosna_u.transform.Find("SOSNA U").parent = turret.GetComponent<LateFollowTarget>()._lateFollowers[0].transform;

                            turret.Find("LUNA").localScale = new Vector3(0f, 0f, 0f);
                            turret.Find("night sight cover").localScale = new Vector3(0f, 0f, 0f);
                        }

                        if (!only_smoke)
                        {
                            Transform hull_kontakt = kontakt.transform.Find("HULL ERA");
                            Transform turret_kontakt = kontakt.transform.Find("TURRET ERA");
                            Transform mantlet_k1 = null;

                            turret_kontakt.parent = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret");

                            if (has_k1)
                            {
                                mantlet_k1 = kontakt.transform.Find("MANTLET MOUNT");
                                mantlet_k1.parent = vic.transform.Find("---MESH---/HULL/TURRET/GUN");

                                if (!has_sosna && has_reflective_plates) {
                                    turret_kontakt.Find("Cube.052 (2)").gameObject.SetActive(false);
                                    turret_kontakt.Find("Cube.053 (2)").gameObject.SetActive(false);
                                    turret_kontakt.Find("ARMOUR/Cube.052 (1)").gameObject.SetActive(false);
                                    turret_kontakt.Find("ARMOUR/Cube.053 (1)").gameObject.SetActive(false);
                                }
                            }

                            Transform hull_armour = hull_kontakt.Find("ARMOUR");
                            LateFollow hull_follow = hull_kontakt.Find("ARMOUR").gameObject.AddComponent<LateFollow>();
                            hull_follow.FollowTarget = vic.transform;
                            hull_follow.enabled = true;
                            hull_follow.Awake();
                            hull_kontakt.Find("ARMOUR").parent = null;

                            LateFollow turret_follow = turret_kontakt.Find("ARMOUR").gameObject.AddComponent<LateFollow>();
                            turret_follow.FollowTarget = vic.transform.Find("---MESH---/HULL/TURRET");
                            turret_follow.enabled = true;
                            turret_follow.Awake();
                            turret_kontakt.Find("ARMOUR").parent = null;

                            if (has_k5)
                            {
                                kontakt.transform.Find("TURRET PLATE").parent = turret_follow.transform;
                                kontakt.transform.Find("HULL ERA FRONT HULL/ARMOUR").parent = hull_follow.transform;
                                kontakt.transform.Find("HULL PLATE").parent = hull_follow.transform;

                                if (has_sosna)
                                {
                                    GameObject b3_k5 = GameObject.Instantiate(t72b_k5_b3_full, vic.transform.Find("T72M1_mesh (1)/T72M1_hull"));
                                    b3_k5.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                                    Transform b3_k5_turret = b3_k5.transform.Find("TURRET ERA");
                                    b3_k5_turret.parent = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret");
                                    b3_k5_turret.transform.Find("ARMOUR").parent = turret_follow.transform;

                                    if (has_ubh)
                                    {
                                        GameObject ubh_kit = GameObject.Instantiate(t72b3m_ubh_kit, vic.transform.Find("T72M1_mesh (1)/T72M1_hull"));
                                        ubh_kit.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                                        Transform ubh_turret = ubh_kit.transform.Find("TURRET STUFF");
                                        Transform ubh_hull = ubh_kit.transform.Find("HULL STUFF");

                                        LateFollow hull_follow_relikt = ubh_hull.Find("RELIKT BAGS/ARMOUR").gameObject.AddComponent<LateFollow>();
                                        hull_follow_relikt.FollowTarget = vic.transform;
                                        hull_follow_relikt.enabled = true;
                                        hull_follow_relikt.Awake();
                                        ubh_hull.Find("RELIKT BAGS/ARMOUR").parent = null;
                                        ubh_hull.Find("SIDE SLAT").parent = hull_follow_relikt.transform;

                                        ubh_turret.parent = vic.transform.Find("---MESH---/HULL/TURRET/T72M1_turret");
                                        ubh_turret.Find("TURRET K5/ARMOUR").parent = turret_follow.transform;
                                        ubh_turret.Find("TURRET RELIKT/ARMOUR").parent = turret_follow.transform;
                                        ubh_turret.Find("TURRET SLAT").parent = turret_follow.transform;

                                        hull_kontakt.gameObject.SetActive(false);
                                        for (int i = 0; i < 6; i++) {
                                            hull_armour.transform.GetChild(i).gameObject.SetActive(false);
                                        }
                                    }
                                }
                            }

                            if (has_k1)
                            {
                                LateFollow k1_mantlet_follow = mantlet_k1.Find("MANTLET K1 ARMOUR").gameObject.AddComponent<LateFollow>();
                                k1_mantlet_follow.FollowTarget = vic.transform.Find("---MESH---/HULL/TURRET/GUN");
                                k1_mantlet_follow.enabled = true;
                                k1_mantlet_follow.Awake();
                                mantlet_k1.Find("MANTLET K1 ARMOUR").parent = null;
                            }

                            Mesh hull_mesh = t72b_hull;
                            if (has_k5) {
                                hull_mesh = t72b_k5_hull_mesh;
                            }

                            Transform hull_rend = vic.transform.Find("T72M1_mesh (1)/T72M1_hull");
                            hull_rend.GetComponent<MeshFilter>().sharedMesh = hull_mesh;
                        }

                        string name = "";

                        if (is_soviet) {
                            if (has_k1 && !has_reflective_plates) name = "T-72AV";
                            if (!has_k5 && !has_k1 && has_reflective_plates) name = "T-72B obr.1984";
                            if (!has_k5 && has_k1 && has_reflective_plates) name = "T-72B obr.1985";
                            if (has_k5) name = "T-72B obr.1989";
                        }

                        if (!is_soviet) {
                            if (has_k1 && !has_reflective_plates) name = "KPz T-72M1V";
                            if (has_k5) name = "KPz T-72M1M";
                            if (!has_k5 && !has_k1 && has_reflective_plates) name = "T-72M1M";
                            if (!has_k5 && has_k1 && has_reflective_plates) name = "T-72M1M";
                        }

                        if (has_k5 && has_sosna) name = "T-72B3";
                        if (has_k1 && has_sosna && has_reflective_plates) name = "T-72B1MS";
                        if (name == "T-72B3" && has_ubh) name = "T-72B3M";

                        vic._friendlyName = name;
                    }
                }
            }

            yield break;
        }

        public class PreviouslyT72M : MonoBehaviour { }

        [HarmonyPatch(typeof(UnitSpawner), "SpawnUnit", new Type[] { typeof(string), typeof(UnitMetaData), typeof(WaypointHolder), typeof(Transform) })]
        public static class OverrideT72M
        {
            private static void Prefix(out bool __state, UnitSpawner __instance, ref string uniqueName)
            {
                __state = false;

                bool conversion_reqd = t72m_composite_cheeks.Value || t72m_super_composite_cheeks.Value || era_t72m.Value || k5_t72m.Value;

                if (uniqueName == "T72M" && conversion_reqd)
                {
                    __state = true;
                    uniqueName = "T72M1";
                }
            }

            private static void Postfix(bool __state, ref IUnit __result)
            {
                if (__state)
                {
                    PreviouslyT72M comp = __result.transform.gameObject.AddComponent<PreviouslyT72M>();
                    comp.enabled = false;
                }
            }
        }

        public static void LoadAssets() 
        {
            if (assets_loaded) return;
            if (!t72_patch.Value) return;

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

            t72b_only_smoke = t72b_bundle.LoadAsset<GameObject>("T72B ONLY SMOKE");
            t72b_only_smoke.hideFlags = HideFlags.DontUnloadUnusedAsset;

            sosna_u = t72b_bundle.LoadAsset<GameObject>("SOSNA U");
            sosna_u.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t72b_hull = t72b_bundle.LoadAsset<Mesh>("t72b_hull");
            t72b_hull.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t72av_k1_full = t72av_bundle.LoadAsset<GameObject>("T72AV");
            t72av_k1_full.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t72av_turret = t72av_bundle.LoadAsset<Mesh>("t72av_turret");
            t72av_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t72av_sosna_turret = t72av_bundle.LoadAsset<Mesh>("t72av_sosnau");
            t72av_sosna_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

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
            Util.SetupFLIRShaders(sosna_u);

            Transform hull_k1 = t72b_k1_full.transform.Find("HULL ERA/ARMOUR");
            Transform turret_k1 = t72b_k1_full.transform.Find("TURRET ERA/ARMOUR");
            Transform mantlet_k1 = t72b_k1_full.transform.Find("MANTLET MOUNT/MANTLET K1 ARMOUR");
            Kontakt1.Setup(hull_k1, hull_k1.parent);
            Kontakt1.Setup(turret_k1, turret_k1.parent);
            Kontakt1.Setup(mantlet_k1, mantlet_k1.parent.Find("K1 MANTLET"));

            hull_k1 = t72av_k1_full.transform.Find("HULL ERA/ARMOUR");
            turret_k1 = t72av_k1_full.transform.Find("TURRET ERA/ARMOUR");
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
            hull_plate_armor.PrimaryHeatRha = 15f;
            hull_plate_armor.PrimarySabotRha = 15f;

            GameObject splash_guard = t72b_k5_1989_full.transform.Find("HULL PLATE/SPLASH GUARD ARMOUR").gameObject;
            splash_guard.tag = "Penetrable";
            splash_guard.layer = 8;
            UniformArmor splash_guard_armor = splash_guard.AddComponent<UniformArmor>();
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

            Transform ubh_turret_k5 = t72b3m_ubh_kit.transform.Find("TURRET STUFF/TURRET K5/ARMOUR");
            Transform ubh_turret_relikt = t72b3m_ubh_kit.transform.Find("TURRET STUFF/TURRET RELIKT/ARMOUR");
            Transform ubh_hull_relikt = t72b3m_ubh_kit.transform.Find("HULL STUFF/RELIKT BAGS/ARMOUR");

            Kontakt5.Setup(ubh_turret_k5, ubh_turret_k5.parent);
            Relikt.Setup(ubh_turret_relikt, ubh_turret_relikt.parent);
            Relikt.Setup(ubh_hull_relikt, ubh_hull_relikt.parent);

            GameObject turret_slat = t72b3m_ubh_kit.transform.Find("TURRET STUFF/TURRET SLAT/ARMOUR").gameObject;
            turret_slat.tag = "Penetrable";
            turret_slat.layer = 8;
            UniformArmor turret_slat_armor = turret_slat.AddComponent<UniformArmor>();
            turret_slat_armor._name = "turret slat armour";
            turret_slat_armor.PrimaryHeatRha = 60f;
            turret_slat_armor.PrimarySabotRha = 15f;

            GameObject hull_slat = t72b3m_ubh_kit.transform.Find("HULL STUFF/SIDE SLAT/ARMOUR").gameObject;
            hull_slat.tag = "Penetrable";
            hull_slat.layer = 8;
            UniformArmor hull_slat_armor = hull_slat.AddComponent<UniformArmor>();
            hull_slat_armor._name = "hull side slat armour";
            hull_slat_armor.PrimaryHeatRha = 60f;
            hull_slat_armor.PrimarySabotRha = 15f;

            GameObject sosna_sight_complex = sosna_u.transform.Find("SOSNA U/SIGHT COMPLEX").gameObject;
            sosna_sight_complex.tag = "Penetrable";
            sosna_sight_complex.layer = 8;
            UniformArmor sosna_sight_complex_armor = sosna_sight_complex.AddComponent<UniformArmor>();
            sosna_sight_complex_armor._name = "Sosna-U sight complex";
            sosna_sight_complex_armor.PrimaryHeatRha = 30f;
            sosna_sight_complex_armor.PrimarySabotRha = 30f;

            GameObject sosna_cover = sosna_u.transform.Find("SOSNA U/FRONT COVER").gameObject;
            sosna_cover.tag = "Penetrable";
            sosna_cover.layer = 8;
            UniformArmor sosna_cover_armor = sosna_cover.AddComponent<UniformArmor>();
            sosna_cover_armor._name = "front cover";
            sosna_cover_armor.PrimaryHeatRha = 10f;
            sosna_cover_armor.PrimarySabotRha = 10f;

            GameObject sosna_shield = sosna_u.transform.Find("SOSNA U/SHIELD").gameObject;
            sosna_shield.tag = "Penetrable";
            sosna_shield.layer = 8;
            UniformArmor sosna_shield_armor = sosna_shield.AddComponent<UniformArmor>();
            sosna_shield_armor._name = "shield";
            sosna_shield_armor.PrimaryHeatRha = 15f;
            sosna_shield_armor.PrimarySabotRha = 15f;

            assets_loaded = true;
        }

        public static void Init()
        {
            if (!t72_patch.Value) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}