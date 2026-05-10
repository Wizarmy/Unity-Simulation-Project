using TMPro;
using UnityEngine;

public class FloatingNameText : MonoBehaviour
{
    private Transform _mainCameraTransform;
    [SerializeField] private TMP_Text nameText; 
    
    private BaseEntity _attachedEntity;

    [Header("Name Positioning")]
    [SerializeField, Tooltip("How much extra height above the very top of the head")]
    private float extraHeightAboveHead = 0.5f;

    public void InitializeFloatingNameText(BaseEntity attachedToEntity)
    {
        _attachedEntity = attachedToEntity;
        SetName(attachedToEntity.EntityName);

        _mainCameraTransform = Camera.main ? Camera.main.transform : null;

        UpdatePositionAboveHead();   // Initial position
        
        if (_attachedEntity?.Body != null)
            _attachedEntity.Body.OnScaleChanged += OnEntityScaleChanged;
    }

    private void OnEntityScaleChanged(float newScale)
    {
        UpdatePositionAboveHead();
    }

    /// <summary>
    /// Places the name text exactly 0.5f (or extraHeightAboveHead) above the TOP of the head.
    /// </summary>
    private void UpdatePositionAboveHead()
    {
        var head = _attachedEntity.Body.HeadTransform;
        
        head.TryGetComponent(out Renderer headRenderer);
        // bounds.max = the highest world-space point of the entire head mesh
        var topOfHeadWorld = headRenderer.bounds.max;

        // Add the desired extra height above the top of the head
        var targetWorldPos = topOfHeadWorld +  Vector3.up * extraHeightAboveHead;

        // Convert from world → local space of the Entity root (name is usually a child of the entity)
        var localPos = _attachedEntity.Body.transform.InverseTransformPoint(targetWorldPos);

        // Keep name perfectly centered (X=0, Z=0)
        transform.localPosition = new Vector3(0f, localPos.y, 0f);
    }

    public void SetName(string entityName)
    {
        if (nameText != null)
            nameText.text = entityName;
    }
    
    private void LateUpdate()
    {
        if (_mainCameraTransform)
        {
            transform.LookAt(transform.position + _mainCameraTransform.rotation * Vector3.forward,
                             _mainCameraTransform.rotation * Vector3.up);
        }
    }

    private void OnDestroy()
    {
        if (_attachedEntity?.Body != null)
            _attachedEntity.Body.OnScaleChanged -= OnEntityScaleChanged;
    }
}