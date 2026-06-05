using RoR2.Skills;
using RiskyMithrix.Modules;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using RoR2.Projectile;
using UnityEngine.UIElements;

namespace RiskyMithrix.Artifact
{
    public class BrotherChallengeArtifact
    {
        public static ArtifactDef artifactDef;

        internal static void Init()
        {
            if (!PluginConfig.Artifact.enabled.Value) return;
            CreateArtifactDef();
            ExtraArmor();
            SlideDebuffCleanse();
            FasterLeap();
            PizzaAfterLeap();

            if (PluginConfig.Artifact.moreSlide.Value || PluginConfig.Artifact.moreMelee.Value || PluginConfig.Artifact.extraSpeed.Value)
            {
                RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            }

            if (PluginConfig.Artifact.forceEnableInEclipse.Value)
            {
                Run.onRunStartGlobal += Run_onRunStartGlobal;
            }
        }

        private static void Run_onRunStartGlobal(Run obj)
        {
            /*Debug.Log("Dumping gamemodes");
            foreach (var str in GameModeCatalog.nameToIndex)
            {
                Debug.Log(str);
            }*/
            if (NetworkServer.active && RunArtifactManager.instance && Run.instance.gameModeIndex == GameModeCatalog.FindGameModeIndex("EclipseRun"))
            {
                RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, true);
            }
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            bool isMithrix = sender.bodyIndex == RoR2Content.BodyPrefabs.BrotherBody.bodyIndex || sender.bodyIndex == RoR2Content.BodyPrefabs.ITBrotherBody.bodyIndex;
            if (isMithrix && RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(artifactDef))
            {
                if (PluginConfig.Artifact.moreMelee.Value)
                {
                    //args.primarySkill.bonusStockAdd += 1; //doesnt work
                    args.primarySkill.cooldownMultiplier *= 0.5f;
                    args.secondarySkill.bonusStockAdd += 1;
                    args.secondarySkill.cooldownMultiplier *= 0.75f;
                }

                if (PluginConfig.Artifact.moreSlide.Value)
                {
                    args.utilitySkill.bonusStockAdd += 1;
                    args.utilitySkill.cooldownMultiplier *= 2f / 3f;
                }

                if (PluginConfig.Artifact.extraSpeed.Value)
                {
                    args.moveSpeedMultAdd += 0.25f;
                }
            }
        }

        private static void CreateArtifactDef()
        {
            if (artifactDef) return;

            artifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
            artifactDef.nameToken = "RISKYMITHRIX_BROTHERCHALLENGEARTIFACT_NAME";
            artifactDef.descriptionToken = "RISKYMITHRIX_BROTHERCHALLENGEARTIFACT_DESCRIPTION";
            artifactDef.smallIconSelectedSprite = PluginAssets.mainAssetBundle.LoadAsset<Sprite>("texArtifactBrotherChallengeEnabled");
            artifactDef.smallIconDeselectedSprite = PluginAssets.mainAssetBundle.LoadAsset<Sprite>("texArtifactBrotherChallengeDisabled");

            PluginContentPack.artifactDefs.Add(artifactDef);

            LanguageAPI.Add("RISKYMITHRIX_BROTHERCHALLENGEARTIFACT_NAME", "Artifact of Hatred");
            LanguageAPI.Add("RISKYMITHRIX_BROTHERCHALLENGEARTIFACT_DESCRIPTION", "<style=cIsHealth>MY HATRED FOR YOU VERMIN CANNOT BE DESCRIBED.</style>");

            //TODO: YOUR LANGUAGE HERE
            //LanguageAPI.Add("RISKYMITHRIX_BROTHERCHALLENGEARTIFACT_NAME", "Translated Artifact Name", "languageCode");
            //LanguageAPI.Add("RISKYMITHRIX_BROTHERCHALLENGEARTIFACT_DESCRIPTION", "ranslated Artifact Description", "languageCode");
        }

        #region pizza after leap
        private static void PizzaAfterLeap()
        {
            if (!PluginConfig.Artifact.pizzaOnLeapP1.Value && !PluginConfig.Artifact.pizzaOnLeapP2.Value) return;
            On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter += ExitSkyLeap_OnEnter1;
        }

        private static void ExitSkyLeap_OnEnter1(On.EntityStates.BrotherMonster.ExitSkyLeap.orig_OnEnter orig, EntityStates.BrotherMonster.ExitSkyLeap self)
        {
            orig(self);
            if (self.isAuthority && RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(artifactDef))
            {
                //No need to modify phase like in other parts of the code
                int phase = 1;
                if (PhaseCounter.instance) phase = PhaseCounter.instance.phase;

                bool shouldSpawnPhase1 = PluginConfig.Artifact.pizzaOnLeapP1.Value;
                bool shouldSpawnPhase2 = phase > 1 && PluginConfig.Artifact.pizzaOnLeapP2.Value;

                if (shouldSpawnPhase1 || shouldSpawnPhase2)
                {
                    bool isCrit = self.RollCrit();
                    int pillarCount = 9;
                    for (int i = 0; i < pillarCount; i++)
                    {
                        float angleOffset = i * 360f / pillarCount;

                        FireProjectileInfo fpi = new FireProjectileInfo
                        {
                            projectilePrefab = PluginAssets.Projectiles.UltLineRightPrefab,
                            damage = self.damageStat * 6f,
                            crit = isCrit,
                            damageTypeOverride = DamageType.CrippleOnHit,
                            owner = self.gameObject,
                            force = 0f,
                            procChainMask = default,
                            position = self.characterBody ? self.characterBody.footPosition : self.transform.position,
                            rotation = Quaternion.Euler(0f, angleOffset, 0f)
                        };
                        ProjectileManager.instance.FireProjectile(fpi);
                    }
                }
            }
        }
        #endregion

        #region faster leap
        private static void FasterLeap()
        {
            if (!PluginConfig.Artifact.fasterLeap.Value) return;
            On.EntityStates.BrotherMonster.HoldSkyLeap.FixedUpdate += HoldSkyLeap_FixedUpdate;
            On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter += ExitSkyLeap_OnEnter;
        }

        private static void ExitSkyLeap_OnEnter(On.EntityStates.BrotherMonster.ExitSkyLeap.orig_OnEnter orig, EntityStates.BrotherMonster.ExitSkyLeap self)
        {
            float origDuration = EntityStates.BrotherMonster.ExitSkyLeap.baseDuration;
            bool artifactEnabled = RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(artifactDef);
            if (artifactEnabled)
            {
                EntityStates.BrotherMonster.ExitSkyLeap.baseDuration *= 0.5f;
            }
            orig(self);
            if (artifactEnabled)
            {
                EntityStates.BrotherMonster.ExitSkyLeap.baseDuration = origDuration;
            }
        }

        private static void HoldSkyLeap_FixedUpdate(On.EntityStates.BrotherMonster.HoldSkyLeap.orig_FixedUpdate orig, EntityStates.BrotherMonster.HoldSkyLeap self)
        {
            orig(self);
            bool artifactEnabled = RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(artifactDef);
            if (artifactEnabled && self.isAuthority && self.fixedAge >= 1.5f)
            {
                self.fixedAge += EntityStates.BrotherMonster.HoldSkyLeap.duration;
            }
        }
        #endregion

        #region slide cleanse
        private static bool slideCleanseHookAdded = false;
        private static void SlideDebuffCleanse()
        {
            if (!PluginConfig.Artifact.slideCleanse.Value) return;
            On.RoR2.RunArtifactManager.SetArtifactEnabledServer += SetSlideCleanse;
        }

        private static void SetSlideCleanse(On.RoR2.RunArtifactManager.orig_SetArtifactEnabledServer orig, RunArtifactManager self, ArtifactDef artifactDef, bool newEnabled)
        {
            orig(self, artifactDef, newEnabled);
            if (artifactDef == BrotherChallengeArtifact.artifactDef) SetSlideCleanseHook(newEnabled);
        }

        private static void SetSlideCleanseHook(bool enabled)
        {
            if (enabled == slideCleanseHookAdded) return;

            SkillDef slide = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/Slide.asset").WaitForCompletion();
            if (enabled)
            {
                slideCleanseHookAdded = true;
                On.EntityStates.BrotherMonster.BaseSlideState.OnEnter += CleanseDebuffsOnSlide;
            }
            else
            {
                slideCleanseHookAdded = false;
                On.EntityStates.BrotherMonster.BaseSlideState.OnEnter -= CleanseDebuffsOnSlide;
            }
        }

        private static void CleanseDebuffsOnSlide(On.EntityStates.BrotherMonster.BaseSlideState.orig_OnEnter orig, EntityStates.BrotherMonster.BaseSlideState self)
        {
            orig(self);
            if (NetworkServer.active && self.characterBody)
            {
                CleanseSystem.CleanseBodyServer(self.characterBody, true, false, false, true, true, false);
            }
        }
        #endregion

        #region extra armor
        private static void ExtraArmor()
        {
            if (!PluginConfig.Artifact.extraArmor.Value) return;
            On.RoR2.RunArtifactManager.SetArtifactEnabledServer += SetExtraArmor;
        }

        private static void SetExtraArmor(On.RoR2.RunArtifactManager.orig_SetArtifactEnabledServer orig, RunArtifactManager self, ArtifactDef artifactDef, bool newEnabled)
        {
            orig(self, artifactDef, newEnabled);
            if (artifactDef == BrotherChallengeArtifact.artifactDef) SetExtraArmorHook(newEnabled);
        }

        private static bool extraArmorHookAdded = false;
        private static void SetExtraArmorHook(bool enabled)
        {
            if (enabled == extraArmorHookAdded) return;

            if (enabled)
            {
                extraArmorHookAdded = true;
                On.RoR2.CharacterBody.Start += ExtraAdaptiveArmor;
            }
            else
            {
                extraArmorHookAdded = false;
                On.RoR2.CharacterBody.Start -= ExtraAdaptiveArmor;
            }
        }

        private static void ExtraAdaptiveArmor(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active && self.inventory && (self.bodyIndex == RoR2Content.BodyPrefabs.BrotherBody.bodyIndex || self.bodyIndex == RoR2Content.BodyPrefabs.ITBrotherBody.bodyIndex))
            {
                int adaptiveArmorCount = self.inventory.GetItemCountPermanent(RoR2Content.Items.AdaptiveArmor);
                if (adaptiveArmorCount < 2)
                {
                    self.inventory.GiveItemPermanent(RoR2Content.Items.AdaptiveArmor, 2 - adaptiveArmorCount);
                }
            }
        }
        #endregion
    }
}
