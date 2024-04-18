using Godot;
using System;

public partial class MusicNoteFloat : RoutineGameObject
{
    [Export]
    public NodePath spritePath;
    private AnimatedSprite2D sprite;

    public override void _Ready()
    {
        hitbox = GetNode<Area2D>(hitboxPath);

        sprite = GetNode<AnimatedSprite2D>(spritePath);
        sprite.Play("bounce");
    }

    public override void IncrementRoutine()
    {
        hitbox.SetDeferred("monitorable", false);
        Player playerRef = GameController.Instance.GetPlayer();
        playerRef.noteCount++;
        HUD.Instance.SetNoteCount(playerRef.noteCount);
        GameController.Instance.AddScore(10);
        sprite.AnimationFinished += OnBurst;
        sprite.Play("burst");
        
    }

    public void OnBurst()
    {
        QueueFree();
    }
}
