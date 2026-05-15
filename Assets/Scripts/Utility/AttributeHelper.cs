/// <summary>
/// Static helper for quick access to cached attributes
/// </summary>
public static class AttributeHelper
{
    /// <summary>
    /// Gets the corresponding cached Primary Attribute from EntityAttributes using the enum
    /// </summary>
    public static BaseAttribute GetPrimaryAttribute(EntityAttributes attributes, PrimaryAttributeType type)
    {
        if (attributes == null) return null;

        return type switch
        {
            PrimaryAttributeType.Strength     => attributes.Strength,
            PrimaryAttributeType.Agility      => attributes.Agility,
            PrimaryAttributeType.Constitution => attributes.Constitution,
            PrimaryAttributeType.Dexterity    => attributes.Dexterity,
            PrimaryAttributeType.Intelligence => attributes.Intelligence,
            PrimaryAttributeType.Luck         => attributes.Luck,
            PrimaryAttributeType.Stamina      => attributes.Stamina,
            PrimaryAttributeType.Willpower    => attributes.Willpower,
            PrimaryAttributeType.Wisdom       => attributes.Wisdom,
            _ => null
        };
    }

    /// <summary>
    /// Overload for convenience when you already have a BaseEntity
    /// </summary>
    public static BaseAttribute GetPrimaryAttribute(BaseEntity entity, PrimaryAttributeType type)
    {
        return entity != null ? GetPrimaryAttribute(entity.Attributes, type) : null;
    }
    
    /// <summary>
    /// Gets Damage Attribute by DamageType
    /// </summary>
    public static BaseAttribute GetDamageAttribute(EntityAttributes attributes, DamageType damageType)
    {
        if (attributes == null) return null;

        return damageType switch
        {
            DamageType.Blunt     => attributes.BluntDamage,
            DamageType.Piercing  => attributes.PiercingDamage,
            DamageType.Slash     => attributes.SlashDamage,
            DamageType.Elemental => attributes.ElementalDamage,
            DamageType.Nature    => attributes.NatureDamage,
            DamageType.Arcane    => attributes.ArcaneDamage,
            _ => null
        };
    }

    public static BaseAttribute GetDamageAttribute(BaseEntity entity, DamageType damageType)
    {
        return entity != null ? GetDamageAttribute(entity.Attributes, damageType) : null;
    }

    /// <summary>
    /// Gets Resistance Attribute by DamageType
    /// </summary>
    public static BaseAttribute GetResistanceAttribute(EntityAttributes attributes, DamageType damageType)
    {
        if (attributes == null) return null;

        return damageType switch
        {
            DamageType.Blunt     => attributes.BluntResist,
            DamageType.Piercing  => attributes.PiercingResist,
            DamageType.Slash     => attributes.SlashResist,
            DamageType.Elemental => attributes.ElementalResist,
            DamageType.Nature    => attributes.NatureResist,
            DamageType.Arcane    => attributes.ArcaneResist,
            _ => null
        };
    }

    public static BaseAttribute GetResistanceAttribute(BaseEntity entity, DamageType damageType)
    {
        return entity != null ? GetResistanceAttribute(entity.Attributes, damageType) : null;
    }
}