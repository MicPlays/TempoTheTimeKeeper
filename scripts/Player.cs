using Godot;
using System;
using System.Collections.Generic;

public partial class Player : Node2D
{
    private String state;
    private SolidObject playerObject;
    public int pushRadius = 10;
    Dictionary<string, Sensor> sensorTable;
    public override void _Ready()
    {
        //set player object properties (might make export vars later)
        playerObject = (SolidObject)this.FindChild("Object");
        playerObject.xSpeed = 0f;
        playerObject.ySpeed = 0f;
        playerObject.groundAngle = 0f;
        playerObject.groundSpeed = 0f;
        playerObject.widthRadius = 9;
        playerObject.heightRadius = 19;

        //get player's sensors
        var sensors = this.FindChild("Sensors").GetChildren();
        sensorTable = new Dictionary<string, Sensor>
        {
            { "A", (Sensor)sensors[0] },
            { "B", (Sensor)sensors[1] },
            { "C", (Sensor)sensors[2] },
            { "D", (Sensor)sensors[3] },
            { "E", (Sensor)sensors[4] },
            { "F", (Sensor)sensors[5] }
        };

        //move sensors to relative locations
        sensorTable["A"].Position = new Vector2(-playerObject.widthRadius, playerObject.heightRadius/2);
        sensorTable["B"].Position = new Vector2(playerObject.widthRadius, playerObject.heightRadius/2);
        sensorTable["C"].Position = new Vector2(-playerObject.widthRadius, -playerObject.heightRadius/2);
        sensorTable["D"].Position = new Vector2(playerObject.widthRadius, -playerObject.heightRadius/2);
        sensorTable["E"].Position = new Vector2(-pushRadius, 0);
        sensorTable["F"].Position = new Vector2(pushRadius, 0);

    }

    public override void _PhysicsProcess(double delta)
    {
        bool groundCollision = true;
        int groundDistance, groundAngle;
        int[] groundAData = sensorTable["A"].CheckForTile();
        int[] groundBData = sensorTable["B"].CheckForTile();

        //use lowest distance between ground sensors
        if (groundAData[0] == groundBData[0])
        {
            groundDistance = groundAData[0];
            groundAngle = groundAData[0];
        }
            
        else if (groundAData[0] < groundBData[0])
        {
            groundDistance = groundAData[0];
            groundAngle = groundAData[0];
        }
        else 
        {
            groundDistance = groundBData[0];
            groundAngle = groundBData[0];
        }

        //if player is too far off the ground, they won't collide. this formula
        //takes player speed into account. the faster they move, the further they can
        //be off the ground and still collide
        if (groundDistance > Mathf.Min(Mathf.Abs(playerObject.xSpeed) + 4, 14))
            //GD.Print(groundDistance);
            groundCollision = false;

        //eventually, collision calculation will be based off of the current mode.
        //for now, just assume floor mode
        if (groundCollision)
        {
            Position = new Vector2(GlobalPosition.X, GlobalPosition.Y + groundDistance); 
            playerObject.groundAngle = groundAngle;
        }

        //shitty input (will change later)
        if (Input.IsKeyPressed(Key.A))
        {
            Position = new Vector2(Position.X - 1, Position.Y);
        }
        else if (Input.IsKeyPressed(Key.D))
        {
            Position = new Vector2(Position.X + 1, Position.Y);
        }
    }
}
