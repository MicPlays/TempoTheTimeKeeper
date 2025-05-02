using Godot;
using System;

public partial class Tambaroo : EnemyBase, IAttackableKnockback
{
    [Export]
    public NodePath spritePath;
    public AnimatedSprite2D sprite;
    [Export]
    public NodePath physicsPath;
    public SimpleEnemyPhysics physics;
    [Export]
    public NodePath gcPath;
    public SimpleGroundCollision gc;
    [Export]
    public NodePath pcPath;
    public SimplePushCollision pc;
    public Timer stunTimer;
    [Export]
    public float stunTimerMax = 20f;
    [Export]
    public NodePath attackBoxPath;
    public AttackHitbox attackHitbox;
    public bool attacking;
    public bool stunned;
    [Export]
    public int attackRadius = 26;
    [Export]
    public float detectionRange = 100f;

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
        stunned = false;
        attacking = false;

        sprite.AnimationFinished += OnAnimationFinished;

        attackHitbox = GetNode<AttackHitbox>(attackBoxPath);
        attackHitbox.damage = 1;
        attackHitbox.knockbackAmount = 0;
        attackHitbox.AreaEntered += AttackBoxCollision;
    }

    public override void _PhysicsProcess(double delta)
    {
        float deltaTime = (float)delta;
        bool pushCollision = pc.PushCollisionProcess();
        if (pushCollision) 
        {
            xSpeed *= -1;
            if (!stunned) sprite.FlipH = true;
        }
        physics.MoveEnemyObject();
        bool groundCollision = gc.GroundCollisionProcess();
        if (!groundCollision) physics.ApplyGravity(deltaTime);
        else ySpeed = 0;   

        if (stunned)
        {
            physics.ApplyFriction(deltaTime);
            if (xSpeed == 0 && ySpeed == 0)
            {
                if (health > 0)
                {
                    stunned = false;
                    sprite.Play("walk");
                    if (sprite.FlipH) xSpeed = physics.MOVE_SPEED * deltaTime;
                    else xSpeed = -physics.MOVE_SPEED * deltaTime;
                }
            }
        }
        else
        {
            if (!attacking)
            {
                bool canAttack = AttackRadiusCheck();
                if (canAttack)
                {
                    sprite.Play("attack");
                    xSpeed = 0;
                    attacking = true;
                }
            }
            else
            {
                if (sprite.FlipH)
                    attackHitbox.Position = new Vector2(attackRadius, attackHitbox.Position.Y);
                else attackHitbox.Position = new Vector2(-attackRadius, attackHitbox.Position.Y);
                if (sprite.Frame == 9)
                    ToggleAttackHitbox(true);
                else if (sprite.Frame == 12)
                    ToggleAttackHitbox(false);
            }
        }
    }

    public void Damage(float damage, float knockbackForce, Vector2 knockbackDirection)
    {
        ToggleAttackHitbox(false);
        health -= damage;
        xSpeed = 0;
        physics.ApplyKnockback(knockbackForce, knockbackDirection, (float)GetPhysicsProcessDeltaTime());
        if (health <= 0)
        {
            LevelManager.Instance.GetLevel().AddScore(200);
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

    public override void EnableObject()
    {
        SetPhysicsProcess(true);
        sprite.Play("walk");
        ToggleAttackHitbox(false);
        xSpeed = -physics.MOVE_SPEED * (float)GetPhysicsProcessDeltaTime();
    }

    public override void DisableObject()
    {
        SetPhysicsProcess(false);
        if (health <= 0)
            QueueFree();
    }

    public void OnAnimationFinished()
    {
        if (attacking)
        {
            bool canAttack = AttackRadiusCheck();
            if (canAttack) sprite.Play("attack");
            else
            {
                sprite.Play("walk");
                if (sprite.FlipH)
                    xSpeed = physics.MOVE_SPEED *(float)GetPhysicsProcessDeltaTime();
                else xSpeed = -physics.MOVE_SPEED *(float)GetPhysicsProcessDeltaTime();
                attacking = false;
            }
        }
    }

    public bool AttackRadiusCheck()
    {
        float playerX = LevelManager.Instance.GetLevel().player.GlobalPosition.X;
        int playerDir = Mathf.Sign(GlobalPosition.X - playerX);
        if (playerDir < 0)
        {
            if (playerX < GlobalPosition.X + detectionRange) return true;
            else return false;
        }
        else
        {
            if (playerX > GlobalPosition.X - detectionRange) return true;
            else return false;
        }
    }

    public void OnStunTimeout()
    {
        bool canAttack = AttackRadiusCheck();
        if (canAttack) sprite.Play("attack");
        else
        {
            sprite.Play("walk");
            if (sprite.FlipH)
                xSpeed = physics.MOVE_SPEED *(float)GetPhysicsProcessDeltaTime();
            else xSpeed = -physics.MOVE_SPEED *(float)GetPhysicsProcessDeltaTime();
            attacking = false;
        }
    }

    public void ToggleAttackHitbox(bool toggle)
    {
        attackHitbox.SetDeferred("monitorable", toggle);
        attackHitbox.SetDeferred("monitoring", toggle);
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

}   
