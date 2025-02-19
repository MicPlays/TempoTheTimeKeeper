using Godot;
using System;

public partial class TestEnemy : EnemyBase, IAttackable
{
    [Export]
    public NodePath gcPath;
    public SimpleGroundCollision gc;
    [Export]
    public NodePath pcPath;
    public SimplePushCollision pc;
    [Export]
    public float MOVE_SPEED {get; set;}

    public override void _Ready()
    {
        screenNotifier = GetNode<VisibleOnScreenNotifier2D>(screenNotifierPath);
        screenNotifier.ScreenEntered += EnableObject;
        screenNotifier.ScreenExited += DisableObject;
        spawnCoords = GlobalPosition;
        hitbox = GetNode<Hitbox>(hitboxPath);
        gc = GetNode<SimpleGroundCollision>(gcPath);
        gc.collider = this;
        gc.Init();
        pc = GetNode<SimplePushCollision>(pcPath);
        pc.collider = this;
        pc.Init();
    }

    public override void _PhysicsProcess(double delta)
    {
        bool pushCollision = pc.PushCollisionProcess();
        if (pushCollision) xSpeed *= -1;

        GlobalPosition = new Vector2(xSpeed + GlobalPosition.X, GlobalPosition.Y);

        bool groundCollision = gc.GroundCollisionProcess();
        if (!groundCollision)
        {
            xSpeed *= -1;
        } 
        
    }

    public override void EnableObject()
    {
        GlobalPosition = spawnCoords;
        SetPhysicsProcess(true);
        xSpeed = MOVE_SPEED * (float)GetPhysicsProcessDeltaTime();
    }

    public override void DisableObject()
    {
        SetPhysicsProcess(false);
    }

    public override void Damage()
    {
        QueueFree();
    }
}
