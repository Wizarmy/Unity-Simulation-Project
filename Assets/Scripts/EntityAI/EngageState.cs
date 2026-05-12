using UnityEngine;

public class EngageState : AIState
{
    private float _nextAbilityCheckTime;
    private const float AbilityCheckInterval = 0.8f;   // How often AI considers using abilities

    public override void Enter()
    {
        _nextAbilityCheckTime = Time.time;
    }

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
        {
            AI.EnterChase();
            return;
        }
        else if (distance < 1.5f)
        {
            AI.BackAway(AI.CombatTarget);
        }

        // === NEW: Try to use abilities ===
        TryUseAbility();
    }

    private void TryUseAbility()
    {
        if (Time.time < _nextAbilityCheckTime) 
            return;

        _nextAbilityCheckTime = Time.time + AbilityCheckInterval;

        var abilities = AI.ParentEntity.ActiveAbilities;
        if (abilities.Count == 0) return;

        // Simple strategy: Try abilities in order (you can improve this later with priorities, ranges, etc.)
        foreach (var ability in abilities)
        {
            if (ability.CanUse() && ability.Data.range.IsInRange(Vector3.Distance(
                    AI.BodyTransform.position, AI.CombatTarget.Body.transform.position)))
            {
                bool success = ability.TryUse(AI.CombatTarget);
                if (success)
                {
                    MessageManager.Instance.Log($"[AI] {AI.ParentEntity.EntityName} used {ability.Data.abilityName}!");
                    return; // Only use one ability per check
                }
            }
        }
    }
}