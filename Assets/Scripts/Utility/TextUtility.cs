using UnityEngine;

public static class TextUtility
{
    /// <summary>
    /// Smart float formatting:
    /// - Whole numbers → no decimal point
    /// - Non-whole numbers → up to 'maxDecimalPlaces', but trims trailing zeros
    ///   Example: FormatSmart(3.10f, 2) → "3.1"
    ///            FormatSmart(3.14f, 2) → "3.14"
    ///            FormatSmart(3.00f, 2) → "3"
    /// </summary>
    public static string FormatSmart(float value, int maxDecimalPlaces = 2)
    {
        if (maxDecimalPlaces < 0)
            maxDecimalPlaces = 0;

        // If it's effectively a whole number, always return as integer
        if (Mathf.Approximately(value % 1f, 0f))
            return value.ToString("F0");

        // Format with the maximum allowed decimals
        var formatted = value.ToString($"F{maxDecimalPlaces}");

        // Trim trailing zeros and the decimal point if nothing remains after it
        if (formatted.Contains('.'))
        {
            formatted = formatted.TrimEnd('0').TrimEnd('.');
        }

        return formatted;
    }
}