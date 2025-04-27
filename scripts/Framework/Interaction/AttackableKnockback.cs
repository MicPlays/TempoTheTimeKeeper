using Godot;
using System;

public interface IAttackableKnockback
{
    public void Damage(float amount, float knockback, Vector2 knockbackDirection);
}
