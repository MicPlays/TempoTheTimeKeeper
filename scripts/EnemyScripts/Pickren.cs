using Godot;
using System;

public partial class Pickren : EnemyBase, IAttackableKnockback
{
    public static PackedScene projectile;
    [Export]
    public NodePath gcPath;
    public SimpleGroundCollision gc;
    [Export]
    public NodePath spritePath;
    public AnimatedSprite2D sprite;
    [Export]
    public NodePath physicsPath;
    public SimpleEnemyPhysics physics;
    [Export]
    public NodePath pcPath;
    public SimplePushCollision pc;
    public Timer stunTimer;
    public Timer projectileSpawnTimer;
    public Timer attackDelayTimer;
    [Export]
    public float stunTimerMax = 20f;
    [Export]
    public float projectileSpawnTime = 0.5f;
    [Export]
    public float attackDelayTime = 2f;
    public bool stunned = false;
    public bool attacking = false;
    [Export]
    public float detectionRange = 100f;

    public override void _Ready()
    {
        screenNotifier = GetNode<VisibleOnScreenNotifier2D>(screenNotifierPath);
        screenNotifier.ScreenEntered += EnableObject;
        screenNotifier.ScreenExited += DisableObject;
        screenNotifier.Visible = true;
        spawnCoords = GlobalPosition;
        hitbox = GetNode<Hitbox>(hitboxPath);

        gc = GetNode<SimpleGroundCollision>(gcPath);
        gc.collider = this;
        gc.Init();

        pc = GetNode<SimplePushCollision>(pcPath);
        pc.collider = this;
        pc.Init();

        physics = GetNode<SimpleEnemyPhysics>(physicsPath);
        physics.enemy = this;
        sprite = GetNode<AnimatedSprite2D>(spritePath);

        stunTimer = new Timer();
        stunTimer.OneShot = true;
        AddChild(stunTimer);
        stunTimer.Timeout += OnStunTimeout;

        projectileSpawnTimer = new Timer();
        projectileSpawnTimer.OneShot = true;
        AddChild(projectileSpawnTimer);
        projectileSpawnTimer.Timeout += OnProjectileTimer;

        attackDelayTimer = new Timer();
        attackDelayTimer.OneShot = true;
        AddChild(attackDelayTimer);
        attackDelayTimer.Timeout += OnAttackTimeout;
        GlobalPosition = spawnCoords;

        sprite.AnimationFinished += OnAttackFinished;
    }

    public override void EnableObject()
    {
        SetPhysicsProcess(true);
        sprite.Play("idle");
        int direction = Mathf.Sign(LevelManager.Instance.GetLevel().player.GlobalPosition.X - GlobalPosition.X);
        if (direction > 0)
        {
            sprite.FlipH = true;
        }
            
    }

    public override void DisableObject()
    {
        SetPhysicsProcess(false);
        if (health <= 0)
            QueueFree();
    }

    public override void _PhysicsProcess(double delta)
    {
        float deltaTime = (float)delta;
        bool pushCollision = pc.PushCollisionProcess();
        if (pushCollision) xSpeed = 0;
        physics.MoveEnemyObject();
        physics.ApplyFriction(deltaTime);
        bool groundCollision = gc.GroundCollisionProcess();
        if (!groundCollision) physics.ApplyGravity(deltaTime);
        else ySpeed = 0;

        if (stunned)
        {
            if (xSpeed == 0 && ySpeed == 0)
            {
                stunned = false;
                sprite.Play("idle");
            }
        }
        else
        {
            if (!attacking)
            {
                float playerX = LevelManager.Instance.GetLevel().player.GlobalPosition.X;
                if (sprite.FlipH)
                {
                    if (playerX < GlobalPosition.X + detectionRange)
                    {
                        sprite.Play("attack");
                        projectileSpawnTimer.Start(projectileSpawnTime);
                        attackDelayTimer.Start(attackDelayTime);
                        attacking = true;
                    }
                }
                else
                {
                    if (playerX > GlobalPosition.X - detectionRange)
                    {
                        sprite.Play("attack");
                        projectileSpawnTimer.Start(projectileSpawnTime);
                        attackDelayTimer.Start(attackDelayTime);
                        attacking = true;
                    }
                }
            }
        }
    }


    public void Damage(float damage, float knockbackForce, Vector2 knockbackDirection)
    {
        health -= damage;
        physics.ApplyKnockback(knockbackForce, knockbackDirection, (float)GetPhysicsProcessDeltaTime());
        if (health <= 0)
        {
            stunTimer.Paused = true;
            stunTimer.Timeout -= OnStunTimeout;
            projectileSpawnTimer.Paused = true;
            projectileSpawnTimer.Timeout -= OnProjectileTimer;
            attackDelayTimer.Paused = true;
            attackDelayTimer.Timeout -= OnAttackTimeout;
            sprite.AnimationFinished -= OnAttackFinished;
            hitbox.SetDeferred("monitorable", false);
            hitbox.SetDeferred("monitoring", false);
            sprite.Play("death");
            
        }
        else 
        {
            if (ySpeed < 0 || xSpeed != 0)
            {
                sprite.Play("knocked");
                stunned = true;
            }
                
            else 
            {
                stunTimer.Start(stunTimerMax);
                sprite.Play("hurt");
            }
        }
        
    }

    public void OnStunTimeout()
    {
        sprite.Play("idle");
    }

    public void OnProjectileTimer()
    {
        GD.Print("spawn projectile");
        PickrenProjectile proj = (PickrenProjectile)projectile.Instantiate();
        LevelManager.Instance.GetLevel().projectileContainer.AddChild(proj);
        if (sprite.FlipH)
        {
            proj.direction = 1;
            proj.sprite.FlipH = true;
        } 
        else proj.direction = -1;
        proj.GlobalPosition = GlobalPosition;
    }

    public void OnAttackTimeout()
    {
        attacking = false;
    }

    public void OnAttackFinished()
    {
        if (sprite.Animation == "attack")
        {
            if (health > 0) sprite.Play("idle");
        }
        
    }

    public static void LoadResources()
    {
        projectile = GD.Load<PackedScene>(PackedSceneConstants.PickrenProjectile);
    }
}
