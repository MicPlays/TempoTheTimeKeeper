using Godot;
using System;
using System.Collections.Generic;

public partial class Tempo : Player
{
    public float speedBoostInputTimer = 0;
    public float xSpeedBuffer = 0;
    [Export]
    public NodePath attackBoxPath {get; set;}
    public Area2D attackBox;
    [Export]
    public NodePath regularSpritePath;
    [Export]
    public NodePath sticklessSpritePath;
    public AnimatedSprite2D regularSprite;
    public AnimatedSprite2D sticklessSprite;

    public override void _Ready()
    {
        base._Ready();
        attackBox = GetNode<Area2D>(attackBoxPath);
        attackBox.AreaEntered += AttackBoxCollision;
        TempoCollisionComponent tcc = (TempoCollisionComponent)cc;
        tcc.ToggleAttackHitbox(false);

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
                ((IAttackable)attackable.parentObject).Damage();
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

    public override void Damage()
    {
        if (health == 0)
            psm.TransitionState(new PlayerDeath());
        else
        {
            HUD.Instance.SetHealth(health - 1);
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
            HUD.Instance.SetHealth(health);
            health++;
            return true;
        }
    }
}
