using UnityEngine;

public class LifeAttribute : DerivedAttribute
{
    [Header("RegenAttribute")]
    [field:SerializeField] public BaseAttribute RegenAttribute { get; protected set; }

    private float _regenRate = 0f;           // Cached percent-per-second value
    private bool _isSubscribedToRegen = false;

    protected override void Awake()
    {
        base.Awake();
        AttributeType = AttributeTypeEnum.Life;

        SubscribeToRegenAttribute();
    }

    private void SubscribeToRegenAttribute()
    {
        if (RegenAttribute == null || _isSubscribedToRegen) return;
        RegenAttribute.CurrentValueChanged += OnRegenValueChanged;
        _isSubscribedToRegen = true;
        UpdateRegenRate(); // Initial sync
    }

    private void OnRegenValueChanged()
    {
        UpdateRegenRate();
    }

    private void UpdateRegenRate()
    {
        _regenRate = RegenAttribute ? RegenAttribute.CurrentValue : // This is already in percent per second
            0f;
    }

    protected override void UnsubscribeFromDependencies()
    {
        base.UnsubscribeFromDependencies();

        if (RegenAttribute != null && _isSubscribedToRegen)
        {
            RegenAttribute.CurrentValueChanged -= OnRegenValueChanged;
            _isSubscribedToRegen = false;
        }
    }

    private void FixedUpdate()
    {
        if (Mathf.Approximately(_regenRate, 0f) || MaxValue <= 0f)
            return;

        var deltaTime = Time.fixedDeltaTime;
        float percentPerSecond;

        // Over max → Degeneration (5x faster)
        if (CurrentValue > MaxValue)
        {
            percentPerSecond = -_regenRate * 5f;
        }
        // Below max → Normal Regeneration
        else if (CurrentValue < MaxValue)
        {
            percentPerSecond = _regenRate;
        }
        else
        {
            return; // Exactly at max → no change
        }

        var regenAmount = (percentPerSecond / 100f) * MaxValue * deltaTime;

        var newCurrentValue = CurrentValue + regenAmount;

        // Optional: Prevent going below 0 (uncomment if desired)
        // newCurrentValue = Mathf.Max(0f, newCurrentValue);

        SetCurrentValue(newCurrentValue);
    }
    
}