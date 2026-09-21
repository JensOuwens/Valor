using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [SerializeField] private BoxCollider hitbox;
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private int damage = 25;

    private readonly HashSet<IDamagable> hitTargets = new();

    public void EnableHitbox()
    {
        hitTargets.Clear();
        hitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        CheckHitbox();
        hitbox.enabled = false;
    }

    private void CheckHitbox()
    {
        Debug.Log("Triggered");

        Vector3 center = hitbox.transform.TransformPoint(hitbox.center);
        Vector3 halfExtents = Vector3.Scale(hitbox.size, hitbox.transform.lossyScale) * 0.5f;

        Collider[] colliders = Physics.OverlapBox(
            center,
            halfExtents,
            hitbox.transform.rotation,
            damageableLayers
        );

        foreach (Collider collider in colliders)
        {
            Debug.Log("Found one: " + collider.name);

            IDamagable damageable = collider.GetComponentInParent<IDamagable>();

            if (damageable == null)
                continue;

            Debug.Log("Damageable good");

            if (hitTargets.Contains(damageable))
                continue;

            hitTargets.Add(damageable);
            damageable.Hurt(damage);
        }
    }
}