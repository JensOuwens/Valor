namespace Valor;
using Godot;
using System;

public partial class PickupableItem : Area3D
{
	[Export] public ItemData ItemData { get; set; }

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is PlayerScript player)
		{
			player.inventory.AddToInventory(ItemData);

			QueueFree();
		}
	}
}
