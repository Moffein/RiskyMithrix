using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RiskyMithrix.Modules
{
    public class PluginContentPack : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => "com.RiskyLives.RiskyMithrix";

        public static List<SkillDef> skillDefs = new List<SkillDef>();
        public static List<GameObject> projectilePrefabs = new List<GameObject>();
        public static List<Type> entityStatesTypes = new List<Type>();
        public static List<EffectDef> effectDefs = new List<EffectDef>();
        public static List<ArtifactDef> artifactDefs = new List<ArtifactDef>();

        internal void Init()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            contentPack.projectilePrefabs.Add(projectilePrefabs.ToArray());
            contentPack.entityStateTypes.Add(entityStatesTypes.ToArray());
            contentPack.skillDefs.Add(skillDefs.ToArray());
            contentPack.effectDefs.Add(effectDefs.ToArray());
            contentPack.artifactDefs.Add(artifactDefs.ToArray());

            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
