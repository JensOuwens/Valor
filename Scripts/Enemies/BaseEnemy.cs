using Godot;
using System;
using Valor;

public partial class BaseEnemy : CharacterBody3D, IDamageable
{
	public float Health { get; set; }

	[Export] protected float MaxHealth { get; set; }
	[Export] protected float Damage { get; set; }
	[Export] protected float Firerate { get; set; }
	[Export] protected float Movespeed { get; set; }
	[Export] public float AggroRange { get; set; }

	protected CharacterBody3D Body;
	protected NavigationAgent3D NavAgent;

	public override void _Ready()
	{
		Health = MaxHealth;

		Body = this;
		NavAgent = Body.GetNode<NavigationAgent3D>("NavigationAgent3D");
	}
	
	public void Hurt(float damage, Vector3 hitDirection)
	{
		Health -= damage;
		
		GD.Print("Damage");

		if (Health <= 0)
			Die();
	}

	protected void Die()
	{
		QueueFree();
	}
}
