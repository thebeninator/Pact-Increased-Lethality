using System;
using System.Linq;
using FMOD.Studio;
using GHPC.Audio;
using GHPC.Effects;
using GHPC;
using UnityEngine;

namespace PactIncreasedLethality.APS
{
    internal class APSLauncher
    {
        private static AmmoType dummy_he;

        public IUnit parent_unit;
        private Transform[] projectiles;
        private int projectile_count = 0;
        private int current_projectile_idx = 0;
        public bool IsEmpty => projectile_count <= 0;

        public static void Init()
        {
            if (dummy_he != null) return;

            dummy_he = new AmmoType();
            dummy_he.DetonateEffect = Resources.FindObjectsOfTypeAll<GameObject>().Where(o => o.name == "HE_explosion").First();
            dummy_he.ImpactEffectDescriptor = new ParticleEffectsManager.ImpactEffectDescriptor()
            {
                HasImpactEffect = true,
                ImpactCategory = ParticleEffectsManager.Category.HighExplosive,
                EffectSize = ParticleEffectsManager.EffectSize.MainGun,
                RicochetType = ParticleEffectsManager.RicochetType.None,
                Flags = ParticleEffectsManager.ImpactModifierFlags.Large,
                MinFilterStrictness = ParticleEffectsManager.FilterStrictness.Low
            };
        }

        public APSLauncher(Transform projectile_holder)
        {
            projectile_count = projectile_holder.childCount;
            projectiles = new Transform[projectile_count];
            for (int i = 0; i < projectile_count; i++)
            {
                Transform projectile = projectile_holder.GetChild(i);
                projectiles[i] = projectile;
                GameObject fx = GameObject.Instantiate(SharedAssets.tow_front_blast, projectile);
                fx.transform.localPosition = Vector3.zero;
                fx.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            }
        }

        public void FireProjectile(Vector3 pos)
        {
            Transform current_projectile = projectiles[current_projectile_idx];

            ParticleEffectsManager.Instance.CreateImpactEffectOfType(dummy_he, 
                ParticleEffectsManager.FusedStatus.Fuzed, ParticleEffectsManager.SurfaceMaterial.None, false, pos);
            ImpactSFXManager.Instance.PlaySimpleImpactAudio(ImpactAudioType.Missile, pos);

            FmodGenericAudioManager.PlayOneShot(
                "event:/Weapons/launcher_9M14",
                current_projectile.transform.position,
                new ValueTuple<PARAMETER_ID, float>
                (
                    FmodInteriorTracker._instance._isInteriorParameterID, 
                    FmodInteriorTracker.IsInteriorView(parent_unit) ? 1f : 0f
                )
            );

            current_projectile.GetComponentInChildren<ParticleSystem>().Play();
            current_projectile.gameObject.GetComponent<MeshRenderer>().enabled = false;
            projectile_count--;
            current_projectile_idx++;
        }
    }
}
