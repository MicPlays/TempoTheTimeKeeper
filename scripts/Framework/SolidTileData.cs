using Godot;
using System;

public partial class SolidTileData
{
    public float distance;
    public float angle;
    public bool flagged;

    public SolidTileData(float distance, float angle, bool flagged)
    {
        this.distance = distance;
        this.angle = angle;
        this.flagged = flagged;
    }
}
