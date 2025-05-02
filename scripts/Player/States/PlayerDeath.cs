using Godot;
using System;

public partial class PlayerDeath : BaseState
{
    public override void Enter(BaseStateMachine sm)
    {
         if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;
            player.pc.SetDeathSpeedAndDirection((float)player.GetPhysicsProcessDeltaTime());
            if (LevelManager.Instance.GetLevel().activeCamera is PlayerCam)
                ((PlayerCam)LevelManager.Instance.GetLevel().activeCamera).cameraLocked = true;
            player.hitbox.SetDeferred("monitorable", false);
        }
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;
            float deltaTime = (float)delta;
            player.playerSprite.Play("death");
            player.pc.MovePlayerObject();
            player.pc.ApplyGravity(deltaTime);
        }
    }

    public override void Exit(BaseStateMachine sm)
    {

    }
}
