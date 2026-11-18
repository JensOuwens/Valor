using Godot;
using System;
using Valor;

/// <summary>
/// Handles player inventory management
/// TODO: add inventory displaying
/// </summary>
public partial class PlayerInventory
{
	private Godot.Collections.Array<ItemData> items  = new Godot.Collections.Array<ItemData>();
	private RichTextLabel targetLabel;
	public PlayerInventory(RichTextLabel targetLabel)
	{
		this.targetLabel = targetLabel;
	}

	public void AddToInventory(ItemData item)
	{
		items.Add(item);
		DisplayInventory();
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

		DisplayInventory();
	}

	public bool IsInInvetory(string ItemName)
	{
		foreach (ItemData item in items)
		{
			if (item.Item_Name == ItemName)
			{
				return true;
				break;
			}
		}

		return false;
	}
	
	public void DebugInventory()
	{
		GD.Print("Debug Inventory");
		foreach (ItemData item in items)
		{
			GD.Print(item.Item_Name);
		}
	}

	public void DisplayInventory()
	{
		targetLabel.Text = "";
		foreach (ItemData item in items)
		{
			targetLabel.Text += item.Item_Name + "\n";
		}
	}
}
