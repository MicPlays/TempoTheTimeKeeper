using Godot;
using System;

[Tool]
public partial class WorldGrid : Node2D
{
    private Vector2 topLeftCorner;
    [Export]
    public int xChunkSize;
    [Export]
    public int yChunkSize;
    private Chunk[,] chunks;

    public override void _Ready()
    {
        this.topLeftCorner = this.Position;
        this.chunks = new Chunk[xChunkSize, yChunkSize];
    }
}
