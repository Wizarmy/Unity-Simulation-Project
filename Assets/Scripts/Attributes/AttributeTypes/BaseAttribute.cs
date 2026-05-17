using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAttribute : MonoBehaviour
{
    [Header("Core Values")]
    [field: SerializeField] public string AttributeName { get; protected set; }
    [field: SerializeField] public string AttributeDescription { get; protected set; }
    [field: SerializeField] public AttributeTypeEnum AttributeType { get; protected set; }
    [field: SerializeField] public bool AttributeCanLevel { get; protected set; }
    [field: SerializeField] public AttributeLevel AttributeLevel { get; protected set; } = new AttributeLevel();
    
    [field: SerializeField] public float BaseValue { get; protected set; }
    [field: SerializeField] public float CurrentValue { get; protected set; }
    [field: SerializeField] public float MaxValue { get; protected set; }
    [field: SerializeField] public float FlatModifierValue { get; protected set; }
    [field: SerializeField] public float PercentModifierValue { get; protected set; }
    
    // Events
    public event Action CurrentValueChanged;
    public event Action MaxValueChanged;
    
    [Header("ParentAttributes")]
    [field: SerializeField] public EntityAttributes ParentAttributes { get; protected set; }

    [field: Header("AttributeModifiers")]
    [field: SerializeField]
    public List<AttributeModifier> AttributeModifiers { get; protected set; } = new List<AttributeModifier>();
    
    protected virtual void Awake()
    {
        AttributeLevel.InitilizeAttributeLevel(this);
        AttributeLevel.OnLevelUp += CalculateMaxValue;
        SubscribeToDependencies();                    // ← NEW
    }

  

    // NEW: Centralized dependency subscription (no more copy-paste in subclasses)
    protected virtual void SubscribeToDependencies() { }
    protected virtual void UnsubscribeFromDependencies() { }

    public void Initialize(EntityAttributes parentAttributes, string attributeName, string attributeDescription,
        AttributeTypeEnum attributeType, bool attributeCanLevel)
    {
        ParentAttributes = parentAttributes;
        AttributeName = attributeName;
        AttributeDescription = attributeDescription;
        AttributeType = attributeType;
        AttributeCanLevel = attributeCanLevel;
    }

    public void Initialize(float baseValue, bool attributeCanLevel)
    {
        BaseValue = baseValue;
        AttributeCanLevel = attributeCanLevel;
    }

    protected void CalculateModifierValues()
    {
        FlatModifierValue = PercentModifierValue = 0f;

        foreach (var modifier in AttributeModifiers)
        {
            FlatModifierValue += modifier.FlatBonus;
            PercentModifierValue += modifier.PercentBonus;
        }
    }
    
    protected virtual float GetLevelBonus()
    {
        // Cultivation is special - it always applies its level as a multiplier source
        if (AttributeType == AttributeTypeEnum.Cultivation)
            return AttributeLevel.CurrentLevel;

        // Normal behavior for other attributes
        return AttributeCanLevel ? AttributeLevel.CurrentLevel : 0f;
    }

    protected virtual float GetExtraDerivedValue()
    {
        return 0f;
    }

    protected virtual float GetFinalMultiplier()
    {
        return 1f;
    }

    protected virtual void CalculateMaxValue()
    {
        CalculateModifierValues();
        var levelBonus = GetLevelBonus();
        var extraDerivedValue = GetExtraDerivedValue();
        var calculatedValue = (BaseValue + levelBonus + FlatModifierValue + extraDerivedValue) 
                            * (1 + PercentModifierValue) 
                            * GetFinalMultiplier();
        SetMaxValue(calculatedValue);
    }

    public void AddExperience(float experience)
    {
        if (AttributeCanLevel)
        {
            AttributeLevel.AddExperience(experience);
        }
    }

    public void SetCanLevel(bool canLevel)
    {
        AttributeCanLevel = canLevel;
    }

    public void SetCurrentValueToMaxValue()
    {
        SetCurrentValue(MaxValue);
    }

    //Test method
    public void AddToBaseValue(float valueToAdd)
    {
        BaseValue += valueToAdd;
        CalculateMaxValue();
    }

    public void SetBaseValue(float baseValue)
    {
        BaseValue = baseValue;
        CalculateMaxValue();
    }

    public void SetCurrentValue(float newCurrentValue)
    {
        var oldCurrentValue = CurrentValue;
        CurrentValue = newCurrentValue;

        if (!Mathf.Approximately(oldCurrentValue, CurrentValue))
        {
            CurrentValueChanged?.Invoke();
        }
    }

    public virtual void SetMaxValue(float newMaxValue)
    {
        var oldMaxValue = MaxValue;
        MaxValue = newMaxValue;

        if (!Mathf.Approximately(oldMaxValue, MaxValue))
        {
            MaxValueChanged?.Invoke();
        }
        
        if (AttributeType != AttributeTypeEnum.Life)
            SetCurrentValue(MaxValue);
    }

    public virtual void ChangeCurrentValueByAmount(float amountToChangeBy)
    {
        SetCurrentValue(CurrentValue + amountToChangeBy);
    }

    // NEW: Proper cleanup for ALL attributes (prevents memory leaks)
    protected virtual void OnDestroy()
    {
        AttributeLevel.OnLevelUp -= CalculateMaxValue;
        UnsubscribeFromDependencies();
    }
}