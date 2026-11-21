using Godot;

namespace Valor.UI;

[Tool] //added so that it works with TextBoxWithItemIcons
public partial class InventoryItem : Node
{
    [Export] public Label AmountLabel;
    [Export] public TextureRect IconRect;

    public void SetData(int amount, string iconResourceId)
    {
        if (this.AmountLabel != null)
        {
            this.AmountLabel.Text = amount.ToString();
        }

        if (this.IconRect == null)
        {
            GD.PushWarning("[InventoryItem] IconRect is null.");
            return;
        }

        if (string.IsNullOrWhiteSpace(iconResourceId))
        {
            this.IconRect.Texture = null;
            return;
        }

        string resolvedPath = iconResourceId;
        if (iconResourceId.StartsWith("uid://", System.StringComparison.OrdinalIgnoreCase))
        {
            long uid = ResourceLoader.GetResourceUid(iconResourceId);
            if (uid == ResourceUid.InvalidId || !ResourceUid.HasId(uid))
            {
                GD.PushWarning($"[InventoryItem] Invalid or unknown UID: {iconResourceId}");
                this.IconRect.Texture = null;
                return;
            }

            resolvedPath = ResourceUid.GetIdPath(uid);
        }

        if (!ResourceLoader.Exists(resolvedPath))
        {
            GD.PushWarning($"[InventoryItem] Resolved path does not exist: {resolvedPath} (from {iconResourceId})");
            this.IconRect.Texture = null;
            return;
        }

        Resource loaded = ResourceLoader.Load(resolvedPath);
        if (loaded is Texture2D directTexture)
        {
            this.IconRect.Texture = directTexture;
            return;
        }

        // If the UID points to an Item/Icon resource rather than a Texture2D directly,
        // extract its texture property (adjust "Icon" if your property has another name).
        if (loaded != null)
        {
            Variant iconProp = loaded.Get("Icon");
            if (iconProp.VariantType != Variant.Type.Nil && iconProp.AsGodotObject() is Texture2D nestedTexture)
            {
                this.IconRect.Texture = nestedTexture;
                return;
            }
        }

        GD.PushWarning(
            $"[InventoryItem] UID '{iconResourceId}' -> '{resolvedPath}' loaded as '{loaded?.GetType().Name ?? "null"}', " +
            "and no Texture2D property was found."
        );
        this.IconRect.Texture = null;
    }
}