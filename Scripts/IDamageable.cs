using Godot;
using System;

public interface IDamageable
{
	public float health { get; set; }

	public void Hurt(float Damage);
}
