using System.Collections;
using System.IO;
using GHPC.State;
using GHPC.Vehicle;
using GHPC.Weapons;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using FMOD;
using FMODUnity;
using HarmonyLib;
using GHPC.Camera;
using GHPC.Weaponry;

namespace PactIncreasedLethality
{

    [HarmonyPatch(typeof(WeaponAudio), "FinalStartLoop")]
    public class AGS17_Sound
    {
        public static FMOD.Sound[] sounds = new FMOD.Sound[6];
        public static FMOD.Sound[] sounds_exterior = new FMOD.Sound[7];

        public static bool Prefix(WeaponAudio __instance)
        {
            if (__instance.SingleShotMode && __instance.SingleShotEventPaths[0] == "blyat")
            {
                var corSystem = RuntimeManager.CoreSystem;

                Vector3 vec = __instance.transform.position;

                VECTOR pos = new VECTOR();
                pos.x = vec.x;
                pos.y = vec.y;
                pos.z = vec.z;

                VECTOR vel = new VECTOR();
                vel.x = 0f;
                vel.y = 0f;
                vel.z = 0f;

                bool interior = !CameraManager._instance.ExteriorMode && __instance == Mod.player_manager.CurrentPlayerWeapon.Weapon.WeaponSound;

                ChannelGroup channelGroup;
                corSystem.createChannelGroup("master", out channelGroup);

                channelGroup.setVolumeRamp(false);
                channelGroup.setMode(MODE._3D_WORLDRELATIVE);

                FMOD.Channel channel;
                corSystem.playSound(interior ? sounds[UnityEngine.Random.Range(0, sounds.Length)] : sounds_exterior[UnityEngine.Random.Range(0, sounds_exterior.Length)], channelGroup, true, out channel);

                float game_vol = Mod.audio_settings_manager._previousVolume;
                float gun_vol = (interior) ? (game_vol + 0.10f * (game_vol * 10)) : (game_vol + 0.07f * (game_vol * 10));

                channel.setVolume(gun_vol);
                channel.setVolumeRamp(false);
                channel.set3DAttributes(ref pos, ref vel);
                channelGroup.set3DAttributes(ref pos, ref vel);
                channel.setPaused(false);

                return false;
            }

            return true;
        }
    }

    public class BMP1
    {
        static MelonPreferences_Entry<bool> bmp1_patch;
        static MelonPreferences_Entry<bool> ags_17_bmp1;
        static MelonPreferences_Entry<bool> ags_17_bmp1p;
        static MelonPreferences_Entry<bool> vog17m1_hedp;

        static WeaponSystemCodexScriptable gun_ags17;
        static AmmoClipCodexScriptable clip_codex_vog17;
        static AmmoType.AmmoClip clip_vog17;
        static AmmoCodexScriptable ammo_codex_vog17;
        static AmmoType ammo_vog17;

        static AmmoClipCodexScriptable clip_codex_vog17m1;
        static AmmoType.AmmoClip clip_vog17m1;
        static AmmoCodexScriptable ammo_codex_vog17m1;
        static AmmoType ammo_vog17m1;

        static AmmoType ammo_3uor6;

        public static void Config(MelonPreferences_Category cfg)
        {
            bmp1_patch = cfg.CreateEntry<bool>("BMP-1 Patch", true);
            bmp1_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";

            ags_17_bmp1 = cfg.CreateEntry<bool>("AGS-17D Coax (BMP-1)", true);
            ags_17_bmp1.Comment = "Replaces PKT coax with AGS-17D 30mm grenade launcher";

            ags_17_bmp1p = cfg.CreateEntry<bool>("AGS-17D Coax (BMP-1P)", true);
            ags_17_bmp1p.Comment = "Replaces PKT coax with AGS-17D 30mm grenade launcher";

            vog17m1_hedp = cfg.CreateEntry<bool>("Use VOG-17M1 HEDP", false);
            vog17m1_hedp.Comment = "Fictional grenade for the AGS-17D. Behaves like a HEAT round";
        }

        public static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;

                string name = vic.FriendlyName;

                if (!name.Contains("BMP-1")) continue;

                if (!name.Contains("G") && (ags_17_bmp1.Value && name == "BMP-1") || (ags_17_bmp1p.Value && name == "BMP-1P"))
                {
                    LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                    WeaponSystem coax = vic.GetComponent<WeaponsManager>().Weapons[2].Weapon;
                    coax.WeaponSound.SingleShotMode = true;
                    coax.WeaponSound.SingleShotEventPaths = new string[] { "blyat" };
                    coax.BaseDeviationAngle *= 12f;
                    coax.SetCycleTime(0.19f);
                    coax.CodexEntry = gun_ags17;
                    coax.Feed.AmmoTypeInBreech = null;
                    coax.Feed.ReadyRack.ClipTypes[0] = vog17m1_hedp.Value ? clip_vog17m1 : clip_vog17;
                    coax.Feed.ReadyRack.Awake();
                    coax.Feed.Start();

                    vic._friendlyName += "G";
                }
            }
            yield break;
        }

        public static void Init()
        {
            if (!bmp1_patch.Value) return;

            if (gun_ags17 == null)
            {
                var corSystem = FMODUnity.RuntimeManager.CoreSystem;

                for (int i = 0; i < 6; i++)
                {
                    corSystem.createSound(
                        Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/ags17", "aavp7a1_mk19_fire_1p_0" + (i + 1) + ".ogg"), MODE._3D_IGNOREGEOMETRY, out AGS17_Sound.sounds[i]);
                    AGS17_Sound.sounds[i].set3DMinMaxDistance(35f, 5000f);
                }

                for (int i = 0; i < 7; i++)
                {
                    corSystem.createSound(
                        Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/ags17", "aavp7a1_mk19_fire_close_3p_0" + (i + 1) + ".ogg"), MODE._3D_INVERSETAPEREDROLLOFF, out AGS17_Sound.sounds_exterior[i]);
                    AGS17_Sound.sounds_exterior[i].set3DMinMaxDistance(35f, 5000f);
                }

                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "3UOR6 HE-T") { ammo_3uor6 = s.AmmoType; break; } 
                }

                gun_ags17 = ScriptableObject.CreateInstance<WeaponSystemCodexScriptable>();
                gun_ags17.name = "gun_ags17";
                gun_ags17.CaliberMm = 30;
                gun_ags17.FriendlyName = "30mm grenade launcher AGS-17D";
                gun_ags17.Type = WeaponSystemCodexScriptable.WeaponType.GrenadeLauncher;

                ammo_vog17 = new AmmoType();
                Util.ShallowCopy(ammo_vog17, ammo_3uor6);
                ammo_vog17.Name = "VOG-17M HE";
                ammo_vog17.MuzzleVelocity = 600f;
                ammo_vog17.VisualType = LiveRoundMarshaller.LiveRoundVisualType.Bullet;
                ammo_vog17.UseTracer = false;
                ammo_vog17.RhaPenetration = 5f;
                ammo_vog17.DetonateSpallCount = 50;
                ammo_vog17.TntEquivalentKg = 0.064f;
                ammo_vog17.ArmingDistance = 25f;
                ammo_vog17.ImpactFuseTime = 0f;

                ammo_codex_vog17 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_vog17.AmmoType = ammo_vog17;
                ammo_codex_vog17.name = "ammo_vog17";

                clip_vog17 = new AmmoType.AmmoClip();
                clip_vog17.Capacity = 300;
                clip_vog17.Name = "VOG-17M HE";
                clip_vog17.MinimalPattern = new AmmoCodexScriptable[1];
                clip_vog17.MinimalPattern[0] = ammo_codex_vog17;

                clip_codex_vog17 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_vog17.name = "clip_vog17";
                clip_codex_vog17.ClipType = clip_vog17;

                ammo_vog17m1 = new AmmoType();
                Util.ShallowCopy(ammo_vog17m1, ammo_3uor6);
                ammo_vog17m1.Name = "VOG-17M1 HEDP";
                ammo_vog17m1.MuzzleVelocity = 600f;
                ammo_vog17m1.VisualType = LiveRoundMarshaller.LiveRoundVisualType.Bullet;
                ammo_vog17m1.UseTracer = false;
                ammo_vog17m1.RhaPenetration = 50f;
                ammo_vog17m1.DetonateSpallCount = 20;
                ammo_vog17m1.TntEquivalentKg = 0.032f;
                ammo_vog17m1.ArmingDistance = 25f;
                ammo_vog17m1.Category = AmmoType.AmmoCategory.ShapedCharge;
                ammo_vog17m1.SpallMultiplier = 0.1f;
                ammo_vog17m1.ImpactFuseTime = 0f;
                ammo_vog17m1.ShatterOnRicochet = false;
                ammo_vog17m1.AlwaysProduceBlast = true;

                ammo_codex_vog17m1 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_vog17m1.AmmoType = ammo_vog17m1;
                ammo_codex_vog17m1.name = "ammo_vog17m1";

                clip_vog17m1 = new AmmoType.AmmoClip();
                clip_vog17m1.Capacity = 300;
                clip_vog17m1.Name = "VOG-17M1 HEDP";
                clip_vog17m1.MinimalPattern = new AmmoCodexScriptable[1];
                clip_vog17m1.MinimalPattern[0] = ammo_codex_vog17m1;

                clip_codex_vog17m1 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_vog17m1.name = "clip_vog17m1";
                clip_codex_vog17m1.ClipType = clip_vog17m1;
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }
}

