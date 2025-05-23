using Godot;
using System;

public partial class TempoWallJump : BaseState
{
    bool isSliding = false;
    public float wallMagnetTimer;
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            player.speedBoostInputTimer = 0;
            player.playerSprite.Play("wallattach");
            if (player.xSpeedBuffer < 0)
                player.playerSprite.FlipH = false;
            else player.playerSprite.FlipH = true;
            wallMagnetTimer = player.wallMagnetTimerMax;
        }
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        if (sm is PlayerStateMachine)
        {
            float deltaTime = (float)delta;
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
            TempoPhysicsComponent tpc = (TempoPhysicsComponent)player.pc;
            player.speedBoostInputTimer += Mathf.Clamp((float)delta, 0, 10f * deltaTime);

            if (wallMagnetTimer == 0)
            {
                if (Input.IsActionPressed("left"))
                {
                    bool isColliding = tcc.AirLeftWallCollisionCheck();
                    if (!isColliding && !player.pushingAgainstObject)
                    {
                        player.pc.LeftAirForce(deltaTime);
                        psm.TransitionState(new PlayerFall());
                        player.playerSprite.Play("fall");
                        return;
                    }
                    else if (player.pushingAgainstObject && !player.pushingLeft)
                    {
                        player.pc.LeftAirForce(deltaTime);
                        player.pushingAgainstObject = false;
                        psm.TransitionState(new PlayerFall());
                        player.playerSprite.Play("fall");
                        return;
                    }
                    
                }
                if (Input.IsActionPressed("right"))
                {
                    bool isColliding = tcc.AirRightWallCollisionCheck();
                    if (!isColliding && !player.pushingAgainstObject)
                    {
                        player.pc.RightAirForce(deltaTime);
                        psm.TransitionState(new PlayerFall());
                        player.playerSprite.Play("fall");
                        return;
                    }
                    else if (player.pushingAgainstObject && player.pushingLeft)
                    {
                        player.pc.RightAirForce(deltaTime);
                        player.pushingAgainstObject = false;
                        psm.TransitionState(new PlayerFall());
                        player.playerSprite.Play("fall");
                        return;
                    }
                }
            }

            wallMagnetTimer = Mathf.Clamp(wallMagnetTimer - deltaTime, 0, player.wallMagnetTimerMax);
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
                if (airAngle == 270) 
                {
                    player.playerSprite.Play("wallslide");
                    isSliding = true;
                }
                player.cc.DetermineAirCollisionMode(airAngle);
            }
        }
    }

    public override void Exit(BaseStateMachine sm)
    {
        isSliding = false;
        wallMagnetTimer = 0;
    }
}
