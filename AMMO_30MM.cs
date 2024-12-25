using UnityEngine;

namespace PactIncreasedLethality
{
    public class AMMO_30MM
    {
        public static AmmoClipCodexScriptable clip_codex_3ubr8;
        public static AmmoType.AmmoClip clip_3ubr8;
        public static AmmoCodexScriptable ammo_codex_3ubr8;
        public static AmmoType ammo_3ubr8;

        public static AmmoClipCodexScriptable clip_codex_3uof8;
        public static AmmoType.AmmoClip clip_3uof8;
        public static AmmoCodexScriptable ammo_codex_3uof8;
        public static AmmoType ammo_3uof8;

        public static AmmoType ammo_3ubr6;
        public static AmmoClipCodexScriptable clip_codex_3ubr6;

        public static AmmoType ammo_3uor6;
        public static AmmoCodexScriptable ammo_codex_3uor6;
        public static AmmoClipCodexScriptable clip_codex_3uor6;

        public static void Init()
        {
            if (ammo_3ubr8 == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "3UBR6 APBC-T")
                    {
                        ammo_3ubr6 = s.AmmoType;
                    }

                    if (s.AmmoType.Name == "3UOR6 HE-T")
                    {
                        ammo_3uor6 = s.AmmoType;
                        ammo_codex_3uor6 = s;
                    }

                    if (ammo_3ubr6 != null && ammo_3uor6 != null) break;
                }

                foreach (AmmoClipCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoClipCodexScriptable)))
                {
                    if (s.name == "clip_3UOR6_340rd_load") { clip_codex_3uor6 = s; }
                    if (s.name == "clip_3UBR6_160rd_load") { clip_codex_3ubr6 = s; }
                    if (clip_codex_3ubr6 != null && clip_codex_3uor6 != null) break;
                }

                ammo_3ubr8 = new AmmoType();
                Util.ShallowCopy(ammo_3ubr8, ammo_3ubr6);
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
                Util.ShallowCopy(ammo_3uof8, ammo_3uor6);
                ammo_3uof8.Name = "3UOF8 HEFI";
                ammo_3uof8.UseTracer = false;
                ammo_3uof8.TntEquivalentKg = 0.049f;

                ammo_3uof8.VisualType = GHPC.Weapons.LiveRoundMarshaller.LiveRoundVisualType.Custom;

                ammo_codex_3uof8 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
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

                clip_codex_3uof8 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3uof8.name = "clip_3uof8";
                clip_codex_3uof8.ClipType = clip_3uof8;
            }
        }
    }
}
