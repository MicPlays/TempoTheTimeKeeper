using Godot;
using System;

public partial class TempoPhysicsComponent : PlayerPhysicsComponent
{
    [ExportGroup("Physics Constants")]
    [Export]
    private float SPEED_BOOST_TIME_WINDOW {get; set;} = 5f;
    [Export]
    private float SUPER_SPEED_BOOST_TIME_WINDOW {get; set;} = 3.5f;
    [Export]
    private float GROUND_JUMP_BOOST {get; set;} = 60f;
    [Export]
    private float WALL_JUMP_BOOST {get; set;} = 60f;
    [Export]
    private float WALL_JUMP_FORCE {get; set;} = 300f;
    [Export]
    private float WALL_SLIDE_FORCE {get; set;} = 9.5625f;
    [Export]
    private float ATTACK_FORCE {get; set;} = 50f;
    [Export]
    public float LUNGE_ACTIVATE_SPEED {get; set;} = 180f;
    [Export]
    public float AERIAL_ATTACK_BOUNCE_FORCE {get; set;} = 180f;
    [Export]
    public float AERIAL_ATTACK_FORWARD_FORCE {get; set;} = 50f;
    [Export]
    public float AERIAL_ATTACK_MAX_BOUNCE_HEIGHT {get; set;} = 40f;

    //triggered false if button is held before landing
    public bool canBoostJump = true;

    public override void Jump(float delta)
    {
        Tempo tempo = (Tempo)player;
        player.xSpeed -= JUMP_FORCE * (float)delta * Mathf.Sin(Mathf.DegToRad(player.groundAngle));
        if (canBoostJump)
        {
            if (tempo.speedBoostInputTimer < SUPER_SPEED_BOOST_TIME_WINDOW * delta)
            {
                GD.Print("superjump");
                player.xSpeed += Mathf.Sign(player.xSpeed) * (GROUND_JUMP_BOOST * delta);
            }
            else if (tempo.speedBoostInputTimer < SPEED_BOOST_TIME_WINDOW * delta)
            {
                GD.Print("boosted jump");
                player.xSpeed += Mathf.Sign(player.xSpeed) * (GROUND_JUMP_BOOST * delta);
                if (Mathf.Abs(player.xSpeed) < TOP_SPEED)
                    player.xSpeed = Mathf.Sign(player.xSpeed) * Mathf.Abs(player.xSpeed);
            }
        }
        player.ySpeed -= JUMP_FORCE * delta * Mathf.Cos(Mathf.DegToRad(player.groundAngle));
        if (Mathf.Abs(player.xSpeed) / (360 * delta) > 1)
            SHORT_HOP_FLOOR = -240f * delta;
        else SHORT_HOP_FLOOR = Mathf.Lerp(-240f * delta, -150f * (float)delta, Mathf.Abs(player.xSpeed) / (360 * delta));
        player.standingOnObject = false;
        tempo.psm.TransitionState(new PlayerJump());
    }
    public void WallJump(float delta)
    {
        Tempo tempo = (Tempo)player;
        if (tempo.speedBoostInputTimer <= SUPER_SPEED_BOOST_TIME_WINDOW * delta)
        {
            GD.Print("super wall jump");
            player.xSpeed = -tempo.xSpeedBuffer / 2;
            player.xSpeed += Mathf.Sign(player.xSpeed) * ((WALL_JUMP_FORCE * delta) + (WALL_JUMP_BOOST * delta));
            player.ySpeed = -(JUMP_FORCE * delta);
        }
        else if (tempo.speedBoostInputTimer <= SPEED_BOOST_TIME_WINDOW * delta)
        {
            GD.Print("boosted wall jump");
            player.xSpeed = -Mathf.Sign(tempo.xSpeedBuffer) * ((WALL_JUMP_FORCE * delta) + (WALL_JUMP_BOOST * delta));
            player.ySpeed = -(JUMP_FORCE * delta);
        }
        else
        {
            GD.Print("regular wall jump");
            player.xSpeed += -Mathf.Sign(tempo.xSpeedBuffer) * WALL_JUMP_FORCE * delta;
            player.ySpeed = -(JUMP_FORCE * delta);
        }
    }

    public void WallSlide(float delta)
    {
        if (player.ySpeed >= 0)
            player.ySpeed -= WALL_SLIDE_FORCE * delta;
    }

    public void ApplyAttackForce(float delta)
    {
        if ((player.groundAngle >= 316f && player.groundAngle <= 360f) || (player.groundAngle >= 0f && player.groundAngle <= 44f))
        {
            player.playerSprite.RotationDegrees = 0;
            if (player.playerSprite.FlipH) player.groundSpeed -=  ATTACK_FORCE * delta;
            else player.groundSpeed +=  ATTACK_FORCE * delta;
        }
        else if (player.groundAngle >= 45f && player.groundAngle <= 135f)
        {
            player.playerSprite.RotationDegrees = 90;
            if (player.playerSprite.FlipH) player.groundSpeed +=  ATTACK_FORCE * delta;
            else player.groundSpeed -=  ATTACK_FORCE * delta;
        }
        else if (player.groundAngle >= 136f && player.groundAngle <= 224f)
        {
            player.playerSprite.RotationDegrees = 180;
            if (player.playerSprite.FlipH) player.groundSpeed +=  ATTACK_FORCE * delta;
            else player.groundSpeed -=  ATTACK_FORCE * delta;
        }
        else 
        {
            player.playerSprite.RotationDegrees = 270;
            if (player.playerSprite.FlipH) player.groundSpeed -=  ATTACK_FORCE * delta;
            else player.groundSpeed +=  ATTACK_FORCE * delta;
        }
        
    }

    public void AerialAttackHit()
    {
        float deltaTime = (float)GetPhysicsProcessDeltaTime();
        if (player.ySpeed < 0)
            player.ySpeed -= Mathf.Abs(player.ySpeed / 4);
        else 
        {
            if (player.ySpeed < AERIAL_ATTACK_MAX_BOUNCE_HEIGHT * deltaTime)
                player.ySpeed -= AERIAL_ATTACK_BOUNCE_FORCE * deltaTime;
            else 
                player.ySpeed -= Mathf.Abs(player.ySpeed * 2);
        }
        player.xSpeed += Mathf.Sign(player.xSpeed) * AERIAL_ATTACK_FORWARD_FORCE * deltaTime;
    }
}
