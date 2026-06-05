using R2API;
using RiskyMithrix.Artifact;
using RiskyMithrix.Modules;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RiskyMithrix.Changes
{
    internal static class SprintBashChanges
    {
        internal static void Init()
        {
            AddFireProjectiles();
            AntiTrimp();
            FasterAttack();
        }
        
        private static void FasterAttack()
        {
            if (!PluginConfig.SprintBash.fasterAttack.Value) return;
            PluginUtils.SetAddressableEntityStateField("RoR2/Base/Brother/EntityStates.BrotherMonster.SprintBash.asset", "baseDuration", "2");  //Vanilla is 4
        }

        private static void AntiTrimp()
        {
            if (!PluginConfig.SprintBash.antiTrimp.Value) return;
            On.EntityStates.BrotherMonster.SprintBash.FixedUpdate += SprintBash_FixedUpdate;
        }

        private static void SprintBash_FixedUpdate(On.EntityStates.BrotherMonster.SprintBash.orig_FixedUpdate orig, EntityStates.BrotherMonster.SprintBash self)
        {
            orig(self);
            if (self.isAuthority && self.characterMotor && self.characterMotor.velocity.y > 0) self.characterMotor.velocity.y = 0f;
        }

        private static void AddFireProjectiles()
        {
            if (!PluginConfig.SprintBash.fireProjectilesPhase2.Value && !PluginConfig.SprintBash.fireProjectilesPhase1.Value) return;
            On.EntityStates.BrotherMonster.SprintBash.OnEnter += SprintBash_OnEnter;
        }

        private static void SprintBash_OnEnter(On.EntityStates.BrotherMonster.SprintBash.orig_OnEnter orig, EntityStates.BrotherMonster.SprintBash self)
        {
            orig(self);
            if (!self.isAuthority) return;

            bool phase1Gate = PluginConfig.SprintBash.fireProjectilesPhase1.Value;
            bool phase2Gate = PluginConfig.SprintBash.fireProjectilesPhase2.Value && PhaseCounter.instance && PhaseCounter.instance.phase > 1;
            if (!phase1Gate && !phase2Gate) return;

            Ray aimRay = self.GetAimRay();

            Vector3 rhs = Vector3.Cross(Vector3.up, aimRay.direction);
            Vector3 axis = Vector3.Cross(aimRay.direction, rhs);

            bool challengeArtifactEnabled = BrotherChallengeArtifact.artifactDef && RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(BrotherChallengeArtifact.artifactDef);
            int projectileCount = 5;
            if (challengeArtifactEnabled && PluginConfig.Artifact.moreSprintBashShards.Value) projectileCount += 4;
            int centerIndex = projectileCount / 2;

            float currentSpread = 0f;
            float angle = 0f;
            float num2 = 0f;
            num2 = UnityEngine.Random.Range(1f + currentSpread, 1f + currentSpread) * projectileCount * 1.25f;
            angle = num2 / (projectileCount - 1f);

            Vector3 direction = Quaternion.AngleAxis(-num2 * 0.5f, axis) * aimRay.direction;
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            Ray aimRay2 = new Ray(aimRay.origin, direction);
            for (int i = 0; i < projectileCount; i++)
            {
                DamageTypeCombo dtc = (DamageTypeCombo)DamageSource.Secondary | DamageType.CrippleOnHit;
                dtc.AddModdedDamageType(PluginAssets.ModdedDamageTypes.SprintBashShards);   //Makes only 1 shotgun shot count as a hit

                ProjectileManager.instance.FireProjectile(PluginAssets.Projectiles.SprintBashProjectilePrefab,
                    aimRay2.origin,
                    Util.QuaternionSafeLookRotation(aimRay2.direction),
                    self.gameObject,
                    self.damageStat * 0.1f, //Vanilla shards are 0.05f
                    0f,
                    self.isCritAuthority,
                    DamageColorIndex.Default,
                    null,
                    -1f,
                    dtc);
                aimRay2.direction = rotation * aimRay2.direction;
            }
        }
    }
}
