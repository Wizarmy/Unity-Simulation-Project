using UnityEngine;

public class EngageState : AIState
{
    public override void Update()
    {
        if (!AI.CombatTarget)
        {
           AI.EnterChase();
            return;
        }

        AI.LookAtTarget(AI.CombatTarget.Body.HeadTransform.position);

        float distance = Vector3.Distance(AI.BodyTransform.position, AI.CombatTarget.Body.transform.position);

        if (distance > 3f)
            AI.EnterChase();
        else if (distance < 1.5f)
            AI.BackAway(AI.CombatTarget);
    }
}