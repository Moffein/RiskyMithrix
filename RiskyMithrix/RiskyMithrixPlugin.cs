using BepInEx;
using R2API.Utils;
using RiskyMithrix.Artifact;
using RiskyMithrix.Changes;
using RiskyMithrix.Modules;
using RoR2;
using System;
using System.Security;
using System.Security.Permissions;
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace RiskyMithrix
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInPlugin("com.RiskyLives.RiskyMithrix", "RiskyMithrix", "1.3.8")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class RiskyMithrix : BaseUnityPlugin
    {
        private void Awake()
        {
            PluginConfig.ReadConfig(Config);
            new PluginContentPack().Init();
            PluginAssets.Init();
            GeneralChanges.Init();
            SprintBashChanges.Init();
            LunarShardChanges.Init();
            SkyLeapChanges.Init();

            //Scuffed atm
            //AntiFlyingAttack.Init();
            WeaponSlamChanges.Init();

            BrotherChallengeArtifact.Init();

            Interactor i = PluginAssets.BrotherBodyObject.GetComponent<Interactor>();
            if (i)
            {
                i.maxInteractionDistance = 10f;
            }
        }
    }
}
