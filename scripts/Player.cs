using Godot;
using System;

public partial class Player : Node2D
{
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsKeyPressed(Key.W))
        {
            Position = new Vector2(Position.X, Position.Y - 1);
        }
        else if (Input.IsKeyPressed(Key.S))
        {
            Position = new Vector2(Position.X, Position.Y + 1);
        }
        else if (Input.IsKeyPressed(Key.A))
        {
            Position = new Vector2(Position.X - 1, Position.Y);
        }
        else if (Input.IsKeyPressed(Key.D))
        {
            Position = new Vector2(Position.X + 1, Position.Y);
        }
    }
}
