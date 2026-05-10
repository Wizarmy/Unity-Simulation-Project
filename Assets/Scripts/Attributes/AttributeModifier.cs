using System;
using UnityEngine;

[Serializable]
public class AttributeModifier
{
    [field: SerializeField] public string Source { get; private set; }
    [field: SerializeField] public float FlatBonus { get; private set; }
    [field: SerializeField] public float PercentBonus { get; private set; }
  
    public AttributeModifier(string source, float flat = 0f, float percent = 1f)
    {
        Source = source;
        FlatBonus = flat;
        PercentBonus = percent;
    }
}