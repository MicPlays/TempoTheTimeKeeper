using Godot;
using System;

public partial class AttackHitbox : Hitbox
{
    public float damage;
    public override void _Ready()
    {
        parentObject = GetNode<GameObject>(parentObjectPath);
    }

    public void SetDamage(float amount)
    {
        damage = amount;
    }
}

