using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Enemy : MonoBehaviour, IDamagable
{
    [Header("Health")]
    [SerializeField] protected int startingHealth = 100;

    public int health { get; set; }
    public int maxHealth { get; set; }

    [Header("Detection")]
    [SerializeField] protected float detectionRadius = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected LayerMask detectionLayers;

    [Header("Movement")]
    [SerializeField] protected float movementSpeed = 3.5f;

    protected NavMeshAgent agent;
    protected IDamagable currentTarget;
    protected Transform currentTargetTransform;

    // Keeps track of enemies that currently exist.
    private static readonly List<Enemy> allEnemies = new();

    protected virtual void Awake()
    {
        health = startingHealth;
        maxHealth = startingHealth;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = movementSpeed;
    }

    protected virtual void OnEnable()
    {
        allEnemies.Add(this);
    }

    protected virtual void OnDisable()
    {
        allEnemies.Remove(this);
    }

    protected virtual void Update()
    {
        if (currentTarget == null)
        {
            SearchForTarget();
        }

        if (currentTarget != null)
        {
            ChaseTarget();
        }

        UpdateAttack();
    }

    // ------------------------------------------------------------------
    // DAMAGE / DEATH
    // ------------------------------------------------------------------

    public virtual void Hurt(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    // ------------------------------------------------------------------
    // DETECTION
    // ------------------------------------------------------------------

    protected virtual void SearchForTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            detectionLayers
        );

        foreach (Collider hit in hits)
        {
            IDamagable damageable = hit.GetComponent<IDamagable>();

            if (damageable == null)
                continue;

            // Don't target ourselves.
            if (damageable == this)
                continue;

            // This is something we can attack.
            SetTarget(damageable, hit.transform);

            // Tell nearby enemies that we found a target.
            AlertNearbyEnemies(damageable, hit.transform);

            break;
        }
    }

    protected void SetTarget(IDamagable target, Transform targetTransform)
    {
        currentTarget = target;
        currentTargetTransform = targetTransform;
    }

    // Called when another enemy sees something.
    public virtual void ReceiveAlert(IDamagable target, Transform targetTransform)
    {
        if (currentTarget != null)
            return;

        if (target == this)
            return;

        float distance = Vector3.Distance(transform.position, targetTransform.position);

        if (distance <= detectionRadius)
        {
            SetTarget(target, targetTransform);
        }
    }

    protected virtual void AlertNearbyEnemies(
        IDamagable target,
        Transform targetTransform)
    {
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy == this)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance <= detectionRadius)
            {
                enemy.ReceiveAlert(target, targetTransform);
            }
        }
    }

    // ------------------------------------------------------------------
    // MOVEMENT
    // ------------------------------------------------------------------

    protected virtual void ChaseTarget()
    {
        if (currentTargetTransform == null)
        {
            ClearTarget();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            currentTargetTransform.position
        );

        if (distance > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(currentTargetTransform.position);
        }
        else
        {
            agent.isStopped = true;

            Vector3 direction =
                currentTargetTransform.position - transform.position;

            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    protected void ClearTarget()
    {
        currentTarget = null;
        currentTargetTransform = null;

        agent.isStopped = false;
    }

    // ------------------------------------------------------------------
    // ATTACK
    // ------------------------------------------------------------------

    protected abstract void UpdateAttack();

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}