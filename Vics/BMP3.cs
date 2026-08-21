using GHPC.State;
using ModUtil;
using System.Collections;
using GHPC.Vehicle;
using UnityEngine;
using MelonLoader.Utils;
using System.IO;
using System.Linq;
using HarmonyLib;
using GHPC;
using GHPC.Weapons;
using GHPC.Equipment.Optics;
using System.Collections.Generic;
using GHPC.Mission;
using GHPC.AI;
using GHPC.Mission.Data;
using System;

namespace PactIncreasedLethality
{
    public class BMP3 : Module
    {
        private static Material bmp3_material;
        private static GameObject bmp3_prefab;
        private static string current_spawn_id = "";

        private static void Reposition(Transform target, Transform to, bool delete = false)
        {
            target.SetParent(to);
            target.localPosition = Vector3.zero;
            target.SetParent(to.parent);
            if (delete)
            {
                GameObject.Destroy(to.gameObject);
            }
        }

        [HarmonyPatch(typeof(UnitSpawner), "SpawnUnit", new Type[] { typeof(string), typeof(UnitMetaData), typeof(WaypointHolder), typeof(Transform) })]
        public static class BMP3Marker
        {
            private static void Prefix(UnitSpawner __instance, UnitMetaData metaData)
            {
                current_spawn_id = metaData.Name;
            }
        }

        [HarmonyPatch(typeof(TrackedWheelNodeConfig), "Awake")]
        public static class BMP3SpawnHandler
        {
            private static void Prefix(TrackedWheelNodeConfig __instance)
            {
                Vehicle vic = __instance.GetComponentInParent<Vehicle>();

                if (bmp3_prefab != null && vic._uniqueName == "BMP2_SA" /*&& current_spawn_id.Contains("BMP3")*/)
                {
                    GameObject bmp3 = GameObject.Instantiate(bmp3_prefab, vic.transform);
                    bmp3.transform.localPosition = Vector3.zero;

                    TrackedWheelNodeConfig wheel_node_cfg = vic.transform.Find("WheelControllers").GetComponent<TrackedWheelNodeConfig>();
                    Transform wheel_arms = bmp3.transform.Find("RIG/HULL/wheel arms");
                    Transform track_nodes = bmp3.transform.Find("RIG/HULL/tracks");

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
                }
            }
        }

        private static void HandleConversion(Vehicle vic)
        {
            if (vic == null) return;
            if (vic.UniqueName != "BMP2_SA") return;
            //if (!vic.gameObject.name.Contains("BMP3")) return;

            vic._friendlyName = "BMP-3";

            Transform bmp2_rig = vic.transform.Find("BMP2_rig");
            Transform bmp2_hull = bmp2_rig.Find("HULL");
            Transform bmp2_turret = bmp2_hull.Find("TURRET");

            vic.transform.Find("BMP2_visual").gameObject.SetActive(false);
            vic.transform.Find("BMP2_markings").gameObject.SetActive(false);
            bmp2_hull.Find("numbers").gameObject.SetActive(false);
            bmp2_turret.Find("convoylight_002").gameObject.SetActive(false);
            bmp2_turret.Find("tactical marker").gameObject.SetActive(false);
            bmp2_turret.Find("tactical marker").gameObject.SetActive(false);
            bmp2_turret.Find("konkurs_azimuth").gameObject.SetActive(false);
            bmp2_turret.Find("turret scripts/R123_Prefab").gameObject.SetActive(false);

            Transform bmp3 = vic.transform.Find("bempeh3(Clone)");
            Transform bmp3_turret = bmp3.transform.Find("RIG/HULL/TURRET");

            bmp2_turret.Find("fire control").SetParent(bmp3_turret);
            bmp2_turret.Find("turret scripts").SetParent(bmp3_turret);
            bmp2_turret.Find("Main gun/mantlet scripts").SetParent(bmp3_turret.Find("MANTLET"));

            AimablePlatform turret_platform = bmp3_turret.Find("turret scripts").GetComponent<AimablePlatform>();
            turret_platform.Transform = bmp3_turret;

            AimablePlatform mantlet_platform = bmp3_turret.Find("MANTLET/mantlet scripts").GetComponent<AimablePlatform>();
            mantlet_platform.Transform = bmp3_turret.Find("MANTLET");

            Reposition
            (
                target: vic.DesignatedCameraSlots.Where(o => o.name == "commander head").First().transform,
                to: bmp3_turret.transform.Find("commander head"),
                delete: true
            );

            Reposition
            (
                target: bmp2_turret.Find("gunner day sight 1P3-3"),
                to: bmp3_turret.transform.Find("gps"),
                delete: false
            );

            Reposition
            (
                target: bmp2_turret.Find("gunner night sight 1P3-3"),
                to: bmp3_turret.transform.Find("gps"),
                delete: true
            );

            Reposition
            (
                target: bmp3_turret.Find("MANTLET/mantlet scripts/30mm Gun 2A42"),
                to: bmp3_turret.transform.Find("MANTLET/2a72 muzzle identity"),
                delete: false
            );

            Reposition
            (
                target: bmp2_turret.Find("Main gun/Muzzle identity"),
                to: bmp3_turret.transform.Find("MANTLET/2a72 muzzle identity"),
                delete: true
            );

            Reposition
            (
                target: bmp3_turret.Find("MANTLET/mantlet scripts/7.62mm Machine Gun PKT"),
                to: bmp3_turret.transform.Find("MANTLET/pkt muzzle identity"),
                delete: true
            );

            Reposition
            (
                target: bmp2_turret.Find("konkurs_azimuth/konkurs_elevation/launcher elevation/Launcher 9P135M"),
                to: bmp3_turret.transform.Find("MANTLET/2a70 muzzle identity"),
                delete: true
            );

            WeaponSystemInfo ws_gun_2a70 = vic.LoadoutManager._weaponsManager.GetWeaponInfoByRole(WeaponSystemRole.MountedLauncher);
            WeaponSystem wpn_gun_2a70 = ws_gun_2a70.Weapon;

            WeaponSystemInfo ws_gun_30_2a72 = vic.LoadoutManager._weaponsManager.GetWeaponInfoByRole(WeaponSystemRole.MainGun);
            WeaponSystem wpn_gun_30_2a72 = ws_gun_30_2a72.Weapon;

            FireControlSystem fcs = wpn_gun_30_2a72.FCS;
            UsableOptic day_optic = Util.GetDayOptic(fcs);

            day_optic.slot.ExclusiveWeapons = new WeaponSystem[] { };
            day_optic.slot.LinkedNightSight.ExclusiveWeapons = new WeaponSystem[] { };
            List<WeaponSystem> temp_linked = fcs.LinkedWeaponSystems.ToList();
            temp_linked.Add(wpn_gun_2a70);
            fcs.LinkedWeaponSystems = temp_linked.ToArray();

            ws_gun_2a70.Name = "100mm cannon 2A70";
            ws_gun_2a70.FCS = fcs;
            wpn_gun_2a70._muzzleIdentity = wpn_gun_2a70.transform;
            wpn_gun_2a70.FCS = fcs;
            wpn_gun_2a70.TriggerAudioController = null;
            wpn_gun_2a70.WireGuided = false;
            wpn_gun_2a70.TriggerHoldTime = 0f;
            //wpn_gun_2a70.WeaponSound.SingleShotEventPaths[0] = "event:/Weapons/canon_105mm-L7";

            ws_gun_30_2a72.Name = "30mm gun 2A72";
            wpn_gun_30_2a72.CodexEntry = null;
            wpn_gun_30_2a72.BaseDeviationAngle = 0.155f;
            wpn_gun_30_2a72._cycleTimeSeconds = 0.16f;
            wpn_gun_30_2a72.Feed._totalCycleTime = 0.16f;
            wpn_gun_30_2a72.WeaponSound.SingleShotByDefault = true;
            wpn_gun_30_2a72.WeaponSound.SingleShotMode = true;
            wpn_gun_30_2a72.WeaponSound.SingleShotEventPaths = new string[] { "actually_2a72" };
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
                BaseAmmoClipsReference = new UnityEngine.AddressableAssets.AssetReference(),
                UseDecalLayout = false,
                AlternativeClasses = new UnitClass[] { UnitClass.APC, UnitClass.Scout },
                Name = "BMP3",
                FriendlyName = "BMP-3"
            });

            unit_prefab_lookup.AllUnits = all_units_list.ToArray();
        }

        public static void Init()
        {
            //if (!t72_patch.Value) return;

            StateController.RunOrDefer(GameState.PlayerReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
