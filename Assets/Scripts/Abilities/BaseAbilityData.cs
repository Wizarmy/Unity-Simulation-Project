using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ability Data", fileName = "New Ability Data")]
public class BaseAbilityData : ScriptableObject
{
    [Header("Core Info")]
    public string abilityName = "New Ability";
    [TextArea] public string description = "";
    public Sprite icon;

    [Header("Skill Requirement")]
    public string linkedSkillName = "Unarmed";
    public int minimumSkillLevel;

    [Header("Effects (what this ability actually does)")]
    public List<AbilityEffect> effects = new List<AbilityEffect>();

    [Header("Combat Stats")]
    public float cooldown = 1.2f;          // ← Moved here

    public ResourceCost resourceCost;
    public MinMaxRange range = new MinMaxRange();

    [Header("Animation / Visuals")]
    public string animationTrigger = "Punch";

    [Header("Modifiers")]
    public List<AttributeModifier> onHitModifiers = new List<AttributeModifier>();

    [Header("Advanced")]
    public bool isRanged;
    public float projectileSpeed;
    public GameObject vfxPrefab;
}