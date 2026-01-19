using UnityEngine;
using GHPC.Weapons;
using GHPC.Equipment.Optics;
using Reticle;
using System.Collections.Generic;
using GHPC.Vehicle;
using MelonLoader;

namespace PactIncreasedLethality
{
    public class BOM
    {
        private static ReticleSO reticleSO_atgm;
        private static ReticleMesh.CachedReticle reticle_cached_atgm;
        private static bool assets_loaded = false;

        public static void Add(Transform optic, Transform laser_canvas = null) 
        {
            GameObject reticle_mesh_atgm = GameObject.Instantiate(optic.Find("Reticle Mesh").gameObject, optic);
            reticle_mesh_atgm.SetActive(false);
            reticle_mesh_atgm.GetComponent<ReticleMesh>().reticleSO = reticleSO_atgm;
            reticle_mesh_atgm.GetComponent<ReticleMesh>().reticle = reticle_cached_atgm;
            reticle_mesh_atgm.GetComponent<ReticleMesh>().SMR = null;
            reticle_mesh_atgm.GetComponent<ReticleMesh>().Load();

            ATGMSight sight = optic.gameObject.AddComponent<ATGMSight>(); 
            sight.original_reticle_mesh = optic.Find("Reticle Mesh").GetComponent<ReticleMesh>();
            sight.atgm_reticle_mesh = reticle_mesh_atgm.GetComponent<ReticleMesh>();
            if (laser_canvas)
                sight.laser_canvas = laser_canvas;
            sight.enabled = true;
        }

        public class ATGMSight : MonoBehaviour 
        {
            UsableOptic optic;
            FireControlSystem fcs;
            public ReticleMesh original_reticle_mesh;
            public ReticleMesh atgm_reticle_mesh;
            public Transform laser_canvas;
            private bool was_missile = false;
            float original_default_fov;
            float[] original_other_fovs; 

            void Awake()
            { 
                optic = GetComponent<UsableOptic>();
                fcs = optic.FCS;
                original_default_fov = optic.slot.DefaultFov;
                original_other_fovs = (float[])optic.slot.OtherFovs.Clone();
                fcs.AmmoTypeChanged += FCS_AmmoTypeChanged;
            }

            void FCS_AmmoTypeChanged(AmmoType ammo_type)
            {
                if (ammo_type.ShortName == AmmoType.AmmoShortName.Missile)
                {
                    if (laser_canvas) laser_canvas.gameObject.SetActive(false);

                    optic.slot.DefaultFov = 4.2f;
                    if (optic.slot.OtherFovs.Length > 0) optic.slot.OtherFovs[0] = 4.2f;

                    optic.reticleMesh = atgm_reticle_mesh;
                    original_reticle_mesh.gameObject.SetActive(false);
                    atgm_reticle_mesh.gameObject.SetActive(true);

                    Mod.camera_manager.ZoomChanged();
                    was_missile = true;
                }
                else
                {
                    if (!was_missile) return;
                    if (laser_canvas) laser_canvas.gameObject.SetActive(true);

                    optic.slot.DefaultFov = original_default_fov;
                    optic.slot.OtherFovs = original_other_fovs;

                    optic.reticleMesh = original_reticle_mesh;
                    original_reticle_mesh.gameObject.SetActive(true);
                    atgm_reticle_mesh.gameObject.SetActive(false);

                    Mod.camera_manager.ZoomChanged();
                    was_missile = false;
                }
            }
        }

        public static void LoadAssets()
        {
            if (assets_loaded) return;

            reticleSO_atgm = ScriptableObject.Instantiate(ReticleMesh.cachedReticles["T55"].tree);
            reticleSO_atgm.name = "T55_atgm";

            Util.ShallowCopy(reticle_cached_atgm, ReticleMesh.cachedReticles["T55"]);
            reticle_cached_atgm.tree = reticleSO_atgm;

            reticle_cached_atgm.tree.lights = new List<ReticleTree.Light>() {
                new ReticleTree.Light(),
            };

            reticle_cached_atgm.tree.lights[0] = ReticleMesh.cachedReticles["T55"].tree.lights[0];
            reticle_cached_atgm.mesh = null;

            reticleSO_atgm.planes[0].elements = new List<ReticleTree.TransformElement>();
            ReticleTree.Angular eeeee = new ReticleTree.Angular(new Vector2(), null);
            eeeee.name = "Boresight";
            eeeee.align = ReticleTree.GroupBase.Alignment.Boresight;

            // centre chevron
            for (int i = -1; i <= 1; i += 2)
            {
                ReticleTree.Line chev_line = new ReticleTree.Line();
                chev_line.thickness.mrad = 0.1833f;
                chev_line.length.mrad = 2.0944f;
                chev_line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                chev_line.length.unit = AngularLength.AngularUnit.MIL_USSR;
                chev_line.rotation.mrad = i == 1 ? 5497.787f : 785.398f;
                chev_line.position = new ReticleTree.Position(0.6756f * i, -0.6756f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);

                ReticleTree.Line side = new ReticleTree.Line();
                side.thickness.mrad = 0.1833f;
                side.length.mrad = 7.0944f;
                side.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
                side.length.unit = AngularLength.AngularUnit.MIL_USSR;
                side.position = new ReticleTree.Position(5f * i, 0, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);

                side.illumination = ReticleTree.Light.Type.NightIllumination;
                chev_line.illumination = ReticleTree.Light.Type.NightIllumination;

                eeeee.elements.Add(chev_line);
                eeeee.elements.Add(side);
            }

            ReticleTree.Line middle_line = new ReticleTree.Line();
            middle_line.thickness.mrad = 0.1833f;
            middle_line.length.mrad = 7.0944f;
            middle_line.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
            middle_line.length.unit = AngularLength.AngularUnit.MIL_USSR;
            middle_line.position = new ReticleTree.Position(0f, -5f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
            middle_line.rotation.mrad = 1570.8f;

            ReticleTree.Line middle_line2 = new ReticleTree.Line();
            middle_line2.thickness.mrad = 0.1833f;
            middle_line2.length.mrad = 7.0944f / 2f;
            middle_line2.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
            middle_line2.length.unit = AngularLength.AngularUnit.MIL_USSR;
            middle_line2.position = new ReticleTree.Position(-3f, -5f, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);

            ReticleTree.Line middle_line3 = new ReticleTree.Line();
            middle_line3.thickness.mrad = 0.1833f;
            middle_line3.length.mrad = 5.0944f / 2f;
            middle_line3.thickness.unit = AngularLength.AngularUnit.MIL_USSR;
            middle_line3.length.unit = AngularLength.AngularUnit.MIL_USSR;
            middle_line3.position = new ReticleTree.Position(5f, -7.0944f / 2, AngularLength.AngularUnit.MIL_NATO, LinearLength.LinearUnit.M);
            middle_line3.rotation.mrad = 1570.8f;

            middle_line.illumination = ReticleTree.Light.Type.NightIllumination;
            middle_line2.illumination = ReticleTree.Light.Type.NightIllumination;
            middle_line3.illumination = ReticleTree.Light.Type.NightIllumination;

            eeeee.elements.Add(middle_line);
            eeeee.elements.Add(middle_line2);
            eeeee.elements.Add(middle_line3);

            reticleSO_atgm.planes[0].elements.Add(eeeee);

            assets_loaded = true;
        }
    }
}
