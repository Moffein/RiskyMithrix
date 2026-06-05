using RiskyMithrix.Modules;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.RiskyMithrix.Weapon
{
    public class FireLunarOrb : BaseState
    {
        public static float damageCoefficient = 2f;
        public static float baseDuration = 0.25f;
        public static GameObject muzzleFlashEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/MuzzleflashLunarShard.prefab").WaitForCompletion();
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                Transform transform = FindModelChild("MuzzleLeft");
                if (transform)
                {
                    aimRay.origin = transform.position;
                }
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    position = aimRay.origin,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    crit = RollCrit(),
                    damage = damageStat * damageCoefficient,
                    damageColorIndex = DamageColorIndex.Default,
                    owner = gameObject,
                    procChainMask = default,
                    force = 0f,
                    useFuseOverride = false,
                    useSpeedOverride = false,
                    target = null,
                    projectilePrefab = PluginAssets.Projectiles.LunarOrbProjectilePrefab
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            PlayAnimation("Gesture, Additive", "FireLunarShards");
            PlayAnimation("Gesture, Override", "FireLunarShards");
            AddRecoil(-0.6f, -1.2f, -4.5f, 4.5f);
            characterBody.AddSpreadBloom(0.4f);
            EffectManager.SimpleMuzzleFlash(muzzleFlashEffectPrefab, gameObject, "MuzzleLeft", false);
            //Util.PlaySound("Play_moonBrother_m1_laser_shoot", gameObject);
            Util.PlaySound("Play_lunar_wisp_attack2_launch", gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
