using Godot;
using System;

public partial class TempoGroundAttack : TempoGrounded
{
    public override void Enter(BaseStateMachine sm)
    {
        
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
            if (player.controlLockTimer == 0)
            {
                tcc.ToggleAttackHitbox(false);
                psm.TransitionState(new TempoGrounded());
            }
        }
        base.Run(sm, delta);
    }

    public override void Exit(BaseStateMachine sm)
    {
        
    }
}
