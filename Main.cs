﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using GHPC.State;
using PactIncreasedLethality;
using System.Collections;
using MelonLoader.Utils;
using System.IO;
using Thermals;
using GHPC;
using GHPC.Audio;
using GHPC.Player;
using GHPC.Camera;
using FMODUnity;
using FMOD;

[assembly: MelonInfo(typeof(PactIncreasedLethalityMod), "Pact Increased Lethality", "1.6.2", "ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace PactIncreasedLethality
{
    public class PactIncreasedLethalityMod : MelonMod
    {
        public static GameObject[] vic_gos;
        public static MelonPreferences_Category cfg;

        private GameObject game_manager;
        public static AudioSettingsManager audio_settings_manager;
        public static PlayerInput player_manager;
        public static CameraManager camera_manager;

        public IEnumerator GetVics(GameState _) {
            vic_gos = GameObject.FindGameObjectsWithTag("Vehicle");

            yield break;
        }

        public override void OnInitializeMelon()
        {
            cfg = MelonPreferences.CreateCategory("PactIncreasedLethality");
            T55.Config(cfg);
            T72.Config(cfg);
            T64.Config(cfg);
            T62.Config(cfg);
            T80.Config(cfg);
            BMP1.Config(cfg);
            BMP2.Config(cfg);
            BTR60.Config(cfg);
            Kontakt1.Config(cfg);
            Drozd.Config(cfg);
            PactThermal.Config(cfg);

            var corSystem = FMODUnity.RuntimeManager.CoreSystem;

            corSystem.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/zsu", "zsu_23_shot.wav"), MODE._3D_INVERSEROLLOFF, out BMP2.ReplaceSound.sound);
            BMP2.ReplaceSound.sound.set3DMinMaxDistance(30f, 1300f);

            corSystem.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/zsu", "zsu_23_shot_exterior.wav"), MODE._3D_INVERSEROLLOFF, out BMP2.ReplaceSound.sound_exterior);
            BMP2.ReplaceSound.sound_exterior.set3DMinMaxDistance(30f, 1300f);

            corSystem.createSound(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/btr60a", "btr2a72_interior.ogg"), MODE._3D_INVERSEROLLOFF, out BMP2.ReplaceSound.sound_alt);
            BMP2.ReplaceSound.sound_alt.set3DMinMaxDistance(30f, 1300f);
        }

        public override void OnLateUpdate()
        {
            T55.OnLateUpdate();
        }

        public override void OnUpdate() {
            BMP2.Update();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Kontakt1.LoadTex();

            if (Util.menu_screens.Contains(sceneName)) return;

            game_manager = GameObject.Find("_APP_GHPC_");
            audio_settings_manager = game_manager.GetComponent<AudioSettingsManager>();
            player_manager = game_manager.GetComponent<PlayerInput>();
            camera_manager = game_manager.GetComponent<CameraManager>();

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(GetVics), GameStatePriority.Medium);

            Kontakt1.Init();
            Kontakt5.Init();
            APFSDS_125mm.Init();
            AMMO_30MM.Init();
            PactThermal.Init();
            Sosna.Init();
            Drozd.Init();
            ProximityFuse.Init();
            EFP.Init();
            T72.Init();
            BMP2.Init();
            T55.Init();
            BMP1.Init();
            T64.Init();
            T62.Init();
            T80.Init();
        }
    }
}
