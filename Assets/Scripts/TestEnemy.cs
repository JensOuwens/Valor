using System;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IDamagable
{
    public int health { get; set; }
    public int maxHealth { get; set; }
    public void Hurt(int damage)
    {
        health -= damage;
        Debug.Log(health);
    }

    private void Awake()
    {
        maxHealth = 100;
        health =  maxHealth;
    }
}
