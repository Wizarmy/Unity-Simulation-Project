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

    [Header("Targeting")]
    public bool affectsSelf = false;     // e.g. self-heal or self-damage

    [Header("Visuals / Feedback")]
    public string hitAnimationTrigger = "Hit";
    public GameObject hitVFXPrefab;
    
}