using Godot;
using System;

public partial class Target : GameObject, IAttackable
{
    public override void _Ready()
    {
        hitbox = GetNode<Area2D>(hitboxPath);
    }

    public void Damage()
    {
        GD.Print("hit");
    }
}
