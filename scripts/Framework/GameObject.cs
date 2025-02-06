using Godot;
using System;
using System.Collections.Generic;

public abstract partial class GameObject : Node2D
{
    public float xSpeed = 0;
    public float ySpeed = 0;
    public float groundSpeed = 0;
    public float groundAngle = 0;
    public int widthRadius = 5;
    public int heightRadius = 5;
    [Export]
    public NodePath hitboxPath;
    public Area2D hitbox;
}