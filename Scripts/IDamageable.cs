using Godot;
using System;

/// <summary>
/// interface to signify this class is able to take damage
/// </summary>
public interface IDamageable
{
	public float health { get; set; }

	public void Hurt(float Damage);
}
