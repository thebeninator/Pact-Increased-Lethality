using System.Collections;
using System.Collections.Generic;
using GHPC.Equipment.Optics;
using GHPC.State;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using MelonLoader;
using Reticle;
using HarmonyLib;
using UnityEngine;
using FMOD;
using FMODUnity;
using MelonLoader.Utils;
using System.IO;
using GHPC;
using GHPC.Player;
using GHPC.Equipment;
using GHPC.Effects;
using GHPC.Camera;
using GHPC.Crew;
using GHPC.Weaponry;
using System;

namespace PactIncreasedLethality
{
    public class BMP2
    {
        [HarmonyPatch(typeof(WeaponAudio), "FinalStartLoop")]
        public static class ReplaceSound
        {
            public static FMOD.Sound sound_exterior;
            public static FMOD.Sound sound;
            public static FMOD.Sound sound_alt;

            public static List<Channel> channels = new List<Channel>() { };
            public static void Cleanup()
            {
                if (channels.Count > 2)
                {
                    channels[0].stop();
                    channels.RemoveAt(0);
                }

                for (int i = 0; i < channels.Count; i++)
                {
                    bool playing;
                    channels[i].isPlaying(out playing);
                    if (!playing) channels.RemoveAt(i);
                }
            }

            public static bool Prefix(WeaponAudio __instance)
            {
                if (__instance.SingleShotMode && __instance.SingleShotEventPaths[0].Contains("event:/Weapons/autocannon_2a42_single"))
                {
                    var corSystem = RuntimeManager.CoreSystem;

                    Vector3 vec = __instance.transform.position;

                    VECTOR pos = new VECTOR();
                    pos.x = vec.x;
                    pos.y = vec.y;
                    pos.z = vec.z;

                    VECTOR vel = new VECTOR();
                    vel.x = __instance.transform.forward.x * 10f;
                    vel.y = __instance.transform.forward.y * 10f;
                    vel.z = __instance.transform.forward.z * 10f;

                    bool interior = !CameraManager._instance.ExteriorMode && __instance == Mod.player_manager.CurrentPlayerWeapon.Weapon.WeaponSound;

                    ChannelGroup channelGroup;
                    corSystem.createChannelGroup("master", out channelGroup);

                    channelGroup.setVolumeRamp(false);
                    channelGroup.setMode(MODE._3D_WORLDRELATIVE);

                    FMOD.Channel channel;
                    FMOD.Sound sound_interior = __instance.SingleShotEventPaths[0].Contains("actually_2a72") ? sound_alt : sound;
                    FMOD.Sound s = interior ? sound_interior : sound_exterior;

                    Cleanup();

                    corSystem.playSound(s, channelGroup, true, out channel);
                    channels.Add(channel);

                    float game_vol = Mod.audio_settings_manager._previousVolume;
                    float gun_vol = interior ? game_vol + 0.0185f * (game_vol * 10f) : game_vol;

                    channel.setVolume(gun_vol);
                    channel.setVolumeRamp(false);
                    channel.set3DAttributes(ref pos, ref vel);
                    channelGroup.set3DAttributes(ref pos, ref vel);
                    channel.setPaused(false);

                    return false;
                }

                return true;
            }
        }

        static WeaponSystemCodexScriptable weapon_2a7m;

        static AmmoClipCodexScriptable clip_codex_9m113_as;
        static AmmoType.AmmoClip clip_9m113_as;
        static AmmoCodexScriptable ammo_codex_9m113_as;
        static AmmoType ammo_9m113_as;

        static AmmoClipCodexScriptable clip_codex_9m133;
        static AmmoType.AmmoClip clip_9m133;
        static AmmoCodexScriptable ammo_codex_9m133;
        static AmmoType ammo_9m133;

        static AmmoClipCodexScriptable clip_codex_bzt;
        static AmmoType.AmmoClip clip_bzt;
        static AmmoCodexScriptable ammo_codex_bzt;
        static AmmoType ammo_bzt;

        static AmmoCodexScriptable ammo_codex_apds;
        static AmmoType ammo_apds;

        static AmmoClipCodexScriptable clip_codex_ofz;
        static AmmoType.AmmoClip clip_ofz;
        static AmmoCodexScriptable ammo_codex_ofz;
        static AmmoType ammo_ofz;

        static AmmoCodexScriptable ammo_codex_ofzt;
        static AmmoType ammo_ofzt;

        static MelonPreferences_Entry<bool> bmp2_patch;
        static MelonPreferences_Entry<bool> use_3ubr8;
        static MelonPreferences_Entry<bool> use_3uof8;
        static MelonPreferences_Entry<bool> use_9m113as;
        static MelonPreferences_Entry<bool> super_fcs;
        static MelonPreferences_Entry<bool> has_lrf;
        static MelonPreferences_Entry<bool> has_thermals;
        static MelonPreferences_Entry<string> thermals_quality;
        static MelonPreferences_Entry<bool> has_kornets;
        static MelonPreferences_Entry<bool> zsu_conversion;
        static MelonPreferences_Entry<int> zsu_conversion_chance;

        static GameObject zsu_barrel;
        static GameObject zsu_full;
        static Material brdm_nsv_barrel_mat;
        static Mesh brdm_nsv_barrel_mesh;

        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static ReticleSO reticleSO_lrf;
        static ReticleMesh.CachedReticle reticle_cached_lrf;
        static Mesh turret_no_smokes;
        static Mesh bmp2m_turret;
        static Mesh turret_only_thermals;
        static GameObject bmp2m_kit;

        private static bool assets_loaded = false;

        public class MultiBarrelFix : MonoBehaviour {
            public AmmoFeed feed;
            public GameObject[] loaded_objects;
            public int max_ammo;
            public WeaponSystem weapon;

            void Update() {
                if (feed.CurrentClipRemainingCount == max_ammo) {
                    foreach (GameObject obj in loaded_objects) { obj.SetActive(true); }
                    return;
                }

                for (int i = 0; i < max_ammo - feed.CurrentClipRemainingCount; i++) {
                    loaded_objects[i].SetActive(false);
                }
            }
        }

        public static void Config(MelonPreferences_Category cfg)
        {
            bmp2_patch = cfg.CreateEntry<bool>("BMP-2 Patch", true);
            bmp2_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            use_3ubr8 = cfg.CreateEntry<bool>("Use 3UBR8", true);
            use_3ubr8.Comment = "Replaces 3UBR6; has improved penetration and better ballistics";

            use_3uof8 = cfg.CreateEntry<bool>("Use 3UOF8", true);
            use_3uof8.Comment = "Mixed belt of 3UOR6 and 3UOF8 (1:2); 3UOF8 has more explosive filler but no tracer";

            has_kornets = cfg.CreateEntry<bool>("Use 9M133 Kornet", false);
            has_kornets.Comment = "4x ready-to-fire missiles w/ 1 reload; stabilized launchers; 40 second reload time";

            super_fcs = cfg.CreateEntry<bool>("Super FCS (BMP-2)", false);
            super_fcs.Comment = "Point-n-shoot, thermal sight, autotracking";

            has_lrf = cfg.CreateEntry<bool>("Laser Rangefinder (BMP-2)", false);
            has_lrf.Comment = "Point-n-shoot; automatic lead";

            has_thermals = cfg.CreateEntry<bool>("Has Thermals (BMP-2)", false);
            thermals_quality = cfg.CreateEntry<string>("Thermals Quality (BMP-2)", "High");
            thermals_quality.Comment = "Low, High";

            use_9m113as = cfg.CreateEntry<bool>("Use 9M113AS", false);
            use_9m113as.Comment = "Fictional overfly-top-attack ATGM with dual warhead; aim above target";

            zsu_conversion = cfg.CreateEntry<bool>("BMP-23-4 Conversion", false);
            zsu_conversion.Comment = "Quad 23mm autocannons with mixed BZT (API-T)/OFZ (HE) and APDS-T belts; laser rangefinder; single-axis stabilized automatic lead";

            zsu_conversion_chance = cfg.CreateEntry<int>("BMP-23-4 Conversion Chance", 30);
            zsu_conversion_chance.Comment = "Integer; default: 30%";
        }

        public static void Update()
        {         
            if (!zsu_conversion.Value) return;
            if (Mod.player_manager == null) return;
            ReplaceSound.Cleanup();   
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (vic.FriendlyName != "BMP-2") continue;
                if (vic.GetComponent<AlreadyConverted>() != null) continue;

                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();

                WeaponSystem main_gun = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                WeaponSystemInfo main_gun_info = vic.WeaponsManager.Weapons[0];

                WeaponSystemInfo atgm_info = loadout_manager._weaponsManager.Weapons[1];
                WeaponSystem atgm = atgm_info.Weapon;

                UsableOptic day_optic = Util.GetDayOptic(main_gun.FCS);
                UsableOptic night_optic = day_optic.slot.LinkedNightSight.PairedOptic;

                Transform vis_turret = vic.transform.Find("BMP2_visual/turret");
                Transform turret = vic.transform.Find("BMP2_rig/HULL/TURRET");
                SkinnedMeshRenderer turret_smr = vis_turret.GetComponent<SkinnedMeshRenderer>();

                vic.gameObject.AddComponent<AlreadyConverted>();

                int rand = UnityEngine.Random.Range(1, 100);
                bool is_zsu = zsu_conversion.Value && rand <= zsu_conversion_chance.Value;

                AmmoClipCodexScriptable ap = use_3ubr8.Value ? Ammo_30mm.clip_codex_3ubr8 : Assets.clip_codex_3ubr6;
                AmmoClipCodexScriptable he = use_3uof8.Value ? Ammo_30mm.clip_codex_3uof8 : Assets.clip_codex_3uor6;

                if (super_fcs.Value)
                {
                    MissileGuidanceUnit computer = vic.GetComponentInChildren<MissileGuidanceUnit>();
                    FireControlSystem fcs = main_gun.FCS;
                    CustomGuidanceComputer gc = fcs.transform.parent.gameObject.AddComponent<CustomGuidanceComputer>();
                    gc.fcs = fcs;
                    gc.mgu = computer;
                    if (!has_kornets.Value)
                    {
                        gc.enabled = false;
                    }

                    SuperFCS.Add(day_optic, night_optic, vic.WeaponsManager.Weapons[2], main_gun_info, gc, vesna: true);
                }

                if (is_zsu)
                {
                    GameObject hide_barrel = new GameObject("hide_barrel");
                    GameObject h = GameObject.Instantiate(hide_barrel, vic.transform.Find("BMP2_rig/HULL/TURRET"));
                    h.transform.localScale = new Vector3(0f, 0.0f, 1f);
                    h.transform.localPosition = new Vector3(-0.1f, 0.3325f, 0.5f);

                    GameObject zsu_barrels = GameObject.Instantiate(zsu_full, vic.transform.Find("BMP2_rig/HULL/TURRET/Main gun"));
                    zsu_barrels.transform.localPosition = new Vector3(0.04f, -0.08f, 1.7589f);
                    zsu_barrels.transform.localScale = new Vector3(1.4f, 1.4f, 1.8f);

                    vic.transform.Find("BMP2_rig/HULL/TURRET/Main gun").GetComponent<LateFollowTarget>().enabled = false;

                    SkinnedMeshRenderer rend = vic.transform.Find("BMP2_visual/turret").GetComponent<SkinnedMeshRenderer>();
                    Transform[] new_bones = new Transform[11];
                    rend.bones.CopyTo(new_bones, 0);
                    new_bones[0] = h.transform;

                    vic.transform.Find("BMP2_visual/turret").GetComponent<SkinnedMeshRenderer>().bones = new_bones;

                    List<BarrelInfo> barrels = new List<BarrelInfo>();
                    main_gun._isMultiBarrel = true;

                    GameObject muzzle_flash_go = GameObject.Instantiate(main_gun._muzzleEffects[0].gameObject);
                    muzzle_flash_go.transform.Find("Muzzle Flash Side Brake Right").gameObject.SetActive(false);
                    muzzle_flash_go.transform.Find("Muzzle Flash Side Brake Left").gameObject.SetActive(false);
                    muzzle_flash_go.transform.Find("Muzzle Flash Brake R").gameObject.SetActive(false);
                    muzzle_flash_go.transform.Find("Muzzle Flash Brake L").gameObject.SetActive(false);

                    Transform zsu = vic.transform.Find("BMP2_rig/HULL/TURRET/Main gun/zsu full(Clone)").transform;
                    for (int i = 0; i < 4; i++)
                    {
                        BarrelInfo barrel = new BarrelInfo();
                        GameObject muzzle_flash_copy = GameObject.Instantiate(muzzle_flash_go, zsu.GetChild(i).GetChild(0));

                        muzzle_flash_copy.transform.localPosition = new Vector3(0f, 0f, 0.12f);

                        barrel.MuzzleIdentity = zsu.GetChild(i);
                        barrel.MuzzleEffects = new ParticleSystem[] { muzzle_flash_copy.GetComponent<ParticleSystem>() };
                        barrel.RoundLoadedObject = new GameObject();
                        barrel.ImpulseLocation = vic.transform.Find("BMP2_rig/HULL/TURRET/Main gun");
                        barrels.Add(barrel);
                    }

                    main_gun.WeaponSound.SingleShotMode = true;
                    main_gun.WeaponSound.SingleShotByDefault = true;
                    main_gun._cycleTimeSeconds = 0.070f;
                    main_gun.Feed._totalCycleTime = 0.070f;
                    main_gun.CodexEntry = weapon_2a7m;
                    main_gun.MultiBarrels = barrels.ToArray();
                    main_gun.Impulse = 350f;
                    main_gun.BaseDeviationAngle = 0.13f;

                    //weapon.FCS.gameObject.AddComponent<LockOnLead>();

                    loadout_manager._weaponsManager.Weapons = new WeaponSystemInfo[] { loadout_manager._weaponsManager.Weapons[0] };
                    (vic.CrewManager.GetCrewBrain(CrewPosition.Gunner) as GunnerBrain).WeaponsModule.Weapons.RemoveRange(1, 2);
                    vic.transform.Find("BMP2_rig/HULL/TURRET/konkurs_azimuth").gameObject.SetActive(false);
                    vic.transform.Find("BMP2_rig/HULL/TURRET/konkurs_azimuth").localScale = new Vector3(0f, 0f, 0f);

                    vic.AimablePlatforms[1].LocalEulerLimits.x = -5f;

                    if (!super_fcs.Value)
                    {
                        day_optic.Alignment = OpticAlignment.FcsRange;
                        day_optic.ForceHorizontalReticleAlign = true;
                        day_optic.slot.LinkedNightSight.PairedOptic.Alignment = OpticAlignment.FcsRange;
                        day_optic.UseRotationForShake = false;
                        main_gun.FCS.MaxLaserRange = 4000f;
                        main_gun.FCS._currentRange = 200f;
                        main_gun.FCS.SuperleadWeapon = true;
                        main_gun.FCS.SuperelevateWeapon = true;
                        main_gun.FCS.TraverseBufferSeconds = 1f;
                        main_gun.FCS.RegisteredRangeLimits = new Vector2(200, 4000);
                        main_gun.FCS.RecordTraverseRateBuffer = true;
                        main_gun.FCS._autoDumpViaPalmSwitches = true;
                        main_gun.FCS.WeaponAuthoritative = false;
                        main_gun.FCS.InertialCompensation = false;
                        main_gun.FCS.LaserAim = LaserAimMode.ImpactPoint;

                        if (!reticleSO)
                        {
                            reticleSO = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["BMP-2_BPK-1-42"].tree);
                            reticleSO.name = "bmp2static";

                            Util.ShallowCopy(reticle_cached, ReticleMesh.cachedReticles["BMP-2_BPK-1-42"]);
                            reticle_cached.tree = reticleSO;

                            ReticleTree.Angular angular = (reticle_cached.tree.planes[0].elements[1] as ReticleTree.Angular);
                            angular.align = ReticleTree.GroupBase.Alignment.Impact;
                            angular.elements.RemoveRange(0, 3);
                            (angular.elements[1] as ReticleTree.Angular).align = ReticleTree.GroupBase.Alignment.Boresight;
                        }

                        day_optic.reticleMesh.reticleSO = reticleSO;
                        day_optic.reticleMesh.reticle = reticle_cached;
                        day_optic.reticleMesh.SMR = null;
                        day_optic.reticleMesh.Load();
                    }

                    vic._friendlyName = "BMP-23-4";

                    he = clip_codex_bzt;
                    ap = clip_codex_ofz;
                }

                if (has_lrf.Value && !is_zsu && !super_fcs.Value) {
                    day_optic.Alignment = OpticAlignment.FcsRange;
                    day_optic.ForceHorizontalReticleAlign = true;
                    day_optic.slot.LinkedNightSight.PairedOptic.Alignment = OpticAlignment.FcsRange;
                    day_optic.RotateAzimuth = true;
                    day_optic.slot.VibrationBlurScale = 0.01f;
                    day_optic.slot.VibrationShakeMultiplier = 0f;
                    day_optic.slot.DefaultFov = 9f;
                    day_optic.slot.OtherFovs = new float[] { 6f };
                    main_gun.FCS.MaxLaserRange = 4000f;
                    main_gun.FCS._currentRange = 200f;
                    main_gun.FCS.SuperleadWeapon = true;
                    main_gun.FCS.SuperelevateWeapon = true;
                    main_gun.FCS.TraverseBufferSeconds = 1f;
                    main_gun.FCS.RegisteredRangeLimits = new Vector2(200, 4000);
                    main_gun.FCS.RecordTraverseRateBuffer = true;
                    main_gun.FCS._autoDumpViaPalmSwitches = true;
                    main_gun.FCS.WeaponAuthoritative = false;
                    main_gun.FCS.InertialCompensation = false;
                    main_gun.FCS.LaserAim = LaserAimMode.ImpactPoint;
                    main_gun.FCS._fixParallaxForVectorMode = true;

                    night_optic.RotateAzimuth = true;

                    day_optic.reticleMesh.reticleSO = reticleSO_lrf ;
                    day_optic.reticleMesh.reticle = reticle_cached_lrf;
                    day_optic.reticleMesh.SMR = null;
                    day_optic.reticleMesh.Load();

                    if (!has_thermals.Value)
                    {
                        night_optic.reticleMesh.reticleSO = reticleSO_lrf;
                        night_optic.reticleMesh.reticle = reticle_cached_lrf;
                        night_optic.reticleMesh.SMR = null;
                        night_optic.reticleMesh.Load();
                    }
                }

                if (has_thermals.Value && !super_fcs.Value) {
                    PactThermal.Add(night_optic, thermals_quality.Value.ToLower(), is_point_n_shoot: true);
                    night_optic.slot.SpriteType = GHPC.Camera.CameraSpriteManager.SpriteType.DefaultScope;
                    turret_smr.sharedMesh = turret_only_thermals;
                    vic.InfraredSpotlights[0].GetComponent<Light>().gameObject.SetActive(false);
                    foreach (LightBandExclusiveItem lamp in vic._equipmentManager.AllLamps[0].Lamps)
                    {
                        lamp.gameObject.SetActive(false);
                    }
                }

                if (has_kornets.Value && !is_zsu)
                {
                    vic._friendlyName = "BMP-2M";

                    turret_smr.sharedMesh = bmp2m_turret;

                    turret.Find("konkurs_azimuth/konkurs_elevation/launcher_ramp/launcher_tube/konk001").gameObject.SetActive(false);

                    GameObject _bmp2m_kit = GameObject.Instantiate(bmp2m_kit, vis_turret);
                    _bmp2m_kit.transform.localEulerAngles = new Vector3(270f, 180f, 0f);
                    _bmp2m_kit.transform.parent = turret;
                    _bmp2m_kit.transform.localEulerAngles = new Vector3(0f, 90f, 0f);

                    Transform launcher = _bmp2m_kit.transform.Find("launcher");
                    launcher.parent = turret.Find("konkurs_azimuth/konkurs_elevation/launcher elevation");

                    Material mat = turret_smr.materials[0];
                    Transform smoke_launcher = _bmp2m_kit.transform.Find("bmp2_front_smokes");

                    for (int i = 0; i < smoke_launcher.childCount; i++)
                    {
                        Transform smoke = smoke_launcher.GetChild(i);
                        Material[] smokes_mat = smoke.GetComponent<MeshRenderer>().materials;
                        smokes_mat[0] = mat;
                        smoke.GetComponent<MeshRenderer>().materials = smokes_mat;
                    }

                    VehicleSmokeManager smoke_manager = vic.transform.Find("BMP2 -Smoke Launcher System").GetComponent<VehicleSmokeManager>();
                    for (int i = 0; i < 6; i++)
                    {
                        VehicleSmokeManager.SmokeSlot slot = smoke_manager._smokeSlots[i];
                        Transform smoke_cap = smoke_launcher.transform.GetChild(i + 1);
                        slot.DisplayBone = smoke_cap;
                        slot.ScaleBoneToZero = true;
                        slot.SpawnLocation.transform.SetParent(smoke_cap);
                        slot.SpawnLocation.transform.position = smoke_cap.GetComponent<Renderer>().bounds.center;
                    }

                    AimablePlatform[] new_mounts = new AimablePlatform[] { main_gun.FCS.Mounts[0], main_gun.FCS.Mounts[1], atgm.FCS.Mounts[1] };
                    main_gun.FCS.Mounts = new_mounts;
                    main_gun.FCS.LaserAim = LaserAimMode.ImpactPoint;
                    atgm.FCS.Mounts[0].enabled = false;
                    atgm.FCS.enabled = false;
                    atgm.FCS.gameObject.transform.SetParent(main_gun.FCS.transform, false);
                    atgm.GuidanceUnit.AimElement = main_gun.FCS.transform;

                    atgm_info.FCS = main_gun.FCS;
                    atgm_info.ExcludeFromFcsUpdates = false;
                    atgm.FCS = main_gun.FCS;
                    atgm.FCS.Mounts[2].StabilizerActive = true;
                    atgm.FCS.Mounts[2].Stabilized = true;
                    atgm.WireGuided = false;
                    atgm_info.Name = "Twin ATGM Launchers";
                    atgm._isMultiBarrel = true;
                    atgm.TriggerHoldTime = 0.35f;
                    atgm.TriggerAudioController.enabled = false;
                    atgm.TriggerAudioController.gameObject.SetActive(false);
                    atgm.FireWhileGuidingMissile = false;

                    atgm.Feed.RoundCycleStages[0].Duration = 40f;
                    AmmoFeed.ReloadStage[] temp = atgm.Feed.ClipReloadStages;
                    temp[0].Duration = 10f;
                    atgm.Feed.ClipReloadStages = new AmmoFeed.ReloadStage[] {
                        temp[0],
                        temp[0],
                        temp[0],
                        temp[0],
                    };
                    atgm.Feed._totalReloadTime = 40f;

                    GameObject missile_fx = vic.transform.Find("BMP2_rig/HULL/TURRET/konkurs_azimuth/konkurs_elevation/launcher elevation/Launcher 9P135M/Effects").gameObject;

                    MultiBarrelFix mbf = atgm.gameObject.AddComponent<MultiBarrelFix>();
                    mbf.feed = atgm.Feed;
                    mbf.max_ammo = 4;
                    mbf.loaded_objects = new GameObject[] {
                        launcher.Find("cap0").gameObject,
                        launcher.Find("cap1").gameObject,
                        launcher.Find("cap2").gameObject,
                        launcher.Find("cap3").gameObject
                    };

                    mbf.weapon = atgm;

                    missile_fx.transform.SetParent(launcher.Find("effect0"), false);
                    GameObject.Instantiate(missile_fx, launcher.Find("effect1"));
                    GameObject.Instantiate(missile_fx, launcher.Find("effect2"));
                    GameObject.Instantiate(missile_fx, launcher.Find("effect3"));

                    List<BarrelInfo> atgm_barrels = new List<BarrelInfo>() { };

                    for (int i = 0; i <= 3; i++)
                    {
                        BarrelInfo barrel_info = new BarrelInfo();
                        barrel_info.RoundLoadedObject = mbf.loaded_objects[i];
                        barrel_info.MuzzleIdentity = launcher.Find("cap" + i + "/muzzle");
                        barrel_info.MuzzleEffects = new ParticleSystem[] {
                        launcher.transform.Find("effect" + i).GetChild(0).Find("TOW Backblast FX").GetComponent<ParticleSystem>(),
                        launcher.transform.Find("effect" + i).GetChild(0).Find("TOW Front FX").GetComponent<ParticleSystem>()
                    };
                        atgm_barrels.Add(barrel_info);
                    }

                    atgm.MultiBarrels = atgm_barrels.ToArray();

                    day_optic.slot.ExclusiveWeapons = new WeaponSystem[] { main_gun, loadout_manager._weaponsManager.Weapons[2].Weapon, atgm };
                    main_gun.FCS.LinkedWeaponSystems = day_optic.slot.ExclusiveWeapons;

                    GHPC.Weapons.AmmoRack atgm_rack = atgm.Feed.ReadyRack;
                    atgm_rack.ClipTypes[0] = clip_9m133;
                    atgm_rack.StoredClips = new List<AmmoType.AmmoClip>()
                    {
                        clip_9m133,
                        clip_9m133,
                    };

                    atgm.Feed.AmmoTypeInBreech = null;
                    atgm.Feed.Start();
                }

                loadout_manager.LoadedAmmoList.AmmoClips[0] = ap;
                loadout_manager.LoadedAmmoList.AmmoClips[1] = he;

                GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[0].Rack;
                Util.EmptyRack(rack);

                loadout_manager.SpawnCurrentLoadout();
                main_gun.Feed.AmmoTypeInBreech = null;
                main_gun.Feed.LoadedClipType = null;
                main_gun.Feed.Start();
                loadout_manager.RegisterAllBallistics();
                
                if (use_9m113as.Value && !has_kornets.Value && !is_zsu)
                {
                    GHPC.Weapons.AmmoRack atgm_rack = atgm.Feed.ReadyRack;

                    atgm_rack.ClipTypes[0] = clip_9m113_as;
                    atgm_rack.StoredClips = new List<AmmoType.AmmoClip>()
                    {
                        clip_9m113_as,
                        clip_9m113_as,
                        clip_9m113_as,
                        clip_9m113_as,
                        clip_9m113_as,
                    };

                    atgm.Feed.AmmoTypeInBreech = null;
                    atgm.Feed.Start();
                }
            }

            yield break;
        }

        public static void LRFReticle() {
            reticleSO_lrf = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["BMP-2_BPK-1-42"].tree);
            reticleSO_lrf.name = "bmp2_lrf_ac";

            Util.ShallowCopy(reticle_cached_lrf, ReticleMesh.cachedReticles["BMP-2_BPK-1-42"]);
            reticle_cached_lrf.tree = reticleSO_lrf;

            reticleSO_lrf.lights = new List<ReticleTree.Light>
            {
                new ReticleTree.Light()
            };

            reticleSO_lrf.lights[0].color = new RGB(3f, 3f, 0f);
            reticleSO_lrf.lights[0].type = ReticleTree.Light.Type.Powered;

            ReticleTree.Angular angular = (reticle_cached_lrf.tree.planes[0].elements[1] as ReticleTree.Angular);
            ReticleTree.Angular angular2 = (reticle_cached_lrf.tree.planes[0].elements[0] as ReticleTree.Angular);

            angular2.elements.RemoveRange(0, 2);

            angular.align = ReticleTree.GroupBase.Alignment.Impact;
            angular.elements.RemoveRange(0, 3);
            angular.elements.RemoveAt(1);

            ReticleTree.Angular sight_picture = angular.elements[0] as ReticleTree.Angular;

            for (int i = 1; i < sight_picture.elements.Count; i++) {
                ReticleTree.Line line = (sight_picture.elements[i] as ReticleTree.Angular).elements[0] as ReticleTree.Line;
                line.length.mrad *= 0.66f;
                line.position.y *= 0.66f;
                line.roundness = 0f;
            }

            foreach (ReticleTree.Line line in (sight_picture.elements[0] as ReticleTree.Angular).elements) {
                line.length.mrad *= 0.66f;
                line.position.y *= 0.66f;
                line.position.x *= 0.66f;
            }

            for (int i = 1; i >= -1; i -= 2)
            {
                ReticleTree.Line l1 = new ReticleTree.Line();
                l1.thickness.mrad = 0.2094f;
                l1.roundness = 0f;
                l1.illumination = ReticleTree.Light.Type.Powered;
                l1.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;
                l1.rotation.mrad = 4712.389f;
                l1.length.mrad = 4f;
                l1.position = new ReticleTree.Position(-7.854f * i, -0.5896f);

                ReticleTree.Line l2 = new ReticleTree.Line();
                l2.thickness.mrad = 0.2094f;
                l2.roundness = 0f;
                l2.illumination = ReticleTree.Light.Type.Powered;
                l2.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;
                l2.length.mrad = 1f;
                l2.position = new ReticleTree.Position(-7.854f * i + 0.5f * i, -0.5896f + 2f);

                ReticleTree.Line l3 = new ReticleTree.Line();
                l3.thickness.mrad = 0.2094f;
                l3.roundness = 0f;
                l3.illumination = ReticleTree.Light.Type.Powered;
                l3.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;
                l3.length.mrad = 1f;
                l3.position = new ReticleTree.Position(-7.854f * i + 0.5f * i, -0.5896f - 2f);

                sight_picture.elements.Add(l1);
                sight_picture.elements.Add(l2);
                sight_picture.elements.Add(l3);
            }
        }


        public static void LoadAssets()
        {
            if (assets_loaded) return;

            AssetBundle bmp2m_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "bmp2m"));
            turret_no_smokes = bmp2m_bundle.LoadAsset<Mesh>("bmp2_no_smokes.asset");
            turret_no_smokes.hideFlags = HideFlags.DontUnloadUnusedAsset;

            bmp2m_turret = bmp2m_bundle.LoadAsset<Mesh>("bmp2m_turret.asset");
            bmp2m_turret.hideFlags = HideFlags.DontUnloadUnusedAsset;

            turret_only_thermals = bmp2m_bundle.LoadAsset<Mesh>("turret_thermals_only.asset");
            turret_only_thermals.hideFlags = HideFlags.DontUnloadUnusedAsset;

            bmp2m_kit = bmp2m_bundle.LoadAsset<GameObject>("BMP2M_KIT.prefab");
            bmp2m_kit.hideFlags = HideFlags.DontUnloadUnusedAsset;

            Util.SetupFLIRShaders(bmp2m_kit);

            ammo_9m113_as = new AmmoType();
            Util.ShallowCopy(ammo_9m113_as, Assets.ammo_9m113);
            ammo_9m113_as.Name = "9M113AS Konkurs";
            ammo_9m113_as.Category = AmmoType.AmmoCategory.Explosive;
            ammo_9m113_as.RhaPenetration = 10f;
            ammo_9m113_as.NoisePowerX = 0f;
            ammo_9m113_as.NoisePowerY = 0f;

            ammo_codex_9m113_as = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_9m113_as.AmmoType = ammo_9m113_as;
            ammo_codex_9m113_as.name = "ammo_9m113_as";

            clip_9m113_as = new AmmoType.AmmoClip();
            clip_9m113_as.Capacity = 1;
            clip_9m113_as.Name = "9M113AS Konkurs";
            clip_9m113_as.MinimalPattern = new AmmoCodexScriptable[] { ammo_codex_9m113_as };
            clip_9m113_as.MinimalPattern[0] = ammo_codex_9m113_as;

            clip_codex_9m113_as = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_9m113_as.name = "clip_9m113_as";
            clip_codex_9m113_as.ClipType = clip_9m113_as;

            AmmoType ammo_9m113_efp = new AmmoType();
            ammo_9m113_efp.Name = "9M113AS Konkurs EFP";
            ammo_9m113_efp.Category = AmmoType.AmmoCategory.Explosive;
            ammo_9m113_efp.RhaPenetration = 300f;
            ammo_9m113_efp.Normalize = true;
            ammo_9m113_efp.Mass = 3f;
            ammo_9m113_efp.TntEquivalentKg = 0.9f;
            ammo_9m113_efp.ImpactFuseTime = 0.005f;
            ammo_9m113_efp.SectionalArea = ammo_9m113_as.SectionalArea / 1.5f;

            EFP.AddEFP(ammo_9m113_as, ammo_9m113_efp, true);

            //////////////////////

            ammo_9m133 = new AmmoType();
            Util.ShallowCopy(ammo_9m133, Assets.ammo_9m113);
            ammo_9m133.Name = "9M133 Kornet";
            ammo_9m133.Category = AmmoType.AmmoCategory.ShapedCharge;
            ammo_9m133.RhaPenetration = 1200f;
            ammo_9m133.MuzzleVelocity = 300f;
            ammo_9m133.SpiralPower = 20f;
            ammo_9m133.TntEquivalentKg = 6.5f;
            ammo_9m133.SpallMultiplier = 2.3f;
            ammo_9m133._radius = 0.076f;
            ammo_9m133.TurnSpeed = 2f;
            ammo_9m133.SectionalArea = 0.018146f;
            ammo_9m133.Guidance = AmmoType.GuidanceType.Laser;

            ammo_codex_9m133 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_9m133.AmmoType = ammo_9m133;
            ammo_codex_9m133.name = "ammo_9m133";

            clip_9m133 = new AmmoType.AmmoClip();
            clip_9m133.Capacity = 4;
            clip_9m133.Name = "9M133 Kornet";
            clip_9m133.MinimalPattern = new AmmoCodexScriptable[] { ammo_codex_9m133 };
            clip_9m133.MinimalPattern[0] = ammo_codex_9m133;

            clip_codex_9m133 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_9m133.name = "clip_9m133";
            clip_codex_9m133.ClipType = clip_9m133;

            /////////////////////

            ammo_apds = new AmmoType();
            Util.ShallowCopy(ammo_apds, Assets.ammo_3ubr6);
            ammo_apds.Name = "23mm APDS-T";
            ammo_apds.Mass = 0.190f;
            ammo_apds.MuzzleVelocity = 1120f;
            ammo_apds.RhaPenetration = 70f;
            ammo_apds.Caliber = 23f;
            ammo_apds.Coeff = 0.012f;
            ammo_apds.SectionalArea = 0.0005f;

            ammo_codex_apds = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_apds.AmmoType = ammo_apds;
            ammo_codex_apds.name = "ammo_apds";

            ammo_bzt = new AmmoType();
            Util.ShallowCopy(ammo_bzt, Assets.ammo_3ubr6);
            ammo_bzt.Name = "23mm BZT";
            ammo_bzt.Mass = 0.190f;
            ammo_bzt.MuzzleVelocity = 970f;
            ammo_bzt.RhaPenetration = 40f;
            ammo_bzt.Caliber = 23f;
            ammo_bzt.Coeff = 0.012f;
            ammo_bzt.SectionalArea = 0.0005f;

            ammo_codex_bzt = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_bzt.AmmoType = ammo_bzt;
            ammo_codex_bzt.name = "ammo_bzt";

            clip_bzt = new AmmoType.AmmoClip();
            clip_bzt.Capacity = 165;
            clip_bzt.Name = "23mm APDS-T";
            clip_bzt.MinimalPattern = new AmmoCodexScriptable[] {
                ammo_codex_apds,
            };

            clip_codex_bzt = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_bzt.name = "clip_bzt";
            clip_codex_bzt.ClipType = clip_bzt;

            ////////////////////

            ammo_ofzt = new AmmoType();
            Util.ShallowCopy(ammo_ofzt, Assets.ammo_3uor6);
            ammo_ofzt.Name = "23mm OFZT";
            ammo_ofzt.UseTracer = true;
            ammo_ofzt.TntEquivalentKg = 0.020f;
            ammo_ofzt.MuzzleVelocity = 980f;
            ammo_ofzt.Caliber = 23f;
            ammo_ofzt.Coeff = 0.012f;
            ammo_ofzt.SectionalArea = 0.0005f;

            ammo_codex_ofzt = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_ofzt.AmmoType = ammo_ofzt;
            ammo_codex_ofzt.name = "ammo_ofzt";

            ammo_ofz = new AmmoType();
            Util.ShallowCopy(ammo_ofz, Assets.ammo_3uor6);
            ammo_ofz.Name = "23mm OFZ";
            ammo_ofz.UseTracer = false;
            ammo_ofz.TntEquivalentKg = 0.025f;
            ammo_ofz.MuzzleVelocity = 980f;
            ammo_ofz.Caliber = 23f;
            ammo_ofz.Coeff = 0.012f;
            ammo_ofz.SectionalArea = 0.0005f;

            ammo_codex_ofz = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_ofz.AmmoType = ammo_ofz;
            ammo_codex_ofz.name = "ammo_ofz";

            clip_ofz = new AmmoType.AmmoClip();
            clip_ofz.Capacity = 600;
            clip_ofz.Name = "23mm BZT/OFZ";
            clip_ofz.MinimalPattern = new AmmoCodexScriptable[] {
                ammo_codex_ofz,
                ammo_codex_bzt,
                ammo_codex_ofz,
                ammo_codex_bzt,
                ammo_codex_ofz,
            };

            clip_codex_ofz = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_ofz.name = "clip_ofz";
            clip_codex_ofz.ClipType = clip_ofz;

            GameObject rig = Assets.BRDM2.transform.Find("BRDM2_1983 (1)/BRDM_hull_1983").gameObject;
            SkinnedMeshRenderer rig_renderer = rig.GetComponent<SkinnedMeshRenderer>();

            brdm_nsv_barrel_mat = Material.Instantiate(rig_renderer.sharedMaterial);
            brdm_nsv_barrel_mesh = Mesh.Instantiate(rig_renderer.sharedMesh);
           
            zsu_barrel = new GameObject("zsu");
            Transform barrel_transform = GameObject.Instantiate(new GameObject("barrel"), zsu_barrel.transform).transform;
            SkinnedMeshRenderer rend = zsu_barrel.AddComponent<SkinnedMeshRenderer>();
            rend.sharedMaterial = brdm_nsv_barrel_mat;
            rend.sharedMesh = brdm_nsv_barrel_mesh;

            Transform[] bones = new Transform[25];
            for (int i = 0; i < 25; i++)
                bones[i] = barrel_transform;
            rend.bones = bones;

            Matrix4x4[] binds = new Matrix4x4[25];
            for (int i = 0; i < 25; i++)
                binds[i] = new Matrix4x4();
            binds[10] = new Matrix4x4(
                new Vector4(0f, 0.00055f, 1.31486f, 0f),
                new Vector4(1.31486f, 0.00021f, 0f, 0f),
                new Vector4(-0.00021f, 1.31486f, -0.00055f, 0f),
                new Vector4(0.00014f, -0.87824f, -1.38419f, 1f)
            );

            rend.sharedMesh.bindposes = binds;
            rend.rootBone = barrel_transform;

            zsu_full = new GameObject("zsu full");
            zsu_full.transform.localScale = new Vector3(1.4f, 1.4f, 1.3f);
            Vector3[] rots = new Vector3[] { new Vector3(0f, 0f, 45f), new Vector3(0f, 0f, -45f), new Vector3(0f, 0f, -135f), new Vector3(0f, 0f, 135f) };
            Vector3[] pos = new Vector3[] { Vector3.zero, new Vector3(-0.1491f, 0f, 0f), new Vector3(-0.1551f, 0.0836f, 0f), new Vector3(-0.005f, 0.0836f, 0f) };

            for (int i = 0; i < 4; i++)
            {
                GameObject barrel = GameObject.Instantiate(zsu_barrel, zsu_full.transform);
                barrel.transform.localEulerAngles = rots[i];
                barrel.transform.localPosition = pos[i];
            }

            weapon_2a7m = ScriptableObject.CreateInstance<WeaponSystemCodexScriptable>();
            weapon_2a7m.name = "gun_2a7m";
            weapon_2a7m.CaliberMm = 23;
            weapon_2a7m.FriendlyName = "23mm guns 2A7M";
            weapon_2a7m.Type = WeaponSystemCodexScriptable.WeaponType.Autocannon;

            LRFReticle();

            assets_loaded = true;
        }

        public static void Init()
        {
            if (!bmp2_patch.Value) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }
}