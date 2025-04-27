using Godot;
using System;

public partial class TempoLungeTransition : BaseState
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
                tcc.ToggleAttackHitbox(false, 0, 0);
                player.SetState((int)PlayerStates.Grounded);
            }
            player.controlLockTimer = Mathf.Clamp(player.controlLockTimer - deltaTime, 0, 30 * deltaTime);
            
        }
    }

    public override void Exit(BaseStateMachine sm)
    {
        
    }
}
