using UnityEngine;

public interface IDamagable
{
    int health { get; set; }
    int maxHealth { get; set; }

    public void Hurt(int damage);

}
