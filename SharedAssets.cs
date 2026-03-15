using System.Linq;
using GHPC.Camera;
using GHPC.Vehicle;
using GHPC.Weaponry;
using NWH.VehiclePhysics;
using Reticle;
using TMPro;
using UnityEngine;

namespace PactIncreasedLethality
{
    internal class SharedAssets : Module
    {
        internal static AmmoType ammo_3bm32;
        internal static AmmoType ammo_kobra;
        internal static AmmoClipCodexScriptable clip_codex_3bm22;
        internal static AmmoClipCodexScriptable clip_codex_3bm32;

        internal static AmmoClipCodexScriptable clip_codex_3bk18m;

        internal static VehicleController abrams_vic_controller;

        internal static GameObject m1ip_range_canvas;
        internal static GameObject m2_bradley_canvas;

        internal static GameObject t80b_canvas;

        internal static GameObject crt_shock_go;
        internal static TMP_FontAsset tpd_etch_sdf;

        internal static Material green_flir_mat;
        internal static GameObject flir_post_green;

        internal static GameObject brdm2_hull;

        internal static Material t80b_mat;
        internal static TMP_FontAsset sov_7_segment_font;

        public override void LoadStaticAssets()
        {
            AmmoClipCodexScriptable[] clip_codex_scriptables = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>();
            AmmoCodexScriptable[] codex_scriptables = Resources.FindObjectsOfTypeAll<AmmoCodexScriptable>();

            clip_codex_3bk18m = clip_codex_scriptables.Where(o => o.name == "clip_3BK18M").FirstOrDefault();
            clip_codex_3bm22 = clip_codex_scriptables.Where(o => o.name == "clip_3BM22").FirstOrDefault();
            clip_codex_3bm32 = clip_codex_scriptables.Where(o => o.name == "clip_3BM32").FirstOrDefault();
            ammo_3bm32 = clip_codex_3bm32.ClipType.MinimalPattern[0].AmmoType;

            ammo_kobra = codex_scriptables.Where(o => o.name == "ammo_9M112M").FirstOrDefault().AmmoType;

            tpd_etch_sdf = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Where(o => o.name == "TPD_Etch SDF").FirstOrDefault();
        }

        public override void LoadDynamicAssets()
        {
            Vehicle m1ip = AssetUtil.LoadVanillaVehicle("M1IP");
            Transform m1ip_flir = m1ip.transform.Find("Turret Scripts/GPS/FLIR");
            abrams_vic_controller = m1ip.GetComponent<VehicleController>();
            m1ip_range_canvas = m1ip.transform.Find("Turret Scripts/GPS/Optic/Abrams GPS canvas").gameObject;
            crt_shock_go = m1ip_flir.Find("Scanline FOV change").gameObject;
            flir_post_green = m1ip_flir.Find("FLIR Post Processing - Green").gameObject;
            green_flir_mat = m1ip_flir.GetComponent<CameraSlot>().FLIRBlitMaterialOverride;
            m1ip_flir.Find("Reticle Mesh WFOV").GetComponent<ReticleMesh>().Load();

            Vehicle t55a = AssetUtil.LoadVanillaVehicle("T55A");
            t55a.transform.Find("Gun Scripts/Sights (and FCS)/NVS/Reticle Mesh").GetComponent<ReticleMesh>().Load();
            t55a.WeaponsManager.Weapons[0].FCS.AuthoritativeOptic.reticleMesh.Load();

            Vehicle m2_bradley = AssetUtil.LoadVanillaVehicle("M2BRADLEY");
            m2_bradley_canvas = m2_bradley.transform.Find("FCS and sights/GPS Optic/M2 Bradley GPS canvas").gameObject;

            Vehicle t80b = AssetUtil.LoadVanillaVehicle("T80B");
            t80b_canvas = t80b.transform.Find("---MAIN GUN SCRIPTS---/2A46-2/1G42 gunner's sight/GPS/1G42 Canvas").gameObject;
            t80b_mat = t80b.transform.Find("T80_mesh/body").GetComponent<MeshRenderer>().materials[0];
            t80b.transform.Find("---MAIN GUN SCRIPTS---/2A46-2/TPN‑3‑49 night sight/Reticle Mesh").GetComponent<ReticleMesh>().Load();
            sov_7_segment_font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Where(o => o.name == "Soviet7SegmentFont SDF").FirstOrDefault();

            Vehicle brdm2 = AssetUtil.LoadVanillaVehicle("BRDM2");
            brdm2_hull = brdm2.transform.Find("BRDM2_1983 (1)/BRDM_hull_1983").gameObject;
        }
    }
}
