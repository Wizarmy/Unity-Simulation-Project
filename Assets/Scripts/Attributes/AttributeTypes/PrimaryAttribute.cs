using System;
using UnityEngine;

public class PrimaryAttribute : BaseAttribute
{
    [Header("Cultivation Attribute")]
    [field: SerializeField] public BaseAttribute CultivationAttribute { get; set; }

    protected override void Awake()
    {
        base.Awake();
        AttributeType = AttributeTypeEnum.Primary;
    }

    protected override void SubscribeToDependencies()
    {
        base.SubscribeToDependencies();
        if (CultivationAttribute != null)
        {
            CultivationAttribute.CurrentValueChanged += CalculateMaxValue;
        }
    }

    protected override void UnsubscribeFromDependencies()
    {
        base.UnsubscribeFromDependencies();
        if (CultivationAttribute != null)
        {
            CultivationAttribute.CurrentValueChanged -= CalculateMaxValue;
        }
    }

    protected override float GetLevelBonus()
    {
        return AttributeCanLevel ? AttributeLevel.CurrentLevel / 10f : 0f;
    }

    protected override float GetFinalMultiplier()
    {
        return 1f + (CultivationAttribute != null ? CultivationAttribute.CurrentValue / 10f : 0f);
    }
}