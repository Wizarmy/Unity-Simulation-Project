using UnityEngine;

public class SkillAttribute : BaseAttribute
{
    private float _levelBonusPerPoint = 1f;

    protected override void SubscribeToDependencies()
    {
        // Skills can subscribe to other attributes here later (e.g. Unarmed boosts Strength)
    }

    public void SetLevelBonusPerPoint(float value) => _levelBonusPerPoint = value;

    protected override float GetLevelBonus()
    {
        if (!AttributeCanLevel) return 0f;
        return AttributeLevel.CurrentLevel * _levelBonusPerPoint;
    }

    // You can override CalculateMaxValue here later for skill-specific logic
}