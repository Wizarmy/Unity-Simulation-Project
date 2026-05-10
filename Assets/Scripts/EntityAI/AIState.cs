public abstract class AIState
{
    protected EntityAI AI { get; private set; }

    public void Initialize(EntityAI ai) => AI = ai;

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}