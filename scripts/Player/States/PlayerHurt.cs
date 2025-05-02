using Godot;
using System;

public partial class PlayerHurt : BaseState
{
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;
            player.isInvuln = true;
            player.cc.SwitchGroundCollisionMode(0);
            player.cc.SwitchPushCollisionMode(0);
            player.pc.ApplyHurtForce((float)player.GetPhysicsProcessDeltaTime());
            player.hitbox.SetCollisionMaskValue(3, false);
        }
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        float deltaTime = (float)delta;
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;

            player.pc.MovePlayerObject();
            player.pc.ApplyGravity(deltaTime);

            player.pc.RotateGroundAngle();
            float airAngle = player.cc.GetAngleOfAirMovement();
            
            player.cc.AirPushCollisionProcess();
            player.cc.DetermineAirCollisionMode(airAngle);
            player.playerSprite.Play("hurt");
        }
    }

    public override void Exit(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;
            player.flashActive = true;
        }
    }
}
