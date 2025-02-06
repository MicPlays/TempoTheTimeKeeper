using Godot;
using System;

public partial class PlayerJump : PlayerFall
{
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;

            player.cc.SwitchGroundCollisionMode(0);
            player.cc.SwitchPushCollisionMode(0);
        }
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        float deltaTime = (float)delta;
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;
            if (Input.IsActionJustReleased("jump"))
            {
                player.pc.JumpRelease(deltaTime);
                player.playerSprite.SpeedScale = 1.0f;
                player.currentFrame = 0;
                psm.TransitionState(new PlayerFall());
                return;
            }
            base.Run(sm, delta);
        }
    }
    public override void Exit(BaseStateMachine sm)
    {
        
    }
}
