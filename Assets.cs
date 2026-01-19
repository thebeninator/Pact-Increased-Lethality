using System.Linq;
using GHPC.Camera;
using GHPC.Effects.Voices;
using GHPC.Vehicle;
using GHPC.Weaponry;
using NWH.VehiclePhysics;
using Reticle;
using TMPro;
using UnityEngine;

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

        internal static AmmoType ammo_3ubr6;
        internal static AmmoClipCodexScriptable clip_codex_3ubr6;

        internal static AmmoType ammo_3uor6;
        internal static AmmoCodexScriptable ammo_codex_3uor6;
        internal static AmmoClipCodexScriptable clip_codex_3uor6;

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
        internal static GameObject night_sight_shutter;

        internal static Vehicle BRDM2;

        internal static void Load() {
            if (done) return;

            Vehicle[] vics = Resources.FindObjectsOfTypeAll<Vehicle>();
            AmmoClipCodexScriptable[] clip_codex_scriptables = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>();
            AmmoCodexScriptable[] codex_scriptables = Resources.FindObjectsOfTypeAll<AmmoCodexScriptable>();

            Vehicle t72m1 = vics.Where(o => o.gameObject.name == "T72M1").First();
            t72m1.transform.Find("---MAIN GUN SCRIPTS---/2A46/TPD-K1 gunner's sight/GPS/Reticle Mesh").GetComponent<ReticleMesh>().Load();
            night_sight_shutter = t72m1.transform.Find("---TURRET SCRIPTS---/shutter parent").gameObject;

            Vehicle m1ip = vics.Where(o => o.gameObject.name == "_M1IP (variant)").First();
            abrams_vic_controller = m1ip.GetComponent<VehicleController>();
            m1ip_range_canvas = m1ip.transform.Find("Turret Scripts/GPS/Optic/Abrams GPS canvas").gameObject;
            crt_shock_go = m1ip.transform.Find("Turret Scripts/GPS/FLIR/Scanline FOV change").gameObject;
            m1ip.transform.Find("Turret Scripts/GPS/FLIR/Reticle Mesh WFOV").GetComponent<ReticleMesh>().Load();
            flir_post_green = m1ip.transform.Find("Turret Scripts/GPS/FLIR/FLIR Post Processing - Green").gameObject;
            green_flir_mat = m1ip.transform.Find("Turret Scripts/GPS/FLIR").GetComponent<CameraSlot>().FLIRBlitMaterialOverride;

            Vehicle bmp2 = vics.Where(o => o.gameObject.name == "BMP2 Soviet").First();
            bmp2.transform.Find("fire control/gunner day sight 1P3-3/Optic/Reticle Mesh").GetComponent<ReticleMesh>().Load();

            Vehicle m60a1 = vics.Where(o => o.gameObject.name == "M60A1").First();
            m60a1_nvs = m60a1.transform.Find("Turret Scripts/Sights/NVS").gameObject;

            Vehicle t64b = vics.Where(o => o.gameObject.name == "T64B 1984").First();
            t64b.transform.Find("---MAIN GUN SCRIPTS---/2A46/TPN‑3‑49 night sight/Reticle Mesh").GetComponent<ReticleMesh>().Load();

            Vehicle t55a = vics.Where(o => o.gameObject.name == "T55A").First();
            t55a.transform.Find("Gun Scripts/Sights (and FCS)/NVS/Reticle Mesh").GetComponent<ReticleMesh>().Load();
            t55a.WeaponsManager.Weapons[0].FCS.AuthoritativeOptic.reticleMesh.Load();

            Vehicle m2_bradley = vics.Where(o => o.gameObject.name == "M2 Bradley").First();
            m2_bradley_canvas = m2_bradley.transform.Find("FCS and sights/GPS Optic/M2 Bradley GPS canvas").gameObject;

            Vehicle t80b = vics.Where(o => o.gameObject.name == "T80B").First();
            t80b_canvas = t80b.transform.Find("---MAIN GUN SCRIPTS---/2A46-2/1G42 gunner's sight/GPS/1G42 Canvas").gameObject;
            soviet_crew_voice = t80b.GetComponentInChildren<CrewVoiceHandler>().gameObject;

            BRDM2 = vics.Where(o => o.gameObject.name == "BRDM2").First();

            clip_codex_3bk18m = clip_codex_scriptables.Where(o => o.name == "clip_3BK18M").First();

            clip_codex_3bm22 = clip_codex_scriptables.Where(o => o.name == "clip_3BM22").First();

            clip_codex_3bm32 = clip_codex_scriptables.Where(o => o.name == "clip_3BM32").First();
            ammo_3bm32 = clip_codex_3bm32.ClipType.MinimalPattern[0].AmmoType;

            ammo_kobra = codex_scriptables.Where(o => o.AmmoType.Name == "9M112M Kobra").First().AmmoType;

            clip_codex_3ubr6 = clip_codex_scriptables.Where(o => o.name == "clip_3UBR6_160rd_load").First();
            ammo_3ubr6 = clip_codex_3ubr6.ClipType.MinimalPattern[0].AmmoType;

            clip_codex_3uor6 = clip_codex_scriptables.Where(o => o.name == "clip_3UOR6_340rd_load").First();
            ammo_codex_3uor6 = clip_codex_3uor6.ClipType.MinimalPattern[0];
            ammo_3uor6 = clip_codex_3uor6.ClipType.MinimalPattern[0].AmmoType;

            ammo_3bk5m = codex_scriptables.Where(o => o.AmmoType.Name == "3BK5M HEAT-FS-T").First().AmmoType;
            ammo_9m111 = codex_scriptables.Where(o => o.AmmoType.Name == "9M111 Fagot").First().AmmoType;
            ammo_3of412 = codex_scriptables.Where(o => o.AmmoType.Name == "3OF412 HE-T").First().AmmoType;
            ammo_3bm20 = codex_scriptables.Where(o => o.AmmoType.Name == "3BM20 APFSDS-T").First().AmmoType;
            clip_codex_br412d = clip_codex_scriptables.Where(o => o.name == "clip_BR-412D").First();

            ammo_9m113 = codex_scriptables.Where(o => o.AmmoType.Name == "9M113 Konkurs").First().AmmoType;

            tpd_etch_sdf = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Where(o => o.name == "TPD_Etch SDF").First();

            done = true;
        }
    }
}
