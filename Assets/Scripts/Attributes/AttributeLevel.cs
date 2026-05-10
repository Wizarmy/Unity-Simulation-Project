using System;
using UnityEngine;

[Serializable]
public class AttributeLevel
{
    // ==================== CONFIGURATION ====================
    [field: SerializeField] public float InitialExpToNextLevel { get; protected set; } = 10f;
    [field: SerializeField] public float LevelExpMultiplier { get; protected set; } = 1.2f;

    private const int MAX_LEVEL_UP_ITERATIONS = 1000;
    private BaseAttribute _parentAttribute;

    // ==================== CURRENT STATE ====================
    [Header("Level Information")]
    [field: SerializeField] public int CurrentLevel { get; protected set; }

    [Header("Experience")]
    [field: SerializeField] public float CurrentLevelExp { get; protected set; }
    [field: SerializeField] public float ExpToNextLevel { get; protected set; }
    [field: SerializeField] public float TotalExpGained { get; protected set; }

    [Header("Progress")]
    [field: SerializeField, ReadOnly] public float ProgressPercentage { get; private set; }

    // ==================== EVENTS ====================
    public event Action OnLevelUp;
    public event Action OnExpChanged;

    // ==================== PROPERTIES ====================
    public float ProgressPercentageRaw => 
        ExpToNextLevel > 0 ? (CurrentLevelExp / ExpToNextLevel) * 100f : 0f;

    // ==================== CONSTRUCTORS ====================
    public AttributeLevel() => ResetToInitialCurve();

    public AttributeLevel(float initialExp, float multiplier = 1.2f)
        => SetLevelingCurve(initialExp, multiplier);

    public void InitilizeAttributeLevel(BaseAttribute parentAttribute)
    {
        _parentAttribute = parentAttribute;
    }

    // ==================== CURVE CONFIG ====================
    public void SetLevelingCurve(float initialExpToNextLevel, float levelExpMultiplier = 1.2f)
    {
        InitialExpToNextLevel = Mathf.Max(1f, initialExpToNextLevel);
        LevelExpMultiplier = Mathf.Max(1.01f, levelExpMultiplier);
        ResetToInitialCurve();

        Debug.Log($"[AttributeLevel] Curve updated → Initial: {InitialExpToNextLevel} | x{LevelExpMultiplier:F2}");
    }

    private void ResetToInitialCurve() => ExpToNextLevel = InitialExpToNextLevel;

    // ==================== STATIC UTILITIES ====================
    public static float CalculateExpRequiredForLevel(int level, float initialExp, float multiplier)
    {
        if (level <= 0) return initialExp;
        float exp = initialExp;
        for (int i = 0; i < level; i++)
            exp = Mathf.Ceil(exp * multiplier);
        return exp;
    }

    public static float CalculateTotalExpForLevel(int level, float initialExp, float multiplier)
    {
        if (level <= 0) return 0f;
        float total = 0f;
        float next = initialExp;
        for (int i = 0; i < level; i++)
        {
            total += next;
            next = Mathf.Ceil(next * multiplier);
        }
        return total;
    }

    // ==================== CORE LOGIC ====================
    private void LevelUpOnce()
    {
        CurrentLevelExp -= ExpToNextLevel;
        CurrentLevel++;
        ExpToNextLevel = Mathf.Ceil(ExpToNextLevel * LevelExpMultiplier);
    }

    private void ApplyExperience(float amount, bool resetProgress = false)
    {
        if (amount <= 0f) return;

        if (resetProgress)
        {
            CurrentLevel = 0;
            CurrentLevelExp = 0f;
            ResetToInitialCurve();
            TotalExpGained = amount;
        }
        else
        {
            TotalExpGained += amount;
        }

        CurrentLevelExp += amount;

        int iterations = 0;
        bool leveledUp = false;

        while (CurrentLevelExp >= ExpToNextLevel && ExpToNextLevel > 0f && iterations < MAX_LEVEL_UP_ITERATIONS)
        {
            iterations++;
            LevelUpOnce();
            leveledUp = true;
        }

        ProgressPercentage = ProgressPercentageRaw;
        OnExpChanged?.Invoke();

        if (leveledUp)
            OnLevelUp?.Invoke();

        if (iterations >= MAX_LEVEL_UP_ITERATIONS)
            Debug.LogWarning("[AttributeLevel] Max level-up iterations reached.");
    }

    // ==================== PUBLIC API ====================
    public void AddExperience(float amount) 
        => ApplyExperience(amount);

    public void SetTotalExpGained(float totalExp) 
        => ApplyExperience(Mathf.Max(0f, totalExp), resetProgress: true);

    public void SetLevel(int targetLevel)
    {
        CurrentLevel = Mathf.Max(0, targetLevel);
        CurrentLevelExp = 0f;
        ExpToNextLevel = CalculateExpRequiredForLevel(CurrentLevel, InitialExpToNextLevel, LevelExpMultiplier);
        TotalExpGained = CalculateTotalExpForLevel(CurrentLevel, InitialExpToNextLevel, LevelExpMultiplier);
        ProgressPercentage = ProgressPercentageRaw;

        OnExpChanged?.Invoke();
    }
}