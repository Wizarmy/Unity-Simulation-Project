using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float maxHeadTurnAngle = 75f;

    [Header("AI Links")]
    [field: SerializeField] public BaseEntity ParentEntity { get; protected set; }
    [field: SerializeField] public Transform BodyTransform { get; protected set; }
    [field: SerializeField] public float CurrentMovementSpeed { get; protected set; } = 5f;
    [field: SerializeField] public float CurrentTurnSpeed { get; protected set; } = 120f;
    [field: SerializeField] public Transform HeadTransform { get; protected set; }
    [field: SerializeField] public int Team { get; protected set; }

    public List<ActiveAbility> Abilities => ParentEntity?.ActiveAbilities;

    // States
    private AIState _currentState;
    private readonly IdleState _idleState = new IdleState();
    private readonly ChaseState _chaseState = new ChaseState();
    private readonly EngageState _engageState = new EngageState();

    [field: SerializeField] public BaseEntity CombatTarget { get; private set; }

    private Coroutine _targetSearchCoroutine;

    public void Initialize(BaseEntity entity)
    {
        ParentEntity = entity;
        Team = entity.EntityTeam;

        HeadTransform = ParentEntity.Body.HeadTransform;
        BodyTransform = ParentEntity.Body.transform;

        CurrentMovementSpeed = 5f;
        CurrentTurnSpeed = 120f;

        GameManager.Instance.OnCombatStateChanged += OnCombatStateChanged;

        _idleState.Initialize(this);
        _chaseState.Initialize(this);
        _engageState.Initialize(this);

        EnterState(_idleState);
    }

    private void OnCombatStateChanged(bool inCombat)
    {
        if (ParentEntity == null || !ParentEntity.IsAlive)
            return;

        if (inCombat)
        {
            EnterState(_chaseState);
            StartTargetSearch();
        }
        else
        {
            CombatTarget = null;
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
        try { StopCoroutine(_targetSearchCoroutine); } catch { }
        _targetSearchCoroutine = null;
    }

    private IEnumerator TargetSearchRoutine()
    {
        while (true)
        {
            FindNearestEnemy();
            yield return new WaitForSeconds(0.8f);   // Slightly faster than before
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

        MessageManager.Instance.Log($"[EntityAI] {ParentEntity.EntityName} → {_currentState.GetType().Name}");
    }

    private void Update()
    {
        if (ParentEntity == null || !ParentEntity.IsAlive)
        {
            enabled = false;
            return;
        }
        _currentState?.Update();
    }

    // ====================== Core Methods ======================

    public void LookAtTarget(Vector3 targetPosition)
    {
        if (BodyTransform)
        {
            var dir = (targetPosition - BodyTransform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                var targetRot = Quaternion.LookRotation(dir);
                BodyTransform.rotation = Quaternion.RotateTowards(
                    BodyTransform.rotation, targetRot, CurrentTurnSpeed * Time.deltaTime);
            }
        }

        if (!HeadTransform) return;

        var headDir = (targetPosition - HeadTransform.position).normalized;
        var targetHeadRot = Quaternion.LookRotation(headDir);
        var bodyRot = BodyTransform.rotation;
        var relativeRot = Quaternion.Inverse(bodyRot) * targetHeadRot;

        var euler = relativeRot.eulerAngles;
        var vertical = euler.x > 180f ? euler.x - 360f : euler.x;
        vertical = Mathf.Clamp(vertical, -45f, 45f);

        var horizontal = euler.y > 180f ? euler.y - 360f : euler.y;
        if (Mathf.Abs(horizontal) > maxHeadTurnAngle)
            horizontal = Mathf.Sign(horizontal) * maxHeadTurnAngle;

        relativeRot = Quaternion.Euler(vertical, horizontal, 0f);
        var finalHeadRot = bodyRot * relativeRot;

        HeadTransform.rotation = Quaternion.RotateTowards(
            HeadTransform.rotation, finalHeadRot, CurrentTurnSpeed * 1.5f * Time.deltaTime);
    }

    public void MoveTowardsTarget(BaseEntity target)
    {
        if (!target) return;

        var direction = target.Body.transform.position - BodyTransform.position;
        var distance = direction.magnitude;
        if (distance <= 3f) return;

        var moveDir = direction.normalized;
        BodyTransform.position += moveDir * (CurrentMovementSpeed * Time.deltaTime);
    }

    public void BackAway(BaseEntity target)
    {
        if (!target) return;

        var direction = target.Body.transform.position - BodyTransform.position;
        var moveDir = -direction.normalized;
        BodyTransform.position += moveDir * (CurrentMovementSpeed * 0.7f * Time.deltaTime);
    }

    public void FindNearestEnemy()
    {
        CombatTarget = null;
        var closestDistance = float.MaxValue;

        foreach (var entity in EntityManager.Instance.EntityList)
        {
            if (!entity || !entity.IsAlive || entity == ParentEntity || entity.EntityTeam == Team)
                continue;

            var distance = Vector3.Distance(BodyTransform.position, entity.Body.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                CombatTarget = entity;
            }
        }
    }

    // ====================== Reset Helpers ======================

    public void ForceIdle()
    {
        if (ParentEntity == null) return;

        CombatTarget = null;
        StopTargetSearch();
        EnterState(_idleState);

        Debug.Log($"[EntityAI] ForceIdle called on {ParentEntity.EntityName}");
    }
}