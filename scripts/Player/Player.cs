using Godot;
using System;
using System.Collections.Generic;

public partial class Player : GameObject, IAttackable
{
    //Components and their NodePaths
    [Export]
    public NodePath spritePath;
    [Export]
    public NodePath stateMachinePath;
    [Export]
    public NodePath collisionPath;
    [Export]
    public NodePath physicsPath;
    [Export]
    public int pushRadius = 10;
    public PlayerCollisionComponent cc;
    public PlayerPhysicsComponent pc;
    public PlayerStateMachine psm;
    public AnimatedSprite2D playerSprite;
    //player's current collision layer
    public int currentLayer;
    public int currentFrame;
    public float controlLockTimer = 0;
    public float invulnTimer = 0;
    public bool isInvuln = false;
    public float invulnFlashTimer;
    public bool flashActive = false;
    [Export]
    public float invulnLength {get; set;} = 120f;
    [Export]
    public float invulnFlashInterval {get; set;} = 30f;
    [Export]
    public int maxHealth = 3;
    public int health = 3;
    public bool standingOnObject = false;

    //player stats
    public int noteCount = 0;

    public override void _Ready()
    {
        //set player object properties (might make export vars later)
        xSpeed = 0f;
        ySpeed = 0f;
        groundAngle = 0f;
        groundSpeed = 0f;
        currentLayer = 0;

        //get player components
        playerSprite = GetNode<AnimatedSprite2D>(spritePath);
        cc = GetNode<PlayerCollisionComponent>(collisionPath);
        cc.player = this;
        hitbox = cc;
        cc.Init();
        pc = GetNode<PlayerPhysicsComponent>(physicsPath);
        pc.player = this;
        psm = GetNode<PlayerStateMachine>(stateMachinePath);
        psm.player = this;
        psm.SetState(new PlayerFall());
    }

    public override void _PhysicsProcess(double delta)
    {
        //handle invulnerability (extends across multiple states)
        if (isInvuln)
        {
            invulnTimer += (float)delta;
            if (flashActive)
            {
                invulnFlashTimer += (float)delta;
                if (invulnFlashTimer <= invulnFlashInterval * (float)delta)
                    playerSprite.Visible = !playerSprite.Visible;
                else invulnFlashTimer = 0;
            }
            if (invulnTimer > invulnLength * (float)delta)
            {
                playerSprite.Visible = true;
                isInvuln = false;
                invulnTimer = 0;
                invulnFlashTimer = 0;
                flashActive = false;
                hitbox.SetCollisionMaskValue(3, true);
            }
        }
        if (GlobalPosition.Y >= LevelManager.Instance.GetLevel().killbarrierY && !(psm.CurrentState is PlayerDeath))
        {
            psm.TransitionState(new PlayerDeath());
        }
    }

    public virtual void Damage(float amount)
    {
        if (!isInvuln)
        {
            if (health == 0)
                psm.TransitionState(new PlayerDeath());
            else
            {
                LevelManager.Instance.GetLevel().hud.SetHealth(health - 1);
                health--;
                psm.TransitionState(new PlayerHurt());
            }
        }
        
    }
    
    public virtual bool Heal()
    {
        if (health == maxHealth)
            return false;
        else 
        {
            LevelManager.Instance.GetLevel().hud.SetHealth(health);
            health++;
            return true;
        }
    }

    public override void _Draw()
    {
        /*
        Rect2 rect = new Rect2(-pushRadius, -heightRadius, pushRadius * 2, heightRadius * 2);
        GD.Print(rect);
        DrawRect(rect, new Color(Colors.Orange));
        */
    }

    public virtual void SetState(int stateNum)
    {
        switch (stateNum)
        {
            case (int)PlayerStates.Grounded:
                psm.TransitionState(new PlayerGrounded());
                break;
            case (int)PlayerStates.Fall:
                psm.TransitionState(new PlayerFall());
                break;
            case (int)PlayerStates.Jump:
                psm.TransitionState(new PlayerJump());
                break;
            case (int)PlayerStates.Hurt:
                psm.TransitionState(new PlayerHurt());
                break;
            case (int)PlayerStates.Death:
                psm.TransitionState(new PlayerDeath());
                break;
        }
    }
    
    public void ApplyKnockback(float knockbackAmount, Vector2 knockbackDirection){}
}

public enum PlayerStates
{
    Grounded,
    Fall,
    Jump,
    Hurt,
    Death
}


