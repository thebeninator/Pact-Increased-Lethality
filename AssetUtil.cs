using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GHPC.Mission;
using GHPC.Vehicle;
using UnityEngine.AddressableAssets;

namespace PactIncreasedLethality
{
    internal class AssetUtil
    {
        private static UnitPrefabLookupScriptable.UnitPrefabMetadata[] lookup_all_units;
        private static List<AssetReference> loaded_asset_references = new List<AssetReference>();

        internal static Vehicle LoadVanillaVehicle(string name)
        {
            if (lookup_all_units == null)
            {
                lookup_all_units = Resources.FindObjectsOfTypeAll<UnitPrefabLookupScriptable>().FirstOrDefault().AllUnits;
            }
            AssetReference prefab_ref = lookup_all_units.Where(o => o.Name == name).FirstOrDefault().PrefabReference;

            if (prefab_ref.Asset == null)
            {
                loaded_asset_references.Add(prefab_ref);
                return prefab_ref.LoadAssetAsync<GameObject>().WaitForCompletion().GetComponent<Vehicle>();
            }

            return (prefab_ref.Asset as GameObject).GetComponent<Vehicle>();
        }

        internal static void ReleaseVanillaAssets()
        {
            foreach (AssetReference prefab in loaded_asset_references)
            {
                prefab.ReleaseAsset();
            }

            loaded_asset_references.Clear();
        }

        internal static bool VehicleInMission(string name)
        {
            foreach (var unit in UnitSpawner.Instance._loadedUnits)
            {
                if (unit.Asset.name == name)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool VehicleInMission(string[] name)
        {
            foreach (var unit in UnitSpawner.Instance._loadedUnits)
            {
                if (name.Contains(unit.Asset.name))
                {
                    return true;
                }
            }

            return false;
        }

        private static void CloneVanillaGameObject(ref GameObject dest, GameObject source)
        {
            source.SetActive(false);
            dest = GameObject.Instantiate(source);
            source.SetActive(true);
        }

        private static void CloneVanillaMaterial(ref Material dest, Material source)
        {
            dest = new Material(source);
        }
    }
}

