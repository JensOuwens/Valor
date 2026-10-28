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
        public double duration;
    }
    
    [Export] private AnimationPlayer animationPlayer;
    [Export] private Node3D handOne;
    [Export] private Node3D handTwo;
    private PlayerAnimation currentAnimation;

    public override void _Ready()
    {
        if (!CanPlayAnimation()) return;
        this.animationPlayer.AnimationFinished += AnimationPlayerOnAnimationFinished;
    }

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

        if (CanAddToHand(hand)) hand.AddChild(newItem);
        else GD.PrintErr("hand is not assigned");
    }
    
    // TODO: Remove Item from hand func

    public void PlayAnim(PlayerAnimation animation)
    {
        if (!CanPlayAnimation()) return;
        if (animation.priority < this.currentAnimation.priority) return;
        this.currentAnimation = animation;

        string animString = AnimEnumToString(this.currentAnimation.name);
        
        switch (this.currentAnimation.name)
        {
            case EPlayerAnimNames.Idle:
            case EPlayerAnimNames.Walk:
            case EPlayerAnimNames.Heal:
            case EPlayerAnimNames.Dodge:
            case EPlayerAnimNames.Attack:
                PlayAnimationWithDuration(animString, this.currentAnimation.duration);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void PlayAnimationWithDuration(string animName, double targetDuration)
    {
        double customSpeed = 1.0f;

        // If a specific duration is requested (non 0 duration), compute speed scale
        if (targetDuration > 0.0f)
        {
            Animation anim = this.animationPlayer.GetAnimation(animName);
            if (anim != null && anim.Length > 0.0f) 
                customSpeed = anim.Length / targetDuration;
        }

        this.animationPlayer.Play(animName, customBlend: -1.0f, customSpeed: (float)customSpeed);
    }

    private string AnimEnumToString(EPlayerAnimNames name)
    {
        return name switch
        {
            EPlayerAnimNames.Idle => "Idle",
            EPlayerAnimNames.Walk => "Walk",
            EPlayerAnimNames.Heal => "Heal",
            EPlayerAnimNames.Dodge => "Dodge",
            EPlayerAnimNames.Attack => "Attack0",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private EPlayerAnimNames AnimStringToEnum(string name)
    {
        return name switch
        {
            "Idle" => EPlayerAnimNames.Idle,
            "Walk" => EPlayerAnimNames.Walk,
            "Heal" => EPlayerAnimNames.Heal,
            "Dodge" => EPlayerAnimNames.Dodge,
            "Attack0" => EPlayerAnimNames.Attack,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private void AnimationPlayerOnAnimationFinished(StringName animName)
    {
        if (AnimStringToEnum(animName) == this.currentAnimation.name) 
            this.currentAnimation.priority = 0;
    }
}