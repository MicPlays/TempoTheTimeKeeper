using Godot;
using System;

public partial class TempoGrounded : PlayerGrounded
{
    public override void Enter(StateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            player.speedBoostInputTimer = 0;
        }
    }

    public override void Run(StateMachine sm, double delta)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            player.speedBoostInputTimer += Mathf.Clamp((float)delta, 0, 10f * (float)delta);
            base.Run(sm, delta);
        }
    }
    
    public override void Exit(StateMachine sm)
    {
        
    }
}
