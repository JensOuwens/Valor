using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Valor.QuestSystem;

namespace Valor.UI;

public partial class QuestPopUp : Control
{
    private int _runCounter;

    [Export]
    public Godot.Collections.Array<QuestBackboardList> Backboards { get; set; }

    [Export]
    public TextureRect Background { get; set; }

    [Export]
    public VBoxContainer Content { get; set; }
    [Export]
    public MarginContainer ContentMargin { get; set; }

    [Export]
    public Label NameQuestNpc { get; set; }
    [Export]
    public Label NameQuest { get; set; }
    [Export]
    public TextBoxWithItemIcons QuestText { get; set; }

    [Export]
    public TextureButton AcceptButton { get; set; }
    [Export]
    public TextureButton DeclineButton { get; set; }

    public override void _Ready()
    {
        base._Ready();
        GD.Print($"[QuestPopUp:_Ready] Backboards count={this.Backboards?.Count ?? -1}");

        if (this.Backboards == null) return;

        foreach (QuestBackboardList backboardList in this.Backboards)
        {
            GD.Print($"[QuestPopUp:_Ready] HeightKey={backboardList?.HeightKey}, count={backboardList?.Backboards?.Count ?? -1}");
            if (backboardList?.Backboards == null) continue;

            foreach (QuestBackboard backboard in backboardList.Backboards)
            {
                GD.Print($"[QuestPopUp:_Ready]   Backboard: {backboard?.WidthKey}, Max={backboard?.MaxSize}, Min={backboard?.MinSize}, Constraints={backboard?.ContentConstraints}");
            }
        }
    }

    private int _activeRunId;
    private CancellationTokenSource _runCts;

    public void SetQuest(QuestData playerCurrentQuest, QuestData.QuestDialogueState dialogueState)
    {
        int runId = ++this._runCounter;
        this._runCts?.Cancel();
        this._runCts?.Dispose();
        this._runCts = new CancellationTokenSource();
        this._activeRunId = runId;

        GD.Print($"[QuestPopUp:SetQuest #{runId}] Start | Quest='{playerCurrentQuest?.QuestName}', State={dialogueState}");

        this.NameQuestNpc.Text = playerCurrentQuest.NpcName;
        this.NameQuest.Text = playerCurrentQuest.QuestName;

        string text = playerCurrentQuest.QuestDialogue.FirstOrDefault(obj => obj.State == dialogueState)?.Text;
        GD.Print($"[QuestPopUp:SetQuest #{runId}] Dialogue text length={text?.Length ?? -1} | preview='{text?[..Math.Min(text.Length, 60)]}'");

        _ = TryGetCorrectBlackboard(text, runId, this._runCts.Token);
        GD.Print($"[QuestPopUp:SetQuest #{runId}] Dispatched TryGetCorrectBlackboard");
    }

    private async Task TryGetCorrectBlackboard(string text, int runId)
    {
        Stopwatch totalSw = Stopwatch.StartNew();
        Stopwatch stepSw = new();

        GD.Print($"[Backboard #{runId}] Enter TryGetCorrectBlackboard");

        if (this.Backboards == null || this.Backboards.Count == 0)
        {
            GD.PushWarning($"[Backboard #{runId}] Backboards is null or empty! Returning early.");
            return;
        }

        QuestBackboard exactMatch = null;
        QuestBackboard leastOverflowFallback = null;
        float leastOverflow = float.MaxValue;
        float tieBreakerArea = float.MaxValue;

        int listIdx = 0;
        foreach (QuestBackboardList backboardList in this.Backboards)
        {
            GD.Print($"[Backboard #{runId}] --- List[{listIdx}] HeightKey='{backboardList?.HeightKey}' (t={totalSw.ElapsedMilliseconds}ms) ---");

            if (backboardList == null)
            {
                GD.PushWarning($"[Backboard #{runId}] List[{listIdx}] is null, skipping");
                listIdx++;
                continue;
            }

            this.ContentMargin.AddThemeConstantOverride("margin_left", backboardList.MarginLeft);
            this.ContentMargin.AddThemeConstantOverride("margin_right", backboardList.MarginRight);
            this.ContentMargin.AddThemeConstantOverride("margin_top", backboardList.MarginTop);
            this.ContentMargin.AddThemeConstantOverride("margin_bottom", backboardList.MarginBottom);
            GD.Print($"[Backboard #{runId}] List[{listIdx}] applied margins L={backboardList.MarginLeft}, R={backboardList.MarginRight}, T={backboardList.MarginTop}, B={backboardList.MarginBottom}");

            if (backboardList.Backboards == null || backboardList.Backboards.Count == 0)
            {
                GD.PushWarning($"[Backboard #{runId}] List[{listIdx}] Backboards is null or empty");
                listIdx++;
                continue;
            }

            int bbIdx = 0;
            foreach (QuestBackboard backboard in backboardList.Backboards)
            {
                if (backboard == null)
                {
                    GD.PushWarning($"[Backboard #{runId}] List[{listIdx}] Backboard[{bbIdx}] is null, skipping");
                    bbIdx++;
                    continue;
                }

                GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx} '{backboard.WidthKey}'] Setting Max={backboard.MaxSize}, Min={backboard.MinSize}, Size={backboard.MaxSize} (t={totalSw.ElapsedMilliseconds}ms)");
                this.CustomMaximumSize = backboard.MaxSize;
                this.CustomMinimumSize = backboard.MinSize;
                this.Size = this.CustomMaximumSize;

                stepSw.Restart();
                GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] Awaiting ProcessFrame #1...");
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] Resumed ProcessFrame #1 in {stepSw.ElapsedMilliseconds}ms (PopupSize={this.Size}, ContentSize={this.Content.Size})");

                stepSw.Restart();
                GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] Setting QuestText & calling Rebuild()...");
                this.QuestText.Text = text;
                this.QuestText.Rebuild();
                GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] Rebuild() finished in {stepSw.ElapsedMilliseconds}ms (QuestText child count={this.QuestText.GetChildCount()})");

                stepSw.Restart();
                GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] Awaiting ProcessFrame #2...");
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] Resumed ProcessFrame #2 in {stepSw.ElapsedMilliseconds}ms (ContentSize={this.Content.Size})");

                Vector2 size = this.Content.GetCombinedMinimumSize();
                GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] Content min size={size} vs Constraints={backboard.ContentConstraints}");

                float overflowX = Math.Max(0f, size.X - backboard.ContentConstraints.X);
                float overflowY = Math.Max(0f, size.Y - backboard.ContentConstraints.Y);
                GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] overflowX={overflowX}, overflowY={overflowY}");

                if (overflowX == 0f && overflowY == 0f)
                {
                    exactMatch = backboard;
                    GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] EXACT MATCH FOUND! Breaking inner loop.");
                    break;
                }

                float totalOverflow = overflowX + overflowY;
                float area = backboard.ContentConstraints.X * backboard.ContentConstraints.Y;

                bool beatsOverflow = totalOverflow < leastOverflow;
                bool tiesOverflowBetterArea = Math.Abs(totalOverflow - leastOverflow) < 0.001f && area < tieBreakerArea;

                if (beatsOverflow || tiesOverflowBetterArea)
                {
                    leastOverflow = totalOverflow;
                    tieBreakerArea = area;
                    leastOverflowFallback = backboard;
                    GD.Print($"[Backboard #{runId}] [L{listIdx}:B{bbIdx}] New fallback candidate (totalOverflow={totalOverflow}, area={area})");
                }

                bbIdx++;
            }

            if (exactMatch != null)
            {
                GD.Print($"[Backboard #{runId}] Breaking outer loop due to exact match.");
                break;
            }

            listIdx++;
        }

        GD.Print($"[Backboard #{runId}] Finished loops in {totalSw.ElapsedMilliseconds}ms. exactMatch!=null: {exactMatch != null}, fallback!=null: {leastOverflowFallback != null}");

        QuestBackboard chosen = exactMatch ?? leastOverflowFallback;

        if (chosen == null)
        {
            GD.PushWarning($"[Backboard #{runId}] Backboard cannot be found for size current Quest");
            return;
        }

        if (exactMatch == null)
        {
            GD.PushWarning($"[Backboard #{runId}] No exact fit; using least-overflow backboard ({chosen.MaxSize}, overflow={leastOverflow:0.#}px)");
        }
        else
        {
            GD.Print($"[Backboard #{runId}] Size found: {chosen.MaxSize}");
        }

        this.CustomMaximumSize = chosen.MaxSize;
        this.CustomMinimumSize = chosen.MinSize;
        this.Background.Texture = chosen.Texture;
        GD.Print($"[Backboard #{runId}] Applied chosen backboard Max={chosen.MaxSize}, Min={chosen.MinSize}, Texture={chosen.Texture?.ResourcePath}");

        stepSw.Restart();
        GD.Print($"[Backboard #{runId}] Awaiting final ProcessFrame before ShrinkToFit()...");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GD.Print($"[Backboard #{runId}] Final ProcessFrame resumed in {stepSw.ElapsedMilliseconds}ms");

        ShrinkToFit(runId);
        GD.Print($"[Backboard #{runId}] Done in {totalSw.ElapsedMilliseconds}ms total.");
    }

    /// <summary>
    /// Shrinks the MarginContainer to fit its current visible children + margin theme constants.
    /// </summary>
    private void ShrinkToFit(int runId = -1)
    {
        Vector2 minSize = GetCombinedMinimumSize();
        GD.Print($"[Backboard #{runId}] ShrinkToFit: old Size={this.Size}, new minSize={minSize}");
        this.Size = minSize;
    }
}