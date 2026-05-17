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
    
    [Header("Lifecycle")]
    [field: SerializeField] public bool IsAlive { get; private set; } = true; 
    public bool IsPersistent { get; set; } = false;

    /// <summary>
    /// Fired whenever abilities are added, removed, or cleared.
    /// Useful for UI to refresh ability buttons.
    /// </summary>
    public event Action OnAbilitiesChanged;
    public event Action<BaseEntity> OnDied;

    /// <summary>
    /// Fired when this entity has finished all setup/reset operations 
    /// (attributes maxed, AI ready, abilities added, etc.)
    /// </summary>
    public event Action<BaseEntity> OnEntitySetupComplete;

    #region Initialization
    public virtual void InitializeEntity(string entityName, AvatarBody avatarBody,
        int team = 0, float entityScale = 1f, bool useRandomColors = true)
    {
        this.name = EntityName = entityName;
        EntityTeam = team;

        SetupAvatar(avatarBody, entityScale, team);
        InitializeAttributes();

        if (AbilityContainer == null)
        {
            var containerGo = new GameObject("AbilityContainer");
            AbilityContainer = containerGo.transform;
            AbilityContainer.SetParent(transform);
        }

        StartCoroutine(DelayedSetLifeToMax());

        AI = gameObject.AddComponent<EntityAI>();
        AI.Initialize(this);

        AddAbility("Punch");
        AddAbility("Dodge");

        // === DELAYED signal so subscribers have time to register ===
        StartCoroutine(DelayedSetupComplete());
    }
    #endregion

    #region Reset
    public virtual void FullReset(Vector3 newPosition)
    {
        Body.transform.position = newPosition;
        gameObject.SetActive(true);

        BringToLife();

        if (Attributes != null)
            Attributes.ResetAllToBase();

        AI?.ForceIdle();

        ClearAllAbilities();
        AddAbility("Punch");
        AddAbility("Dodge");

        // Re-register with EntityManager in case it was removed
        if (!EntityManager.Instance.EntityList.Contains(this))
        {
            EntityManager.Instance.EntityList.Add(this);
            Debug.Log($"[BaseEntity] Re-added {EntityName} to EntityManager list");
        }

        // Delayed signal
        StartCoroutine(DelayedSetupComplete());
    }
    #endregion
    
    private IEnumerator DelayedSetupComplete()
    {
        yield return null;           // Wait one frame so subscribers can hook up
        OnEntitySetupComplete?.Invoke(this);
        Debug.Log($"[BaseEntity] Setup complete for {EntityName}");
    }

    #region Ability Management
    public void AddAbility(string abilityName)
    {
        var data = Resources.Load<BaseAbilityData>($"Abilities/{abilityName}");
        if (data == null)
        {
            Debug.LogWarning($"[BaseEntity] Could not load ability data: {abilityName}");
            return;
        }

        if (!string.IsNullOrEmpty(data.linkedSkillName))
            Attributes.AddSkill(data.linkedSkillName);

        var abilityGo = new GameObject(data.abilityName);
        abilityGo.transform.SetParent(AbilityContainer);

        var activeAbility = abilityGo.AddComponent<ActiveAbility>();
        activeAbility.Initialize(this, data);

        ActiveAbilities.Add(activeAbility);

        OnAbilitiesChanged?.Invoke();
    }

    public void ClearAllAbilities()
    {
        foreach (var ability in ActiveAbilities)
        {
            if (ability != null && ability.gameObject != null)
                Destroy(ability.gameObject);
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
            if (ability == null || ability.Data == null || ability.Data.abilityName != abilityName)
                continue;

            Destroy(ability.gameObject);
            ActiveAbilities.RemoveAt(i);

            Debug.Log($"[BaseEntity] Removed ability '{abilityName}' from {EntityName}");
            OnAbilitiesChanged?.Invoke();
            return;
        }
    }
    #endregion

    #region Death Handling
    public virtual void Die(BaseEntity killer = null)
    {
        if (!IsAlive) return;

        IsAlive = false;

        Debug.Log($"[BaseEntity] {EntityName} (Team {EntityTeam}) has died!");

        if (AI != null)
            AI.enabled = false;

        OnDied?.Invoke(this);
    }

    public virtual void BringToLife()
    {
        if (IsAlive) return;

        IsAlive = true;

        Debug.Log($"[BaseEntity] {EntityName} brought back to life.");

        if (AI != null)
            AI.enabled = true;

        Attributes?.SetAllLifeAttributeToMax();
    }
    #endregion

    #region Private Helpers
    private IEnumerator DelayedSetLifeToMax()
    {
        yield return null;
        yield return null;
        Attributes?.SetAllLifeAttributeToMax();
    }

    private void SetupAvatar(AvatarBody avatarBody, float scale, int team)
    {
        Body = avatarBody;
        avatarBody.transform.SetParent(transform);
        avatarBody.InitializeAvatar(scale, team);
    }
    #endregion

    protected virtual void OnDestroy()
    {
        if (IsPersistent)
        {
            Debug.Log($"[BaseEntity] Skipping OnDestroy cleanup for persistent entity: {EntityName}");
            return;
        }

        if (AI != null)
            Destroy(AI);

        ClearAllAbilities();
    }

    protected abstract void InitializeAttributes();
}