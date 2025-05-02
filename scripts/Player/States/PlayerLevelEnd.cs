using Godot;
using System;

public partial class PlayerLevelEnd : PlayerGrounded
{
    public float lockTimer = 0;
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;
            lockTimer = 250f * (float)player.GetPhysicsProcessDeltaTime();
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

            if (lockTimer == 0)
            {
                player.pc.RightGroundForce(deltaTime);
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
            }
           else base.Run(sm, deltaTime);
           lockTimer = Mathf.Clamp(lockTimer - deltaTime, 0, 40 * deltaTime);
        }
    }
    public override void Exit(BaseStateMachine sm)
    {
        
    }
}
