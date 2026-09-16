using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Moving, Attacking, Dead }
    public State currentState = State.Idle;

    [Header("Movement")]
    public float moveSpeed = 1.2f;
    public float stopDistance = 1.3f;
    public float obstacleAvoidanceRadius = 0.5f;
    public bool isFlying = false;
    public float flyHeight = 1.2f;

    [Header("Combat")]
    public float attackDamage = 15f;
    public float attackCooldown = 2.5f;
    private float _lastAttackTime;

    private Transform _player;
    private ARSceneManager _arSceneManager;
    private Animator _animator;
    private EnemyStats _stats;

    private void Start()
    {
        if (Camera.main != null) _player = Camera.main.transform;
        else _player = FindFirstObjectByType<AudioListener>()?.transform;

        _arSceneManager = FindFirstObjectByType<ARSceneManager>();
        _animator = GetComponent<Animator>();
        _stats = GetComponent<EnemyStats>();
        _lastAttackTime = -attackCooldown;
    }

    private void Update()
    {
        if (currentState == State.Dead) return;

        if (_stats != null && _stats.IsDead)
        {
            currentState = State.Dead;
            return;
        }

        if (_player == null)
        {
            if (Camera.main != null) _player = Camera.main.transform;
            return;
        }

        if (ARGameManager.Instance != null && ARGameManager.Instance.currentState != GameState.Gameplay)
        {
            PlayAnimation("IdleBattle");
            return;
        }

        Vector3 flatPlayerPos = _player.position;
        if (!isFlying) flatPlayerPos.y = transform.position.y;
        float distanceToPlayer = Vector3.Distance(transform.position, flatPlayerPos);

        switch (currentState)
        {
            case State.Idle:
                if (distanceToPlayer > stopDistance)
                {
                    currentState = State.Moving;
                }
                else
                {
                    FaceTarget(_player.position);
                    TryAttack();
                }
                break;

            case State.Moving:
                if (distanceToPlayer <= stopDistance)
                {
                    currentState = State.Idle;
                    PlayAnimation("IdleBattle");
                }
                else
                {
                    MoveTowardsPlayer();
                }
                break;

            case State.Attacking:
                if (Time.time > _lastAttackTime + 1.0f)
                {
                    currentState = State.Idle;
                }
                break;
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 targetPos = _player.position;
        if (!isFlying)
        {
            targetPos.y = transform.position.y;
        }
        else
        {
            targetPos.y = Mathf.Max(targetPos.y, flyHeight);
        }

        Vector3 direction = (targetPos - transform.position).normalized;
        Vector3 avoidance = CalculateAvoidance();
        direction = (direction + avoidance).normalized;

        if (direction != Vector3.zero)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 6f * Time.deltaTime);
        }

        PlayAnimation("WalkFWD");
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 8f * Time.deltaTime);
        }
    }

    private Vector3 CalculateAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        Collider[] obstacles = Physics.OverlapSphere(transform.position, obstacleAvoidanceRadius);
        foreach (var obs in obstacles)
        {
            if (obs.transform == transform || obs.transform == _player || obs.isTrigger) continue;
            
            Vector3 diff = transform.position - obs.transform.position;
            if (diff.sqrMagnitude > 0.001f)
            {
                avoidance += diff.normalized / diff.magnitude;
            }
        }

        return avoidance;
    }

    private void TryAttack()
    {
        if (Time.time > _lastAttackTime + attackCooldown)
        {
            _lastAttackTime = Time.time;
            currentState = State.Attacking;
            PlayAnimation("Attack01");
            
            var playerStats = PlayerStats.Instance != null ? PlayerStats.Instance : _player.GetComponentInParent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(attackDamage);
                Debug.Log($"[SAO] {gameObject.name} attacked player for {attackDamage} damage!");
            }
        }
    }

    private void PlayAnimation(string stateName)
    {
        if (_animator != null && _animator.HasState(0, Animator.StringToHash(stateName)))
        {
            _animator.CrossFade(stateName, 0.2f);
        }
    }
}
