namespace Valor;
using Godot;
using System;

public partial class PickupableItem : Area3D
{
	[Export] public ItemData ItemData { get; set; }

	public override void _Ready() => this.BodyEntered += OnBodyEntered;

	private void OnBodyEntered(Node3D body)
	{
		if (body is not PlayerScript player) return;
		player.inventory.AddToInventory(this.ItemData);
		player.Playerpickedup(this.ItemData.Item_Name);

		QueueFree();
	}
}
