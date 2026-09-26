using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GHPC.Mission;
using GHPC.Vehicle;
using UnityEngine.AddressableAssets;
using System.Collections;
using GHPC.State;
using MelonLoader;
using UnityEngine.Scripting;
using System;

namespace ModUtil
{
    internal class AssetUtil
    {
        private static UnitPrefabLookupScriptable.UnitPrefabMetadata[] lookup_all_units;
        private static List<AssetReference> loaded_asset_references = new List<AssetReference>();
        private static List<AssetReference> temp_asset_references = new List<AssetReference>();
        private static List<GameObject> cloned_vanilla_assets = new List<GameObject>();

        internal static Vehicle LoadVanillaVehicle(string name, bool temp = false)
        {
            if (lookup_all_units == null)
            {
                lookup_all_units = Resources.FindObjectsOfTypeAll<UnitPrefabLookupScriptable>().FirstOrDefault().AllUnits;
            }

            AssetReference prefab_ref = lookup_all_units.Where(o => o.Name == name).FirstOrDefault().PrefabReference;

            if (prefab_ref.Asset == null)
            {
                if (temp && !loaded_asset_references.Contains(prefab_ref))
                {
                    temp_asset_references.Add(prefab_ref);
                } 
                
                if (!temp)
                {
                    loaded_asset_references.Add(prefab_ref);

                    if (temp_asset_references.Contains(prefab_ref))
                    {
                        temp_asset_references.Remove(prefab_ref);
                    }
                }

                return prefab_ref.LoadAssetAsync<GameObject>().WaitForCompletion().GetComponent<Vehicle>();
            }

            return (prefab_ref.Asset as GameObject).GetComponent<Vehicle>();
        }

        internal static void ReleaseTempVanillaAssets()
        {
            ReleaseAssets(temp_asset_references, true);
        }

        internal static void ReleaseVanillaAssets()
        {
            ReleaseAssets(loaded_asset_references);
        }

        internal static IEnumerator ReleaseTempVanillaAssetsDeferred(GameState _)
        {
            ReleaseTempVanillaAssets();
            yield break;
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

        public static GameObject CloneVanillaGameObject(string from_id, string path)
        {
            GameObject from = LoadVanillaVehicle(from_id, true).gameObject;
            GameObject to_clone = from.transform.Find(path).gameObject;

            to_clone.SetActive(false);
            GameObject clone = GameObject.Instantiate(to_clone);
            clone.name = clone.name.Substring(0, clone.name.Length - "(Clone)".Length);
            //cloned_vanilla_assets.Add(clone);
            to_clone.SetActive(true);

            return clone;
        }

        private static void CloneVanillaMaterial(ref Material dest, Material source)
        {
            dest = new Material(source);
        }

        private static void ReleaseAssets(List<AssetReference> asset_references, bool hard_destroy = false)
        {
            foreach (AssetReference prefab in asset_references)
            {
                if (hard_destroy)
                {
                    GameObject.Destroy(prefab.Asset as GameObject);
                }
                prefab.ReleaseAsset();
            }

            asset_references.Clear();
        }
    }
}