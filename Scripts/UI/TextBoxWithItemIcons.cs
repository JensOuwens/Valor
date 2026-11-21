using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

namespace Valor.UI;

[Tool]
public partial class TextBoxWithItemIcons : Node
{
    private PackedScene inventoryItem;
    private string text;
    private bool updateQueued;
    private bool processFrameConnected;
    private bool isTearingDown;

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

    public override void _Ready()
    {
        this.isTearingDown = false;
        ScheduleUpdate();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            this.isTearingDown = false;
            ScheduleUpdate();
        }
        else if (what == NotificationExitTree || what == NotificationPredelete)
        {
            this.isTearingDown = true;
            CancelScheduledUpdate();
            QueueFreePreviewChildren();
        }

        base._Notification(what);
    }

    protected override void Dispose(bool disposing)
    {
        this.isTearingDown = true;
        CancelScheduledUpdate();

        if (disposing)
        {
            this.inventoryItem = null;
            this.text = null;
        }

        base.Dispose(disposing);
    }

    private void ScheduleUpdate()
    {
        if (this.isTearingDown || this.updateQueued || IsQueuedForDeletion() || !IsInsideTree())
            return;

        SceneTree tree = GetTree();
        if (tree == null)
            return;

        this.updateQueued = true;

        if (!this.processFrameConnected)
        {
            tree.ProcessFrame += OnProcessFrameUpdate;
            this.processFrameConnected = true;
        }
    }

    private void CancelScheduledUpdate()
    {
        this.updateQueued = false;

        if (!this.processFrameConnected)
            return;

        this.processFrameConnected = false;

        if (!IsInstanceValid(this))
            return;

        SceneTree tree = GetTree();
        if (tree != null && GodotObject.IsInstanceValid(tree))
        {
            tree.ProcessFrame -= OnProcessFrameUpdate;
        }
    }

    private void OnProcessFrameUpdate()
    {
        CancelScheduledUpdate();

        if (this.isTearingDown || IsQueuedForDeletion() || !IsInsideTree())
            return;

        ApplyUpdateChildren();
    }

    private void QueueFreePreviewChildren()
    {
        for (int i = GetChildCount() - 1; i >= 0; i--)
        {
            Node child = GetChildOrNull<Node>(i);
            if (child == null || !IsInstanceValid(child) || child.IsQueuedForDeletion())
                continue;

            if (child is not (Label or UI.InventoryItem))
                continue;

            child.QueueFree();
        }
    }

    private void ApplyUpdateChildren()
    {
        string sourceText = this.text ?? string.Empty;
        List<object> segments = ParseSegments(sourceText);

        List<Label> availableLabels = [];
        List<InventoryItem> availableItems = [];
        List<Node> extraNodes = [];

        foreach (Node child in GetChildren())
        {
            if (child == null || !IsInstanceValid(child) || child.IsQueuedForDeletion())
                continue;

            if (child is Label label)
                availableLabels.Add(label);
            else if (child is InventoryItem item)
                availableItems.Add(item);
            else
                extraNodes.Add(child);
        }

        int placedCount = 0;

        for (int i = 0; i < segments.Count; i++)
        {
            object segment = segments[i];

            if (segment is string textSegment)
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

                label.Text = textSegment;
                MoveChild(label, placedCount++);
            }
            else if (segment is ItemIcon data)
            {
                InventoryItem item = null;

                if (availableItems.Count > 0)
                {
                    item = availableItems[0];
                    availableItems.RemoveAt(0);
                }
                else if (this.inventoryItem != null)
                {
                    Node instantiated = this.inventoryItem.Instantiate();
                    if (instantiated is InventoryItem typedItem)
                    {
                        item = typedItem;
                        AddChild(item);
                    }
                    else if (instantiated != null)
                    {
                        GD.PushWarning($"{nameof(TextBoxWithItemIcons)}: InventoryItem scene root is not {nameof(InventoryItem)}.");
                        instantiated.QueueFree();
                    }
                }

                if (item == null)
                    continue;

                item.SetData(data.Amount, data.IconId);
                MoveChild(item, placedCount++);
            }
        }

        foreach (Label unused in availableLabels)
            QueueFreeIfValid(unused);

        foreach (InventoryItem unused in availableItems)
            QueueFreeIfValid(unused);

        foreach (Node extra in extraNodes)
            QueueFreeIfValid(extra);
    }

    private void QueueFreeIfValid(Node node)
    {
        if (this.isTearingDown || node == null || !IsInstanceValid(node) || node.IsQueuedForDeletion())
            return;

        node.QueueFree();
    }

    private static List<object> ParseSegments(string sourceText)
    {
        List<object> segments = [];
        const string prefix = "ItemIcon:";
        int lastIndex = 0;

        while (lastIndex < sourceText.Length)
        {
            int prefixIndex = sourceText.IndexOf(prefix, lastIndex, StringComparison.Ordinal);
            if (prefixIndex == -1)
            {
                segments.Add(sourceText[lastIndex..]);
                break;
            }

            if (prefixIndex > lastIndex)
                segments.Add(sourceText[lastIndex..prefixIndex]);

            int jsonStart = prefixIndex + prefix.Length;
            if (jsonStart >= sourceText.Length || sourceText[jsonStart] != '{')
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

    private static bool TryParseItemIcon(string jsonSlice, out ItemIcon iconData)
    {
        iconData = default;
        if (string.IsNullOrWhiteSpace(jsonSlice))
            return false;

        int amount = 0;
        bool hasAmount = false;
        string iconId = null;

        int amountKeyIndex = jsonSlice.IndexOf("\"Amount\"", StringComparison.OrdinalIgnoreCase);
        if (amountKeyIndex != -1)
        {
            int colonIndex = jsonSlice.IndexOf(':', amountKeyIndex + 8);
            if (colonIndex != -1)
            {
                int start = colonIndex + 1;
                while (start < jsonSlice.Length && char.IsWhiteSpace(jsonSlice[start]))
                    start++;

                int end = start;
                if (end < jsonSlice.Length && (jsonSlice[end] == '-' || jsonSlice[end] == '+'))
                    end++;
                while (end < jsonSlice.Length && char.IsDigit(jsonSlice[end]))
                    end++;

                if (end > start && int.TryParse(jsonSlice[start..end], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedAmount))
                {
                    amount = parsedAmount;
                    hasAmount = true;
                }
            }
        }

        int iconKeyIndex = jsonSlice.IndexOf("\"IconId\"", StringComparison.OrdinalIgnoreCase);
        if (iconKeyIndex != -1)
        {
            int colonIndex = jsonSlice.IndexOf(':', iconKeyIndex + 8);
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

        if (!hasAmount && iconId == null)
            return false;

        iconData = new ItemIcon(amount, iconId);
        return true;
    }

    private static bool TryFindMatchingBrace(string text, int openBraceIndex, out int closeBraceIndex)
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