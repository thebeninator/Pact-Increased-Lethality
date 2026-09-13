using System.Collections.Generic;
using System.Linq;
using CustomVicUnityScripts;
using GHPC;
using UnityEngine;

namespace CustomVicUtil
{
    internal class Helpers
    {
        private static Material aar_generic_mat;

        public static void ProcessArmourScripts(Transform holder, Dictionary<string, ArmorCodexScriptable> armour_codices)
        {
            if (aar_generic_mat == null)
            {
                aar_generic_mat = Resources.FindObjectsOfTypeAll<Material>().Where(o => o.name == "AarGeneric").First();
            }

            foreach (Transform t in holder)
            {
                t.gameObject.layer = 8;
                t.gameObject.tag = "Penetrable";

                CVUniformArmor cv_uniform = t.GetComponent<CVUniformArmor>();
                CVVariableArmor cv_variable = t.GetComponent<CVVariableArmor>();
                CVAarVisual cv_aar_visual = t.GetComponent<CVAarVisual>();

                if (cv_aar_visual != null)
                {
                    AarVisual aar_visual = t.gameObject.AddComponent<AarVisual>();
                    aar_visual.HideUntilAar = cv_aar_visual.HideUntilAar;
                    aar_visual.AarMaterial = cv_aar_visual.UseGenericMaterial ? aar_generic_mat : cv_aar_visual.AarMaterial;
                    aar_visual.PreserveLayerUntilAar = cv_aar_visual.PreserveLayerUntilAar;
                    aar_visual.SwitchMaterials = cv_aar_visual.SwitchMaterials;
                }

                if (cv_uniform != null)
                {
                    UniformArmor uniform = t.gameObject.AddComponent<UniformArmor>();

                    uniform._name = cv_uniform.Name;
                    uniform.PrimaryHeatRha = cv_uniform.PrimaryHeatRha;
                    uniform.PrimarySabotRha = cv_uniform.PrimarySabotRha;
                    uniform._canRicochet = cv_uniform.CanRicochet;
                    uniform._canShatterLongRods = cv_uniform.CanShatterLongRods;
                    uniform.AngleMatters = cv_uniform.AngleMatters;
                    uniform.ThicknessListed = cv_uniform.ThicknessListedIsActual 
                        ? UniformArmor.ThicknessMode.ActualThickness : UniformArmor.ThicknessMode.RHAE;
                    if (cv_uniform.ArmorCodexId != "")
                    {
                        uniform._armorType = armour_codices[cv_uniform.ArmorCodexId];
                    }

                    Component.DestroyImmediate(cv_uniform);
                }

                if (cv_variable != null)
                {
                    VariableArmor variable = t.gameObject.AddComponent<VariableArmor>();

                    variable._name = cv_variable.Name;

                    if (cv_variable.ArmorCodexId != "")
                    {
                        variable._armorType = armour_codices[cv_variable.ArmorCodexId];
                    }

                    Component.DestroyImmediate(cv_variable);
                }

                if (t.childCount > 0)
                {
                    ProcessArmourScripts(t, armour_codices);
                }
            }
        }
    }
}
