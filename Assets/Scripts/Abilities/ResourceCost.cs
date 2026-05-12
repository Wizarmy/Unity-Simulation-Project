using System;
using UnityEngine;

[Serializable]
public struct ResourceCost
{
    [Header("Resource Costs")]
    [Tooltip("Health cost (e.g. for self-damage abilities)")]
    public float Health;

    [Tooltip("Energy / Stamina cost")]
    public float Energy;

    [Tooltip("Mana cost")]
    public float Mana;

    /// <summary>
    /// Returns true if the cost is effectively zero (no resources required).
    /// </summary>
    public bool IsZero => Health <= 0f && Energy <= 0f && Mana <= 0f;

    public ResourceCost(float health = 0f, float energy = 0f, float mana = 0f)
    {
        Health = Mathf.Max(0f, health);
        Energy = Mathf.Max(0f, energy);
        Mana = Mathf.Max(0f, mana);
    }

    /// <summary>
    /// Creates a copy with all costs multiplied by a factor (useful for scaling with level/skill).
    /// </summary>
    public ResourceCost Scale(float multiplier) => new ResourceCost(
        Health * multiplier,
        Energy * multiplier,
        Mana * multiplier
    );
}