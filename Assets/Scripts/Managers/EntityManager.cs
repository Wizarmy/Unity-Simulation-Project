using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    #region Singleton
    private static EntityManager _instance;
    public static EntityManager Instance
    {
        get
        {
            if (!_instance)
            {
                Debug.LogWarning("EntityManager.Instance accessed before Awake. Make sure the manager exists in the scene.");
            }
            return _instance;
        }
    }

    [Header("Entity List")]
    [field: SerializeField] public List<BaseEntity> EntityList { get; private set; } = new List<BaseEntity>();

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
    
    [Header("Prefabs")]
    [SerializeField] private AvatarBody avatarBodyPrefab;
    [SerializeField] private CharacterEntity characterEntityPrefab;
    [SerializeField] private MobEntity mobEntityPrefab;
    
    public void CreateEntity(string entityName = "Entity",
        Vector3 position = default,
        int team = 0,
        float scale = 1f,
        EntityType entityType = EntityType.Mob)
    {

        BaseEntity entity;
        if (entityType == EntityType.Character)
        {
            entity = Instantiate(characterEntityPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            entity = Instantiate(mobEntityPrefab, Vector3.zero, Quaternion.identity);
        }
        
        var avatarBody = Instantiate(avatarBodyPrefab, position, Quaternion.identity);
    
        entity.transform.SetParent(transform);

        entity.InitializeEntity(entityName, avatarBody, team, scale);
    
        UIManager.Instance.CreateFloatingNameText(entity);
        UIManager.Instance.CreateFloatingDisplayPanel(entity);
    
        EntityList.Add(entity);
        entity.OnDied += HandleEntityDied;
        
    }
    
    public BaseEntity FindEntity(string entityName, bool caseSensitive = false)
    {
        if (string.IsNullOrEmpty(entityName))
            return null;

        foreach (var entity in EntityList)
        {
            if (entity == null || !entity.IsAlive)
                continue;

            bool matches = caseSensitive 
                ? entity.EntityName == entityName 
                : string.Equals(entity.EntityName, entityName, System.StringComparison.OrdinalIgnoreCase);

            if (matches)
                return entity;
        }

        return null;
    }
    
    public void CheckCombatEnd()
    {
        if (!GameManager.Instance.IsInCombat) 
        {
            Debug.Log("[EntityManager] CheckCombatEnd - Not in combat");
            return;
        }

        var aliveByTeam = new Dictionary<int, int>();

        Debug.Log($"[EntityManager] Checking combat end. Total entities in list: {EntityList.Count}");

        for (int i = EntityList.Count - 1; i >= 0; i--)
        {
            var entity = EntityList[i];
            if (entity == null || !entity.IsAlive)
            {
                Debug.Log($"[EntityManager] Removing dead/null entity: {(entity != null ? entity.EntityName : "null")}");
                EntityList.RemoveAt(i);
                continue;
            }

            if (!aliveByTeam.ContainsKey(entity.EntityTeam))
                aliveByTeam[entity.EntityTeam] = 0;

            aliveByTeam[entity.EntityTeam]++;
            Debug.Log($"[EntityManager] Alive: {entity.EntityName} (Team {entity.EntityTeam})");
        }

        int teamsWithAlive = aliveByTeam.Count;

        Debug.Log($"[EntityManager] Teams with alive entities: {teamsWithAlive} | Alive counts: {string.Join(", ", aliveByTeam.Select(kvp => $"Team{kvp.Key}:{kvp.Value}"))}");

        if (teamsWithAlive <= 1)
        {
            int winnerTeam = (teamsWithAlive == 1) ? aliveByTeam.First().Key : -1;
            Debug.Log($"[EntityManager] === COMBAT END CONDITION MET! Winner Team: {winnerTeam} ===");
            GameManager.Instance.EndCombat(winnerTeam);
        }
        else
        {
            Debug.Log("[EntityManager] Combat continues - multiple teams still alive");
        }
    }
    
    public void HandleEntityDied(BaseEntity entity)
    {
        if (entity == null) return;

        Debug.Log($"[EntityManager] Handling death of {entity.EntityName} (Team {entity.EntityTeam})");

        // IMPORTANT: Do NOT remove persistent entities (Wizarmy)
        if (!entity.IsPersistent)
        {
            EntityList.Remove(entity);
            Debug.Log($"[EntityManager] Removed non-persistent entity: {entity.EntityName}");
        }
        else
        {
            Debug.Log($"[EntityManager] Preserving persistent entity: {entity.EntityName} (still in list)");
        }

        CheckCombatEnd();
    }
}