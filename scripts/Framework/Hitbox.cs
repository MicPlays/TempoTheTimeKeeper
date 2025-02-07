using Godot;
using System;

public partial class Hitbox : Area2D
{
    [Export]
    public NodePath parentObjectPath;
    public GameObject parentObject;

    public override void _Ready()
    {
        parentObject = GetNode<GameObject>(parentObjectPath);
    }
}
