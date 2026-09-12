using GHPC.State;
using ModUtil;
using System.Collections;
using GHPC.Vehicle;
using UnityEngine;
using MelonLoader.Utils;
using System.IO;
using System.Linq;
using GHPC;
using GHPC.Weapons;
using GHPC.Equipment.Optics;
using System.Collections.Generic;
using GHPC.Mission;
using CustomVicUtil;
using System;
using NWH.VehiclePhysics;
using GHPC.Camera;
using GHPC.Player;
using GHPC.Utility;
using UnityEngine.AddressableAssets;
using CustomVicUnityScripts;
using MelonLoader;

namespace PactIncreasedLethality
{
    public class BMP3 : ModUtil.Module
    {
        private static GameObject bmp3_prefab;

        private static readonly string[] bmp2_turret_lf_retained = new string[]
        {
            "AmmoRack_2000X7.62mm",
            "AmmoRack_340X30mmHE",
            "AmmoRack_160X30mmAP",
            "AmmoRack_2000X7.62mm vis",
            "AmmoRack_340X30mmHE vis",
            "AmmoRack_160X30mmAP vis",
        };

        private static readonly string[] bmp2_turret_lf_objects = new string[]
        {
            "Gunner_Seat",
            "Commander_Seat",
            "Turret_TraverseMotor_PartOf2E36-1Stabilizer",
            "Turret_ManualDrive",
            "Gun_Elevation_ManualDrive",
            "Gun_Elevation_Motor_PartOf2E36-1Stabilizer",
            "Gunner_ElectronicControls",
            "Commander_ElectronicControls",
        };

        private static readonly AmmoFeed.ReloadStage[] bmp3_reload_sequence = new AmmoFeed.ReloadStage[]
        {
            new AmmoFeed.ReloadStage()
            {
                Duration = 0.4f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[] {}
            },
            new AmmoFeed.ReloadStage()
            {
                Duration = 0.4f,
                WaitForCarousel = true,
                LessWaitSeconds = 0.2f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[] {}
            },
            new AmmoFeed.ReloadStage()
            {
                Duration = 0.2f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[]
                {
                    new AmmoFeed.ReloadStage.FMODParameter()
                    {
                        Name = "WaitForCarousel",
                        Value = 0
                    }
                }
            },
            new AmmoFeed.ReloadStage()
            {
                Duration = 2f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[] {}
            },
            new AmmoFeed.ReloadStage()
            {
                Duration = 0.9f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[] {}
            },
        };

        private static readonly Action<GameObject, Vehicle> on_spawned = (GameObject instance, Vehicle original_vic) =>
        {
            TrackedWheelNodeConfig wheel_node_cfg = original_vic.transform.Find("WheelControllers").GetComponent<TrackedWheelNodeConfig>();
            Transform wheel_arms = instance.transform.Find("RIG/HULL/wheel arms");
            Transform track_nodes = instance.transform.Find("RIG/HULL/tracks");

            float[] wheel_z = new float[]
            {
                1.019085f,
                0.2451292f,
                -0.6806521f,
                -1.473438f,
                -2.177661f,
                -3.055225f
            };

            for (int i = 0; i < 12; i++)
            {
                Transform arm = wheel_arms.GetChild(i);
                wheel_node_cfg.SwingArms[i] = arm.gameObject;
                wheel_node_cfg.VisualNodes[i] = arm.GetChild(0).GetChild(0).gameObject;
                wheel_node_cfg.TrackNodes[i] = track_nodes.GetChild(i).gameObject;

                int right_side = -1 * (i >= 6 ? 1 : -1);
                wheel_node_cfg.transform.GetChild(i).transform.localPosition = new Vector3(-1.208f * right_side, 0.771f, wheel_z[i % 6]);
            }
        };  

        private static void HandleConversion(Vehicle vic)
        {
            if (vic == null) return;
            if (vic.UniqueName != "PIL_BMP3") return;
            //if (!vic.gameObject.name.Contains("BMP3")) return;

            vic._friendlyName = "BMP-3";

            Transform bmp2_rig = vic.transform.Find("BMP2_rig");
            Transform bmp2_hull = bmp2_rig.Find("HULL");
            Transform bmp2_turret = bmp2_hull.Find("TURRET");

            LateFollowTarget bmp2_turret_lft = bmp2_turret.GetComponent<LateFollowTarget>();
            LateFollowTarget bmp2_hull_lft = vic.GetComponent<LateFollowTarget>();
            LateFollowTarget bmp2_mantlet_lft = bmp2_turret.Find("Main gun").GetComponent<LateFollowTarget>();
            LateFollowTarget bmp2_tc_ring_lft = bmp2_turret.Find("TC ring").GetComponent<LateFollowTarget>();

            IList<LateFollow> bmp2_hull_late_followers = bmp2_hull_lft._lateFollowers;
            IList<LateFollow> bmp2_turret_late_followers = bmp2_turret_lft._lateFollowers;
            IList<LateFollow> bmp2_mantlet_late_followers = bmp2_mantlet_lft._lateFollowers;
            IList<LateFollow> bmp2_tc_ring_late_followers = bmp2_tc_ring_lft._lateFollowers;

            Transform bmp2_tc_ring_aar = bmp2_turret.Find("TC ring/CUPOLA");
            Transform bmp2_tc_ring_aar_armour = bmp2_tc_ring_late_followers.Where(o => o.name == "CUPOLA").First().transform;

            Transform bmp2_mantlet_aar = bmp2_turret.Find("Main gun/GUN");
            Transform bmp2_mantlet_aar_armour = bmp2_mantlet_late_followers[0].transform;

            Transform bmp2_turret_aar = bmp2_turret.Find("TURRET");
            Transform bmp2_turret_aar_armour = bmp2_turret_late_followers.Where(o => o.name == "TURRET").First().transform;

            Transform bmp3 = vic.transform.Find("bempeh3(Clone)");
            Transform bmp3_hull = bmp3.transform.Find("RIG/HULL");
            Transform bmp3_turret = bmp3_hull.transform.Find("TURRET");
            Transform bmp3_mantlet = bmp3_turret.transform.Find("MANTLET");

            Transform bmp3_turret_follower = bmp3_turret.Find("turret late follow");
            Transform bmp3_mantlet_follower = bmp3_turret.Find("mantlet late follow");
            Transform bmp3_hull_follower = bmp3_hull.Find("hull late follow");

            LateFollowTarget bmp3_hull_lft = bmp3_hull.gameObject.AddComponent<LateFollowTarget>();
            LateFollow bmp3_hull_lf = bmp3_hull_follower.gameObject.AddComponent<LateFollow>();
            bmp3_hull_lf.FollowTarget = bmp3_hull;
            bmp3_hull_lf.ForceToRoot = true;
            bmp3_hull_lf.enabled = true;
            bmp3_hull_lf.Awake();

            LateFollowTarget bmp3_turret_lft = bmp3_turret.gameObject.AddComponent<LateFollowTarget>();
            LateFollow bmp3_turret_lf = bmp3_turret_follower.gameObject.AddComponent<LateFollow>();
            bmp3_turret_lf.FollowTarget = bmp3_turret;
            bmp3_turret_lf.ForceToRoot = true;
            bmp3_turret_lf.enabled = true;
            bmp3_turret_lf.Awake();

            LateFollowTarget bmp3_mantlet_lft = bmp3_mantlet.gameObject.AddComponent<LateFollowTarget>();
            LateFollow bmp3_mantlet_lf = bmp3_mantlet_follower.gameObject.AddComponent<LateFollow>();
            bmp3_mantlet_lf.FollowTarget = bmp3_mantlet;
            bmp3_mantlet_lf.ForceToRoot = true;
            bmp3_mantlet_lf.enabled = true;
            bmp3_mantlet_lf.Awake();

            vic.transform.Find("BMP2_visual").gameObject.SetActive(false);
            vic.transform.Find("BMP2_markings").gameObject.SetActive(false);
            bmp2_hull.Find("numbers").gameObject.SetActive(false);
            bmp2_turret.Find("convoylight_002").gameObject.SetActive(false);
            bmp2_turret.Find("tactical marker").gameObject.SetActive(false);
            bmp2_turret.Find("tactical marker").gameObject.SetActive(false);
            bmp2_turret.Find("konkurs_azimuth").gameObject.SetActive(false);
            bmp2_turret.Find("turret scripts/R123_Prefab").gameObject.SetActive(false);

            bmp2_turret.Find("fire control").SetParent(bmp3_turret);
            bmp2_turret.Find("turret scripts").SetParent(bmp3_turret);
            bmp2_turret.Find("Main gun/mantlet scripts").SetParent(bmp3_turret.Find("MANTLET"));

            AimablePlatform turret_platform = bmp3_turret.Find("turret scripts").GetComponent<AimablePlatform>();
            turret_platform.Transform = bmp3_turret;

            AimablePlatform mantlet_platform = bmp3_turret.Find("MANTLET/mantlet scripts").GetComponent<AimablePlatform>();
            mantlet_platform.Transform = bmp3_turret.Find("MANTLET");

            Util.Reposition
            (
                target: bmp2_turret_lft.LateFollowers[1].transform.Find("COMMANDER"),
                to: bmp3_turret_follower.Find("commander marker"),
                delete_to: true
            );

            Util.Reposition
            (
                target: bmp2_turret_lft.LateFollowers[1].transform.Find("GUNNER"),
                to: bmp3_turret_follower.Find("gunner marker"),
                delete_to: true
            );

            Util.Reposition
            (
                target: vic.transform.Find("DRIVER"),
                to: bmp3.Find("driver marker"),
                delete_to: true
            );

            Util.Reposition
            (
                target: vic.DesignatedCameraSlots.Where(o => o.name == "commander head").First().transform,
                to: bmp3_turret.transform.Find("commander head"),
                delete_to: true
            );

            Util.Reposition
            (
                targets: new Transform[] 
                { 
                    bmp2_turret.Find("gunner day sight 1P3-3"), 
                    bmp2_turret.Find("gunner night sight 1P3-3") 
                },
                to: bmp3_turret.transform.Find("gps"),
                delete_to: true
            );

            Util.Reposition
            (
                targets: new Transform[] 
                { 
                    bmp3_turret.Find("MANTLET/mantlet scripts/30mm Gun 2A42"), 
                    bmp2_turret.Find("Main gun/Muzzle identity") 
                },
                to: bmp3_turret.transform.Find("MANTLET/2a72 muzzle identity"),
                delete_to: true
            );

            Util.Reposition
            (
                target: bmp3_turret.Find("MANTLET/mantlet scripts/7.62mm Machine Gun PKT"),
                to: bmp3_turret.transform.Find("MANTLET/pkt muzzle identity"),
                delete_to: true
            );

            Util.Reposition
            (
                target: bmp2_turret.Find("konkurs_azimuth/konkurs_elevation/launcher elevation/Launcher 9P135M"),
                to: bmp3_turret.transform.Find("MANTLET/2a70 muzzle identity"),
                delete_to: true
            );

            Util.Reposition
            (
                targets: new Transform[]
                {
                    bmp2_mantlet_aar.Find("MG_Coax_PKT_7.62mm"),
                    bmp2_mantlet_aar_armour.Find("MG_Coax_PKT_7.62mm")
                },
                to: bmp3_mantlet_follower.Find("MG_Coax_PKT_7.62mm marker"),
                delete_to: true
            );

            Util.Reposition
            (
                targets: new Transform[]
                {
                    bmp2_tc_ring_aar.Find("Commander_Periscope_TKN-3B_001"),
                    bmp2_tc_ring_aar_armour.Find("Commander_Periscope_TKN-3B_001")
                },
                to: bmp3_turret_follower.Find("Commander_Periscope_TKN-3B_001 marker"),
                delete_to: true
            );

            foreach (string part_id in bmp2_turret_lf_objects)
            {
                Util.Reposition
                (
                    targets: new Transform[]
                    {
                        bmp2_turret_aar.Find(part_id),
                        bmp2_turret_aar_armour.Find(part_id)
                    },
                    to: bmp3_turret_follower.Find(part_id + " marker"),
                    delete_to: true
                );
            }

            LateFollow bmp2_turret_aar_armour_lf = bmp2_turret_aar_armour.GetComponent<LateFollow>();
            bmp2_turret_late_followers.Remove(bmp2_turret_aar_armour_lf);
            Component.DestroyImmediate(bmp2_turret_aar_armour_lf);

            Util.Reposition
            (
                targets: new Transform[]
                {
                    bmp2_turret_aar,
                    bmp2_turret_aar_armour
                },
                to: bmp3_turret_follower.Find("TURRET marker"),
                delete_to: true
            );

            bool turret_search_non_retained(Transform o)
            {
                return o.name != "TURRET" && !bmp2_turret_lf_retained.Contains(o.name);
            }

            void destroy_followers(IList<LateFollow> followers)
            {
                foreach (LateFollow f in followers)
                {
                    GameObject.DestroyImmediate(f.gameObject);
                }

                followers.Clear();
            }

            List<Transform> bmp2_turret_aar_delete = bmp2_turret_aar.GetComponentsInChildren<Transform>()
                .Where(turret_search_non_retained).ToList();

            List<Transform> bmp2_turret_aar_armour_delete = bmp2_turret_aar_armour.GetComponentsInChildren<Transform>()
                .Where(turret_search_non_retained).ToList();

            List<Transform> bmp2_turret_delete_combined = new List<Transform>(bmp2_turret_aar_delete);
            bmp2_turret_delete_combined.AddRange(bmp2_turret_aar_armour_delete);

            foreach (Transform t in bmp2_turret_delete_combined)
            {
                if (t != null)
                {
                    GameObject.DestroyImmediate(t.gameObject);
                }
            }

            destroy_followers(bmp2_hull_late_followers);
            destroy_followers(bmp2_mantlet_late_followers);
            destroy_followers(bmp2_turret_late_followers);
            destroy_followers(bmp2_tc_ring_late_followers);

            GameObject.DestroyImmediate(vic.transform.Find("BMP2 AAR visuals only").gameObject);
            GameObject.DestroyImmediate(bmp2_tc_ring_aar.gameObject);
            GameObject.DestroyImmediate(bmp2_mantlet_aar.gameObject);

            List<CameraSlot> designated_camera_slots_temp = vic._designatedCameraSlots.ToList();
            designated_camera_slots_temp.RemoveAt(2);
            vic._designatedCameraSlots = designated_camera_slots_temp.ToArray();

            if (vic.GetInstanceID() == PlayerInput.Instance.CurrentPlayerUnit.GetInstanceID())
            {
                CameraManager.Instance.RescanCamSlots(vic._designatedCameraSlots);
            }

            WeaponSystemInfo ws_gun_2a70 = vic.LoadoutManager._weaponsManager.GetWeaponInfoByRole(WeaponSystemRole.MountedLauncher);
            WeaponSystem wpn_gun_2a70 = ws_gun_2a70.Weapon;

            WeaponSystemInfo ws_gun_30_2a72 = vic.LoadoutManager._weaponsManager.GetWeaponInfoByRole(WeaponSystemRole.MainGun);
            WeaponSystem wpn_gun_30_2a72 = ws_gun_30_2a72.Weapon;

            FireControlSystem fcs = wpn_gun_30_2a72.FCS;
            UsableOptic day_optic = Util.GetDayOptic(fcs);

            Transform autoloader_carousel = bmp3_turret_follower.Find("autoloader/carousel");

            GHPC.Weapons.AmmoRack gun_2a70_rack = wpn_gun_2a70.Feed.ReadyRack;
            gun_2a70_rack.ClipTypes[0] = Ammo_100mm.clip_3of70;
            gun_2a70_rack.ClipCapacity = 22;
            gun_2a70_rack.UseVisibleRounds = true;

            for (int i = 0; i < 22; i++)
            {
                gun_2a70_rack.VisualSlots.Add(autoloader_carousel.GetChild(i));
                gun_2a70_rack.AddVisibleClip(i, Ammo_100mm.clip_3of70, false);
            }

            AmmoCarousel ammo_carousel = bmp3_turret_follower.Find("autoloader").gameObject.AddComponent<AmmoCarousel>();
            ammo_carousel.RotationTransform = autoloader_carousel;
            ammo_carousel.Mode = AmmoCarousel.RotationMode.Bidirectional;
            ammo_carousel.Acceleration = 50f;
            ammo_carousel.RotationSpeedDegreesPerSecond = 70f;
            ammo_carousel.Rack = wpn_gun_2a70.Feed.ReadyRack;
            ammo_carousel.Feed = wpn_gun_2a70.Feed;
            ammo_carousel.Capacity = 22;
            ammo_carousel.enabled = true;

            day_optic.slot.ExclusiveWeapons = new WeaponSystem[] { };
            day_optic.slot.LinkedNightSight.ExclusiveWeapons = new WeaponSystem[] { };
            day_optic.Alignment = OpticAlignment.BoresightStabilized;
            day_optic.RotateAzimuth = true;

            fcs.WeaponAuthoritative = false;
            fcs.MaxLaserRange = 4000f;
            fcs.LaserAim = LaserAimMode.ImpactPoint;
            fcs.SuperelevateWeapon = true;
            List<WeaponSystem> temp_linked = fcs.LinkedWeaponSystems.ToList();
            temp_linked.Add(wpn_gun_2a70);
            fcs.LinkedWeaponSystems = temp_linked.ToArray();

            ws_gun_2a70.Name = "100mm cannon 2A70";
            ws_gun_2a70.FCS = fcs;
            ws_gun_2a70.ExcludeFromFcsUpdates = false;
            wpn_gun_2a70._muzzleIdentity = wpn_gun_2a70.transform;
            wpn_gun_2a70.FCS = fcs;
            wpn_gun_2a70.TriggerAudioController = null;
            wpn_gun_2a70.WireGuided = false;
            wpn_gun_2a70.TriggerHoldTime = 0f;
            wpn_gun_2a70.WeaponSound.SingleShotEventPaths[0] = "event:/Weapons/canon_105mm-L7";
            wpn_gun_2a70.Impulse = 3500f;
            wpn_gun_2a70._impulseLocation = bmp3_turret.Find("MANTLET");
            wpn_gun_2a70.BaseDeviationAngle = 0.060f;
            wpn_gun_2a70.Feed._clipReloadFMODEvent = "event:/Effects/Reload/MZ_Autoloader";
            wpn_gun_2a70.Feed.Carousel = ammo_carousel;
            wpn_gun_2a70.Feed.ClipReloadStages = bmp3_reload_sequence;
            wpn_gun_2a70.Feed.HumanLoaded = false;
            wpn_gun_2a70.Feed.AmmoTypeInBreech = null;
            wpn_gun_2a70.Feed.Start();

            ws_gun_30_2a72.Name = "30mm gun 2A72";
            wpn_gun_30_2a72.CodexEntry = null;
            wpn_gun_30_2a72.BaseDeviationAngle = 0.155f;
            wpn_gun_30_2a72._cycleTimeSeconds = 0.16f;
            wpn_gun_30_2a72.Feed._totalCycleTime = 0.16f;
            wpn_gun_30_2a72.WeaponSound.SingleShotByDefault = true;
            wpn_gun_30_2a72.WeaponSound.SingleShotMode = true;
            wpn_gun_30_2a72.WeaponSound.SingleShotEventPaths = new string[] { "actually_2a72" };

            NwhChassis chassis = vic.GetComponent<NwhChassis>();
            chassis._maxForwardSpeed = 21f;
            chassis._maxReverseSpeed = 6.3f;

            VehicleController vic_controller = vic.GetComponent<VehicleController>();
            vic_controller.engine.power = 500f;
            vic_controller.engine.maxRPM = 5000f;
        }

        private static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                HandleConversion(vic);
            }

            yield break;
        }

        public override void LoadStaticAssets()
        {
            //bmp3_material = new Material(Shader.Find("GHPC/VehicleShader"));
            //bmp3_material.name = "bmp3";
            AssetBundle bmp3_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "bmp3"));

            Texture bmp3_albedo = bmp3_bundle.LoadAsset<Texture>("bmp3 albedo.TGA");
            Texture bmp3_occlusion = bmp3_bundle.LoadAsset<Texture>("bmp3 ao.png");
            Texture bmp3_normal = bmp3_bundle.LoadAsset<Texture>("bmp3 normal.TGA");
            Texture bmp3_sm = bmp3_bundle.LoadAsset<Texture>("bmp3 sm.png");

            Texture bmp3_track_albedo = bmp3_bundle.LoadAsset<Texture>("bmp3 track albedo.png");
            Texture bmp3_track_occlusion = bmp3_bundle.LoadAsset<Texture>("bmp3 track ao.png");
            Texture bmp3_track_normal = bmp3_bundle.LoadAsset<Texture>("bmp3 track normal.TGA");
            Texture bmp3_track_sm = bmp3_bundle.LoadAsset<Texture>("bmp3 track sm.png");

            bmp3_prefab = bmp3_bundle.LoadAsset<GameObject>("bempeh3.prefab");
            bmp3_prefab.hideFlags = HideFlags.DontUnloadUnusedAsset;

            Transform autoloader_carousel = bmp3_prefab.transform.Find("RIG/HULL/TURRET/turret late follow/autoloader/carousel");
            for (int i = 0; i < 22; i++)
            {
                AmmoVisualPlaceholder placeholder = autoloader_carousel.GetChild(i).gameObject.AddComponent<AmmoVisualPlaceholder>();
                placeholder.RackIndex = i;
            }

            Transform hull_ammo_rack = bmp3_prefab.transform.Find("RIG/HULL/hull late follow/hull rack");
            for (int i = 0; i < 18; i++)
            {
                AmmoVisualPlaceholder placeholder = hull_ammo_rack.GetChild(i).gameObject.AddComponent<AmmoVisualPlaceholder>();
                placeholder.RackIndex = i;
            }

            Transform wheel_arms = bmp3_prefab.transform.Find("RIG/HULL/wheel arms");

            Transform[] wheel_arms_transforms = new Transform[12];
            for (int i = 0; i < 12; i++)
            {
                wheel_arms_transforms[i] = wheel_arms.GetChild(i);
            }

            for (int i = 0; i < 12; i++)
            {
                Transform arm = wheel_arms_transforms[i];
                GameObject rotator = new GameObject();
                rotator.name = "rotator " + arm.name;
                rotator.transform.SetParent(wheel_arms);
                rotator.transform.position = arm.position;
                rotator.transform.localEulerAngles = new Vector3(-213.965f, 0f, 180f);
                arm.SetParent(rotator.transform);
            }

            Helpers.ProcessArmourScripts(bmp3_prefab.transform.Find("RIG/HULL/hull late follow/hull armour"));
            Helpers.ProcessArmourScripts(bmp3_prefab.transform.Find("RIG/HULL/TURRET/turret late follow/turret armour"));
            Helpers.ProcessArmourScripts(bmp3_prefab.transform.Find("RIG/HULL/TURRET/mantlet late follow/mantlet armour"));

            Material bmp3_material = Resources.FindObjectsOfTypeAll<Material>().Where(o => o.name == "MI_East_IFV_BMP3_01").First();
            bmp3_material.shader = Shader.Find("GHPC/VehicleShader");
            bmp3_material.EnableKeyword("_METALLICGLOSSMAP");
            bmp3_material.EnableKeyword("_NORMALMAP");
            bmp3_material.EnableKeyword("_ALPHATEST_ON");
            bmp3_material.SetTexture("_Albedo", bmp3_albedo);
            bmp3_material.SetTexture("_Occlusion", bmp3_occlusion);
            bmp3_material.SetTexture("_Normal", bmp3_normal);
            bmp3_material.SetTexture("_Smoothness", bmp3_sm);

            Material bmp3_track_material = Resources.FindObjectsOfTypeAll<Material>().Where(o => o.name == "MI_East_IFV_BMP3_02").First();
            bmp3_track_material.shader = Shader.Find("TrackShader");
            bmp3_track_material.EnableKeyword("_METALLICGLOSSMAP");
            bmp3_track_material.EnableKeyword("_NORMALMAP");
            bmp3_track_material.EnableKeyword("_ALPHATEST_ON");
            bmp3_track_material.SetTexture("_Colour", bmp3_track_albedo);
            bmp3_track_material.SetTexture("_MainTex", bmp3_track_albedo);
            bmp3_track_material.SetTexture("_Occlusion", bmp3_track_occlusion);
            bmp3_track_material.SetTexture("_Normal", bmp3_track_normal);
            bmp3_track_material.SetTexture("_Smoothness", bmp3_track_sm);

            UnitPrefabLookupScriptable unit_prefab_lookup = Resources.FindObjectsOfTypeAll<UnitPrefabLookupScriptable>().First();
            List<UnitPrefabLookupScriptable.UnitPrefabMetadata> all_units_list = unit_prefab_lookup.AllUnits.ToList();

            UnitPrefabLookupScriptable.UnitPrefabMetadata bmp2_metadata = all_units_list.Where(o => o.Name == "BMP2_SA").First();

            all_units_list.Add(new UnitPrefabLookupScriptable.UnitPrefabMetadata()
            {
                AllowInCustomizer = true,
                Class = UnitClass.IFV,
                Army = Resources.FindObjectsOfTypeAll<ArmyBasicInfoScriptable>().Where(o => o.name == "USSR").First(),
                DecalLayout = GHPC.AI.Platoons.PlatoonDecalHelper.DecalLayoutPreset.Simple3,
                PrefabReference = bmp2_metadata.PrefabReference,
                BaseAmmoClipsReference = new AssetReference(),
                UseDecalLayout = false,
                AlternativeClasses = new UnitClass[] { UnitClass.APC, UnitClass.Scout },
                Name = "PIL_BMP3",
                FriendlyName = "BMP-3"
            });

            unit_prefab_lookup.AllUnits = all_units_list.ToArray();

            CustomSpawnInfo bmp3_spawn_info = new CustomSpawnInfo()
            {
                Prefab = bmp3_prefab,
                Id = "PIL_BMP3",
                TargetId = "BMP2_SA",
                OnSpawned = on_spawned,
            };

            CustomSpawnHandler.RegisterCustomVic(bmp3_spawn_info);
        }

        public static void Init()
        {
            //if (!t72_patch.Value) return;

            StateController.RunOrDefer(GameState.PlayerReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
