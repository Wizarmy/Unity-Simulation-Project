using UnityEngine;

public class IdleState : AIState
{
    private float _nextLookTime;
    
    private Vector3 _currentLookTarget;

    private const float MinLookTime = 2f;
    private const float MaxLookTime = 6f;

    public override void Enter()
    {
        ChooseNewLookTarget();
    }

    public override void Update()
    {
        if (Time.time >= _nextLookTime)
            ChooseNewLookTarget();

        AI.LookAtTarget(_currentLookTarget);
    }
    
    private void ChooseNewLookTarget()
    {
        var randomDir = Random.insideUnitSphere;
        randomDir.y = Mathf.Abs(randomDir.y) * 0.6f;

        _currentLookTarget = AI.BodyTransform.position + randomDir.normalized * Random.Range(6f, 18f);
        _nextLookTime = Time.time + Random.Range(MinLookTime, MaxLookTime);
    }
}