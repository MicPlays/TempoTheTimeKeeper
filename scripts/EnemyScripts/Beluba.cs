using Godot;
using System;

public partial class Beluba : EnemyBase, IAttackableKnockback
{
    [Export]
    public NodePath spritePath;
    public AnimatedSprite2D sprite;
    [Export]
    public NodePath physicsPath;
    public BelubaPhysics physics;
    [Export]
    public NodePath gcPath;
    public SimpleGroundCollision gc;
    public Timer stunTimer;
    [Export]
    public float stunTimerMax = 20f;
    [Export]
    public NodePath attackBoxPath;
    public AttackHitbox attackHitbox;
    public bool stunned = false;
    public bool falling = false;
    public bool recovering = false;
    public bool inflating = false;
    public bool flailing = false;
    [Export]
    public float xDetectionRadius =  20f;
    [Export]
    public float yDetectionRadius = 100f;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        screenNotifier = GetNode<VisibleOnScreenNotifier2D>(screenNotifierPath);
        screenNotifier.ScreenEntered += EnableObject;
        screenNotifier.ScreenExited += DisableObject;
        screenNotifier.Visible = true;
        spawnCoords = GlobalPosition;
        hitbox = GetNode<Hitbox>(hitboxPath);

        gc = GetNode<SimpleGroundCollision>(gcPath);
        gc.collider = this;
        gc.Init();

        physics = GetNode<BelubaPhysics>(physicsPath);
        physics.enemy = this;
        sprite = GetNode<AnimatedSprite2D>(spritePath);

        stunTimer = new Timer();
        stunTimer.OneShot = true;
        AddChild(stunTimer);
        stunTimer.Timeout += OnStunTimeout;
        stunned = false;

        sprite.AnimationFinished += OnAnimationFinished;

        attackHitbox = GetNode<AttackHitbox>(attackBoxPath);
        attackHitbox.damage = 1;
        attackHitbox.knockbackAmount = 0;
        attackHitbox.AreaEntered += AttackBoxCollision;
    }

    public override void _PhysicsProcess(double delta)
    {
        float deltaTime = (float)delta;
        if (stunned)
        {
            physics.Fall(deltaTime);
            physics.MoveEnemyObject();
            bool groundCollision = gc.GroundCollisionProcess();
            if (groundCollision)
            {
                ySpeed = 0;
                stunned = false;
                flailing = true;
                stunTimer.Start(stunTimerMax);
            }
        }
        else if (falling)
        {
            physics.Fall(deltaTime);
            physics.MoveEnemyObject();
            bool groundCollision = gc.GroundCollisionProcess();
            if (groundCollision)
            {
                ySpeed = 0;
                falling = false;
                sprite.Play("crash");
                ToggleAttackHitbox(false);
            }
        }
        else if (recovering)
        {
            physics.Rise(deltaTime);
            float predictedPosition = GlobalPosition.Y + ySpeed;
            if (predictedPosition <= spawnCoords.Y)
            {
                GlobalPosition = spawnCoords;
                recovering = false;
                ySpeed = 0;
                sprite.Play("float");
            }
            else GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Y + ySpeed);
        }
        else if (!flailing)
        {
            bool canAttack = AttackRadiusCheck();
            if (canAttack)
            {
                sprite.Play("inflate");
                inflating = true;
            }
        }
    }

    public void Damage(float damage, float knockbackForce, Vector2 knockbackDirection)
    {
        LevelManager.Instance.GetLevel().AddScore(200);
        ToggleAttackHitbox(false);
        health -= damage;
        physics.ApplyKnockback(knockbackForce, Vector2.Down, (float)GetPhysicsProcessDeltaTime());
        if (health <= 0)
        {
            stunned = true;
            stunTimer.Paused = true;
            stunTimer.Timeout -= OnStunTimeout;
            sprite.AnimationFinished -= OnAnimationFinished;
            hitbox.SetDeferred("monitorable", false);
            hitbox.SetDeferred("monitoring", false);
            attackHitbox.SetDeferred("monitorable", false);
            attackHitbox.SetDeferred("monitoring", false);
            attackHitbox.AreaEntered -= AttackBoxCollision;
            sprite.Play("death");
        }
        else if (flailing || sprite.Animation == "crash")
        {
            stunTimer.Start(stunTimerMax);
            sprite.Play("hurt");
        }
        else 
        {
            sprite.Play("knocked");
            stunned = true;
        }
    }

    public override void EnableObject()
    {
        SetPhysicsProcess(true);
        sprite.Play("float");
        ToggleAttackHitbox(false);
    }

    public override void DisableObject()
    {
        SetPhysicsProcess(false);
        if (health <= 0)
            QueueFree();
    }

    public void OnStunTimeout()
    {
        if (health > 0)
        {
            recovering = true;
            flailing = false;
            sprite.Play("recover");
        }
    }

    public void OnAnimationFinished()
    {
        if (sprite.Animation == "inflate")
        {
            sprite.Play("fall");
            ToggleAttackHitbox(true);
            falling = true;
            inflating = false;
        }
        else if (sprite.Animation == "crash")
        {
            sprite.Play("recover");
            recovering = true;
            ToggleAttackHitbox(false);
        }
        else if (sprite.Animation == "recover")
        {
            sprite.Play("float");
        }
        else if (sprite.Animation == "hurt")
            sprite.Play("flailing");
    }

    public void AttackBoxCollision(Area2D area)
    {
        Hitbox attackable = (Hitbox)area;
        if (attackable != null)
        {
            if (attackable.parentObject is Player)
            {
                ((IAttackable)attackable.parentObject).Damage(1);
            }
        }
    }

    public void ToggleAttackHitbox(bool toggle)
    {
        attackHitbox.SetDeferred("monitorable", toggle);
        attackHitbox.SetDeferred("monitoring", toggle);
    }

    public bool AttackRadiusCheck()
    {
        Vector2 playerLoc = LevelManager.Instance.GetLevel().player.GlobalPosition;
        if (Mathf.Abs(GlobalPosition.X - playerLoc.X) <= xDetectionRadius)
        {
            if (playerLoc.Y > GlobalPosition.Y && playerLoc.Y < GlobalPosition.Y + yDetectionRadius)
                return true;
            else return false;
        }
        else return false;
    }
}
