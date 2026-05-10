using UnityEngine;

public class CharacterEntity : BaseEntity
{
    protected override void InitializeAttributes()
    {
        
        Attributes.SetCanLevelUp(true);
        Debug.Log($"[CharacterEntity] ✅ Initializing CharacterAttributes for {EntityName}");
      
    }
}