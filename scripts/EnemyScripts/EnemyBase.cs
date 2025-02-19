using Godot;
using System;

public abstract partial class EnemyBase : GameObject, IAttackable
{
    [Export]
    public NodePath screenNotifierPath;
    public VisibleOnScreenNotifier2D screenNotifier;
    [Export]
    public int mapLayer;
    public Vector2 spawnCoords;

    public abstract void EnableObject();

    public abstract void DisableObject();
    public abstract void Damage();
}
