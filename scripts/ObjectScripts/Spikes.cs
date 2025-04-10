using Godot;
using System;

public partial class Spikes : SolidObject
{
    public enum Facing
    {
        Left,
        Right,
        Up,
        Down
    }
    [Export]
    public NodePath spritePath;
    public Sprite2D sprite;
    [Export]
    public Facing facing {get; set;}
    public override void _Ready()
    {
        base._Ready();
        sprite = GetNode<Sprite2D>(spritePath);
        hitbox = GetNode<Area2D>(hitboxPath);
        hitbox.AreaEntered += OnAreaEnter;
        switch (facing)
        {
            case Facing.Left:
                sprite.RotationDegrees = 270;
                hitbox.Position += new Vector2(-16, 0);
                hitbox.RotationDegrees = 270;
                break;
            case Facing.Right:
                sprite.RotationDegrees = 90;
                hitbox.Position += new Vector2(-16, 0);
                hitbox.RotationDegrees = 90;
                break;
            case Facing.Up:
                sprite.RotationDegrees = 0;
                hitbox.Position += new Vector2(0, -16);
                break;
            case Facing.Down:
                sprite.RotationDegrees = 180;
                hitbox.Position += new Vector2(0, 16);
                break;   
        }
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
                        if (attackable.parentObject.xSpeed <= 0)
                            otherObject.Damage(9999);
                        break;
                    case Facing.Right:
                        if (attackable.parentObject.xSpeed >= 0)
                            otherObject.Damage(9999);
                        break;
                    case Facing.Up:
                        if (attackable.parentObject.ySpeed >= 0 && (GlobalPosition.Y - attackable.parentObject.GlobalPosition.Y) > 0)
                            otherObject.Damage(9999);
                        break;
                    case Facing.Down:
                        if (attackable.parentObject.ySpeed < 0)
                            otherObject.Damage(9999);
                        break;
                }
            }
                
        }
    }
}
