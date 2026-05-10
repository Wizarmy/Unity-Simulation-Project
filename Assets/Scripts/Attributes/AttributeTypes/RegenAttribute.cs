using UnityEngine;

public class RegenAttribute : DerivedAttribute
{
    protected override void Awake()
    {
        base.Awake();
        AttributeType = AttributeTypeEnum.Regen;
    }

    protected override float GetLevelBonus()
    {
        return AttributeCanLevel ? AttributeLevel.CurrentLevel / 100f : 0f;
    }
}