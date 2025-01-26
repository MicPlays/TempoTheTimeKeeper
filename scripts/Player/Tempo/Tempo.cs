using Godot;
using System;

public partial class Tempo : Player
{
    public float speedBoostInputTimer = 0;
    public float xSpeedBuffer = 0;

    public override void _Ready()
    {
        base._Ready();
    }
}
