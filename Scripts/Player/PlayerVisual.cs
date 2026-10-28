using Godot;

namespace Valor;

/// <summary>
/// Holds player visual helper functions
/// </summary>
public partial class PlayerVisual : Node3D
{
    [Export] private AnimationPlayer animationPlayer;
    [Export] private Node3D handOne;
    [Export] private Node3D handTwo;

    // public override void _Ready() => 
    //     this.animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    
    private bool CanPlayAnimation() => this.animationPlayer != null;
    private bool CanAddToHand(Node3D hand) => hand != null && hand.GetChildCount() == 0;
    
    public void AddItemToHandOne(Node3D newItem) => AddItemToHand(this.handOne, newItem);
    public void AddItemToHandTwo(Node3D newItem) => AddItemToHand(this.handTwo, newItem);
    private void AddItemToHand(Node3D hand, Node3D newItem)
    {
        if (newItem == null)
        {
            GD.PrintErr("Provided item is null");
            return;
        }

        if (CanAddToHand(hand))
            hand.AddChild(newItem);
        else
            GD.PrintErr("hand is not assigned");
    }
    
    // TODO: Remove Item from hand func

    public void PlayIdleAnim()
    {
        if (!CanPlayAnimation()) return;
        this.animationPlayer.Play("Idle");
    }
    
    public void PlayWalkAnim()
    {
        if (!CanPlayAnimation()) return;
        this.animationPlayer.Play("Walk");
    }
    
    public void PlayAttackAnim()
    {
        if (!CanPlayAnimation()) return;
        this.animationPlayer.Play("Attack0");
    }
    
    public void PlayHealAnim()
    {
        if (!CanPlayAnimation()) return;
        this.animationPlayer.Play("Heal");
    }
    
    public void PlayDodgeAnim()
    {
        if (!CanPlayAnimation()) return;
        this.animationPlayer.Play("Dodge");
    }
}