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
using Thermals;
using HarmonyLib;
using GHPC.Audio;
using GHPC.Effects;
using GHPC.Equipment;
using Reticle;
using static UnityEngine.GraphicsBuffer;
using static PactIncreasedLethality.T72;

namespace PactIncreasedLethality
{
    public class T80
    {
        static MelonPreferences_Entry<bool> t80_patch;
        static MelonPreferences_Entry<bool> super_engine;
        static VehicleController abrams_vic_controller;
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

        public class Kontakt5Visual : MonoBehaviour {
            public MeshRenderer visual;
            public string type;
            public static Dictionary<string, Material> destroyed_mats = new Dictionary<string, Material>() {
                ["HULL"] = hull_destroyed,
                ["TURRET"] = turret_destroyed,
                ["SIDES"] = sides_destroyed
            };
        }

        [HarmonyPatch(typeof(GHPC.UniformArmor), "Detonate")]
        public static class K5Detonate {
            private static void Postfix(GHPC.UniformArmor __instance) {
                if (__instance.transform.GetComponent<Kontakt5Visual>() == null) return;
                Kontakt5Visual vis = __instance.transform.GetComponent<Kontakt5Visual>();

                Material[] new_mat = vis.visual.materials;

                for (int i = 0; i < new_mat.Length; i++)
                {
                    if (new_mat[i].name.Contains("plate") && !new_mat[i].name.Contains("dark")) {
                        new_mat[i] = Kontakt5Visual.destroyed_mats[vis.type];
                    }
                }

                vis.visual.materials = new_mat;

                ParticleEffectsManager.Instance.CreateImpactEffectOfType(
                    Kontakt5.dummy_he, ParticleEffectsManager.FusedStatus.Fuzed, ParticleEffectsManager.SurfaceMaterial.Steel, false, __instance.transform.position);
                ImpactSFXManager.Instance.PlaySimpleImpactAudio(ImpactAudioType.MainGunHeat, __instance.transform.position);
            }
        }

        static void K5Setup(Transform vis_transform, Transform k5_t, string type) {
            UniformArmor k5_armour = k5_t.gameObject.AddComponent<UniformArmor>();
            k5_armour._name = "Kontakt-5";
            k5_armour.PrimaryHeatRha = 250f;
            k5_armour.PrimarySabotRha = 120f;
            k5_armour.SecondaryHeatRha = 0f;
            k5_armour.SecondarySabotRha = 0f;
            k5_armour._canShatterLongRods = true;
            k5_armour._normalizesHits = false;
            k5_armour.AngleMatters = false;
            k5_armour._isEra = true;
            k5_armour._armorType = Kontakt5.kontakt5_so;

            Kontakt5Visual vis = k5_t.gameObject.AddComponent<Kontakt5Visual>();
            vis.visual = vis_transform.transform.GetChild(k5_t.GetSiblingIndex()).GetComponent<MeshRenderer>();
            vis.type = type;
        }

        public class UpdateAmmoTypeUI : MonoBehaviour
        {
            GameObject ap;
            GameObject heat;
            GameObject he;
            GameObject glatgm;
            GameObject current_display;
            public FireControlSystem fcs;
            public Transform canvas; 

            Dictionary<AmmoType.AmmoShortName, GameObject> displays;

            void Awake()
            {
                ap = canvas.transform.Find("ammo text APFSDS (TMP)").gameObject;
                heat = canvas.transform.Find("ammo text HEAT (TMP)").gameObject;
                he = canvas.transform.Find("ammo text HE (TMP)").gameObject;
                glatgm = canvas.transform.Find("ammo text GLATGM (TMP)").gameObject;

                current_display = ap;

                displays = new Dictionary<AmmoType.AmmoShortName, GameObject>()
                {
                    [AmmoType.AmmoShortName.Sabot] = ap,
                    [AmmoType.AmmoShortName.Heat] = heat,
                    [AmmoType.AmmoShortName.He] = he,
                    [AmmoType.AmmoShortName.Missile] = glatgm,
                };
            }

            void Update()
            {
                if (displays[fcs.CurrentAmmoType.ShortName] != current_display)
                {
                    current_display.SetActive(false);
                    current_display = displays[fcs.CurrentAmmoType.ShortName];
                    current_display.SetActive(true);
                }
            }
        }

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
                Transform turret = vic.transform.Find("T80B_rig/HULL/TURRET");

                if (zoom_snapper.Value && !super_fcs_t80.Value)
                    day_optic.gameObject.AddComponent<DigitalZoomSnapper>();

                int rand = UnityEngine.Random.Range(0, AMMO_125mm.ap.Count);
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
                    AmmoClipCodexScriptable codex = AMMO_125mm.ap[ammo_str];
                    AmmoClipCodexScriptable atgm_codex = null;
                    if (t80_atgm_type.Value != "9M112M")
                        atgm_codex  = AMMO_125mm.atgm[t80_atgm_type.Value];

                    loadout_manager.LoadedAmmoTypes[0] = codex;

                    if (atgm_codex != null)
                        loadout_manager.LoadedAmmoTypes[3] = atgm_codex;

                    for (int i = 0; i < loadout_manager.RackLoadouts.Length; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        rack.ClipTypes[0] = codex.ClipType;

                        if (atgm_codex != null)
                            rack.ClipTypes[3] = atgm_codex.ClipType;

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
                UpdateAmmoTypeUI ui_fix = day_optic.gameObject.AddComponent<UpdateAmmoTypeUI>();
                ui_fix.canvas = canvas;
                ui_fix.fcs = weapon.FCS;

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
                    Sosna.Add(day_optic, weapon.FCS.NightOptic, vic.WeaponsManager.Weapons[1], true);

                    CustomGuidanceComputer gc = weapon.FCS.gameObject.AddComponent<CustomGuidanceComputer>();
                    gc.fcs = weapon.FCS;
                    gc.mgu = weapon.FCS.GetComponent<MissileGuidanceUnit>();

                    LockOnLead s = weapon.FCS.gameObject.AddComponent<LockOnLead>();
                    s.fcs = weapon.FCS;
                    s.guidance_computer = gc;

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
                    turret_rend.gameObject.AddComponent<HeatSource>();

                    turret.Find("turret numbers").gameObject.SetActive(false);
                    vic.transform.Find("T80B_stowage/towropes_front").gameObject.SetActive(false);

                    for (int i = 1; i <= 5; i++)
                        turret.Find("smoke_l_" + i).localScale = Vector3.zero;
                    for (int i = 1; i <= 3; i++)
                        turret.Find("smoke_r_" + i).localScale = Vector3.zero;

                    Transform hull_rend = vic.transform.Find("T80_mesh/body");
                    hull_rend.GetComponent<MeshFilter>().sharedMesh = t80bv_hull;
                    hull_rend.gameObject.AddComponent<HeatSource>();

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
                    turret_rend.gameObject.AddComponent<HeatSource>();

                    turret.Find("turret numbers").gameObject.SetActive(false);
                    vic.transform.Find("T80B_stowage/towropes_front").gameObject.SetActive(false);

                    for (int i = 1; i <= 5; i++)
                        turret.Find("smoke_l_" + i).localScale = Vector3.zero;
                    for (int i = 1; i <= 3; i++)
                        turret.Find("smoke_r_" + i).localScale = Vector3.zero;

                    Transform hull_rend = vic.transform.Find("T80_mesh/body");
                    hull_rend.GetComponent<MeshFilter>().sharedMesh = hull_cleaned_mesh;
                    hull_rend.gameObject.AddComponent<HeatSource>();

                    Transform skirts_rend = vic.transform.Find("T80_mesh/skirts");
                    skirts_rend.GetComponent<MeshFilter>().sharedMesh = skirts_cleaned_mesh;
                    skirts_rend.gameObject.AddComponent<HeatSource>();

                    GameObject k5_turret = GameObject.Instantiate(t80u_turret, turret_rend);
                    k5_turret.transform.localEulerAngles = new Vector3(90f, 0f, 180f);

                    LateFollow k5_turret_follow = k5_turret.transform.Find("armour").gameObject.AddComponent<LateFollow>();
                    k5_turret_follow.FollowTarget = turret;
                    k5_turret_follow.enabled = true;
                    k5_turret_follow.Awake();
                    k5_turret.transform.Find("armour").parent = null;

                    GameObject k5_hull = GameObject.Instantiate(t80u_hull, hull_rend);
                    k5_hull.transform.localEulerAngles = new Vector3(0f, -180f, 0f);

                    LateFollow k5_hull_follow = k5_hull.transform.Find("armour").gameObject.AddComponent<LateFollow>();
                    k5_hull_follow.FollowTarget = vic.transform;
                    k5_hull_follow.enabled = true;
                    k5_hull_follow.Awake();
                    k5_hull.transform.Find("armour").parent = null;

                    GameObject k5_sides = GameObject.Instantiate(t80u_hull_sides, skirts_rend);
                    k5_sides.transform.localEulerAngles = new Vector3(0f, -180f, 0f);

                    LateFollow k5_sides_follow = k5_sides.transform.Find("armour").gameObject.AddComponent<LateFollow>();
                    k5_sides_follow.FollowTarget = vic.transform;
                    k5_sides_follow.enabled = true;
                    k5_sides_follow.Awake();
                    k5_sides.transform.Find("armour").parent = null;

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

            if (gun_2a46m4 == null) {
                gun_2a46m4 = ScriptableObject.CreateInstance<WeaponSystemCodexScriptable>();
                gun_2a46m4.name = "gun_2a46m4";
                gun_2a46m4.CaliberMm = 125;
                gun_2a46m4.FriendlyName = "125mm Gun 2A46M-4";
                gun_2a46m4.Type = WeaponSystemCodexScriptable.WeaponType.LargeCannon;
            }

            if (turret_cleaned_mesh == null)
            {
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

                UniformArmor hull_plate = t80u_hull.transform.Find("armour/plate").gameObject.AddComponent<UniformArmor>();
                hull_plate._name = "glacis addon plate";
                hull_plate._armorType = Armour.ru_hhs_armor;
                hull_plate.PrimaryHeatRha = 30f;
                hull_plate.PrimarySabotRha = 30f;
                hull_plate._canShatterLongRods = true;
                hull_plate._normalizesHits = true;
                hull_plate.AngleMatters = true; 

                UniformArmor hull_splash_guard = t80u_hull.transform.Find("armour/splash").gameObject.AddComponent<UniformArmor>();
                hull_splash_guard._name = "glacis splash guard";
                hull_splash_guard._armorType = Armour.ru_welded_armor;
                hull_splash_guard.PrimaryHeatRha = 3f;
                hull_splash_guard.PrimarySabotRha = 3f;
                hull_splash_guard._canShatterLongRods = true;
                hull_splash_guard._normalizesHits = true;
                hull_splash_guard.AngleMatters = true;

                t80u_front_flaps.tag = "Penetrable";
                t80u_front_flaps.layer = 8;
                UniformArmor front_rubber_flaps = t80u_front_flaps.AddComponent<UniformArmor>();
                front_rubber_flaps._name = "rubber flap";
                front_rubber_flaps.PrimaryHeatRha = 5f;
                front_rubber_flaps.PrimarySabotRha = 5f;
                front_rubber_flaps._canShatterLongRods = false;
                front_rubber_flaps._normalizesHits = false;
                front_rubber_flaps.AngleMatters = false;

                foreach (Transform t in t80u_hull.transform) {
                    if (t.name == "armour") continue;
                    foreach (Material mat in t.GetComponent<MeshRenderer>().materials) {
                        mat.shader = Shader.Find("Standard (FLIR)");
                    }
                }
                t80u_hull.AddComponent<HeatSource>();

                foreach (Transform t in t80u_turret.transform)
                {
                    if (t.name == "armour") continue;
                    if (t.name.Contains("rubber")) continue;
                    if (t.name.Contains("t80u_smokes")) continue;
                    foreach (Material mat in t.GetComponent<MeshRenderer>().materials)
                    {
                        mat.shader = Shader.Find("Standard (FLIR)");
                    }
                }
                t80u_turret.AddComponent<HeatSource>();

                foreach (Transform t in t80u_hull_sides.transform)
                {
                    if (t.name == "armour") continue;
                    foreach (Material mat in t.GetComponent<MeshRenderer>().materials)
                    {
                        mat.shader = Shader.Find("Standard (FLIR)");
                    }
                }
                t80u_hull_sides.AddComponent<HeatSource>();

                foreach (Transform armour_transform in t80u_hull.transform.Find("armour")) {
                    armour_transform.gameObject.tag = "Penetrable";
                    armour_transform.gameObject.layer = 8;

                    if (!armour_transform.name.Contains("kontakt")) continue;

                    K5Setup(t80u_hull.transform, armour_transform, "HULL");
                }

                foreach (Transform armour_transform in t80u_turret.transform.Find("armour"))
                {
                    armour_transform.gameObject.tag = "Penetrable";
                    armour_transform.gameObject.layer = 8;

                    if (armour_transform.name.Contains("rubber")) {
                        UniformArmor rubber_flaps = armour_transform.gameObject.AddComponent<UniformArmor>();
                        rubber_flaps._name = "rubber flap";
                        rubber_flaps.PrimaryHeatRha = 5f;
                        rubber_flaps.PrimarySabotRha = 5f;
                        rubber_flaps._canShatterLongRods = false;
                        rubber_flaps._normalizesHits = false;
                        rubber_flaps.AngleMatters = false;
                    }

                    if (!armour_transform.name.Contains("kontakt")) continue;

                    K5Setup(t80u_turret.transform, armour_transform, "TURRET");
                }

                foreach (Transform armour_transform in t80u_hull_sides.transform.Find("armour"))
                {
                    armour_transform.gameObject.tag = "Penetrable";
                    armour_transform.gameObject.layer = 8;

                    if (!armour_transform.name.Contains("kontakt")) continue;

                    K5Setup(t80u_hull_sides.transform, armour_transform, "SIDES");
                }

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
            }

            StateController.WaitForComplete(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
