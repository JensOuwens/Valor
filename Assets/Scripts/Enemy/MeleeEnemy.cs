using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Header("Melee")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float attackCooldown = 1f;

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
        currentTarget.Hurt(damage);
    }
}