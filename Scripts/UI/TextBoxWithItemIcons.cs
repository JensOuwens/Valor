using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace Valor.UI;

[Tool]
public partial class TextBoxWithItemIcons : Node
{
    private PackedScene inventoryItem;
    private string text = string.Empty;

    private string lastAppliedText;
    private PackedScene lastAppliedInventoryItem;

    private bool hasAppliedOnce;
    private bool updateQueued;
    private bool isApplyingUpdate;
    private bool isTearingDown;

    [ExportToolButton("Bake Copy")]
    private Callable BakeCopyButton => Callable.From(BakeCopy);

    [Export]
    public PackedScene InventoryItem
    {
        get => this.inventoryItem;
        set
        {
            if (this.inventoryItem == value)
                return;

            this.inventoryItem = value;
            ScheduleUpdate();
        }
    }

    [Export(PropertyHint.MultilineText)]
    public string Text
    {
        get => this.text;
        set
        {
            if (this.text == value)
                return;

            this.text = value;
            ScheduleUpdate();
        }
    }

    private readonly record struct ItemIcon(int Amount, string IconId);

    // ---------------------------------------------------------------------
    // Bake / Debug Snapshot
    // ---------------------------------------------------------------------

    public void BakeCopy()
    {
        if (!Engine.IsEditorHint())
        {
            GD.PushWarning($"{nameof(TextBoxWithItemIcons)}: BakeCopy requires Engine.IsEditorHint() to be true.");
            return;
        }

        if (this.isTearingDown || !IsInsideTree() || !IsInstanceValid(this) || IsQueuedForDeletion())
        {
            GD.PushWarning($"{nameof(TextBoxWithItemIcons)}: Node is not valid or not inside the scene tree.");
            return;
        }

        Node parent = GetParent();
        Node sceneRoot = GetTree()?.EditedSceneRoot;

        if (parent == null || !IsInstanceValid(parent) || parent.IsQueuedForDeletion() ||
            sceneRoot == null || !IsInstanceValid(sceneRoot))
        {
            GD.PushWarning($"{nameof(TextBoxWithItemIcons)}: Cannot bake copy without a valid parent node and EditedSceneRoot.");
            return;
        }

        // Ensure current segments are built before baking.
        this.updateQueued = false;
        this.hasAppliedOnce = false;
        ApplyUpdateChildren();

        if (!IsInstanceValid(this) || IsQueuedForDeletion())
            return;

        string baseName = Name;
        int insertIndex = GetIndex() + 1;

        Node bakedRoot = new Node
        {
            Name = $"{baseName}_Baked"
        };

        List<object> segments = ParseSegments(this.text ?? string.Empty);
        List<(InventoryItem Item, Label SiblingLabel)> pendingSizing = [];
        Label lastPlacedLabel = null;

        foreach (object segment in segments)
        {
            if (segment is string textSegment)
            {
                Label label = new Label
                {
                    Name = "Label",
                    Text = textSegment
                };
                bakedRoot.AddChild(label);
                lastPlacedLabel = label;
            }
            else if (segment is ItemIcon iconData)
            {
                if (this.inventoryItem == null)
                    continue;

                Node instance = this.inventoryItem.Instantiate();
                if (instance is not InventoryItem typedItem)
                {
                    GD.PushWarning($"{nameof(TextBoxWithItemIcons)}: InventoryItem scene root is not InventoryItem.");
                    instance?.QueueFree();
                    continue;
                }

                bakedRoot.AddChild(typedItem);
                typedItem.SetData(iconData.Amount, iconData.IconId);

                Label siblingLabel = lastPlacedLabel;
                if (siblingLabel == null)
                {
                    siblingLabel = new Label
                    {
                        Name = "Label",
                        Text = string.Empty
                    };
                    bakedRoot.AddChild(siblingLabel);
                    lastPlacedLabel = siblingLabel;
                }

                pendingSizing.Add((typedItem, siblingLabel));
            }
        }

        parent.AddChild(bakedRoot);
        parent.MoveChild(bakedRoot, Math.Min(insertIndex, parent.GetChildCount() - 1));

        SetOwnerForBake(bakedRoot, sceneRoot);

        foreach ((InventoryItem item, Label siblingLabel) in pendingSizing)
        {
            if (IsInstanceValid(item) && !item.IsQueuedForDeletion() &&
                IsInstanceValid(siblingLabel) && !siblingLabel.IsQueuedForDeletion())
            {
                item.TargetSiblingPath = item.GetPathTo(siblingLabel);
            }
        }
    }

    private static void SetOwnerForBake(Node node, Node sceneRoot)
    {
        if (node == null || !IsInstanceValid(node) || node.IsQueuedForDeletion())
            return;

        node.Owner = sceneRoot;

        // Stop before descending into external sub-scene instances (such as InventoryItem)
        // so their internal nodes and scripts stay owned by their own PackedScene.
        if (node != sceneRoot && !string.IsNullOrEmpty(node.SceneFilePath))
            return;

        foreach (Node child in node.GetChildren())
        {
            SetOwnerForBake(child, sceneRoot);
        }
    }

    // ---------------------------------------------------------------------
    // Lifecycle
    // ---------------------------------------------------------------------

    public override void _EnterTree()
    {
        this.isTearingDown = false;
        ScheduleUpdate();
    }

    public override void _Ready()
    {
        ScheduleUpdate();
    }

    public override void _ExitTree()
    {
        this.isTearingDown = true;
        this.updateQueued = false;
    }

    // ---------------------------------------------------------------------
    // Update scheduling
    // ---------------------------------------------------------------------

    public void ForceRebuild()
    {
        this.hasAppliedOnce = false;
        ScheduleUpdate();
    }

    private void ScheduleUpdate()
    {
        if (this.isTearingDown ||
            this.isApplyingUpdate ||
            this.updateQueued ||
            IsQueuedForDeletion() ||
            !IsInsideTree())
        {
            return;
        }

        string currentText = this.text ?? string.Empty;

        if (this.hasAppliedOnce &&
            string.Equals(currentText, this.lastAppliedText, StringComparison.Ordinal) &&
            this.inventoryItem == this.lastAppliedInventoryItem)
        {
            return;
        }

        this.updateQueued = true;

        new Callable(this, MethodName.OnDeferredUpdate).CallDeferred();
    }

    private void OnDeferredUpdate()
    {
        this.updateQueued = false;

        if (this.isTearingDown ||
            this.isApplyingUpdate ||
            !IsInstanceValid(this) ||
            IsQueuedForDeletion() ||
            !IsInsideTree())
        {
            return;
        }

        ApplyUpdateChildren();
    }

    // ---------------------------------------------------------------------
    // Preview child reconciliation
    // ---------------------------------------------------------------------

    private void ApplyUpdateChildren()
    {
        string sourceText = this.text ?? string.Empty;

        if (this.hasAppliedOnce &&
            string.Equals(sourceText, this.lastAppliedText, StringComparison.Ordinal) &&
            this.inventoryItem == this.lastAppliedInventoryItem)
        {
            return;
        }

        this.isApplyingUpdate = true;

        try
        {
            List<object> segments = ParseSegments(sourceText);

            List<Label> availableLabels = [];
            List<InventoryItem> availableItems = [];

            foreach (Node child in GetChildren())
            {
                if (child == null ||
                    !IsInstanceValid(child) ||
                    child.IsQueuedForDeletion())
                {
                    continue;
                }

                if (child is Label label)
                    availableLabels.Add(label);
                else if (child is InventoryItem item)
                    availableItems.Add(item);
            }

            List<(InventoryItem Item, Label SiblingLabel)> pendingSizing = [];
            Label lastPlacedLabel = null;
            int placedCount = 0;

            foreach (object segment in segments)
            {
                if (segment is string textSegment)
                {
                    Label label = AcquireLabel(availableLabels, textSegment);
                    PlaceChild(label, placedCount++);
                    lastPlacedLabel = label;
                }
                else if (segment is ItemIcon iconData)
                {
                    InventoryItem item = GetAvailableItem(availableItems);

                    if (item == null)
                        continue;

                    PlaceChild(item, placedCount++);
                    item.SetData(iconData.Amount, iconData.IconId);

                    Label siblingLabel = lastPlacedLabel;
                    if (siblingLabel == null)
                    {
                        siblingLabel = AcquireLabel(availableLabels, string.Empty);
                        PlaceChild(siblingLabel, placedCount++);
                        lastPlacedLabel = siblingLabel;
                    }

                    pendingSizing.Add((item, siblingLabel));
                }
            }

            RemoveUnusedChildren(availableLabels);
            RemoveUnusedChildren(availableItems);

            foreach ((InventoryItem item, Label siblingLabel) in pendingSizing)
            {
                if (IsInstanceValid(item) && !item.IsQueuedForDeletion() &&
                    IsInstanceValid(siblingLabel) && !siblingLabel.IsQueuedForDeletion())
                {
                    item.TargetSiblingPath = item.GetPathTo(siblingLabel);
                }
            }

            this.lastAppliedText = sourceText;
            this.lastAppliedInventoryItem = this.inventoryItem;
            this.hasAppliedOnce = true;
        }
        finally
        {
            this.isApplyingUpdate = false;
        }
    }

    private Label AcquireLabel(List<Label> availableLabels, string textValue)
    {
        Label label;

        if (availableLabels.Count > 0)
        {
            label = availableLabels[0];
            availableLabels.RemoveAt(0);
        }
        else
        {
            label = new Label { Name = "Label" };
            AddChild(label);
        }

        if (label.Text != textValue)
            label.Text = textValue;

        return label;
    }

    private InventoryItem GetAvailableItem(List<InventoryItem> availableItems)
    {
        if (availableItems.Count > 0)
        {
            InventoryItem item = availableItems[0];
            availableItems.RemoveAt(0);
            return item;
        }

        if (this.inventoryItem == null)
            return null;

        Node instance = this.inventoryItem.Instantiate();

        if (instance is InventoryItem typedItem)
        {
            AddChild(typedItem);
            return typedItem;
        }

        if (instance != null)
        {
            GD.PushWarning(
                $"{nameof(TextBoxWithItemIcons)}: InventoryItem scene root is not {nameof(this.InventoryItem)}.");

            instance.QueueFree();
        }

        return null;
    }

    private void PlaceChild(Node child, int index)
    {
        if (child.GetIndex() != index)
            MoveChild(child, index);
    }

    private void RemoveUnusedChildren<T>(List<T> unusedChildren) where T : Node
    {
        foreach (T child in unusedChildren)
        {
            if (child == null ||
                !IsInstanceValid(child) ||
                child.IsQueuedForDeletion())
            {
                continue;
            }

            child.QueueFree();
        }
    }

    // ---------------------------------------------------------------------
    // Parsing
    // ---------------------------------------------------------------------

    private static List<object> ParseSegments(string sourceText)
    {
        List<object> segments = [];

        const string prefix = "ItemIcon:";
        int lastIndex = 0;

        while (lastIndex < sourceText.Length)
        {
            int prefixIndex = sourceText.IndexOf(
                prefix,
                lastIndex,
                StringComparison.Ordinal);

            if (prefixIndex == -1)
            {
                segments.Add(sourceText[lastIndex..]);
                break;
            }

            if (prefixIndex > lastIndex)
                segments.Add(sourceText[lastIndex..prefixIndex]);

            int jsonStart = prefixIndex + prefix.Length;

            if (jsonStart >= sourceText.Length ||
                sourceText[jsonStart] != '{')
            {
                segments.Add(sourceText[prefixIndex..(prefixIndex + prefix.Length)]);
                lastIndex = prefixIndex + prefix.Length;
                continue;
            }

            if (!TryFindMatchingBrace(sourceText, jsonStart, out int jsonEnd))
            {
                segments.Add(sourceText[prefixIndex..]);
                break;
            }

            string jsonSlice = sourceText[jsonStart..(jsonEnd + 1)];

            if (TryParseItemIcon(jsonSlice, out ItemIcon iconData))
            {
                segments.Add(iconData);
                lastIndex = jsonEnd + 1;
            }
            else
            {
                segments.Add(sourceText[prefixIndex..(jsonEnd + 1)]);
                lastIndex = jsonEnd + 1;
            }
        }

        return segments;
    }

    private static bool TryParseItemIcon(
        string jsonSlice,
        out ItemIcon iconData)
    {
        iconData = default;

        if (string.IsNullOrWhiteSpace(jsonSlice))
            return false;

        int amount = 0;
        bool hasAmount = false;
        string iconId = null;

        int amountKeyIndex = jsonSlice.IndexOf(
            "\"Amount\"",
            StringComparison.OrdinalIgnoreCase);

        if (amountKeyIndex != -1)
        {
            int colonIndex = jsonSlice.IndexOf(':', amountKeyIndex);

            if (colonIndex != -1)
            {
                int start = colonIndex + 1;

                while (start < jsonSlice.Length &&
                       char.IsWhiteSpace(jsonSlice[start]))
                {
                    start++;
                }

                int end = start;

                if (end < jsonSlice.Length &&
                    (jsonSlice[end] == '-' || jsonSlice[end] == '+'))
                {
                    end++;
                }

                while (end < jsonSlice.Length &&
                       char.IsDigit(jsonSlice[end]))
                {
                    end++;
                }

                if (end > start &&
                    int.TryParse(
                        jsonSlice[start..end],
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int parsedAmount))
                {
                    amount = parsedAmount;
                    hasAmount = true;
                }
            }
        }

        int iconKeyIndex = jsonSlice.IndexOf(
            "\"ItemIconResourcePath\"",
            StringComparison.OrdinalIgnoreCase);

        if (iconKeyIndex == -1)
        {
            iconKeyIndex = jsonSlice.IndexOf(
                "\"IconId\"",
                StringComparison.OrdinalIgnoreCase);
        }

        if (iconKeyIndex != -1)
        {
            int colonIndex = jsonSlice.IndexOf(':', iconKeyIndex);

            if (colonIndex != -1)
            {
                int firstQuote = jsonSlice.IndexOf('"', colonIndex + 1);

                if (firstQuote != -1)
                {
                    int i = firstQuote + 1;
                    bool escaped = false;

                    while (i < jsonSlice.Length)
                    {
                        char c = jsonSlice[i];

                        if (escaped)
                        {
                            escaped = false;
                        }
                        else if (c == '\\')
                        {
                            escaped = true;
                        }
                        else if (c == '"')
                        {
                            iconId = jsonSlice[(firstQuote + 1)..i];
                            break;
                        }

                        i++;
                    }
                }
            }
        }

        if (!hasAmount && string.IsNullOrWhiteSpace(iconId))
            return false;

        iconData = new ItemIcon(amount, iconId);
        return true;
    }

    private static bool TryFindMatchingBrace(
        string text,
        int openBraceIndex,
        out int closeBraceIndex)
    {
        int depth = 0;
        bool inString = false;
        bool escaped = false;

        for (int i = openBraceIndex; i < text.Length; i++)
        {
            char c = text[i];

            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (c == '"')
            {
                inString = true;
            }
            else if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;

                if (depth == 0)
                {
                    closeBraceIndex = i;
                    return true;
                }
            }
        }

        closeBraceIndex = -1;
        return false;
    }
}