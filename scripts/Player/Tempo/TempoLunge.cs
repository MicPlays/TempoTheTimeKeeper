using Godot;
using System;

public partial class TempoLunge : TempoGrounded
{
    public float lungeTimer = 20f;
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            TempoPhysicsComponent tpc = (TempoPhysicsComponent)player.pc;
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
            lungeTimer = player.lungeTimerMax;
            tpc.ApplyAttackForce((float)player.GetPhysicsProcessDeltaTime());
            player.controlLockTimer = 20 * (float)player.GetPhysicsProcessDeltaTime();
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

            //wall collision
            bool isVertical = tcc.AttackCollisionProcess(player.groundAngle);
            float wallDistance = player.cc.PushCollisionProcess(isVertical);
            wallDistance *= 60 * deltaTime;

            player.pc.SetSpeedOnGround(isVertical, wallDistance);
            player.pc.MovePlayerObject();

            if (lungeTimer == 0 || Input.IsActionJustPressed("attack"))
            {
                tcc.ToggleAttackHitbox(true, 100f);
                player.xSpeed = 0;
                player.groundSpeed = 0;
                player.controlLockTimer = 8f * deltaTime;
                lungeTimer = 20;
                player.psm.TransitionState(new TempoLungeTransition());
            }
            
            bool groundCollision = player.cc.GroundCollisionProcess();
            if (groundCollision)
            {
                if (Mathf.Abs(-player.groundAngle - player.playerSprite.RotationDegrees) > 20f) 
                    player.playerSprite.RotationDegrees = -player.groundAngle;
            }
            else
            {
                player.playerSprite.Play("fall");
                player.playerSprite.SpeedScale = 1.0f;
                return;
            }
            lungeTimer = Mathf.Clamp(lungeTimer - deltaTime, 0, player.lungeTimerMax * deltaTime);
        }
    }

    public override void Exit(BaseStateMachine sm)
    {
        
    }
}
