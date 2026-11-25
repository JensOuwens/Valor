using Godot;

namespace Valor.UI;

[Tool, GlobalClass]
public partial class HealthBarUi : Range
{
    private const string FillRectName = "__InternalHealthFill";

    private ColorRect fillRect;
    private bool markedDirty = true;
    private bool valueChangedConnected;

    [Export] public Node3D playerScriptRef;
    private PlayerBlackboard playerBlackboardRef;

    private double prevValue;
    private double prevMin;
    private double prevMax;
    private double prevStep;
    private double prevPage;

    public override void _EnterTree()
    {
        base._EnterTree();
        
        EnsureFillRect();
        ConnectValueChanged();
        this.markedDirty = true;
    }

    public override void _Ready()
    {
        base._Ready();
        
        EnsureFillRect();
        ConnectValueChanged();
        
        SnapshotRangeState();
        this.markedDirty = true;
        
        SetProcess(true);
    }

    public override void _ExitTree()
    {
        DisconnectValueChanged();
        CleanupFillRect();
        base._ExitTree();
    }
    
    private void ConnectValueChanged()
    {
        if (this.valueChangedConnected) return;
        this.ValueChanged += OnRangeValueChanged;
        this.valueChangedConnected = true;
    }

    private void DisconnectValueChanged()
    {
        if (!this.valueChangedConnected) return;
        this.ValueChanged -= OnRangeValueChanged;
        this.valueChangedConnected = false;
    }

    #region Fillrect
    private void EnsureFillRect()
    {
        if (IsInstanceValid(this.fillRect) && this.fillRect.GetParent() == this)
            return;

        for (int i = 0; i < GetChildCount(includeInternal: true); i++)
        {
            if (GetChild(i, includeInternal: true) is not ColorRect existing || existing.Name != FillRectName) continue;
            this.fillRect = existing;
            ConfigureFillRectDefaults(this.fillRect);
            return;
        }

        this.fillRect = new ColorRect { Name = FillRectName };
        ConfigureFillRectDefaults(this.fillRect);

        AddChild(this.fillRect, false, InternalMode.Front);
    }

    private static void ConfigureFillRectDefaults(ColorRect rect)
    {
        rect.MouseFilter = MouseFilterEnum.Ignore;
        rect.Color = Colors.Crimson;
    }

    private void CleanupFillRect()
    {
        if (IsInstanceValid(this.fillRect))
        {
            if (this.fillRect.IsInsideTree())
                this.fillRect.QueueFree();
            else
                this.fillRect.Free();
        }

        this.fillRect = null;
    }
    #endregion

    private void OnRangeValueChanged(double newValue)
    {
        if (Mathf.IsEqualApprox(this.prevValue, newValue)) return;
        this.prevValue = newValue;
        this.markedDirty = true;
    }

    public override bool _Set(StringName property, Variant value)
    {
        bool changed = base._Set(property, value);

        if (property == Range.PropertyName.MinValue ||
            property == Range.PropertyName.MaxValue ||
            property == Range.PropertyName.Step ||
            property == Range.PropertyName.Page ||
            property == Range.PropertyName.Value)
        {
            this.markedDirty = true;
        }

        return changed;
    }

    public override void _Process(double delta)
    {
        if (HasRangeChanged())
        {
            SnapshotRangeState();
            this.markedDirty = true;
        }

        // If runtime playerScript 
        if (this.playerScriptRef is PlayerScript { Blackboard: not null } player)
        {
            this.playerBlackboardRef = player.Blackboard;

            if (!Mathf.IsEqualApprox(this.MaxValue, this.playerBlackboardRef.MaxHealth) ||
                !Mathf.IsEqualApprox(this.Value, this.playerBlackboardRef.CurrentHealth))
            {
                this.MaxValue = this.playerBlackboardRef.MaxHealth;
                this.Value = this.playerBlackboardRef.CurrentHealth;
                SnapshotRangeState();
                this.markedDirty = true;
            }
        }

        bool isEditor = Engine.IsEditorHint();
        if (isEditor && !this.markedDirty)
        {
            base._Process(delta);
            return;
        }

        EnsureFillRect();
        ApplyVisuals();
        this.markedDirty = false;

        base._Process(delta);
    }

    private bool HasRangeChanged()
    {
        return !Mathf.IsEqualApprox(this.prevValue, this.Value) ||
               !Mathf.IsEqualApprox(this.prevMin, this.MinValue) ||
               !Mathf.IsEqualApprox(this.prevMax, this.MaxValue) ||
               !Mathf.IsEqualApprox(this.prevStep, this.Step) ||
               !Mathf.IsEqualApprox(this.prevPage, this.Page);
    }

    private void SnapshotRangeState()
    {
        this.prevValue = this.Value;
        this.prevMin = this.MinValue;
        this.prevMax = this.MaxValue;
        this.prevStep = this.Step;
        this.prevPage = this.Page;
    }

    private void ApplyVisuals()
    {
        if (!IsInstanceValid(this.fillRect)) return;

        double span = this.MaxValue - this.MinValue;
        float ratio = span > 0.0
            ? Mathf.Clamp((float)((this.Value - this.MinValue) / span), 0f, 1f)
            : 0f;

        this.fillRect.Position = Vector2.Zero;
        this.fillRect.Size = new Vector2(this.Size.X * ratio, this.Size.Y);

        QueueRedraw();
    }
}