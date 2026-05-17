using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogWarning("GameManager.Instance accessed before Awake. Make sure the manager exists in the scene.");
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
    }
    #endregion
    
    [Header("Ground")]
    [SerializeField] private Transform groundPlane;
    [Header("CombatStatus")]
    
    [field:SerializeField] public bool IsInCombat { get; private set; }
    public int LastWinningTeam { get; private set; } = -1;
    
    [Header("Simulation Speed")]
    [SerializeField] private float _simulationSpeed = 10f;
    public float SimulationSpeed 
    { 
        get => _simulationSpeed; 
        set 
        {
            _simulationSpeed = Mathf.Clamp(value, 0.1f, 200f);
            Time.timeScale = _simulationSpeed;
            Debug.Log($"[GameManager] Simulation speed set to {_simulationSpeed}x");
        }
    }

    public void SetSimulationSpeed(float newSpeed)
    {
        SimulationSpeed = newSpeed;
    }

    [Header("Death Settings")]
    [field: SerializeField] public float DeathDestroyDelay { get; private set; } = 0.2f;
    
    public void EndCombat(int winningTeam = -1)
    {
        if (!IsInCombat) return;

        IsInCombat = false;
        LastWinningTeam = winningTeam; 
        OnCombatStateChanged?.Invoke(false);

        string message;
        if (winningTeam == 0)
            message = "Player Team Wins!";
        else if (winningTeam == 1)
            message = "Enemy Team Wins!";
        else
            message = "Draw - Everyone is dead!";

        Debug.Log($"[GameManager] Combat Ended! {message}");

        // Show on screen
        MessageManager.Instance.Log(message);

        // Optional: Disable AI on remaining entities, freeze input, etc.
    }
    
    
    // Event for other systems to listen to combat state changes
    public event Action<bool> OnCombatStateChanged;
    
    public void InitializeGame(bool spawnDefaultEntities = false)
    {
        ResizeGround();

        if (spawnDefaultEntities)
            CreateEntities();
    }

    public void ToggleInCombatState()
    {
        IsInCombat = !IsInCombat;
        if (!IsInCombat)
            SimulationSpeed = 1f;
        
        Debug.Log($"[SimulationManager] Combat {(IsInCombat ? "STARTED" : "STOPPED")}");
        OnCombatStateChanged?.Invoke(IsInCombat);
    }
    
    public void SetCombatState(bool inCombat)
    {
        if (IsInCombat == inCombat) 
        {
            Debug.Log($"[GameManager] Combat state already {inCombat}, no change.");
            return;
        }

        IsInCombat = inCombat;
        OnCombatStateChanged?.Invoke(inCombat);
    
        Debug.Log($"[GameManager] Combat state set to {(inCombat ? "STARTED" : "ENDED")}");
    }
    
    private void CreateEntities()
    {
        if (SimulationSettings.StartWithMainCharacter)
        {
            var mainPos = GetRandomSpawnPosition();
            EntityManager.Instance.CreateEntity(
                entityName: "Wizarmy",
                position: mainPos,
                team: 0,
                scale: 1.15f,
                entityType: EntityType.Character
            );
        }
        
        // Spawn Regular Friends
        for (var i = 0; i < SimulationSettings.FriendsCount; i++)
        {
            var pos = GetRandomSpawnPosition();
            EntityManager.Instance.CreateEntity($"Friend_{i+1}", pos, team: 0, entityType: EntityType.Mob);
        }

        // Spawn Enemies
        for (var i = 0; i < SimulationSettings.EnemiesCount; i++)
        {
            var pos = GetRandomSpawnPosition();
            EntityManager.Instance.CreateEntity($"Enemy_{i+1}", pos, team: 1, entityType: EntityType.Mob);
        }

        Debug.Log($"[SimulationManager] Spawned {SimulationSettings.FriendsCount} friends, {SimulationSettings.EnemiesCount} enemies. " +
                  $"Main Character: {SimulationSettings.StartWithMainCharacter}");
        
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        if (groundPlane == null)
            return new Vector3(Random.Range(-20f, 20f), 1f, Random.Range(-20f, 20f));

        var size = groundPlane.localScale * 10f;
        var halfX = size.x * 0.45f;
        var halfZ = size.z * 0.45f;

        return new Vector3(
            Random.Range(-halfX, halfX),
            1f,
            Random.Range(-halfZ, halfZ)
        );
    }
    
    private void ResizeGround()
    {
        var mainCharacterCount = SimulationSettings.StartWithMainCharacter ? 1 : 0;
        var totalEntities = SimulationSettings.FriendsCount + SimulationSettings.EnemiesCount + mainCharacterCount;

        if (groundPlane == null) return;

        const int minSide = 40;
        var desiredSide = Mathf.Max(minSide, Mathf.Sqrt(totalEntities) * 20f + 10f);

        var scaleFactor = desiredSide / 10f;
        groundPlane.localScale = new Vector3(scaleFactor, 1f, scaleFactor);
    }
    
}