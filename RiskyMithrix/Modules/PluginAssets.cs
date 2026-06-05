using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RiskyMithrix.Modules
{
    public static class PluginAssets
    {
        public static AssetBundle mainAssetBundle;
        public static GameObject BrotherHurtMasterObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtMaster.prefab").WaitForCompletion();
        public static GameObject BrotherMasterObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherMaster.prefab").WaitForCompletion();
        public static GameObject ITBrotherMasterObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ITBrotherMaster.prefab").WaitForCompletion();
        public static GameObject BrotherBodyObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherBody.prefab").WaitForCompletion();
        public static GameObject ITBrotherBodyObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/ITBrotherBody.prefab").WaitForCompletion();
        public static GameObject BrotherHurtBodyObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtBody.prefab").WaitForCompletion();

        public static BodyIndex BrotherBodyIndex, ITBrotherBodyIndex, BrotherHurtBodyIndex;

        public static class Projectiles
        {
            public static GameObject SprintBashProjectilePrefab;
            public static GameObject LunarOrbProjectilePrefab;
            public static GameObject FirePillarVanillaPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherFirePillar.prefab").WaitForCompletion();
            public static GameObject UltLineRightPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateRight.prefab").WaitForCompletion();
            public static GameObject AntiFlyingUltLineVanillaPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BrotherHaunt/BrotherUltLineProjectileStatic.prefab").WaitForCompletion();
            public static GameObject AntiFlyingUltOrbVanillaPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSunderWave, Energized.prefab").WaitForCompletion();
            public static GameObject FlamePillarMovingPrefab;
        }

        public static class SkillDefs
        {
            public static SkillDef SprintLunarShardVanilla = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/FireLunarShards.asset").WaitForCompletion();
            public static SkillDef FireLunarShardsHurt = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/FireLunarShardsHurt.asset").WaitForCompletion();
            public static SkillDef FireLunarOrb;
        }

        public static class Effects
        {
            public static GameObject SkyLeapPredictionEffect;
        }

        public static class ModdedDamageTypes
        {
            public static DamageAPI.ModdedDamageType SprintBashShards;
        }
        internal static void Init()
        {
            if (!mainAssetBundle)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RiskyMithrix.riskymithrixbundle"))
                {
                    mainAssetBundle = AssetBundle.LoadFromStream(stream);
                }
            }
            RoR2Application.onLoad += OnLoad;
            CreateAssets();
        }

        private static void OnLoad()
        {
            BrotherBodyIndex = BodyCatalog.FindBodyIndex("BrotherBody");
            ITBrotherBodyIndex = BodyCatalog.FindBodyIndex("ITBrotherBody");
            BrotherHurtBodyIndex = BodyCatalog.FindBodyIndex("BrotherHurtBody");
        }

        private static void CreateAssets()
        {
            SetupSprintBashDamageType();
            SetupSprintBashProjectile();
            SetupLunarOrb();
            SetupSkyLeapPredictionEffect();
            SetupFlamePillarMoving();
        }

        private static void SetupSprintBashDamageType()
        {
            ModdedDamageTypes.SprintBashShards = DamageAPI.ReserveDamageType();
            On.RoR2.HealthComponent.TakeDamageProcess += TakeDamageProcess_PreventSprintBashMultihit;
        }

        private static void TakeDamageProcess_PreventSprintBashMultihit(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.HasModdedDamageType(ModdedDamageTypes.SprintBashShards))
            {
                if (self.body)
                {
                    //too lazy to make a new buff, just reuse the Voidsent Flame buff to check this
                    if (self.body.HasBuff(DLC1Content.Buffs.ExplodeOnDeathVoidHiddenCooldown))
                    {
                        damageInfo.rejected = true;
                    }
                    else
                    {
                        damageInfo.RemoveModdedDamageType(ModdedDamageTypes.SprintBashShards);
                        self.body.AddTimedBuff(DLC1Content.Buffs.ExplodeOnDeathVoidHiddenCooldown, 0.5f);
                    }
                }
            }
            orig(self, damageInfo);
        }

        private static void SetupFlamePillarMoving()
        {
            GameObject projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/FalseSonBoss/FalseSonFissurePillar.prefab").WaitForCompletion()
                .InstantiateClone("RiskyMithrixFlamePillarMoving", true);
            ProjectileController pc = projectilePrefab.GetComponent<ProjectileController>();
            pc.ghostPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherFirePillarGhost.prefab").WaitForCompletion();
            ProjectileDotZone pdz = projectilePrefab.GetComponent<ProjectileDotZone>();
            pdz.damageCoefficient = 0.5f;
            pdz.fireFrequency = 10;
            pdz.resetFrequency = 3;
            pdz.overlapProcCoefficient = 0.2f;
            PluginContentPack.projectilePrefabs.Add(projectilePrefab);
            Projectiles.FlamePillarMovingPrefab = projectilePrefab;
        }

        private static void SetupSkyLeapPredictionEffect()
        {
            /*GameObject effect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Meteor/MeteorStrikePredictionEffect.prefab").WaitForCompletion()
                .InstantiateClone("RiskyMithrixSkyLeapPredictionEffect", false);
            effect.GetComponent<DestroyOnTimer>().duration = 3f;
            effect.GetComponentInChildren<ObjectScaleCurve>().timeMax = 3f;
            PluginContentPack.effectDefs.Add(new EffectDef(effect));
            Effects.SkyLeapPredictionEffect = effect;*/
            Effects.SkyLeapPredictionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemSpawnEffect.prefab").WaitForCompletion();
        }

        private static void SetupLunarOrb()
        {
            GameObject projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion()
                .InstantiateClone("RiskyMithrixLunarOrbProjectile", true);
            ProjectileImpactExplosion pie = projectilePrefab.GetComponent<ProjectileImpactExplosion>();
            pie.falloffModel = BlastAttack.FalloffModel.None;

            ProjectileSimple ps = projectilePrefab.GetComponent<ProjectileSimple>();
            ps.desiredForwardSpeed = 90f;   //vanilla 60f
            PluginContentPack.projectilePrefabs.Add(projectilePrefab);
            Projectiles.LunarOrbProjectilePrefab = projectilePrefab;

            //projectilePrefab.layer = LayerIndex.projectileWorldOnly.intVal;

            Transform model = projectilePrefab.transform.Find("Model");
            UnityEngine.Object.Destroy(model);

            Transform detonator  = projectilePrefab.transform.Find("Model");

            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<ModelLocator>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<AssignTeamFilterToTeamComponent>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<HealthComponent>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<CharacterBody>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<SkillLocator>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<TeamComponent>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<DisableCollisionsBetweenColliders>());
        }

        private static void SetupSprintBashProjectile()
        {
            GameObject projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/LunarShardProjectile.prefab").WaitForCompletion()
                .InstantiateClone("RiskyMithrixLunarShardNoTrackingProjectile", true);
            projectilePrefab.GetComponent<ProjectileController>().isPrediction = true;

            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<ProjectileSteerTowardTarget>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<ProjectileTargetComponent>());

            PluginContentPack.projectilePrefabs.Add(projectilePrefab);
            Projectiles.SprintBashProjectilePrefab = projectilePrefab;
        }
    }
}
