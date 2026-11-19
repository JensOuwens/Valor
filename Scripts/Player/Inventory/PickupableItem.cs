namespace Valor;
using Godot;
using System;

public partial class PickupableItem : Area3D
{
	[Export] public ItemData ItemData { get; set; }

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
        
        if (ItemData == null ||  ItemData.Item_Model == null) return;
        
        Node3D instance = ItemData.Item_Model.Instantiate<Node3D>();
        AddChild(instance);

        GetNode<MeshInstance3D>("MeshInstance3D").Visible = false;
    }

	private void OnBodyEntered(Node3D body)
    {
        if (body is not PlayerScript player) return;
        player.inventory.AddToInventory(this.ItemData);
        player.Playerpickedup(this.ItemData.Item_Name);

        QueueFree();
    }
}
