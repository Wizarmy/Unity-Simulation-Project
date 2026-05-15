using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEntity : MonoBehaviour
{
    [Header("Basic Information")]
    [field: SerializeField] public string EntityName { get; protected set; }
    [field: SerializeField] public int EntityTeam { get; protected set; }

    [Header("Avatar")]
    [field: SerializeField] public AvatarBody Body { get; protected set; }

    [Header("Entity AI")]
    [field: SerializeField] public EntityAI AI { get; protected set; }

    [Header("Attributes")]
    [field: SerializeField] public EntityAttributes Attributes { get; protected set; }

    [Header("Abilities")]
    [field: SerializeField] public Transform AbilityContainer { get; protected set; }
    [field: SerializeField] public List<ActiveAbility> ActiveAbilities { get; protected set; } = new List<ActiveAbility>();

    /// <summary>
    /// Fired whenever abilities are added, removed, or cleared.
    /// Useful for UI to refresh ability buttons.
    /// </summary>
    public event Action OnAbilitiesChanged;


    #region Initialization
    public virtual void InitializeEntity(string entityName, AvatarBody avatarBody,
        int team = 0, float entityScale = 1f, bool useRandomColors = true)
    {
        this.name = EntityName = entityName;
        EntityTeam = team;

        SetupAvatar(avatarBody, entityScale, team);
        InitializeAttributes();

        // Create Ability Container if it doesn't exist
        if (AbilityContainer == null)
        {
            var containerGo = new GameObject("AbilityContainer");
            AbilityContainer = containerGo.transform;
            AbilityContainer.SetParent(transform);
        }

        // Wait for attribute dependencies before setting life to max
        StartCoroutine(DelayedSetLifeToMax());

        AI = gameObject.AddComponent<EntityAI>();
        AI.Initialize(this);

        // Default starting ability
        AddAbility("Punch");
        AddAbility("Dodge");
        
    }
    #endregion


    #region Ability Management
    public void AddAbility(string abilityName)
    {
        var data = Resources.Load<BaseAbilityData>($"Abilities/{abilityName}");
        if (data == null)
        {
            Debug.LogWarning($"[BaseEntity] Could not load ability data: {abilityName}");
            return;
        }

        // Ensure linked skill exists
        if (!string.IsNullOrEmpty(data.linkedSkillName))
            Attributes.AddSkill(data.linkedSkillName);

        // Create dedicated child GameObject under AbilityContainer
        var abilityGo = new GameObject(data.abilityName);
        abilityGo.transform.SetParent(AbilityContainer);

        // Add runtime component
        var activeAbility = abilityGo.AddComponent<ActiveAbility>();
        activeAbility.Initialize(this, data);

        ActiveAbilities.Add(activeAbility);

        Debug.Log($"[BaseEntity] ✅ Added ability '{data.abilityName}' to {EntityName}");

        OnAbilitiesChanged?.Invoke();
    }

    public void ClearAllAbilities()
    {
        foreach (var ability in ActiveAbilities)
        {
            if (ability != null && ability.gameObject != null)
            {
                Destroy(ability.gameObject);
            }
        }

        ActiveAbilities.Clear();

        Debug.Log($"[BaseEntity] Cleared all abilities from {EntityName}");
        OnAbilitiesChanged?.Invoke();
    }

    public void RemoveAbility(string abilityName)
    {
        for (var i = ActiveAbilities.Count - 1; i >= 0; i--)
        {
            var ability = ActiveAbilities[i];
            if (ability == null || 
                ability.Data == null || 
                ability.Data.abilityName != abilityName)
                continue;

            Destroy(ability.gameObject);
            ActiveAbilities.RemoveAt(i);

            Debug.Log($"[BaseEntity] Removed ability '{abilityName}' from {EntityName}");
            OnAbilitiesChanged?.Invoke();
            return;
        }
    }
    #endregion


    #region Private Helpers
    private IEnumerator DelayedSetLifeToMax()
    {
        yield return null;
        yield return null;

        if (!Attributes) yield break;

        Attributes.SetAllLifeAttributeToMax();
    }

    private void SetupAvatar(AvatarBody avatarBody, float scale, int team)
    {
        Body = avatarBody;
        avatarBody.transform.SetParent(transform);
        avatarBody.InitializeAvatar(scale, team);
    }
    #endregion


    // ====================== CLEANUP ======================
    protected virtual void OnDestroy()
    {
        if (AI != null)
        {
            Destroy(AI);
        }

        // Automatic cleanup of all abilities
        ClearAllAbilities();
    }

    protected abstract void InitializeAttributes();
}