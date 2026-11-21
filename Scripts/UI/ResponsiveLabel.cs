using Godot;

namespace Valor.UI;

[GlobalClass]
public partial class ResponsiveLabel : Label
{
    [Export(PropertyHint.Range, "6,256,1")]
    public int MinFontSize { get; set; } = 12;

    [Export(PropertyHint.Range, "8,512,1")]
    public int MaxFontSize { get; set; } = 64;

    [Export(PropertyHint.Range, "0.1,5.0,0.05")]
    public float FontSizeStepMultiplier { get; set; } = 1.0f;

    [Export]
    public bool AutoShrinkOnly { get; set; } = true;

    private string trackedText = string.Empty;
    private Vector2 lastSize = Vector2.Zero;
    private int lastAppliedFontSize = -1;

    // Shadow Label.Text so external assignments or Inspector edits trigger a resize check.
    private new string Text
    {
        get => base.Text;
        set
        {
            if (base.Text == value)
                return;

            base.Text = value;
            this.trackedText = value;
            UpdateFontSize();
        }
    }

    public override void _Ready()
    {
        this.trackedText = base.Text;
        this.lastSize = this.Size;
        this.Resized += OnResized;
        UpdateFontSize();
    }

    public override void _Process(double delta)
    {
        // Catches cases where Godot's layout pass or external code mutates base.Text
        // or Size without going through the shadowed Text property or Resized signal.
        if (base.Text == this.trackedText && this.Size == this.lastSize) return;

        this.trackedText = base.Text;
        this.lastSize = this.Size;
        UpdateFontSize();
    }

    private void OnResized()
    {
        this.lastSize = this.Size;
        UpdateFontSize();
    }

    private void UpdateFontSize()
    {
        if (!IsInsideTree()) return;

        int minSize = Mathf.Max(1, Mathf.Min(this.MinFontSize, this.MaxFontSize));
        int maxSize = Mathf.Max(minSize, Mathf.Max(this.MinFontSize, this.MaxFontSize));

        if (string.IsNullOrEmpty(base.Text) || this.Size.X <= 1f || this.Size.Y <= 1f)
        {
            ApplyFontSize(maxSize);
            return;
        }

        Font font = GetThemeFont("font");
        if (font == null) return;

        float wrapWidth = this.AutowrapMode != TextServer.AutowrapMode.Off ? this.Size.X : -1f;
        int bestSize = minSize;

        if (this.AutoShrinkOnly)
        {
            // Find the largest font size (up to MaxFontSize) that fits within Size.
            int low = minSize;
            int high = maxSize;

            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                if (FitsWithinBox(font, base.Text, mid, wrapWidth, this.Size))
                {
                    bestSize = mid;
                    low = mid + 1;
                }
                else
                    high = mid - 1;
            }
        }
        else
        {
            // Scale proportionally from a baseline reference, then clamp to fit.
            int baseFontSize = GetThemeDefaultFontSize();
            Vector2 referenceBox = new Vector2(200f, 60f);
            float scale = Mathf.Min(this.Size.X / referenceBox.X, this.Size.Y / referenceBox.Y);
            int candidate = Mathf.Clamp(Mathf.RoundToInt(baseFontSize * scale * this.FontSizeStepMultiplier), minSize, maxSize);

            bestSize = candidate;
            while (bestSize > minSize && !FitsWithinBox(font, base.Text, bestSize, wrapWidth, this.Size)) bestSize--;
        }

        ApplyFontSize(bestSize);
    }

    private bool FitsWithinBox(Font font, string text, int fontSize, float wrapWidth, Vector2 boxSize)
    {
        Vector2 measured = font.GetMultilineStringSize(
            text,
            this.HorizontalAlignment,
            wrapWidth,
            fontSize,
            -1,
            TextServer.LineBreakFlag.Mandatory | TextServer.LineBreakFlag.WordBound,
            TextServer.JustificationFlag.None
        );

        return measured.X <= boxSize.X && measured.Y <= boxSize.Y;
    }

    private void ApplyFontSize(int fontSize)
    {
        if (this.lastAppliedFontSize == fontSize) return;

        this.lastAppliedFontSize = fontSize;
        AddThemeFontSizeOverride("font_size", fontSize);
    }
}