using UnityEngine;

public class XPManager : MonoBehaviour
{
    private static XPManager _instance;
    public static XPManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Distributes XP based on the entity's XPAllocation settings.
    /// Uses cached attributes for better performance.
    /// </summary>
    public void DistributeXP(BaseEntity entity, float totalXP)
    {
        if (entity?.Attributes == null || totalXP <= 0f) return;

        var allocation = entity.Attributes.XpAllocation;
        allocation.Normalize();

        Debug.Log($"[XPManager] Distributing {totalXP:F1} XP to {entity.EntityName}");

        // Cultivation (usually the most important)
        DistributeTo(entity, entity.Attributes.Cultivation, allocation.cultivationPercent, totalXP);

        // Primary Attributes
        DistributeTo(entity, entity.Attributes.Strength,     allocation.strengthPercent,     totalXP);
        DistributeTo(entity, entity.Attributes.Agility,      allocation.agilityPercent,      totalXP);
        DistributeTo(entity, entity.Attributes.Constitution, allocation.constitutionPercent, totalXP);
        DistributeTo(entity, entity.Attributes.Dexterity,    allocation.dexterityPercent,    totalXP);
        DistributeTo(entity, entity.Attributes.Intelligence, allocation.intelligencePercent, totalXP);
        DistributeTo(entity, entity.Attributes.Luck,         allocation.luckPercent,         totalXP);
        DistributeTo(entity, entity.Attributes.Stamina,      allocation.staminaPercent,      totalXP);
        DistributeTo(entity, entity.Attributes.Willpower,    allocation.willpowerPercent,    totalXP);
        DistributeTo(entity, entity.Attributes.Wisdom,       allocation.wisdomPercent,       totalXP);

        // Core / Life Attributes
        DistributeTo(entity, entity.Attributes.Health, allocation.healthPercent, totalXP);
        DistributeTo(entity, entity.Attributes.Mana,   allocation.manaPercent,   totalXP);
        DistributeTo(entity, entity.Attributes.Energy, allocation.energyPercent, totalXP);

        // Regen Attributes
        DistributeTo(entity, entity.Attributes.HealthRegen, allocation.healthRegenPercent, totalXP);
        DistributeTo(entity, entity.Attributes.ManaRegen,   allocation.manaRegenPercent,   totalXP);
        DistributeTo(entity, entity.Attributes.EnergyRegen, allocation.energyRegenPercent, totalXP);

        // Damage Attributes
        DistributeTo(entity, entity.Attributes.BluntDamage,     allocation.strengthPercent, totalXP); // example mapping
        DistributeTo(entity, entity.Attributes.PiercingDamage,  allocation.dexterityPercent, totalXP);
        DistributeTo(entity, entity.Attributes.SlashDamage,     allocation.strengthPercent, totalXP);
        DistributeTo(entity, entity.Attributes.ElementalDamage, allocation.intelligencePercent, totalXP);
        DistributeTo(entity, entity.Attributes.NatureDamage,    allocation.wisdomPercent, totalXP);
        DistributeTo(entity, entity.Attributes.ArcaneDamage,    allocation.intelligencePercent, totalXP);

        // Resistance Attributes
        DistributeTo(entity, entity.Attributes.BluntResist,     allocation.constitutionPercent, totalXP);
        DistributeTo(entity, entity.Attributes.PiercingResist,  allocation.dexterityPercent, totalXP);
        DistributeTo(entity, entity.Attributes.SlashResist,     allocation.constitutionPercent, totalXP);
        DistributeTo(entity, entity.Attributes.ElementalResist, allocation.intelligencePercent, totalXP);
        DistributeTo(entity, entity.Attributes.NatureResist,    allocation.wisdomPercent, totalXP);
        DistributeTo(entity, entity.Attributes.ArcaneResist,    allocation.intelligencePercent, totalXP);

        // Entity Level (overall progression)
        DistributeTo(entity, entity.Attributes.EntityLevel, 100f, totalXP);

        Debug.Log($"[XPManager] Finished XP distribution for {entity.EntityName}");
    }

    /// <summary>
    /// Helper to safely give XP to one attribute
    /// </summary>
    private void DistributeTo(BaseEntity entity, BaseAttribute attribute, float percent, float totalXP)
    {
        if (attribute == null || percent <= 0.01f) return;

        float xpAmount = totalXP * (percent / 100f);
        attribute.AddExperience(xpAmount);

        Debug.Log($"[XP] {entity.EntityName} → {attribute.AttributeName} +{xpAmount:F1} XP ({percent:F1}%)");
    }
}