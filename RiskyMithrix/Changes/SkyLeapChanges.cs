using RiskyMithrix.Artifact;
using RiskyMithrix.Components;
using RiskyMithrix.Modules;
using RoR2;
using RoR2.Navigation;
using RoR2.Projectile;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RiskyMithrix.Changes
{
    internal static class SkyLeapChanges
    {
        internal static void Init()
        {
            TargetPlayers();
            CreatePillar();
        }

        private static void CreatePillar()
        {
            On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter += ExitSkyLeap_OnEnter;
        }

        private static void ExitSkyLeap_OnEnter(On.EntityStates.BrotherMonster.ExitSkyLeap.orig_OnEnter orig, EntityStates.BrotherMonster.ExitSkyLeap self)
        {
            orig(self);
            if (!self.isAuthority) return;

            Vector3 position = self.transform.position;
            if (self.characterBody) position = self.characterBody.footPosition;

            int phase = 1;
            if (PhaseCounter.instance) phase = PhaseCounter.instance.phase;
            bool challengeArtifactEnabled = BrotherChallengeArtifact.artifactDef && RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(BrotherChallengeArtifact.artifactDef);
            if (challengeArtifactEnabled) phase++;

            int pillarCount = (phase > 1) ? PluginConfig.SkyLeap.firePillarsPhase2.Value : PluginConfig.SkyLeap.firePillarsPhase1.Value;
            if (pillarCount <= 0) return;

            bool isCrit = self.RollCrit();
            //Stationary Center Pillar
            ProjectileManager.instance.FireProjectileWithoutDamageType(PluginAssets.Projectiles.FirePillarVanillaPrefab,
                position,
                Quaternion.identity,
                self.gameObject,
                self.damageStat * 6f,
                0f,
                isCrit, DamageColorIndex.Default, null, -1f);
            pillarCount--;

            float anglePerPillar = 360f / pillarCount;
            Vector3 forwardDirection = self.transform.forward;
            if (self.characterDirection) forwardDirection = self.characterDirection.forward;
            for (int i = 0; i < pillarCount; i++)
            {
                ProjectileManager.instance.FireProjectile(
                            PluginAssets.Projectiles.FlamePillarMovingPrefab,
                            position,
                            Util.QuaternionSafeLookRotation(forwardDirection),
                            self.gameObject,
                           self.damageStat * 6f,
                           0f,
                           isCrit,
                           DamageColorIndex.Default,
                           null,
                           -1f,
                           DamageTypeCombo.GenericPrimary);
                forwardDirection = Quaternion.AngleAxis(anglePerPillar, Vector3.up) * forwardDirection;
            }

            if (challengeArtifactEnabled && PluginConfig.Artifact.groundOrbOnLeap.Value) // && phase > 2
            {
                int orbCount = 6;
                float num = 360f / (float)orbCount;
                Vector3 point = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
                Vector3 footPosition = self.characterBody.footPosition;
                for (int i = 0; i < orbCount; i++)
                {
                    Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * point;
                    ProjectileManager.instance.FireProjectileWithoutDamageType(PluginAssets.Projectiles.AntiFlyingUltOrbVanillaPrefab, footPosition, Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * 4f, 4000f, isCrit, DamageColorIndex.Default, null, -1f);
                }
            }
        }

        private static void TargetPlayers()
        {
            if (!PluginConfig.SkyLeap.directTargetPlayer.Value) return;

            On.EntityStates.BrotherMonster.HoldSkyLeap.OnEnter += HoldSkyLeap_OnEnter;
            On.EntityStates.BrotherMonster.HoldSkyLeap.OnExit += HoldSkyLeap_OnExit;
        }

        private static void HoldSkyLeap_OnExit(On.EntityStates.BrotherMonster.HoldSkyLeap.orig_OnExit orig, EntityStates.BrotherMonster.HoldSkyLeap self)
        {
            if (!self.outer.destroying && self.gameObject)
            {
                var vfx = self.gameObject.GetComponent<SkyLeapVFXComponent>();
                if (vfx) UnityEngine.Object.Destroy(vfx);
            }
            orig(self);
        }

        private static void HoldSkyLeap_OnEnter(On.EntityStates.BrotherMonster.HoldSkyLeap.orig_OnEnter orig, EntityStates.BrotherMonster.HoldSkyLeap self)
        {
            orig(self);
            if (!self.isAuthority) return;

            Vector3 offset = Vector3.zero;
            if (self.characterBody)
            {
                offset = self.transform.position - self.characterBody.footPosition;
            }

            TeamIndex myTeam = self.GetTeam();
            CharacterBody[] selectionSet = null;
            var allEnemyBodies = CharacterBody.instancesList.Where(body =>
            {
                TeamIndex targetTeam = TeamIndex.None;
                if (body.teamComponent) targetTeam = body.teamComponent.teamIndex;
                return targetTeam != myTeam && body.healthComponent && body.healthComponent.alive && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && body.master != null;
            }).ToArray();
            if (allEnemyBodies.Length > 0)
            {
                selectionSet = allEnemyBodies;
                var allPlayers = allEnemyBodies.Where(body => body.isPlayerControlled && !body.IsDrone).ToArray();
                if (allPlayers.Length > 0)
                {
                    selectionSet = allPlayers;
                }
            }

            if (selectionSet != null && selectionSet.Length > 0)
            {
                CharacterBody targetBody = selectionSet[UnityEngine.Random.RandomRangeInt(0, selectionSet.Length)];

                //Find closest node to land on
                RaycastHit raycastHit;
                if (Physics.Raycast(targetBody.footPosition, Vector3.down, out raycastHit, 200f, LayerIndex.world.mask))
                {
                    Vector3 hit = raycastHit.point;
                    if (raycastHit.collider != null)
                    {
                        NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                        var node = groundNodes.FindClosestNodeWithFlagConditions(hit, HullClassification.Golem, NodeFlags.None, NodeFlags.None, false);

                        if (node != null)
                        {
                            groundNodes.GetNodePosition(node, out var destination);
                            if (self.characterMotor && self.characterMotor.Motor && self.characterBody)
                            {
                                Vector3 finalPosition = destination + offset;
                                self.characterMotor.velocity = Vector3.zero;
                                self.characterMotor.Motor.SetPosition(finalPosition);
                            }
                        }
                    }
                }
            }

            //Jank, repeat the thing so that 
            Vector3 effectPos = self.transform.position;
            if (self.characterBody) effectPos = self.characterBody.footPosition;
            var vfxComp = self.gameObject.GetComponent<SkyLeapVFXComponent>();
            if (!vfxComp) vfxComp = self.gameObject.AddComponent<SkyLeapVFXComponent>();
            vfxComp.offset = offset;
            vfxComp.SpawnVFX();
        }
    }
}
