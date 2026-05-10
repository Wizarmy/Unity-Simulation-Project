using UnityEngine;

public class EntityLevel : BaseAttribute
{
    protected override void Awake()
    {
        base.Awake();
        AttributeType = AttributeTypeEnum.EntityLevel;
    
    }
}
