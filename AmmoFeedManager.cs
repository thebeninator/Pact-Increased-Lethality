using System;
using System.Collections.Generic;
using GHPC.Weapons;
using HarmonyLib;

namespace PactIncreasedLethality
{
    internal class ClipRackMap
    {
        public Dictionary<AmmoType.AmmoClip, GHPC.Weapons.AmmoRack> Map = new Dictionary<AmmoType.AmmoClip, GHPC.Weapons.AmmoRack>();
        public Action<AmmoType.AmmoClip> OnChange;

        public GHPC.Weapons.AmmoRack GetRack(AmmoType.AmmoClip clip)
        {
            return Map[clip];
        }

        public void Insert(AmmoType.AmmoClip clip, GHPC.Weapons.AmmoRack rack)
        {
            Map.Add(clip, rack);
        }
    }

    [HarmonyPatch(typeof(AmmoFeed), "FeedNewClip")]
    internal static class AmmoFeedManager
    {
        private static Dictionary<int, ClipRackMap> registry = new Dictionary<int, ClipRackMap>();

        private static void Prefix(AmmoFeed __instance)
        {
            WeaponSystem weapon_system = __instance._weapon as WeaponSystem;
            int id = weapon_system.GetInstanceID();

            if (!registry.ContainsKey(id)) return;
            if (__instance.Reloading) return;

            ClipRackMap map = registry[id];
            AmmoType.AmmoClip queued_clip = __instance.QueuedClipType;
            GHPC.Weapons.AmmoRack queued_rack = map.GetRack(queued_clip);

            if (__instance.ReadyRack.GetInstanceID() != queued_rack.GetInstanceID())
            {
                map.OnChange(queued_clip);
                __instance.ReadyRack = queued_rack;
            }
        }

        public static void Register(int weapon_system_id, ClipRackMap map)
        {
            registry.Add(weapon_system_id, map);
        }
    }

    //internal class AmmoFeedManager : MonoBehaviour
    //{
    //    private WeaponSystem weapon_system;

    //    public AmmoFeed[] AmmoFeeds { get; set; }
    //    public Dictionary<AmmoType.AmmoClip, int> ClipAssociations { get; set; }

    //    void Awake()
    //    {
    //        weapon_system = GetComponent<WeaponSystem>();
    //    }

    //    void Update()
    //    {
    //        AmmoType.AmmoClip queued_clip = weapon_system.Feed.QueuedClipType;
    //        AmmoFeed queued_feed = AmmoFeeds[ClipAssociations[queued_clip]];
    //        if (queued_feed.GetInstanceID() != weapon_system.Feed.GetInstanceID())
    //        {

    //        }
    //    }
    //}
}
