using UnityEngine;
using GHPC.Weaponry;
using System.Linq;
using ModUtil;
using GHPC.Weapons;
using GHPC.Vehicle;
using Reticle;
using MelonLoader;
using GHPC.State;

namespace PactIncreasedLethality
{
    public class Ammo_100mm : Module
    {
        public override GameState LoadState => GameState.TerrainSceneLoaded;

        internal static AmmoClipCodexScriptable clip_codex_3of70;
        internal static AmmoType.AmmoClip clip_3of70 = new AmmoType.AmmoClip();
        internal static AmmoCodexScriptable ammo_codex_3of70;
        internal static AmmoType ammo_3of70 = new AmmoType();
        private static GameObject ammo_3of70_vis;

        internal static AmmoClipCodexScriptable clip_codex_9m117_bmp3;
        internal static AmmoType.AmmoClip clip_9m117_bmp3 = new AmmoType.AmmoClip();
        internal static AmmoCodexScriptable ammo_codex_9m117_bmp3;
        internal static AmmoType ammo_9m117_bmp3 = new AmmoType();
        private static GameObject ammo_9m117_bmp3_vis;

        public override void UnloadDynamicAssets()
        {
            GameObject.DestroyImmediate(ammo_3of70_vis);
            GameObject.DestroyImmediate(ammo_9m117_bmp3_vis);
        }

        public override void LoadDynamicAssets()
        {
            Vehicle t55a = AssetUtil.LoadVanillaVehicle("T55A", true);
            AmmoType ammo_3of412 = Resources.FindObjectsOfTypeAll<AmmoCodexScriptable>().Where(o => o.name == "ammo_3OF412").FirstOrDefault().AmmoType;

            Util.ShallowCopy(ammo_3of70, ammo_3of412);
            ammo_3of70.Name = "3OF70 HEF-T";
            ammo_3of70.Caliber = 100;
            ammo_3of70.Mass = 13.4f;
            ammo_3of70.MuzzleVelocity = 355f;
            ammo_3of70.TntEquivalentKg = 3.5f;
            ammo_3of70.Coeff = 0.12f;
            ammo_3of70.CachedIndex = -1;
            Util.CacheAmmo(ammo_3of70);

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

            ammo_3of70.VisualModel = BMP3.vis_3of70;
            ammo_3of70.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3of70;
            ammo_3of70.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3of70;

            Util.ShallowCopy(ammo_9m117_bmp3, ammo_3of412);
            ammo_9m117_bmp3.Name = "9M117 Bastion";
            ammo_9m117_bmp3.Caliber = 100;
            ammo_9m117_bmp3.Mass = 13.4f;
            ammo_9m117_bmp3.MuzzleVelocity = 355f;
            ammo_9m117_bmp3.TntEquivalentKg = 3.5f;
            ammo_9m117_bmp3.Coeff = 0.12f;
            ammo_9m117_bmp3.CachedIndex = -1;
            Util.CacheAmmo(ammo_9m117_bmp3);

            Util.Coalesce(ref ammo_codex_9m117_bmp3);
            ammo_codex_9m117_bmp3.AmmoType = ammo_9m117_bmp3;
            ammo_codex_9m117_bmp3.name = "ammo_9m117_bmp3";

            clip_9m117_bmp3.Capacity = 1;
            clip_9m117_bmp3.Name = "9M117 Bastion";
            clip_9m117_bmp3.MinimalPattern = new AmmoCodexScriptable[1];
            clip_9m117_bmp3.MinimalPattern[0] = ammo_codex_9m117_bmp3;

            Util.Coalesce(ref clip_codex_9m117_bmp3);
            clip_codex_9m117_bmp3.name = "clip_9m117_bmp3";
            clip_codex_9m117_bmp3.ClipType = clip_9m117_bmp3;

            ammo_9m117_bmp3_vis = GameObject.Instantiate(ammo_3of412.VisualModel);
            ammo_9m117_bmp3_vis.name = "9m117 bmp3 visual";
            ammo_9m117_bmp3.VisualModel = ammo_9m117_bmp3_vis;
            ammo_9m117_bmp3.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_9m117_bmp3;
            ammo_9m117_bmp3.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_9m117_bmp3;
        }
    }
}
