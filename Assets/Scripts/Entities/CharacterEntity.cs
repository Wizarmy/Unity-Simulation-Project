using UnityEngine;

public class CharacterEntity : BaseEntity
{
    protected override void InitializeAttributes()
    {
        Attributes.SetCanLevelUp(true);
        MessageManager.Instance.Log($"[CharacterEntity] ✅ Initializing CharacterAttributes for {EntityName}");
    }
}