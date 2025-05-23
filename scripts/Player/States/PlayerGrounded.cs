using Godot;
using System;

public partial class PlayerGrounded : BaseState
{
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;

            player.currentFrame = 0;
        }
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        float deltaTime = (float)delta;
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;

            if (Mathf.Abs(player.groundSpeed) >= 300 * deltaTime)
            {
                if (player.currentFrame == 0)
                    player.playerSprite.Play("fastrun");
                if (player.groundSpeed < 0)
                    player.playerSprite.FlipH = true;
                else if (player.groundSpeed > 0) player.playerSprite.FlipH = false;
            }
            else if (player.groundSpeed != 0)
            {
                if (player.playerSprite.Animation == "fastrun" && (player.groundSpeed < 300 * deltaTime))
                {
                    GD.Print("here");
                    player.playerSprite.Play("skid");
                }
                else if (player.playerSprite.Animation != "skid")
                {
                    if (player.currentFrame == 0)
                        player.playerSprite.Play("jog");
                    if (player.currentFrame != player.playerSprite.Frame)
                    {
                        player.playerSprite.SpeedScale = Mathf.Floor(Mathf.Max(1, Mathf.Abs(player.groundSpeed)));
                        player.currentFrame = player.playerSprite.Frame;
                    }
                    if (player.groundSpeed < 0)
                        player.playerSprite.FlipH = true;
                    else if (player.groundSpeed > 0) player.playerSprite.FlipH = false;
                }
                else if (player.playerSprite.Frame == 2 && (Mathf.Abs(player.groundSpeed) <= 250 * deltaTime))
                    player.playerSprite.Play("jog");
            }
            else 
            {
                player.playerSprite.Play("idle");
                player.playerSprite.SpeedScale = 1.0f;
                player.currentFrame = 0;
            }

            player.pc.FactorSlope(deltaTime);

            //jump check
            if (Input.IsActionPressed("jump"))
            {
                //activate ceiling sensors for one frame before jump. if ceiling is as close
                //as 6 pixels from the player, don't jump.
                SolidTileData data = player.cc.CeilingSensorCompetition();
                if (data.distance > 6)
                {
                    player.pc.Jump(deltaTime);
                    player.playerSprite.Play("liftoff");
                    player.playerSprite.SpeedScale = 1.0f;
                    return;
                }
            }
            else
            {
                //player movement
                if (player.controlLockTimer == 0)
                {
                    //ground running input
                    if (Input.IsActionPressed("left"))
                        player.pc.LeftGroundForce(deltaTime);
                    else if (Input.IsActionPressed("right"))
                        player.pc.RightGroundForce(deltaTime);
                    else 
                        player.pc.Decelerate(deltaTime);
                }
            }
            //wall collision
            bool isVertical = player.cc.SwitchPushCollisionMode(player.groundAngle);
            float wallDistance = player.cc.PushCollisionProcess(isVertical);
            wallDistance *= 60 * deltaTime;

            player.pc.SetSpeedOnGround(isVertical, wallDistance);

            //Move player
            player.pc.MovePlayerObject();

            if (!player.standingOnObject)
            {
                //ground collision
                bool groundCollision = player.cc.GroundCollisionProcess();
                if (groundCollision)
                {
                    player.playerSprite.RotationDegrees = -player.groundAngle;
                }
                else 
                {
                    player.playerSprite.Play("fall");
                    player.playerSprite.SpeedScale = 1.0f;
                    return;
                }
                
                player.cc.LowStepCorrection();
            }
            if (player.controlLockTimer == 0)
            {
                bool falling = player.pc.CheckForSlip(deltaTime);
                if (falling)
                {
                    player.cc.SwitchGroundCollisionMode(0);
                    player.cc.SwitchPushCollisionMode(0);
                    player.playerSprite.Play("airtransition");
                    player.playerSprite.SpeedScale = 1.0f;
                    player.currentFrame = 0;
                    return;
                }
            }
            else player.controlLockTimer = Mathf.Clamp(player.controlLockTimer - deltaTime, 0, 30 * deltaTime);
        }
    }
    public override void Exit(BaseStateMachine sm)
    {
        
    }
}
