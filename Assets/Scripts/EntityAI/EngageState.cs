using UnityEngine;

public class EngageState : AIState
{
    private float _nextAbilityCheckTime;
    private const float AbilityCheckInterval = 0.8f;

    public override void Enter()
    {
        _nextAbilityCheckTime = Time.time;
    }

    public override void Update()
    {
        if (!AI.CombatTarget || !AI.CombatTarget.IsAlive)
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

        TryUseAbility();
    }

    private void TryUseAbility()
    {
        if (!AI.CombatTarget || !AI.CombatTarget.IsAlive) return;
        
        if (Time.time < _nextAbilityCheckTime) 
            return;

        _nextAbilityCheckTime = Time.time + AbilityCheckInterval;

        ActiveAbility bestAbility = ChooseBestAbility();
        if (bestAbility == null) return;

        bool success = bestAbility.TryUse(AI.CombatTarget);
        if (success)
        {
            MessageManager.Instance.Log($"[AI] {AI.ParentEntity.EntityName} used {bestAbility.Data.abilityName}!");
        }
    }

    /// <summary>
    /// Smart ability selection that respects AbilityType (Attack vs Evasion etc.)
    /// </summary>
    private ActiveAbility ChooseBestAbility()
    {
        var abilities = AI.ParentEntity.ActiveAbilities;
        if (abilities.Count == 0) return null;

        ActiveAbility bestAttack = null;
        float bestAttackScore = -1f;

        foreach (var ability in abilities)
        {
            if (ability == null || ability.Data == null || !ability.CanUse()) 
                continue;

            // Range check
            float distance = Vector3.Distance(AI.BodyTransform.position, AI.CombatTarget.Body.transform.position);
            if (!ability.Data.range.IsInRange(distance))
                continue;

            float score = ScoreAbility(ability);

            if (ability.Data.abilityType == AbilityType.Attack && score > bestAttackScore)
            {
                bestAttackScore = score;
                bestAttack = ability;
            }
        }
        

        return bestAttack;
    }

    private float ScoreAbility(ActiveAbility ability)
    {
        if (ability?.Data == null) return 0f;

        float score = 10f; // Base score

        // Prefer higher damage / better scaling
        foreach (var effect in ability.Data.effects)
        {
            if (effect != null && effect.effectType == AbilityEffectType.Damage)
            {
                score += effect.baseValue * effect.scalingFactor * 2f;
            }
        }

        // Bonus for abilities with higher linked skill
        if (!string.IsNullOrEmpty(ability.Data.linkedSkillName))
        {
            var skill = AI.ParentEntity.Attributes.GetAttribute<SkillAttribute>(ability.Data.linkedSkillName);
            if (skill != null)
                score += skill.CurrentValue * 3f;
        }

        return score;
    }
}