using Godot;
using System;

public abstract partial class GameObject : Node2D
{
    public float xSpeed = 0;
    public float ySpeed = 0;
    public float groundSpeed = 0;
    public float groundAngle = 0;
    public int widthRadius = 5;
    public int heightRadius = 5;

    [Export]
    public ObjectType hitboxReaction;

    [Export]
    public NodePath hitboxPath;
    public Area2D hitbox;
    public CollisionShape2D hitboxRect;
}


public enum ObjectType
{
    Attackable,
    RoutineIncrement,
    Hurt,
    Special,
    Player
}