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
    public Chunk[,] chunks;
    public float xGridSizePixels;
    public float yGridSizePixels;

    public override void _Ready()
    {
        xGridSizePixels = 128f * xChunkSize;
        yGridSizePixels = 128f * yChunkSize;
        GD.Print(new Vector2(xGridSizePixels, yGridSizePixels));
        this.topLeftCorner = this.Position;
        this.chunks = new Chunk[xChunkSize, yChunkSize];

    }

    public override void _Draw()
    {
        //Draw Chunk Grid for debug
        DrawLine(new Vector2(0f, 0f), new Vector2(xChunkSize * 128f, 0f), Colors.Green, 2.0f);
        DrawLine(new Vector2(0f, 0f), new Vector2(0f, yChunkSize * 128f), Colors.Green, 2.0f);
        for (int i = 1; i <= xChunkSize; i++)
        {
            DrawLine(new Vector2(i * 128f, 0f), new Vector2(i * 128f, yChunkSize * 128f), Colors.Green, 2.0f);
        }

        for (int i = 1; i <= yChunkSize; i++)
        {
            DrawLine(new Vector2(0f, i * 128f), new Vector2(xChunkSize * 128f, i * 128f), Colors.Green, 2.0f);
        }
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            QueueRedraw();
        }
         
    }
}
