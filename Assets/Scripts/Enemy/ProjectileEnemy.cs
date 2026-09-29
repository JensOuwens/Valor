using UnityEngine;

public class ProjectileEnemy : Enemy
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [SerializeField] private int damage = 15;
    [SerializeField] private float attackCooldown = 2f;

    private float attackTimer;

    protected override void UpdateAttack()
    {
        if (currentTarget == null)
            return;

        if (currentTargetTransform == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            currentTargetTransform.position
        );

        if (distance > attackRange)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    private void Attack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name} has no projectile prefab.");
            return;
        }

        Transform spawnPoint = projectileSpawnPoint != null
            ? projectileSpawnPoint
            : transform;

        GameObject projectile = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        Projectile projectileScript =
            projectile.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            projectileScript.Initialize(
                currentTarget,
                currentTargetTransform,
                damage
            );
        }
    }
}