public class CultivationAttribute : BaseAttribute
{
    protected override void Awake()
    {
        base.Awake();
        AttributeType=AttributeTypeEnum.Cultivation;
        AttributeLevel.SetLevelingCurve(1000000f,1.5f);
    }
    
}
