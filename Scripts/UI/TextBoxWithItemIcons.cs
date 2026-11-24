using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Godot;

namespace Valor.UI;

[GlobalClass, Tool]
public partial class TextBoxWithItemIcons : FlowContainer
{
    private bool isApplyingUpdate;

    [Export] private Color LabelModulate { get; set; } = Colors.White;
    [Export] public PackedScene InventoryItemScene { get; set; }
    
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = string.Empty;
    
    [ExportToolButton("Build")]
    private Callable BuildButton => Callable.From(Rebuild);

    private sealed record ItemIconPayload(
        int Amount,
        [property: JsonPropertyName("IconId")] string IconId,
        [property: JsonPropertyName("ItemIconResourcePath")] string ResourcePath)
    {
        public string ResolvedIconId => !string.IsNullOrWhiteSpace(this.IconId) ? this.IconId : this.ResourcePath;
        public bool IsValid => this.Amount != 0 || !string.IsNullOrWhiteSpace(this.ResolvedIconId);
    }

    private sealed record Segment(string Text, ItemIconPayload Icon)
    {
        public bool IsIcon => this.Icon != null;
    }

    public void Rebuild()
    {
        if (this.isApplyingUpdate || !IsInstanceValid(this) || IsQueuedForDeletion() || !IsInsideTree())
            return;

        ApplyUpdateChildren();
    }

    private void ApplyUpdateChildren()
    {
        string sourceText = this.Text ?? string.Empty;
        this.isApplyingUpdate = true;

        try
        {
            List<Segment> segments = ParseSegments(sourceText);

            Queue<Label> availableLabels = new();
            Queue<InventoryItem> availableItems = new();

            foreach (Node child in GetChildren())
            {
                if (child == null || !IsInstanceValid(child) || child.IsQueuedForDeletion())
                    continue;

                if (child is Label label)
                    availableLabels.Enqueue(label);
                else if (child is InventoryItem item)
                    availableItems.Enqueue(item);
            }

            PopulateChildren(
                segments,
                createLabel: textValue =>
                {
                    Label label = availableLabels.Count > 0
                        ? availableLabels.Dequeue()
                        : AddChildAndReturn(new Label { Name = "Label" });

                    if (label.Text != textValue)
                        label.Text = textValue;

                    if (label.Modulate != this.LabelModulate)
                        label.Modulate = this.LabelModulate;

                    return label;
                },
                createItem: () => availableItems.Count > 0 ? availableItems.Dequeue() : InstantiateInventoryItem(this),
                placeChild: (child, index) =>
                {
                    if (child.GetIndex() != index)
                        MoveChild(child, index);
                });

            foreach (Label unused in availableLabels)
                if (IsInstanceValid(unused) && !unused.IsQueuedForDeletion())
                    unused.QueueFree();

            foreach (InventoryItem unused in availableItems)
                if (IsInstanceValid(unused) && !unused.IsQueuedForDeletion())
                    unused.QueueFree();
        }
        finally
        {
            this.isApplyingUpdate = false;
        }
    }

    private void PopulateChildren(
        List<Segment> segments,
        Func<string, Label> createLabel,
        Func<InventoryItem> createItem,
        Action<Node, int> placeChild)
    {
        List<(InventoryItem Item, Label SiblingLabel)> pendingSizing = [];
        Label lastPlacedLabel = null;
        int placedIndex = 0;

        foreach (Segment segment in segments)
        {
            if (!segment.IsIcon)
            {
                foreach (string chunk in SplitTextForParentWidth(segment.Text))
                {
                    Label label = createLabel(chunk);
                    placeChild(label, placedIndex++);
                    lastPlacedLabel = label;
                }
                continue;
            }

            InventoryItem item = createItem();
            if (item == null)
                continue;

            placeChild(item, placedIndex++);
            item.SetData(segment.Icon.Amount, segment.Icon.ResolvedIconId);

            if (lastPlacedLabel == null)
            {
                lastPlacedLabel = createLabel(string.Empty);
                placeChild(lastPlacedLabel, placedIndex++);
            }

            pendingSizing.Add((item, lastPlacedLabel));
        }

        foreach ((InventoryItem item, Label siblingLabel) in pendingSizing)
        {
            SceneTree tree = GetTree();
            if (tree == null ||
                !IsInstanceValid(item) || item.IsQueuedForDeletion() || !item.IsInsideTree() || item.GetTree() != tree ||
                !IsInstanceValid(siblingLabel) || siblingLabel.IsQueuedForDeletion() || !siblingLabel.IsInsideTree() || siblingLabel.GetTree() != tree)
            {
                continue;
            }

            if (FindCommonParent(item, siblingLabel) == null && item != siblingLabel.GetParent())
                continue;

            try
            {
                NodePath path = item.GetPathTo(siblingLabel);
                if (!path.IsEmpty)
                    item.TargetSiblingPath = path;
            }
            catch (InvalidOperationException)
            {
                // Node was detached or torn down mid-update.
            }
        }
    }

    private static Node FindCommonParent(Node first, Node second)
    {
        if (first == null || second == null || !IsInstanceValid(first) || !IsInstanceValid(second))
            return null;

        if (first == second)
            return first.GetParent();

        for (Node current = first.GetParent(); current != null && IsInstanceValid(current); current = current.GetParent())
        {
            if (current == second || current.IsAncestorOf(second))
                return current;
        }

        return null;
    }

    private float MeasureTextWidth(string textValue)
    {
        if (string.IsNullOrEmpty(textValue))
            return 0f;

        if (GetParent() is not Control parentControl || !IsInstanceValid(parentControl)) return textValue.Length * 8f;
        Font font = parentControl.GetThemeFont("font", "Label");
        int fontSize = parentControl.GetThemeFontSize("font_size", "Label");
        if (font != null)
            return font.GetStringSize(textValue, HorizontalAlignment.Left, -1f, fontSize).X;

        return textValue.Length * 8f;
    }

    private string[] SplitTextForParentWidth(string textValue)
    {
        if (string.IsNullOrEmpty(textValue))
            return [textValue];
        
        float? maxWidth = GetParent<Control>().GetCombinedMinimumSize().X;
        if (maxWidth <= 0f)
            return [textValue];

        List<string> chunks = [];
        string remaining = textValue;

        while (!string.IsNullOrEmpty(remaining))
        {
            if (MeasureTextWidth(remaining) <= maxWidth)
            {
                chunks.Add(remaining);
                break;
            }

            int fitLength = 0;
            for (int i = 1; i <= remaining.Length; i++)
            {
                if (!(MeasureTextWidth(remaining[..i]) <= maxWidth)) break;
                fitLength = i;
            }

            int splitIndex = -1;
            for (int i = fitLength; i >= 1; i--)
            {
                if (!char.IsWhiteSpace(remaining[i - 1])) continue;
                splitIndex = i;
                break;
            }

            if (splitIndex <= 0)
                splitIndex = Math.Max(1, fitLength);

            string line = remaining[..splitIndex].TrimEnd();
            string nextRemaining = remaining[splitIndex..].TrimStart();

            if (string.IsNullOrEmpty(line) || nextRemaining.Length >= remaining.Length)
            {
                chunks.Add(remaining); 
                break;
            }

            chunks.Add(line);
            remaining = nextRemaining;
        }

        return chunks.Count > 0 ? [.. chunks] : [textValue];
    }

    private InventoryItem InstantiateInventoryItem(Node parent)
    {
        if (this.InventoryItemScene == null)
            return null;

        Node instance = this.InventoryItemScene.Instantiate();
        if (instance is InventoryItem typedItem)
        {
            parent.AddChild(typedItem);
            return typedItem;
        }

        instance?.QueueFree();
        return null;
    }

    private T AddChildAndReturn<T>(T node) where T : Node
    {
        AddChild(node);
        return node;
    }

    private static readonly Regex ItemIconRegex = GetItemIconRegex();
    private static List<Segment> ParseSegments(string sourceText)
    {
        List<Segment> segments = [];
        int cursor = 0;

        foreach (Match match in ItemIconRegex.Matches(sourceText))
        {
            if (match.Index > cursor)
                segments.Add(new Segment(sourceText[cursor..match.Index], null));

            string json = match.Groups[1].Value;
            if (TryDeserializeIcon(json, out ItemIconPayload payload))
            {
                segments.Add(new Segment(null, payload));
                cursor = match.Index + match.Length;
            }
            else
            {
                segments.Add(new Segment(match.Value, null));
                cursor = match.Index + match.Length;
            }
        }

        if (cursor < sourceText.Length)
            segments.Add(new Segment(sourceText[cursor..], null));

        return segments;
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { AllowTrailingCommas = true };
    private static bool TryDeserializeIcon(string json, out ItemIconPayload payload)
    {
        payload = null;
        try
        {
            ItemIconPayload parsed = JsonSerializer.Deserialize<ItemIconPayload>(
                json,
                JsonSerializerOptions);

            if (parsed is not { IsValid: true }) return false;

            payload = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    [GeneratedRegex(@"ItemIcon:(\{[^{}]*\})", RegexOptions.Compiled)]
    private static partial Regex GetItemIconRegex();
}