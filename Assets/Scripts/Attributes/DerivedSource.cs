using System;
using UnityEngine;

[Serializable]
public class DerivedSource
{
    [field: SerializeField] public BaseAttribute Source { get; set; }
    [field: SerializeField] public float Multiplier { get; set; } = 1f;

    public DerivedSource() { }

    public DerivedSource(BaseAttribute source, float multiplier = 1f)
    {
        Source = source;
        Multiplier = multiplier;
    }
}