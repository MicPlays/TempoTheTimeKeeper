using Godot;
using System;

public partial class Drumsticks : GameObject, IRoutineGameObject
{
    [Export]
    public NodePath spritePath;
    private AnimatedSprite2D sprite;
    [Export]
    public NodePath screenNotifierPath;
    public VisibleOnScreenNotifier2D screenNotifer;

    public override void _Ready()
    {
        hitbox = GetNode<Area2D>(hitboxPath);
        hitbox.AreaEntered += OnPlayerEnter;
        screenNotifer = GetNode<VisibleOnScreenNotifier2D>(screenNotifierPath);
        sprite = GetNode<AnimatedSprite2D>(spritePath);
    }

    public void OnPlayerEnter(Area2D playerHitbox)
    {
        if (playerHitbox.CollisionLayer == 1)
            Routine();
    }

    public void Routine()
    {
        Player playerRef = LevelManager.Instance.GetLevel().player;
        bool healed = playerRef.Heal();
        if (healed)
        {
            hitbox.SetDeferred("monitoring", false);
            QueueFree();
        }
        
    }
}
