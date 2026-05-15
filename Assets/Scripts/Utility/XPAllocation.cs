using System;
using UnityEngine;

[Serializable]
public class XPAllocation
{
    [Header("Distribution (0-100%)")]
    [Range(0f, 100f)] public float cultivationPercent = 40f;

    [Space(10)]
    [Header("Primary Attributes Split")]
    [Range(0f, 100f)] public float strengthPercent = 10f;
    [Range(0f, 100f)] public float agilityPercent = 10f;
    [Range(0f, 100f)] public float constitutionPercent = 10f;
    [Range(0f, 100f)] public float dexterityPercent = 10f;
    [Range(0f, 100f)] public float intelligencePercent = 10f;
    [Range(0f, 100f)] public float luckPercent = 10f;
    [Range(0f, 100f)] public float staminaPercent = 10f;
    [Range(0f, 100f)] public float willpowerPercent = 10f;
    [Range(0f, 100f)] public float wisdomPercent = 10f;

    [Space(10)]
    [Header("Life Attributes Split")]
    [Range(0f, 100f)] public float healthPercent;
    [Range(0f, 100f)] public float manaPercent;
    [Range(0f, 100f)] public float energyPercent;

    [Space(10)]
    [Header("Regen Attributes Split")]
    [Range(0f, 100f)] public float healthRegenPercent;
    [Range(0f, 100f)] public float manaRegenPercent;
    [Range(0f, 100f)] public float energyRegenPercent;

    public float TotalPercent => 
        cultivationPercent +
        strengthPercent + agilityPercent + constitutionPercent + dexterityPercent +
        intelligencePercent + luckPercent + staminaPercent + willpowerPercent + wisdomPercent +
        healthPercent + manaPercent + energyPercent +
        healthRegenPercent + manaRegenPercent + energyRegenPercent;

    public XPAllocation()
    {
        SetBalancedAcrossAllAttributes();
    }
    
    public void Normalize()
    {
        var total = TotalPercent;
        if (total <= 0.001f) return;

        var factor = 100f / total;

        cultivationPercent *= factor;
        strengthPercent *= factor;
        agilityPercent *= factor;
        constitutionPercent *= factor;
        dexterityPercent *= factor;
        intelligencePercent *= factor;
        luckPercent *= factor;
        staminaPercent *= factor;
        willpowerPercent *= factor;
        wisdomPercent *= factor;

        healthPercent *= factor;
        manaPercent *= factor;
        energyPercent *= factor;

        healthRegenPercent *= factor;
        manaRegenPercent *= factor;
        energyRegenPercent *= factor;
    }

    public float GetPercent(string attributeName)
    {
        return attributeName switch
        {
            // Primary
            "Strength" => strengthPercent,
            "Agility" => agilityPercent,
            "Constitution" => constitutionPercent,
            "Dexterity" => dexterityPercent,
            "Intelligence" => intelligencePercent,
            "Luck" => luckPercent,
            "Stamina" => staminaPercent,
            "Willpower" => willpowerPercent,
            "Wisdom" => wisdomPercent,

            // Life
            "Health" => healthPercent,
            "Mana" => manaPercent,
            "Energy" => energyPercent,

            // Regen
            "HealthRegen" => healthRegenPercent,
            "ManaRegen" => manaRegenPercent,
            "EnergyRegen" => energyRegenPercent,

            "Cultivation" => cultivationPercent,
            _ => 0f
        };
    }

    // ====================== PRESETS ======================

    public void SetBalancedPrimaryFocus()
    {
        Debug.Log("setBalancedPrimaryFocus selected");
        cultivationPercent = 0f;
        healthPercent = manaPercent = energyPercent = 0f;
        healthRegenPercent = manaRegenPercent = energyRegenPercent = 0f;

        var equalShare = 100f / 9f;

        strengthPercent = agilityPercent = constitutionPercent = dexterityPercent =
        intelligencePercent = luckPercent = staminaPercent = willpowerPercent = wisdomPercent = equalShare;

        Normalize();
    }

    /// <summary>
    /// Balanced across ALL attributes except Cultivation and EntityLevel
    /// (9 Primary + 3 Life + 3 Regen = 15 attributes)
    /// </summary>
    public void SetBalancedAcrossAllAttributes()
    {
        Debug.Log("SetBalancedAcrossAllAttributes selected");
        cultivationPercent = 0f;

        var equalShare = 100f / 15f;   // 15 attributes total

        // Primary
        strengthPercent = agilityPercent = constitutionPercent = dexterityPercent =
        intelligencePercent = luckPercent = staminaPercent = willpowerPercent = wisdomPercent = equalShare;

        // Life
        healthPercent = manaPercent = energyPercent = equalShare;

        // Regen
        healthRegenPercent = manaRegenPercent = energyRegenPercent = equalShare;

        Normalize();
    }

    public void SetCultivationFocus()
    {
        cultivationPercent = 70f;
        var remaining = 30f / 15f;   // spread across 15 attributes

        strengthPercent = agilityPercent = constitutionPercent = dexterityPercent =
        intelligencePercent = luckPercent = staminaPercent = willpowerPercent = wisdomPercent = remaining;

        healthPercent = manaPercent = energyPercent = remaining;
        healthRegenPercent = manaRegenPercent = energyRegenPercent = remaining;

        Normalize();
    }

    public void SetPhysicalFocus()
    {
        cultivationPercent = 10f;
        strengthPercent = 20f;
        constitutionPercent = 20f;
        staminaPercent = 15f;
        agilityPercent = 15f;

        dexterityPercent = intelligencePercent = luckPercent = willpowerPercent = wisdomPercent = 5f;

        healthPercent = 5f;
        energyPercent = 5f;
        healthRegenPercent = 3f;
        energyRegenPercent = 2f;

        Normalize();
    }
}