using System;
using Godot;
using Range = Godot.Range;

namespace Valor.UI;

[Tool, GlobalClass]
public partial class HealthBarUi : Range
{
    private bool markedDirty = true;
    private bool delegatesConnected;

    private Node3D playerScriptRef;
    [Export]
    public Node3D PlayerScriptRef
    {
        get => this.playerScriptRef;
        set
        {
            if (this.playerScriptRef == value) return;
            this.playerScriptRef = value;
            this.markedDirty = true;
        }
    }
    private PlayerBlackboard playerBlackboardRef;
    
    private Godot.Collections.Array<HealthBarItem> textureItems = [];
    [Export]
    public Godot.Collections.Array<HealthBarItem> TextureItems
    {
        get => this.textureItems;
        set
        {
            if (this.textureItems == value) return;
            this.textureItems = value;
            this.markedDirty = true;
        }
    }

    private string seed;
    [Export]
    public string Seed
    {
        get => this.seed;
        set
        {
            if (this.seed == value) return;
            this.seed = value;
            this.markedDirty = true;
        }
    }

    private float itemScale = 1;
    [Export]
    public float ItemScale
    {
        get => this.itemScale;
        set
        {
            if (Mathf.IsEqualApprox(this.itemScale, value)) return;
            this.itemScale = value;
            this.markedDirty = true;
        }
    }

    private Vector2 barPadding = new(8, 8);
    [Export]
    public Vector2 BarPadding
    {
        get => this.barPadding;
        set
        {
            if (this.barPadding.IsEqualApprox(value)) return;
            this.barPadding = value;
            this.markedDirty = true;
        }
    }

    private Vector2 xOffset = new(0.25f, 0.5f);
    [Export]
    private Vector2 XOffset
    {
        get => this.xOffset;
        set
        {
            if (this.xOffset.IsEqualApprox(value)) return;
            this.xOffset = value;
            this.markedDirty = true;
        }
    }

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
            property == Range.PropertyName.Value || 
            property == PropertyName.textureItems ||
            property == PropertyName.seed ||
            property == PropertyName.itemScale ||
            property == PropertyName.barPadding)
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
        if (this.PlayerScriptRef is PlayerScript { Blackboard: not null } player)
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
        Vector2 sizeBar = new (this.Size.X, this.Size.Y);
        if (this.TextureItems.Count <= 0 || sizeBar.X <= 0f || sizeBar.Y <= 0f) return;
        
        // Rect2 fillRect = new(Vector2.Zero, new Vector2(sizeBar.X * ratio, sizeBar.Y));
        // DrawRect(fillRect, Colors.Crimson, filled: true);

        float minX = this.BarPadding.X;
        float minY = this.BarPadding.Y;
        float maxX = Mathf.Max(minX, sizeBar.X - this.BarPadding.X);
        float maxY = Mathf.Max(minY, sizeBar.Y - this.BarPadding.Y);
        float stopX = Mathf.Lerp(minX, maxX, ratio);

        RandomNumberGenerator rng = SeededRng.CreateRng(this.Seed);
        Vector2 currentPos = new(minX, minY);
        bool movingDown = true; // true = +Y (down), false = -Y (up)

        // const int maxIterations = 2000;
        // int iterations = 0;

        while (true)
        {
            HealthBarItem item = GetRandomItem();
            if (item == null) break;

            Vector2 sizeItem = item.size * this.ItemScale;
            Rect2 bounds = new(currentPos, sizeItem);
            DrawTextureRect(item.texture, bounds, tile: false, modulate: item.modulateColor);

            float stepX = Mathf.Max(1f, sizeItem.X * rng.RandfRange(this.XOffset.X, this.XOffset.Y));
            float stepY = sizeItem.Y * (movingDown ? 1f : -1f);
            currentPos += new Vector2(stepX, stepY);

            if (movingDown && currentPos.Y + sizeItem.Y > maxY)
            {
                currentPos.Y = Mathf.Max(minY, maxY - sizeItem.Y);
                movingDown = false;
            }
            else if (!movingDown && currentPos.Y < minY)
            {
                currentPos.Y = minY;
                movingDown = true;
            }

            if (currentPos.X > stopX) break;
        }

        return;

        HealthBarItem GetRandomItem()
        {
            for (int i = 0; i < this.TextureItems.Count; i++)
            {
                HealthBarItem item = this.TextureItems[rng.RandiRange(0, this.TextureItems.Count - 1)];
                if (item != null) return item;
            }
            return null;
        }
    }
}