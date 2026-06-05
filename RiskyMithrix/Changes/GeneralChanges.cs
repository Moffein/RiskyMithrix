using RiskyMithrix.Modules;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RiskyMithrix.Changes
{
    internal static class GeneralChanges
    {
        internal static void Init()
        {
            ModifyStats();
            DebuffResistance();
            AddFreezeResist();
        }
        
        private static void AddFreezeResist()
        {
            if (PluginConfig.General.freezeResist.Value <= 0f || PluginConfig.General.freezeResist.Value >= 1f) return;
            On.RoR2.SetStateOnHurt.SetFrozen += SetStateOnHurt_SetFrozen;
        }

        private static void SetStateOnHurt_SetFrozen(On.RoR2.SetStateOnHurt.orig_SetFrozen orig, SetStateOnHurt self, float duration)
        {
            if (self.targetStateMachine && self.targetStateMachine.commonComponents.characterBody)
            {
                BodyIndex index = self.targetStateMachine.commonComponents.characterBody.bodyIndex;
                if (index == BodyCatalog.FindBodyIndex("BrotherBody") || index == BodyCatalog.FindBodyIndex("ITBrotherBody"))
                {
                    duration *= PluginConfig.General.freezeResist.Value;
                }
            }
            orig(self, duration);
        }

        private static void FrozenState_OnEnter(On.EntityStates.FrozenState.orig_OnEnter orig, EntityStates.FrozenState self)
        {
            orig(self);
        }

        private static void ModifyStats()
        {
            void ChangeStats(GameObject bodyObject)
            {
                CharacterBody body = bodyObject.GetComponent<CharacterBody>();
                if (body != null)
                {
                    if (PluginConfig.General.statChanges.Value && body.baseMaxHealth < 1400f)
                    {
                        body.baseMaxHealth = 1400f;
                        body.levelMaxHealth = 420f;
                    }

                    if (PluginConfig.General.fallImmunity.Value)
                    {
                        body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                    }

                    if (PluginConfig.General.freezeResist.Value <= 0f)
                    {
                        SetStateOnHurt ssoh = bodyObject.GetComponent<SetStateOnHurt>();
                        if (ssoh)
                        {
                            ssoh.canBeFrozen = false;
                        }
                    }
                }
            }
            ChangeStats(PluginAssets.BrotherBodyObject);
            ChangeStats(PluginAssets.ITBrotherBodyObject);

            if (PluginConfig.General.prioritizePlayers.Value)
            {
                void SetPrioritizePlayers(GameObject masterObject)
                {
                    BaseAI ai = masterObject.GetComponent<BaseAI>();
                    if (ai)
                    {
                        ai.prioritizePlayers = true;
                    }
                }
                SetPrioritizePlayers(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherMaster.prefab").WaitForCompletion());
                SetPrioritizePlayers(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ITBrotherMaster.prefab").WaitForCompletion());
            }
        }

        private static void DebuffResistance()
        {
            if (!PluginConfig.General.debuffResist.Value) return;

            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self.bodyIndex == PluginAssets.BrotherBodyIndex || self.bodyIndex == PluginAssets.BrotherHurtBodyIndex)
            {
                float desiredMoveSpeed = self.baseMoveSpeed * (self.isSprinting ? self.sprintingSpeedMultiplier : 1f);
                if (self.moveSpeed > 0f && self.moveSpeed < desiredMoveSpeed)
                {
                    self.moveSpeed = Mathf.Lerp(self.moveSpeed, desiredMoveSpeed, 0.75f);
                }

                if (self.attackSpeed < 1f && self.attackSpeed > 0f)
                {
                    self.attackSpeed = Mathf.Lerp(self.attackSpeed, 1f, 0.75f);
                }
            }
        }
    }
}
