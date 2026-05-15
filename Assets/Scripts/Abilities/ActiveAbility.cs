using UnityEngine;
using System;

public class ActiveAbility : MonoBehaviour
{
    [Header("References")]
    public BaseAbilityData Data;

    [Header("Runtime State")]
    public float CurrentCooldown { get; private set; }
    public bool IsOnCooldown => CurrentCooldown > 0f;

    public event Action<ActiveAbility> OnCooldownStarted;
    public event Action<ActiveAbility> OnCooldownFinished;

    private BaseEntity _owner;

    public void Initialize(BaseEntity owner, BaseAbilityData data)
    {
        _owner = owner;
        Data = data;
        CurrentCooldown = 0f;
    }

    private void Update()
    {
        if (CurrentCooldown > 0f)
        {
            CurrentCooldown -= Time.deltaTime;
            if (CurrentCooldown <= 0f)
            {
                CurrentCooldown = 0f;
                OnCooldownFinished?.Invoke(this);
            }
        }
    }

    public bool TryUse(BaseEntity target)
    {
        if (_owner == null || Data == null || IsOnCooldown) 
            return false;

        if (!CanUse()) 
            return false;

        CurrentCooldown = Data.cooldown;
        OnCooldownStarted?.Invoke(this);

        ResourceUtility.ApplyResourceCost(_owner, Data.resourceCost);

        Debug.Log($"{_owner.EntityName} used {Data.abilityName} on {target?.EntityName}");

        // TODO: Trigger animation, damage, VFX here later
        CombatManager.Instance.ResolveAbility(this, _owner, target);
        return true;
    }

    public bool CanUse()
    {
        if (_owner == null) return false;

        // Skill check
        if (!string.IsNullOrEmpty(Data.linkedSkillName) && Data.minimumSkillLevel > 0)
        {
            var skill = _owner.Attributes.GetAttribute<SkillAttribute>(Data.linkedSkillName);
            if (skill == null || skill.CurrentValue < Data.minimumSkillLevel)
            {
                Debug.LogWarning($"{_owner.EntityName} cannot use {Data.abilityName} — {Data.linkedSkillName} too low");
                return false;
            }
        }

        // Resource check
        return ResourceUtility.CanAfford(_owner, Data.resourceCost);
    }

    public void ResetCooldown() => CurrentCooldown = 0f;
}