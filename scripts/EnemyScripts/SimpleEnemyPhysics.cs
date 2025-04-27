using Godot;
using System;

public partial class SimpleEnemyPhysics : Node
{
    public GameObject enemy;
    [Export]
    public float GRAVITY_FORCE {get; set;} = 13.125f;
    [Export]
    public float FRICTION_SPEED {get; set;} = 2.8125f;
    [Export]
    public float MASS {get; set;} = 5.0f;
    [Export]
    public float MOVE_SPEED {get; set;} = 0f;

    public virtual void ApplyGravity(float delta)
    {
        enemy.ySpeed += GRAVITY_FORCE * delta;
    }

    public virtual void MoveEnemyObject()
    {
        enemy.GlobalPosition = new Vector2(enemy.GlobalPosition.X + enemy.xSpeed, enemy.GlobalPosition.Y + enemy.ySpeed);
    }

    public virtual void ApplyKnockback(float knockbackForce, Vector2 knockbackDirection, float deltaTime)
    {
        float delta = (float)GetPhysicsProcessDeltaTime();
        enemy.xSpeed += ((knockbackForce  * delta) / (MASS * delta)) * knockbackDirection.X;
        enemy.ySpeed += ((knockbackForce * delta) / (MASS * delta)) * knockbackDirection.Y;
    }

    public void ApplyFriction(float deltaTime)
    {
        enemy.xSpeed -= Mathf.Min(Mathf.Abs(enemy.xSpeed), FRICTION_SPEED * deltaTime) * Mathf.Sign(enemy.xSpeed);
    }
}
