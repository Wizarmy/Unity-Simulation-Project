using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScene : MonoBehaviour
{
    [Header("Tutorial Spawn Positions")]
    [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0, 1, -10);
    [SerializeField] private Vector3 enemySpawnPosition = new Vector3(0, 1, 10);
    
    [Header("Entities")]
    private readonly List<BaseEntity> _tutorialEntities = new List<BaseEntity>();
    
    private BaseEntity _wizarmy;
    private BaseEntity _currentGuardian;
    
    [Header("Buttons")]
    [SerializeField] private Button returnToStartButton;
    [SerializeField] private Button toggleTutorialButton;
    
    [Header("Simulation Speed UI")]
    [SerializeField] private Slider simulationSpeedSlider;
    [SerializeField] private TMPro.TextMeshProUGUI speedValueText;
    
    [Header("Enemy Difficulty (Fixed per run)")]
    [SerializeField, Range(0, 50)] 
    private int enemyCultivationLevel = 15;

    private int _readyEntitiesCount;
    private bool _allEntitiesReady;
    private int _deathCount = 0;

    private void Awake()
    {
        SetupButtons();
        
        SimulationSettings.StartWithMainCharacter = false;
        SimulationSettings.FriendsCount = 0;
        SimulationSettings.EnemiesCount = 0;
        
        InitializeSpeedControl();
    }
    
    private void Start()
    {
        GameManager.Instance.SetSimulationSpeed(10f);
        SpawnTutorialSetup();
        
        Debug.Log("[Tutorial] Ready! Press 'Start Combat' to begin the fight.");
    }

    private void SetupButtons()
    {
        if (returnToStartButton != null)
            returnToStartButton.onClick.AddListener(OnReturnToStartClicked);
        
        if (toggleTutorialButton != null)
            toggleTutorialButton.onClick.AddListener(OnStartCombatButtonPressed);
    }

    public void InitializeSpeedControl()
    {
        if (simulationSpeedSlider != null)
        {
            simulationSpeedSlider.minValue = 0.1f;
            simulationSpeedSlider.maxValue = 200f;
            simulationSpeedSlider.value = GameManager.Instance.SimulationSpeed;
            simulationSpeedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
        }
        UpdateSpeedLabel();
    }

    private void OnSpeedSliderChanged(float newValue)
    {
        GameManager.Instance.SetSimulationSpeed(newValue);
        UpdateSpeedLabel();
    }

    private void UpdateSpeedLabel()
    {
        if (speedValueText != null)
            speedValueText.text = $"Simulation Speed: {GameManager.Instance.SimulationSpeed:F1}x";
    }

    public void SpawnTutorialSetup()
    {
        _tutorialEntities.Clear();
        _readyEntitiesCount = 0;
        _allEntitiesReady = false;

        // WIZARMY - Persistent
        if (_wizarmy == null)
        {
            EntityManager.Instance.CreateEntity("Wizarmy", playerSpawnPosition, 0, 1f, EntityType.Character);
            _wizarmy = EntityManager.Instance.FindEntity("Wizarmy");
            
            if (_wizarmy != null)
            {
                _wizarmy.IsPersistent = true;
                SubscribeToEntity(_wizarmy);
            }
        }
        else
        {
            SubscribeToEntity(_wizarmy);
            _wizarmy.FullReset(playerSpawnPosition);
        }

        // GUARDIAN
        if (_currentGuardian == null)
        {
            EntityManager.Instance.CreateEntity("Tutorial Guardian", enemySpawnPosition, 1, 1f, EntityType.Mob);
            _currentGuardian = EntityManager.Instance.FindEntity("Tutorial Guardian");
            
            if (_currentGuardian != null)
                SubscribeToEntity(_currentGuardian);
        }
        else
        {
            SubscribeToEntity(_currentGuardian);
            _currentGuardian.FullReset(enemySpawnPosition);
        }

        if (_currentGuardian != null)
            ApplyCultivationScaling(_currentGuardian);

        Debug.Log($"[Tutorial] Spawned/Reset {_tutorialEntities.Count} entities.");
    }

    private void SubscribeToEntity(BaseEntity entity)
    {
        if (entity == null) return;

        if (!_tutorialEntities.Contains(entity))
            _tutorialEntities.Add(entity);

        entity.OnEntitySetupComplete -= OnEntitySetupComplete;
        entity.OnEntitySetupComplete += OnEntitySetupComplete;

        entity.OnDied -= OnEntityDied;
        entity.OnDied += OnEntityDied;
        
        Debug.Log($"[Tutorial] Subscribed to {entity.EntityName}");
    }

    private void OnEntitySetupComplete(BaseEntity entity)
    {
        _readyEntitiesCount++;
        Debug.Log($"[Tutorial] {entity.EntityName} is ready ({_readyEntitiesCount}/{_tutorialEntities.Count})");

        if (_readyEntitiesCount >= _tutorialEntities.Count)
        {
            _allEntitiesReady = true;
            Debug.Log($"[Tutorial] ✅ ALL ENTITIES READY");
        }
    }

    private void OnEntityDied(BaseEntity deadEntity)
    {
        Debug.Log($"[Tutorial] {deadEntity.EntityName} died!");

        if (deadEntity == _wizarmy)
        {
            _deathCount++;
            Debug.Log($"Death #{_deathCount}");
            StartCoroutine(RespawnAndRestartCombat());
        }
        else if (deadEntity == _currentGuardian)
        {
            Debug.Log($"[Tutorial] 🎉 WIZARMY WON after {_deathCount} deaths!");
        }
    }

    private IEnumerator RespawnAndRestartCombat()
    {
        yield return new WaitForSecondsRealtime(0.01f);
        
        SpawnTutorialSetup();
        
        yield return new WaitUntil(() => _allEntitiesReady);
        
        yield return new WaitForSecondsRealtime(0.01f);

        Debug.Log($"[Tutorial] Auto-starting combat attempt #{_deathCount + 1}");
        GameManager.Instance.SetCombatState(true);   // Force combat ON
    }

    private void ApplyCultivationScaling(BaseEntity enemy)
    {
        if (enemy?.Attributes?.Cultivation == null) return;

        enemy.Attributes.Cultivation.AttributeLevel.SetLevel(enemyCultivationLevel);
        enemy.Attributes.Cultivation.AttributeLevel.ForceFullUpdate();
        enemy.Attributes.SetAllLifeAttributeToMax();

        Debug.Log($"[Tutorial] {enemy.EntityName} scaled to Cultivation Level {enemyCultivationLevel}");
    }

    public void OnStartCombatButtonPressed()
    {
        if (GameManager.Instance.IsInCombat) return;
        if (!_allEntitiesReady)
        {
            Debug.LogWarning("[Tutorial] Not all entities are ready yet!");
            return;
        }

        Debug.Log("[Tutorial] Starting Combat Manually");
        GameManager.Instance.SetCombatState(true);
    }

    private void OnReturnToStartClicked()
    {
        SceneManager.Instance.LoadScene("StartScene");
    }

    public void ForceResetTutorial()
    {
        SpawnTutorialSetup();
    }

    private void OnDestroy()
    {
        foreach (var entity in _tutorialEntities)
        {
            if (entity != null)
            {
                entity.OnEntitySetupComplete -= OnEntitySetupComplete;
                entity.OnDied -= OnEntityDied;
            }
        }

        if (returnToStartButton != null)
            returnToStartButton.onClick.RemoveListener(OnReturnToStartClicked);
        
        if (toggleTutorialButton != null)
            toggleTutorialButton.onClick.RemoveListener(OnStartCombatButtonPressed);
        
        if (simulationSpeedSlider != null)
            simulationSpeedSlider.onValueChanged.RemoveListener(OnSpeedSliderChanged);
    }
}