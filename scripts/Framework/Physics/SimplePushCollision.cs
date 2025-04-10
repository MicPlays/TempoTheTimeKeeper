using Godot;
using System;
using System.Collections.Generic;

public partial class SimplePushCollision : Node
{
    [Export]
    public NodePath lSensor;
    [Export]
    public NodePath rSensor;
    public Dictionary<string, Sensor> sensorTable;
    public EnemyBase collider;

    public void Init()
    {
        sensorTable = new Dictionary<string, Sensor>
        {
            { "A", GetNode<Sensor>(lSensor) },
            { "B", GetNode<Sensor>(rSensor) }
        };

        sensorTable["A"].Position = new Vector2(-collider.widthRadius, 0);
        sensorTable["B"].Position = new Vector2(collider.widthRadius, 0);
    }

    public bool PushCollisionProcess()
    {
        Sensor activeSensor;
        //get active sensor
        if (collider.xSpeed > 0)
            activeSensor = sensorTable["B"];
        else 
            activeSensor = sensorTable["A"];
        
        SolidTileData pushData = activeSensor.CheckForTile(collider.mapLayer);
        if (pushData.distance <= 0)
        {
            //reset player position (push against wall)
            if (collider.xSpeed > 0)
                collider.Position = new Vector2(collider.Position.X + pushData.distance, collider.Position.Y);
            else if (collider.xSpeed < 0)
                collider.Position = new Vector2(collider.Position.X - pushData.distance, collider.Position.Y);
            return true;
        }
        return false;
    }
}
