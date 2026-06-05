using HG;
using RiskyMithrix.Modules;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.RiskyMithrix
{
    public class WorldSlam : BaseState
    {
        private Animator modelAnimator;
        private Transform modelTransform;
        private float duration;
        private GameObject chargeInstance;
        private bool hasAttacked = false;

        public static float baseDuration = 6f;
        public static float damageCoefficient = 6f;
        public static GameObject chargeEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ChargeBrotherFist.prefab").WaitForCompletion();
        public static GameObject slamImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion();
        public static float waveProjectileDamageCoefficient = 4f;
        public static float waveProjectileForce = 4000f;
        public static int waveProjectileCount = 4;
        public static float lineDamageCoefficient = 9f;
        private bool isCrit;

        public override void OnEnter()
        {
            base.OnEnter();
            isCrit = RollCrit();
            modelAnimator = base.GetModelAnimator();
            modelTransform = base.GetModelTransform();
            duration = baseDuration / attackSpeedStat;
            Util.PlayAttackSpeedSound("Play_moonBrother_orb_slam_pre", gameObject, attackSpeedStat);
            base.PlayCrossfade("FullBody Override", "FistSlam", "playbackRate", duration, 0.1f);
            if (base.characterDirection)
            {
                base.characterDirection.moveVector = base.characterDirection.forward;
            }
            if (modelTransform)
            {
                AimAnimator component = modelTransform.GetComponent<AimAnimator>();
                if (component)
                {
                    component.enabled = true;
                }
            }
            Transform transform = base.FindModelChild("MuzzleRight");
            if (transform && chargeEffectPrefab)
            {
                chargeInstance = UnityEngine.Object.Instantiate<GameObject>(chargeEffectPrefab, transform.position, transform.rotation);
                chargeInstance.transform.parent = transform;
                ScaleParticleSystemDuration component2 = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
                if (component2)
                {
                    component2.newDuration = duration / 2.8f;
                }
            }
        }

        // Token: 0x06001A71 RID: 6769 RVA: 0x000140EE File Offset: 0x000122EE
        public override void OnExit()
        {
            if (chargeInstance)
            {
                EntityState.Destroy(chargeInstance);
            }
            PlayAnimation("FullBody Override", "BufferEmpty");
            base.OnExit();
        }

        // Token: 0x06001A72 RID: 6770 RVA: 0x000B506C File Offset: 0x000B326C
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (modelAnimator && (modelAnimator.GetFloat("fist.hitBoxActive") > 0.5f || fixedAge >= duration*0.5f) && !hasAttacked)
            {
                if (chargeInstance)
                {
                    EntityState.Destroy(chargeInstance);
                }
                EffectManager.SimpleMuzzleFlash(slamImpactEffect, gameObject, "MuzzleFloor", false);
                if (base.isAuthority)
                {
                    if (modelTransform)
                    {
                        Transform transform = base.FindModelChild("MuzzleFloor");
                        if (transform)
                        {
                            new BlastAttack
                            {
                                attacker = gameObject,
                                inflictor = gameObject,
                                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                                baseDamage = damageStat * damageCoefficient,
                                baseForce = 5000f,
                                position = transform.position,
                                radius = 12f,
                                bonusForce = new Vector3(0f, 1000f, 0f),
                                crit = isCrit
                            }.Fire();
                        }
                    }
                    float num = 360f / (float)waveProjectileCount;
                    Vector3 point = Vector3.ProjectOnPlane(base.inputBank.aimDirection, Vector3.up);
                    Vector3 footPosition = base.characterBody.footPosition;
                    for (int i = 0; i < waveProjectileCount; i++)
                    {
                        Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * point;
                        ProjectileManager.instance.FireProjectileWithoutDamageType(PluginAssets.Projectiles.AntiFlyingUltOrbVanillaPrefab, footPosition, Util.QuaternionSafeLookRotation(forward), gameObject, base.characterBody.damage * waveProjectileDamageCoefficient, waveProjectileForce, isCrit, DamageColorIndex.Default, null, -1f);
                    }
                    FireAntiFlyingProjectile();
                }
                hasAttacked = true;
            }
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public void FireAntiFlyingProjectile()
        {
            int targetCount = (PhaseCounter.instance && PhaseCounter.instance.phase > 1) ? 3 : 1;
            TeamIndex myTeam = GetTeam();
            var allEnemyBodies = CharacterBody.instancesList.Where(b =>
            {
                return b.teamComponent
                && b.teamComponent.teamIndex != myTeam
                && b.master != null
                && !b.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless)
                && b.healthComponent && b.healthComponent.alive;
            });
            var allPlayerBodies = allEnemyBodies.Where(b => b.isPlayerControlled);
            var allAirbornePlayerBodies = allPlayerBodies.Where(b => b.characterMotor && b.characterMotor.isGrounded);

            for (int i = 0; i < targetCount; i++)
            {
                IEnumerable<CharacterBody> toSelect = null;
                if (allAirbornePlayerBodies.Count() > 0)
                {
                    toSelect = allAirbornePlayerBodies;
                }
                else if (allPlayerBodies.Count() > 0)
                {
                    toSelect = allPlayerBodies;
                }
                else if (allEnemyBodies.Count() > 0)
                {
                    toSelect = allEnemyBodies;
                }

                Vector3 position = transform.position;
                CharacterBody target = null;
                if (toSelect != null)
                {
                    target = toSelect.ToArray()[UnityEngine.Random.RandomRangeInt(0, toSelect.Count())];
                    allAirbornePlayerBodies = allAirbornePlayerBodies.Where(b => b != target);
                    allPlayerBodies = allPlayerBodies.Where(b => b != target);
                    allEnemyBodies = allEnemyBodies.Where(b => b != target);
                }

                if (!target)
                {
                    //Pick Random position
                    float distance = UnityEngine.Random.Range(60f, 120f);
                    float ang = UnityEngine.Random.Range(0f, 360f);

                    Vector3 forward = Vector3.forward * distance;
                    position = Quaternion.AngleAxis(ang, Vector3.up) * forward;
                }

                FireProjectileInfo fpi = new FireProjectileInfo
                {
                    projectilePrefab = PluginAssets.Projectiles.AntiFlyingUltLineVanillaPrefab,
                    damage = damageStat * damageCoefficient,
                    crit = isCrit,
                    damageTypeOverride = DamageType.CrippleOnHit,
                    owner = gameObject,
                    force = 0f,
                    procChainMask = default,
                    position = position,
                    rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f)
                };
                ProjectileManager.instance.FireProjectile(fpi);
            }
        }
    }
}
