using Godot;
using System;

public partial class Sensor : Node2D
{
    [Export]
    public string direction;
    private TileMap tileMap;

    public override void _Ready()
    {
        
    }

    //will return an array later
    public int checkTile() {
        int index = 0;
        return index;
    }
}
