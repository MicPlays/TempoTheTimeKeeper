using Godot;
using System;

public partial class PlayerPhysicsComponent : Node
{
    [ExportGroup("Physics Constants")]
    [Export]
    public float ACC_SPEED {get; set;} = 2.8125f;
    [Export]
    public float DEC_SPEED {get; set;} = 30f;
    [Export]
    public float FRICTION_SPEED {get; set;} = 2.8125f;
    [Export]
    public int TOP_SPEED {get; set;} = 360;
    [Export]
    public float JUMP_FORCE {get; set;} = 390f;
    [Export]
    public float GRAVITY_FORCE {get; set;} = 13.125f;
    [Export]
    public float AIR_ACC_SPEED {get; set;} = 5.625f;
    [Export]
    public float SLOPE_FACTOR {get; set;} = 7.5f;
    [Export]
    public float SLOPE_SPEED_FACTOR {get; set;} = 0.05078125f;
    [Export]
    public float SHORT_HOP_FLOOR {get; set;} = -240f;
    [Export]
    public float HURT_X_FORCE {get; set;} = 120f;
    [Export]
    public float HURT_Y_FORCE {get; set;} = -160f;

    public Player player;

    //adjust ground speed according to slope factor
    public void FactorSlope(float delta)
    {
        if (player.cc.sensorTable["A"].direction != "up" && player.groundSpeed != 0)
            player.groundSpeed -= SLOPE_FACTOR * delta * Mathf.Sin(Mathf.DegToRad(player.groundAngle)); 
    }

    public virtual void Jump(float delta)
    {
        SolidTileData data = player.cc.CeilingSensorCompetition();
        if (data.distance > 6)
        {
            player.xSpeed -= JUMP_FORCE * delta * Mathf.Sin(Mathf.DegToRad(player.groundAngle));
            player.ySpeed -= JUMP_FORCE * delta * Mathf.Cos(Mathf.DegToRad(player.groundAngle));
            if (Mathf.Abs(player.xSpeed) / (360 * (float)delta) > 1)
                SHORT_HOP_FLOOR = -240f * (float)delta;
            else SHORT_HOP_FLOOR = Mathf.Lerp(-240f * (float)delta, -150f * (float)delta, Mathf.Abs(player.xSpeed) / (360 * (float)delta));
            player.psm.TransitionState(new PlayerJump());
        }
    }

    //apply gravity
    public virtual void ApplyGravity(float delta)
    {
        player.ySpeed += GRAVITY_FORCE * delta;
        if (player.ySpeed > 960 * delta) player.ySpeed = 960 * (float)delta;
    }

    public virtual void ApplyHurtForce(float delta)
    {
        player.xSpeed = -Mathf.Sign(player.xSpeed) * HURT_X_FORCE * delta;
        player.ySpeed = HURT_Y_FORCE * delta;
    }

    //rotate player angle back to 0
    public void RotateGroundAngle()
    {
        if (player.groundAngle < 180f)
            player.groundAngle -= 2.8125f;
        else 
            player.groundAngle += 2.8125f;
        if (player.groundAngle > 360f || player.groundAngle < 0)
            player.groundAngle = 0; 
    }

    public virtual void JumpRelease(float delta)
    {
        if (player.ySpeed < -240f * delta)
            player.ySpeed = SHORT_HOP_FLOOR;
    }

    public void LeftGroundForce(float delta)
    {
        if (player.groundSpeed > 0)
            player.groundSpeed -= DEC_SPEED * delta;
        else if (player.groundSpeed > -TOP_SPEED * delta)
        {
            player.groundSpeed -= ACC_SPEED * delta;
            if (player.groundSpeed <= -TOP_SPEED * delta)
                player.groundSpeed = -TOP_SPEED * delta;
        }
    }

    public void RightGroundForce(float delta)
    {
        if (player.groundSpeed < 0)
            player.groundSpeed += DEC_SPEED * delta;
            
        else if (player.groundSpeed < TOP_SPEED * delta)
        {
            player.groundSpeed += ACC_SPEED * delta;
            if (player.groundSpeed >= TOP_SPEED * delta)
                player.groundSpeed = TOP_SPEED * delta;
        }
    }

    public void LeftAirForce(float delta)
    {
        if (player.xSpeed > -TOP_SPEED *  delta)
        {
            player.xSpeed -= AIR_ACC_SPEED * delta;
            if (player.xSpeed <= -TOP_SPEED * delta)
                player.xSpeed = -TOP_SPEED * delta;
        }
    }

    public void RightAirForce(float delta)
    {
        if (player.xSpeed < TOP_SPEED * delta)
        {
            player.xSpeed += AIR_ACC_SPEED * delta;
            if (player.xSpeed >= TOP_SPEED * delta)
                player.xSpeed = TOP_SPEED * delta;
        }
    }

    public void AirDrag(float delta)
    {
        if (player.ySpeed < 0 && player.ySpeed > -240f * delta)
        {
            player.xSpeed -= player.xSpeed/7.5f * delta/256;
        }
    }

    public void Decelerate(float delta)
    {
        player.groundSpeed -= Mathf.Min(Mathf.Abs(player.groundSpeed), FRICTION_SPEED * delta) * Mathf.Sign(player.groundSpeed);
    }

    public void SetSpeedOnGround(bool isVertical, float wallDistance)
    {
        //calculate X and Y speed from Ground Speed and Angle
        if (isVertical)
        {
            player.xSpeed = player.groundSpeed * Mathf.Cos(Mathf.DegToRad(player.groundAngle));
            player.ySpeed = (player.groundSpeed * -Mathf.Sin(Mathf.DegToRad(player.groundAngle))) + wallDistance;
        }
        else 
        {
            player.xSpeed = (player.groundSpeed * Mathf.Cos(Mathf.DegToRad(player.groundAngle))) + wallDistance;
            player.ySpeed = player.groundSpeed * -Mathf.Sin(Mathf.DegToRad(player.groundAngle));
        }

        if (wallDistance != 0)
        {
            player.groundSpeed = 0;
        }
    }

    public void MovePlayerObject()
    {
        //Move player pos
        player.Position = new Vector2(player.GlobalPosition.X + player.xSpeed, player.GlobalPosition.Y + player.ySpeed);
    }

    public virtual bool CheckForSlip(float delta)
    {
        //if ground angle is within slip range and speed is too slow, slip
        if (Mathf.Abs(player.groundSpeed) < 150f * (float)delta && player.groundAngle >= 35f && player.groundAngle <= 326f)
        {
            //lock controls
            player.controlLockTimer = 30 * delta;

            //if ground angle is within fall range, fall instead of slip
            if (player.groundAngle >= 69f && player.groundAngle <= 293f)
            {
                player.psm.TransitionState(new PlayerFall());
                return true;
            }
            //slipping
            else 
            {
                if (player.groundAngle < 180f)
                    player.groundSpeed -= 30f * delta;
                else player.groundSpeed += 30f * delta;;
                return false;
            }
        }
        return false;
    }

    public void AdjustSpeedOnLanding(bool goingDown)
    {
        //calculating groundSpeed on land based on ground angle.
        //start with if floor is in slope range
        if ((player.groundAngle >= 0 && player.groundAngle <= 45f) || (player.groundAngle <= 360f && player.groundAngle >= 316f))
            //ground is somewhat steep and player is moving mostly down
            if (((player.groundAngle >= 24 && player.groundAngle <= 45f) || (player.groundAngle <= 359f && player.groundAngle >= 339f)) && goingDown)
            {
                //use half of ySpeed for this calculation
                player.groundSpeed = -player.ySpeed * 0.5f * Mathf.Sign(Mathf.Sin(Mathf.DegToRad(player.groundAngle)));
            }
            //ground is either very flat or player is moving mostly left or right
            else player.groundSpeed = player.xSpeed;
        //ground is very steep
        else 
        {
            //if moving down, calculate new groundSpeed using ySpeed
            if (goingDown)
                player.groundSpeed = -player.ySpeed * Mathf.Sign(Mathf.Sin(Mathf.DegToRad(player.groundAngle)));
            //just use xSpeed if moving mostly left or right
            else player.groundSpeed = player.xSpeed;
        }
        //don't forget to set ySpeed to 0 at end
        player.ySpeed = 0;
    }

    public void SetDeathSpeedAndDirection(float delta)
    {
        player.xSpeed = 0;
        player.ySpeed = -420f * delta;
    }
}
