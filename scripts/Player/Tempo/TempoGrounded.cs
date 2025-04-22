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
            if (Input.IsActionPressed("attack"))
            {
                if (Mathf.Abs(player.groundSpeed) >= tpc.LUNGE_ACTIVATE_SPEED * deltaTime)
                {
                    player.psm.TransitionState(new TempoLunge());
                    return;
                }
                else 
                {
                    player.psm.TransitionState(new TempoAttackCombo());
                    return;
                }
            }
            player.speedBoostInputTimer += Mathf.Clamp((float)delta, 0, 10f * (float)delta);
            base.Run(sm, delta);
        }
    }
    
    public override void Exit(BaseStateMachine sm)
    {
        
    }
}
