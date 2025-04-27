using Godot;
using System;

public partial class BelubaPhysics : SimpleEnemyPhysics
{
    [Export]
    public float FALL_SPEED {get; set;} = 0f;
    [Export]
    public float RECOVER_SPEED {get; set;} = 0f;

    public override void ApplyKnockback(float knockbackForce, Vector2 knockbackDirection, float deltaTime)
    {
        float delta = (float)GetPhysicsProcessDeltaTime();
        enemy.ySpeed += ((knockbackForce * delta) / (MASS * delta)) * knockbackDirection.Y;
    }

    public void Fall(float deltaTime)
    {
        enemy.ySpeed += FALL_SPEED * deltaTime;
    }

    public void Rise(float deltaTime)
    {
        enemy.ySpeed -= FALL_SPEED * deltaTime;
    }

}
