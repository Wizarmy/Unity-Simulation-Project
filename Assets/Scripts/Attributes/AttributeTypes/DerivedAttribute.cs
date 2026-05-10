using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DerivedAttribute : BaseAttribute
{
    [Header("Derived Sources (Multiple Supported)")]
    [field: SerializeField] 
    public List<DerivedSource> DerivedSources { get; protected set; } = new List<DerivedSource>();
    
    protected override void Awake()
    {
        base.Awake();
        AttributeType = AttributeTypeEnum.Derived;
    }

    protected override void SubscribeToDependencies()
    {
        base.SubscribeToDependencies();

        foreach (var source in DerivedSources.Where(source => source.Source != null))
        {
            source.Source.CurrentValueChanged += CalculateMaxValue;
        }
    }

    protected override void UnsubscribeFromDependencies()
    {
        base.UnsubscribeFromDependencies();

        foreach (var source in DerivedSources.Where(source => source.Source != null))
        {
            source.Source.CurrentValueChanged -= CalculateMaxValue;
        }
    }

    protected override float GetLevelBonus()
    {
        return AttributeCanLevel ? AttributeLevel.CurrentLevel / 10f : 0f;
    }

    protected override float GetExtraDerivedValue()
    {
        return DerivedSources.Where(source => source.Source).Sum(source => source.Source.CurrentValue * source.Multiplier);
    }

    /// <summary>
    /// Helper to easily add a new derived source at runtime
    /// </summary>
    public void AddDerivedSource(BaseAttribute source, float multiplier = 1f)
    {
        if (source == null) return;
        
        DerivedSources.Add(new DerivedSource(source, multiplier));
        source.CurrentValueChanged += CalculateMaxValue;
        CalculateMaxValue(); // Refresh immediately
    }

    /// <summary>
    /// Helper to remove a derived source
    /// </summary>
    public void RemoveDerivedSource(BaseAttribute source)
    {
        var toRemove = DerivedSources.Find(s => s.Source == source);
        if (toRemove == null) return;
        DerivedSources.Remove(toRemove);
        if (source != null)
            source.CurrentValueChanged -= CalculateMaxValue;
        CalculateMaxValue();
    }
}