using UnityEngine;

[System.Serializable]
public struct EntityColor
{
    public Color body;
    public Color head;
    public Color nose;

    public static EntityColor DefaultAlly => new EntityColor
    {
        body = new Color(0.1f, 0.6f, 1f),      // Bright Blue
        head = new Color(0.95f, 0.85f, 0.7f),
        nose = new Color(1f, 0.55f, 0.2f)
    };

    public static EntityColor DefaultEnemy => new EntityColor
    {
        body = new Color(0.9f, 0.2f, 0.2f),    // Strong Red
        head = new Color(0.95f, 0.7f, 0.65f),
        nose = new Color(1f, 0.9f, 0.3f)       // Yellowish accent
    };

    /// <summary>
    /// Generate random but visually pleasing colors
    /// </summary>
    public static EntityColor Random(bool isEnemy)
    {
        var hueMin = isEnemy ? 0f : 0.5f;   // Red side vs Blue side
        var hueMax = isEnemy ? 0.15f : 0.65f;

        return new EntityColor
        {
            body = UnityEngine.Random.ColorHSV(hueMin, hueMax, 0.7f, 1f, 0.8f, 1f),
            head = UnityEngine.Random.ColorHSV(hueMin,hueMax, 0.6f, 1f, 0.7f, 1f),
            nose = UnityEngine.Random.ColorHSV(0.05f, 0.1f, 0.8f, 1f, 0.9f, 1f)
        };
    }
}