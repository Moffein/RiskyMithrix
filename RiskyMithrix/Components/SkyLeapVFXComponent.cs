using RiskyMithrix.Modules;
using RoR2;
using UnityEngine;

namespace RiskyMithrix.Components
{
    //JANK
    public class SkyLeapVFXComponent : MonoBehaviour
    {
        public Vector3 offset = Vector3.zero;
        public float durationBetweenVFX = 1f/2f;
        private float stopwatch = 0f;

        private void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= durationBetweenVFX)
            {
                stopwatch -= durationBetweenVFX;
                SpawnVFX();
            }
        }

        public void SpawnVFX()
        {
            EffectManager.SpawnEffect(PluginAssets.Effects.SkyLeapPredictionEffect,
                new EffectData
                {
                    origin = transform.position - offset
                },
                true
            );
        }
    }
}
