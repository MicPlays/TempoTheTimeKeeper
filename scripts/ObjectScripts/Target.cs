using Godot;
using System;

public partial class Target : GameObject, IAttackable
{
    [Export]
    public float health;
    public override void _Ready()
    {
        hitbox = GetNode<Area2D>(hitboxPath);
    }

    public void Damage(float damage)
    {
        GD.Print(damage);
        health -= damage;
        if (health <= 0)
            QueueFree();
    }
}
