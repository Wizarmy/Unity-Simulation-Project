using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeDuration = 0.4f;

   [SerializeField] private CanvasGroup canvasGroup;
    private Vector3 _startPosition;
    private float _timer;
    private Camera _mainCamera;
    private Vector3 _originalScale;

    private void Awake()
    {
        _mainCamera = FloatingTextManager.Instance?.MainCamera;
        _originalScale = transform.localScale;
    }

    /// <summary>
    /// Show floating text (damage or heal)
    /// </summary>
    public void Activate(string text, Color color, Vector3 worldPosition, bool isCritical = false)
    {
        gameObject.SetActive(true);

        // Random slight horizontal offset
        transform.position = worldPosition + new Vector3(Random.Range(-0.4f, 0.4f), 1.8f, 0);
        _startPosition = transform.position;

        textMesh.text = text;
        textMesh.color = color;
        canvasGroup.alpha = 1f;
        _timer = 0f;

        // Critical hit visual boost
        if (isCritical)
        {
            transform.localScale = _originalScale * 1.85f;
        }
        else
        {
            transform.localScale = _originalScale;
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        // Float upward
        transform.position = _startPosition + new Vector3(0, _timer * floatSpeed, 0);

        // Billboard - Always face camera
        if (_mainCamera != null)
        {
            transform.LookAt(_mainCamera.transform.position);
            transform.Rotate(0, 180f, 0); // Fix text orientation
        }

        // Fade out at the end
        if (_timer > lifetime - fadeDuration)
        {
            float alpha = 1f - (_timer - (lifetime - fadeDuration)) / fadeDuration;
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        if (_timer >= lifetime)
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        transform.localScale = _originalScale; // Reset for pooling
        FloatingTextManager.Instance?.ReturnToPool(this);
    }
}