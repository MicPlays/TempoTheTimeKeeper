using Godot;
using System;

public abstract partial class SolidObject : Node2D
{
    public float xSpeed = 0;
    public float ySpeed = 0;
    public float groundSpeed = 0;
    public float groundAngle = 0;
    public int widthRadius = 5;
    public int heightRadius = 5;

}
