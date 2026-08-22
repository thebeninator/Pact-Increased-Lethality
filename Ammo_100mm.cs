using UnityEngine;
using GHPC.Weaponry;
using System.Linq;
using ModUtil;
using GHPC.Weapons;

namespace PactIncreasedLethality
{
    public class Ammo_100mm : Module
    {
        internal static AmmoClipCodexScriptable clip_codex_3of70;
        internal static AmmoType.AmmoClip clip_3of70 = new AmmoType.AmmoClip();
        internal static AmmoCodexScriptable ammo_codex_3of70;
        internal static AmmoType ammo_3of70 = new AmmoType();
        private static GameObject ammo_3of70_vis;

        public override void UnloadDynamicAssets()
        {
            GameObject.DestroyImmediate(ammo_3of70_vis);
        }

        public override void LoadDynamicAssets()
        {
            Util.ShallowCopy(ammo_3of70, SharedAssets.ammo_3of412);
            ammo_3of70.Name = "3OF70 HEF-T";
            ammo_3of70.Caliber = 100;
            ammo_3of70.Mass = 13.4f;
            ammo_3of70.MuzzleVelocity = 355f;
            ammo_3of70.TntEquivalentKg = 3.5f;

            Util.Coalesce(ref ammo_codex_3of70);
            ammo_codex_3of70.AmmoType = ammo_3of70;
            ammo_codex_3of70.name = "ammo_3of70";

            clip_3of70.Capacity = 1;
            clip_3of70.Name = "3OF70 HEF-T";
            clip_3of70.MinimalPattern = new AmmoCodexScriptable[1];
            clip_3of70.MinimalPattern[0] = ammo_codex_3of70;

            Util.Coalesce(ref clip_codex_3of70);
            clip_codex_3of70.name = "clip_3of70";
            clip_codex_3of70.ClipType = clip_3of70;

            ammo_3of70_vis = GameObject.Instantiate(SharedAssets.ammo_3of412.VisualModel);
            ammo_3of70_vis.name = "3of70 visual";
            ammo_3of70.VisualModel = ammo_3of70_vis;
            ammo_3of70.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3of70;
            ammo_3of70.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3of70;
        }
    }
}
