using Godot;
using System;
using System.Collections.Generic;

public partial class Player : GameObject
{
    //constants
    private const float ACC_SPEED = 0.046875f;
    private const float DEC_SPEED = 0.5f;
    private const float FRICTION_SPEED = 0.046875f;
    private const int TOP_SPEED = 6;
    private const float JUMP_FORCE = 6.5f;
    private const float GRAVITY_FORCE = 0.21875f;
    private const float AIR_ACC_SPEED = 0.09375f;
    private const float SLOPE_FACTOR = 0.125f;
    private const float SLOPE_SPEED_FACTOR =  0.05078125f;

    //Components and their NodePaths
    [Export]
    public NodePath sensorContainerPath;
    [Export]
    public NodePath spritePath;

    private AnimatedSprite2D playerSprite;
    //player's current collision layer
    public int currentLayer;

    //state variables
    public bool isGrounded = true;
    private bool isJumping = false;
    private int controlLockTimer = 0;
    
    //sensor stuff
    public int pushRadius = 10;
    Dictionary<string, Sensor> sensorTable;

    //variable running speed vars
    private int currentFrame = 0;

    //player stats
    public int noteCount = 0;

    public override void _Ready()
    {
        //set player object properties (might make export vars later)
        xSpeed = 0f;
        ySpeed = 0f;
        groundAngle = 0f;
        groundSpeed = 0f;
        widthRadius = 9;
        heightRadius = 20;

        //get player components
        playerSprite = GetNode<AnimatedSprite2D>(spritePath);
        hitbox = GetNode<Area2D>(hitboxPath);
        hitboxRect = hitbox.GetChild<CollisionShape2D>(0);

        hitbox.AreaEntered += OnHitboxEnter;

        currentLayer = 0;

        //get player's sensors
        var sensors = GetNode(sensorContainerPath).GetChildren();
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
        sensorTable["A"].Position = new Vector2(-widthRadius, heightRadius);
        sensorTable["B"].Position = new Vector2(widthRadius, heightRadius);
        sensorTable["C"].Position = new Vector2(-widthRadius, -heightRadius);
        sensorTable["D"].Position = new Vector2(widthRadius, -heightRadius);
        sensorTable["E"].Position = new Vector2(-pushRadius, 0);
        sensorTable["F"].Position = new Vector2(pushRadius, 0);

    }

    public override void _Process(double delta)
    {
        //ground state
        if (isGrounded)
        {   
            //placeholder animation code
            if (groundSpeed != 0)
            {
                if (currentFrame == 0)
                    playerSprite.Play("jog");
                if (currentFrame != playerSprite.Frame)
                {
                    playerSprite.SpeedScale = Mathf.Floor(Mathf.Max(1, Mathf.Abs(groundSpeed)));
                    currentFrame = playerSprite.Frame;
                }
                if (groundSpeed < 0)
                    playerSprite.FlipH = true;
                else playerSprite.FlipH = false;
            }
            else 
            {
                playerSprite.Play("windyidle");
                playerSprite.SpeedScale = 1.0f;
                currentFrame = 0;
            }

            //adjust ground speed according to slope factor 
            if (sensorTable["A"].direction != "up" && groundSpeed != 0)
                groundSpeed -= SLOPE_FACTOR * Mathf.Sin(Mathf.DegToRad(groundAngle)); 
                
            
            //jump check
            if (Input.IsActionPressed("jump"))
            {
                //activate ceiling sensors for one frame before jump. if ceiling is as close
                //as 6 pixels from the player, don't jump.
                SolidTileData data = CeilingSensorCompetition();
                if (data.distance > 6)
                {
                    xSpeed -= JUMP_FORCE * Mathf.Sin(Mathf.DegToRad(groundAngle));
                    ySpeed -= JUMP_FORCE * Mathf.Cos(Mathf.DegToRad(groundAngle));
                    isGrounded = false;
                    SwitchGroundCollisionMode(0);
                    SwitchPushCollisionMode(0);
                    isJumping = true;
                    playerSprite.Play("liftoff");
                    playerSprite.SpeedScale = 1.0f;
                }
            }
            //if jumping, immediately go airborne, do not execute grounded 
            //code as player will immediately snap back to ground
            if (isGrounded)
            {
                if (controlLockTimer == 0)
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
                }

                bool isVertical = SwitchPushCollisionMode(groundAngle);
                float wallDistance = PushCollisionProcess(isVertical);

                //calculate X and Y speed from Ground Speed and Angle
                if (isVertical)
                {
                    xSpeed = groundSpeed * Mathf.Cos(Mathf.DegToRad(groundAngle));
                    ySpeed = (groundSpeed * -Mathf.Sin(Mathf.DegToRad(groundAngle))) + wallDistance;
                }
                else 
                {
                    xSpeed = (groundSpeed * Mathf.Cos(Mathf.DegToRad(groundAngle))) + wallDistance;
                    ySpeed = groundSpeed * -Mathf.Sin(Mathf.DegToRad(groundAngle));
                }

                if (wallDistance != 0)
                {
                    groundSpeed = 0;
                }

                //Move player pos
                Position = new Vector2(GlobalPosition.X + xSpeed, GlobalPosition.Y + ySpeed);

                
                //ground collision process
                SwitchGroundCollisionMode(groundAngle);
                bool groundCollision = true;
                SolidTileData groundData = GroundSensorCompetition();

                //if player is too far off the ground, they won't collide. this formula
                //takes player speed into account. the faster they move, the further they can
                //be off the ground and still collide
                if (sensorTable["A"].direction == "right" || sensorTable["A"].direction == "left")
                {
                    if (groundData.distance > Mathf.Min(Mathf.Abs(ySpeed) + 4, 14))
                        groundCollision = false;
                }
                else 
                {
                    if (groundData.distance > Mathf.Min(Mathf.Abs(xSpeed) + 4, 14))
                        groundCollision = false;
                }

                //eventually, collision calculation will be based off of the current mode.
                //for now, just assume floor mode
                if (groundCollision)
                {
                    if (sensorTable["A"].direction == "right")
                        Position = new Vector2(GlobalPosition.X + groundData.distance - 1,  GlobalPosition.Y); 
                    else if (sensorTable["A"].direction == "down")
                        Position = new Vector2(GlobalPosition.X, GlobalPosition.Y + groundData.distance); 
                    else if (sensorTable["A"].direction == "up")
                        Position = new Vector2(GlobalPosition.X, GlobalPosition.Y - groundData.distance);
                    else 
                        Position = new Vector2(GlobalPosition.X - groundData.distance + 1,  GlobalPosition.Y);

                    if (groundData.flagged)
                        groundAngle = (Mathf.Round(groundAngle / 90) % 4) * 90;
                    else
                        groundAngle = groundData.angle;
                    
                    playerSprite.RotationDegrees = -groundAngle;
                }
                else 
                {
                    isGrounded = false;
                    playerSprite.Play("airtransition");
                    playerSprite.SpeedScale = 1.0f;
                    currentFrame = 0;
                    SwitchGroundCollisionMode(0);
                    SwitchPushCollisionMode(0);
                }
                //if on flat ground, move sensors down to account for low steps
                if (groundAngle == 0)
                {
                    sensorTable["E"].Position = new Vector2(-pushRadius, 4);
                    sensorTable["F"].Position = new Vector2(pushRadius, 4);
                }

                //handle slipping/falling down slopes that are too steep
                if (controlLockTimer == 0)
                {
                    //if ground angle is within slip range and speed is too slow, slip
                    if (Mathf.Abs(groundSpeed) < 2.5f && groundAngle >= 35f && groundAngle <= 326f)
                    {
                        //lock controls
                        controlLockTimer = 30;

                        //if ground angle is within fall range, fall instead of slip
                        if (groundAngle >= 69f && groundAngle <= 293f)
                        {
                            isGrounded = false;
                            SwitchGroundCollisionMode(0);
                            SwitchPushCollisionMode(0);
                        }
                        //slipping
                        else 
                        {
                            if (groundAngle < 180f)
                                groundSpeed -= 0.5f;
                            else groundSpeed += 0.5f;
                        }
                    }
                }
                else 
                    controlLockTimer--;
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
                    playerSprite.Play("airtransition");
                    playerSprite.SpeedScale = 1.0f;
                    currentFrame = 0;
                }
            }

            if (playerSprite.Animation == "airtransition" && playerSprite.Frame == 1)
                playerSprite.Play("fall");
            if (playerSprite.Animation == "liftoff" && playerSprite.Frame == 2)
                playerSprite.Play("airtime");

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
            if (groundAngle > 360f || groundAngle < 0)
                groundAngle = 0; 

            playerSprite.RotationDegrees = -groundAngle;

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
                CeilingCollisionProcess();
            }
            else if (airAngle >= 46f && airAngle <= 135f)
            {
                //mostly up, ceiling and push sensors active
                AirPushCollisionProcess("E");
                AirPushCollisionProcess("F");
                CeilingCollisionProcess();
                
            }
            else if (airAngle >= 136f && airAngle <= 225f)
            {
                //mostly left, left, ceiling and ground
                AirPushCollisionProcess("E");
                AirGroundCollisionProcess(false);
                CeilingCollisionProcess();
            }
            else 
            {
                //mostly down, ground and push active
                AirPushCollisionProcess("E");
                AirPushCollisionProcess("F");
                AirGroundCollisionProcess(true);
            }
            
            if ((playerSprite.Animation == "airtime" || playerSprite.Animation == "liftoff") && airAngle >= 180f && (airAngle <= 360f || airAngle == 0))
                playerSprite.Play("airtransition");
        }
    }

    //push sensor collision process when airborne
    public void AirPushCollisionProcess(string sensorString)
    {
        SolidTileData pushData = sensorTable[sensorString].CheckForTile(currentLayer);
        if (pushData.distance <= 0)
        {
            if (xSpeed > 0)
                Position = new Vector2(Position.X + pushData.distance, Position.Y);
            else if (xSpeed < 0)
                Position = new Vector2(Position.X - pushData.distance, Position.Y);
            xSpeed = 0;
        }
    }

    //push sensor collision process when grounded. returns distance from tile to move the player with
    public float PushCollisionProcess(bool isVertical)
    {
        bool canCollide = false;
        if ((groundAngle >= 316f && groundAngle <= 360f) || (groundAngle >= 0f && groundAngle <= 44f))
        {
            if (groundSpeed != 0)
                canCollide = true;
        }
        else if (groundAngle >= 136f && groundAngle <= 224f)
        {
            if (groundSpeed != 0)
                canCollide = true;
        }
        //if player isn't moving, don't check sensors
        if (canCollide)
        {
            Sensor activeSensor;
            //get active sensor
            if (groundSpeed > 0)
                activeSensor = sensorTable["F"];
            else 
                activeSensor = sensorTable["E"];

            SolidTileData data;
            //player will not have moved yet, move push sensors to account for this
            if (isVertical)
            {
                activeSensor.Position = new Vector2(activeSensor.Position.X, activeSensor.Position.Y + groundSpeed);
                data = activeSensor.CheckForTile(currentLayer);
                activeSensor.Position = new Vector2(activeSensor.Position.X, activeSensor.Position.Y - groundSpeed);
            }
            else 
            {
                activeSensor.Position = new Vector2(activeSensor.Position.X + groundSpeed, activeSensor.Position.Y);
                data = activeSensor.CheckForTile(currentLayer);
                activeSensor.Position = new Vector2(activeSensor.Position.X - groundSpeed, activeSensor.Position.Y);
            }
            
            if (data.distance < 0)
            {
                if (activeSensor.direction == "left")
                    return -data.distance;
                else 
                    return data.distance;
            }
            else return 0;
        }
        else return 0;
    }

    //ground collision processing when in the air
    public void AirGroundCollisionProcess(bool goingDown)
    {
        bool groundCollision = true;
        SolidTileData groundData = GroundSensorCompetition();
        if (groundData.distance >= 0)
            groundCollision = false;
        //additional checks for ground collision based on air movement direction
        if (groundCollision) 
        {
            if (goingDown)
            {
                SolidTileData groundAData = sensorTable["A"].CheckForTile(currentLayer);
                SolidTileData groundBData = sensorTable["B"].CheckForTile(currentLayer);
                if (!(groundAData.distance >= -(ySpeed + 8) || groundBData.distance >= -(ySpeed + 8)))
                    groundCollision = false;
            }
            else
            {
                if (!(ySpeed >= 0))
                    groundCollision = false;
            }
            if (groundCollision)
            {
                Position = new Vector2(GlobalPosition.X, GlobalPosition.Y + groundData.distance); 
                isGrounded = true;
                isJumping = false;
                if (groundData.flagged)
                    groundAngle = (Mathf.Round(groundAngle / 90) % 4) * 90;
                else
                    groundAngle = groundData.angle;

                //calculating groundSpeed on land based on ground angle.
                //start with if floor is in slope range
                if ((groundAngle >= 0 && groundAngle <= 45f) || (groundAngle <= 360f && groundAngle >= 316f))
                    //ground is somewhat steep and player is moving mostly down
                    if (((groundAngle >= 24 && groundAngle <= 45f) || (groundAngle <= 359f && groundAngle >= 339f)) && goingDown)
                    {
                        //use half of ySpeed for this calculation
                        groundSpeed = -ySpeed * 0.5f * Mathf.Sign(Mathf.Sin(Mathf.DegToRad(groundAngle)));
                    }
                    //ground is either very flat or player is moving mostly left or right
                    else groundSpeed = xSpeed;
                //ground is very steep
                else 
                {
                    //if moving down, calculate new groundSpeed using ySpeed
                    if (goingDown)
                        groundSpeed = -ySpeed * Mathf.Sign(Mathf.Sin(Mathf.DegToRad(groundAngle)));
                    //just use xSpeed if moving mostly left or right
                    else groundSpeed = xSpeed;
                }
                //don't forget to set ySpeed to 0 at end
                ySpeed = 0;
            }
        }
    }

    //get shortest distance between the two ground sensors. 
    //returns SolidTileData object.
    public SolidTileData GroundSensorCompetition()
    {
        SolidTileData groundAData = sensorTable["A"].CheckForTile(currentLayer);
        SolidTileData groundBData = sensorTable["B"].CheckForTile(currentLayer);

        if (groundAData.distance == groundBData.distance)
            return groundAData;
        else if (groundAData.distance < groundBData.distance)
            return groundAData;
        else
            return groundBData;
    }

    //like ground sensor competition, but for ceiling sensors
    public SolidTileData CeilingSensorCompetition()
    {
        SolidTileData ceilingCData = sensorTable["C"].CheckForTile(currentLayer);
        SolidTileData ceilingDData = sensorTable["D"].CheckForTile(currentLayer);

        if (ceilingCData.distance == ceilingDData.distance)
            return ceilingDData;
        else if (ceilingCData.distance < ceilingDData.distance)
            return ceilingCData;
        else
            return ceilingDData;
    }

    //Ceiling sensor processing. Ceiling sensors are only active when airborne.
    public void CeilingCollisionProcess()
    {
        SolidTileData data = CeilingSensorCompetition();
        if (data.distance < 0)
        {
            Position = new Vector2(Position.X, Position.Y - data.distance);
            //later, detect if player lands
            ySpeed = 0;
        }
    }

    //rotate push sensors according to ground angle. returns if the sensors
    //are aligned horizontally (false) or vertically (true) with the ground
    public bool SwitchPushCollisionMode(float currentAngle)
    {
        if ((currentAngle >= 316f && currentAngle <= 360f) || (currentAngle >= 0f && currentAngle <= 44f))
        {
            sensorTable["E"].Position = new Vector2(-pushRadius, 0);
            sensorTable["E"].direction = "left";
            sensorTable["F"].Position = new Vector2(pushRadius, 0);
            sensorTable["F"].direction = "right";
            return false;
        }
        else if (currentAngle >= 45f && currentAngle <= 135f)
        {
            sensorTable["E"].Position = new Vector2(0, pushRadius);
            sensorTable["E"].direction = "down";
            sensorTable["F"].Position = new Vector2(0, -pushRadius);
            sensorTable["F"].direction = "up";
            return true;
        }
        else if (currentAngle >= 136f && currentAngle <= 224f)
        {
            sensorTable["E"].Position = new Vector2(pushRadius, 0);
            sensorTable["E"].direction = "right";
            sensorTable["F"].Position = new Vector2(-pushRadius, 0);
            sensorTable["F"].direction = "left";
            return false;
        }
        else 
        {
            sensorTable["E"].Position = new Vector2(0, -pushRadius);
            sensorTable["E"].direction = "up";
            sensorTable["F"].Position = new Vector2(0, pushRadius);
            sensorTable["F"].direction = "down";
            return true;
        }
    }

    public void SwitchGroundCollisionMode(float currentAngle)
    {
        if ((currentAngle >= 315f && currentAngle <= 360f) || (currentAngle >= 0f && currentAngle <= 45f))
        {
            sensorTable["A"].Position = new Vector2(-widthRadius, heightRadius);
            sensorTable["A"].direction = "down";
            sensorTable["B"].Position = new Vector2(widthRadius, heightRadius);
            sensorTable["B"].direction = "down";
        }
        else if (currentAngle >= 46f && currentAngle <= 134f)
        {
            sensorTable["A"].Position = new Vector2(heightRadius, -widthRadius);
            sensorTable["A"].direction = "right";
            sensorTable["B"].Position = new Vector2(heightRadius, widthRadius);
            sensorTable["B"].direction = "right";
        }
        else if (currentAngle >= 135f && currentAngle <= 225f)
        {
            sensorTable["A"].Position = new Vector2(widthRadius, -heightRadius);
            sensorTable["A"].direction = "up";
            sensorTable["B"].Position = new Vector2(-widthRadius, -heightRadius);
            sensorTable["B"].direction = "up";
        }
        else 
        {
            sensorTable["A"].Position = new Vector2(-heightRadius, widthRadius);
            sensorTable["A"].direction = "left";
            sensorTable["B"].Position = new Vector2(-heightRadius, -widthRadius);
            sensorTable["B"].direction = "left";
        }
    }

    public void OnHitboxEnter(Area2D other)
    {
        GameObject otherObject = other.GetParent<GameObject>();
        if (otherObject == null) return;
        
        if (otherObject.hitboxReaction == ObjectType.RoutineIncrement)
        {
            ((RoutineGameObject)otherObject).IncrementRoutine();
        }
    }
}
