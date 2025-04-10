using Godot;
using System;
using System.Collections.Generic;

public partial class MusicNoteFloat : GameObject, IRoutineGameObject
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
        hitbox.SetDeferred("monitoring", false);
        Player playerRef = LevelManager.Instance.GetLevel().player;
        playerRef.noteCount++;
        LevelManager.Instance.GetLevel().hud.SetNoteCount(playerRef.noteCount);
        LevelManager.Instance.GetLevel().AddScore(10);
        sprite.AnimationFinished += OnBurst;
        sprite.Play("burst");
    }

    public void OnBurst()
    {
        sprite.AnimationFinished -= OnBurst;
        QueueFree();
    }

}
