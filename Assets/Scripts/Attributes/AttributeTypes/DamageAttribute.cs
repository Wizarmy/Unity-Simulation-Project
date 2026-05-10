using UnityEngine;

public class DamageAttribute : BaseAttribute
{
    protected override void Awake()
    {
        base.Awake();
        AttributeType = AttributeTypeEnum.Damage;
    
    }
}
