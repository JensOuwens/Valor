using Godot;
using System;
using Valor;

public partial class MeleeEnemy : BaseEnemy
{
	private Node3D Player;

	public override void _Ready()
	{
		base._Ready();

		Player = GetTree().GetFirstNodeInGroup("Player") as Node3D;
	}
}
