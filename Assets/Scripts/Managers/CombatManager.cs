using UnityEngine;

public class CombatManager : MonoBehaviour
{
    #region Singleton
    private static CombatManager _instance;
    public static CombatManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogWarning("CombatManager.Instance accessed before Awake.");
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
    #endregion

    #region Serialized Settings
    [Header("Combat Settings")]
    [SerializeField] private bool allowFriendlyFire = false;

    [Header("Hit Chance Settings")]
    [SerializeField, Range(0f, 1f)] private float baseHitChance = 0.95f; // Note: currently unused in IsMiss

    [Header("Formula Coefficients")]
    [SerializeField] private float attributeBonusPerPoint = 0.02f;

    [SerializeField] private float missBaseChance = 0.75f;
    [SerializeField] private float missSkillFactor = 0.15f;
    [SerializeField] private float missAgilityFactor = 0.008f;
    [SerializeField] private float missLuckFactor = 0.005f;
    [SerializeField, Range(0f, 1f)] private float missMinChance = 0.35f;
    [SerializeField, Range(0f, 1f)] private float missMaxChance = 0.98f;

    [SerializeField] private float dodgeBaseChance = 0.05f;
    [SerializeField] private float dodgeEvasionFactor = 0.012f;
    [SerializeField] private float dodgeDefenderAgilityFactor = 0.006f;
    [SerializeField] private float dodgeAttackerSkillReductionFactor = 0.008f;
    [SerializeField] private float dodgeAttackerAgilityReductionFactor = 0.004f;
    [SerializeField, Range(0f, 1f)] private float dodgeMinChance = 0.02f;
    [SerializeField, Range(0f, 1f)] private float dodgeMaxChance = 0.45f;

    [SerializeField, Range(0f, 1f)] private float baseCritChance = 0.15f;
    [SerializeField] private float critLuckFactor = 0.003f;
    [SerializeField, Range(0f, 1f)] private float critMinChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float critMaxChance = 0.40f;
    [SerializeField] private float critMultiplier = 1.8f;

    [Header("Default Attribute Values")]
    [SerializeField] private float defaultAttributeValue = 10f;
    [SerializeField] private float defaultEvasionValue = 5f;

    [Header("Experience Settings")]
    [SerializeField, Range(0f, 1f)] private float linkedSkillExperienceFraction = 0.333f;
    [SerializeField, Range(0f, 1f)] private float dodgeEvasionExperienceFraction = 0.333f;

    [Header("Floating Text Settings")]
    [SerializeField] private float missTextYOffset = 1.2f;
    [SerializeField] private float damageTextYOffset = 0.5f;
    
   
    #endregion

    #region Main Entry Point
    public void ResolveAbility(ActiveAbility ability, BaseEntity caster, BaseEntity target)
    {
        if (caster == null || target == null || ability?.Data == null)
        {
            Debug.LogWarning("[Combat] ResolveAbility called with null references.");
            return;
        }

        Debug.Log($"[Combat] {caster.EntityName} attempts {ability.Data.abilityName} on {target.EntityName}");

        if (ability.Data.abilityType != AbilityType.Attack)
        {
            Debug.Log($"[Combat] Skipped non-Attack ability: {ability.Data.abilityName}");
            return;
        }

        if (!allowFriendlyFire && caster.EntityTeam == target.EntityTeam)
        {
            Debug.Log($"[Combat] Friendly fire blocked: {caster.EntityName} → {target.EntityName}");
            return;
        }

        if (IsMiss(caster, ability))
        {
            MessageManager.Instance?.Log($"{caster.EntityName}'s {ability.Data.abilityName} missed!");

            if (FloatingTextManager.Instance != null)
            {
                Vector3 missPos = caster.Body?.transform.position ?? caster.transform.position;
                missPos.y += missTextYOffset;
                FloatingTextManager.Instance.ShowMiss(missPos);
            }
            return;
        }

        if (TargetDodges(caster, target, ability))
        {
            MessageManager.Instance?.Log($"{target.EntityName} dodged the attack!");

            // Evasion receives 1/3 of the total damage that WOULD have been applied
            GrantEvasionExperienceOnDodge(caster, target, ability);
            return;
        }

        foreach (var effect in ability.Data.effects)
        {
            if (effect == null || effect.effectType != AbilityEffectType.Damage) continue;

            float rawDamage = CalculateRawDamage(caster, ability, effect);
            bool isCrit = IsCriticalHit(caster);
            float finalDamage = isCrit ? rawDamage * critMultiplier : rawDamage;

            ApplyDamage(target, finalDamage, effect.damageType, effect.primaryEffectDefenceAttribute, isCrit, caster, ability);
        }
    }
    #endregion

    #region Helper Methods
    private float GetLinkedSkillLevel(BaseEntity caster, ActiveAbility ability)
    {
        if (string.IsNullOrEmpty(ability.Data.linkedSkillName)) return 1f;

        var skill = caster.Attributes.GetAttribute<SkillAttribute>(ability.Data.linkedSkillName);
        return skill?.CurrentValue ?? 1f;
    }

    private void ApplyExperience(BaseEntity entity, BaseAttribute attribute, float amount, string source = "")
    {
        if (attribute == null || amount <= 0f) return;

        if (entity is not CharacterEntity)
        {
           // Debug.Log($"[Combat XP] Skipped XP gain for {entity.EntityName} (not a CharacterEntity)");
            return;
        }

        attribute.AddExperience(amount);

        string logSource = string.IsNullOrEmpty(source) ? attribute.AttributeName : source;
      //  Debug.Log($"[Combat XP] {logSource} gained +{amount:F1} XP");
    }

    private void ApplyLinkedSkillExperience(BaseEntity caster, ActiveAbility ability, float baseAmount)
    {
        if (caster == null || ability?.Data == null || string.IsNullOrEmpty(ability.Data.linkedSkillName))
            return;

        var skillAttr = caster.Attributes.GetAttribute<SkillAttribute>(ability.Data.linkedSkillName);
        if (skillAttr == null) return;

        float skillXP = baseAmount * linkedSkillExperienceFraction;
        ApplyExperience(caster, skillAttr, skillXP, $"{caster.EntityName}'s {skillAttr.AttributeName} (linked skill)");
    }

    private void GrantEvasionExperienceOnDodge(BaseEntity caster, BaseEntity target, ActiveAbility ability)
    {
        float potentialDamage = CalculatePotentialTotalDamage(caster, target, ability);
        if (potentialDamage <= 0f) return;

        var evasionAttr = target.Attributes.GetAttribute("Evasion");
        if (evasionAttr == null) return;

        float evasionXP = potentialDamage * dodgeEvasionExperienceFraction;
        ApplyExperience(target, evasionAttr, evasionXP, $"{target.EntityName}'s Evasion (successful dodge)");
    }
    #endregion

    #region Combat Resolution
    private bool IsMiss(BaseEntity caster, ActiveAbility ability)
    {
      //  Debug.Log($"[IsMiss] Checking miss for {caster.EntityName} using {ability.Data.abilityName}");

        float skillLevel = GetLinkedSkillLevel(caster, ability);
        float agility = caster.Attributes.Agility?.CurrentValue ?? defaultAttributeValue;
        float luck = caster.Attributes.Luck?.CurrentValue ?? defaultAttributeValue;

        float hitChance = missBaseChance + (skillLevel * missSkillFactor)
                        + (agility * missAgilityFactor) + (luck * missLuckFactor);
        hitChance = Mathf.Clamp(hitChance, missMinChance, missMaxChance);

        bool missed = Random.value > hitChance;
     //   Debug.Log($"[IsMiss] Hit Chance: {hitChance:P1} → {(missed ? "MISS!" : "Hit")}");
        return missed;
    }

    private bool TargetDodges(BaseEntity caster, BaseEntity target, ActiveAbility ability)
    {
    //    Debug.Log($"[TargetDodges] Checking dodge for {target.EntityName} against {caster.EntityName}");

        float attackerSkill = GetLinkedSkillLevel(caster, ability);
        float attackerAgility = caster.Attributes.Agility?.CurrentValue ?? defaultAttributeValue;
        float defenderEvasion = target.Attributes.GetAttribute("Evasion")?.CurrentValue ?? defaultEvasionValue;
        float defenderAgility = target.Attributes.Agility?.CurrentValue ?? defaultAttributeValue;

        float dodgeChance = dodgeBaseChance 
                          + (defenderEvasion * dodgeEvasionFactor) 
                          + (defenderAgility * dodgeDefenderAgilityFactor)
                          - (attackerSkill * dodgeAttackerSkillReductionFactor) 
                          - (attackerAgility * dodgeAttackerAgilityReductionFactor);

        dodgeChance = Mathf.Clamp(dodgeChance, dodgeMinChance, dodgeMaxChance);

        bool dodged = Random.value < dodgeChance;
     //   Debug.Log($"[TargetDodges] Dodge Chance: {dodgeChance:P1} → {(dodged ? "DODGED!" : "Failed to dodge")}");
        return dodged;
    }

    private bool IsCriticalHit(BaseEntity caster)
    {
        float luck = caster.Attributes.Luck?.CurrentValue ?? defaultAttributeValue;
        float critChance = baseCritChance + (luck * critLuckFactor);
        critChance = Mathf.Clamp(critChance, critMinChance, critMaxChance);

        bool isCrit = Random.value < critChance;
      //  Debug.Log($"[IsCriticalHit] Crit Chance: {critChance:P1} → {(isCrit ? "CRITICAL!" : "Normal")}");
        return isCrit;
    }
    #endregion

    #region Damage Calculation
    private float CalculateRawDamage(BaseEntity caster, ActiveAbility ability, AbilityEffect effect)
    {
   //     Debug.Log($"[CalculateRawDamage] START → {caster.EntityName} | {ability.Data.abilityName} ({effect.damageType})");

        float skillLevel = GetLinkedSkillLevel(caster, ability);
        float skillMultiplier = 1f + (skillLevel * attributeBonusPerPoint);
       // Debug.Log($"[CalculateRawDamage] Skill Multiplier: {skillMultiplier:F3}");

        float rawEffectDamage = effect.baseValue * effect.scalingFactor * skillMultiplier;
      //  Debug.Log($"[CalculateRawDamage] Base + Skill = {rawEffectDamage:F2}");

        var dmgAttr = AttributeHelper.GetDamageAttribute(caster, effect.damageType);
        if (dmgAttr != null)
        {
            float dmgBonus = dmgAttr.CurrentValue * attributeBonusPerPoint;
            rawEffectDamage *= (1f + dmgBonus);
         //   Debug.Log($"[CalculateRawDamage] Damage Attribute ({dmgAttr.AttributeName}) bonus: +{dmgBonus:P1}");
        }

        if (effect.primaryEffectAttribute != PrimaryAttributeType.None)
        {
            var primaryAttr = AttributeHelper.GetPrimaryAttribute(caster, effect.primaryEffectAttribute);
            if (primaryAttr != null)
            {
                float primaryBonus = primaryAttr.CurrentValue * attributeBonusPerPoint;
                rawEffectDamage *= (1f + primaryBonus);
             //   Debug.Log($"[CalculateRawDamage] Primary Attribute ({primaryAttr.AttributeName}) bonus: +{primaryBonus:P1}");
            }
        }

    //    Debug.Log($"[CalculateRawDamage] FINAL Raw Damage: {rawEffectDamage:F2}");
        return rawEffectDamage;
    }

    /// <summary>
    /// Applies all post-crit damage reductions (resistance + primary defence) and returns the final amount.
    /// Used by both real damage application AND potential damage calculation for Evasion XP.
    /// </summary>
    private float ApplyDamageReductions(BaseEntity target, float damageAfterCrit, DamageType damageType,
                                        PrimaryAttributeType defenceAttribute, bool logReductions = true)
    {
        float finalAmount = Mathf.Max(1f, damageAfterCrit);

        // Resistance Attribute
        var resistAttr = AttributeHelper.GetResistanceAttribute(target, damageType);
        if (resistAttr != null)
        {
            float resist = resistAttr.CurrentValue * attributeBonusPerPoint;
            finalAmount *= (1f - Mathf.Clamp01(resist));

            if (logReductions)
                Debug.Log($"[ApplyDamage] Resistance ({resistAttr.AttributeName}) reduction: -{resist:P1}");
        }

        // Primary Defence Attribute
        if (defenceAttribute != PrimaryAttributeType.None)
        {
            var primaryDefence = AttributeHelper.GetPrimaryAttribute(target, defenceAttribute);
            if (primaryDefence != null)
            {
                float defenceMod = primaryDefence.CurrentValue * attributeBonusPerPoint;
                finalAmount *= (1f - Mathf.Clamp01(defenceMod));

                if (logReductions)
                    Debug.Log($"[ApplyDamage] Primary Defence ({primaryDefence.AttributeName}) reduction: -{defenceMod:P1}");
            }
        }

        finalAmount = Mathf.Max(1f, finalAmount);
        return finalAmount;
    }

    private void ApplyDamage(BaseEntity target, float amount, DamageType damageType,
                           PrimaryAttributeType defenceAttribute, bool isCrit, BaseEntity attacker, ActiveAbility ability)
    {
        if (target?.Attributes?.Health == null) return;

        float finalAmount = ApplyDamageReductions(target, amount, damageType, defenceAttribute, logReductions: false);

//        Debug.Log($"[ApplyDamage] Incoming damage: {finalAmount:F2} | Type: {damageType}");
//        Debug.Log($"[ApplyDamage] Final damage after all reductions: {finalAmount:F2}");

        // === FLOATING DAMAGE TEXT ===
        if (FloatingTextManager.Instance != null)
        {
            Vector3 spawnPos = target.Body.HeadTransform.position;
            spawnPos.y += damageTextYOffset;
            FloatingTextManager.Instance.ShowDamage(spawnPos, finalAmount, isCrit);
        }

        // === Experience Gain ===
        if (attacker != null)
        {
            var dmgAttr = AttributeHelper.GetDamageAttribute(attacker, damageType);
            ApplyExperience(attacker, dmgAttr, finalAmount, $"{attacker.EntityName}'s {dmgAttr?.AttributeName}");

            ApplyLinkedSkillExperience(attacker, ability, finalAmount);
        }

        var resistAttr = AttributeHelper.GetResistanceAttribute(target, damageType); // for XP
        ApplyExperience(target, resistAttr, finalAmount, $"{target.EntityName}'s {resistAttr?.AttributeName}");

        // Apply Damage
        target.Attributes.Health.ChangeCurrentValueByAmount(-finalAmount);

        // Combat Log
        string logMsg = isCrit
            ? $"<color=yellow>CRITICAL!</color> {target.EntityName} took {finalAmount:F0} {damageType} damage!"
            : $"{target.EntityName} took {finalAmount:F0} {damageType} damage.";

        MessageManager.Instance?.Log(logMsg);

        if (target.Attributes.Health.CurrentValue <= 0f)
        {
            HandleDeath(attacker, target);
        }
    }

    /// <summary>
    /// Calculates the exact total damage that WOULD have been applied (used only for Evasion XP on dodge).
    /// Reuses ApplyDamageReductions so there is no duplicated reduction logic.
    /// </summary>
    private float CalculatePotentialTotalDamage(BaseEntity caster, BaseEntity target, ActiveAbility ability)
    {
        float total = 0f;

        foreach (var effect in ability.Data.effects)
        {
            if (effect == null || effect.effectType != AbilityEffectType.Damage) continue;

            float rawDamage = CalculateRawDamage(caster, ability, effect);
            bool isCrit = IsCriticalHit(caster);
            float damageAfterCrit = isCrit ? rawDamage * critMultiplier : rawDamage;

            float finalDamage = ApplyDamageReductions(target, damageAfterCrit, effect.damageType,
                                                    effect.primaryEffectDefenceAttribute, logReductions: false);

            total += finalDamage;
        }

        return total;
    }
    #endregion

    #region Death Handling
    private void HandleDeath(BaseEntity killer, BaseEntity victim)
    {
        if (victim == null || !victim.IsAlive) 
        {
            Debug.Log($"[CombatManager] HandleDeath skipped - victim null or already dead");
            return;
        }

        Debug.Log($"[CombatManager] HandleDeath called → {victim.EntityName} (Team {victim.EntityTeam}) Health: {victim.Attributes.Health.CurrentValue}");

        victim.Die(killer);

        if (killer is CharacterEntity charKiller && victim.Attributes != null)
        {
            XPManager.Instance.DistributeXP(killer, victim.Attributes.EntityPower);
        }
    }
    #endregion
}