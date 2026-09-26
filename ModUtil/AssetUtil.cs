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
using UnityEngine.SceneManagement;

// TODO: gameobject singleton for asset refs

namespace ModUtil
{
    internal class AssetPrefabReferenceDatabase : MonoBehaviour
    {
        public List<AssetReference> LoadedAssetReferences = new List<AssetReference>();
        public List<AssetReference> TempAssetReferences = new List<AssetReference>();
        public static AssetPrefabReferenceDatabase Instance;

        void OnDestroy()
        {
            ReleaseVanillaAssets();
            ReleaseTempVanillaAssets();
        }

        public static void Create(int build_idx)
        {
            if (Instance != null) return;

            GameObject maybe_db = GameObject.Find("ASSET REF DATABASE");

            if (maybe_db != null)
            {
                Instance = maybe_db.GetComponent<AssetPrefabReferenceDatabase>();
                return;
            }

            GameObject db = new GameObject("ASSET REF DATABASE");
            Instance = db.AddComponent<AssetPrefabReferenceDatabase>();
            SceneManager.MoveGameObjectToScene(db, SceneManager.GetSceneByBuildIndex(build_idx));
        }

        public void AddReference(AssetReference prefab_ref, bool temp = false)
        {
            if (temp && !LoadedAssetReferences.Contains(prefab_ref))
            {
                TempAssetReferences.Add(prefab_ref);
            }

            if (!temp)
            {
                LoadedAssetReferences.Add(prefab_ref);

                if (TempAssetReferences.Contains(prefab_ref))
                {
                    TempAssetReferences.Remove(prefab_ref);
                }
            }
        }

        public void ReleaseTempVanillaAssets()
        {
            ReleaseAssets(TempAssetReferences, true);
        }

        public void ReleaseVanillaAssets()
        {
            ReleaseAssets(LoadedAssetReferences);
        }

        public IEnumerator ReleaseTempVanillaAssetsDeferred(GameState _)
        {
            ReleaseTempVanillaAssets();
            yield break;
        }

        private void ReleaseAssets(List<AssetReference> asset_references, bool hard_destroy = false)
        {
            foreach (AssetReference prefab in asset_references)
            {
                if (hard_destroy)
                {
                    GameObject.DestroyImmediate(prefab.Asset);
                }
                prefab.ReleaseAsset();
            }

            asset_references.Clear();
        }
    }

    internal class AssetUtil
    {
        private static UnitPrefabLookupScriptable.UnitPrefabMetadata[] lookup_all_units;
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
                AssetPrefabReferenceDatabase.Instance.AddReference(prefab_ref, temp);
                return prefab_ref.LoadAssetAsync<GameObject>().WaitForCompletion().GetComponent<Vehicle>();
            }

            return (prefab_ref.Asset as GameObject).GetComponent<Vehicle>();
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
    }
}