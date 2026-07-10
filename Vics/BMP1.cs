using System.Collections;
using System.IO;
using GHPC.State;
using GHPC.Vehicle;
using GHPC.Weapons;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using FMOD;
using FMODUnity;
using HarmonyLib;
using GHPC.Camera;
using GHPC.Weaponry;
using GHPC;
using UnityEngine.Rendering.PostProcessing;
using TMPro;
using GHPC.UI.Map;
using GHPC.UI;
using ModUtil;
using System.Linq;

namespace PactIncreasedLethality
{
    [HarmonyPatch(typeof(WeaponAudio), "FinalStartLoop")]
    public class AGS17_Sound
    {
        public static FMOD.Sound[] sounds_interior = new FMOD.Sound[6];
        public static FMOD.Sound[] sounds_exterior = new FMOD.Sound[7];

        public static bool Prefix(WeaponAudio __instance)
        {
            if (__instance.SingleShotMode && __instance.SingleShotEventPaths[0] == "blyat")
            {
                FMOD.Sound sound_interior = sounds_interior[UnityEngine.Random.Range(0, sounds_interior.Length)];
                FMOD.Sound sound_exterior = sounds_exterior[UnityEngine.Random.Range(0, sounds_exterior.Length)];

                var corSystem = RuntimeManager.CoreSystem;

                Vector3 vec = __instance.transform.position;

                VECTOR pos = new VECTOR();
                pos.x = vec.x;
                pos.y = vec.y;
                pos.z = vec.z;

                VECTOR vel = new VECTOR();
                vel.x = 0f;
                vel.y = 0f;
                vel.z = 0f;

                bool is_player_instance = __instance == Mod.player_manager.CurrentPlayerWeapon.Weapon.WeaponSound;
                bool interior = !CameraManager._instance.ExteriorMode && is_player_instance;

                FMOD.Channel new_channel;
                FMOD.Sound sound = interior ? sound_interior : sound_exterior;

                corSystem.playSound(sound, Mod.audio_channel_group, true, out new_channel);

                float game_vol = Mod.audio_settings_manager._previousVolume;
                float gun_vol = interior ? game_vol * 1.1f : game_vol * 1.1f;

                if (!is_player_instance && !CameraManager._instance.ExteriorMode)
                {
                    gun_vol *= 0.5f;
                }

                new_channel.setVolume(gun_vol);
                new_channel.set3DAttributes(ref pos, ref vel);
                new_channel.setPaused(false);
                new_channel.clearHandle();

                return false;
            }

            return true;
        }
    }

    internal class SmartRoundHandler : MonoBehaviour
    {
        private LiveRound self;
        private static AmmoType guided_round = null;
        private bool started_guidance = false;

        void Awake()
        {
            self = this.GetComponent<LiveRound>();
        }

        void Update()
        {
            if (!started_guidance && Vector3.Angle(this.transform.forward, Vector3.up) >= 80f)
            {
                MissileGuidanceUnit mgu = ArtilleryCamManager.Instance.GetComponent<MissileGuidanceUnit>();

                if (guided_round == null)
                {
                    guided_round = new AmmoType();
                    Util.ShallowCopy(guided_round, self.Info);
                    guided_round.Guidance = AmmoType.GuidanceType.Laser;
                    guided_round.TurnSpeed = 0.07f;
                }

                self.Info = guided_round;
                self.Guided = true;

                mgu.AddMissile(self);
                started_guidance = true;
            }
        }
    }


    //[HarmonyPatch(typeof(GHPC.Weapons.LiveRound), "Start")]
    //public static class SmartRound
    //{
    //    public static void Prefix(GHPC.Weapons.LiveRound __instance)
    //    {
    //        if (__instance.Info.Name == "PG-15V HEAT")
    //        {
    //            __instance.gameObject.AddComponent<SmartRoundHandler>();
    //        }
    //    }
    //}

    internal class ArtilleryManager : MonoBehaviour
    {
        public static ArtilleryManager ActiveInstance;
        public Vehicle self;
        public FireControlSystem fcs;
        public AimablePlatform turret_platform;
        public AimablePlatform gun_platform;
        public Vector3? last_coords = null;
        public GameObject monitor;
        public bool ele_locked = false;

        private float cd = 0.0f;
        private float fov = 90f;
        private TextMeshProUGUI elevation;
        private TextMeshProUGUI range;
        private TextMeshProUGUI xcoord;
        private TextMeshProUGUI ycoord;
        private TextMeshProUGUI rot_diff;

        private Transform traverse_lock;
        private Transform elevation_lock;

        void Awake()
        {
            elevation = monitor.transform.Find("UI/ELEV/num").GetComponent<TextMeshProUGUI>();
            range = monitor.transform.Find("UI/RANGE/num").GetComponent<TextMeshProUGUI>();
            xcoord = monitor.transform.Find("UI/XCOORD/num").GetComponent<TextMeshProUGUI>();
            ycoord = monitor.transform.Find("UI/YCOORD/num").GetComponent<TextMeshProUGUI>();
            rot_diff = monitor.transform.Find("UI/ROT DIFF/num").GetComponent<TextMeshProUGUI>();
            elevation_lock = monitor.transform.Find("UI/ELE LOCK");
            traverse_lock = monitor.transform.Find("UI/TRA LOCK");
        }

        void Update()
        {
            if (cd > 0.0f)
            {
                cd -= Time.deltaTime;
            }

            ArtilleryCamManager arty_cam = ArtilleryCamManager.Instance;

            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                if (!last_coords.HasValue)
                {
                    last_coords = self.transform.position;
                }

                monitor.SetActive(!monitor.activeSelf);
                arty_cam.SetActive(!arty_cam.enabled);
                arty_cam.SetCoords(last_coords.Value);

                if (monitor.activeSelf)
                {
                    ActiveInstance = this;
                }
                else
                {
                    //ActiveInstance = null;
                }
            }

            if (!monitor.activeSelf) return;

            elevation.text = gun_platform.GetCurrentEulerAngle().ToString("0.00");

            if (cd <= 0.0f)
            {
                Vector3 weapon_dir = Vector3.ProjectOnPlane(fcs.CurrentWeaponSystem.MuzzleIdentity.forward, Vector3.up);
                Vector3 self_to_cam = Vector3.ProjectOnPlane(arty_cam.transform.position - self.transform.position, Vector3.up);

                xcoord.text = arty_cam.transform.position.x.ToString("0.00");
                ycoord.text = arty_cam.transform.position.z.ToString("0.00");
                rot_diff.text = Vector3.Angle(self_to_cam, weapon_dir).ToString("0.00") + "°";

                cd = 0.1f;
            }

            if (Input.GetKey(KeyCode.Keypad4))
            {
                fov += 1f;

                if (fov >= 120f)
                {
                    fov = 120f;
                }

                arty_cam.FovChanged(fov);
            }

            if (Input.GetKey(KeyCode.Keypad5))
            {
                fov -= 1f;

                if (fov <= 20f)
                {
                    fov = 20f;
                }

                arty_cam.FovChanged(fov);
            }

            if (Input.GetKey(KeyCode.Keypad1))
            {
                RaycastHit raycast_hit;
                if (Physics.Raycast(arty_cam.transform.position, arty_cam.cam.transform.forward, out raycast_hit, 9999f, ConstantsAndInfoManager.Instance.LaserRangefinderLayerMask.value))
                {
                    fcs.SetAimWorldPosition(raycast_hit.point);
                }
            }

            if (Input.GetKey(KeyCode.Keypad2))
            {
                RaycastHit raycast_hit;
                if (Physics.Raycast(arty_cam.transform.position, arty_cam.cam.transform.forward, out raycast_hit, 9999f, ConstantsAndInfoManager.Instance.LaserRangefinderLayerMask.value))
                {
                    float dist = (raycast_hit.point - self.transform.position).magnitude;
                    fcs.SetRange(dist, true);
                    fcs._targetRange = dist;
                    range.text = dist.ToString("0000");
                }
            }
        }
    }
    internal class ArtilleryCamManager : MonoBehaviour
    {
        public static ArtilleryCamManager Instance;
        public Camera cam;

        void Awake()
        {
            Instance = this;
            cam = this.GetComponentInChildren<Camera>();
            MapController.Instance.AddAutoTrackingIcon("Obs Satellite", MapIconType.CommsTarget, Color.black, cam.transform, false);
        }

        public void SetActive(bool active)
        {
            cam.enabled = active;
            this.enabled = active;
        }

        public void SetCoords(Vector3 coords)
        {
            this.transform.position = coords + Vector3.up * 50f;
        }
        public void FovChanged(float new_fov)
        {
            cam.fieldOfView = new_fov;
        }
        public void Update()
        {
            Vector3 move = Vector3.zero;
            float sensitivity = 0.12f;

            int going_up = Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
            int going_down = Input.GetKey(KeyCode.DownArrow) ? 1 : 0;
            int going_left = Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
            int going_right = Input.GetKey(KeyCode.RightArrow) ? 1 : 0;

            move += (Vector3.forward * going_up) + (Vector3.back * going_down);
            move += (Vector3.right * going_right) + (Vector3.left * going_left);

            if (Input.GetKey(KeyCode.RightShift))
            {
                sensitivity = 1.0f;
            }

            if (Input.GetKey(KeyCode.RightControl))
            {
                sensitivity = 0.08f;
            }

            this.transform.position += move * sensitivity;
        }
    }

    public class BMP1 : Module
    {
        static MelonPreferences_Entry<bool> bmp1_patch;
        internal static MelonPreferences_Entry<bool> ags_17_bmp1;
        internal static MelonPreferences_Entry<bool> ags_17_bmp1p;
        static MelonPreferences_Entry<bool> vog17m1_hedp;

        static WeaponSystemCodexScriptable gun_ags17;
        static AmmoClipCodexScriptable clip_codex_vog17;
        static AmmoType.AmmoClip clip_vog17;
        static AmmoCodexScriptable ammo_codex_vog17;
        static AmmoType ammo_vog17;

        static AmmoClipCodexScriptable clip_codex_vog17m1;
        static AmmoType.AmmoClip clip_vog17m1;
        static AmmoCodexScriptable ammo_codex_vog17m1;
        static AmmoType ammo_vog17m1;
        static GameObject arty_monitor;
        static GameObject arty_cam;

        public static void Config(MelonPreferences_Category cfg)
        {
            bmp1_patch = cfg.CreateEntry<bool>("BMP-1 Patch", true);
            bmp1_patch.Description = "//////////////////////////////////////////////////////////////////////////////////////////";

            ags_17_bmp1 = cfg.CreateEntry<bool>("AGS-17D Coax (BMP-1)", true);
            ags_17_bmp1.Comment = "Replaces PKT coax with AGS-17D 30mm grenade launcher";

            ags_17_bmp1p = cfg.CreateEntry<bool>("AGS-17D Coax (BMP-1P)", true);
            ags_17_bmp1p.Comment = "Replaces PKT coax with AGS-17D 30mm grenade launcher";

            vog17m1_hedp = cfg.CreateEntry<bool>("Use VOG-17M1 HEDP", false);
            vog17m1_hedp.Comment = "Fictional grenade for the AGS-17D. Behaves like a HEAT round";
        }

        public static IEnumerator Convert(GameState _)
        {
            //if (ArtilleryCamManager.Instance == null)
            //{
            //    GameObject _arty_cam = GameObject.Instantiate(arty_cam);
            //    ArtilleryCamManager arty_cam_man = _arty_cam.AddComponent<ArtilleryCamManager>();
            //    arty_cam_man.SetActive(false);
            //    MissileGuidanceUnit mgu = _arty_cam.AddComponent<MissileGuidanceUnit>();
            //    mgu.AimElement = _arty_cam.GetComponentInChildren<Camera>().transform;
            //}

            foreach (Vehicle vic in Mod.vics)
            {
                GameObject vic_go = vic.gameObject;

                if (vic == null) continue;
                if (vic.GetComponent<AlreadyConverted>()) continue;

                string name = vic.FriendlyName;

                if (!name.Contains("BMP-1")) continue;
                LoadoutManager loadout_manager = vic.GetComponent<LoadoutManager>();
                FireControlSystem fcs = loadout_manager._weaponsManager.Weapons[0].FCS;
                //fcs.SuperelevateWeapon = true;
                //fcs._originalRangeLimits = new Vector2(0f, 3000f);
                //fcs.RegisteredRangeLimits = new Vector2(0f, 3000f);
                //fcs.CurrentStabMode = StabilizationMode.Vector;
                //fcs.Mounts[0]._stabActive = true;
                //fcs.Mounts[0].Stabilized = true;
                //fcs.Mounts[0].StabilizerActive = true;
                //fcs.RangeStep = 10;
                //fcs._originalRangeStep = 10;
                //fcs.DisplayRangeIncrement = 10;

                //fcs.Mounts[1]._stabActive = true;
                //fcs.Mounts[1].Stabilized = true;
                //fcs.Mounts[1].StabilizerActive = true;
                //fcs.Mounts[1].LocalEulerLimits = new Vector2(-4f, 50f);
                //fcs.StabsActive = true;

                //Transform optic = vic.transform.Find("BMP1_rig/HULL/TURRET/GUN/Gun Scripts/gunner day sight/Optic");
                //GameObject _arty_monitor = GameObject.Instantiate(arty_monitor, optic);
                //_arty_monitor.SetActive(false);

                //ArtilleryManager arty_man = optic.gameObject.AddComponent<ArtilleryManager>();
                //arty_man.monitor = _arty_monitor;
                //arty_man.self = vic;
                //arty_man.fcs = fcs;
                //arty_man.gun_platform = fcs.Mounts[1];
                //arty_man.turret_platform = fcs.Mounts[0];

                if (!name.Contains("G") && (ags_17_bmp1.Value && name == "BMP-1") || (ags_17_bmp1p.Value && name == "BMP-1P"))
                {
                    WeaponSystem coax = vic.GetComponent<WeaponsManager>().Weapons[2].Weapon;
                    coax.WeaponSound.SingleShotMode = true;
                    coax.WeaponSound.SingleShotEventPaths = new string[] { "blyat" };
                    coax.BaseDeviationAngle *= 12f;
                    coax.SetCycleTime(0.19f);
                    coax.CodexEntry = gun_ags17;
                    coax.Feed.AmmoTypeInBreech = null;
                    coax.Feed.ReadyRack.ClipTypes[0] = vog17m1_hedp.Value ? clip_vog17m1 : clip_vog17;
                    coax.Feed.ReadyRack.Awake();
                    coax.Feed.Start();

                    vic._friendlyName += "G";
                }

                vic.gameObject.AddComponent<AlreadyConverted>();
            }

            yield break;
        }

        public override void LoadDynamicAssets()
        {
            if (!bmp1_patch.Value) return;

            //BallisticComputer pg15v_bc = BallisticComputerRepository._precalculatedComputers.Where(o => o.Key.Name == "PG-15V HEAT").First().Value;
            //pg15v_bc.Ammo.MuzzleVelocity = 200f;
            //pg15v_bc.Ammo.Coeff = 0.2f; // note, ballistic sim can't keep up with high drag projectiles
            //pg15v_bc.Ammo.TntEquivalentKg *= 1.5f;
            //pg15v_bc.Ammo.RhaPenetration = 900f;

            //pg15v_bc.MaxFlightTimeSeconds = 150f;
            //pg15v_bc.MaxElevation = 120f;
            //pg15v_bc.MaxFineElevation = 120f;
            //pg15v_bc.ElevationStep = 0.20f;
            //pg15v_bc.FineElevationStep = 0.05f;
            //pg15v_bc.SimTimeStep = 0.005f;

            //pg15v_bc.RefreshData();

            string[] bmp1s = { "BMP1", "BMP1 Soviet", "BMP1P (Variant)", "BMP1P (Variant) Soviet" };
            bool has_vog_bmp1 = AssetUtil.VehicleInMission(bmp1s) && (ags_17_bmp1.Value || ags_17_bmp1p.Value);

            if (!has_vog_bmp1) return;

            gun_ags17 = ScriptableObject.CreateInstance<WeaponSystemCodexScriptable>();
            gun_ags17.name = "gun_ags17";
            gun_ags17.CaliberMm = 30;
            gun_ags17.FriendlyName = "30mm grenade launcher AGS-17D";
            gun_ags17.Type = WeaponSystemCodexScriptable.WeaponType.GrenadeLauncher;

            ammo_vog17 = new AmmoType();
            Util.ShallowCopy(ammo_vog17, Ammo_30mm.ammo_3uor6);
            ammo_vog17.Name = "VOG-17M HE";
            ammo_vog17.MuzzleVelocity = 200f;
            ammo_vog17.VisualType = LiveRoundMarshaller.LiveRoundVisualType.Bullet;
            ammo_vog17.UseTracer = false;
            ammo_vog17.RhaPenetration = 5f;
            ammo_vog17.DetonateSpallCount = 50;
            ammo_vog17.TntEquivalentKg = 0.064f;
            ammo_vog17.ArmingDistance = 25f;
            ammo_vog17.ImpactFuseTime = 0f;

            ammo_codex_vog17 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_vog17.AmmoType = ammo_vog17;
            ammo_codex_vog17.name = "ammo_vog17";

            clip_vog17 = new AmmoType.AmmoClip();
            clip_vog17.Capacity = 300;
            clip_vog17.Name = "VOG-17M HE";
            clip_vog17.MinimalPattern = new AmmoCodexScriptable[1];
            clip_vog17.MinimalPattern[0] = ammo_codex_vog17;

            clip_codex_vog17 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_vog17.name = "clip_vog17";
            clip_codex_vog17.ClipType = clip_vog17;

            ammo_vog17m1 = new AmmoType();
            Util.ShallowCopy(ammo_vog17m1, Ammo_30mm.ammo_3uor6);
            ammo_vog17m1.Name = "VOG-17M1 HEDP";
            ammo_vog17m1.MuzzleVelocity = 200f;
            ammo_vog17m1.VisualType = LiveRoundMarshaller.LiveRoundVisualType.Bullet;
            ammo_vog17m1.UseTracer = false;
            ammo_vog17m1.RhaPenetration = 50f;
            ammo_vog17m1.DetonateSpallCount = 20;
            ammo_vog17m1.TntEquivalentKg = 0.032f;
            ammo_vog17m1.ArmingDistance = 25f;
            ammo_vog17m1.Category = AmmoType.AmmoCategory.ShapedCharge;
            ammo_vog17m1.SpallMultiplier = 0.1f;
            ammo_vog17m1.ImpactFuseTime = 0f;
            ammo_vog17m1.ShatterOnRicochet = false;
            ammo_vog17m1.AlwaysProduceBlast = true;

            ammo_codex_vog17m1 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
            ammo_codex_vog17m1.AmmoType = ammo_vog17m1;
            ammo_codex_vog17m1.name = "ammo_vog17m1";

            clip_vog17m1 = new AmmoType.AmmoClip();
            clip_vog17m1.Capacity = 300;
            clip_vog17m1.Name = "VOG-17M1 HEDP";
            clip_vog17m1.MinimalPattern = new AmmoCodexScriptable[1];
            clip_vog17m1.MinimalPattern[0] = ammo_codex_vog17m1;

            clip_codex_vog17m1 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
            clip_codex_vog17m1.name = "clip_vog17m1";
            clip_codex_vog17m1.ClipType = clip_vog17m1;
        }

        public override void LoadStaticAssets()
        {
            if (!bmp1_patch.Value) return;

            var corSystem = FMODUnity.RuntimeManager.CoreSystem;

            for (int i = 0; i < 6; i++)
            {
                corSystem.createSound(
                    Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/ags17", "aavp7a1_mk19_fire_1p_0" + (i + 1) + ".ogg"), MODE._3D_IGNOREGEOMETRY, out AGS17_Sound.sounds_interior[i]);
                AGS17_Sound.sounds_interior[i].set3DMinMaxDistance(300f, 550f);
            }

            for (int i = 0; i < 7; i++)
            {
                corSystem.createSound(
                    Path.Combine(MelonEnvironment.ModsDirectory + "/PIL/ags17", "aavp7a1_mk19_fire_close_3p_0" + (i + 1) + ".ogg"), MODE._3D_INVERSETAPEREDROLLOFF, out AGS17_Sound.sounds_exterior[i]);
                AGS17_Sound.sounds_exterior[i].set3DMinMaxDistance(50f, 550f);
            }

            //var arty_bundle = AssetBundle.LoadFromFile(Path.Combine(MelonEnvironment.ModsDirectory + "/PIL", "bmp1_arty"));
            //arty_monitor = arty_bundle.LoadAsset<GameObject>("BMP1 ARTY MONITOR.prefab");
            //arty_monitor.hideFlags = HideFlags.DontUnloadUnusedAsset;

            //arty_cam = arty_bundle.LoadAsset<GameObject>("BMP1 ARTY CAM.prefab");
            //arty_cam.hideFlags = HideFlags.DontUnloadUnusedAsset;
            //arty_cam.GetComponentInChildren<PostProcessLayer>().volumeLayer = 1 << 23;
            //arty_cam.transform.Find("volume").gameObject.layer = 23;
        }

        public static void Init()
        {
            if (!bmp1_patch.Value) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }
}

