using Godot;
using System;
using System.Collections.Generic;

public partial class Tempo : Player
{
    public float speedBoostInputTimer = 0;
    public float xSpeedBuffer = 0;
    [Export]
    public int maxAttackCombo;
    [Export]
    public int attackRadius;
    [Export]
    public NodePath attackBoxPath {get; set;}
    public AttackHitbox attackBox;
    [Export]
    public NodePath regularSpritePath;
    [Export]
    public NodePath sticklessSpritePath;
    [Export]
    public float comboTimerMax;
    [Export]
    public float lungeTimerMax;
    [Export]
    public float airAttackMax;
    [Export]
    public float airAttackSpriteTime;
    public AnimatedSprite2D regularSprite;
    public AnimatedSprite2D sticklessSprite;
    public bool pushingAgainstObject;
    public bool pushingLeft;

    public override void _Ready()
    {
        base._Ready();
        attackBox = GetNode<AttackHitbox>(attackBoxPath);
        attackBox.AreaEntered += AttackBoxCollision;
        TempoCollisionComponent tcc = (TempoCollisionComponent)cc;
        tcc.ToggleAttackHitbox(false, 0);

        regularSprite = GetNode<AnimatedSprite2D>(regularSpritePath);
        sticklessSprite = GetNode<AnimatedSprite2D>(sticklessSpritePath);
        playerSprite = regularSprite;
        sticklessSprite.Visible = false;
    }

    public void AttackBoxCollision(Area2D area)
    {
        Hitbox attackable = (Hitbox)area;
        if (attackable != null)
        {
            if (attackable.parentObject is IAttackable)
            {
                ((IAttackable)attackable.parentObject).Damage(attackBox.damage);
                if (psm.CurrentState is TempoAerialAttack)
                {
                    TempoPhysicsComponent tpc = (TempoPhysicsComponent)pc;
                    tpc.AerialAttackHit();
                }
            }
                
        }
    }

    public void ToggleSticks(bool toggle)
    {
        if (toggle)
        {
            playerSprite = regularSprite;
            regularSprite.Visible = true;
            sticklessSprite.Visible = false;
        }
        else
        {
            playerSprite = sticklessSprite;
            sticklessSprite.Visible = true;
            regularSprite.Visible = false;
        }
    }

    public override void Damage(float amount)
    {
        if (health == 0)
            psm.TransitionState(new PlayerDeath());
        else
        {
            LevelManager.Instance.GetLevel().hud.SetHealth(health - 1);
            health--;
            if (health == 0)
                ToggleSticks(false);
            psm.TransitionState(new PlayerHurt());
        }
    }

    public override bool Heal()
    {
        if (health == maxHealth)
            return false;
        else 
        {
            if (health == 0) ToggleSticks(true);
            LevelManager.Instance.GetLevel().hud.SetHealth(health);
            health++;
            return true;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("attack"))
        {
            if (psm.CurrentState is PlayerFall || psm.CurrentState is PlayerJump)
            {
                psm.TransitionState(new TempoAerialAttack());
            }
        }
        if (Input.IsActionPressed("jump"))
        {
            if (psm.CurrentState is PlayerFall)
            {
                TempoPhysicsComponent tpc = (TempoPhysicsComponent)pc;
                tpc.canBoostJump = false;
            }
        }
        if (@event.IsActionReleased("jump"))
        {
            if (psm.CurrentState is PlayerFall)
            {
                TempoPhysicsComponent tpc = (TempoPhysicsComponent)pc;
                tpc.canBoostJump = true;
            }
        }
    }

    public override void SetState(int stateNum)
    {
        switch (stateNum)
        {
            case (int)TempoStates.Grounded:
                psm.TransitionState(new TempoGrounded());
                break;
            case (int)TempoStates.Fall:
                psm.TransitionState(new PlayerFall());
                break;
            case (int)TempoStates.Jump:
                psm.TransitionState(new PlayerJump());
                break;
            case (int)TempoStates.WallJump:
                psm.TransitionState(new TempoWallJump());
                break;
            case (int)TempoStates.Lunge:
                psm.TransitionState(new TempoLunge());
                break;
            case (int)TempoStates.Hurt:
                psm.TransitionState(new PlayerHurt());
                break;
            case (int)TempoStates.Death:
                psm.TransitionState(new PlayerDeath());
                break;
            case (int)TempoStates.LungeTransition:
                psm.TransitionState(new TempoLungeTransition());
                break;
        }
    }
}
public enum TempoStates
{
    Grounded,
    Fall,
    Jump,
    WallJump,
    Lunge,
    Hurt,
    Death,
    LungeTransition
}
