using Godot;
using System;

public partial class TempoGrounded : PlayerGrounded
{
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            player.speedBoostInputTimer = 0;
        }
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        if (sm is PlayerStateMachine)
        {
            float deltaTime = (float)delta;
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            TempoPhysicsComponent tpc = (TempoPhysicsComponent)player.pc;
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
            player.speedBoostInputTimer += Mathf.Clamp((float)delta, 0, 10f * (float)delta);
            if (player.groundSpeed != 0)
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
            else 
            {
                player.playerSprite.Play("windyidle");
                player.playerSprite.SpeedScale = 1.0f;
                player.currentFrame = 0;
            }

            player.pc.FactorSlope(deltaTime);

            //jump pressed in timing
            if (Input.IsActionJustPressed("jump"))
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
            //jump is held before landing
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

            if (player.controlLockTimer == 0 && player.health > 0)
            {
                if (Input.IsActionJustPressed("attack"))
                {
                    tpc.ApplyAttackForce(deltaTime);
                    tcc.ToggleAttackHitbox(true);
                    player.controlLockTimer = 20 * deltaTime;
                    psm.TransitionState(new TempoGroundAttack());   
                }
            }

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
