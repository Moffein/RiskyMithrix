using RiskyMithrix.Components;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace EntityStates.RiskyMithrix.HammerSlam
{
    public class RotateToTarget : BaseState
    {
        public static float rotationSpeed = 600f;
        public static float maxDuration = 0.3f;
        public static float angleTolerance = 0.5f;
        public static float initialAngleTolerance = 10f;

        public CharacterBody target;

        public override void OnEnter()
        {
            if (isAuthority)
            {
                if (!target)
                {
                    outer.SetNextState(new EntityStates.BrotherMonster.WeaponSlam());
                    return;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                //Handle if target dies mid-state
                if (!target || fixedAge >= maxDuration)
                {
                    outer.SetNextState(new EntityStates.BrotherMonster.WeaponSlam());
                    return;
                }

                Vector3 myPos = transform.position;
                if (characterBody) myPos = characterBody.corePosition;

                Vector3 myForward = transform.forward;
                if (characterDirection) myForward = characterDirection.forward;

                Vector2 forward2d = new Vector2(myForward.x, myForward.z);
                forward2d.Normalize();

                //Get current angle between target and hammer
                Vector3 diff = target.corePosition - myPos;
                Vector2 enemyAngle = new Vector2(diff.x, diff.z);
                enemyAngle.Normalize();

                float angleDiff = Vector2.Angle(forward2d, enemyAngle);

                float desiredAngleTolerance = angleTolerance;
                if (fixedAge <= 0.033f) desiredAngleTolerance = initialAngleTolerance;
                //Stop if angle is close enough to enemy
                if (angleDiff <= desiredAngleTolerance)
                {
                    outer.SetNextState(new EntityStates.BrotherMonster.WeaponSlam());
                    return;
                }

                float angleToUse = Mathf.Min(angleDiff, rotationSpeed * GetDeltaTime());
                diff.Normalize();
                //Rotate
                Vector3 desiredVector = Vector3.RotateTowards(myForward, diff, Mathf.Deg2Rad * angleToUse, Mathf.Infinity);
                SetForward(desiredVector);
            }
        }

        public Vector3 GetForward()
        {
            if (characterDirection)
            {
                return characterDirection.forward;
            }
            else
            {
                return transform.forward;
            }
        }

        public void SetForward(Vector3 forward)
        {
            if (characterDirection)
            {
                characterDirection.forward = forward;
            }
            else
            {
                transform.forward = forward;
            }

            if (inputBank)
            {
                inputBank.aimDirection = forward;
            }

            if (!characterBody.isPlayerControlled && characterBody.master && characterBody.master.aiComponents != null)
            {
                foreach (BaseAI ai in characterBody.master.aiComponents)
                {
                    if (ai != null) ai.bodyInputs.desiredAimDirection = forward;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
