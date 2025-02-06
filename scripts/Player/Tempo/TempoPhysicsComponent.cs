using Godot;
using System;

public partial class TempoPhysicsComponent : PlayerPhysicsComponent
{
    [ExportGroup("Physics Constants")]
    private float SPEED_BOOST_TIME_WINDOW {get; set;} = 5f;
    private float SUPER_SPEED_BOOST_TIME_WINDOW {get; set;} = 3.5f;
    private float GROUND_JUMP_BOOST {get; set;} = 60f;
    private float WALL_JUMP_BOOST {get; set;} = 60f;
    private float WALL_JUMP_FORCE {get; set;} = 300f;
    private float WALL_SLIDE_FORCE {get; set;} = 9.5625f;

    public void TempoJump(float delta)
    {
        Tempo tempo = (Tempo)player;
        player.xSpeed -= JUMP_FORCE * (float)delta * Mathf.Sin(Mathf.DegToRad(player.groundAngle));
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
        player.ySpeed -= JUMP_FORCE * delta * Mathf.Cos(Mathf.DegToRad(player.groundAngle));
        if (Mathf.Abs(player.xSpeed) / (360 * delta) > 1)
            SHORT_HOP_FLOOR = -240f * delta;
        else SHORT_HOP_FLOOR = Mathf.Lerp(-240f * delta, -150f * (float)delta, Mathf.Abs(player.xSpeed) / (360 * delta));
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
}
