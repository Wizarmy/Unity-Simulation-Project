using System;

[Serializable]
public class SkillDefinition
{
    public string name;
    public string description;
    public AttributeTypeEnum type = AttributeTypeEnum.Skill; // You should have this enum
    public float baseValue = 10f;
    public bool canLevel = true;
    public float levelBonusPerPoint = 1f;
    // Add more fields later (icon, category, etc.)
}