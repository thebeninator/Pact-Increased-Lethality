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
using System.Collections.Generic;

[assembly: MelonInfo(typeof(Mod), "Pact Increased Lethality", "2.0.7", "ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace PactIncreasedLethality
{
    public class Mod : MelonMod
    {
        public static Vehicle[] vics;
        public static Dictionary<string, Module> modules = new Dictionary<string, Module>();
        public static MelonPreferences_Category cfg;

        private GameObject game_manager;
        public static AudioSettingsManager audio_settings_manager;
        public static PlayerInput player_manager;
        public static CameraManager camera_manager;

        public IEnumerator OnGameReady(GameState _) 
        {
            game_manager = GameObject.Find("_APP_GHPC_");
            audio_settings_manager = game_manager.GetComponent<AudioSettingsManager>();
            player_manager = game_manager.GetComponent<PlayerInput>();
            camera_manager = game_manager.GetComponent<CameraManager>();
            vics = GameObject.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);

            Ammo_125mm.CreateCompositeOptimizations();

            foreach (string id in modules.Keys)
            {
                Module module = modules[id];
                bool loaded = module.TryLoadDynamicAssets();

                if (loaded) {
                    MelonLogger.Msg("PIL dynamic assets loaded from module: " + id);
                }
            }

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

            modules.Add("SharedAssets", new SharedAssets());
            modules.Add("AMMO_30MM", new Ammo_30mm());
            modules.Add("T72", new T72());
            modules.Add("T80", new T80());
            //modules.Add("T55", new T55());
            //modules.Add("T62", new T62());
            modules.Add("BMP2", new BMP2());
            modules.Add("BMP1", new BMP1());
            modules.Add("BTR60", new BTR60());
            modules.Add("SuperFCS", new SuperFCS());
            modules.Add("PactThermal", new PactThermal());
            modules.Add("1A40", new FireControlSystem1A40());
            modules.Add("BOM", new BOM());
        }

        public override void OnUpdate() 
        {
            BMP2.Update();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu2_Scene" || sceneName == "MainMenu2-1_Scene" || sceneName == "t64_menu")
            {
                foreach (string id in modules.Keys) 
                {
                    Module module = modules[id];
                    bool static_loaded = module.TryLoadStaticAssets();  
                    bool dynamic_unloaded = module.TryUnloadDynamicAssets();
                    
                    if (static_loaded) 
                    {
                        MelonLogger.Msg("PIL static assets loaded from module: " + id);
                    }

                    if (dynamic_unloaded)
                    {
                        MelonLogger.Msg("PIL dynamic assets unloaded from module: " + id);
                    }
                }

                Ammo_125mm.LoadAssets();

                AssetUtil.ReleaseVanillaAssets();
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