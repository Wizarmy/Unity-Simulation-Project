using System.Collections.Generic;
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
        
    }
}