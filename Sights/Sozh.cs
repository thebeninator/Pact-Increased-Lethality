using System.Collections.Generic;
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

            Angular angular_root = new Angular
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

            void create_vert_line(int offset, bool down)
            {
                Line line = new Line();
                float y = (7.2f + 2.283f * offset) * (down ? -1 : 1);
                line.position = new Position(0f, y, AngularUnit.MIL_USSR);
                line.length = new AngularLength(1.25f, AngularUnit.MIL_USSR);
                line.thickness = new AngularLength(0.15f, AngularUnit.MIL_USSR);
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

            angular_root.elements.Add(lines_root);
            angular_root.elements.Add(chevron_root);
            ffp.elements.Add(angular_root);
            planes.Add(ffp);
        }

        public override void LoadStaticAssets()
        {
            DayReticle();
        }
    }
}
