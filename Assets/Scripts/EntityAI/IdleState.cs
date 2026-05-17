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
        // NEW: Full safety guard
        if (AI == null || AI.ParentEntity == null || !AI.ParentEntity.IsAlive)
        {
            _nextLookTime = Time.time + 10f; // prevent spam
            return;
        }

        // Prefer looking at a living combat target if available
        if (AI.CombatTarget != null && AI.CombatTarget.IsAlive)
        {
            _currentLookTarget = AI.CombatTarget.Body.HeadTransform.position;
        }
        else
        {
            // Safe random look (no dead targets)
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y) * 0.6f + 0.2f; // bias upward

            _currentLookTarget = AI.BodyTransform.position + randomDir.normalized * Random.Range(6f, 18f);
        }

        _nextLookTime = Time.time + Random.Range(MinLookTime, MaxLookTime);
    }
}