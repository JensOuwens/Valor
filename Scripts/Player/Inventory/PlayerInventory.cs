using Godot;
using System;
using Valor;

/// <summary>
/// Handles player inventory management
/// </summary>
public partial class PlayerInventory
{
	private Godot.Collections.Array<ItemData> items  = new Godot.Collections.Array<ItemData>();

	public void AddToInventory(ItemData item)
	{
		items.Add(item);
	}
	
	public void RemoveFromInventory(string ItemName)
	{
		foreach (ItemData item in items)
		{
			if (item.Item_Name == ItemName)
			{
				items.Remove(item);
				break;
			}
		}
	}
	
	public void DebugInventory()
	{
		GD.Print("Debug Inventory");
		foreach (ItemData item in items)
		{
			GD.Print(item.Item_Name);
		}
	}
}
