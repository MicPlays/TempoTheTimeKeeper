using Godot;
using System;
using System.Transactions;

public partial class PlayerCam : CameraHolder
{
    public Node2D target;
    public bool cameraLocked = false;

    public override Camera2D GetCamera()
    {
        if (IsInsideTree())
            return (Camera2D)GetNodeOrNull("Camera2D");
        else return null;
    }

    public override void _Process(double delta)
    {
        if (!cameraLocked)
            GlobalPosition = target.GlobalPosition;       
    }
}
