namespace Valor;

using Godot;
using System;

[GlobalClass]
public partial class ItemData : Resource
{
	[Export] public string Item_Name;
	[Export] public Texture2D Item_Icon;
	[Export] public string Item_Description;
	[Export] public ItemType  Item_Type;
	
	public ItemData() : this(null, null, null, ItemType.JunkItem) {}

	public ItemData(string itemName, Texture2D itemIcon, string itemDescription, ItemType itemType)
	{
		this.Item_Name = itemName;
		this.Item_Icon = itemIcon;
		this.Item_Description = itemDescription;
		this.Item_Type = itemType;
	}
}
