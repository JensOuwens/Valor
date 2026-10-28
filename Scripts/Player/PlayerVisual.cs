using System;
using Godot;

namespace Valor;

/// <summary>
/// Holds player visual helper functions
/// </summary>
public partial class PlayerVisual : Node3D
{
    public enum EPlayerAnimNames
    {
        Idle,
        Walk,
        Heal,
        Dodge,
        Attack
    }

    public struct PlayerAnimation
    {
        public EPlayerAnimNames name;
        public int priority;
    }
    
    [Export] private AnimationPlayer animationPlayer;
    [Export] private Node3D handOne;
    [Export] private Node3D handTwo;
    private PlayerAnimation currentAnimation;
    
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

    public void PlayAnim(PlayerAnimation animation)
    {
        if (!CanPlayAnimation()) return;
        if (animation.priority < this.currentAnimation.priority) return;
        this.currentAnimation = animation;

        switch (this.currentAnimation.name)
        {
            case EPlayerAnimNames.Idle:
                this.animationPlayer.Play("Idle");
                break;
            case EPlayerAnimNames.Walk:
                this.animationPlayer.Play("Walk");
                break;
            case EPlayerAnimNames.Heal:
                this.animationPlayer.Play("Heal");
                break;
            case EPlayerAnimNames.Dodge:
                this.animationPlayer.Play("Dodge");
                break;
            case EPlayerAnimNames.Attack:
                this.animationPlayer.Play("Attack0");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}