using Godot;
using System;

public partial class TempoWallSlide : TempoWallJump
{
    public override void Enter(StateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            player.playerSprite.Play("wallslide");
        }
    }

    public override void Run(StateMachine sm, double delta)
    {
        if (sm is PlayerStateMachine)
        {
            float deltaTime = (float)delta;
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
            TempoPhysicsComponent tpc = (TempoPhysicsComponent)player.pc;
            player.speedBoostInputTimer += Mathf.Clamp((float)delta, 0, 10f * deltaTime);

            if (Input.IsActionPressed("left"))
            {
                bool isColliding = tcc.AirLeftWallCollisionCheck();
                if (!isColliding)
                {
                    player.pc.LeftAirForce(deltaTime);
                    psm.TransitionState(new PlayerFall());
                    player.playerSprite.Play("fall");
                    return;
                }
                
            }
            if (Input.IsActionPressed("right"))
            {
                bool isColliding = tcc.AirRightWallCollisionCheck();
                if (!isColliding)
                {
                    player.pc.RightAirForce(deltaTime);
                    psm.TransitionState(new PlayerFall());
                    player.playerSprite.Play("fall");
                    return;
                }
            }

            player.pc.MovePlayerObject();
            player.pc.ApplyGravity(deltaTime);

            if (Input.IsActionJustPressed("jump"))
            {
                tpc.WallJump(deltaTime);
                player.psm.TransitionState(new PlayerJump());
                if (player.xSpeed < 0)
                    player.playerSprite.FlipH = true;
                else player.playerSprite.FlipH = false;
                player.playerSprite.Play("walljump");
            }
            else
            {
                tpc.WallSlide(deltaTime);
                float airAngle = player.cc.GetAngleOfAirMovement();
                player.cc.DetermineAirCollisionMode(airAngle);
            }
        }
    }

    public override void Exit(StateMachine sm)
    {

    }
}
