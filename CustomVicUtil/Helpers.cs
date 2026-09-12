using CustomVicUnityScripts;
using GHPC;
using UnityEngine;

namespace CustomVicUtil
{
    internal class Helpers
    {
        public static void ProcessArmourScripts(Transform holder)
        {
            void process(Transform t)
            {
                t.gameObject.layer = 8;
                t.gameObject.tag = "Penetrable";

                CVUniformArmor cv_uniform = t.GetComponent<CVUniformArmor>();
                CVVariableArmor cv_variable = t.GetComponent<CVVariableArmor>();

                if (cv_uniform != null)
                {
                    UniformArmor uniform = t.gameObject.AddComponent<UniformArmor>();

                    uniform.PrimaryHeatRha = cv_uniform.PrimaryHeatRha;
                    uniform.PrimarySabotRha = cv_uniform.PrimarySabotRha;
                    uniform._canRicochet = cv_uniform.CanRicochet;
                    uniform._canShatterLongRods = cv_uniform.CanShatterLongRods;
                    uniform.AngleMatters = cv_uniform.AngleMatters;
                    uniform.ThicknessListed = cv_uniform.ThicknessListedIsActual ? UniformArmor.ThicknessMode.ActualThickness : UniformArmor.ThicknessMode.RHAE;
                    uniform._name = cv_uniform.Name;

                    Component.DestroyImmediate(cv_uniform);
                }

                if (cv_variable != null)
                {
                    VariableArmor variable = t.gameObject.AddComponent<VariableArmor>();

                    variable._name = cv_variable.Name;

                    Component.DestroyImmediate(cv_variable);
                }

                if (t.childCount > 0)
                {
                    ProcessArmourScripts(t);
                }
            }

            foreach (Transform t in holder)
            {
                process(t);
            }
        }
    }
}
