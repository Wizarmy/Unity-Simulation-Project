using System.Collections;
using UnityEngine;

public abstract class BaseEntity : MonoBehaviour
{
    [Header("Basic Information")]
    [field: SerializeField] public string EntityName { get; protected set; }
    [field: SerializeField] public int EntityTeam { get; protected set; }
    
    [Header("Avatar")]
    [field: SerializeField] public AvatarBody Body { get; protected set; }
    
    [Header("EntityAI")]
    [field: SerializeField] public EntityAI AI { get; protected set; }
    [Header("Attributes")]
    [field: SerializeField] public EntityAttributes Attributes { get; protected set; }

    public virtual void InitializeEntity(string entityName, AvatarBody avatarBody,
        int team = 0, float entityScale = 1f, bool useRandomColors = true)
    {
        this.name = EntityName = entityName;
        EntityTeam = team;
        
        SetupAvatar(avatarBody, entityScale, team);
        
        InitializeAttributes();
        
        //Wait 1-2 frames for attribute dependencies (DerivedAttribute, LifeAttribute, etc.)
        //to fully calculate their MaxValue before setting Life to full.
        StartCoroutine(DelayedSetLifeToMax());
        
        
        AI = gameObject.AddComponent<EntityAI>();
        AI.Initialize(this);

       
    }
    
    private IEnumerator DelayedSetLifeToMax()
    {
        yield return null;
        yield return null;

        if (!Attributes) yield break;
       
        Attributes.SetAllLifeAttributeToMax();
    }

    private void SetupAvatar(AvatarBody avatarBody, float scale, int team)
    {
        Body = avatarBody;
        avatarBody.transform.SetParent(transform);
        avatarBody.InitializeAvatar(scale, team);
    }

    protected abstract void InitializeAttributes();

    // ====================== CLEANUP ======================
    protected virtual void OnDestroy()
    {
        
        // Optional: clean up AI if needed
        if (AI != null)
        {
            Destroy(AI);
        }
    }
}