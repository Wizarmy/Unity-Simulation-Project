using System;
using System.Collections.Generic;
using UnityEngine;

public class AttributeManager : MonoBehaviour
{
    #region Singleton (Instanced - put one in your scene)
    private static AttributeManager _instance;
    public static AttributeManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject); // Optional - keep across scenes
        PreloadAllData();
    }
    #endregion
    
    [Header("JSON Files (Resources folder)")]
    [SerializeField] private string skillsJsonPath = "Data/skills";

    private Dictionary<string, SkillDefinition> _skillDefinitions = new Dictionary<string, SkillDefinition>(StringComparer.OrdinalIgnoreCase);

    private void PreloadAllData()
    {
        LoadSkills();
        Debug.Log($"[AttributeManager] Preloaded {_skillDefinitions.Count} skill definitions.");
    }
    
    private void LoadSkills()
    {
        var jsonText = Resources.Load<TextAsset>(skillsJsonPath);
        if (jsonText == null)
        {
            Debug.LogError($"[AttributeManager] Could not load skills JSON at Resources/{skillsJsonPath}.json");
            return;
        }

        var wrapper = JsonUtility.FromJson<SkillDefinitionWrapper>(jsonText.text);
        if (wrapper?.skills == null) return;

        _skillDefinitions.Clear();
        foreach (var skill in wrapper.skills)
        {
            _skillDefinitions[skill.name] = skill;
        }
    }

    public SkillDefinition GetSkillDefinition(string skillName)
    {
        if (_skillDefinitions.TryGetValue(skillName, out var def))
            return def;

        Debug.LogWarning($"[AttributeManager] Skill definition not found: {skillName}");
        return null;
    }

    // ====================== ADD SKILL LOGIC ======================
    /// <summary>
    /// Best place for AddSkill: AttributeManager knows about templates.
    /// It creates the SkillAttribute and returns it so EntityAttributes can add it.
    /// </summary>
    public SkillAttribute CreateAndAddSkill(EntityAttributes entityAttributes, string skillName)
    {
        var def = GetSkillDefinition(skillName);
        if (def == null) return null;

        // Create the component on the entity's SkillsContainer (you added this)
        var skillObj = new GameObject(skillName);
        skillObj.transform.SetParent(entityAttributes.SkillsContainer);

        var skill = skillObj.AddComponent<SkillAttribute>();

        // Initialize using your existing system
        skill.Initialize(entityAttributes, def.name, def.description, def.type, def.canLevel);
        skill.Initialize(def.baseValue, def.canLevel);

        // Optional: store level bonus multiplier if you want custom behavior
        skill.SetLevelBonusPerPoint(def.levelBonusPerPoint);

        entityAttributes.AddAttribute(skill); // NEW method we'll add below

        return skill;
    }
    
    
}

// Tiny wrapper for JSON array
[Serializable]
public class SkillDefinitionWrapper
{
    public List<SkillDefinition> skills;
}
