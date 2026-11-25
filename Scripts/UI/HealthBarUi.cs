using Godot;

namespace Valor.UI;

[Tool, GlobalClass]
public partial class HealthBarUi : Range
{
    private bool markedDirty = true;
    private bool delegatesConnected;

    [Export] public Node3D playerScriptRef;
    private PlayerBlackboard playerBlackboardRef;
    
    [Export] public Godot.Collections.Array<HealthBarItem> textureItems;
    [Export] public string seed;

    private double prevValue;
    private double prevMin;
    private double prevMax;
    private double prevStep;
    private double prevPage;

    public override void _EnterTree()
    {
        base._EnterTree();
        
        ConnectDelegates();
        this.markedDirty = true;
    }

    public override void _Ready()
    {
        base._Ready();
        
        ConnectDelegates();
        
        SnapshotRangeState();
        this.markedDirty = true;
        
        SetProcess(true);
    }

    public override void _ExitTree()
    {
        DisconnectDelegates();
        base._ExitTree();
    }
    
    private void ConnectDelegates()
    {
        if (this.delegatesConnected) return;
        this.ValueChanged += OnRangeValueChanged;
        this.Resized += QueueRedraw;
        this.delegatesConnected = true;
    }

    private void DisconnectDelegates()
    {
        if (!this.delegatesConnected) return;
        this.ValueChanged -= OnRangeValueChanged;
        this.Resized -= QueueRedraw;
        this.delegatesConnected = false;
    }

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
            this.markedDirty = true;

        return changed;
    }

    public override void _Process(double delta)
    {
        if (HasRangeChanged())
        {
            SnapshotRangeState();
            this.markedDirty = true;
        }

        // If in runtime, playerScript actually exists
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

        QueueRedraw();
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

    public override void _Draw()
    {
        double span = this.MaxValue - this.MinValue;
        float ratio = span > 0.0
            ? Mathf.Clamp((float)((this.Value - this.MinValue) / span), 0f, 1f)
            : 0f;
        Vector2 size = new (this.Size.X, this.Size.Y);;
        if (size.X <= 0f || size.Y <= 0f) return;

        // TODO: fill available space randomly from left to right
        // choose textures randomly too
        // Make sure the space is actually filled (somehow)
        
        // RandomNumberGenerator rng = SeededRng.CreateRng(this.seed);
        //
        // Rect2 bounds = new Rect2(Vector2.Zero, Size);
        // DrawTextureRect(_texture, bounds, tile: false, modulate: _modulate);
        
        Rect2 fillRect = new(Vector2.Zero, new Vector2(size.X * ratio, size.Y));
        DrawRect(fillRect, Colors.Crimson, filled: true);
    }
}