using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Base Ability", fileName = "New Ability")]
public class BaseAbility : ScriptableObject
{
    [Header("Core Info")]
    public string abilityName = "New Ability";
    [TextArea] public string description = "";
    public Sprite icon;

    [Header("Skill Requirement")]
    public string linkedSkillName = "Unarmed";   // e.g. "Unarmed", "Sword", "Evasion"
    public int minimumSkillLevel;

    [Header("Damage")]
    public DamageType damageType = DamageType.Blunt;
    public float baseDamage = 3f;
    public float damageScaling = 1.0f;           // Multiplier based on linked skill level

    [Header("Combat Stats")]
    public float cooldown = 1.2f;

    public ResourceCost resourceCost = new ResourceCost();
    public MinMaxRange range = new MinMaxRange();
    

    [Header("Animation / Visuals")]
    public string animationTrigger = "Punch";

    [Header("Modifiers (applied on hit)")]
    public List<AttributeModifier> onHitModifiers = new List<AttributeModifier>();

    // Optional future extensions
    [Header("Advanced")]
    public bool isRanged;
    public float projectileSpeed;
    public GameObject vfxPrefab;


    public virtual void Execute(BaseEntity caster, BaseEntity target)
    {
        if(CanAfford(caster))
        {
            
        }
    }

    public bool CanAfford(BaseEntity caster)
    {
        if (resourceCost.Health > 0f)
        {
            var health = caster.Attributes.Health;
            if (health != null && health.CurrentValue < resourceCost.Health)
                return false;
        }

        if (resourceCost.Energy > 0f)
        {
            var energy = caster.Attributes.Energy;
            if (energy != null && energy.CurrentValue < resourceCost.Energy)
                return false;
        }

        if (resourceCost.Mana > 0f)
        {
            var mana = caster.Attributes.Mana;
            if (mana != null && mana.CurrentValue < resourceCost.Mana)
                return false;
        }
        
        return true;
    }

    public void ApplyResourceCost(BaseEntity caster)
    {
        if (resourceCost.Health > 0f)
        {
            if (caster.Attributes.Health)
            {
                caster.Attributes.Health.ChangeCurrentValueByAmount(-resourceCost.Health);
            }
        }

        if (resourceCost.Energy > 0f)
        {
            if (caster.Attributes.Energy)
            {
                caster.Attributes.Energy.ChangeCurrentValueByAmount(-resourceCost.Energy);
            }
        }

        if (resourceCost.Mana > 0f)
        {
            if (caster.Attributes.Mana)
            {
                caster.Attributes.Mana.ChangeCurrentValueByAmount(-resourceCost.Mana);
            }
                
        }
    }

}