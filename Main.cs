using System.Linq;
using MelonLoader;
using UnityEngine;
using GHPC.State;
using System.Collections;
using MelonLoader.Utils;
using System.IO;
using GHPC.Audio;
using GHPC.Player;
using GHPC.Camera;
using FMOD;
using GHPC.Vehicle;
using PactIncreasedLethality;
using ModUtil;
using ActiveProtectionSystem;

[assembly: MelonInfo(typeof(Mod), "Pact Increased Lethality", "2.1.7", "ATLAS")]
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

        internal static FMOD.ChannelGroup audio_channel_group;

        public IEnumerator OnGameReady(GameState _) 
        {
            game_manager = GameObject.Find("_APP_GHPC_");
            audio_settings_manager = game_manager.GetComponent<AudioSettingsManager>();
            player_manager = game_manager.GetComponent<PlayerInput>();
            camera_manager = game_manager.GetComponent<CameraManager>();
            vics = GameObject.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);

            module_manager.LoadAllDynamicAssets();
            Ammo_125mm.CreateCompositeOptimizations();

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

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            module_manager.UnloadAllDynamicAssets();

            if (sceneName == "MainMenu2_Scene" || sceneName == "MainMenu2-1_Scene" || sceneName == "t64_menu")
            {
                module_manager.LoadAllStaticAssets();
                AssetUtil.ReleaseVanillaAssets();
                APSLauncher.Init();
            }

            //TODO why is this needed?        
            if (sceneName == "GT01_Beginers_Luck") 
            {
                AssetUtil.LoadVanillaVehicle("T72M");
            }

            if (Util.menu_screens.Contains(sceneName)) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(OnGameReady), GameStatePriority.Medium);

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
        }
    }
}