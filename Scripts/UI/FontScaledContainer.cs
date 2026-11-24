using System;
using System.Diagnostics;
using Godot;

namespace Valor.UI;

/// <summary>
/// Scales itself and its children based on a sibling text/control component.
/// </summary>
[Tool]
[GlobalClass]
public partial class FontScaledContainer : Container
{
    private int _sortCount;
    private int _refreshCount;
    private int _targetChangedCount;
    private int _parentOrderChangedCount;

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
            if (!IsActiveInTree()) return;
            ResolveTarget();
            this.lastObservedFontSize = GetTargetFontSize();
            RefreshMinimumSize("Setter:TargetSiblingPath");
        }
    }

    [Export(PropertyHint.Range, "0.25,10.0,0.05")]
    public float WidthScale
    {
        get => this.widthScale;
        set
        {
            this.widthScale = value;
            if (!IsActiveInTree()) return;
            RefreshMinimumSize("Setter:WidthScale");
        }
    }

    [Export(PropertyHint.Range, "0.25,10.0,0.05")]
    public float HeightScale
    {
        get => this.heightScale;
        set
        {
            this.heightScale = value;
            if (!IsActiveInTree()) return;
            RefreshMinimumSize("Setter:HeightScale");
        }
    }

    [Export]
    public Vector2 Padding
    {
        get => this.padding;
        set
        {
            this.padding = value;
            if (!IsActiveInTree()) return;
            RefreshMinimumSize("Setter:Padding");
        }
    }

    [Export]
    public Vector2 MinSize
    {
        get => this.minSize;
        set
        {
            this.minSize = value;
            if (!IsActiveInTree()) return;
            RefreshMinimumSize("Setter:MinSize");
        }
    }

    [Export]
    public Vector2 MaxSize
    {
        get => this.maxSize;
        set
        {
            this.maxSize = value;
            if (!IsActiveInTree()) return;
            RefreshMinimumSize("Setter:MaxSize");
        }
    }

    private Control resolvedTarget;
    private Node observedParent;
    private bool isParentSubscribed;
    private bool isTargetSubscribed;
    private bool isExitingOrDisposing;
    private int lastObservedFontSize = -1;

    private Action parentChildOrderChangedHandler;
    private Action targetChangedHandler;
    private Action targetExitedHandler;

    public FontScaledContainer()
    {
        this.parentChildOrderChangedHandler = OnParentChildOrderChanged;
        this.targetChangedHandler = OnTargetChanged;
        this.targetExitedHandler = OnTargetExited;
    }

    private bool IsActiveInTree() =>
        !this.isExitingOrDisposing &&
        IsInstanceValid(this) &&
        !IsQueuedForDeletion() &&
        IsInsideTree();

    public override void _Ready()
    {
        if (!IsInstanceValid(this) || IsQueuedForDeletion() || !IsInsideTree())
            return;

        this.isExitingOrDisposing = false;
        ReconnectSubscriptions();
        this.lastObservedFontSize = GetTargetFontSize();
        GD.Print($"[FontScaledContainer:{Name}:_Ready] target={this.resolvedTarget?.Name ?? "null"}, fontSize={this.lastObservedFontSize}");
        RefreshMinimumSize("_Ready");
    }

    public override Vector2 _GetMinimumSize()
    {
        int fontSize = GetTargetFontSize();
        float width = (fontSize * this.WidthScale) + this.Padding.X;
        float height = (fontSize * this.HeightScale) + this.Padding.Y;

        float clampedX = Mathf.Clamp(width, this.MinSize.X, this.MaxSize.X);
        float clampedY = Mathf.Clamp(height, this.MinSize.Y, this.MaxSize.Y);

        Vector2 result = new(clampedX, clampedY);
        GD.Print($"[FontScaledContainer:{Name}:_GetMinimumSize] fontSize={fontSize}, raw=({width:0.#},{height:0.#}), clamped={result}");
        return result;
    }

    public override void _ExitTree()
    {
        GD.Print($"[FontScaledContainer:{Name}:_ExitTree]");
        this.isExitingOrDisposing = true;
        DisconnectAll();
        base._ExitTree();
    }

    protected override void Dispose(bool disposing)
    {
        this.isExitingOrDisposing = true;
        if (disposing)
        {
            DisconnectAll();
        }

        base.Dispose(disposing);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationSortChildren)
        {
            int count = ++this._sortCount;
            if (IsActiveInTree())
            {
                Stopwatch sw = Stopwatch.StartNew();
                GD.Print($"[FontScaledContainer:{Name}:Sort #{count}] Start | Size={this.Size}, Padding={this.Padding}, ChildCount={GetChildCount()}");
                LayoutChildren();
                GD.Print($"[FontScaledContainer:{Name}:Sort #{count}] End in {sw.ElapsedMilliseconds}ms | Size={this.Size}");
            }
            else
            {
                GD.Print($"[FontScaledContainer:{Name}:Sort #{count}] Skipped (not active in tree)");
            }
        }
        else if (what == NotificationParented)
        {
            if (!IsInstanceValid(this) || IsQueuedForDeletion() || !IsInsideTree())
                return;

            this.isExitingOrDisposing = false;
            ReconnectSubscriptions();
            this.lastObservedFontSize = GetTargetFontSize();
            GD.Print($"[FontScaledContainer:{Name}:NotificationParented] parent={GetParent()?.Name}, target={this.resolvedTarget?.Name ?? "null"}");
            RefreshMinimumSize("NotificationParented");
        }
        else if (what == NotificationUnparented)
        {
            GD.Print($"[FontScaledContainer:{Name}:NotificationUnparented]");
            DisconnectAll();
        }
    }

    private void ReconnectSubscriptions()
    {
        DisconnectAll();
        if (!IsActiveInTree())
            return;

        ObserveParent();
        ResolveTarget();
    }

    private void DisconnectAll()
    {
        DisconnectTarget();
        DisconnectParent();
    }

    private void LayoutChildren()
    {
        Rect2 innerRect = new(
            this.Padding.X * 0.5f,
            this.Padding.Y * 0.5f,
            Mathf.Max(0f, this.Size.X - this.Padding.X),
            Mathf.Max(0f, this.Size.Y - this.Padding.Y)
        );
        GD.Print($"[FontScaledContainer:{Name}:LayoutChildren] innerRect={innerRect}");

        int idx = 0;
        foreach (Node child in GetChildren())
        {
            if (child is Control { Visible: true } control &&
                IsInstanceValid(control) &&
                !control.IsQueuedForDeletion() &&
                control.IsInsideTree())
            {
                GD.Print($"[FontScaledContainer:{Name}:LayoutChildren] Child[{idx}] '{control.Name}' ({control.GetType().Name}) before FitChildInRect: Size={control.Size}, MinSize={control.CustomMinimumSize}");
                FitChildInRect(control, innerRect);
                GD.Print($"[FontScaledContainer:{Name}:LayoutChildren] Child[{idx}] '{control.Name}' after FitChildInRect: Size={control.Size}, MinSize={control.CustomMinimumSize}");
            }
            idx++;
        }
    }

    private void ObserveParent()
    {
        DisconnectParent();

        if (!IsActiveInTree())
            return;

        Node nextParent = GetParent();
        if (nextParent == null || !IsInstanceValid(nextParent) || nextParent.IsQueuedForDeletion() || !nextParent.IsInsideTree())
            return;

        try
        {
            if (!nextParent.IsConnected(Node.SignalName.ChildOrderChanged, Callable.From(this.parentChildOrderChangedHandler)))
            {
                nextParent.ChildOrderChanged += this.parentChildOrderChangedHandler;
            }

            this.observedParent = nextParent;
            this.isParentSubscribed = true;
        }
        catch (ObjectDisposedException)
        {
            this.observedParent = null;
            this.isParentSubscribed = false;
        }
    }

    private void DisconnectParent()
    {
        Node parentToDisconnect = this.observedParent;
        bool wasSubscribed = this.isParentSubscribed;

        this.observedParent = null;
        this.isParentSubscribed = false;

        if (!wasSubscribed || parentToDisconnect == null)
            return;

        try
        {
            if (IsInstanceValid(parentToDisconnect) &&
                parentToDisconnect.IsConnected(Node.SignalName.ChildOrderChanged, Callable.From(this.parentChildOrderChangedHandler)))
            {
                parentToDisconnect.ChildOrderChanged -= this.parentChildOrderChangedHandler;
            }
        }
        catch (ObjectDisposedException)
        {
            // Ignore if already disposed during shutdown/reload.
        }
    }

    private void OnParentChildOrderChanged()
    {
        if (!IsActiveInTree())
            return;

        int count = ++this._parentOrderChangedCount;
        GD.Print($"[FontScaledContainer:{Name}:OnParentChildOrderChanged #{count}] parent={GetParent()?.Name}");

        // If reparented without a clean notification, keep observedParent accurate.
        if (GetParent() != this.observedParent)
        {
            ObserveParent();
        }

        Control previous = this.resolvedTarget;
        ResolveTarget();
        if (previous != this.resolvedTarget)
            this.lastObservedFontSize = -1;

        this.lastObservedFontSize = GetTargetFontSize();
        RefreshMinimumSize($"OnParentChildOrderChanged#{count}");
    }

    private void ResolveTarget()
    {
        DisconnectTarget();

        if (!IsActiveInTree())
            return;

        Control candidate = null;
        if (!this.TargetSiblingPath.IsEmpty && HasNode(this.TargetSiblingPath))
        {
            Node found = GetNodeOrNull<Node>(this.TargetSiblingPath);
            if (found is Control control &&
                control != this &&
                IsInstanceValid(control) &&
                !control.IsQueuedForDeletion() &&
                control.IsInsideTree() &&
                control.GetTree() == GetTree())
            {
                candidate = control;
            }
        }

        candidate ??= FindFirstSiblingControl();

        if (candidate == null ||
            !IsInstanceValid(candidate) ||
            candidate.IsQueuedForDeletion() ||
            !candidate.IsInsideTree() ||
            candidate.GetTree() != GetTree())
        {
            return;
        }

        try
        {
            if (!candidate.IsConnected(Control.SignalName.ThemeChanged, Callable.From(this.targetChangedHandler)))
            {
                candidate.ThemeChanged += this.targetChangedHandler;
            }

            if (!candidate.IsConnected(Node.SignalName.TreeExited, Callable.From(this.targetExitedHandler)))
            {
                candidate.TreeExited += this.targetExitedHandler;
            }

            this.resolvedTarget = candidate;
            this.isTargetSubscribed = true;
        }
        catch (ObjectDisposedException)
        {
            this.resolvedTarget = null;
            this.isTargetSubscribed = false;
        }
    }

    private void DisconnectTarget()
    {
        Control targetToDisconnect = this.resolvedTarget;
        bool wasSubscribed = this.isTargetSubscribed;

        this.resolvedTarget = null;
        this.isTargetSubscribed = false;

        if (!wasSubscribed || targetToDisconnect == null)
            return;

        try
        {
            if (IsInstanceValid(targetToDisconnect))
            {
                if (targetToDisconnect.IsConnected(Control.SignalName.ThemeChanged, Callable.From(this.targetChangedHandler)))
                {
                    targetToDisconnect.ThemeChanged -= this.targetChangedHandler;
                }

                if (targetToDisconnect.IsConnected(Node.SignalName.TreeExited, Callable.From(this.targetExitedHandler)))
                {
                    targetToDisconnect.TreeExited -= this.targetExitedHandler;
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Ignore if already disposed during shutdown/reload.
        }
    }

    private Control FindFirstSiblingControl()
    {
        Node parent = GetParent();
        if (parent == null || !IsInstanceValid(parent) || parent.IsQueuedForDeletion() || !parent.IsInsideTree())
            return null;

        SceneTree tree = GetTree();
        foreach (Node sibling in parent.GetChildren())
        {
            if (sibling == this)
                continue;

            if (sibling is Control control &&
                IsInstanceValid(control) &&
                !control.IsQueuedForDeletion() &&
                control.IsInsideTree() &&
                control.GetTree() == tree)
            {
                return control;
            }
        }

        return null;
    }

    private void OnTargetChanged()
    {
        if (!IsActiveInTree())
            return;

        int newSize = GetTargetFontSize();
        int count = ++this._targetChangedCount;
        GD.Print($"[FontScaledContainer:{Name}:OnTargetChanged #{count}] last={this.lastObservedFontSize}, new={newSize}");
        if (newSize == this.lastObservedFontSize)
            return;

        this.lastObservedFontSize = newSize;
        RefreshMinimumSize($"OnTargetChanged#{count}");
    }

    private void OnTargetExited()
    {
        if (!IsActiveInTree())
            return;

        GD.Print($"[FontScaledContainer:{Name}:OnTargetExited]");
        ResolveTarget();
        this.lastObservedFontSize = GetTargetFontSize();
        RefreshMinimumSize("OnTargetExited");
    }

    private void RefreshMinimumSize(string reason = "Unknown")
    {
        if (!IsActiveInTree())
            return;

        int count = ++this._refreshCount;
        GD.Print($"[FontScaledContainer:{Name}:RefreshMinimumSize #{count}] reason={reason}, Size={this.Size}");
        UpdateMinimumSize();
        ResetSize();
        QueueSort();
        GD.Print($"[FontScaledContainer:{Name}:RefreshMinimumSize #{count}] done | Size={this.Size}");
    }

    private int GetTargetFontSize()
    {
        if (!IsActiveInTree() ||
            this.resolvedTarget == null ||
            !IsInstanceValid(this.resolvedTarget) ||
            this.resolvedTarget.IsQueuedForDeletion() ||
            !this.resolvedTarget.IsInsideTree())
        {
            return GetThemeDefaultFontSize();
        }

        if (this.resolvedTarget.HasThemeFontSizeOverride("font_size"))
        {
            int overridden = this.resolvedTarget.GetThemeFontSize("font_size");
            if (overridden > 0) return overridden;
        }

        int controlFontSize = this.resolvedTarget.GetThemeFontSize("font_size");
        return controlFontSize > 0 ? controlFontSize : GetThemeDefaultFontSize();
    }
}