using System;
using UnityEngine;

[Serializable]
public struct MinMaxRange
{
    [Tooltip("Minimum range (0 = melee)")]
    public float Min;

    [Tooltip("Maximum range (0 = unlimited)")]
    public float Max;

    public MinMaxRange(float min = 0f, float max = 0f)
    {
        Min = Mathf.Max(0f, min);
        Max = Mathf.Max(min, max);   // Ensure Max is never smaller than Min
    }

    /// <summary>
    /// Returns true if the given distance is within this range.
    /// </summary>
    public bool IsInRange(float distance)
    {
        // If Max is 0, treat it as unlimited range
        if (Max <= 0f)
            return distance >= Min;

        return distance >= Min && distance <= Max;
    }

    /// <summary>
    /// Returns true if the target is within range from the source position.
    /// </summary>
    public bool IsInRange(Vector3 sourcePosition, Vector3 targetPosition)
    {
        float distance = Vector3.Distance(sourcePosition, targetPosition);
        return IsInRange(distance);
    }

    public override string ToString() => $"[{Min:F1} - {Max:F1}]";
}