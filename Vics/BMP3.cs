using GHPC.State;
using ModUtil;
using System.Collections;
using GHPC.Vehicle;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class BMP3 : Module
    {
        private static void HandleConversion(Vehicle vic)
        {
            if (vic == null) return;
            if (vic.UniqueName != "BMP2_SA") return;

            Transform bmp2_rig = vic.transform.Find("BMP2_rig");
            Transform bmp2_hull = bmp2_rig.Find("HULL");
            Transform bmp2_turret = bmp2_hull.Find("TURRET");

            vic.transform.Find("BMP2_visual").gameObject.SetActive(false);
            vic.transform.Find("BMP2_markings").gameObject.SetActive(false);
            bmp2_hull.Find("numbers").gameObject.SetActive(false);
            bmp2_turret.Find("convoylight_002").gameObject.SetActive(false);
            bmp2_turret.Find("tactical marker").gameObject.SetActive(false);
            bmp2_turret.Find("tactical marker").gameObject.SetActive(false);
            bmp2_turret.Find("konkurs_azimuth").gameObject.SetActive(false);
            bmp2_turret.Find("turret scripts/R123_Prefab").gameObject.SetActive(false);
        }

        private static IEnumerator Convert(GameState _)
        {
            foreach (Vehicle vic in Mod.vics)
            {
                HandleConversion(vic);
            }

            yield break;
        }


        public static void Init()
        {
            //if (!t72_patch.Value) return;

            StateController.RunOrDefer(GameState.PlayerReady, new GameStateEventHandler(Convert), GameStatePriority.Medium);
        }
    }
}
