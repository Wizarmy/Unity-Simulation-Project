using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    #region Singleton
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogWarning("UIManager.Instance accessed before Awake. Make sure the manager exists in the scene.");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        // No DontDestroyOnLoad() → destroyed when scene unloads
        
        InstantiateCharacterPanel();
        SetupInput();
    }
    #endregion

    [Header("Floating UI Prefabs")]
    [SerializeField] private FloatingNameText floatingNameTextPrefab;
    [SerializeField] private CharacterDisplayPanel characterDisplayPanelPrefab; 
    [SerializeField] private FloatingLifeBarDisplayPanel floatingDisplayPanelPrefab;

    [Header("Instantiated Panels")]
    private CharacterDisplayPanel _characterDisplayPanelInstance;
     
    private readonly Dictionary<BaseEntity, FloatingNameText> _activeNameTexts = 
        new Dictionary<BaseEntity, FloatingNameText>();
      
    private readonly Dictionary<BaseEntity, FloatingLifeBarDisplayPanel> _activeLifeDisplayPanels = 
        new Dictionary<BaseEntity, FloatingLifeBarDisplayPanel>();
    
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;   // ← Assign your Input Action Asset
    private InputAction _toggleCharacterPanelAction;
    
    #region Input Setup

    private void SetupInput()
    {
        if (inputActions == null)
        {
            Debug.LogWarning("InputActions asset not assigned in UIManager. Toggle with P will not work.");
            return;
        }

        _toggleCharacterPanelAction = inputActions.FindAction("UI/ToggleCharacterPanel"); // Adjust path if needed

        if (_toggleCharacterPanelAction != null)
        {
            _toggleCharacterPanelAction.performed += OnToggleCharacterPanel;
            _toggleCharacterPanelAction.Enable();
        }
        else
        {
            Debug.LogWarning("Action 'UI/ToggleCharacterPanel' not found in InputActions asset.");
        }
    }

    private void OnToggleCharacterPanel(InputAction.CallbackContext context)
    {
        if (_characterDisplayPanelInstance == null) return;

        // Only toggle if a valid entity is currently selected / tracked
        BaseEntity selectedEntity = GetCurrentlySelectedEntity(); // ← You need to implement this

        if (selectedEntity == null)
        {
            Debug.Log("Cannot toggle Character Panel: No entity selected.");
            return;
        }

        // Toggle logic
        if (IsCharacterPanelVisible())
        {
            HideCharacterPanel();
        }
        else
        {
            ShowCharacterPanel(selectedEntity);
        }
    }

    #endregion
    
    
    #region Character Display Panel
    
  
    private void InstantiateCharacterPanel()
    {
        if (characterDisplayPanelPrefab == null)
        {
            Debug.LogError("CharacterDisplayPanel Prefab is not assigned in UIManager!");
            return;
        }

        _characterDisplayPanelInstance = Instantiate(characterDisplayPanelPrefab);
        _characterDisplayPanelInstance.name = "Character Display Panel (Runtime)";
        
        _characterDisplayPanelInstance.panelRoot.SetActive(false); // Start hidden
    }
    
    public void ShowCharacterPanel(BaseEntity entity)
    {
        if (_characterDisplayPanelInstance == null)
        {
            Debug.LogWarning("CharacterDisplayPanel has not been instantiated!");
            return;
        }

        _characterDisplayPanelInstance.Show(entity);
    }

    public void ChangeCharacterPanelEntity(BaseEntity newEntity, bool forceRefresh = false)
    {
        _characterDisplayPanelInstance?.ChangeDisplayedEntity(newEntity, forceRefresh);
    }

    public void HideCharacterPanel()
    {
        _characterDisplayPanelInstance?.Hide();
    }

    public bool IsCharacterPanelVisible()
    {
        return _characterDisplayPanelInstance != null && 
               _characterDisplayPanelInstance.panelRoot.activeSelf;
    }
    
    #endregion
    
    private BaseEntity GetCurrentlySelectedEntity()
    {
        // Example implementations:

        // Option 1: If you have a Selection Manager
        // return SelectionManager.Instance?.CurrentSelectedEntity;

        // Option 2: If EntityManager has a "player" or "focused" entity
        // return EntityManager.Instance?.PlayerEntity;

        // Option 3: Temporary fallback - return first entity in list
        if (EntityManager.Instance?.EntityList.Count > 0)
            return EntityManager.Instance.EntityList[0];

        return null;
    }
    
    public void CreateFloatingDisplayPanel(BaseEntity entity)
    {
        if (entity == null) return;

        // Create Floating Name Text
        if (floatingDisplayPanelPrefab == null || _activeLifeDisplayPanels.ContainsKey(entity)) return;
        var floatingDisplayPanel = Instantiate(floatingDisplayPanelPrefab, entity.Body.transform);
        floatingDisplayPanel.transform.localPosition = new Vector3(0, 3f, 0); // Above the bars
        floatingDisplayPanel.InitializeFloatingLifeAttributeDisplayPanel(entity);
        _activeLifeDisplayPanels[entity] = floatingDisplayPanel;

    }
    
    public void CreateFloatingNameText(BaseEntity entity)
    {
        if (entity == null) return;

        // Create Floating Name Text
        if (floatingNameTextPrefab == null || _activeNameTexts.ContainsKey(entity)) return;
        var nameText = Instantiate(floatingNameTextPrefab, entity.Body.transform);
        nameText.transform.localPosition = new Vector3(0, 3f, 0); // Above the bars
        nameText.InitializeFloatingNameText(entity);
        _activeNameTexts[entity] = nameText;

    }
    
    public void RemoveFloatingNameText(BaseEntity entity)
    {

        if (!_activeNameTexts.TryGetValue(entity, out var nameText)) return;
        if (nameText != null) Destroy(nameText.gameObject);
        _activeNameTexts.Remove(entity);
    }
    
    public void RemoveFloatingDisplayPanel(BaseEntity entity)
    {

        if (!_activeLifeDisplayPanels.TryGetValue(entity, out var displayPanel)) return;
        if (displayPanel != null) Destroy(displayPanel.gameObject);
        _activeLifeDisplayPanels.Remove(entity);
    }

    public void ClearAllFloatingUI()
    {
        foreach (var entity in _activeNameTexts.Keys.ToList())
        {
            RemoveFloatingNameText(entity);
            RemoveFloatingDisplayPanel(entity);
        }
            
        _activeNameTexts.Clear();
        _activeLifeDisplayPanels.Clear();   
    }
    
    private void OnDestroy()
    {
        ClearAllFloatingUI();
    }
}