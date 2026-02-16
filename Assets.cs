using System.Collections.Generic;
using System.Linq;
using GHPC.Camera;
using GHPC.Effects.Voices;
using GHPC.Mission;
using GHPC.Vehicle;
using GHPC.Weaponry;
using NWH.VehiclePhysics;
using Reticle;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactIncreasedLethality
{
    internal class Assets
    {
        private static bool done = false;

        internal static AmmoType ammo_3bm32;
        internal static AmmoType ammo_kobra;
        internal static AmmoClipCodexScriptable clip_codex_3bm22;
        internal static AmmoClipCodexScriptable clip_codex_3bm32;

        internal static AmmoClipCodexScriptable clip_codex_3bk18m;

        internal static VehicleController abrams_vic_controller;

        internal static AmmoType ammo_3bk5m;
        internal static AmmoType ammo_3of412;
        internal static AmmoType ammo_9m111;
        internal static AmmoType ammo_3bm20;
        internal static AmmoClipCodexScriptable clip_codex_br412d;

        internal static AmmoType ammo_9m113;

        internal static GameObject soviet_crew_voice;
        internal static GameObject m1ip_range_canvas;
        internal static GameObject m60a1_nvs;
        internal static GameObject m2_bradley_canvas;

        internal static GameObject t80b_canvas;

        internal static GameObject crt_shock_go;
        internal static TMP_FontAsset tpd_etch_sdf;

        internal static Material green_flir_mat;
        internal static GameObject flir_post_green;

        internal static GameObject brdm2_hull;

        private static List<AssetReference> loaded_asset_references = new List<AssetReference>();

        internal static Vehicle LoadVanillaVehicle(string name)
        {
            var lookup_all_units = Resources.FindObjectsOfTypeAll<UnitPrefabLookupScriptable>().FirstOrDefault().AllUnits;

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

        internal static void Load()
        {
            if (done) return;

            Vehicle m1ip = LoadVanillaVehicle("M1IP");
            Transform m1ip_flir = m1ip.transform.Find("Turret Scripts/GPS/FLIR");
            //abrams_vic_controller = m1ip.GetComponent<VehicleController>(); //FIXME
            CloneVanillaGameObject(ref m1ip_range_canvas, m1ip.transform.Find("Turret Scripts/GPS/Optic/Abrams GPS canvas").gameObject);
            CloneVanillaGameObject(ref crt_shock_go, m1ip_flir.Find("Scanline FOV change").gameObject);
            CloneVanillaGameObject(ref flir_post_green, m1ip_flir.Find("FLIR Post Processing - Green").gameObject);
            CloneVanillaMaterial(ref green_flir_mat, m1ip_flir.GetComponent<CameraSlot>().FLIRBlitMaterialOverride);
            m1ip_flir.Find("Reticle Mesh WFOV").GetComponent<ReticleMesh>().Load();

            Vehicle bmp2 = LoadVanillaVehicle("BMP2_SA");
            bmp2.transform.Find("fire control/gunner day sight 1P3-3/Optic/Reticle Mesh").GetComponent<ReticleMesh>().Load();

            Vehicle m60a1 = LoadVanillaVehicle("M60A1");
            CloneVanillaGameObject(ref m60a1_nvs, m60a1.transform.Find("Turret Scripts/Sights/NVS").gameObject);

            Vehicle t64b = LoadVanillaVehicle("T64B");
            t64b.transform.Find("---MAIN GUN SCRIPTS---/2A46/TPN‑3‑49 night sight/Reticle Mesh").GetComponent<ReticleMesh>().Load();

            Vehicle t55a = LoadVanillaVehicle("T55A");
            t55a.transform.Find("Gun Scripts/Sights (and FCS)/NVS/Reticle Mesh").GetComponent<ReticleMesh>().Load();
            t55a.WeaponsManager.Weapons[0].FCS.AuthoritativeOptic.reticleMesh.Load();

            Vehicle m2_bradley = LoadVanillaVehicle("M2BRADLEY");
            CloneVanillaGameObject(ref m2_bradley_canvas, m2_bradley.transform.Find("FCS and sights/GPS Optic/M2 Bradley GPS canvas").gameObject);

            Vehicle t80b = LoadVanillaVehicle("T80B");
            CloneVanillaGameObject(ref t80b_canvas, t80b.transform.Find("---MAIN GUN SCRIPTS---/2A46-2/1G42 gunner's sight/GPS/1G42 Canvas").gameObject);
            CloneVanillaGameObject(ref soviet_crew_voice, t80b.GetComponentInChildren<CrewVoiceHandler>().gameObject);

            Vehicle brdm2 = LoadVanillaVehicle("BRDM2");
            CloneVanillaGameObject(ref brdm2_hull, brdm2.transform.Find("BRDM2_1983 (1)/BRDM_hull_1983").gameObject);

            //LoadVanillaVehicle("STATIC_9K111_SA"); // ammo

            AmmoClipCodexScriptable[] clip_codex_scriptables = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>();
            AmmoCodexScriptable[] codex_scriptables = Resources.FindObjectsOfTypeAll<AmmoCodexScriptable>();

            clip_codex_3bk18m = clip_codex_scriptables.Where(o => o.name == "clip_3BK18M").FirstOrDefault();
            clip_codex_3bm22 = clip_codex_scriptables.Where(o => o.name == "clip_3BM22").FirstOrDefault();
            clip_codex_3bm32 = clip_codex_scriptables.Where(o => o.name == "clip_3BM32").FirstOrDefault();
            ammo_3bm32 = clip_codex_3bm32.ClipType.MinimalPattern[0].AmmoType;

            ammo_kobra = codex_scriptables.Where(o => o.name == "ammo_9M112M").FirstOrDefault().AmmoType;

            //ammo_3bk5m = codex_scriptables.Where(o => o.name == "ammo_3BK5M").FirstOrDefault().AmmoType;
            //ammo_3of412 = codex_scriptables.Where(o => o.name == "ammo_3OF412").FirstOrDefault().AmmoType;
            //ammo_3bm20 = codex_scriptables.Where(o => o.name == "ammo_3BM20").FirstOrDefault().AmmoType;
            //clip_codex_br412d = clip_codex_scriptables.Where(o => o.name == "clip_BR-412D").FirstOrDefault();

            //ammo_9m111 = codex_scriptables.Where(o => o.name == "ammo_9M111").FirstOrDefault().AmmoType;
            //ammo_9m113 = codex_scriptables.Where(o => o.name == "ammo_9M113").FirstOrDefault().AmmoType;

            //tpd_etch_sdf = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Where(o => o.name == "TPD_Etch SDF").FirstOrDefault();

            done = true;
        }
    }
}
