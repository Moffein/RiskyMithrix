using RiskyMithrix.Modules;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RiskyMithrix.Changes
{
    internal static class LunarShardChanges
    {
        internal static void Init()
        {
            ReplaceLunarShard();
        }

        private static void ReplaceLunarShard()
        {
            SkillDef skill = ScriptableObject.CreateInstance<SkillDef>();
            skill.skillName = "SprintShootOrbs";
            (skill as ScriptableObject).name = skill.skillName;
            skill.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/FireLunarShards.asset").WaitForCompletion().icon;
            skill.activationStateMachineName = "Weapon";
            skill.activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.RiskyMithrix.Weapon.FireLunarOrb));
            skill.interruptPriority = EntityStates.InterruptPriority.Any;
            skill.baseRechargeInterval = 5f;
            skill.baseMaxStock = 5;
            skill.rechargeStock = 4;
            skill.requiredStock = 1;
            skill.stockToConsume = 1;
            skill.resetCooldownTimerOnUse = false;
            skill.fullRestockOnAssign = true;
            skill.dontAllowPastMaxStocks = false;
            skill.beginSkillCooldownOnSkillEnd = false;
            skill.cancelSprintingOnActivation = false;
            skill.forceSprintDuringState = false;
            skill.canceledFromSprinting = false;
            skill.isCombatSkill = true;
            skill.mustKeyPress = false;
            skill.keywordTokens = new string[0];
            PluginContentPack.skillDefs.Add(skill);
            PluginContentPack.entityStatesTypes.Add(typeof(EntityStates.RiskyMithrix.Weapon.FireLunarOrb));
            PluginAssets.SkillDefs.FireLunarOrb = skill;
            
            void ReplaceSkillOverride(GameObject bodyObject)
            {
                SkillLocator skillLocator = bodyObject.GetComponent<SkillLocator>();
                ConditionalSkillOverride cso = bodyObject.GetComponent<ConditionalSkillOverride>();

                //Fully rebuild this to prevent a nullref
                List<ConditionalSkillOverride.ConditionalSkillInfo> newOverrides = new List<ConditionalSkillOverride.ConditionalSkillInfo>();
                foreach (ConditionalSkillOverride.ConditionalSkillInfo csi in cso.conditionalSkillInfos)
                {
                    var newCsi = new ConditionalSkillOverride.ConditionalSkillInfo
                    {
                        skillSlot = csi.skillSlot,
                        airborneSkillDef = csi.airborneSkillDef == PluginAssets.SkillDefs.SprintLunarShardVanilla ? PluginAssets.SkillDefs.FireLunarOrb : csi.airborneSkillDef,
                        sprintSkillDef = csi.sprintSkillDef == PluginAssets.SkillDefs.SprintLunarShardVanilla ? PluginAssets.SkillDefs.FireLunarOrb : csi.sprintSkillDef,
                    };
                    newOverrides.Add(newCsi);
                }
                cso.conditionalSkillInfos = newOverrides.ToArray();
            }

            void ModifyMaster(GameObject masterObject)
            {
                AISkillDriver[] drivers = masterObject.GetComponents<AISkillDriver>();
                foreach (var driver in drivers)
                {
                    if (driver.requiredSkill == PluginAssets.SkillDefs.SprintLunarShardVanilla || driver.requiredSkill == PluginAssets.SkillDefs.FireLunarShardsHurt) driver.requiredSkill = PluginAssets.SkillDefs.FireLunarOrb;
                }
            }

            if (PluginConfig.LunarShard.replaceLunarShard.Value)
            {
                ReplaceSkillOverride(PluginAssets.BrotherBodyObject);
                ReplaceSkillOverride(PluginAssets.ITBrotherBodyObject);
                ModifyMaster(PluginAssets.BrotherMasterObject);
                ModifyMaster(PluginAssets.ITBrotherMasterObject);
            }

            if (PluginConfig.LunarShard.replaceLunarShardPhase4.Value)
            {
                SkillFamily family = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Brother/BrotherHurtPrimaryFamily.asset").WaitForCompletion();
                family.variants[0].skillDef = PluginAssets.SkillDefs.FireLunarOrb;
                ModifyMaster(PluginAssets.BrotherHurtMasterObject);
            }
        }
    }
}
