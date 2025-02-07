using Godot;
using System;
using System.Collections.Generic;

public partial class Player : GameObject
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

    //player stats
    public int noteCount = 0;

    public override void _Ready()
    {
        //set player object properties (might make export vars later)
        xSpeed = 0f;
        ySpeed = 0f;
        groundAngle = 0f;
        groundSpeed = 0f;
        widthRadius = 9;
        heightRadius = 20;
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
                hitbox.SetCollisionLayerValue(1, true);
            }
        }
    }
}
