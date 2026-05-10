using System.Collections;
using UnityEngine;

public class EntityAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float maxHeadTurnAngle = 75f;
    
    [SerializeField] private BaseEntity parentEntity;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform headTransform;

    private float _currentMovementSpeed = 5f;
    private float _currentTurnSpeed = 120f;

    private int _team;

    // States
    private AIState _currentState;
    private readonly IdleState _idleState = new IdleState();
    private readonly ChaseState _chaseState = new ChaseState();
    private readonly EngageState _engageState = new EngageState();

    public BaseEntity ParentEntity => parentEntity;
    public Transform HeadTransform => headTransform;
    public Transform BodyTransform => bodyTransform;
    public float CurrentMovementSpeed => _currentMovementSpeed;
    public float CurrentTurnSpeed => _currentTurnSpeed;
    public int Team => _team;
    [field: SerializeField] public BaseEntity CombatTarget { get; private set; }
   
    private Coroutine _targetSearchCoroutine;

    public void Initialize(BaseEntity entity)
    {
        parentEntity = entity;
        _team = entity.EntityTeam;
        
        headTransform= parentEntity.Body.HeadTransform;
        bodyTransform = parentEntity.Body.transform;


        _currentMovementSpeed = 5f;
        _currentTurnSpeed = 120f;
        
        GameManager.Instance.OnCombatStateChanged += OnCombatStateChanged;
        
        // Initialize states
        _idleState.Initialize(this);
        _chaseState.Initialize(this);
        _engageState.Initialize(this);

        EnterState(_idleState);
    }
    
    private void OnCombatStateChanged(bool inCombat)
    {
        if (inCombat)
        {
            EnterState(_chaseState);
            StartTargetSearch();
        }
        else
        {
            EnterState(_idleState);
            StopTargetSearch();
        }
    }

    private void StartTargetSearch()
    {
        _targetSearchCoroutine ??= StartCoroutine(TargetSearchRoutine());
    }

    private void StopTargetSearch()
    {
        if (_targetSearchCoroutine == null) return;
        StopCoroutine(_targetSearchCoroutine);
        _targetSearchCoroutine = null;
    }

    private IEnumerator TargetSearchRoutine()
    {
        while (true)           // Much better than while(true)
        {
            FindNearestEnemy();
            yield return new WaitForSeconds(1f);
        }
    }
    
    
    public void EnterIdle() => EnterState(_idleState);
    public void EnterChase() => EnterState(_chaseState);
    public void EnterEngage() => EnterState(_engageState);

    private void EnterState(AIState newState)
    {
        if (_currentState == newState) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();

        Debug.Log($"[EntityAI] {parentEntity.EntityName} → {_currentState.GetType().Name}");
    }

    private void Update()
    {
        _currentState?.Update();
    }

    // ====================== Core Methods (Used by States) ======================

    public void LookAtTarget(Vector3 targetPosition)
    {
        // Body - upright only
        if (bodyTransform)
        {
            var dir = (targetPosition - bodyTransform.position).normalized;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                var targetRot = Quaternion.LookRotation(dir);
                bodyTransform.rotation = Quaternion.RotateTowards(
                    bodyTransform.rotation, targetRot, _currentTurnSpeed * Time.deltaTime);
            }
        }

        // Head with limits
        if (!headTransform) return;

        var headDir = (targetPosition - headTransform.position).normalized;
        var targetHeadRot = Quaternion.LookRotation(headDir);

        var bodyRot = bodyTransform.rotation;
        var relativeRot = Quaternion.Inverse(bodyRot) * targetHeadRot;

        var euler = relativeRot.eulerAngles;
        var vertical = euler.x > 180f ? euler.x - 360f : euler.x;
        vertical = Mathf.Clamp(vertical, -45f, 45f);

        var horizontal = euler.y > 180f ? euler.y - 360f : euler.y;
        if (Mathf.Abs(horizontal) > maxHeadTurnAngle)
            horizontal = Mathf.Sign(horizontal) * maxHeadTurnAngle;

        relativeRot = Quaternion.Euler(vertical, horizontal, 0f);
        var finalHeadRot = bodyRot * relativeRot;

        headTransform.rotation = Quaternion.RotateTowards(
            headTransform.rotation, finalHeadRot, _currentTurnSpeed * 1.5f * Time.deltaTime);
    }

    public void MoveTowardsTarget(BaseEntity target)
    {
        if (!target) return;

        var direction = target.Body.transform.position - bodyTransform.position;
        var distance = direction.magnitude;

        if (distance <= 3f) return;

        var moveDir = direction.normalized;
        bodyTransform.position += moveDir * (_currentMovementSpeed * Time.deltaTime);
    }

    public void BackAway(BaseEntity target)
    {
        if (!target) return;

        var direction = target.Body.transform.position - bodyTransform.position;
        var moveDir = -direction.normalized;

        bodyTransform.position += moveDir * (_currentMovementSpeed * 0.7f * Time.deltaTime);
    }

    public void FindNearestEnemy()
    {
        CombatTarget = null;
        var closestDistance = float.MaxValue;

        foreach (var entity in EntityManager.Instance.EntityList)
        {
            if (!entity || entity == parentEntity || entity.EntityTeam == _team)
                continue;

            var distance = Vector3.Distance(bodyTransform.position, entity.Body.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                CombatTarget = entity;
            }
        }
    }
    
}