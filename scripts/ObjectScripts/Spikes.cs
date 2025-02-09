using Godot;
using System;

public partial class Spikes : GameObject
{
    public enum Facing
    {
        Left,
        Right,
        Up,
        Down
    }
    [Export]
    public Facing facing {get; set;}
    public override void _Ready()
    {
        hitbox = GetNode<Area2D>(hitboxPath);
        hitbox.AreaEntered += OnAreaEnter;
    }

    public void OnAreaEnter(Area2D area)
    {
        if (area is Hitbox)
        {
            Hitbox attackable = (Hitbox)area;
            if (attackable.parentObject is IAttackable)
            {
                IAttackable otherObject = (IAttackable)attackable.parentObject;
                switch (facing)
                {
                    case Facing.Left:
                        if (attackable.parentObject.xSpeed < 0)
                            otherObject.Damage();
                        break;
                    case Facing.Right:
                        if (attackable.parentObject.xSpeed > 0)
                            otherObject.Damage();
                        break;
                    case Facing.Up:
                        if (attackable.parentObject.ySpeed >= 0 && (GlobalPosition.Y - attackable.parentObject.GlobalPosition.Y) > 0)
                            otherObject.Damage();
                        break;
                    case Facing.Down:
                        if (attackable.parentObject.ySpeed < 0)
                            otherObject.Damage();
                        break;
                }
            }
                
        }
    }
}
