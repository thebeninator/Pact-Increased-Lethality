using GHPC.Weapons;
using UnityEngine;

namespace PactIncreasedLethality.Vics.BMP3
{
    internal class BMP3Constants
    {
        internal static readonly string[] bmp2_turret_lf_retained = new string[]
        {
            "AmmoRack_2000X7.62mm",
            "AmmoRack_340X30mmHE",
            "AmmoRack_160X30mmAP",
            "AmmoRack_2000X7.62mm vis",
            "AmmoRack_340X30mmHE vis",
            "AmmoRack_160X30mmAP vis",
            "Gun_ControlBox_BU-25-2S",
            "Radio R-123M"
        };

        internal static readonly string[] bmp2_turret_lf_reposition = new string[]
        {
            "Gunner_Seat",
            "Commander_Seat",
            "Turret_TraverseMotor_PartOf2E36-1Stabilizer",
            "Turret_ManualDrive",
            "Gun_Elevation_ManualDrive",
            "Gun_Elevation_Motor_PartOf2E36-1Stabilizer",
            "Gunner_ElectronicControls",
            "Commander_ElectronicControls",
        };

        internal static readonly AmmoFeed.ReloadStage[] bmp3_reload_sequence = new AmmoFeed.ReloadStage[]
        {
            new AmmoFeed.ReloadStage()
            {
                Duration = 0.4f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[] {}
            },
            new AmmoFeed.ReloadStage()
            {
                Duration = 0.4f,
                WaitForCarousel = true,
                LessWaitSeconds = 0.2f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[] {}
            },
            new AmmoFeed.ReloadStage()
            {
                Duration = 0.2f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[]
                {
                    new AmmoFeed.ReloadStage.FMODParameter()
                    {
                        Name = "WaitForCarousel",
                        Value = 0
                    }
                }
            },
            new AmmoFeed.ReloadStage()
            {
                Duration = 2f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[] {}
            },
            new AmmoFeed.ReloadStage()
            {
                Duration = 0.9f,
                StageClips = new AudioClip[] { },
                AnimatedParts = new AnimatedPart[] { },
                ClipReloadFMODParameters = new AmmoFeed.ReloadStage.FMODParameter[] {}
            },
        };
    }
}
