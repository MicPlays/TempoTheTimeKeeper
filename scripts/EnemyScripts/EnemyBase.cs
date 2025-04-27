using Godot;
using System;

public abstract partial class EnemyBase : GameObject
{
    [Export]
    public NodePath screenNotifierPath;
    public VisibleOnScreenNotifier2D screenNotifier;
    [Export]
    public int mapLayer;
    [Export]
    public float health;
    public Vector2 spawnCoords;

    public abstract void EnableObject();

    public abstract void DisableObject();
}
