using System.Collections.Generic;
using System.Linq;
using GHPC.Weaponry;
using GHPC.Weapons;
using ModUtil;
using Reticle;
using UnityEngine;
using static Reticle.AngularLength;
using static Reticle.ReticleTree;

namespace PactIncreasedLethality
{
    internal class Sozh : Module
    {
        internal static ReticleSO day_reticleSO;
        internal static ReticleSO night_reticleSO;
        private static VerticalBallistic mg_vert_ballistic;

        private void DayReticle()
        {
            day_reticleSO = new ReticleSO();
            day_reticleSO.name = "SOZH-DAY";

            day_reticleSO.lights = new List<ReticleTree.Light>()
            {
                new ReticleTree.Light()
                {
                    type = ReticleTree.Light.Type.NightIllumination,
                    color = new RGB(5.9922f, 0.502f, 0f, true)
                }
            };

            List<FocalPlane> planes = day_reticleSO.planes;

            FocalPlane ffp = new FocalPlane(type: FocalPlane.Type.First);
            ffp.elements = new List<TransformElement>();

            Angular ffp_root = new Angular
            (
                position: Vector2.zero,
                parent: null,
                align: GroupBase.Alignment.Boresight
            );

            Angular chevron_root = new Angular
            (
                position: Vector2.zero,
                parent: null,
                align: GroupBase.Alignment.None
            );

            Angular lines_root = new Angular
            (
                position: Vector2.zero,
                parent: null,
                align: GroupBase.Alignment.None
            );

            Angular mg_range_root = new Angular
            (
                position: Vector2.zero,
                parent: null,
                align: GroupBase.Alignment.None
            );

            for (int i = -1; i <= 1; i += 2)
            {
                Line chevron_line = new Line();
                chevron_line.position = new Position(0.45f * i, -0.93f, AngularUnit.MIL_NATO);
                chevron_line.length = new AngularLength(2f, AngularUnit.MIL_USSR);
                chevron_line.thickness = new AngularLength(0.15f, AngularUnit.MIL_USSR);
                chevron_line.roundness = 1f;
                chevron_line.rotation = new AngularLength(i == -1 ? 65.03f : 294.97f, AngularUnit.DEG);
                chevron_line.visualType = VisualElement.Type.Painted;
                chevron_line.illumination = ReticleTree.Light.Type.NightIllumination;

                chevron_root.elements.Add(chevron_line);
            }

            for (int i = -1; i <= 1; i += 2)
            {
                Line long_horz = new Line();
                long_horz.position = new Position((2.283f + 4f) * i, 0f, AngularUnit.MIL_NATO);
                long_horz.length = new AngularLength(8f, AngularUnit.MIL_USSR);
                long_horz.thickness = new AngularLength(0.15f, AngularUnit.MIL_USSR);
                long_horz.roundness = 1f;
                long_horz.visualType = VisualElement.Type.Painted;
                long_horz.illumination = ReticleTree.Light.Type.NightIllumination;

                lines_root.elements.Add(long_horz);
            }

            void create_vert_line(int offset, bool down)
            {
                Line line = new Line();
                float y = (6.5f + 2.283f * offset) * (down ? -1 : 1);
                line.position = new Position(0.33f, y, AngularUnit.MIL_USSR);
                line.length = new AngularLength(0.660f, AngularUnit.MIL_USSR);
                line.thickness = new AngularLength(0.15f, AngularUnit.MIL_USSR);
                line.roundness = 1f;
                line.visualType = VisualElement.Type.Painted;
                line.illumination = ReticleTree.Light.Type.NightIllumination;

                lines_root.elements.Add(line);
            }

            void create_horz_line(int offset, bool left)
            {
                Line line = new Line();
                float x = (3f + 2.283f * offset) * (left ? -1 : 1);
                line.position = new Position(x, -0.3125f, AngularUnit.MIL_USSR);
                line.length = new AngularLength(0.625f, AngularUnit.MIL_USSR);
                line.thickness = new AngularLength(0.15f, AngularUnit.MIL_USSR);
                line.rotation = new AngularLength(90f, AngularUnit.DEG);
                line.roundness = 1f;
                line.visualType = VisualElement.Type.Painted;
                line.illumination = ReticleTree.Light.Type.NightIllumination;

                lines_root.elements.Add(line);
            }

            for (int i = 0; i < 5; i++)
            {
                if (i < 4)
                {
                    create_vert_line(i, false);
                }

                create_vert_line(i, true);
            }

            for (int i = 0; i < 3; i++)
            {
                create_horz_line(i, false);
                create_horz_line(i, true);
            }

            mg_vert_ballistic = new VerticalBallistic(Vector2.zero, mg_range_root);
            AmmoCodexScriptable mg_ammo = Resources.FindObjectsOfTypeAll<AmmoCodexScriptable>()
                .Where(o => o.name == "ammo_762x54R").First();

            mg_vert_ballistic.projectile = mg_ammo;
            mg_vert_ballistic.bc = BallisticComputerRepository.Instance.GetOrRegister(mg_ammo.AmmoType);

            for (int i = 0; i < 3; i++)
            {
                Angular range_marking_root = new Angular(new Vector2(-0.625f, 400f + 200 * i), mg_vert_ballistic);

                ReticleTree.Text range_text = new ReticleTree.Text();
                range_text.font = SharedAssets.tpd_etch_sdf;
                range_text.fontSize = 12f;
                range_text.text = ((400 + 200 * i) / 100).ToString();
                range_text.illumination = ReticleTree.Light.Type.NightIllumination;
                range_text.visualType = VisualElement.Type.Painted;
                range_text.position = new ReticleTree.Position(-2.5f, 0f);

                Line range_line = new Line();
                range_line.position = new Position(0f, 0f, AngularUnit.MIL_USSR);
                range_line.length = new AngularLength(1.25f, AngularUnit.MIL_USSR);
                range_line.thickness = new AngularLength(0.15f, AngularUnit.MIL_USSR);
                range_line.roundness = 1f;
                range_line.visualType = VisualElement.Type.Painted;
                range_line.illumination = ReticleTree.Light.Type.NightIllumination;

                range_marking_root.elements.Add(range_text);
                range_marking_root.elements.Add(range_line);
                mg_vert_ballistic.elements.Add(range_marking_root);
            }

            mg_range_root.elements.Add(mg_vert_ballistic);

            Line chevron_vert_line = new Line();
            chevron_vert_line.position = new Position(0, -0.625f*4.5f, AngularUnit.MIL_USSR);
            chevron_vert_line.length = new AngularLength(1.25f, AngularUnit.MIL_USSR);
            chevron_vert_line.thickness = new AngularLength(0.15f, AngularUnit.MIL_USSR);
            chevron_vert_line.rotation = new AngularLength(90f, AngularUnit.DEG);
            chevron_vert_line.roundness = 1f;
            chevron_vert_line.visualType = VisualElement.Type.Painted;
            chevron_vert_line.illumination = ReticleTree.Light.Type.NightIllumination;
            lines_root.elements.Add(chevron_vert_line);

            ffp_root.elements.Add(mg_range_root);
            ffp_root.elements.Add(lines_root);
            ffp_root.elements.Add(chevron_root);
            ffp.elements.Add(ffp_root);
            planes.Add(ffp);
        }

        public override void UnloadDynamicAssets()
        {
            ScriptableObject.DestroyImmediate(day_reticleSO);
            ScriptableObject.DestroyImmediate(night_reticleSO);
        }

        public override void LoadDynamicAssets()
        {
            DayReticle();
        }
    }
}
