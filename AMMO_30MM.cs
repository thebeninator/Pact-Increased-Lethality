using UnityEngine;
using GHPC.Weaponry;

namespace PactIncreasedLethality
{
    public class Ammo_30mm
    {
        internal static bool assets_loaded = false;
        public static AmmoClipCodexScriptable clip_codex_3ubr8;
        public static AmmoType.AmmoClip clip_3ubr8;
        public static AmmoCodexScriptable ammo_codex_3ubr8;
        public static AmmoType ammo_3ubr8;

        public static AmmoClipCodexScriptable clip_codex_3uof8;
        public static AmmoType.AmmoClip clip_3uof8;
        public static AmmoCodexScriptable ammo_codex_3uof8;
        public static AmmoType ammo_3uof8;

        public static void LoadAssets()
        {
            if (assets_loaded) return;

            ammo_3ubr8 = new AmmoType();
            Util.ShallowCopy(ammo_3ubr8, Assets.ammo_3ubr6);
            ammo_3ubr8.Name = "3UBR8 APDS-T";
            ammo_3ubr8.Mass = 0.222f;
            ammo_3ubr8.Coeff = 0.012f;
            ammo_3ubr8.MuzzleVelocity = 1120f;
            ammo_3ubr8.RhaPenetration = 72f;
            ammo_3ubr8.VisualType = GHPC.Weapons.LiveRoundMarshaller.LiveRoundVisualType.Custom;

            ammo_codex_3ubr8 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_3ubr8.AmmoType = ammo_3ubr8;
            ammo_codex_3ubr8.name = "ammo_3ubr8";

            clip_3ubr8 = new AmmoType.AmmoClip();
            clip_3ubr8.Capacity = 160;
            clip_3ubr8.Name = "3UBR8 APDS-T";
            clip_3ubr8.MinimalPattern = new AmmoCodexScriptable[1];
            clip_3ubr8.MinimalPattern[0] = ammo_codex_3ubr8;

            clip_codex_3ubr8 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_3ubr8.name = "clip_3ubr8";
            clip_codex_3ubr8.ClipType = clip_3ubr8;

            /////////////////////

            ammo_3uof8 = new AmmoType();
            Util.ShallowCopy(ammo_3uof8, Assets.ammo_3uor6);
            ammo_3uof8.Name = "3UOF8 HEFI";
            ammo_3uof8.UseTracer = false;
            ammo_3uof8.TntEquivalentKg = 0.049f;

            ammo_3uof8.VisualType = GHPC.Weapons.LiveRoundMarshaller.LiveRoundVisualType.Custom;

            ammo_codex_3uof8 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_3uof8.AmmoType = ammo_3uof8;
            ammo_codex_3uof8.name = "ammo_3uof8";

            Assets.ammo_3ubr6.VisualType = GHPC.Weapons.LiveRoundMarshaller.LiveRoundVisualType.Custom;
            Assets.ammo_3uor6.VisualType = GHPC.Weapons.LiveRoundMarshaller.LiveRoundVisualType.Custom;

            clip_3uof8 = new AmmoType.AmmoClip();
            clip_3uof8.Capacity = 340;
            clip_3uof8.Name = "3UOR6 HE-T/3UOF8 HEFI";
            clip_3uof8.MinimalPattern = new AmmoCodexScriptable[] {
                ammo_codex_3uof8,
                ammo_codex_3uof8,
                Assets.ammo_codex_3uor6,
            };
            clip_3uof8.MinimalPattern[0] = ammo_codex_3uof8;

            clip_codex_3uof8 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_3uof8.name = "clip_3uof8";
            clip_codex_3uof8.ClipType = clip_3uof8;

            assets_loaded = true;
        }
    }
}
