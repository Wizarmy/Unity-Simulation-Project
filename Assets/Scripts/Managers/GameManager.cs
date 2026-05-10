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
    
    
    // Event for other systems to listen to combat state changes
    public event Action<bool> OnCombatStateChanged;
    
    public void InitializeGame()
    {
        ResizeGround();
        CreateEntities();
    }

    public void ToggleInCombatState()
    {
        IsInCombat = !IsInCombat;
        Debug.Log($"[SimulationManager] Combat {(IsInCombat ? "STARTED" : "STOPPED")}");
        OnCombatStateChanged?.Invoke(IsInCombat);
        

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