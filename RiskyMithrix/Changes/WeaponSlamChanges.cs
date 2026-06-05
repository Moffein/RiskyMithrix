using Mono.Cecil.Cil;
using MonoMod.Cil;
using RiskyMithrix.Artifact;
using RiskyMithrix.Components;
using RiskyMithrix.Modules;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RiskyMithrix.Changes
{
    internal static class WeaponSlamChanges
    {
        internal static void Init()
        {
            PluginContentPack.entityStatesTypes.Add(typeof(EntityStates.RiskyMithrix.HammerSlam.GetBestTarget));
            PluginContentPack.entityStatesTypes.Add(typeof(EntityStates.RiskyMithrix.HammerSlam.RotateToTarget));
            StopMomentum();
            RotateBeforeUse();
            FlamePillars();
            FasterAttack();
        }

        private static void FasterAttack()
        {
            if (!PluginConfig.WeaponSlam.fasterAttack.Value) return;
            On.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate += WeaponSlam_FixedUpdate;
            On.EntityStates.BrotherMonster.WeaponSlam.OnExit += WeaponSlam_OnExit;
            On.EntityStates.BrotherMonster.WeaponSlam.OnEnter += WeaponSlam_OnEnter_StoreAnimRate;
        }

        private static void WeaponSlam_OnEnter_StoreAnimRate(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnEnter orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            orig(self);
            var comp = self.GetComponent<WeaponSlamRateStorangeComponent>();
            if (!comp) comp = self.gameObject.AddComponent<WeaponSlamRateStorangeComponent>();
            if (comp)
            {
                comp.ResetRate();
                if (self.modelAnimator)
                {
                    comp.StoreOrigRate(self.modelAnimator.GetFloat("WeaponSlam.playbackRate"));
                }
            }
        }

        private static void WeaponSlam_OnExit(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnExit orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            if (self.hasDoneBlastAttack)
            {
                var comp = self.GetComponent<WeaponSlamRateStorangeComponent>();
                if (comp)
                {
                    //float rate = self.modelAnimator.GetFloat("WeaponSlam.playbackRate");
                    comp.ResetRate();
                }
            }
            orig(self);
        }

        private static void WeaponSlam_FixedUpdate(On.EntityStates.BrotherMonster.WeaponSlam.orig_FixedUpdate orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            bool didBlastAttack = self.hasDoneBlastAttack;
            orig(self);

            if (!didBlastAttack && self.hasDoneBlastAttack)
            {
                var comp = self.GetComponent<WeaponSlamRateStorangeComponent>();
                if (comp)
                {
                    float newRate = comp.GetOrigRate();
                    if (newRate > 0f)
                    {
                        float rateScalar = 2f;
                        self.modelAnimator.SetFloat("WeaponSlam.playbackRate", rateScalar * newRate);
                        float remainingTime = EntityStates.BrotherMonster.WeaponSlam.duration - self.fixedAge;  //why is duration static???
                        if (remainingTime > 0f)
                        {
                            self.fixedAge += (remainingTime / rateScalar) * 1.2f;   //There's endlag where he stands afk, +20% fixes that
                        }
                    }
                }
            }
        }

        private static void FlamePillars()
        {
            if (!PluginConfig.WeaponSlam.spawnFirePillars.Value) return;
            IL.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate += OverrideVanillaProjectiles;
        }

        private static void OverrideVanillaProjectiles(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After, x => x.MatchCall<PhaseCounter>("get_instance")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<PhaseCounter, EntityStates.BrotherMonster.WeaponSlam, PhaseCounter>>((phaseCounter, self) =>
                {

                    int phase = 1;
                    if (phaseCounter) phase = phaseCounter.phase;
                    bool challengeArtifactEnabled = BrotherChallengeArtifact.artifactDef && RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(BrotherChallengeArtifact.artifactDef);
                    if (challengeArtifactEnabled) phase++;

                    bool crit = self.RollCrit();
                    bool shouldShootForwardWave = phase > 1 || PluginConfig.WeaponSlam.phase1SunderWave.Value;
                    bool shouldShoot360Wave = phase > 1 && PluginConfig.WeaponSlam.phase2SunderWave.Value;

                    if (shouldShoot360Wave)
                    {
                        //360 Sunder Waves
                        int projectileCount = EntityStates.BrotherMonster.ExitSkyLeap.waveProjectileCount;
                        Transform transform2 = self.FindModelChild(EntityStates.BrotherMonster.WeaponSlam.muzzleString);
                        float num = 360f / (float)projectileCount;
                        Vector3 point = Vector3.ProjectOnPlane(self.characterDirection.forward, Vector3.up);
                        Vector3 position = self.characterBody.footPosition;
                        if (transform2)
                        {
                            position = transform2.position;
                        }
                        for (int i = 0; i < projectileCount; i++)
                        {
                            Vector3 forward = Quaternion.AngleAxis(num * ((float)i - (float)projectileCount / 2f), Vector3.up) * point;
                            ProjectileManager.instance.FireProjectile(EntityStates.BrotherMonster.WeaponSlam.waveProjectilePrefab, position, Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * EntityStates.BrotherMonster.WeaponSlam.waveProjectileDamageCoefficient, EntityStates.BrotherMonster.WeaponSlam.waveProjectileForce, crit, DamageColorIndex.Default, null, -1f, (DamageTypeCombo)DamageSource.Primary | DamageType.CrippleOnHit);
                        }
                    }
                    else if (shouldShootForwardWave)
                    {
                        //Vanilla Sunder Waves
                        int projectileCount = EntityStates.BrotherMonster.WeaponSlam.waveProjectileCount;
                        Transform transform2 = self.FindModelChild(EntityStates.BrotherMonster.WeaponSlam.muzzleString);
                        float num = EntityStates.BrotherMonster.WeaponSlam.waveProjectileArc / (float)projectileCount;
                        Vector3 point = Vector3.ProjectOnPlane(self.characterDirection.forward, Vector3.up);
                        Vector3 position = self.characterBody.footPosition;
                        if (transform2)
                        {
                            position = transform2.position;
                        }
                        for (int i = 0; i < projectileCount; i++)
                        {
                            Vector3 forward = Quaternion.AngleAxis(num * ((float)i - (float)projectileCount / 2f), Vector3.up) * point;
                            ProjectileManager.instance.FireProjectile(EntityStates.BrotherMonster.WeaponSlam.waveProjectilePrefab, position, Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * EntityStates.BrotherMonster.WeaponSlam.waveProjectileDamageCoefficient, EntityStates.BrotherMonster.WeaponSlam.waveProjectileForce, crit, DamageColorIndex.Default, null, -1f, (DamageTypeCombo)DamageSource.Primary | DamageType.CrippleOnHit);
                        }
                    }

                    int pillarCount = phase > 1 ? PluginConfig.WeaponSlam.firePillarsPhase2.Value : PluginConfig.WeaponSlam.firePillarsPhase1.Value;

                    Vector3 flamePos = self.characterBody.footPosition;
                    Transform muzzle = self.FindModelChild(EntityStates.BrotherMonster.WeaponSlam.muzzleString);
                    if (muzzle) flamePos = muzzle.transform.position;

                    Vector3 forwardDirection = self.transform.forward;
                    if (self.characterDirection) forwardDirection = self.characterDirection.forward;

                    float desiredMaxAngle = 120f;
                    float angleOffsetPerPillar = desiredMaxAngle / pillarCount;
                    Vector3 firingAngle = Quaternion.AngleAxis(-angleOffsetPerPillar * (int)(pillarCount / 2), Vector3.up)* forwardDirection;
                     
                    for (int i = 0; i < pillarCount; i++)
                    {
                        ProjectileManager.instance.FireProjectile(
                            PluginAssets.Projectiles.FlamePillarMovingPrefab,
                            flamePos,
                            Util.QuaternionSafeLookRotation(firingAngle),
                            self.gameObject,
                           self.damageStat * 6f,
                           0f,
                           crit,
                           DamageColorIndex.Default,
                           null,
                           -1f,
                           DamageTypeCombo.GenericPrimary);
                        firingAngle = Quaternion.AngleAxis(angleOffsetPerPillar, Vector3.up) * firingAngle;
                    }

                    if (challengeArtifactEnabled && phase > 2 && PluginConfig.Artifact.groundOrbOnSlam.Value)
                    {
                        Transform orbOrigin = self.FindModelChild("SlamZone");
                        int orbCount = 6;
                        float num = 360f / (float)orbCount;
                        Vector3 point = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
                        Vector3 footPosition = orbOrigin ? orbOrigin.position : self.characterBody.footPosition;
                        for (int i = 0; i < orbCount; i++)
                        {
                            Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * point;
                            ProjectileManager.instance.FireProjectileWithoutDamageType(PluginAssets.Projectiles.AntiFlyingUltOrbVanillaPrefab, footPosition, Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * 2f, 4000f, crit, DamageColorIndex.Default, null, -1f);
                        }
                    }

                    return null;
                });
            }
            else
            {
                Debug.LogError("RiskyMithrix: WeaponSlam FlamePillars DisableVanillaProjectiles failed.");
            }
        }

        private static void RotateBeforeUse()
        {
            if (!PluginConfig.WeaponSlam.rotateBeforeUse.Value) return;
            SkillDef weaponSlamSkill = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/WeaponSlam.asset").WaitForCompletion();
            weaponSlamSkill.activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.RiskyMithrix.HammerSlam.GetBestTarget));
            On.EntityStates.BrotherMonster.WeaponSlam.OnEnter += WeaponSlam_OnEnter_FixRotation;
        }

        private static void WeaponSlam_OnEnter_FixRotation(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnEnter orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            orig(self);
            if (self.modelTransform)
            {
                AimAnimator component = self.modelTransform.GetComponent<AimAnimator>();
                if (component)
                {
                    component.enabled = false;
                }
            }

            if (self.characterDirection)
            {
                self.characterDirection.moveVector = self.characterDirection.forward;
            }
        }

        private static void StopMomentum()
        {
            if (!PluginConfig.WeaponSlam.stopOnUse.Value) return;
            On.EntityStates.BrotherMonster.WeaponSlam.OnEnter += WeaponSlam_OnEnter;
        }

        private static void WeaponSlam_OnEnter(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnEnter orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            orig(self);
            if (self.isAuthority && self.characterMotor)
            {
                self.characterMotor.velocity = new UnityEngine.Vector3(0f, self.characterMotor.velocity.y, 0f);
            }
        }
    }
}
