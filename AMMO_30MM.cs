using UnityEngine;
using GHPC.Weaponry;
using System.Linq;

namespace PactIncreasedLethality
{
    public class Ammo_30mm : Module
    {
        internal static AmmoClipCodexScriptable clip_codex_3ubr8;
        internal static AmmoType.AmmoClip clip_3ubr8;
        internal static AmmoCodexScriptable ammo_codex_3ubr8;
        internal static AmmoType ammo_3ubr8;

        internal static AmmoClipCodexScriptable clip_codex_3uof8;
        internal static AmmoType.AmmoClip clip_3uof8;
        internal static AmmoCodexScriptable ammo_codex_3uof8;
        internal static AmmoType ammo_3uof8;

        internal static AmmoClipCodexScriptable clip_codex_3ubr6;
        internal static AmmoType ammo_3ubr6;

        internal static AmmoClipCodexScriptable clip_codex_3uor6;
        internal static AmmoCodexScriptable ammo_codex_3uor6;
        internal static AmmoType ammo_3uor6;

        public override void LoadDynamicAssets()
        {
            string[] bmp2s = { "BMP2 Soviet", "BMP2" };
            string[] btr60s = { "BTR60PB", "BTR60PB Soviet" };
            string[] bmp1s = { "BMP1", "BMP1 Soviet", "BMP1P (Variant)", "BMP1P (Variant) Soviet" };

            bool has_30mm_btr60 = AssetUtil.VehicleInMission(btr60s) && BTR60.autocannon.Value;
            bool has_vog_bmp1 = AssetUtil.VehicleInMission(bmp1s) && (BMP1.ags_17_bmp1.Value || BMP1.ags_17_bmp1p.Value);

            if (!AssetUtil.VehicleInMission(bmp2s) && !has_30mm_btr60 && !has_vog_bmp1) return;

            AssetUtil.LoadVanillaVehicle("BMP2_SA");

            AmmoClipCodexScriptable[] clip_codex_scriptables = Resources.FindObjectsOfTypeAll<AmmoClipCodexScriptable>();
            AmmoCodexScriptable[] codex_scriptables = Resources.FindObjectsOfTypeAll<AmmoCodexScriptable>();

            clip_codex_3ubr6 = clip_codex_scriptables.Where(o => o.name == "clip_3UBR6_160rd_load").FirstOrDefault();
            ammo_3ubr6 = clip_codex_3ubr6.ClipType.MinimalPattern[0].AmmoType;

            clip_codex_3uor6 = clip_codex_scriptables.Where(o => o.name == "clip_3UOR6_340rd_load").FirstOrDefault();
            ammo_codex_3uor6 = clip_codex_3uor6.ClipType.MinimalPattern[0];
            ammo_3uor6 = clip_codex_3uor6.ClipType.MinimalPattern[0].AmmoType;

            ammo_3ubr8 = new AmmoType();
            Util.ShallowCopy(ammo_3ubr8, ammo_3ubr6);
            ammo_3ubr8.Name = "3UBR8 APDS-T";
            ammo_3ubr8.Mass = 0.222f;
            ammo_3ubr8.Coeff = 0.012f;
            ammo_3ubr8.MuzzleVelocity = 1120f;
            ammo_3ubr8.RhaPenetration = 72f;
            ammo_3ubr8.VisualType = GHPC.Weapons.LiveRoundMarshaller.LiveRoundVisualType.Custom;

            Util.Coalesce(ref ammo_codex_3ubr8);
            ammo_codex_3ubr8.AmmoType = ammo_3ubr8;
            ammo_codex_3ubr8.name = "ammo_3ubr8";

            clip_3ubr8 = new AmmoType.AmmoClip();
            clip_3ubr8.Capacity = 160;
            clip_3ubr8.Name = "3UBR8 APDS-T";
            clip_3ubr8.MinimalPattern = new AmmoCodexScriptable[1];
            clip_3ubr8.MinimalPattern[0] = ammo_codex_3ubr8;

            Util.Coalesce(ref clip_codex_3ubr8);
            clip_codex_3ubr8.name = "clip_3ubr8";
            clip_codex_3ubr8.ClipType = clip_3ubr8;

            /////////////////////

            ammo_3uof8 = new AmmoType();
            Util.ShallowCopy(ammo_3uof8, ammo_3uor6);
            ammo_3uof8.Name = "3UOF8 HEFI";
            ammo_3uof8.UseTracer = false;
            ammo_3uof8.TntEquivalentKg = 0.049f;

            ammo_3uof8.VisualType = GHPC.Weapons.LiveRoundMarshaller.LiveRoundVisualType.Custom;

            Util.Coalesce(ref ammo_codex_3uof8);
            ammo_codex_3uof8.AmmoType = ammo_3uof8;
            ammo_codex_3uof8.name = "ammo_3uof8";

            ammo_3ubr6.VisualType = GHPC.Weapons.LiveRoundMarshaller.LiveRoundVisualType.Custom;
            ammo_3uor6.VisualType = GHPC.Weapons.LiveRoundMarshaller.LiveRoundVisualType.Custom;

            clip_3uof8 = new AmmoType.AmmoClip();
            clip_3uof8.Capacity = 340;
            clip_3uof8.Name = "3UOR6 HE-T/3UOF8 HEFI";
            clip_3uof8.MinimalPattern = new AmmoCodexScriptable[] {
                ammo_codex_3uof8,
                ammo_codex_3uof8,
                ammo_codex_3uor6,
            };
            clip_3uof8.MinimalPattern[0] = ammo_codex_3uof8;

            Util.Coalesce(ref clip_codex_3uof8);
            clip_codex_3uof8.name = "clip_3uof8";
            clip_codex_3uof8.ClipType = clip_3uof8;
        }
    }
}
