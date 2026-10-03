using System.Collections.Generic;
using System.Linq;
using CustomVicUnityScripts;
using GHPC;
using GHPC.Effects;
using GHPC.Equipment;
using UnityEngine;

namespace CustomVicUtil
{
    internal class Helpers
    {
        private static Dictionary<CVGenericAarMat, Material> generic_aar_mats = new Dictionary<CVGenericAarMat, Material>();

        public static void ProcessCVScripts(Transform holder, Dictionary<string, ArmorCodexScriptable> armour_codices = null)
        {
            if (generic_aar_mats.Count == 0)
            {
                Material find_aar_material(string name)
                {
                    return Resources.FindObjectsOfTypeAll<Material>().Where(o => o.name == name).First();
                }

                generic_aar_mats.Add(CVGenericAarMat.Generic, find_aar_material("AarGeneric"));
                generic_aar_mats.Add(CVGenericAarMat.HE, find_aar_material("AarHE"));
                generic_aar_mats.Add(CVGenericAarMat.HEAT, find_aar_material("AarHEAT"));
                generic_aar_mats.Add(CVGenericAarMat.APFSDS, find_aar_material("AarAPFSDS"));
                generic_aar_mats.Add(CVGenericAarMat.APHE, find_aar_material("AarAPHE"));
                generic_aar_mats.Add(CVGenericAarMat.ATGM, find_aar_material("AarATGM"));
                generic_aar_mats.Add(CVGenericAarMat.Crew, find_aar_material("AarCrew"));
                generic_aar_mats.Add(CVGenericAarMat.Flammable, find_aar_material("AarFlammable"));
            }

            foreach (Transform t in holder)
            {
                CVUniformArmor cv_uniform = t.GetComponent<CVUniformArmor>();
                CVVariableArmor cv_variable = t.GetComponent<CVVariableArmor>();
                CVAarVisual cv_aar_visual = t.GetComponent<CVAarVisual>();
                CVFlammableItem cv_flammable_item = t.GetComponent<CVFlammableItem>();
                CVDestructibleComponent cv_destructible_component = t.GetComponent<CVDestructibleComponent>();

                if (cv_flammable_item != null)
                {
                    FlammableItem flammable_item = t.gameObject.AddComponent<FlammableItem>();
                    flammable_item._selfOxidizing = cv_flammable_item.SelfOxidizing;
                    flammable_item._ignitionOverpressureBar = cv_flammable_item.IgnitionOverpressureBar;
                    flammable_item._explosive = cv_flammable_item.Explosive;
                    flammable_item._flameHeightMeters = cv_flammable_item.FlameHeightMetres;
                    flammable_item._ignitionTempMax = cv_flammable_item.IgnitionTempMax;
                    flammable_item._ignitionTempMin = cv_flammable_item.IgnitionTempMin;
                    flammable_item._shortestBurnTime = cv_flammable_item.ShortestBurnTime;
                    flammable_item.HeatTransferRatio = cv_flammable_item.HeatTransferRatio;
                    flammable_item._burnTemperature = cv_flammable_item.BurnTemperature;
                    flammable_item._tntEquivalent = cv_flammable_item.TntEquivalent;

                    Component.DestroyImmediate(cv_flammable_item);
                }

                if (cv_destructible_component != null)
                {
                    GHPC.Equipment.DestructibleComponent destructible_component = t.gameObject.AddComponent<GHPC.Equipment.DestructibleComponent>();
                    destructible_component._name = cv_destructible_component.Name;
                    destructible_component._health = cv_destructible_component.Health;
                    destructible_component._damageThreshold = cv_destructible_component.DamageThreshold;
                    destructible_component._pressureTolerance = cv_destructible_component.PressureTolerance;

                    Component.DestroyImmediate(cv_destructible_component);
                }

                if (cv_aar_visual != null)
                {
                    AarVisual aar_visual = t.gameObject.AddComponent<AarVisual>();
                    aar_visual.RenderMode = (AarVisual.AarVisualRenderMode)cv_aar_visual.RenderMode;
                    aar_visual.HideUntilAar = cv_aar_visual.HideUntilAar;
                    aar_visual.AarMaterial = cv_aar_visual.UseGenericMaterial ? generic_aar_mats[cv_aar_visual.GenericMaterial] : cv_aar_visual.AarMaterial;
                    aar_visual.PreserveLayerUntilAar = cv_aar_visual.PreserveLayerUntilAar;
                    aar_visual.SwitchMaterials = cv_aar_visual.SwitchMaterials;

                    if (cv_aar_visual.SwitchMaterialsImmediate)
                    {
                        MeshRenderer renderer = t.GetComponent<MeshRenderer>();
                        renderer.material = aar_visual.AarMaterial;
                    }

                    Component.DestroyImmediate(cv_aar_visual);
                }

                if (cv_uniform != null)
                {
                    t.gameObject.layer = 8;
                    t.gameObject.tag = "Penetrable";

                    UniformArmor uniform = t.gameObject.AddComponent<UniformArmor>();

                    uniform._name = cv_uniform.Name;
                    uniform.PrimaryHeatRha = cv_uniform.PrimaryHeatRha;
                    uniform.PrimarySabotRha = cv_uniform.PrimarySabotRha;
                    uniform._canRicochet = cv_uniform.CanRicochet;
                    uniform._canShatterLongRods = cv_uniform.CanShatterLongRods;
                    uniform.AngleMatters = cv_uniform.AngleMatters;
                    uniform.ThicknessListed = cv_uniform.ThicknessListedIsActual 
                        ? UniformArmor.ThicknessMode.ActualThickness : UniformArmor.ThicknessMode.RHAE;
                    if (cv_uniform.ArmorCodexId != "" && armour_codices != null)
                    {
                        uniform._armorType = armour_codices[cv_uniform.ArmorCodexId];
                    }

                    Component.DestroyImmediate(cv_uniform);
                }

                if (cv_variable != null)
                {
                    t.gameObject.layer = 8;
                    t.gameObject.tag = "Penetrable";

                    VariableArmor variable = t.gameObject.AddComponent<VariableArmor>();

                    variable._name = cv_variable.Name;

                    if (cv_variable.ArmorCodexId != "" && armour_codices != null)
                    {
                        variable._armorType = armour_codices[cv_variable.ArmorCodexId];
                    }

                    Component.DestroyImmediate(cv_variable);
                }

                if (t.childCount > 0)
                {
                    ProcessCVScripts(t, armour_codices);
                }
            }
        }
    }
}
