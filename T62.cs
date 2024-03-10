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
using UnityEngine;

namespace PactIncreasedLethality
{
    public class T62
    {
        static MelonPreferences_Entry<bool> t62_patch;
        static MelonPreferences_Entry<bool> better_stab;
        static MelonPreferences_Entry<bool> has_lrf;
        static MelonPreferences_Entry<bool> has_drozd;

        static GameObject range_readout;
        static ReticleSO reticleSO;
        static ReticleMesh.CachedReticle reticle_cached;

        public static void Config(MelonPreferences_Category cfg)
        {
            t62_patch = cfg.CreateEntry<bool>("T-62 Patch", true);
            t62_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";
            better_stab = cfg.CreateEntry<bool>("Better Stabilizer (T-62)", true);
            better_stab.Comment = "Less reticle blur, shake while on the move";
            has_lrf = cfg.CreateEntry<bool>("Laser Rangefinder (T-62)", true);
            has_lrf.Comment = "Only gives range: user will need to set range manually";

            has_drozd = cfg.CreateEntry<bool>("Drozd APS (T-62)", true);
            has_drozd.Comment = "Intercepts incoming projectiles; covers the frontal arc of the tank relative to where the turret is facing";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (GameObject vic_go in PactIncreasedLethalityMod.vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (vic.FriendlyName != "T-62") continue;
                if (vic_go.GetComponent<AlreadyConverted>() != null) continue;

                vic_go.AddComponent<AlreadyConverted>();

                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                WeaponSystem weapon = vic.GetComponent<WeaponsManager>().Weapons[0].Weapon;

                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();

                if (has_lrf.Value)
                {
                    weapon.FCS.gameObject.AddComponent<LimitedLRF>();
                    fcs.MaxLaserRange = 6000f;
                }

                UsableOptic day_optic = Util.GetDayOptic(fcs);

                if (better_stab.Value)
                {
                    day_optic.slot.VibrationBlurScale = 0.1f;
                    day_optic.slot.VibrationShakeMultiplier = 0.2f;
                }

                if (!has_lrf.Value)
                {
                    continue;
                }

                GameObject t = GameObject.Instantiate(range_readout);
                t.GetComponent<Reparent>().NewParent = Util.GetDayOptic(fcs).transform;
                t.transform.GetChild(0).transform.localPosition = new Vector3(-284.1897f, -5.5217f, 0.1f);
                t.SetActive(true);

                weapon.FCS.GetComponent<LimitedLRF>().canvas = t.transform;


                if (!reticleSO)
                {
                    ReticleTree.Angular reticle = null;

                    reticleSO = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T62 Corrected"].tree);
                    reticleSO.name = "T62withdalaser";

                    Util.ShallowCopy(reticle_cached, ReticleMesh.cachedReticles["T62 Corrected"]);
                    reticle_cached.tree = reticleSO;

                    reticle_cached.tree.lights = new List<ReticleTree.Light>() {
                        new ReticleTree.Light(),
                        new ReticleTree.Light()
                    };

                    reticle_cached.tree.lights[0] = ReticleMesh.cachedReticles["T62 Corrected"].tree.lights[0];
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
                }

                day_optic.reticleMesh.reticleSO = reticleSO;
                day_optic.reticleMesh.reticle = reticle_cached;
                day_optic.reticleMesh.SMR = null;
                day_optic.reticleMesh.Load();

                if (has_drozd.Value)
                {
                    List<DrozdLauncher> launchers = new List<DrozdLauncher>();
                    vic.transform.Find("---T62_rig---/HULL/TURRET/ammobox").gameObject.SetActive(false);

                    Vector3[] launcher_positions = new Vector3[] {
                        new Vector3(-1.2952f, -0.1383f, -0.2131f),
                        new Vector3(-1.2543f, 0.1291f, -0.2131f),
                        new Vector3(1.2952f, -0.1383f, -0.2131f),
                        new Vector3(1.2543f, 0.1291f, -0.2131f),
                    };

                    Vector3[] launcher_rots = new Vector3[] 
                    {
                        new Vector3(0f, 0f, 0f),
                        new Vector3(0f, 335.8091f, 0f),
                        new Vector3(0f, 0f, 0f),
                        new Vector3(0f, -335.8091f, 0f)
                    };

                    for (var i = 0; i < launcher_positions.Length; i++)
                    {
                        GameObject launcher = GameObject.Instantiate(DrozdLauncher.drozd_launcher_visual, vic.transform.Find("---T62_rig---/HULL/TURRET"));
                        launcher.transform.localPosition = launcher_positions[i];
                        launcher.transform.localEulerAngles = launcher_rots[i];

                        if (i > 1)
                        {
                            launcher.transform.localScale = Vector3.Scale(launcher.transform.localScale, new Vector3(-1f, 1f, 1f));
                        }

                        launchers.Add(launcher.GetComponent<DrozdLauncher>());
                    }

                    Drozd.AttachDrozd(
                        vic.transform.Find("---T62_rig---/HULL/TURRET"), vic, new Vector3(0f, 0f, 9.5f),
                        launchers.GetRange(0, 2).ToArray(), launchers.GetRange(2, 2).ToArray()
                    );

                    vic._friendlyName += "D";
                }
            }

            yield break;
        }

        public static void Init()
        {
            if (!t62_patch.Value) return;

            if (!range_readout)
            {
                foreach (Vehicle obj in Resources.FindObjectsOfTypeAll(typeof(Vehicle)))
                {
                    if (obj.name == "M1IP")
                    {
                        range_readout = GameObject.Instantiate(obj.transform.Find("Turret Scripts/GPS/Optic/Abrams GPS canvas").gameObject);
                        GameObject.Destroy(range_readout.transform.GetChild(2).gameObject);
                        GameObject.Destroy(range_readout.transform.GetChild(0).gameObject);
                        range_readout.AddComponent<Reparent>();
                        range_readout.SetActive(false);
                        range_readout.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        range_readout.name = "t62 range canvas";

                        TextMeshProUGUI text = range_readout.GetComponentInChildren<TextMeshProUGUI>();
                        text.color = new Color(255f, 0f, 0f);
                        text.faceColor = new Color(255f, 0f, 0f);
                        text.outlineColor = new Color(100f, 0f, 0f, 0.5f);

                        break;
                    }
                }
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }

    }
}
