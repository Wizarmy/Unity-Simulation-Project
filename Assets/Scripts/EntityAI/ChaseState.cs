using UnityEngine;

public class ChaseState : AIState
{
    public override void Enter()
    {
        AI.FindNearestEnemy();
    }

    public override void Update()
    {
        if (!AI.CombatTarget)
        {
            AI.FindNearestEnemy();
            if (!AI.CombatTarget) return;
        }

        AI.MoveTowardsTarget(AI.CombatTarget);
        AI.LookAtTarget(AI.CombatTarget.Body.HeadTransform.position);

        var distance = Vector3.Distance(AI.BodyTransform.position, AI.CombatTarget.Body.transform.position);
        if (distance <= 3.0f)
           AI.EnterEngage();
    }
}