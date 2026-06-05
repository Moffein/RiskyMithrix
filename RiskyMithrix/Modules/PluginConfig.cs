using BepInEx.Configuration;

namespace RiskyMithrix.Modules
{
    public static class PluginConfig
    {
        internal const string catGeneral = "General";
        internal const string catSprintBash = "Sprint Bash";
        internal const string catLunarShard = "Lunar Shards";
        internal const string catSkyLeap = "Sky Leap";
        internal const string catAntiFlying = "Anti Flying Attack";
        internal const string catWeaponSlam = "Weapon Slam";
        internal const string catDash = "Dash";
        internal const string catArtifact = "Artifact of Hatred";

        public static class General
        {
            public static ConfigEntry<bool> statChanges;
            public static ConfigEntry<bool> debuffResist;
            public static ConfigEntry<bool> fallImmunity;
            public static ConfigEntry<float> freezeResist;
            public static ConfigEntry<bool> prioritizePlayers;
        }

        public static class WeaponSlam
        {
            public static ConfigEntry<bool> stopOnUse;
            public static ConfigEntry<bool> rotateBeforeUse;
            public static ConfigEntry<bool> spawnFirePillars;
            public static ConfigEntry<int> firePillarsPhase1;
            public static ConfigEntry<int> firePillarsPhase2;
            public static ConfigEntry<bool> fasterAttack;
            public static ConfigEntry<bool> phase1SunderWave;
            public static ConfigEntry<bool> phase2SunderWave;
        }

        public static class SprintBash
        {
            public static ConfigEntry<bool> fireProjectilesPhase1;
            public static ConfigEntry<bool> fireProjectilesPhase2;
            public static ConfigEntry<bool> antiTrimp;
            public static ConfigEntry<bool> fasterAttack;
        }

        public static class LunarShard
        {
            public static ConfigEntry<bool> replaceLunarShard;
            public static ConfigEntry<bool> replaceLunarShardPhase4;
        }

        public static class SkyLeap
        {
            public static ConfigEntry<bool> directTargetPlayer;
            public static ConfigEntry<bool> createPillar;
            public static ConfigEntry<int> firePillarsPhase1;
            public static ConfigEntry<int> firePillarsPhase2;
        }

        public static class AntiFlyingAttack
        {
            public static ConfigEntry<bool> enabled;
        }

        public static class Dash
        {
            public static ConfigEntry<bool> removeBackdashMoveScaling;
        }

        public static class Artifact
        {
            public static ConfigEntry<bool> enabled;
            public static ConfigEntry<bool> forceEnableInEclipse;
            public static ConfigEntry<bool> extraArmor;
            public static ConfigEntry<bool> extraSpeed;
            public static ConfigEntry<bool> groundOrbOnSlam;
            public static ConfigEntry<bool> groundOrbOnLeap;
            public static ConfigEntry<bool> moreMelee;
            public static ConfigEntry<bool> moreSlide;
            public static ConfigEntry<bool> slideCleanse;
            public static ConfigEntry<bool> fasterLeap;
            public static ConfigEntry<bool> moreSprintBashShards;
            public static ConfigEntry<bool> pizzaOnLeapP1;
            public static ConfigEntry<bool> pizzaOnLeapP2;
        }

        internal static void ReadConfig(ConfigFile config)
        {
            General.statChanges = config.Bind(new ConfigDefinition(catGeneral, "Stat Changes"), true);
            General.debuffResist = config.Bind(new ConfigDefinition(catGeneral, "Debuff Resistance"), true, new ConfigDescription("Slows and Attack Speed reduction are less effective."));
            General.fallImmunity = config.Bind(new ConfigDefinition(catGeneral, "Fall Damage Immunity"), true);
            General.freezeResist = config.Bind(new ConfigDefinition(catGeneral, "Freeze Resistance"), 0.5f, new ConfigDescription("Affects how long Freeze lasts on Mithrix. Set to 0 to make him immune to Freeze, 1 to make it work like Vanilla."));
            General.prioritizePlayers = config.Bind(new ConfigDefinition(catGeneral, "Prioritize Players"), true);

            WeaponSlam.stopOnUse = config.Bind(new ConfigDefinition(catWeaponSlam, "Stop Momentum"), true, new ConfigDescription("Stop Mithrix's movement before using Slam so that he doesn't overshoot his target when moving too fast."));
            WeaponSlam.rotateBeforeUse = config.Bind(new ConfigDefinition(catWeaponSlam, "Rotate Before Use"), true, new ConfigDescription("Rotate towards a player before using Slam."));
            WeaponSlam.spawnFirePillars = config.Bind(new ConfigDefinition(catWeaponSlam, "Flame Pillars"), true);
            WeaponSlam.firePillarsPhase1 = config.Bind(new ConfigDefinition(catWeaponSlam, "Flame Pillars - Phase 1 Count"), 3);
            WeaponSlam.firePillarsPhase2 = config.Bind(new ConfigDefinition(catWeaponSlam, "Flame Pillars - Phase 2 Count"), 5);
            WeaponSlam.phase1SunderWave = config.Bind(new ConfigDefinition(catWeaponSlam, "Flame Pillars - Phase 1 Sunder Waves"), true);
            WeaponSlam.phase2SunderWave = config.Bind(new ConfigDefinition(catWeaponSlam, "Flame Pillars - Phase 2 360 degree Sunder Waves"), true);
            WeaponSlam.fasterAttack = config.Bind(new ConfigDefinition(catWeaponSlam, "Faster Attack"), true);

            SprintBash.fireProjectilesPhase1 = config.Bind(new ConfigDefinition(catSprintBash, "Fire Projectiles Always"), true, new ConfigDescription("Always fire projectiles during Sprint Bash."));
            SprintBash.fireProjectilesPhase2 = config.Bind(new ConfigDefinition(catSprintBash, "Fire Projectiles in Phase 2"), true);
            SprintBash.antiTrimp = config.Bind(new ConfigDefinition(catSprintBash, "Anti-Trimp"), true, new ConfigDescription("Prevent Mithrix from launching himself into the sky."));
            SprintBash.fasterAttack = config.Bind(new ConfigDefinition(catSprintBash, "Faster Attack"), true);

            LunarShard.replaceLunarShard = config.Bind(new ConfigDefinition(catLunarShard, "Replace Sprinting Lunar Shardswith Orbs"), true);
            LunarShard.replaceLunarShardPhase4 = config.Bind(new ConfigDefinition(catLunarShard, "Replace Phase 4 Lunar Shards with Orbs"), true);

            SkyLeap.directTargetPlayer = config.Bind(new ConfigDefinition(catSkyLeap, "Target Players"), true);
            SkyLeap.createPillar = config.Bind(new ConfigDefinition(catSkyLeap, "Flame Pillar"), true);
            SkyLeap.firePillarsPhase1 = config.Bind(new ConfigDefinition(catSkyLeap, "Flame Pillars - Phase 1 Count"), 4);
            SkyLeap.firePillarsPhase2 = config.Bind(new ConfigDefinition(catSkyLeap, "Flame Pillars - Phase 2 Count"), 7);

            //AntiFlyingAttack.enabled = config.Bind(new ConfigDefinition(catAntiFlying, "Enable Attack"), true);

            Dash.removeBackdashMoveScaling = config.Bind(new ConfigDefinition(catDash, "Backwards Dash - Disable Move Speed Scaling"), true);

            Artifact.enabled = config.Bind(new ConfigDefinition(catArtifact, "Enable Artifact"), true);
            Artifact.forceEnableInEclipse = config.Bind(new ConfigDefinition(catArtifact, "Force Enable in Eclipse"), false);
            Artifact.extraArmor = config.Bind(new ConfigDefinition(catArtifact, "Extra Adaptive Armor"), true);
            Artifact.extraSpeed = config.Bind(new ConfigDefinition(catArtifact, "Extra Speed"), true);
            Artifact.groundOrbOnSlam = config.Bind(new ConfigDefinition(catArtifact, "Spawn Orbs on Weapon Slam"), true, new ConfigDescription("Requires Weapon Slam Flame Pillars."));
            Artifact.groundOrbOnLeap = config.Bind(new ConfigDefinition(catArtifact, "Spawn Orbs on Sky Leap"), true, new ConfigDescription("Requires Sky Leap Flame Pillars."));
            Artifact.moreMelee = config.Bind(new ConfigDefinition(catArtifact, "More Melee"), true, new ConfigDescription("More stocks and lower cooldowns on melee attacks."));
            Artifact.moreSlide = config.Bind(new ConfigDefinition(catArtifact, "More Dashes"), true, new ConfigDescription("More stocks and lower cooldowns on dashes."));
            Artifact.slideCleanse = config.Bind(new ConfigDefinition(catArtifact, "Dash Cleanse"), true, new ConfigDescription("Dash cleanses debuffs."));
            Artifact.fasterLeap = config.Bind(new ConfigDefinition(catArtifact, "Faster Sky Leap"), true);
            Artifact.moreSprintBashShards = config.Bind(new ConfigDefinition(catArtifact, "More Sprint Bash Shards"), true, new ConfigDescription("Requires Sprint Bash Fire Projectiles."));
            Artifact.pizzaOnLeapP1 = config.Bind(new ConfigDefinition(catArtifact, "Pizza After Sky Leap - Phase 1"),false);
            Artifact.pizzaOnLeapP2 = config.Bind(new ConfigDefinition(catArtifact, "Pizza After Sky Leap - Phase 2"), true);
        }
    }
}
