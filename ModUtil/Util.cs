using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GHPC;
using GHPC.Equipment.Optics;
using GHPC.Weapons;
using GHPC.Thermals;
using UnityEngine;
using GHPC.Effects;

namespace ModUtil
{
    public sealed class AlreadyConverted : MonoBehaviour
    {
        void Awake() 
        {
            enabled = false;
        }
    }

    public sealed class Util
    {
        public static ImpactEffectsDatabaseScriptable impact_fx_db;

        public static string[] menu_screens = new string[] {
            "MainMenu2_Scene",
            "MainMenu2-1_Scene",
            "LOADER_MENU",
            "LOADER_INITIAL",
            "t64_menu"
        };

        public static void CacheAmmo(AmmoType ammo)
        {
            if (impact_fx_db == null)
            {
                impact_fx_db = Resources.FindObjectsOfTypeAll<ImpactEffectsDatabaseScriptable>()[0];
            }

            int id;
            ImpactDecalsManager.Instance._ImpactDecalsScriptable.CacheNewData(ammo, out id);
            impact_fx_db.CacheNewData(ammo, out id);
            ammo.CachedIndex = id;
        }

        public static void CreateUniformArmour(GameObject go, string name, float rha_sabot, float rha_heat, ArmorCodexScriptable codex = null) {
            UniformArmor component = go.AddComponent<UniformArmor>();
            go.tag = "Penetrable";
            go.layer = 8;
            component.SetName(name);
            component.PrimaryHeatRha = rha_heat;
            component.PrimarySabotRha = rha_sabot;

            if (codex != null) {
                component._armorType = codex;
            }
        }

        public static T[] AppendToArray<T>(T[] array, T new_item)
        {
            List<T> values = new List<T>();
            foreach (T old_item in array)
            {
                values.Add(old_item);
            }

            values.Add(new_item);

            return values.ToArray();
        }

        public static void Coalesce<T>(ref T obj) where T : new()
        {
            if (obj != null) return;
            obj = new T();
        }

        public static void SetupFLIRShaders(GameObject parent)
        {
            foreach (MeshRenderer mrend in parent.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material mat in mrend.materials)
                {
                    mat.shader = Shader.Find("Standard (FLIR)");
                }
            }

            HeatSource src = parent.AddComponent<HeatSource>();
            src.heat = 0.4f;
        }

        public static void ShallowCopy(System.Object dest, System.Object src)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] destFields = dest.GetType().GetFields(flags);
            FieldInfo[] srcFields = src.GetType().GetFields(flags);

            foreach (FieldInfo srcField in srcFields)
            {
                FieldInfo destField = destFields.FirstOrDefault(field => field.Name == srcField.Name);

                if (destField != null && !destField.IsLiteral)
                {
                    if (srcField.FieldType == destField.FieldType)
                        destField.SetValue(dest, srcField.GetValue(src));
                }
            }
        }

        public static UsableOptic GetDayOptic(FireControlSystem fcs)
        {
            if (fcs.MainOptic.slot.IsLinkedNightSight)
            {
                return fcs.MainOptic.slot.LinkedDaySight.PairedOptic;
            }
            else
            {
                return fcs.MainOptic;
            }
        }

        public static void EmptyRack(GHPC.Weapons.AmmoRack rack)
        {
            MethodInfo removeVis = typeof(GHPC.Weapons.AmmoRack).GetMethod("RemoveAmmoVisualFromSlot", BindingFlags.Instance | BindingFlags.NonPublic);

            PropertyInfo stored_clips = typeof(GHPC.Weapons.AmmoRack).GetProperty("StoredClips");
            stored_clips.SetValue(rack, new List<AmmoType.AmmoClip>());

            rack.SlotIndicesByAmmoType = new Dictionary<AmmoType, List<byte>>();

            foreach (Transform transform in rack.VisualSlots)
            {
                AmmoStoredVisual vis = transform.GetComponentInChildren<AmmoStoredVisual>();

                if (vis != null && vis.AmmoType != null)
                {
                    removeVis.Invoke(rack, new object[] { transform });
                }
            }
        }
    }
}