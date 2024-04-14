using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Camera;
using GHPC.Equipment.Optics;
using GHPC.Player;
using GHPC.State;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using GHPC;
using MelonLoader;
using Reticle;
using TMPro;
using UnityEngine;
using NWH;
using NWH.VehiclePhysics;
using static UnityEngine.GraphicsBuffer;
using GHPC.UI.Tips;
using MelonLoader.Utils;
using System.IO;
using Thermals;
using GHPC.Equipment;

namespace PactIncreasedLethality
{
    public class T80
    {
        static MelonPreferences_Entry<bool> t80_patch;
        static MelonPreferences_Entry<bool> super_engine;
        static VehicleController abrams_vic_controller;
        static Dictionary<string, AmmoClipCodexScriptable> ap;
        static MelonPreferences_Entry<string> t80_ammo_type;
        static MelonPreferences_Entry<bool> t80_random_ammo;
        static MelonPreferences_Entry<bool> thermals;
        static MelonPreferences_Entry<string> thermals_quality;
        static MelonPreferences_Entry<bool> zoom_snapper;
        static MelonPreferences_Entry<bool> super_fcs_t80;
        static MelonPreferences_Entry<bool> kontakt5;

        static Mesh turret_cleaned_mesh; 

        public static void Config(MelonPreferences_Category cfg)
        {
            t80_patch = cfg.CreateEntry<bool>("T-80B Patch", true);
            t80_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            super_engine = cfg.CreateEntry<bool>("Super Engine/Transmission (T-80B)", false);
            super_engine.Comment = "vrrrrrrrrrrooooooooom";

            t80_ammo_type = cfg.CreateEntry<string>("AP Round (T-80B)", "3BM32");
            t80_ammo_type.Comment = "3BM32, 3BM26 (composite optimized), 3BM42 (composite optimized)";

            t80_random_ammo = cfg.CreateEntry<bool>("Random AP Round (T-80B)", false);
            t80_random_ammo.Comment = "Randomizes ammo selection for T-64As (3BM26, 3BM32, 3BM42)";

            zoom_snapper = cfg.CreateEntry<bool>("Quick Zoom Switch", true);
            zoom_snapper.Comment = "Press middle mouse to instantly switch between low and high magnification on the daysight";

            super_fcs_t80 = cfg.CreateEntry<bool>("Super FCS (T-80B)", false);
            super_fcs_t80.Comment = "basically sosna-u lol (digital 4x-12x zoom, 2-axis stabilizer w/ lead, point-n-shoot)";

            thermals = cfg.CreateEntry<bool>("Has Thermals (T-80B)", true);
            thermals.Comment = "Replaces night vision sight with thermal sight";
            thermals_quality = cfg.CreateEntry<string>("Thermals Quality (T-80B)", "High");
            thermals_quality.Comment = "Low, High";

            kontakt5 = cfg.CreateEntry<bool>("Kontakt-5 ERA (T-80B)", true);
            kontakt5.Comment = "    B           I           G           brick";
        }

        public static IEnumerator Convert(GameState _)
        {
            if (ap == null)
                ap = new Dictionary<string, AmmoClipCodexScriptable>()
                {
                    ["3BM32"] = APFSDS_125mm.clip_codex_3bm32,
                    ["3BM26"] = APFSDS_125mm.clip_codex_3bm26,
                    ["3BM42"] = APFSDS_125mm.clip_codex_3bm42,
                };

            foreach (Vehicle vic in PactIncreasedLethalityMod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (!vic.FriendlyName.Contains("T-80")) continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                UsableOptic day_optic = Util.GetDayOptic(weapon.FCS);
                if (zoom_snapper.Value)
                    day_optic.gameObject.AddComponent<DigitalZoomSnapper>();

                int rand = UnityEngine.Random.Range(0, ap.Count);
                string ammo_str = t80_random_ammo.Value ? ammo_str = ap.ElementAt(rand).Key : t80_ammo_type.Value;

                vic.AimablePlatforms[1].transform.Find("optic cover parent").gameObject.SetActive(false);

                if (thermals.Value)
                {
                    PactThermal.Add(weapon.FCS.NightOptic, thermals_quality.Value.ToLower());
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);
    
                    weapon.FCS.NightOptic.Alignment = OpticAlignment.BoresightStabilized;
                    weapon.FCS.NightOptic.RotateAzimuth = true;
                }

                try
                {
                    AmmoClipCodexScriptable codex = ap[ammo_str];
                    loadout_manager.LoadedAmmoTypes[0] = codex;
                    for (int i = 0; i < loadout_manager.RackLoadouts.Length; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        rack.ClipTypes[0] = codex.ClipType;
                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();
                }
                catch (Exception)
                {
                    MelonLogger.Msg("Loading default 3BM32 for " + vic.FriendlyName);
                }

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
                    chassis._maxReverseSpeed = 11.176f;
                    chassis._originalEnginePower = 1750.99f;
                }

                if (super_fcs_t80.Value)
                {
                    weapon.FCS.transform.Find("GPS/1G42 Canvas").gameObject.SetActive(false);
                    Sosna.Add(day_optic, weapon.FCS.NightOptic, vic.WeaponsManager.Weapons[1]);
                }

                if (kontakt5.Value)
                {
                    Transform turret = vic.transform.Find("T80B_rig/HULL/TURRET");
                    Transform turret_rend = turret.Find("turret");
                    turret_rend.GetComponent<MeshFilter>().sharedMesh = turret_cleaned_mesh;
                    turret_rend.GetComponent<MeshRenderer>().materials[1].color = new Color(0, 0, 0, 0);
                    turret_rend.gameObject.AddComponent<HeatSource>();

                    turret.Find("turret numbers").gameObject.SetActive(false);
                    vic.transform.Find("T80B_stowage/towropes_front").gameObject.SetActive(false);

                    for (int i = 1; i <= 5; i++)
                        turret.Find("smoke_l_" + i).localScale = Vector3.zero;
                    for (int i = 1; i <= 3; i++)
                        turret.Find("smoke_r_" + i).localScale = Vector3.zero;

                    GameObject kontakt_5_turret = GameObject.Instantiate(Kontakt5.t80_kontakt_5_turret_array);
                    turret_rend.gameObject.AddComponent<LateFollowTarget>();
                    LateFollow k5_turret_follow = kontakt_5_turret.AddComponent<LateFollow>();
                    k5_turret_follow.FollowTarget = turret_rend;
                    k5_turret_follow.enabled = true;
                    k5_turret_follow.Awake();
                    k5_turret_follow._localPosShift = new Vector3(-1.2174f, -0.6388f, -0.185f);
                    k5_turret_follow._localRotShift = Quaternion.Euler(90f, 0f, 0f);

                    GameObject kontakt_5_roof = GameObject.Instantiate(Kontakt5.t80_kontakt_5_roof_array);
                    LateFollow k5_roof_follow = kontakt_5_roof.AddComponent<LateFollow>();
                    k5_roof_follow.FollowTarget = turret_rend;
                    k5_roof_follow.enabled = true;
                    k5_roof_follow.Awake();
                    k5_roof_follow._localPosShift = new Vector3(0.23f, -0.5f, -0.5f);
                    k5_roof_follow._localRotShift = Quaternion.Euler(90f, 0f, 0f);

                    GameObject kontakt_5_hull = GameObject.Instantiate(Kontakt5.t80_kontakt_5_hull_array);
                    vic.transform.Find("T80B_rig/HULL").gameObject.AddComponent<LateFollowTarget>();
                    LateFollow k5_hull_follow = kontakt_5_hull.AddComponent<LateFollow>();
                    k5_hull_follow.FollowTarget = vic.transform.Find("T80B_rig/HULL");
                    k5_hull_follow.enabled = true;
                    k5_hull_follow.Awake();
                    k5_hull_follow._localPosShift = new Vector3(-0.75f, 0.07f, 2.99f);
                    k5_hull_follow._localRotShift = Quaternion.Euler(0f, 180f, 0f);

                    for (int i = 1; i >= -1; i -= 2)
                    {
                        GameObject kontakt_5_side_hull = GameObject.Instantiate(Kontakt5.kontakt_5_side_hull_array_ext);
                        kontakt_5_side_hull.transform.localScale = new Vector3(1f, 1f, i);
                        LateFollow k5_side_follow = kontakt_5_side_hull.AddComponent<LateFollow>();
                        k5_side_follow.FollowTarget = vic.transform.Find("T80B_rig/HULL");
                        k5_side_follow.enabled = true;
                        k5_side_follow.Awake();
                        k5_side_follow._localPosShift = new Vector3(1.8f * i, 0.01f, 2.43f);
                        k5_side_follow._localRotShift = Quaternion.Euler(0f, 90f, 0f);
                    }

                    vic._friendlyName += "V";
                }
            }

            yield break;
        }

        public static void Init()
        {
            if (!t80_patch.Value) return;

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

            if (turret_cleaned_mesh == null)
            {
                var blyat_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t80_turret_cleaned"));
                turret_cleaned_mesh = blyat_bundle.LoadAsset<Mesh>("t80turret_front_cleaned.asset");
                turret_cleaned_mesh.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
