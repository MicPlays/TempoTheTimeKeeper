using Godot;
using System;
using System.Collections.Generic;

public partial class SimpleGroundCollision : Node
{
    [Export]
    public NodePath sensorPath;
    public Sensor groundSensor;
    public EnemyBase collider;

    public virtual void Init()
    {
        groundSensor = GetNode<Sensor>(sensorPath);
        groundSensor.Position = new Vector2(0, collider.heightRadius);
    }

    public virtual bool GroundCollisionProcess()
    {
        bool groundCollision = true;
        SolidTileData groundData = groundSensor.CheckForTile(collider.mapLayer);
        if (groundData.distance > 6)
            groundCollision = false;

        if (groundCollision)
        {
            collider.GlobalPosition = new Vector2(collider.GlobalPosition.X, collider.GlobalPosition.Y + groundData.distance);
            return true;
        }
        else return false; //handle this seperately
    }

    public virtual void SnapToGround()
    {
        SolidTileData groundData = groundSensor.CheckForTile(collider.mapLayer);
        collider.GlobalPosition = new Vector2(collider.GlobalPosition.X, collider.GlobalPosition.Y - groundData.distance);
    }
}
