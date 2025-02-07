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
            IncrementRoutine();
    }

    public void IncrementRoutine()
    {
        hitbox.SetDeferred("monitoring", false);
        Player playerRef = GameController.Instance.GetPlayer();
        playerRef.noteCount++;
        HUD.Instance.SetNoteCount(playerRef.noteCount);
        GameController.Instance.AddScore(10);
        sprite.AnimationFinished += OnBurst;
        sprite.Play("burst");
    }

    public void OnBurst()
    {
        sprite.AnimationFinished -= OnBurst;
        QueueFree();
    }

}
