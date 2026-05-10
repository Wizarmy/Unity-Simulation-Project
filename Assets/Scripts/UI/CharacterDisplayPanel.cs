using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDisplayPanel : MonoBehaviour
{
    [Header("Main References")] 
    public GameObject panelRoot;
    [SerializeField] private TMP_Text entityNameText;
    [SerializeField] private TMP_Text totalPowerText;

    [Header("Navigation")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;

    [Header("Attribute Display Container")]
    [SerializeField] private Transform attributeContainer;

    [Header("Prefab")]
    [SerializeField] private AttributeInfoPanel attributeDisplayPrefab;

    private readonly List<AttributeInfoPanel> _activeDisplays = new List<AttributeInfoPanel>();
    private readonly Queue<AttributeInfoPanel> _pool = new Queue<AttributeInfoPanel>();

    private BaseEntity _currentEntity;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        SetupNavigationButtons();
        PrewarmPool(20); // Pre-create some panels
    }

    private void PrewarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var panel = Instantiate(attributeDisplayPrefab, attributeContainer);
            panel.gameObject.SetActive(false);
            _pool.Enqueue(panel);
        }
    }

    private void SetupNavigationButtons()
    {
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(ShowPreviousEntity);

        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(ShowNextEntity);
    }

    public void Show(BaseEntity entity)
    {
        if (entity == null) return;

        ChangeDisplayedEntity(entity);
        panelRoot.SetActive(true);
    }

    public void ChangeDisplayedEntity(BaseEntity newEntity, bool forceRefresh = false)
    {
        if (newEntity == null) 
            return;

        // Only skip if it's the same entity AND we don't want to force refresh
        if (newEntity == _currentEntity && !forceRefresh)
            return;

        UnsubscribeFromCurrentEntity();
        _currentEntity = newEntity;

        _currentEntity.Attributes.OnPowerChanged += UpdatePowerDisplay;

        RefreshDisplaysSmooth();
    }

    private void ShowPreviousEntity() => CycleEntity(-1);
    private void ShowNextEntity() => CycleEntity(1);

    private void CycleEntity(int direction)
    {
        var list = EntityManager.Instance.EntityList;
        if (list.Count == 0) return;

        int currentIndex = list.IndexOf(_currentEntity);
        int nextIndex = (currentIndex + direction + list.Count) % list.Count;

        ChangeDisplayedEntity(list[nextIndex]);
    }

    private void RefreshDisplaysSmooth()
    {
        bool wasVisible = panelRoot.activeSelf;
        panelRoot.SetActive(false);

        RefreshDisplays();

        ForceTopLeftPosition();

        panelRoot.SetActive(wasVisible);
    }

    private void RefreshDisplays()
    {
        if (_currentEntity == null) return;

        entityNameText.text = _currentEntity.EntityName;
        UpdatePowerDisplay(_currentEntity.Attributes.EntityPower);

        // Return old displays to pool
        foreach (var display in _activeDisplays)
        {
            if (display != null)
            {
                display.gameObject.SetActive(false);
                _pool.Enqueue(display);
            }
        }
        _activeDisplays.Clear();

        // Get displays from pool or create new
        foreach (var kvp in _currentEntity.Attributes.AttributeList)
        {
            AttributeInfoPanel display;

            if (_pool.Count > 0)
            {
                display = _pool.Dequeue();
                display.gameObject.SetActive(true);
            }
            else
            {
                display = Instantiate(attributeDisplayPrefab, attributeContainer);
            }

            display.gameObject.name = $"{kvp.AttributeName} Display";
            display.InitializeAttributeInfoPanel(kvp,false);
            _activeDisplays.Add(display);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)attributeContainer);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panelRoot.transform);
    }

    private void ForceTopLeftPosition()
    {
        if (panelRoot == null) return;

        var rect = (RectTransform)panelRoot.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, 0);
    }

    private void UpdatePowerDisplay(float newPower)
    {
        totalPowerText.text = $"Power: {newPower:F1}";
    }

    private void UnsubscribeFromCurrentEntity()
    {
        if (_currentEntity?.Attributes != null)
        {
            _currentEntity.Attributes.OnPowerChanged -= UpdatePowerDisplay;
        }
    }

    public void Hide()
    {
        UnsubscribeFromCurrentEntity();
        panelRoot.SetActive(false);
        _currentEntity = null;
    }

    private void OnDestroy()
    {
        if (leftArrowButton != null)
            leftArrowButton.onClick.RemoveListener(ShowPreviousEntity);
        if (rightArrowButton != null)
            rightArrowButton.onClick.RemoveListener(ShowNextEntity);

        Hide();
    }
}