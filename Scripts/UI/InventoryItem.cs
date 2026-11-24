using Godot;

namespace Valor.UI;

[Tool]
public partial class InventoryItem : FontScaledContainer
{
    [Export] public Label AmountLabel;
    [Export] public TextureRect IconRect;

    public void SetData(int amount, string iconResourceId)
    {
        
        if (this.AmountLabel != null)
            this.AmountLabel.Text = amount + "x";

        if (this.IconRect == null)
        {
            // GD.PushWarning("[InventoryItem] IconRect is null.");
            return;
        }

        if (string.IsNullOrWhiteSpace(iconResourceId))
        {
            this.IconRect.Texture = null;
            // GD.PushWarning("[InventoryItem] IconResourceId is null or empty.");
            return;
        }

        string loadPath = iconResourceId;
        if (iconResourceId.StartsWith("uid://", System.StringComparison.OrdinalIgnoreCase))
        {
            long uid = ResourceLoader.GetResourceUid(iconResourceId);
            if (uid == ResourceUid.InvalidId || !ResourceUid.HasId(uid))
            {
                // GD.PushWarning($"[InventoryItem] Invalid or unknown UID: {iconResourceId}");
                this.IconRect.Texture = null;
                return;
            }

            loadPath = iconResourceId;
        }

        Resource loaded = ResourceLoader.Load(loadPath);
        if (loaded is not Texture2D directTexture)
        {
            this.IconRect.Texture = null;
            return;
        }

        this.IconRect.Texture = directTexture;
        // GD.Print($"[InventoryItem] Assigned {directTexture.ResourcePath ?? loadPath} to {this.IconRect.Name}");
    }
}