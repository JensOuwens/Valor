using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    private IDamagable target;
    private Transform targetTransform;
    private int damage;

    public void Initialize(
        IDamagable target,
        Transform targetTransform,
        int damage)
    {
        this.target = target;
        this.targetTransform = targetTransform;
        this.damage = damage;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (target == null || targetTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction =
            targetTransform.position - transform.position;

        if (direction.magnitude < 0.2f)
        {
            HitTarget();
            return;
        }

        transform.position +=
            direction.normalized * speed * Time.deltaTime;

        transform.forward = direction.normalized;
    }

    private void HitTarget()
    {
        target.Hurt(damage);
        Destroy(gameObject);
    }
}