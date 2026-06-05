using RiskyMithrix.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiskyMithrix.Changes
{
    internal static class DashChanges
    {
        internal static void Init()
        {
            RemoveBackdashMoveScaling();
        }

        private static void RemoveBackdashMoveScaling()
        {
            if (!PluginConfig.Dash.removeBackdashMoveScaling.Value) return;

            On.EntityStates.BrotherMonster.SlideBackwardState.OnEnter += SlideBackwardState_OnEnter;
        }

        private static void SlideBackwardState_OnEnter(On.EntityStates.BrotherMonster.SlideBackwardState.orig_OnEnter orig, EntityStates.BrotherMonster.SlideBackwardState self)
        {
            orig(self);
            if (self.characterBody)
            {
                self.moveSpeedStat = self.characterBody.baseMoveSpeed;
            }
        }
    }
}
