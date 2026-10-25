using System;
using FMOD;
using FMODUnity;
using GHPC.Camera;
using GHPC.Player;
using GHPC.Weapons;
using HarmonyLib;
using UnityEngine;

namespace PactIncreasedLethality.Vics.BMP3
{
    internal class BMP3Patches
    {
        // todo: need to make a centralized patch for audio
        [HarmonyPatch(typeof(WeaponAudio), "FinalStartLoop")]
        public static class ReplaceInterior2A70Sfx
        {
            public static FMOD.Sound sound_interior;

            public static bool Prefix(WeaponAudio __instance)
            {
                if (__instance.SingleShotMode && __instance.LoopEventPath == "2a70")
                {
                    WeaponAudio current_player_wpn_audio = Mod.player_manager.CurrentPlayerWeapon.Weapon.WeaponSound;
                    bool is_player_instance = __instance.GetInstanceID() == current_player_wpn_audio.GetInstanceID();
                    bool interior = !CameraManager._instance.ExteriorMode && is_player_instance;

                    if (!interior) return true;

                    var cor_system = RuntimeManager.CoreSystem;

                    Vector3 vec = __instance.transform.position;

                    VECTOR fmod_pos = new VECTOR();
                    fmod_pos.x = vec.x;
                    fmod_pos.y = vec.y;
                    fmod_pos.z = vec.z;

                    VECTOR vel = new VECTOR();
                    vel.x = 0f;
                    vel.y = 0f;
                    vel.z = 0f;
                    FMOD.Channel new_channel;

                    cor_system.playSound(sound_interior, Mod.audio_channel_group, true, out new_channel);

                    new_channel.setVolume(Mod.audio_settings_manager._previousVolume * 2.25f);
                    new_channel.set3DAttributes(ref fmod_pos, ref vel);
                    new_channel.setPaused(false);
                    new_channel.clearHandle();

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(GHPC.Weapons.AmmoRack), "HasClipWithPattern", new Type[] { typeof(AmmoType.AmmoClip) })]
        public static class test
        {
            public static bool Prefix(GHPC.Weapons.AmmoRack __instance, AmmoType.AmmoClip pattern, ref bool __result)
            {
                if (pattern.Name == Ammo_100mm.clip_9m117_bmp3.Name)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerInput), "QueueAmmoIndex", new Type[] { typeof(int) })]
        public static class AllowQueueAmmoFor2A72
        {
            public static void Postfix(PlayerInput __instance, int index)
            {
                if (__instance.CurrentPlayerWeapon.Weapon.name != "bmp3 2a72") return;

                WeaponSystemInfo active_wsi = __instance.CurrentPlayerCrewBrain.WeaponsModule.ActiveWeapon;
                GHPC.Weapons.AmmoRack ready_rack = active_wsi.Weapon.Feed.ReadyRack;

                if (index >= ready_rack.ClipTypes.Length)
                {
                    return;
                }

                __instance.CurrentPlayerCrewBrain.WeaponsModule.QueueClipType(ready_rack.ClipTypes[index], false);
            }
        }
    }
}
