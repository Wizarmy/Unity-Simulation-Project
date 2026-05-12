using System;

public static class ResourceUtility
{
    /// <summary>
    /// Checks if the caster can afford the resource cost.
    /// </summary>
    /// /// <param name="caster"></param>
    /// <param name="cost"></param>
    /// <param name="onInsufficientResources">Optional callback invoked when any resource is missing.</param>
    
    public static bool CanAfford(BaseEntity caster, ResourceCost cost, Action<string> onInsufficientResources = null)
    {
        if (cost.IsZero || caster?.Attributes == null) 
            return true;

        // Health check
        if (cost.Health > 0f)
        {
            var health = caster.Attributes.Health;
            if (health != null && health.CurrentValue < cost.Health)
            {
                onInsufficientResources?.Invoke($"Not enough Health. Required: {cost.Health}, Available: {health.CurrentValue}");
                return false;
            }
        }

        // Energy check
        if (cost.Energy > 0f)
        {
            var energy = caster.Attributes.Energy;
            if (energy != null && energy.CurrentValue < cost.Energy)
            {
                onInsufficientResources?.Invoke($"Not enough Energy. Required: {cost.Energy}, Available: {energy.CurrentValue}");
                return false;
            }
        }

        // Mana check
        if (cost.Mana > 0f)
        {
            var mana = caster.Attributes.Mana;
            if (mana != null && mana.CurrentValue < cost.Mana)
            {
                onInsufficientResources?.Invoke($"Not enough Mana. Required: {cost.Mana}, Available: {mana.CurrentValue}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Applies the resource cost to the caster.
    /// </summary>
    /// /// <param name="caster"></param>
    /// <param name="cost"></param>
    /// <param name="onResourceDepleted">Optional callback when a resource is actually deducted.</param>
    public static void ApplyResourceCost(BaseEntity caster, ResourceCost cost, Action<string> onResourceDepleted = null)
    {
        if (cost.IsZero || caster?.Attributes == null) 
            return;

        if (cost.Health > 0f)
        {
            var health = caster.Attributes.Health;
            if (health != null)
            {
                health.ChangeCurrentValueByAmount(-cost.Health);
                onResourceDepleted?.Invoke($"Health -{cost.Health}");
            }
        }

        if (cost.Energy > 0f)
        {
            var energy = caster.Attributes.Energy;
            if (energy != null)
            {
                energy.ChangeCurrentValueByAmount(-cost.Energy);
                onResourceDepleted?.Invoke($"Energy -{cost.Energy}");
            }
        }

        if (cost.Mana > 0f)
        {
            var mana = caster.Attributes.Mana;
            if (mana != null)
            {
                mana.ChangeCurrentValueByAmount(-cost.Mana);
                onResourceDepleted?.Invoke($"Mana -{cost.Mana}");
            }
        }
    }
}