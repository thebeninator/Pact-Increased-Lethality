using System;
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

[assembly: MelonInfo(typeof(PactIncreasedLethalityMod), "Pact Increased Lethality", "1.0.0", "ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

/// todo
// [*?] figure out reference shenanigans w/ reticle cloning
// [] applique for t55 
// [] the brick

namespace PactIncreasedLethality
{
    public class PactIncreasedLethalityMod : MelonMod
    {
        public static GameObject[] vic_gos;
        public static MelonPreferences_Category cfg;

        public IEnumerator GetVics(GameState _) {
            vic_gos = GameObject.FindGameObjectsWithTag("Vehicle");

            yield break;
        }

        public override void OnInitializeMelon()
        {
            cfg = MelonPreferences.CreateCategory("PactIncreasedLethality");
            T55.Config(cfg);
            T72.Config(cfg);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu2_Scene" || sceneName == "LOADER_MENU" || sceneName == "LOADER_INITIAL" || sceneName == "t64_menu") return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(GetVics), GameStatePriority.Low);
            T72.Init();
            T55.Init();
        }
    }
}
