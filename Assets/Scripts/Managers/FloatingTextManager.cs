using UnityEngine;
using System.Collections.Generic;

public class FloatingTextManager : MonoBehaviour
{
    private static FloatingTextManager _instance;
    public static FloatingTextManager Instance => _instance;

    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private int initialPoolSize = 60;
    [SerializeField] private int maxPoolSize = 200;

    private readonly Queue<FloatingText> _pool = new Queue<FloatingText>();
    [SerializeField] private Canvas worldCanvas;

    // Cached camera for performance
    public Camera MainCamera { get; private set; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        MainCamera = Camera.main;

        if (MainCamera == null)
            Debug.LogWarning("[FloatingTextManager] No Main Camera found in scene!");

        PrewarmPool();
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewText();
        }
    }

    private FloatingText CreateNewText()
    {
        var ft = Instantiate(floatingTextPrefab, worldCanvas ? worldCanvas.transform : transform);
        ft.gameObject.SetActive(false);
        _pool.Enqueue(ft);
        return ft;
    }

    /// <summary>
    /// Show damage text (red, bigger on crit)
    /// </summary>
    public void ShowDamage(Vector3 worldPos, float damage, bool isCrit = false)
    {
        if (floatingTextPrefab == null) return;

        string text = $"{Mathf.RoundToInt(damage)}";
        Color color = new Color(1f, 0.25f, 0.25f); // Default red

        var ft = GetFromPool();
        ft.Activate(text, color, worldPos, isCrit);
    }
    
    /// <summary>
    /// New: Show "Miss" text
    /// </summary>
    public void ShowMiss(Vector3 worldPos)
    {
        if (floatingTextPrefab == null) return;

        string text = "MISS";
        Color color = new Color(0.6f, 0.8f, 1f); // Light blue / cyan - very visible but not aggressive

        var ft = GetFromPool();
        ft.Activate(text, color, worldPos, false);
    }

    /// <summary>
    /// Show healing text (green)
    /// </summary>
    public void ShowHeal(Vector3 worldPos, float healAmount)
    {
        if (floatingTextPrefab == null) return;

        string text = $"+{Mathf.RoundToInt(healAmount)}";
        Color color = new Color(0.3f, 1f, 0.4f); // Bright green

        var ft = GetFromPool();
        ft.Activate(text, color, worldPos, false);
    }

    private FloatingText GetFromPool()
    {
        if (_pool.Count == 0)
            CreateNewText();

        return _pool.Dequeue();
    }

    public void ReturnToPool(FloatingText text)
    {
        if (text == null) return;

        if (_pool.Count < maxPoolSize)
            _pool.Enqueue(text);
        else
            Destroy(text.gameObject);
    }
}