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

        static Material t72m1_material;
        static GameObject t72m1_composite_cheeks;
        static Mesh t72m1_turret_mesh;
        static GameObject super_comp_cheeks;

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

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in PactIncreasedLethalityMod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (!vic.FriendlyName.Contains("T-72")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                if (vic.UniqueName == "T72M1" && t72m1_super_composite_cheeks.Value) {
                    Transform turret = vic.transform.Find("---MESH---/HULL/TURRET");
                    Transform turret_followers = turret.GetComponent<LateFollowTarget>()._lateFollowers[0].transform;
                    turret_followers.Find("ARMOR/Composite Armor Array").gameObject.SetActive(false);

                    GameObject super_cheeks = GameObject.Instantiate(super_comp_cheeks, turret_followers);
                    super_cheeks.transform.localPosition = new Vector3(0.5695f, 1.7236f, -0.7984f);
                }

                if (vic.UniqueName == "T72M" && (t72m_composite_cheeks.Value || t72m_super_composite_cheeks.Value))
                {
                    Transform turret = vic.transform.Find("T72M_skirts_rig/HULL/TURRET");
                    Transform turret_rend = turret.Find("T72M_turret");

                    if (!t72m_super_composite_cheeks.Value)
                    {
                        GameObject.Instantiate(t72m1_composite_cheeks, turret.GetComponent<LateFollowTarget>()._lateFollowers[0].transform.Find("ARMOR"));
                    }
                    else
                    {
                        Transform turret_followers = turret.GetComponent<LateFollowTarget>()._lateFollowers[0].transform;
                        GameObject super_cheeks = GameObject.Instantiate(super_comp_cheeks, turret_followers);
                        super_cheeks.transform.localPosition = new Vector3(0.5695f, 1.7236f, -0.7984f);
                    }

                    turret_rend.GetComponent<MeshFilter>().sharedMesh = t72m1_turret_mesh;
                    turret_rend.GetComponent<MeshRenderer>().materials = new Material[] { t72m1_material };
                    turret_rend.gameObject.AddComponent<HeatSource>();

                    vic._friendlyName = "T-72M1";
                    vic._uniqueName = "T72M1";
                    vic_go.AddComponent<PreviouslyT72M>();
                }

                // SOVIET CREW 
                if ((soviet_t72m1.Value && vic.UniqueName == "T72M1") || (soviet_t72m.Value && vic.UniqueName == "T72M"))
                {
                    vic._friendlyName = vic.FriendlyName == "T-72M1" ? "T-72A" : "T-72";

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

                if ((era_t72m1.Value && (vic.FriendlyName == "T-72M1" || vic.FriendlyName == "T-72A"))
                    || (era_t72m.Value && (vic.FriendlyName == "T-72M" || vic.FriendlyName == "T-72")))
                {
                    var hull_late_followers = vic.GetComponent<LateFollowTarget>()._lateFollowers;
                    var turret_late_followers = vic.AimablePlatforms.Where(o => o.name == "---TURRET SCRIPTS---").First().transform.parent.GetComponent<LateFollowTarget>()._lateFollowers;

                    GameObject hull_array = GameObject.Instantiate(Kontakt1.kontakt_1_hull_array,
                        hull_late_followers.Where(o => o.name == "T-72 HULL COLLIDERS").First().transform.Find("ARMOR"));
                    hull_array.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                    hull_array.transform.localPosition = new Vector3(-0.8219f, 0.7075f, 2.4288f);

                    GameObject turret_array = GameObject.Instantiate(Kontakt1.kontakt_1_turret_array,
                        turret_late_followers.Where(o => o.name == "T-72 TURRET COLLIDERS").First().transform.Find("ARMOR"));
                    turret_array.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                    turret_array.transform.localPosition = new Vector3(0.0199f, 2.1973f, -0.8363f);

                    if ((vic.FriendlyName == "T-72M" || vic.FriendlyName == "T-72") && (vic.transform.Find("T72M_gills_rig 1") != null))
                    {
                        GameObject.Destroy(hull_array.transform.Find("left side skirt array").gameObject);
                        GameObject.Destroy(hull_array.transform.Find("right side skirt array").gameObject);
                    }

                    vic.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    vic._friendlyName += "V";
                }

                vic.AimablePlatforms[1].transform.Find("optic cover parent").gameObject.SetActive(false);

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                UsableOptic night_optic = fcs.NightOptic;
                UsableOptic day_optic = Util.GetDayOptic(fcs);

                if (better_stab.Value)
                {
                    day_optic.slot.VibrationBlurScale = 0.05f;
                    day_optic.slot.VibrationShakeMultiplier = 0.1f;
                }

                string ammo_str = (vic.UniqueName == "T72M") ? t72m_ammo_type.Value : t72m1_ammo_type.Value;
                int rand = UnityEngine.Random.Range(0, AMMO_125mm.ap.Count);

                if (t72m_random_ammo.Value && vic.UniqueName == "T72M")
                    ammo_str = t72m_random_ammo_pool.Value.ElementAt(rand);

                if (t72m1_random_ammo.Value && vic.UniqueName == "T72M1")
                    ammo_str = t72m1_random_ammo_pool.Value.ElementAt(rand);

                string heat_str = (vic.UniqueName == "T72M") ? t72m_heat_type.Value : t72m1_heat_type.Value;

                try
                {
                    AmmoClipCodexScriptable codex = AMMO_125mm.ap[ammo_str];
                    loadout_manager.LoadedAmmoTypes[0] = codex;

                    if (heat_str == "3BK18M")
                        loadout_manager.LoadedAmmoTypes[1] = clip_codex_3bk18m;

                    //loadout_manager.LoadedAmmoTypes[2] = clip_codex_3of26_vt;
                    for (int i = 0; i <= 4; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        rack.ClipTypes[0] = codex.ClipType;
                        if (heat_str == "3BK18M")
                            rack.ClipTypes[1] = clip_codex_3bk18m.ClipType;

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

                if ((thermals_t72m1.Value && vic.UniqueName == "T72M1") || (thermals_t72m.Value && vic.UniqueName == "T72M"))
                {
                    PactThermal.Add(weapon.FCS.NightOptic, thermals_quality.Value.ToLower(), (super_fcs_t72m1.Value && vic.UniqueName == "T72M1") || (super_fcs_t72m.Value && vic.UniqueName == "T72M"));
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);
                }

                if ((super_fcs_t72m1.Value && vic.UniqueName == "T72M1") || (super_fcs_t72m.Value && vic.UniqueName == "T72M"))
                {
                    fcs.transform.localPosition = new Vector3(-0.803f, 0.32f, -5.547f);
                    Sosna.Add(day_optic, night_optic, vic.WeaponsManager.Weapons[1]);
                }

                if (vic.UniqueName == "T72M1" && k5_t72m1.Value)
                {
                    Transform turret;
                    Transform turret_rend;

                    if (!vic.GetComponent<PreviouslyT72M>())
                    {
                        turret = vic.transform.Find("---MESH---/HULL/TURRET");
                        turret_rend = turret.Find("T72M1_turret");
                        turret_rend.GetComponent<MeshFilter>().sharedMesh = (thermals_t72m1.Value) ? b3_turret_cleaned_mesh : turret_cleaned_mesh;
                        turret_rend.gameObject.AddComponent<HeatSource>();
                    }
                    else {
                        turret = vic.transform.Find("T72M_skirts_rig/HULL/TURRET");
                        turret_rend = turret.Find("T72M_turret");
                        turret_rend.GetComponent<MeshFilter>().sharedMesh = turret_cleaned_mesh;
                    }

                    if (thermals_t72m1.Value && super_fcs_t72m1.Value && !vic.GetComponent<PreviouslyT72M>())
                    {
                        turret.Find("LUNA").localScale = Vector3.zero;
                    }
                    
                    GameObject kontakt_5_turret = GameObject.Instantiate((thermals_t72m1.Value && super_fcs_t72m1.Value && !vic.GetComponent<PreviouslyT72M>()) ? Kontakt5.t72b3_kontakt_5_turret_array : Kontakt5.t72_kontakt_5_turret_array);
                    turret_rend.gameObject.AddComponent<LateFollowTarget>();
                    LateFollow k5_turret_follow = kontakt_5_turret.AddComponent<LateFollow>();
                    k5_turret_follow.FollowTarget = turret_rend;
                    k5_turret_follow.enabled = true;
                    k5_turret_follow.Awake();
                    k5_turret_follow._localPosShift = new Vector3(-0.085f, 0.078f, 0.118f);
                    k5_turret_follow._localRotShift = Quaternion.Euler(00f, 90f, 0f);

                    GameObject kontakt_5_roof = GameObject.Instantiate(Kontakt5.t72_kontakt_5_roof_array);
                    LateFollow k5_roof_follow = kontakt_5_roof.AddComponent<LateFollow>();
                    k5_roof_follow.FollowTarget = turret_rend;
                    k5_roof_follow.enabled = true;
                    k5_roof_follow.Awake();
                    k5_roof_follow._localPosShift = new Vector3(-0.06f, 0.108f, -0.02f);
                    k5_roof_follow._localRotShift = Quaternion.Euler(0f, 90f, 0f);

                    if (!vic.GetComponent<PreviouslyT72M>())
                    {
                        turret.Find("smoke rack").localScale = Vector3.zero;
                        vic.transform.Find("---MESH---/equipment").gameObject.SetActive(false);

                        Transform hull_rend = vic.transform.Find("T72M1_mesh (1)/T72M1_hull");
                        hull_rend.GetComponent<MeshFilter>().sharedMesh = hull_cleaned_mesh;
                        hull_rend.GetComponent<MeshRenderer>().materials[1].color = new Color(0, 0, 0, 0);
                        hull_rend.GetComponent<MeshRenderer>().materials[2].color = new Color(0, 0, 0, 0);
                        hull_rend.gameObject.AddComponent<HeatSource>();

                        GameObject kontakt_5_hull = GameObject.Instantiate(Kontakt5.t80_kontakt_5_hull_array);
                        vic.transform.Find("---MESH---/HULL/").gameObject.AddComponent<LateFollowTarget>();
                        LateFollow k5_hull_follow = kontakt_5_hull.AddComponent<LateFollow>();
                        k5_hull_follow.FollowTarget = vic.transform.Find("---MESH---/HULL");
                        k5_hull_follow.enabled = true;
                        k5_hull_follow.Awake();
                        k5_hull_follow._localPosShift = new Vector3(-0.75f, -0.0756f, 2.55f);
                        k5_hull_follow._localRotShift = Quaternion.Euler(0f, 180f, 0f);

                        for (int i = 1; i >= -1; i -= 2)
                        {
                            GameObject kontakt_5_side_hull = GameObject.Instantiate(Kontakt5.kontakt_5_side_hull_array);
                            kontakt_5_side_hull.transform.localScale = new Vector3(1f, 1f, i);
                            LateFollow k5_side_follow = kontakt_5_side_hull.AddComponent<LateFollow>();
                            k5_side_follow.FollowTarget = vic.transform.Find("---MESH---/HULL");
                            k5_side_follow.enabled = true;
                            k5_side_follow.Awake();
                            k5_side_follow._localPosShift = new Vector3(1.8f * i, 0.01f, 2.43f);
                            k5_side_follow._localRotShift = Quaternion.Euler(0f, 90f, 0f);
                        }
                    }
                    else {
                        turret.Find("smoke rack").localScale = Vector3.zero;
                        vic.transform.Find("T72M_skirts_rig/equipment").gameObject.SetActive(false);

                        Transform hull_rend = vic.transform.Find("T72M_skirt_hull/T72M_skirt_hull");
                        hull_rend.localEulerAngles = new Vector3(0f, 90f, 0f);
                        hull_rend.localScale = new Vector3(10f, 10f, 10f);
                        hull_rend.localPosition = new Vector3(0f, 0.9471f, -1.3261f);

                        hull_rend.GetComponent<MeshFilter>().sharedMesh = hull_cleaned_mesh;
                        Material[] hull_mat = (Material[])hull_rend.GetComponent<MeshRenderer>().materials.Clone();
                        hull_mat[0] = t72m1_material;
                        hull_rend.GetComponent<MeshRenderer>().materials = hull_mat;
                        hull_rend.GetComponent<MeshRenderer>().materials[1].color = new Color(0f, 0f, 0f, 0f);
                        hull_rend.gameObject.AddComponent<HeatSource>();

                        GameObject kontakt_5_hull = GameObject.Instantiate(Kontakt5.t80_kontakt_5_hull_array);
                        vic.transform.Find("T72M_skirts_rig/HULL").gameObject.AddComponent<LateFollowTarget>();
                        LateFollow k5_hull_follow = kontakt_5_hull.AddComponent<LateFollow>();
                        k5_hull_follow.FollowTarget = vic.transform.Find("T72M_skirts_rig/HULL");
                        k5_hull_follow.enabled = true;
                        k5_hull_follow.Awake();
                        k5_hull_follow._localPosShift = new Vector3(-0.75f, -0.0756f, 2.55f);
                        k5_hull_follow._localRotShift = Quaternion.Euler(0f, 180f, 0f);

                        vic.transform.Find("T72M_skirt_hull/base glacis").gameObject.SetActive(false);

                        for (int i = 1; i >= -1; i -= 2)
                        {
                            GameObject kontakt_5_side_hull = GameObject.Instantiate(Kontakt5.kontakt_5_side_hull_array);
                            kontakt_5_side_hull.transform.localScale = new Vector3(1f, 1f, i);
                            LateFollow k5_side_follow = kontakt_5_side_hull.AddComponent<LateFollow>();
                            k5_side_follow.FollowTarget = vic.transform.Find("T72M_skirts_rig/HULL");
                            k5_side_follow.enabled = true;
                            k5_side_follow.Awake();
                            k5_side_follow._localPosShift = new Vector3(1.8f * i, 0.01f, 2.43f);
                            k5_side_follow._localRotShift = Quaternion.Euler(0f, 90f, 0f);
                        }
                    }

                    vic._friendlyName = thermals_t72m1.Value && super_fcs_t72m1.Value && !vic.GetComponent<PreviouslyT72M>() ? "T-72B3" : "T-72BA";
                }

                if ((vic.UniqueName == "T72M") && k5_t72m.Value)
                {
                    /*
                    vic.transform.Find("T72M_skirt_hull").gameObject.SetActive(true);
                    if (vic.transform.Find("T72M_gills_rig 1") != null)
                    {
                        vic.transform.Find("T72M_gills_rig 1").gameObject.SetActive(false);
                        vic.transform.Find("T72M_gills_hull").gameObject.SetActive(false);

                        foreach (LateFollow t in vic_go.GetComponent<LateFollowTarget>()._lateFollowers)
                        {
                            if (t.transform.name.Contains("gill"))
                            {
                                foreach (Transform k in t.transform)
                                {
                                    k.gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                    */

                    Transform turret = vic.transform.Find("T72M_skirts_rig/HULL/TURRET");
                    Transform turret_rend = turret.Find("T72M_turret");
                    GameObject.Instantiate(t72m1_composite_cheeks, turret.GetComponent<LateFollowTarget>()._lateFollowers[0].transform.Find("ARMOR"));
                    turret_rend.GetComponent<MeshFilter>().sharedMesh = turret_cleaned_mesh;
                    turret_rend.GetComponent<MeshRenderer>().materials = new Material[] { t72m1_material };
                    turret_rend.gameObject.AddComponent<HeatSource>();
                    
                    turret.Find("smoke rack").localScale = Vector3.zero;
                    vic.transform.Find("T72M_skirts_rig/equipment").gameObject.SetActive(false);

                    GameObject kontakt_5_turret = GameObject.Instantiate(Kontakt5.t72_kontakt_5_turret_array);
                    turret_rend.gameObject.AddComponent<LateFollowTarget>();
                    LateFollow k5_turret_follow = kontakt_5_turret.AddComponent<LateFollow>();
                    k5_turret_follow.FollowTarget = turret_rend;
                    k5_turret_follow.enabled = true;
                    k5_turret_follow.Awake();
                    k5_turret_follow._localPosShift = new Vector3(-0.085f, 0.078f, 0.118f);
                    k5_turret_follow._localRotShift = Quaternion.Euler(0f, 90f, 0f);

                    GameObject kontakt_5_roof = GameObject.Instantiate(Kontakt5.t72_kontakt_5_roof_array);
                    LateFollow k5_roof_follow = kontakt_5_roof.AddComponent<LateFollow>();
                    k5_roof_follow.FollowTarget = turret_rend;
                    k5_roof_follow.enabled = true;
                    k5_roof_follow.Awake();
                    k5_roof_follow._localPosShift = new Vector3(-0.06f, 0.108f, -0.02f);
                    k5_roof_follow._localRotShift = Quaternion.Euler(0f, 90f, 0f);

                    Transform hull_rend = vic.transform.Find("T72M_skirt_hull/T72M_skirt_hull");
                    hull_rend.localEulerAngles = new Vector3(0f, 90f, 0f);
                    hull_rend.localScale = new Vector3(10f, 10f, 10f);
                    hull_rend.localPosition = new Vector3(0f, 0.9471f, -1.3261f);

                    vic.transform.Find("T72M_skirt_hull/base glacis").gameObject.SetActive(false);

                    hull_rend.GetComponent<MeshFilter>().sharedMesh = hull_cleaned_mesh;
                    Material[] hull_mat = (Material[])hull_rend.GetComponent<MeshRenderer>().materials.Clone();
                    hull_mat[0] = t72m1_material;
                    hull_rend.GetComponent<MeshRenderer>().materials = hull_mat;
                    hull_rend.GetComponent<MeshRenderer>().materials[1].color = new Color(0f, 0f, 0f, 0f);
                    hull_rend.gameObject.AddComponent<HeatSource>();

                    GameObject kontakt_5_hull = GameObject.Instantiate(Kontakt5.t80_kontakt_5_hull_array);
                    vic.transform.Find("T72M_skirts_rig/HULL").gameObject.AddComponent<LateFollowTarget>();
                    LateFollow k5_hull_follow = kontakt_5_hull.AddComponent<LateFollow>();
                    k5_hull_follow.FollowTarget = vic.transform.Find("T72M_skirts_rig/HULL");
                    k5_hull_follow.enabled = true;
                    k5_hull_follow.Awake();
                    k5_hull_follow._localPosShift = new Vector3(-0.75f, -0.0756f, 2.55f);
                    k5_hull_follow._localRotShift = Quaternion.Euler(0f, 180f, 0f);

                    for (int i = 1; i >= -1; i -= 2) {
                        GameObject kontakt_5_side_hull = GameObject.Instantiate(Kontakt5.kontakt_5_side_hull_array);
                        kontakt_5_side_hull.transform.localScale = new Vector3(1f, 1f, i);
                        LateFollow k5_side_follow = kontakt_5_side_hull.AddComponent<LateFollow>();
                        k5_side_follow.FollowTarget = vic.transform.Find("T72M_skirts_rig/HULL");
                        k5_side_follow.enabled = true;
                        k5_side_follow.Awake();
                        k5_side_follow._localPosShift = new Vector3(1.8f * i, 0.01f, 2.43f);
                        k5_side_follow._localRotShift = Quaternion.Euler(0f, 90f, 0f);
                    }

                    vic._friendlyName = "T-72BA";
                }
           
                weapon.Feed.ReloadDuringMissileTracking = true;
                weapon.FireWhileGuidingMissile = false;
                GameObject guidance_computer_obj = GameObject.Instantiate(new GameObject("guidance computer"), vic.AimablePlatforms.Where(o => o.name == "---TURRET SCRIPTS---").First().transform.parent);
                MissileGuidanceUnit computer = guidance_computer_obj.AddComponent<MissileGuidanceUnit>();

                computer.AimElement = fcs.AimTransform;
                weapon.GuidanceUnit = computer;
                
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

            if (t72m1_material == null)
            {
                foreach (Vehicle obj in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
                {
                    if (obj.gameObject.name == "T72M1")
                    {
                        t72m1_turret_mesh = obj.transform.Find("T72M1_mesh (1)/T72M1_turret").GetComponent<MeshFilter>().sharedMesh;
                        t72m1_material = obj.transform.Find("T72M1_mesh (1)/T72M1_turret").GetComponent<MeshRenderer>().materials[0];
                        t72m1_composite_cheeks = obj.transform.Find("T-72 TURRET COLLIDERS/ARMOR/Composite Armor Array").gameObject;
                    }
                }
            }

            if (turret_cleaned_mesh == null)
            {
                var super_comp_cheeks_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "supercompcheeks"));
                super_comp_cheeks = super_comp_cheeks_bundle.LoadAsset<GameObject>("SUPER CHEEKS.prefab");
                super_comp_cheeks.hideFlags = HideFlags.DontUnloadUnusedAsset;

                foreach (Transform t in super_comp_cheeks.transform.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.tag = "Penetrable";
                    t.gameObject.layer = 8;
                }

                GameObject turret_left_cheek = super_comp_cheeks.transform.Find("LEFT COMP CHEEK").gameObject;
                VariableArmor armor_turret_l_cheek = turret_left_cheek.AddComponent<VariableArmor>();
                armor_turret_l_cheek.SetName("turret cheek composite array");
                armor_turret_l_cheek._armorType = Armour.composite_armor;
                armor_turret_l_cheek._spallForwardRatio = 0.2f;
                AarVisual aar_l_cheek = turret_left_cheek.AddComponent<AarVisual>();
                aar_l_cheek.SwitchMaterials = false;
                aar_l_cheek.HideUntilAar = true;

                GameObject turret_right_cheek = super_comp_cheeks.transform.Find("RIGHT COMP CHEEK").gameObject;
                VariableArmor armor_turret_r_cheek = turret_right_cheek.AddComponent<VariableArmor>();
                armor_turret_r_cheek.SetName("turret cheek composite array");
                armor_turret_r_cheek._armorType = Armour.composite_armor;
                armor_turret_r_cheek._spallForwardRatio = 0.2f;
                AarVisual aar_r_cheek = turret_right_cheek.AddComponent<AarVisual>();
                aar_r_cheek.SwitchMaterials = false;
                aar_r_cheek.HideUntilAar = true;

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