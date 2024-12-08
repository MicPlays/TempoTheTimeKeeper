using Godot;
using System;
using System.Collections.Generic;

public partial class Player : GameObject
{
    //constants
    private const float ACC_SPEED = 2.8125f;
    //0.046875f
    private const float DEC_SPEED = 30f;
    //0.5f
    private const float FRICTION_SPEED = 2.8125f;
    private const int TOP_SPEED = 360;
    private const float JUMP_FORCE = 390f;
    //6.5f
    private const float GRAVITY_FORCE = 13.125f;
    //0.21875f
    private const float AIR_ACC_SPEED = 5.625f;
    //0.09375f
    private const float SLOPE_FACTOR = 7.5f;
    //0.125f
    private const float SLOPE_SPEED_FACTOR =  0.05078125f;
    private const float SPEED_BOOST_TIME_WINDOW = 5f;
    private const float SUPER_SPEED_BOOST_TIME_WINDOW = 0.5f;
    private const float GROUND_JUMP_BOOST = 60f;
    private const float WALL_JUMP_BOOST = 60f;
    private const float WALL_JUMP_FORCE = 300f;
    private const float WALL_SLIDE_FORCE = 9.5625f;

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
    public bool isWallJumpProcess = false;
    private float controlLockTimer = 0;
    private bool speedBoostInputTimerActive = false;
    private float speedBoostInputTimer = 0;
    private float shortHopFloor = -150f;
    private bool wallSlideActivated = false;
    
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
                groundSpeed -= SLOPE_FACTOR * (float)delta * Mathf.Sin(Mathf.DegToRad(groundAngle)); 
            
            //jump check
            if (Input.IsActionPressed("jump"))
            {
                //activate ceiling sensors for one frame before jump. if ceiling is as close
                //as 6 pixels from the player, don't jump.
                SolidTileData data = CeilingSensorCompetition();
                if (data.distance > 6)
                {
                    xSpeed -= JUMP_FORCE * (float)delta * Mathf.Sin(Mathf.DegToRad(groundAngle));
                    if (speedBoostInputTimer < SUPER_SPEED_BOOST_TIME_WINDOW * delta)
                    {
                        xSpeed += Mathf.Sign(xSpeed) * (GROUND_JUMP_BOOST * (float)delta);
                    }
                    else if (speedBoostInputTimer < SPEED_BOOST_TIME_WINDOW * delta)
                    {
                        xSpeed += Mathf.Sign(xSpeed) * (GROUND_JUMP_BOOST * (float)delta);
                        if (Mathf.Abs(xSpeed) < TOP_SPEED)
                            xSpeed = Mathf.Sign(xSpeed) * Mathf.Abs(xSpeed);
                    }
                    ySpeed -= JUMP_FORCE * (float)delta * Mathf.Cos(Mathf.DegToRad(groundAngle));
                    if (Mathf.Abs(xSpeed) / (360 * (float)delta) > 1)
                        shortHopFloor = -240f * (float)delta;
                    else shortHopFloor = Mathf.Lerp(-240f * (float)delta, -150f * (float)delta, Mathf.Abs(xSpeed) / (360 * (float)delta));
                    isGrounded = false;
                    SwitchGroundCollisionMode(0);
                    SwitchPushCollisionMode(0);
                    isJumping = true;
                    playerSprite.Play("liftoff");
                    playerSprite.SpeedScale = 1.0f;
                    speedBoostInputTimer = 0;
                    speedBoostInputTimerActive = false;
                }
            }
            //if jumping, immediately go airborne, do not execute grounded 
            //code as player will immediately snap back to ground
            if (isGrounded)
            {
                if (speedBoostInputTimerActive)
                    speedBoostInputTimer += (float)delta;
                if (controlLockTimer == 0)
                {
                    //ground running input
                    if (Input.IsActionPressed("left"))
                    {
                        if (groundSpeed > 0)
                            groundSpeed -= DEC_SPEED * (float)delta;
                            
                        else if (groundSpeed > -TOP_SPEED * (float)delta)
                        {
                            groundSpeed -= ACC_SPEED * (float)delta;
                            if (groundSpeed <= -TOP_SPEED * (float)delta)
                                groundSpeed = -TOP_SPEED * (float)delta;
                        }
                        
                    }
                    else if (Input.IsActionPressed("right"))
                    {
                        if (groundSpeed < 0)
                            groundSpeed += DEC_SPEED * (float)delta;
                            
                        else if (groundSpeed < TOP_SPEED * (float)delta)
                        {
                            groundSpeed += ACC_SPEED * (float)delta;
                            if (groundSpeed >= TOP_SPEED * (float)delta)
                                groundSpeed = TOP_SPEED * (float)delta;
                        }
                    }
                    else 
                        groundSpeed -= Mathf.Min(Mathf.Abs(groundSpeed), FRICTION_SPEED * (float)delta * Mathf.Sign(groundSpeed));
                }

                bool isVertical = SwitchPushCollisionMode(groundAngle);
                float wallDistance = PushCollisionProcess(isVertical);
                wallDistance *= 60 * (float)delta;

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
                    SwitchGroundCollisionMode(groundAngle);
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
                    if (Mathf.Abs(groundSpeed) < 150f * (float)delta && groundAngle >= 35f && groundAngle <= 326f)
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
                                groundSpeed -= 30f * (float)delta;
                            else groundSpeed += 30f * (float)delta;;
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
                    if (ySpeed < -240f * (float)delta)
                        ySpeed = shortHopFloor;
                    isJumping = false;
                    playerSprite.Play("airtransition");
                    playerSprite.SpeedScale = 1.0f;
                    currentFrame = 0;
                }
                else if (playerSprite.Animation == "walljump" && playerSprite.Frame == 2)
                    playerSprite.Play("airtime");
            }

            if (playerSprite.Animation == "airtransition" && playerSprite.Frame == 1)
                playerSprite.Play("fall");
            if (playerSprite.Animation == "liftoff" && playerSprite.Frame == 2)
                playerSprite.Play("airtime");

            //input movement
            if (Input.IsActionPressed("left"))
            {
                if (xSpeed > -TOP_SPEED * (float)delta)
                {
                    xSpeed -= AIR_ACC_SPEED * (float)delta;
                    if (xSpeed <= -TOP_SPEED * (float)delta)
                        xSpeed = -TOP_SPEED * (float)delta;
                }
            }
            else if (Input.IsActionPressed("right"))
            {
                if (xSpeed < TOP_SPEED * (float)delta)
                {
                    xSpeed += AIR_ACC_SPEED * (float)delta;
                    if (xSpeed >= TOP_SPEED * (float)delta)
                        xSpeed = TOP_SPEED * (float)delta;
                }
            }
        
            //air drag
            if (ySpeed < 0 && ySpeed > -240f * (float)delta)
            {
                xSpeed -= xSpeed/7.5f * (float)delta/256;
            }
        
            //Move player pos
            Position = new Vector2(GlobalPosition.X + xSpeed, GlobalPosition.Y + ySpeed);

            //apply gravity
            if (!wallSlideActivated)
            {
                ySpeed += GRAVITY_FORCE * (float)delta;
                if (ySpeed > 960 * (float)delta) ySpeed = 960 * (float)delta;
            }

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

            if (speedBoostInputTimerActive)
                speedBoostInputTimer += (float)delta;

            if ((airAngle >= 0f && airAngle <= 45f) || (airAngle >= 316 && airAngle <= 360))
            {
                //mostly right, right, ceiling, and ground sensors active
                AirPushCollisionProcess("F", (float)delta);
                AirGroundCollisionProcess(false);
                CeilingCollisionProcess(false);
            }
            else if (airAngle >= 46f && airAngle <= 135f)
            {
                //mostly up, ceiling and push sensors active
                AirPushCollisionProcess("E", (float)delta);
                AirPushCollisionProcess("F", (float)delta);
                CeilingCollisionProcess(true);
                
            }
            else if (airAngle >= 136f && airAngle <= 225f)
            {
                //mostly left, left, ceiling and ground
                AirPushCollisionProcess("E", (float)delta);
                AirGroundCollisionProcess(false);
                CeilingCollisionProcess(false);
            }
            else 
            {
                //mostly down, ground and push active
                AirPushCollisionProcess("E", (float)delta);
                AirPushCollisionProcess("F", (float)delta);
                AirGroundCollisionProcess(true);
            }
            
            if ((playerSprite.Animation == "airtime" || playerSprite.Animation == "liftoff") && airAngle >= 180f && (airAngle <= 360f || airAngle == 0))
                playerSprite.Play("airtransition");
        }
    }

    //push sensor collision process when airborne
    public void AirPushCollisionProcess(string sensorString, float delta)
    {
        SolidTileData pushData = sensorTable[sensorString].CheckForTile(currentLayer);
        if (pushData.distance <= 0)
        {
            //reset player position (push against wall) but keep xSpeed for wall jump
            if (xSpeed > 0)
                Position = new Vector2(Position.X + pushData.distance, Position.Y);
            else if (xSpeed < 0)
                Position = new Vector2(Position.X - pushData.distance, Position.Y);
            //wall jump process
            if (!isWallJumpProcess)
            {
                isWallJumpProcess = true;
                speedBoostInputTimerActive = true;
                playerSprite.Play("wallattach");
                if (sensorString == "E") playerSprite.FlipH = false;
                else playerSprite.FlipH = true;
            }
            else
            {
                if (speedBoostInputTimer <= SUPER_SPEED_BOOST_TIME_WINDOW * delta)
                {
                    if (Input.IsActionJustPressed("jump"))
                    {
                        GD.Print("super wall jump");
                        xSpeed = -xSpeed / 2;
                        xSpeed += Mathf.Sign(xSpeed) * ((WALL_JUMP_FORCE * delta) + (WALL_JUMP_BOOST * delta));
                        ySpeed = -(JUMP_FORCE * delta);
                        if (xSpeed < 0)
                            playerSprite.FlipH = true;
                        else playerSprite.FlipH = false;
                        playerSprite.Play("walljump");
                        speedBoostInputTimer = 0;
                        speedBoostInputTimerActive = false;
                        isWallJumpProcess = false;
                        isJumping = true;
                    }
                }
                else if (speedBoostInputTimer <= SPEED_BOOST_TIME_WINDOW * delta)
                {
                    int direction = 1;
                    if (sensorString == "F") direction = -1;
                    xSpeed = 0;
                    if (Input.IsActionJustPressed("jump"))
                    {
                        GD.Print("boosted wall jump");
                        xSpeed = direction * ((WALL_JUMP_FORCE * delta) + (WALL_JUMP_BOOST * delta));
                        ySpeed = -(JUMP_FORCE * delta);
                        if (xSpeed < 0)
                            playerSprite.FlipH = true;
                        else playerSprite.FlipH = false;
                        playerSprite.Play("walljump");
                        speedBoostInputTimer = 0;
                        speedBoostInputTimerActive = false;
                        isWallJumpProcess = false;
                        wallSlideActivated = false;
                        isJumping = true;
                    }
                    else
                    {
                        if (ySpeed > 0)
                            ySpeed += WALL_SLIDE_FORCE * delta;
                        else ySpeed += GRAVITY_FORCE * delta;
                        playerSprite.Play("wallslide");
                        wallSlideActivated = true;
                    }
                }
                else 
                {   
                    xSpeed = 0;
                    //left wall
                    if (Input.IsActionPressed("left") && sensorString == "E")
                    {
                        
                        if (Input.IsActionJustPressed("jump"))
                        {
                            GD.Print("regular wall jump");
                            xSpeed += WALL_JUMP_FORCE * delta;
                            ySpeed = -(JUMP_FORCE * delta);
                            playerSprite.FlipH = false;
                            playerSprite.Play("walljump");
                            speedBoostInputTimer = 0;
                            speedBoostInputTimerActive = false;
                            isWallJumpProcess = false;
                            wallSlideActivated = false;
                            isJumping = true;
                        }
                        else
                        {
                            if (ySpeed > 0)
                                ySpeed += WALL_SLIDE_FORCE * delta;
                            else ySpeed += GRAVITY_FORCE * delta;
                        }
                    }
                    //right wall
                    else if (Input.IsActionPressed("right") && sensorString == "F")
                    {
                        if (Input.IsActionJustPressed("jump"))
                        {
                            GD.Print("regular wall jump");
                            xSpeed -= WALL_JUMP_FORCE * delta;
                            ySpeed = -(JUMP_FORCE * delta);
                            playerSprite.Play("walljump");
                            speedBoostInputTimer = 0;
                            speedBoostInputTimerActive = false;
                            isWallJumpProcess = false;
                            playerSprite.FlipH = true;
                            wallSlideActivated = false;
                            isJumping = true;
                        }
                        else
                        {
                            if (ySpeed > 0)
                                ySpeed += WALL_SLIDE_FORCE * delta;
                            else ySpeed += GRAVITY_FORCE * delta;
                        }
                    }
                }
            }
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
                speedBoostInputTimer = 0;
                speedBoostInputTimerActive = true;
                isWallJumpProcess = false;
                wallSlideActivated = false;
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
    public void CeilingCollisionProcess(bool goingUp)
    {
        SolidTileData data = CeilingSensorCompetition();
        if (data.distance < 0)
        {
            Position = new Vector2(Position.X, Position.Y - data.distance);
            
            if (data.angle < 136f || data.angle > 225f)
            {
                if (goingUp)
                {
                    isGrounded = true;
                    isJumping = false;
                    if (data.flagged)
                        groundAngle = (Mathf.Round(groundAngle / 90) % 4) * 90;
                    else
                        groundAngle = data.angle;
                    groundSpeed = ySpeed * -Mathf.Sign(Mathf.Sin(groundAngle));
                }
                else ySpeed = 0;
            }
            else ySpeed = 0;
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
