using Godot;
using System;

public partial class PlayerStateMachine : BaseStateMachine
{
    public Player player;
    
    public override void TransitionState(BaseState transitionState)
    {
        CurrentState.Exit(this);
        SetState(transitionState);
        CurrentState.Enter(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrentState.Run(this, delta);
    }
}
