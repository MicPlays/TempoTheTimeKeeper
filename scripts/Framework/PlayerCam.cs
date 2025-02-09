using Godot;
using System;

public partial class PlayerCam : Node2D
{
    [Export]
    public NodePath targetPath;
    public Node2D target;
    public bool cameraLocked;

    public override void _Ready()
    {
        
        target = GetNode<Node2D>(targetPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!cameraLocked)
            Position = target.GlobalPosition;
    }
}
