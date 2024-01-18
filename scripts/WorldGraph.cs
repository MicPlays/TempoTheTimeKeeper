using Godot;
using System;

public partial class WorldGraph : Node2D
{
    private Vector2 topLeftCorner;
    [Export]
    public int xChunkSize;
    [Export]
    public int yChunkSize;
    private Node2D[,] chunks;

    public override void _Ready()
    {
        this.topLeftCorner = this.Position;
        this.chunks = new Node2D[xChunkSize, yChunkSize];
    }
}
