using Godot;
using System;

public partial class PickrenProjectile : GameObject
{
    [Export]
    public NodePath screenNotifierPath;
    public VisibleOnScreenNotifier2D screenNotifier;
    [Export]
    public NodePath spritePath;
    public AnimatedSprite2D sprite;
    [Export]
    public float projectileSpeed = 20f;
    public int direction = -1;
    public override void _Ready()
    {
        screenNotifier = GetNode<VisibleOnScreenNotifier2D>(screenNotifierPath);
        screenNotifier.ScreenEntered += EnableObject;
        screenNotifier.ScreenExited += DisableObject;
        sprite = GetNode<AnimatedSprite2D>(spritePath);
        hitbox = GetNode<Hitbox>(hitboxPath);
        hitbox.AreaEntered += OnAreaEnter;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = new Vector2(GlobalPosition.X + xSpeed * (float)delta, GlobalPosition.Y);
    }


    public void EnableObject()
    {
        SetPhysicsProcess(true);
        xSpeed = projectileSpeed * direction;
        sprite.Play("move");
    }

    public void DisableObject()
    {
        QueueFree();
    }

    public void OnAreaEnter(Area2D area)
    {
         if (area is Hitbox)
        {
            Hitbox attackable = (Hitbox)area;
            if (attackable.parentObject is IAttackable)
            {
                IAttackable otherObject = (IAttackable)attackable.parentObject;
                otherObject.Damage(1);
            }
        }
    }
}
