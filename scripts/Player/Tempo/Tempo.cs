using Godot;
using System;
using System.Collections.Generic;

public partial class Tempo : Player
{
    public float speedBoostInputTimer = 0;
    public float xSpeedBuffer = 0;
    [Export]
    public NodePath attackBoxPath {get; set;}
    public Area2D attackBox;

    public override void _Ready()
    {
        base._Ready();
        attackBox = GetNode<Area2D>(attackBoxPath);
        attackBox.AreaEntered += AttackBoxCollision;
        TempoCollisionComponent tcc = (TempoCollisionComponent)cc;
        tcc.ToggleAttackHitbox(false);
    }

    public void AttackBoxCollision(Area2D area)
    {
        Hitbox attackable = (Hitbox)area;
        if (attackable != null)
        {
            if (attackable.parentObject is IAttackable)
                ((IAttackable)attackable.parentObject).Damage();
        }
    }
}
