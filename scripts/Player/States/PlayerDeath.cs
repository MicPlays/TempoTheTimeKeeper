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
            GameController.Instance.playerCam.GlobalPosition = player.GlobalPosition;
            //player.RemoveChild(GameController.Instance.playerCam);
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
            player.pc.MovePlayerObject();
            player.pc.ApplyGravity(deltaTime);
        }
    }

    public override void Exit(BaseStateMachine sm)
    {

    }
}
