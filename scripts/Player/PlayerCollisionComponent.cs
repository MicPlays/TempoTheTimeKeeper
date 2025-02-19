using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerCollisionComponent : Hitbox
{
    public Player player;

    [Export]
    public NodePath sensorContainerPath;
    public Dictionary<string, Sensor> sensorTable;
    //sensor stuff

    public virtual void Init()
    {
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
        sensorTable["A"].Position = new Vector2(-player.widthRadius, player.heightRadius);
        sensorTable["B"].Position = new Vector2(player.widthRadius, player.heightRadius);
        sensorTable["C"].Position = new Vector2(-player.widthRadius, -player.heightRadius);
        sensorTable["D"].Position = new Vector2(player.widthRadius, -player.heightRadius);
        sensorTable["E"].Position = new Vector2(-player.pushRadius, 0);
        sensorTable["F"].Position = new Vector2(player.pushRadius, 0);
    }

    public virtual SolidTileData CeilingSensorCompetition()
    {
        SolidTileData ceilingCData = sensorTable["C"].CheckForTile(player.currentLayer);
        SolidTileData ceilingDData = sensorTable["D"].CheckForTile(player.currentLayer);

        if (ceilingCData.distance == ceilingDData.distance)
            return ceilingDData;
        else if (ceilingCData.distance < ceilingDData.distance)
            return ceilingCData;
        else
            return ceilingDData;
    }

    //rotate push sensors according to ground angle. returns if the sensors
    //are aligned horizontally (false) or vertically (true) with the ground
    public virtual bool SwitchPushCollisionMode(float currentAngle)
    {
        if ((currentAngle >= 316f && currentAngle <= 360f) || (currentAngle >= 0f && currentAngle <= 44f))
        {
            sensorTable["E"].Position = new Vector2(-player.pushRadius, 0);
            sensorTable["E"].direction = "left";
            sensorTable["F"].Position = new Vector2(player.pushRadius, 0);
            sensorTable["F"].direction = "right";
            return false;
        }
        else if (currentAngle >= 45f && currentAngle <= 135f)
        {
            sensorTable["E"].Position = new Vector2(0, player.pushRadius);
            sensorTable["E"].direction = "down";
            sensorTable["F"].Position = new Vector2(0, -player.pushRadius);
            sensorTable["F"].direction = "up";
            return true;
        }
        else if (currentAngle >= 136f && currentAngle <= 224f)
        {
            sensorTable["E"].Position = new Vector2(player.pushRadius, 0);
            sensorTable["E"].direction = "right";
            sensorTable["F"].Position = new Vector2(-player.pushRadius, 0);
            sensorTable["F"].direction = "left";
            return false;
        }
        else 
        {
            sensorTable["E"].Position = new Vector2(0, -player.pushRadius);
            sensorTable["E"].direction = "up";
            sensorTable["F"].Position = new Vector2(0, player.pushRadius);
            sensorTable["F"].direction = "down";
            return true;
        }
    }

    public virtual void SwitchGroundCollisionMode(float currentAngle)
    {
        if ((currentAngle >= 315f && currentAngle <= 360f) || (currentAngle >= 0f && currentAngle <= 45f))
        {
            sensorTable["A"].Position = new Vector2(-player.widthRadius, player.heightRadius);
            sensorTable["A"].direction = "down";
            sensorTable["B"].Position = new Vector2(player.widthRadius, player.heightRadius);
            sensorTable["B"].direction = "down";
        }
        else if (currentAngle >= 46f && currentAngle <= 134f)
        {
            sensorTable["A"].Position = new Vector2(player.heightRadius, -player.widthRadius);
            sensorTable["A"].direction = "right";
            sensorTable["B"].Position = new Vector2(player.heightRadius, player.widthRadius);
            sensorTable["B"].direction = "right";
        }
        else if (currentAngle >= 135f && currentAngle <= 225f)
        {
            sensorTable["A"].Position = new Vector2(player.widthRadius, -player.heightRadius);
            sensorTable["A"].direction = "up";
            sensorTable["B"].Position = new Vector2(-player.widthRadius, -player.heightRadius);
            sensorTable["B"].direction = "up";
        }
        else 
        {
            sensorTable["A"].Position = new Vector2(-player.heightRadius, player.widthRadius);
            sensorTable["A"].direction = "left";
            sensorTable["B"].Position = new Vector2(-player.heightRadius, -player.widthRadius);
            sensorTable["B"].direction = "left";
        }
    }

    public virtual float PushCollisionProcess(bool isVertical)
    {
        bool canCollide = false;
        if ((player.groundAngle >= 316f && player.groundAngle <= 360f) || (player.groundAngle >= 0f && player.groundAngle <= 44f))
        {
            if (player.groundSpeed != 0)
                canCollide = true;
        }
        else if (player.groundAngle >= 136f && player.groundAngle <= 224f)
        {
            if (player.groundSpeed != 0)
                canCollide = true;
        }
        //if player isn't moving, don't check sensors
        if (canCollide)
        {
            Sensor activeSensor;
            //get active sensor
            if (player.groundSpeed > 0)
                activeSensor = sensorTable["F"];
            else 
                activeSensor = sensorTable["E"];

            SolidTileData data;
            //player will not have moved yet, move push sensors to account for this
            if (isVertical)
            {
                activeSensor.Position = new Vector2(activeSensor.Position.X, activeSensor.Position.Y + player.groundSpeed);
                data = activeSensor.CheckForTile(player.currentLayer);
                activeSensor.Position = new Vector2(activeSensor.Position.X, activeSensor.Position.Y - player.groundSpeed);
            }
            else 
            {
                activeSensor.Position = new Vector2(activeSensor.Position.X + player.groundSpeed, activeSensor.Position.Y);
                data = activeSensor.CheckForTile(player.currentLayer);
                activeSensor.Position = new Vector2(activeSensor.Position.X - player.groundSpeed, activeSensor.Position.Y);
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

    public virtual bool GroundCollisionProcess()
    {
        bool groundCollision = true;
        SolidTileData groundData = GroundSensorCompetition();

        //if player is too far off the ground, they won't collide. this formula
        //takes player speed into account. the faster they move, the further they can
        //be off the ground and still collide
        if (sensorTable["A"].direction == "right" || sensorTable["A"].direction == "left")
        {
            if (groundData.distance > Mathf.Min(Mathf.Abs(player.ySpeed) + 4, 14))
                groundCollision = false;
        }
        else 
        {
            if (groundData.distance > Mathf.Min(Mathf.Abs(player.xSpeed) + 4, 14))
                groundCollision = false;
        }
        if (groundCollision)
        {
            if (sensorTable["A"].direction == "right")
                player.Position = new Vector2(player.GlobalPosition.X + groundData.distance - 1,  player.GlobalPosition.Y); 
            else if (sensorTable["A"].direction == "down")
                player.Position = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Y + groundData.distance); 
            else if (sensorTable["A"].direction == "up")
                player.Position = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Y - groundData.distance);
            else 
                player.Position = new Vector2(player.GlobalPosition.X - groundData.distance + 1,  player.GlobalPosition.Y);

            if (groundData.flagged)
                player.groundAngle = (Mathf.Round(player.groundAngle / 90) % 4) * 90;
            else
                player.groundAngle = groundData.angle;
            SwitchGroundCollisionMode(player.groundAngle);
        }
        else 
        {
            SwitchGroundCollisionMode(0);
            SwitchPushCollisionMode(0);
            player.psm.TransitionState(new PlayerFall());
        }
        return groundCollision;
    }

    public virtual bool CheckIfGrounded()
    {
        bool groundCollision = true;
        SolidTileData groundData = GroundSensorCompetition();

        //if player is too far off the ground, they won't collide. this formula
        //takes player speed into account. the faster they move, the further they can
        //be off the ground and still collide
        if (sensorTable["A"].direction == "right" || sensorTable["A"].direction == "left")
        {
            if (groundData.distance > Mathf.Min(Mathf.Abs(player.ySpeed) + 4, 14))
                groundCollision = false;
        }
        else 
        {
            if (groundData.distance > Mathf.Min(Mathf.Abs(player.xSpeed) + 4, 14))
                groundCollision = false;
        }
        return groundCollision;
    }

    public virtual SolidTileData GroundSensorCompetition()
    {
        SolidTileData groundAData = sensorTable["A"].CheckForTile(player.currentLayer);
        SolidTileData groundBData = sensorTable["B"].CheckForTile(player.currentLayer);

        if (groundAData.distance == groundBData.distance)
            return groundAData;
        else if (groundAData.distance < groundBData.distance)
            return groundAData;
        else
            return groundBData;
    }

    public virtual void LowStepCorrection()
    {
        //if on flat ground, move sensors down to account for low steps
        if (player.groundAngle == 0)
        {
            sensorTable["E"].Position = new Vector2(-player.pushRadius, 4);
            sensorTable["F"].Position = new Vector2(player.pushRadius, 4);
        }
    }

    public virtual float GetAngleOfAirMovement()
    {
        //get angle of air movement
        Vector2 airAngleVector = new Vector2(player.xSpeed, player.ySpeed).Normalized();
        float airAngle = Mathf.RadToDeg(Mathf.Atan2(-airAngleVector.Y, airAngleVector.X));
        if (airAngle < 0)
            airAngle += 360f;
        if (airAngle > 360f)
            airAngle -= 360f;

        return airAngle;
    }

    public virtual bool DetermineAirCollisionMode(float airAngle)
    {
        //determine air collision type
        if ((airAngle >= 0f && airAngle <= 45f) || (airAngle >= 316 && airAngle <= 360))
        {
            //mostly right, right, ceiling, and ground sensors active
            bool groundCollision = player.cc.AirGroundCollisionProcess(false);
            if (groundCollision)
            {
                player.pc.AdjustSpeedOnLanding(false);
                player.psm.TransitionState(new PlayerGrounded());
                return true;
            }
            else 
            {
                player.cc.CeilingCollisionProcess(false);
                return false;
            }
        }
        else if (airAngle >= 46f && airAngle <= 135f)
        {
            //mostly up, ceiling and push sensors active
            bool landOnCeiling = player.cc.CeilingCollisionProcess(true);
            if (landOnCeiling)
            {
                player.psm.TransitionState(new PlayerGrounded());
                return true;
            }
            else return false;
        }
        else if (airAngle >= 136f && airAngle <= 225f)
        {
            //mostly left, left, ceiling and ground
            bool groundCollision = player.cc.AirGroundCollisionProcess(false);
            if (groundCollision)
            {
                player.pc.AdjustSpeedOnLanding(false);
                player.psm.TransitionState(new PlayerGrounded());
                return true;
            }
            else 
            {
                player.cc.CeilingCollisionProcess(false);
                return false;
            }
        }
        else 
        {
            //mostly down, ground and push active
            bool groundCollision = player.cc.AirGroundCollisionProcess(true);
            if (groundCollision)
            {
                player.pc.AdjustSpeedOnLanding(true);
                player.psm.TransitionState(new PlayerGrounded());
                return true;
            }
            else return false;
        }
    }

    public virtual void AirPushCollisionProcess()
    {
        Sensor activeSensor;
        //get active sensor
        if (player.xSpeed > 0)
            activeSensor = sensorTable["F"];
        else 
            activeSensor = sensorTable["E"];
        
        SolidTileData pushData = activeSensor.CheckForTile(player.currentLayer);
        if (pushData.distance <= 0)
        {
            //reset player position (push against wall)
            if (player.xSpeed > 0)
                player.Position = new Vector2(player.Position.X + pushData.distance, player.Position.Y);
            else if (player.xSpeed < 0)
                player.Position = new Vector2(player.Position.X - pushData.distance, player.Position.Y);
            player.xSpeed = 0;
        }
    }

    public virtual bool AirGroundCollisionProcess(bool goingDown)
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
                SolidTileData groundAData = sensorTable["A"].CheckForTile(player.currentLayer);
                SolidTileData groundBData = sensorTable["B"].CheckForTile(player.currentLayer);
                if (!(groundAData.distance >= -(player.ySpeed + 8) || groundBData.distance >= -(player.ySpeed + 8)))
                    groundCollision = false;
            }
            else
            {
                if (!(player.ySpeed >= 0))
                    groundCollision = false;
            }
            if (groundCollision)
            {
                player.Position = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Y + groundData.distance); 
                if (groundData.flagged)
                    player.groundAngle = (Mathf.Round(player.groundAngle / 90) % 4) * 90;
                else
                    player.groundAngle = groundData.angle;

                return true;
            }
            else return false;
        }
        else return false;
    }

    public virtual bool CeilingCollisionProcess(bool goingUp)
    {
        SolidTileData data = CeilingSensorCompetition();
        if (data.distance < 0)
        {
            player.Position = new Vector2(player.Position.X, player.Position.Y - data.distance);
            float tileAngle = 0;
            if (data.flagged)
                tileAngle = (Mathf.Round(player.groundAngle / 90) % 4) * 90;
            else tileAngle = data.angle;
            if ((tileAngle > 90 && tileAngle < 136f) || (tileAngle < 270 && tileAngle > 225f))
            {
                if (goingUp)
                {
                    if (data.flagged)
                        player.groundAngle = (Mathf.Round(player.groundAngle / 90) % 4) * 90;
                    else
                        player.groundAngle = data.angle;
                    player.groundSpeed = player.ySpeed * -Mathf.Sign(Mathf.Sin(player.groundAngle));
                    return true;
                }
                else 
                {
                    player.ySpeed = 0;
                    return false;
                }
            }
            else 
            {
                player.ySpeed = 0;
                return false;
            }
        }
        else return false;
    }
}
