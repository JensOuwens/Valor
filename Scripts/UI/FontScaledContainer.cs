using System;
using Godot;


/// <summary>
/// Generated using Gemini, this should scale itself and its children based on a sibling text component
/// </summary>

namespace Valor.UI;

[Tool]
[GlobalClass]
public partial class FontScaledContainer : Container
{
    private NodePath targetSiblingPath = new();
    private float widthScale = 1f;
    private float heightScale = 1f;
    private Vector2 padding = Vector2.Zero;
    private Vector2 minSize = Vector2.Zero;
    private Vector2 maxSize = new(float.MaxValue, float.MaxValue);

    [Export]
    public NodePath TargetSiblingPath
    {
        get => this.targetSiblingPath;
        set
        {
            this.targetSiblingPath = value;
            if (!IsInsideTree() || this.isExitingOrDisposing) return;
            ResolveTarget();
            RefreshMinimumSize();
        }
    }

    [Export(PropertyHint.Range, "0.25,10.0,0.05")]
    public float WidthScale
    {
        get => this.widthScale;
        set
        {
            this.widthScale = value;
            if (!IsInsideTree() || this.isExitingOrDisposing) return;
            RefreshMinimumSize();
        }
    }

    [Export(PropertyHint.Range, "0.25,10.0,0.05")]
    public float HeightScale
    {
        get => this.heightScale;
        set
        {
            this.heightScale = value;
            if (!IsInsideTree() || this.isExitingOrDisposing) return;
            RefreshMinimumSize();
        }
    }

    [Export]
    public Vector2 Padding
    {
        get => this.padding;
        set
        {
            this.padding = value;
            if (!IsInsideTree() || this.isExitingOrDisposing) return;
            RefreshMinimumSize();
        }
    }

    [Export]
    public Vector2 MinSize
    {
        get => this.minSize;
        set
        {
            this.minSize = value;
            if (!IsInsideTree() || this.isExitingOrDisposing) return;
            RefreshMinimumSize();
        }
    }

    [Export]
    public Vector2 MaxSize
    {
        get => this.maxSize;
        set
        {
            this.maxSize = value;
            if (!IsInsideTree() || this.isExitingOrDisposing) return;
            RefreshMinimumSize();
        }
    }

    private Control resolvedTarget;
    private Node observedParent;
    private bool isParentSubscribed;
    private bool isTargetSubscribed;
    private bool isExitingOrDisposing;
    private int lastObservedFontSize = -1;

    public override void _Ready()
    {
        this.isExitingOrDisposing = false;
        ObserveParent();
        ResolveTarget();
        this.lastObservedFontSize = GetTargetFontSize();
        RefreshMinimumSize();
    }

    public override Vector2 _GetMinimumSize()
    {
        int fontSize = GetTargetFontSize();
        float width = (fontSize * this.WidthScale) + this.Padding.X;
        float height = (fontSize * this.HeightScale) + this.Padding.Y;

        float clampedX = Mathf.Clamp(width, this.MinSize.X, this.MaxSize.X);
        float clampedY = Mathf.Clamp(height, this.MinSize.Y, this.MaxSize.Y);

        return new Vector2(clampedX, clampedY);
    }

    public override void _ExitTree()
    {
        this.isExitingOrDisposing = true;
        DisconnectTarget();
        DisconnectParent();
        base._ExitTree();
    }

    protected override void Dispose(bool disposing)
    {
        this.isExitingOrDisposing = true;
        if (disposing)
        {
            DisconnectTarget();
            DisconnectParent();
        }

        base.Dispose(disposing);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationSortChildren)
        {
            if (!this.isExitingOrDisposing)
                LayoutChildren();
        }
        else if (what == NotificationChildOrderChanged)
        {
            if (this.isExitingOrDisposing || !IsInsideTree()) return;
            ResolveTarget();
            RefreshMinimumSize();
        }
        else if (what == NotificationParented)
        {
            this.isExitingOrDisposing = false;
            if (!IsInsideTree()) return;
            ObserveParent();
            ResolveTarget();
            RefreshMinimumSize();
        }
        else if (what == NotificationUnparented)
        {
            DisconnectTarget();
            DisconnectParent();
        }
    }

    private void LayoutChildren()
    {
        Rect2 innerRect = new(
            this.Padding.X * 0.5f,
            this.Padding.Y * 0.5f,
            Mathf.Max(0f, this.Size.X - this.Padding.X),
            Mathf.Max(0f, this.Size.Y - this.Padding.Y)
        );

        foreach (Node child in GetChildren())
        {
            if (child is Control { Visible: true } control && IsInstanceValid(control) && !control.IsQueuedForDeletion())
                FitChildInRect(control, innerRect);
        }
    }

    private void ObserveParent()
    {
        DisconnectParent();

        if (this.isExitingOrDisposing || !IsInsideTree())
            return;

        Node nextParent = GetParent();
        if (nextParent == null || !IsInstanceValid(nextParent) || nextParent.IsQueuedForDeletion())
            return;

        this.observedParent = nextParent;
        this.observedParent.ChildOrderChanged += OnParentChildOrderChanged;
        this.isParentSubscribed = true;
    }

    private void DisconnectParent()
    {
        if (this.isParentSubscribed && this.observedParent != null)
        {
            try
            {
                if (IsInstanceValid(this.observedParent))
                    this.observedParent.ChildOrderChanged -= OnParentChildOrderChanged;
            }
            catch (ObjectDisposedException)
            {
                // Ignore if already disposed during shutdown/reload.
            }
        }

        this.isParentSubscribed = false;
        this.observedParent = null;
    }

    private void OnParentChildOrderChanged()
    {
        if (this.isExitingOrDisposing || !IsInsideTree() || IsQueuedForDeletion())
            return;

        Control previous = this.resolvedTarget;
        ResolveTarget();
        if (previous != this.resolvedTarget)
            this.lastObservedFontSize = -1;

        RefreshMinimumSize();
    }

    private void ResolveTarget()
    {
        DisconnectTarget();

        if (this.isExitingOrDisposing || !IsInsideTree() || IsQueuedForDeletion())
            return;

        Control candidate = null;
        if (!this.TargetSiblingPath.IsEmpty)
            candidate = GetNodeOrNull<Control>(this.TargetSiblingPath);

        candidate ??= FindFirstSiblingControl();

        if (candidate == null || !IsInstanceValid(candidate) || candidate.IsQueuedForDeletion())
            return;

        this.resolvedTarget = candidate;
        this.resolvedTarget.ThemeChanged += OnTargetChanged;
        this.resolvedTarget.TreeExited += OnTargetExited;
        this.isTargetSubscribed = true;
    }

    private void DisconnectTarget()
    {
        if (this.isTargetSubscribed && this.resolvedTarget != null)
        {
            try
            {
                if (IsInstanceValid(this.resolvedTarget))
                {
                    this.resolvedTarget.ThemeChanged -= OnTargetChanged;
                    this.resolvedTarget.TreeExited -= OnTargetExited;
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore if already disposed during shutdown/reload.
            }
        }

        this.isTargetSubscribed = false;
        this.resolvedTarget = null;
    }

    private Control FindFirstSiblingControl()
    {
        Node parent = GetParent();
        if (parent == null || !IsInstanceValid(parent) || parent.IsQueuedForDeletion())
            return null;

        foreach (Node sibling in parent.GetChildren())
        {
            if (sibling == this)
                continue;

            if (sibling is Control control && IsInstanceValid(control) && !control.IsQueuedForDeletion())
                return control;
        }

        return null;
    }

    private void OnTargetChanged()
    {
        if (this.isExitingOrDisposing || !IsInsideTree() || IsQueuedForDeletion())
            return;

        int newSize = GetTargetFontSize();
        if (newSize == this.lastObservedFontSize)
            return;

        this.lastObservedFontSize = newSize;
        RefreshMinimumSize();
    }

    private void OnTargetExited()
    {
        if (this.isExitingOrDisposing || !IsInsideTree() || IsQueuedForDeletion())
            return;

        ResolveTarget();
        this.lastObservedFontSize = GetTargetFontSize();
        RefreshMinimumSize();
    }

    private void RefreshMinimumSize()
    {
        if (this.isExitingOrDisposing || IsQueuedForDeletion() || !IsInsideTree())
            return;

        UpdateMinimumSize();
        ResetSize();
        QueueSort();
    }

    private int GetTargetFontSize()
    {
        if (this.isExitingOrDisposing || !IsInsideTree() || this.resolvedTarget == null || !IsInstanceValid(this.resolvedTarget) || this.resolvedTarget.IsQueuedForDeletion())
            return GetThemeDefaultFontSize();

        if (this.resolvedTarget.HasThemeFontSizeOverride("font_size"))
        {
            int overridden = this.resolvedTarget.GetThemeFontSize("font_size");
            if (overridden > 0) return overridden;
        }

        int controlFontSize = this.resolvedTarget.GetThemeFontSize("font_size");
        return controlFontSize > 0 ? controlFontSize : GetThemeDefaultFontSize();
    }
}