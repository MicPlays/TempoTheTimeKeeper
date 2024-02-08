using Godot;
using System;
using System.Collections.Generic;

public partial class Player : SolidObject
{
    //constants
    private const float ACC_SPEED = 0.046875f;
    private const float DEC_SPEED = 0.5f;
    private const float FRICTION_SPEED = 0.046875f;
    private const int TOP_SPEED = 6;

    private String state;
    public int pushRadius = 10;
    Dictionary<string, Sensor> sensorTable;
    public override void _Ready()
    {
        //set player object properties (might make export vars later)
        xSpeed = 0f;
        ySpeed = 0f;
        groundAngle = 0f;
        groundSpeed = 0f;
        widthRadius = 9;
        heightRadius = 19;

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
        sensorTable["A"].Position = new Vector2(-widthRadius, heightRadius/2);
        sensorTable["B"].Position = new Vector2(widthRadius, heightRadius/2);
        sensorTable["C"].Position = new Vector2(-widthRadius, -heightRadius/2);
        sensorTable["D"].Position = new Vector2(widthRadius, -heightRadius/2);
        sensorTable["E"].Position = new Vector2(-pushRadius, 0);
        sensorTable["F"].Position = new Vector2(pushRadius, 0);

    }

    public override void _Process(double delta)
    {
        //ground running input
        if (Input.IsActionPressed("left"))
        {
            if (groundSpeed > 0)
                groundSpeed -= DEC_SPEED;
                
            else if (groundSpeed > -TOP_SPEED)
            {
                groundSpeed -= ACC_SPEED;
                if (groundSpeed <= -TOP_SPEED)
                    groundSpeed = -TOP_SPEED;
            }
            
        }
        else if (Input.IsActionPressed("right"))
        {
            if (groundSpeed < 0)
                groundSpeed += DEC_SPEED;
                
            else if (groundSpeed < TOP_SPEED)
            {
                groundSpeed += ACC_SPEED;
                if (groundSpeed >= TOP_SPEED)
                    groundSpeed = TOP_SPEED;
            }
        }
        else 
        {
            groundSpeed -= Mathf.Min(Mathf.Abs(groundSpeed), FRICTION_SPEED) * Mathf.Sign(groundSpeed);
        }

        float wallDistance = PushCollisionProcess();

        //calculate X and Y speed from Ground Speed and Angle
        xSpeed = (groundSpeed * Mathf.Cos(Mathf.DegToRad(groundAngle))) + wallDistance;
        ySpeed = groundSpeed * -Mathf.Sin(Mathf.DegToRad(groundAngle));

        if (wallDistance != 0)
        {
            groundSpeed = 0;
        }

        //Move player pos
        Position = new Vector2(GlobalPosition.X + xSpeed, GlobalPosition.Y + ySpeed);

        GroundCollisionProcess();

        //if on flat ground, move sensors down to account for low steps
        if (groundAngle == 0)
        {
            sensorTable["E"].Position = new Vector2(-pushRadius, 4);
            sensorTable["F"].Position = new Vector2(pushRadius, 4);
        }
    }

    public float PushCollisionProcess()
    {
        //if player isn't moving, don't check sensors
        if (groundSpeed != 0)
        {
            Sensor activeSensor;
            //get active sensor
            if (groundSpeed > 0)
                activeSensor = sensorTable["F"];
            else 
                activeSensor = sensorTable["E"];
            //player will not have moved yet, move push sensors to account for this
            activeSensor.Position = new Vector2(activeSensor.Position.X + groundSpeed, activeSensor.Position.Y + groundSpeed);
            float[] data = activeSensor.CheckForTile();
            activeSensor.Position = new Vector2(activeSensor.Position.X - xSpeed, activeSensor.Position.Y - ySpeed);
            if (data[0] < 0)
            {
                if (activeSensor.direction == "left")
                    return -data[0];
                else 
                    return data[0];
            }
            else return 0;
        }
        else return 0;
    }

    public void GroundCollisionProcess()
    {
        bool groundCollision = true;
        float groundDistance, newGroundAngle;
        float[] groundAData = sensorTable["A"].CheckForTile();
        float[] groundBData = sensorTable["B"].CheckForTile();

        //use lowest distance between ground sensors
        if (groundAData[0] == groundBData[0])
        {
            groundDistance = groundAData[0];
            newGroundAngle = groundAData[1];
        }
            
        else if (groundAData[0] < groundBData[0])
        {
            groundDistance = groundAData[0];
            newGroundAngle = groundAData[1];
        }
        else 
        {
            groundDistance = groundBData[0];
            newGroundAngle = groundBData[1];
        }        

        //if player is too far off the ground, they won't collide. this formula
        //takes player speed into account. the faster they move, the further they can
        //be off the ground and still collide
        if (groundDistance > Mathf.Min(Mathf.Abs(xSpeed) + 4, 14))
        {
            groundCollision = false;
        }

        //eventually, collision calculation will be based off of the current mode.
        //for now, just assume floor mode
        if (groundCollision)
        {
            Position = new Vector2(GlobalPosition.X, GlobalPosition.Y + groundDistance); 
            groundAngle = newGroundAngle;
        }
    }

    public override void _Input(InputEvent @event)
    {
        
    }
}
