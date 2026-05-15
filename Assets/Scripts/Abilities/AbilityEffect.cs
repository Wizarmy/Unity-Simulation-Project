using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ability Effect", fileName = "New Effect")]
public class AbilityEffect : ScriptableObject
{
    [Header("Core")]
    public AbilityEffectType effectType = AbilityEffectType.Damage;
    public string effectName = "Damage";

    [Header("Damage / Healing")]
    public DamageType damageType = DamageType.Blunt;
    public float baseValue = 3f;
    public float scalingFactor = 1.0f;   // Multiplied by caster's linked skill level

    [Header("Primary Attribute Bonus")]
    public PrimaryAttributeType primaryEffectAttribute = PrimaryAttributeType.Strength;   // e.g. "Strength", "Intelligence", etc.
    [Range(0f, 0.1f)]
    public float primaryAttributeMultiplier = 0.02f;
    
    [Header("Primary Attribute DefenceBonus")]
    public PrimaryAttributeType primaryEffectDefenceAttribute = PrimaryAttributeType.Constitution;   // e.g. "Strength", "Intelligence", etc.
    [Range(0f, 0.1f)]
    public float primaryAttributeDefenceMultiplier = 0.02f;
    
    [Header("Visuals / Feedback")]
    public string hitAnimationTrigger = "Hit";
    public GameObject hitVFXPrefab;
    
}