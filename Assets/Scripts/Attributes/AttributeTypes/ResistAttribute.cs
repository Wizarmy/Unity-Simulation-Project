using UnityEngine;

public class ResistAttribute : BaseAttribute
{
    protected override void Awake()
    {
        base.Awake();
        AttributeType = AttributeTypeEnum.Resist;
    
    }
}
