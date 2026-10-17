using System.Collections;
using System.IO;
using System.Linq;
using ActiveProtectionSystem;
using FMOD;
using GHPC;
using GHPC.Audio;
using GHPC.Camera;
using GHPC.Mission;
using GHPC.Player;
using GHPC.State;
using GHPC.Vehicle;
using MelonLoader;
using MelonLoader.Utils;
using ModUtil;
using PactIncreasedLethality;
using UnityEngine;

[assembly: MelonInfo(typeof(Mod), "Pact Increased Lethality", "2.1.8", "ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace PactIncreasedLethality
{
    public class Mod : MelonMod
    {
        private static ModuleManager module_manager;
        internal static Vehicle[] vics;
        internal static MelonPreferences_Category cfg;

        private GameObject game_manager;
        internal static AudioSettingsManager audio_settings_manager;
        internal static PlayerInput player_manager;
        internal static CameraManager camera_manager;
        private int valid_scene_count = 0;

        internal static FMOD.ChannelGroup audio_channel_group;

        public IEnumerator MissionUnitsLoaded(GameState _)
        {
            UnitSpawner.Instance.SpawnUnit
            (
                "T72M1",
                new GHPC.Mission.Data.UnitMetaData()
                {
                    Name = "T72M1",
                    Allegiance = Faction.Neutral,
                    Position = new Vector3(849.7374f, 14.4967f, 1412.224f),
                    Rotation = Quaternion.Euler(359.882f, 91.7424f, 359.096f),
                    UnitType = UnitType.GroundVehicle,
                }
            );

            UnitSpawner.Instance.SpawnUnit
            (
                "T72M1",
                new GHPC.Mission.Data.UnitMetaData()
                {
                    Name = "T72M1",
                    Allegiance = Faction.Neutral,
                    Position = new Vector3(330.4259f, 7.1038f, 1432.017f),
                    Rotation = Quaternion.Euler(0.36f, 87.3901f, 359.5795f),
                    UnitType = UnitType.GroundVehicle,
                }
            );

            UnitSpawner.Instance.SpawnUnit
            (
                "T80B",
                new GHPC.Mission.Data.UnitMetaData()
                {
                    Name = "T80B",
                    Allegiance = Faction.Neutral,
                    Position = new Vector3(171.012f, 4.2746f, 1480.873f),
                    Rotation = Quaternion.Euler(359.5852f, 90.3407f, 359.9118f),
                    UnitType = UnitType.GroundVehicle,
                }
            );

            UnitSpawner.Instance.SpawnUnit
            (
                "T80B",
                new GHPC.Mission.Data.UnitMetaData()
                {
                    Name = "T80B",
                    Allegiance = Faction.Neutral,
                    Position = new Vector3(-300.2264f, 3.4258f, 1567.263f),
                    Rotation = Quaternion.Euler(358.8109f, 96.9203f, 1.8364f),
                    UnitType = UnitType.GroundVehicle,
                }
            );

            UnitSpawner.Instance.SpawnUnit
            (
                "T72M1",
                new GHPC.Mission.Data.UnitMetaData()
                {
                    Name = "T72M1",
                    Allegiance = Faction.Neutral,
                    Position = new Vector3(-775.4324f, 6.5356f, 1677.792f),
                    Rotation = Quaternion.Euler(0.6645f, 100.6866f, 359.4566f),
                    UnitType = UnitType.GroundVehicle,
                }
            );

            yield break;
        }

        public IEnumerator OnPlayerReady(GameState _) 
        {
            game_manager = GameObject.Find("_APP_GHPC_");
            audio_settings_manager = game_manager.GetComponent<AudioSettingsManager>();
            player_manager = game_manager.GetComponent<PlayerInput>();
            camera_manager = game_manager.GetComponent<CameraManager>();
            vics = GameObject.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);

            yield break;
        }

        public override void OnInitializeMelon()
        {
            module_manager = new ModuleManager("PIL");
            cfg = MelonPreferences.CreateCategory("PactIncreasedLethality");
            T55.Config(cfg);
            T72.Config(cfg);
            T64A.Config(cfg);
            T64B.Config(cfg);
            T62.Config(cfg);
            T80.Config(cfg);
            BMP1.Config(cfg);
            BMP2.Config(cfg);
            BTR60.Config(cfg);
            Armour.Config(cfg);

            var cor_system = FMODUnity.RuntimeManager.CoreSystem;

            cor_system.createChannelGroup("master", out audio_channel_group);
            audio_channel_group.setVolumeRamp(true);
            audio_channel_group.setMode(MODE._3D_WORLDRELATIVE);

            cor_system.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/zsu", "zsu_23_shot.wav"), MODE._3D_INVERSETAPEREDROLLOFF | MODE.LOWMEM, out BMP2.ReplaceSound.sound);
            BMP2.ReplaceSound.sound.set3DMinMaxDistance(50f, 1200f);

            cor_system.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/zsu", "zsu_23_shot_exterior.wav"), MODE._3D_INVERSETAPEREDROLLOFF | MODE.LOWMEM, out BMP2.ReplaceSound.sound_exterior);
            BMP2.ReplaceSound.sound_exterior.set3DMinMaxDistance(30f, 600f);

            cor_system.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/btr60a", "btr2a72_interior.ogg"), MODE._2D, out BTR60.ReplaceSound.sound_interior);
            BTR60.ReplaceSound.sound_interior.set3DMinMaxDistance(500f, 600f);

            cor_system.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/zsu", "zsu_23_shot_exterior.wav"), MODE._3D_INVERSETAPEREDROLLOFF | MODE.LOWMEM, out BTR60.ReplaceSound.sound_exterior);
            BTR60.ReplaceSound.sound_exterior.set3DMinMaxDistance(20f, 550f);

            module_manager.Add("SharedAssets", new SharedAssets());
            module_manager.Add("AMMO_30MM", new Ammo_30mm());
            module_manager.Add("AMMO_125MM", new Ammo_125mm());
            module_manager.Add("T72", new T72());
            module_manager.Add("T80", new T80());
            module_manager.Add("T55", new T55());
            module_manager.Add("T62", new T62());
            module_manager.Add("BMP2", new BMP2());
            module_manager.Add("BMP1", new BMP1());
            module_manager.Add("BTR60", new BTR60());
            module_manager.Add("SuperFCS", new SuperFCS());
            module_manager.Add("PactThermal", new PactThermal());
            module_manager.Add("1A40", new FireControlSystem1A40());
            module_manager.Add("BOM", new BOM());
        }

        public override void OnUpdate() 
        {
            BMP2.Update();
        }

        public override void OnSceneWasLoaded(int build_index, string scene_name)
        {
            if (Util.MenuScreens.Contains(scene_name))
            {
                module_manager.LoadAllStaticAssets();
                APSLauncher.Init();
            }

            if (scene_name == "TR01_showcase")
            {
                StateController.RunOrDefer(GameState.MissionUnitsLoaded, new GameStateEventHandler(MissionUnitsLoaded), GameStatePriority.Medium);
            }

            if (Util.MenuScreensWithLoader.Contains(scene_name)) return;

            valid_scene_count++;

            if (valid_scene_count == 1)
            {
                module_manager.UnloadAllDynamicAssets();
            }

            if (valid_scene_count == 2)
            {
                module_manager.LoadAllDynamicAssets();
                StateController.RunOrDefer(GameState.PlayerReady, new GameStateEventHandler(OnPlayerReady), GameStatePriority.Highest);
                StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Ammo_125mm.CreateCompositeOptimizationsDeferred), GameStatePriority.Low);

                PactEra.Init();
                Armour.Init();
                ProximityFuse.Init();
                EFP.Init();
                T72.Init();
                T80.Init();
                T55.Init();
                T62.Init();
                T64A.Init();
                T64B.Init();
                BMP1.Init();
                BMP2.Init();
                BTR60.Init();

                valid_scene_count = 0;
            }
        }
    }
}