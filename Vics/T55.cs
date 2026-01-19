using System.Collections.Generic;
using GHPC.Equipment.Optics;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using Reticle;
using UnityEngine;
using GHPC.State;
using System.Collections;
using MelonLoader;
using GHPC;
using TMPro;
using MelonLoader.Utils;
using System.IO;
using NWH.VehiclePhysics;
using GHPC.Weaponry;

namespace PactIncreasedLethality
{
    public class T55
    {
        internal static GameObject range_readout;
        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static AmmoClipCodexScriptable clip_codex_3bm25;
        static AmmoType.AmmoClip clip_3bm25;
        static AmmoCodexScriptable ammo_codex_3bm25;
        static AmmoType ammo_3bm25;
        static GameObject ammo_3bm25_vis = null;

        static AmmoClipCodexScriptable clip_codex_3bk17m;
        static AmmoType.AmmoClip clip_3bk17m;
        static AmmoCodexScriptable ammo_codex_3bk17m;
        static AmmoType ammo_3bk17m;
        static GameObject ammo_3bk17m_vis = null;

        internal static AmmoClipCodexScriptable clip_codex_9m117;
        static AmmoType.AmmoClip clip_9m117;
        static AmmoCodexScriptable ammo_codex_9m117;
        static AmmoType ammo_9m117;
        static GameObject ammo_9m117_vis = null;

        static MelonPreferences_Entry<bool> t55_patch;
        static MelonPreferences_Entry<bool> use_3bk17m;
        static MelonPreferences_Entry<bool> use_3bm25;
        static MelonPreferences_Entry<bool> use_br412d;
        static MelonPreferences_Entry<bool> use_9m117;
        static MelonPreferences_Entry<bool> better_stab;
        static MelonPreferences_Entry<bool> has_lrf;
        static MelonPreferences_Entry<bool> has_drozd;
        static MelonPreferences_Entry<bool> tpn3;
        static MelonPreferences_Entry<bool> applique;
        static MelonPreferences_Entry<bool> engine_upr;

        static Material t55am_turret_material;
        static GameObject t55am_turret_parts;
        static GameObject t55am_hull_parts;
        static GameObject t55am_lrf;
        static GameObject t55am_skirts;
        static Mesh t55am_hull;
        static Texture2D cleaned_texture;

        private static bool assets_loaded = false;

        public static void Config(MelonPreferences_Category cfg)
        {
            t55_patch = cfg.CreateEntry<bool>("T-55 Patch", true);

            use_3bm25 = cfg.CreateEntry<bool>("Use 3BM25", true);
            use_3bm25.Comment = "Replaces 3BM20 (improved penetration)";

            use_3bk17m = cfg.CreateEntry<bool>("Use 3BK17M", true);
            use_3bk17m.Comment = "Replaces 3BK5M (improved ballistics, marginally better penetration)";

            use_9m117 = cfg.CreateEntry<bool>("Use 9M117 (T-55)", true);
            use_9m117.Comment = "GLATGM, has its own sight with fixed 8x magnification";

            use_br412d = cfg.CreateEntry<bool>("Use BR-412D (T-55)", true);
            use_br412d.Comment = "Replaces 3OF412 with BR-412D APHE-T";

            better_stab = cfg.CreateEntry<bool>("Better Stabilizer (T-55)", true);
            better_stab.Comment = "Less reticle blur, shake while on the move";

            has_lrf = cfg.CreateEntry<bool>("Laser Rangefinder (T-55)", true);
            has_lrf.Comment = "Only gives range: user will need to set range manually";

            //has_drozd = cfg.CreateEntry<bool>("Drozd APS (T-55)", false);
            //has_drozd.Comment = "Intercepts incoming projectiles; covers the frontal arc of the tank relative to where the turret is facing";

            tpn3 = cfg.CreateEntry<bool>("TPN-3 Night Sight (T-55)", true);
            tpn3.Comment = "Replaces the night sight with the one found on the T-80B/T-64B";

            applique = cfg.CreateEntry<bool>("BDD Applique (T-55)", true);
            applique.Comment = "Composite applique hull and turret cheek armour";

            engine_upr = cfg.CreateEntry<bool>("Engine Upgrade (T-55)", true);
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (vic.FriendlyName != "T-55A") continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;
                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                UsableOptic day_optic = Util.GetDayOptic(fcs);
                Transform lrf_canvas = null;

                if (has_lrf.Value)
                {
                    GameObject lrf = GameObject.Instantiate(t55am_lrf, vic.transform.Find("T55A_skeleton/HULL/Turret"));

                    Transform laser = lrf.transform.Find("LRF");
                    laser.gameObject.AddComponent<LateFollowTarget>();

                    LateFollow laser_armor = laser.transform.Find("LRF").gameObject.AddComponent<LateFollow>();
                    laser_armor.gameObject.SetActive(true);
                    laser_armor.FollowTarget = laser;
                    laser_armor.ForceToRoot = true;
                    laser_armor.enabled = true;
                    laser_armor.Awake();
                    laser_armor.gameObject.AddComponent<Reparent>();
                    lrf.transform.parent = vic.transform.Find("T55A_skeleton/HULL/Turret/GUN");

                    weapon.FCS.gameObject.AddComponent<LimitedLRF>();
                    fcs.MaxLaserRange = 6000f;

                    GameObject t = GameObject.Instantiate(range_readout);
                    lrf_canvas = t.transform;
                    t.GetComponent<Reparent>().NewParent = Util.GetDayOptic(fcs).transform;
                    t.transform.GetChild(0).transform.localPosition = new Vector3(-284.1897f, -5.5217f, 0.1f);
                    t.SetActive(true);

                    weapon.FCS.GetComponent<LimitedLRF>().canvas = t.transform;

                    if (!reticleSO)
                    {
                        ReticleTree.Angular reticle = null;
                        ReticleTree.Angular reticle_heat = null;

                        reticleSO = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55"].tree);
                        reticleSO.name = "T55withdalaser";

                        Util.ShallowCopy(reticle_cached, ReticleMesh.cachedReticles["T55"]);
                        reticle_cached.tree = reticleSO;

                        reticle_cached.tree.lights = new List<ReticleTree.Light>() {
                            new ReticleTree.Light(),
                            new ReticleTree.Light()
                        };

                        reticle_cached.tree.lights[0] = ReticleMesh.cachedReticles["T55"].tree.lights[0];
                        reticle_cached.tree.lights[1].type = ReticleTree.Light.Type.Powered;
                        reticle_cached.tree.lights[1].color = new RGB(15f, 0f, 0f, true);

                        reticleSO.planes[0].elements.Add(new ReticleTree.Angular(new Vector2(0, 0), null, ReticleTree.GroupBase.Alignment.LasePoint));
                        reticle = reticleSO.planes[0].elements[2] as ReticleTree.Angular;
                        reticle_cached.mesh = null;

                        reticle.elements.Add(new ReticleTree.Circle());
                        reticle.name = "LasePoint";
                        reticle.position = new ReticleTree.Position(0, 0, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                        ReticleTree.Circle circle = reticle.elements[0] as ReticleTree.Circle;
                        circle.radius.mrad = 0.5236f;
                        circle.thickness.mrad = 0.16f;
                        circle.illumination = ReticleTree.Light.Type.Powered;
                        circle.visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;
                        circle.position = new ReticleTree.Position(0, 0, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                        circle.position.x = 0;
                        circle.position.y = 0;

                        if (use_3bk17m.Value)
                        {
                            reticle_heat = ((reticleSO.planes[0].elements[1] as ReticleTree.Angular).elements[2]) as ReticleTree.Angular;
                            (reticle_heat.elements[1] as ReticleTree.VerticalBallistic).projectile = ammo_codex_3bk17m;
                            (reticle_heat.elements[1] as ReticleTree.VerticalBallistic).UpdateBC();
                        }
                    }

                    day_optic.reticleMesh.reticleSO = reticleSO;
                    day_optic.reticleMesh.reticle = reticle_cached;
                    day_optic.reticleMesh.SMR = null;
                    day_optic.reticleMesh.Load();
                }

                if (better_stab.Value)
                {
                    day_optic.slot.VibrationBlurScale = 0.1f;
                    day_optic.slot.VibrationShakeMultiplier = 0.2f;
                }

                if (use_9m117.Value)
                {
                    GameObject guidance_computer_obj = new GameObject("guidance computer");
                    guidance_computer_obj.transform.parent = vic.transform;
                    guidance_computer_obj.AddComponent<MissileGuidanceUnit>();

                    guidance_computer_obj.AddComponent<Reparent>();
                    Reparent reparent = guidance_computer_obj.GetComponent<Reparent>();
                    reparent.NewParent = vic_go.transform.Find("T55A_skeleton/HULL/Turret").gameObject.transform;
                    reparent.Awake();

                    MissileGuidanceUnit computer = guidance_computer_obj.GetComponent<MissileGuidanceUnit>();
                    computer.AimElement = weapon.FCS.AimTransform;
                    weapon.GuidanceUnit = computer;

                    weapon.Feed.ReloadDuringMissileTracking = false;
                    weapon.Feed._missileGuidance = computer;

                    BOM.Add(day_optic.transform, lrf_canvas);

                    loadout_manager.LoadedAmmoList.AmmoClips = Util.AppendToArray(loadout_manager.LoadedAmmoList.AmmoClips, clip_codex_9m117);
                    loadout_manager._totalAmmoTypes = 4;
                    loadout_manager.TotalAmmoCounts = new int[] { 16, 18, 6, 3 };
                }

                if (use_3bm25.Value)
                    loadout_manager.LoadedAmmoList.AmmoClips[0] = clip_codex_3bm25;

                if (use_br412d.Value) 
                    loadout_manager.LoadedAmmoList.AmmoClips[2] = Assets.clip_codex_br412d;

                if (use_3bk17m.Value) 
                    loadout_manager.LoadedAmmoList.AmmoClips[1] = clip_codex_3bk17m;

                for (int i = 0; i <= 4; i++)
                {
                    GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;

                    if (use_9m117.Value && (i == 0 || i == 3))
                    {
                        loadout_manager.RackLoadouts[i].FixedChoices = new LoadoutManager.RackLoadoutFixedChoice[] {
                            new LoadoutManager.RackLoadoutFixedChoice() {
                                AmmoClipIndex = 3,
                                RackSlotIndex = 0,
                            },
                            new LoadoutManager.RackLoadoutFixedChoice() {
                                AmmoClipIndex = 3,
                                RackSlotIndex = 1,
                            }
                        };
                    }
                    
                    Util.EmptyRack(rack);
                }

                loadout_manager.SpawnCurrentLoadout();
                weapon.Feed.AmmoTypeInBreech = null;
                weapon.Feed.Start();
                loadout_manager.RegisterAllBallistics();

                if (tpn3.Value) {
                    TPN3.Add(fcs, day_optic.slot.LinkedNightSight.PairedOptic, day_optic.slot.LinkedNightSight);
                }

                if (engine_upr.Value) {
                    vic.transform.GetComponent<VehicleController>().engine.maxPower = 650f;
                }

                if (applique.Value)
                {
                    SkinnedMeshRenderer turret_skinned = vic.transform.Find("T55A_base (1)/LP_turret").GetComponent<SkinnedMeshRenderer>();
                    Material[] new_materials = turret_skinned.materials;
                    Material mat = new_materials[0];

                    // https://github.com/Unity-Technologies/UnityCsReference/blob/1d7b2b49b93ea5773aa4e8dfa504e3c1533ce282/Editor/Mono/Inspector/StandardShaderGUI.cs#L369
                    mat.SetTexture("_Albedo", cleaned_texture);
                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                    mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                    mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetFloat("_ZWrite", 1.0f);
                    mat.EnableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    new_materials[1] = mat;
                    turret_skinned.materials = new_materials;

                    Transform hull = vic.transform.Find("T55A_base (1)/hull");
                    hull.GetComponent<MeshFilter>().sharedMesh = t55am_hull;

                    Transform turret = vic.transform.Find("T55A_skeleton/HULL/Turret");
                    turret.Find("Night sight cover").localScale = Vector3.zero;
                    turret.Find("T55A_markings").gameObject.SetActive(false);
                    turret.Find("cunt cover").gameObject.SetActive(true); // yes, this is what the glass protecting the gunner's sight is called 

                    GameObject turret_parts = GameObject.Instantiate(t55am_turret_parts, turret);
                    LateFollow turret_armour = turret_parts.transform.Find("ARMOUR").gameObject.AddComponent<LateFollow>();
                    turret_armour.gameObject.SetActive(true);
                    turret_armour.FollowTarget = turret;
                    turret_armour.ForceToRoot = true;
                    turret_armour.enabled = true;
                    turret_armour.Awake();
                    turret_armour.gameObject.AddComponent<Reparent>();

                    hull.gameObject.AddComponent<LateFollowTarget>();
                    GameObject hull_parts = GameObject.Instantiate(t55am_hull_parts, hull);
                    hull_parts.transform.localEulerAngles = Vector3.zero;

                    LateFollow hull_armour = hull_parts.transform.Find("ARMOUR").gameObject.AddComponent<LateFollow>();
                    hull_armour.gameObject.SetActive(true);
                    hull_armour.FollowTarget = hull;
                    hull_armour.ForceToRoot = true;
                    hull_armour.enabled = true;
                    hull_armour.Awake();
                    hull_armour.gameObject.AddComponent<Reparent>();

                    GameObject skirts = GameObject.Instantiate(t55am_skirts, hull);
                    skirts.transform.localEulerAngles = Vector3.zero;
                    skirts.transform.localPosition = new Vector3(0f, 0.05f, 0.01f);

                    Transform fenders_parent = vic.transform.Find("T55A_variant");
                    fenders_parent.Find("cut fender").gameObject.SetActive(false);
                    fenders_parent.Find("rubber fender").gameObject.SetActive(false);
                    fenders_parent.Find("steel fender").gameObject.SetActive(true);

                    vic._friendlyName = "T-55AM2";
                    if (use_9m117.Value) vic._friendlyName += "B";
                }
            }

            yield break;
        }

        public static void LoadAssets() {
            if (assets_loaded) return;
            if (!t55_patch.Value) return;

            range_readout = GameObject.Instantiate(Assets.m1ip_range_canvas);
            GameObject.Destroy(range_readout.transform.GetChild(2).gameObject);
            GameObject.Destroy(range_readout.transform.GetChild(0).gameObject);
            range_readout.AddComponent<Reparent>();
            range_readout.SetActive(false);
            range_readout.hideFlags = HideFlags.DontUnloadUnusedAsset;
            range_readout.name = "t55 range canvas";

            TextMeshProUGUI text = range_readout.GetComponentInChildren<TextMeshProUGUI>();
            text.color = new Color(255f, 0f, 0f);
            text.faceColor = new Color(255f, 0f, 0f);
            text.outlineColor = new Color(100f, 0f, 0f, 0.5f);

            var t55am_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "t55am"));
            t55am_turret_material = t55am_bundle.LoadAsset<Material>("CLEANED MATERIAL.mat");
            t55am_turret_material.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t55am_turret_parts = t55am_bundle.LoadAsset<GameObject>("T55AM TURRET PARTS.prefab");
            t55am_turret_parts.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t55am_hull_parts = t55am_bundle.LoadAsset<GameObject>("T55AM HULL PARTS.prefab");
            t55am_hull_parts.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t55am_skirts = t55am_bundle.LoadAsset<GameObject>("SKIRTS.prefab");
            t55am_skirts.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t55am_lrf = t55am_bundle.LoadAsset<GameObject>("T55AM LRF.prefab");
            t55am_lrf.hideFlags = HideFlags.DontUnloadUnusedAsset;

            cleaned_texture = t55am_bundle.LoadAsset<Texture2D>("CLEANED TEXTURE.png");
            cleaned_texture.hideFlags = HideFlags.DontUnloadUnusedAsset;

            t55am_hull = t55am_bundle.LoadAsset<Mesh>("hull.asset");
            t55am_hull.hideFlags = HideFlags.DontUnloadUnusedAsset;

            Util.SetupFLIRShaders(t55am_turret_parts);
            Util.SetupFLIRShaders(t55am_hull_parts);
            Util.SetupFLIRShaders(t55am_skirts);
            Util.SetupFLIRShaders(t55am_lrf);

            Transform armour = t55am_turret_parts.transform.Find("ARMOUR");
            foreach (Transform t in armour.GetComponentsInChildren<Transform>())
            {
                t.gameObject.tag = "Penetrable";
                t.gameObject.layer = 8;
            }

            Transform hull_armour = t55am_hull_parts.transform.Find("ARMOUR");
            foreach (Transform t in hull_armour.GetComponentsInChildren<Transform>())
            {
                t.gameObject.tag = "Penetrable";
                t.gameObject.layer = 8;
            }

            GameObject lrf = t55am_lrf.transform.Find("LRF/LRF").gameObject;
            lrf.gameObject.tag = "Penetrable";
            lrf.gameObject.layer = 8;

            UniformArmor armor_lrf = lrf.AddComponent<UniformArmor>();
            armor_lrf.SetName("laser rangefinder");
            armor_lrf.PrimaryHeatRha = 15f;
            armor_lrf.PrimarySabotRha = 15f;

            GameObject turret_left_cheek = armour.transform.Find("L COMP CHEEK").gameObject;
            VariableArmor armor_turret_l_cheek = turret_left_cheek.AddComponent<VariableArmor>();
            armor_turret_l_cheek.SetName("metal-polymer block");
            armor_turret_l_cheek._armorType = Armour.cheek_metal_polymer;
            armor_turret_l_cheek._spallForwardRatio = 0.2f;
            AarVisual aar_l_cheek = turret_left_cheek.AddComponent<AarVisual>();
            aar_l_cheek.SwitchMaterials = false;
            aar_l_cheek.HideUntilAar = true;

            GameObject turret_right_cheek = armour.transform.Find("R COMP CHEEK").gameObject;
            VariableArmor armor_turret_r_cheek = turret_right_cheek.AddComponent<VariableArmor>();
            armor_turret_r_cheek.SetName("metal-polymer block");
            armor_turret_r_cheek._armorType = Armour.cheek_metal_polymer;
            armor_turret_r_cheek._spallForwardRatio = 0.2f;
            AarVisual aar_r_cheek = turret_right_cheek.AddComponent<AarVisual>();
            aar_r_cheek.SwitchMaterials = false;
            aar_r_cheek.HideUntilAar = true;

            GameObject turret_left_applique = armour.transform.Find("L APPLIQUE CHEEK").gameObject;
            VariableArmor armor_turret_l_applique = turret_left_applique.AddComponent<VariableArmor>();
            armor_turret_l_applique.SetName("applique cheek armor");
            armor_turret_l_applique._armorType = Armour.bdd_cast_armor;
            armor_turret_l_applique._spallForwardRatio = 0.2f;

            GameObject turret_right_applique = armour.transform.Find("R APPLIQUE CHEEK").gameObject;
            VariableArmor armor_turret_r_applique = turret_right_applique.AddComponent<VariableArmor>();
            armor_turret_r_applique.SetName("applique cheek armor");
            armor_turret_r_applique._armorType = Armour.bdd_cast_armor;
            armor_turret_r_applique._spallForwardRatio = 0.2f;

            GameObject hull_mpoly_block = hull_armour.transform.Find("MPOLY BLOCK").gameObject;
            VariableArmor armor_mpoly_block = hull_mpoly_block.AddComponent<VariableArmor>();
            armor_mpoly_block.SetName("metal-polymer block");
            armor_mpoly_block._armorType = Armour.hull_metal_polymer;
            armor_mpoly_block._spallForwardRatio = 0.01f;
            AarVisual aar_mpoly_block = hull_mpoly_block.AddComponent<AarVisual>();
            aar_mpoly_block.SwitchMaterials = false;
            aar_mpoly_block.HideUntilAar = true;

            GameObject hull_casing = hull_armour.transform.Find("OUTER CASING ").gameObject;
            VariableArmor armor_casing = hull_casing.AddComponent<VariableArmor>();
            armor_casing.SetName("upper glacis applique armor");
            armor_casing._armorType = Armour.ru_welded_armor;
            armor_casing._spallForwardRatio = 0.01f;

            ammo_3bm25 = new AmmoType();
            Util.ShallowCopy(ammo_3bm25, Assets.ammo_3bm20);
            ammo_3bm25.Name = "3BM25 APFSDS-T";
            ammo_3bm25.RhaPenetration = 380f;

            ammo_codex_3bm25 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_3bm25.AmmoType = ammo_3bm25;
            ammo_codex_3bm25.name = "ammo_3bm25";

            clip_3bm25 = new AmmoType.AmmoClip();
            clip_3bm25.Capacity = 1;
            clip_3bm25.Name = "3BM25 APFSDS-T";
            clip_3bm25.MinimalPattern = new AmmoCodexScriptable[1];
            clip_3bm25.MinimalPattern[0] = ammo_codex_3bm25;

            clip_codex_3bm25 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_3bm25.name = "clip_3bm25";
            clip_codex_3bm25.ClipType = clip_3bm25;

            ammo_3bm25_vis = GameObject.Instantiate(Assets.ammo_3bm20.VisualModel);
            ammo_3bm25_vis.name = "3bm25 visual";
            ammo_3bm25.VisualModel = ammo_3bm25_vis;
            ammo_3bm25.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm25;
            ammo_3bm25.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm25;

            ammo_3bk17m = new AmmoType();
            Util.ShallowCopy(ammo_3bk17m, Assets.ammo_3bk5m);
            ammo_3bk17m.Name = "3BK17M HEAT-FS-T";
            ammo_3bk17m.Mass = 10.0f;
            ammo_3bk17m.Coeff = 0.25f;
            ammo_3bk17m.MuzzleVelocity = 1085f;
            ammo_3bk17m.RhaPenetration = 400f;
            ammo_3bk17m.TntEquivalentKg = 0.25f;

            ammo_codex_3bk17m = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_3bk17m.AmmoType = ammo_3bk17m;
            ammo_codex_3bk17m.name = "ammo_3bk17m";

            clip_3bk17m = new AmmoType.AmmoClip();
            clip_3bk17m.Capacity = 1;
            clip_3bk17m.Name = "3BK17M HEAT-FS-T";
            clip_3bk17m.MinimalPattern = new AmmoCodexScriptable[1];
            clip_3bk17m.MinimalPattern[0] = ammo_codex_3bk17m;

            clip_codex_3bk17m = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_3bk17m.name = "clip_3bk17m";
            clip_codex_3bk17m.ClipType = clip_3bk17m;

            ammo_3bk17m_vis = GameObject.Instantiate(Assets.ammo_3bk5m.VisualModel);
            ammo_3bk17m_vis.name = "3bk17m visual";
            ammo_3bk17m.VisualModel = ammo_3bk17m_vis;
            ammo_3bk17m.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bk17m;
            ammo_3bk17m.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bk17m;

            ammo_9m117 = new AmmoType();
            Util.ShallowCopy(ammo_9m117, Assets.ammo_9m111);
            ammo_9m117.Name = "9M117 Bastion";
            ammo_9m117.Mass = 18.8f;
            ammo_9m117.Coeff = 0.25f;
            ammo_9m117.Caliber = 100f;
            ammo_9m117.MuzzleVelocity = 350f;
            ammo_9m117.RhaPenetration = 550f;
            ammo_9m117.TntEquivalentKg = 4.77f;
            ammo_9m117.Guidance = AmmoType.GuidanceType.SACLOS;
            ammo_9m117.TurnSpeed = 0.25f;
            ammo_9m117.RangedFuseTime = 12.5f;
            ammo_9m117.SpiralPower = 25f;
            ammo_9m117.SpiralAngularRate = 1800f;
            ammo_9m117.ArmingDistance = 45f;
            ammo_9m117.ImpactAudio = GHPC.Audio.ImpactAudioType.Missile;
            ammo_9m117.ShortName = AmmoType.AmmoShortName.Missile;
            ammo_9m117.CachedIndex = -1;

            ammo_codex_9m117 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_9m117.AmmoType = ammo_9m117;
            ammo_codex_9m117.name = "ammo_9m117";

            clip_9m117 = new AmmoType.AmmoClip();
            clip_9m117.Capacity = 1;
            clip_9m117.Name = "9M117 Bastion";
            clip_9m117.MinimalPattern = new AmmoCodexScriptable[1];
            clip_9m117.MinimalPattern[0] = ammo_codex_9m117;

            clip_codex_9m117 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_9m117.name = "clip_9m117";
            clip_codex_9m117.ClipType = clip_9m117;

            ammo_9m117_vis = GameObject.Instantiate(Assets.ammo_3of412.VisualModel);
            ammo_9m117_vis.name = "9M117 visual";
            ammo_9m117.VisualModel = ammo_9m117_vis;
            ammo_9m117.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_9m117;
            ammo_9m117.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_9m117;

            assets_loaded = true;
        }

        public static void Init()
        {
            if (!t55_patch.Value) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
