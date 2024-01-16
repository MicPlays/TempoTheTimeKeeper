using Godot;
using System;

public partial class Sensor : Node2D
{
    [Export]
    private Vector2 anchorPoint;
    [Export]
    public int direction;

    //will return an array later
    public int checkTile() {
        int index = 0;
        return index;
    }
}
