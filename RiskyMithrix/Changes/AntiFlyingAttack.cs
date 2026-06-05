using RiskyMithrix.Modules;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RiskyMithrix.Changes
{
    internal static class AntiFlyingAttack
    {
        internal static void Init()
        {
            PluginContentPack.entityStatesTypes.Add(typeof(AntiFlyingAttack));
            if (!PluginConfig.AntiFlyingAttack.enabled.Value) return;

            On.EntityStates.BrotherMonster.WeaponSlam.OnExit += WeaponSlam_OnExit;
        }

        private static void WeaponSlam_OnExit(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnExit orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            self.outer.SetNextState(new EntityStates.RiskyMithrix.WorldSlam());
        }
    }
}
