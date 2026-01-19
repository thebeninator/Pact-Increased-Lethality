using UnityEngine;
using GHPC.Equipment;

namespace PactIncreasedLethality
{
    public class Relikt
    {
        public static EraSchema schema = new EraSchema()
        {
            era_so = ScriptableObject.CreateInstance<ArmorCodexScriptable>(),
            era_armour = new ArmorType(),
            name = "Relikt",
            heat_rha = 400f,
            ke_rha = 250f,
        };

        public static void Setup(Transform era_armour_parent, Transform visual_parent, 
            bool hide_on_detonate = true, Material destroyed_mat = null, string destroyed_target = "")
        {
            PactEra.Setup(Relikt.schema, era_armour_parent.transform, visual_parent.transform, hide_on_detonate, destroyed_mat, destroyed_target);
        }
    }
}