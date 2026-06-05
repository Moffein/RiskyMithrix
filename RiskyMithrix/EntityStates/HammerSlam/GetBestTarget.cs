using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EntityStates.RiskyMithrix.HammerSlam
{
    public class GetBestTarget : BaseState
    {
        public static float maxRange = 45f;
        public static float maxAngle = 180f;    //It's fine to have him 360
        public override void OnEnter()
        {
            base.OnEnter();

            CharacterBody target = FindBestTarget(maxRange);

            if (isAuthority)
            {
                if (target != null)
                {
                    outer.SetNextState(new RotateToTarget()
                    {
                        target = target
                    });
                }
                else
                {
                    outer.SetNextState(new EntityStates.BrotherMonster.WeaponSlam());
                }
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public CharacterBody FindBestTarget(float range)
        {
            float rangeSqr = range * range;
            CharacterBody target = null;

            Vector3 myForward = transform.forward;
            if (characterDirection) myForward = characterDirection.forward;
            Vector2 forward2d = new Vector2(myForward.x, myForward.z);
            forward2d.Normalize();

            Vector3 myPos = transform.position;
            if (characterBody) myPos = characterBody.corePosition;
            TeamIndex myTeam = GetTeam();
            var allEnemyBodies = CharacterBody.instancesList.Where(b =>
            {
                bool isValid = b.teamComponent
                && b.teamComponent.teamIndex != myTeam
                && b.master != null
                && !b.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless)
                && b.healthComponent && b.healthComponent.alive;

                //Range check
                Vector3 posDiff = b.corePosition - myPos;
                Vector2 posDiff2D = new Vector2(posDiff.x, posDiff.z);
                isValid = isValid && posDiff2D.sqrMagnitude <= rangeSqr;

                //Angle check
                if (isValid)
                {
                    Vector2 enemyAngle = new Vector2(posDiff.x, posDiff.z);
                    enemyAngle.Normalize();
                    float angle = Vector2.Angle(forward2d, enemyAngle);
                    isValid = isValid && angle <= maxAngle;
                }

                return isValid;
            });
            var allPlayerBodies = allEnemyBodies.Where(b => b.isPlayerControlled);

            var listToUse = allPlayerBodies.Count() > 0 ? allPlayerBodies : allEnemyBodies;
            if (listToUse.Count() > 0)
            {
                //Find body with the lowest angle diff
                //Inefficient to run this code twice
                float lowestAngle = -1f;
                foreach (CharacterBody body in listToUse)
                {
                    Vector3 posDiff = body.corePosition - myPos;
                    Vector2 enemyAngle = new Vector2(posDiff.x, posDiff.z);
                    enemyAngle.Normalize();
                    float angle = Vector2.Angle(forward2d, enemyAngle);

                    if (angle < lowestAngle || lowestAngle < 0f)
                    {
                        target = body;
                        lowestAngle = angle;
                    }
                }
            }

            return target;
        }
    }
}
