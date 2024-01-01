using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GHPC.Equipment.Optics;
using GHPC.Utility;
using GHPC.Vehicle;
using GHPC.Weapons;
using PactIncreasedLethality;
using Reticle;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;
using GHPC.Equipment;
using GHPC.State;
using System.Collections;
using MelonLoader;
using HarmonyLib;
using GHPC;
using static UnityEngine.GraphicsBuffer;
using TMPro;
using GHPC.UI.Tips;

namespace PactIncreasedLethality
{
    public class T55
    {
        static GameObject range_readout;
        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        static AmmoClipCodexScriptable clip_codex_3bk17m;
        static AmmoType.AmmoClip clip_3bk17m;
        static AmmoCodexScriptable ammo_codex_3bk17m;
        static AmmoType ammo_3bk17m;
        static GameObject ammo_3bk17m_vis = null;

        static AmmoType ammo_3bk5m;

        static MelonPreferences_Entry<bool> t55_patch;
        static MelonPreferences_Entry<bool> use_3bk17m;
        static MelonPreferences_Entry<bool> better_stab;
        static MelonPreferences_Entry<bool> has_lrf;

        public static void Config(MelonPreferences_Category cfg)
        {
            t55_patch = cfg.CreateEntry<bool>("T-55 Patch", true);
            use_3bk17m = cfg.CreateEntry<bool>("Use 3BK17M", true);
            use_3bk17m.Comment = "Replaces 3BK5M (improved ballistics, marginally better penetration)";
            better_stab = cfg.CreateEntry<bool>("Better Stabilizer", true);
            better_stab.Comment = "Less reticle blur, shake while on the move";
            has_lrf = cfg.CreateEntry<bool>("Laser Rangefinder", true);
            has_lrf.Comment = "Only gives range: user will need to set range manually";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (GameObject vic_go in PactIncreasedLethalityMod.vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (vic.FriendlyName != "T-55A") continue;

                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;

                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();

                if (has_lrf.Value)
                    fcs.MaxLaserRange = 6000f;

                UsableOptic day_optic = Util.GetDayOptic(fcs);

                if (better_stab.Value)
                {
                    day_optic.slot.VibrationBlurScale = 0.1f;
                    day_optic.slot.VibrationShakeMultiplier = 0.2f;
                }

                if (use_3bk17m.Value)
                {
                    loadout_manager.LoadedAmmoTypes[1] = clip_codex_3bk17m;
                    for (int i = 0; i <= 4; i++)
                    {
                        GHPC.Weapons.AmmoRack rack = loadout_manager.RackLoadouts[i].Rack;
                        rack.ClipTypes[1] = clip_codex_3bk17m.ClipType;
                        Util.EmptyRack(rack);
                    }

                    loadout_manager.SpawnCurrentLoadout();
                    weapon.Feed.AmmoTypeInBreech = null;
                    weapon.Feed.Start();
                    loadout_manager.RegisterAllBallistics();
                }

                if (day_optic.transform.Find("t55 range canvas(Clone)") || !has_lrf.Value)
                {
                    continue;
                }

                GameObject t = GameObject.Instantiate(range_readout);
                t.GetComponent<Reparent>().NewParent = Util.GetDayOptic(fcs).transform;
                t.transform.GetChild(0).transform.localPosition = new Vector3(-284.1897f, -5.5217f, 0.1f);
                t.SetActive(true);

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
                    reticle_cached.tree.lights[1].color = new RGB(2f, 0f, 0f, true);

                    reticleSO.planes[0].elements.Add(new ReticleTree.Angular(new Vector2(0, 0), null, ReticleTree.GroupBase.Alignment.LasePoint));
                    reticle = reticleSO.planes[0].elements[2] as ReticleTree.Angular;
                    reticle_cached.mesh = null;

                    // AAAAAAAAAAAAAAA
                    reticle.elements.Add(new ReticleTree.Circle());
                    reticle.name = "LasePoint";
                    reticle.position = new ReticleTree.Position(0, 0, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                    (reticle.elements[0] as ReticleTree.Circle).radius.mrad = 0.5236f;
                    (reticle.elements[0] as ReticleTree.Circle).thickness.mrad = 0.16f;
                    (reticle.elements[0] as ReticleTree.Circle).illumination = ReticleTree.Light.Type.Powered;
                    (reticle.elements[0] as ReticleTree.Circle).visualType = ReticleTree.VisualElement.Type.ReflectedAdditive;
                    (reticle.elements[0] as ReticleTree.Circle).position = new ReticleTree.Position(0, 0, AngularLength.AngularUnit.MIL_USSR, LinearLength.LinearUnit.M);
                    (reticle.elements[0] as ReticleTree.Circle).position.x = 0;
                    (reticle.elements[0] as ReticleTree.Circle).position.y = 0;

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

            yield break;
        }

        public static void Init()
        {
            if (!t55_patch.Value) return;
            
            if (!range_readout)
            {

                foreach (GameObject obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
                {
                    if (obj.name == "M1IP")
                    {
                        range_readout = GameObject.Instantiate(obj.transform.Find("Turret Scripts/GPS/Optic/Abrams GPS canvas").gameObject);
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
                    }
                }
            }

            if (ammo_3bk17m == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "3BK5M HEAT-FS-T") { ammo_3bk5m = s.AmmoType; break; }
                }

                ammo_3bk17m = new AmmoType();
                Util.ShallowCopy(ammo_3bk17m, ammo_3bk5m);
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

                ammo_3bk17m_vis = GameObject.Instantiate(ammo_3bk5m.VisualModel);
                ammo_3bk17m_vis.name = "3bk17m visual";
                ammo_3bk17m.VisualModel = ammo_3bk17m_vis;
                ammo_3bk17m.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bk17m;
                ammo_3bk17m.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bk17m;
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }

    // hijack lase method to prevent it from ranging the gun itself 
    [HarmonyPatch(typeof(GHPC.Weapons.FireControlSystem), "DoLase")]
    public static class T55Lase
    {
        private static bool Prefix(GHPC.Weapons.FireControlSystem __instance)
        {
            if (__instance.gameObject.GetComponentInParent<Vehicle>().FriendlyName != "T-55A") {
                return true;
            }

            __instance._laseQueued = false;

            float num = -1f;
            int layerMask = 1 << CodeUtils.LAYER_MASK_VISIBILITYONLY;

            RaycastHit raycastHit;
            if (Physics.Raycast(__instance.LaserOrigin.position, __instance.LaserOrigin.forward, out raycastHit, __instance.MaxLaserRange, layerMask) && raycastHit.collider.tag == "Smoke")
            {
                num = raycastHit.distance;
            }
            if (Physics.Raycast(__instance.LaserOrigin.position, __instance.LaserOrigin.forward, out raycastHit, __instance.MaxLaserRange, ConstantsAndInfoManager.Instance.LaserRangefinderLayerMask.value) && (raycastHit.distance < num || num == -1f))
            {
                num = raycastHit.distance;
            }

            if (num != -1f)
            {
                var range_readout = Util.GetDayOptic(__instance).transform.Find("t55 range canvas(Clone)");
                var text = range_readout.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                text.text = ((int)MathUtil.RoundFloatToMultipleOf(num, 5)).ToString("0000");
            }

            return false;
        }
    }
}
