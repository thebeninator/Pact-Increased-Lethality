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

[assembly: MelonInfo(typeof(Mod), "Pact Increased Lethality", "2.0.7", "ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace PactIncreasedLethality
{
    public class Mod : MelonMod
    {
        public static Vehicle[] vics;
        public static MelonPreferences_Category cfg;

        private GameObject game_manager;
        public static AudioSettingsManager audio_settings_manager;
        public static PlayerInput player_manager;
        public static CameraManager camera_manager;

        public IEnumerator GetVics(GameState _) {
            game_manager = GameObject.Find("_APP_GHPC_");
            audio_settings_manager = game_manager.GetComponent<AudioSettingsManager>();
            player_manager = game_manager.GetComponent<PlayerInput>();
            camera_manager = game_manager.GetComponent<CameraManager>();
            vics = GameObject.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);
            yield break;
        }

        public override void OnInitializeMelon()
        {
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
            //Drozd.Config(cfg);
            Armour.Config(cfg);

            var corSystem = FMODUnity.RuntimeManager.CoreSystem;

            corSystem.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/zsu", "zsu_23_shot.wav"), MODE._3D_INVERSEROLLOFF, out BMP2.ReplaceSound.sound);
            BMP2.ReplaceSound.sound.set3DMinMaxDistance(30f, 1300f);

            corSystem.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/zsu", "zsu_23_shot_exterior.wav"), MODE._3D_INVERSEROLLOFF, out BMP2.ReplaceSound.sound_exterior);
            BMP2.ReplaceSound.sound_exterior.set3DMinMaxDistance(30f, 1300f);

            corSystem.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/btr60a", "btr2a72_interior.ogg"), MODE._3D_INVERSEROLLOFF, out BMP2.ReplaceSound.sound_alt);
            BMP2.ReplaceSound.sound_alt.set3DMinMaxDistance(30f, 1300f);
        }

        public override void OnUpdate() {
            BMP2.Update();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu2_Scene" || sceneName == "MainMenu2-1_Scene" || sceneName == "t64_menu")
            {
                Assets.Load();
                T72.LoadAssets();
                T80.LoadAssets();
                T62.LoadAssets();
                T55.LoadAssets();
                BMP2.LoadAssets();
                BTR60.LoadAssets();
                BOM.LoadAssets();
                PactThermal.LoadAssets();
                FireControlSystem1A40.LoadAssets();
                SuperFCS.LoadAssets();
                Ammo_125mm.LoadAssets();
                Ammo_30mm.LoadAssets();
                TrackingDimensions.Generate();
            }

            if (Util.menu_screens.Contains(sceneName)) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(GetVics), GameStatePriority.Medium);
            
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