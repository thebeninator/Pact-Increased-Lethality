using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Equipment.Optics;
using GHPC.State;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using MelonLoader;
using Reticle;
using TMPro;
using HarmonyLib;
using UnityEngine;
using FMOD;
using FMODUnity;
using GHPC.Crew;

namespace PactIncreasedLethality
{
    public class BMP2
    {
        [HarmonyPatch(typeof(WeaponAudio), "FinalStartLoop")]
        public static class ReplaceSound
        {
            public static FMOD.Sound sound_exterior;
            public static FMOD.Sound sound;

            public static List<Channel> channels = new List<Channel>() { };
            public static void Cleanup() {
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
                if (__instance.SingleShotMode && __instance.SingleShotEventPaths[0] == "event:/Weapons/autocannon_2a42_single")
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

                    bool interior = __instance.IsInterior && __instance == PactIncreasedLethalityMod.player_manager.CurrentPlayerWeapon.Weapon.WeaponSound;

                    ChannelGroup channelGroup;
                    corSystem.createChannelGroup("master", out channelGroup);

                    channelGroup.setVolumeRamp(false);
                    channelGroup.setMode(MODE._3D_WORLDRELATIVE);

                    FMOD.Channel channel;
                    FMOD.Sound s = interior ? sound : sound_exterior;

                    Cleanup();

                    corSystem.playSound(s, channelGroup, true, out channel);
                    channels.Add(channel);

                    float game_vol = PactIncreasedLethalityMod.audio_settings_manager._previousVolume;
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

        static AmmoClipCodexScriptable clip_codex_3ubr8;
        static AmmoType.AmmoClip clip_3ubr8;
        static AmmoCodexScriptable ammo_codex_3ubr8;
        static AmmoType ammo_3ubr8;

        static AmmoClipCodexScriptable clip_codex_3uof8;
        static AmmoType.AmmoClip clip_3uof8;
        static AmmoCodexScriptable ammo_codex_3uof8;
        static AmmoType ammo_3uof8;

        static AmmoClipCodexScriptable clip_codex_9m113_as;
        static AmmoType.AmmoClip clip_9m113_as;
        static AmmoCodexScriptable ammo_codex_9m113_as;
        static AmmoType ammo_9m113_as;

        static AmmoType ammo_9m113; 

        static AmmoType ammo_3ubr6;
        static AmmoClipCodexScriptable clip_codex_3ubr6;

        static AmmoType ammo_3uor6;
        static AmmoCodexScriptable ammo_codex_3uor6;
        static AmmoClipCodexScriptable clip_codex_3uor6;

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
        static MelonPreferences_Entry<bool> zsu_conversion;
        static MelonPreferences_Entry<int> zsu_conversion_chance;

        static GameObject zsu_barrel;
        static GameObject zsu_full;
        static Material brdm_nsv_barrel_mat;
        static Mesh brdm_nsv_barrel_mesh;

        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        public static void Config(MelonPreferences_Category cfg)
        {
            bmp2_patch = cfg.CreateEntry<bool>("BMP-2 Patch", true);
            bmp2_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            use_3ubr8 = cfg.CreateEntry<bool>("Use 3UBR8", true);
            use_3ubr8.Comment = "Replaces 3UBR6; has improved penetration and better ballistics";

            use_3uof8 = cfg.CreateEntry<bool>("Use 3UOF8", true);
            use_3uof8.Comment = "Mixed belt of 3UOR6 and 3UOF8 (1:2); 3UOF8 has more explosive filler but no tracer";

            use_9m113as = cfg.CreateEntry<bool>("Use 9M113AS", false);
            use_9m113as.Comment = "Fictional overfly-top-attack ATGM with dual warhead; aim above target";

            zsu_conversion = cfg.CreateEntry<bool>("BMP-23-4 Conversion", false);
            zsu_conversion.Comment = "Quad 23mm autocannons with mixed BZT (API-T)/OFZ (HE) and APDS-T belts; laser rangefinder; single-axis stabilized automatic lead";

            zsu_conversion_chance = cfg.CreateEntry<int>("BMP-23-4 Conversion Chance", 30);
            zsu_conversion_chance.Comment = "Integer; default: 30%";
        }

        public static void Update() {
        {
            if (!zsu_conversion.Value) return;
            if (PactIncreasedLethalityMod.player_manager == null) return;
            ReplaceSound.Cleanup();
        }
    }

    public static IEnumerator Convert(GameState _)
        {
            foreach (GameObject vic_go in PactIncreasedLethalityMod.vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (vic.FriendlyName != "BMP-2") continue;
                if (vic.GetComponent<AlreadyConverted>() != null) continue;

                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                vic.gameObject.AddComponent<AlreadyConverted>();

                int rand = UnityEngine.Random.Range(1, 100);
                bool is_zsu = zsu_conversion.Value &&  rand <= zsu_conversion_chance.Value;

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
                    weapon._isMultiBarrel = true;

                    GameObject muzzle_flash_go = GameObject.Instantiate(weapon._muzzleEffects[0].gameObject);
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

                    weapon.WeaponSound.SingleShotMode = true;
                    weapon.WeaponSound.SingleShotByDefault = true;
                    weapon._cycleTimeSeconds = 0.070f;
                    weapon.Feed._totalCycleTime = 0.070f;
                    weapon.CodexEntry = weapon_2a7m;
                    weapon.MultiBarrels = barrels.ToArray();
                    weapon.Impulse = 350f;
                    weapon.BaseDeviationAngle = 0.13f;

                    //weapon.FCS.gameObject.AddComponent<LockOnLead>();

                    loadout_manager._weaponsManager.Weapons = new WeaponSystemInfo[] { loadout_manager._weaponsManager.Weapons[0] };
                    (vic.CrewManager.GetCrewBrain(CrewPosition.Gunner) as GunnerBrain).Weapons.RemoveRange(1, 2);
                    vic.transform.Find("BMP2_rig/HULL/TURRET/konkurs_azimuth").gameObject.SetActive(false);
                    vic.transform.Find("BMP2_rig/HULL/TURRET/konkurs_azimuth").localScale = new Vector3(0f, 0f, 0f); 

                    UsableOptic day_optic = Util.GetDayOptic(weapon.FCS);
                    vic.AimablePlatforms[1].LocalEulerLimits.x = -5f;

                    day_optic.Alignment = OpticAlignment.FcsRange;
                    day_optic.ForceHorizontalReticleAlign = true;
                    day_optic.slot.LinkedNightSight.PairedOptic.Alignment = OpticAlignment.FcsRange;
                    day_optic.UseRotationForShake = false;
                    weapon.FCS.MaxLaserRange = 4000f;
                    weapon.FCS._currentRange = 200f;
                    weapon.FCS.SuperleadWeapon = true;
                    weapon.FCS.SuperelevateWeapon = true;
                    weapon.FCS.TraverseBufferSeconds = 1f;
                    weapon.FCS.RegisteredRangeLimits = new Vector2(200, 4000);
                    weapon.FCS.RecordTraverseRateBuffer = true;
                    weapon.FCS._autoDumpViaPalmSwitches = true;
                    weapon.FCS.WeaponAuthoritative = false;
                    weapon.FCS.InertialCompensation = false;
                    weapon.FCS.LaserAim = LaserAimMode.ImpactPoint;

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

                    vic._friendlyName = "BMP-23-4";
                }

                AmmoClipCodexScriptable ap = use_3ubr8.Value ? clip_codex_3ubr8 : clip_codex_3ubr6;
                AmmoClipCodexScriptable he = use_3uof8.Value ? clip_codex_3uof8 : clip_codex_3uor6;

                if (is_zsu) {
                    he = clip_codex_bzt;
                    ap = clip_codex_ofz;
                }

                loadout_manager.LoadedAmmoTypes = new AmmoClipCodexScriptable[] { ap, he };

                GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[0].Rack;
                loadout_manager.RackLoadouts[0].OverrideInitialClips = new AmmoClipCodexScriptable[] { ap, he };
                rack.ClipTypes = new AmmoType.AmmoClip[] { ap.ClipType, he.ClipType };
                Util.EmptyRack(rack);

                loadout_manager.SpawnCurrentLoadout();
                weapon.Feed.AmmoTypeInBreech = null;
                weapon.Feed.LoadedClipType = null;
                weapon.Feed.Start();
                loadout_manager.RegisterAllBallistics();

                if (use_9m113as.Value) {
                    WeaponSystem atgm = vic.GetComponent<WeaponsManager>().Weapons[1].Weapon;
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

        public static void Init()
        {
            if (!bmp2_patch.Value) return;

            if (zsu_conversion.Value && brdm_nsv_barrel_mat == null)
            {
                foreach (Vehicle s in Resources.FindObjectsOfTypeAll<Vehicle>())
                {
                    if (s.name != "BRDM2") continue;

                    GameObject rig = s.transform.Find("BRDM2_1983 (1)/BRDM_hull_1983").gameObject;
                    SkinnedMeshRenderer rig_renderer = rig.GetComponent<SkinnedMeshRenderer>();

                    brdm_nsv_barrel_mat = Material.Instantiate(rig_renderer.sharedMaterial);
                    brdm_nsv_barrel_mesh = Mesh.Instantiate(rig_renderer.sharedMesh);

                    break;
                }

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
            }

            if (ammo_3ubr8 == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "3UBR6 APBC-T") { 
                        ammo_3ubr6 = s.AmmoType;
                    }

                    if (s.AmmoType.Name == "3UOR6 HE-T") { 
                        ammo_3uor6 = s.AmmoType;
                        ammo_codex_3uor6 = s; 
                    }

                    if (s.AmmoType.Name == "9M113 Konkurs")
                    {
                        ammo_9m113 = s.AmmoType;
                    }

                    if (ammo_3ubr6 != null && ammo_3uor6 != null && ammo_9m113 != null) break; 
                }

                foreach (AmmoClipCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoClipCodexScriptable)))
                {
                    if (s.name == "clip_3UOR6_340rd_load") { clip_codex_3uor6 = s; }
                    if (s.name == "clip_3UBR6_160rd_load") { clip_codex_3ubr6 = s; }
                    if (clip_codex_3ubr6 != null && clip_codex_3uor6 != null) break;
                }

                ammo_3ubr8 = new AmmoType();
                Util.ShallowCopy(ammo_3ubr8, ammo_3ubr6);
                ammo_3ubr8.Name = "3UBR8 APDS-T";
                ammo_3ubr8.Mass = 0.222f;
                ammo_3ubr8.Coeff = 0.012f;
                ammo_3ubr8.MuzzleVelocity = 1120f;
                ammo_3ubr8.RhaPenetration = 72f;

                ammo_codex_3ubr8 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3ubr8.AmmoType = ammo_3ubr8;
                ammo_codex_3ubr8.name = "ammo_3ubr8";

                clip_3ubr8 = new AmmoType.AmmoClip();
                clip_3ubr8.Capacity = 160;
                clip_3ubr8.Name = "3UBR8 APDS-T";
                clip_3ubr8.MinimalPattern = new AmmoCodexScriptable[1];
                clip_3ubr8.MinimalPattern[0] = ammo_codex_3ubr8;

                clip_codex_3ubr8 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3ubr8.name = "clip_3ubr8";
                clip_codex_3ubr8.ClipType = clip_3ubr8;

                /////////////////////

                ammo_3uof8 = new AmmoType();
                Util.ShallowCopy(ammo_3uof8, ammo_3uor6);
                ammo_3uof8.Name = "3UOF8 HEFI";
                ammo_3uof8.UseTracer = false;
                ammo_3uof8.TntEquivalentKg = 0.049f;

                ammo_codex_3uof8 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3uof8.AmmoType = ammo_3uof8;
                ammo_codex_3uof8.name = "ammo_3uof8";

                clip_3uof8 = new AmmoType.AmmoClip();
                clip_3uof8.Capacity = 340;
                clip_3uof8.Name = "3UOR6 HE-T/3UOF8 HEFI";
                clip_3uof8.MinimalPattern = new AmmoCodexScriptable[] {
                    ammo_codex_3uof8,
                    ammo_codex_3uof8,
                    ammo_codex_3uor6,
                };
                clip_3uof8.MinimalPattern[0] = ammo_codex_3uof8;

                clip_codex_3uof8 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3uof8.name = "clip_3uof8";
                clip_codex_3uof8.ClipType = clip_3uof8;

                /////////////////////

                ammo_9m113_as = new AmmoType();
                Util.ShallowCopy(ammo_9m113_as, ammo_9m113);
                ammo_9m113_as.Name = "9M113AS Konkurs";
                ammo_9m113_as.Category = AmmoType.AmmoCategory.Explosive;
                ammo_9m113_as.RhaPenetration = 10f;
                ammo_9m113_as.NoisePowerX = 1f;
                ammo_9m113_as.NoisePowerY = 1f;

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

                ////////////////////
                
                ammo_apds = new AmmoType();
                Util.ShallowCopy(ammo_apds, ammo_3ubr6);
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
                Util.ShallowCopy(ammo_bzt, ammo_3ubr6);
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
                Util.ShallowCopy(ammo_ofzt, ammo_3uor6);
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
                Util.ShallowCopy(ammo_ofz, ammo_3uor6);
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
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }    
}