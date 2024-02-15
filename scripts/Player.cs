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
    private const float JUMP_FORCE = 6.5f;
    private const float GRAVITY_FORCE = 0.21875f;
    private const float AIR_ACC_SPEED = 0.09375f;


    //state variables
    private bool isGrounded = true;
    private bool isJumping = false;
    
    //sensor stuff
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
        //ground state
        if (isGrounded)
        {        
            //jump check
            if (Input.IsActionPressed("jump"))
            {
                xSpeed -= JUMP_FORCE * Mathf.Sin(Mathf.DegToRad(groundAngle));
                ySpeed -= JUMP_FORCE * Mathf.Cos(Mathf.DegToRad(groundAngle));
                GD.Print(ySpeed);
                isGrounded = false;
                isJumping = true;
            }
            //if jumping, immediately go airborne, do not execute grounded 
            //code as player will immediately snap back to ground
            if (isGrounded)
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
                    groundSpeed -= Mathf.Min(Mathf.Abs(groundSpeed), FRICTION_SPEED) * Mathf.Sign(groundSpeed);
            
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

                //ground collision process
                bool groundCollision = true;
                float[] groundData = GroundSensorCompetition();

                //if player is too far off the ground, they won't collide. this formula
                //takes player speed into account. the faster they move, the further they can
                //be off the ground and still collide
                if (groundData[0] > Mathf.Min(Mathf.Abs(xSpeed) + 4, 14))
                    groundCollision = false;

                //eventually, collision calculation will be based off of the current mode.
                //for now, just assume floor mode
                if (groundCollision)
                {
                    Position = new Vector2(GlobalPosition.X, GlobalPosition.Y + groundData[0]); 
                    groundAngle = groundData[1];
                }
                else isGrounded = false;

                //if on flat ground, move sensors down to account for low steps
                if (groundAngle == 0)
                {
                    sensorTable["E"].Position = new Vector2(-pushRadius, 4);
                    sensorTable["F"].Position = new Vector2(pushRadius, 4);
                }
            }
            
        }
        //air state
        else 
        {
            //jump release check for variable jump height
            if (isJumping)
            {
                if (Input.IsActionJustReleased("jump"))
                {
                    if (ySpeed < -4)
                        ySpeed = -4;
                    isJumping = false;
                }
            }
            //input movement
            if (Input.IsActionPressed("left"))
                xSpeed -= AIR_ACC_SPEED;
            else if (Input.IsActionPressed("right"))
                xSpeed += AIR_ACC_SPEED;
            
            //air drag
            if (ySpeed < 0 && ySpeed > -4)
                xSpeed -= (xSpeed/0.125f)/256;

            //Move player pos
            Position = new Vector2(GlobalPosition.X + xSpeed, GlobalPosition.Y + ySpeed);

            //apply gravity
            ySpeed += GRAVITY_FORCE;
            if (ySpeed > 16) ySpeed = 16;

            //rotate player angle back to 0
            if (groundAngle < 180f)
                groundAngle -= 2.8125f;
            else 
                groundAngle += 2.8125f;
            if (groundAngle >= 360f || groundAngle < 0)
                groundAngle = 0; 
            
            //get angle of air movement
            Vector2 airAngleVector = new Vector2(xSpeed, ySpeed).Normalized();
            float airAngle = Mathf.RadToDeg(Mathf.Atan2(-airAngleVector.Y, airAngleVector.X));
            if (airAngle < 0)
                airAngle += 360f;
            if (airAngle > 360f)
                airAngle -= 360f;

            sensorTable["E"].Position = new Vector2(-10, 0);
            sensorTable["F"].Position = new Vector2(10, 0);

            if ((airAngle >= 0f && airAngle <= 45f) || (airAngle >= 316 && airAngle <= 360))
            {
                //mostly right, right, ceiling, and ground sensors active
                AirPushCollisionProcess("F");
                AirGroundCollisionProcess(false);
            }
            else if (airAngle >= 46f && airAngle <= 135f)
            {
                //mostly up, ceiling and push sensors active
                AirPushCollisionProcess("E");
                AirPushCollisionProcess("F");
                
            }
            else if (airAngle >= 136f && airAngle <= 225f)
            {
                //mostly left, left, ceiling and ground
                AirPushCollisionProcess("E");
                AirGroundCollisionProcess(false);
            }
            else 
            {
                //mostly down, ground and push active
                AirPushCollisionProcess("E");
                AirPushCollisionProcess("F");
                AirGroundCollisionProcess(true);
            }

        }
    }

    //push sensor collision process when airborne
    public void AirPushCollisionProcess(string sensorString)
    {
        float[] pushData = sensorTable[sensorString].CheckForTile();
        if (pushData[0] < 0)
        {
            if (xSpeed > 0)
                Position = new Vector2(Position.X + pushData[0], Position.Y);
            else if (xSpeed < 0)
                Position = new Vector2(Position.X - pushData[0], Position.Y);
            xSpeed = 0;
        }
    }

    //push sensor collision process when grounded. returns distance from tile to move the player with
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
            activeSensor.Position = new Vector2(activeSensor.Position.X + groundSpeed, activeSensor.Position.Y);
            float[] data = activeSensor.CheckForTile();
            //reset sensor pos as child objects move along with their parents in Godot
            activeSensor.Position = new Vector2(Mathf.Sign(groundSpeed) * pushRadius, 0);
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

    //ground collision processing when in the air
    public void AirGroundCollisionProcess(bool goingDown)
    {
        bool groundCollision = true;
        float[] groundData = GroundSensorCompetition();
        if (groundData[0] >= 0)
            groundCollision = false;
        if (groundCollision) 
        {
            if (goingDown)
            {
                float[] groundAData = sensorTable["A"].CheckForTile();
                float[] groundBData = sensorTable["B"].CheckForTile();
                if (!(groundAData[0] >= -(ySpeed + 8) || groundBData[0] >= -(ySpeed + 8)))
                    groundCollision = false;
            }
            else
            {
                if (!(ySpeed >= 0))
                    groundCollision = false;
            }

            if (groundCollision)
            {
                Position = new Vector2(GlobalPosition.X, GlobalPosition.Y + groundData[0]); 
                groundAngle = groundData[1];
                //groundSpeed will likely need to be a combination of x and y direction going forward
                groundSpeed = xSpeed;
                ySpeed = 0;
                isGrounded = true;
                isJumping = false;
            }
        }
    }

    //get shortest distance between the two ground sensors. 
    //returns a float array, index 0 being distance, index 1 being the tile angle value
    public float[] GroundSensorCompetition()
    {
        float groundDistance, newGroundAngle;
        float[] groundAData = sensorTable["A"].CheckForTile();
        float[] groundBData = sensorTable["B"].CheckForTile();

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
        return new float[] {groundDistance, newGroundAngle};
    }
}
